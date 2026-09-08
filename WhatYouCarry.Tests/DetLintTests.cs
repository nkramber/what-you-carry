using System;
using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Tools.DetLint;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The determinism lint tool (D-67, D-202, G-2, G-21; PR-3 exit tests 6 and 7).</summary>
/// <remarks>
/// Each fragment below compiles on its own, because the scan reads symbols and not words. A fragment that names
/// a type it does not declare resolves to nothing, and the scan then reports nothing.
/// </remarks>
public sealed class DetLintTests
{
    /// <summary>PR-3 exit test 6. A file that calls System.Math.Sin gives one finding.</summary>
    [Fact]
    public void LintFailsSystemMath()
    {
        LintFinding finding = Assert.Single(Scan("public static class A { public static double B(double c) => System.Math.Sin(c); }"));
        Assert.Equal("L-MATH", finding.Rule);
        Assert.Equal("Math.Sin", finding.Symbol);
        Assert.Equal(1, finding.Line);
        Assert.Contains("DetMath", finding.Detail, StringComparison.Ordinal);
    }

    /// <summary>PR-3 exit test 7. The Core project of this checkout has no finding (G-19).</summary>
    [Fact]
    public void LintPassesCore()
    {
        string root = RepositoryRoot.Find();
        IReadOnlyList<LintFinding> findings = CoreSourceScan.Run(root);
        Assert.True(findings.Count == 0, string.Join(Environment.NewLine, findings));

        // The scan must read real files. A pass over an empty list would also report no finding.
        Assert.NotEmpty(CoreSourceScan.SourceFiles(root));
        Assert.Contains(CoreSourceScan.SourceFiles(root), path => path.EndsWith("DetMath.cs", StringComparison.Ordinal));
    }

    /// <summary>Each banned symbol gives a finding with its rule id.</summary>
    [Theory]
    [InlineData("var a = new System.Random();", "L-RANDOM")]
    [InlineData("var a = System.DateTime.Now;", "L-CLOCK")]
    [InlineData("var a = System.DateTimeOffset.Now;", "L-CLOCK")]
    [InlineData("var a = System.Environment.TickCount;", "L-CLOCK")]
    [InlineData("var a = System.Diagnostics.Stopwatch.StartNew();", "L-CLOCK")]
    [InlineData("var a = new System.Numerics.Vector3();", "L-SIMD")]
    [InlineData("var a = System.Runtime.Intrinsics.Vector128<float>.Zero;", "L-SIMD")]
    [InlineData("var a = System.Numerics.Vector<float>.Zero;", "L-SIMD")]
    [InlineData("dynamic a = 1;", "L-DYNAMIC")]
    public void EachBannedSymbolGivesItsRule(string statement, string rule)
    {
        LintFinding finding = Assert.Single(Scan($"public static class A {{ public static void B() {{ {statement} }} }}"));
        Assert.Equal(rule, finding.Rule);
    }

    /// <summary>A using directive of a banned namespace is a finding, and it appears once and not twice.</summary>
    [Theory]
    [InlineData("using System.Reflection;", "L-REFLECTION")]
    [InlineData("using System.Runtime.Intrinsics;", "L-SIMD")]
    public void ABannedNamespaceImportIsOneFinding(string directive, string rule)
    {
        LintFinding finding = Assert.Single(Scan(directive + "\npublic static class A { }"));
        Assert.Equal(rule, finding.Rule);
        Assert.Equal(1, finding.Line);
    }

    /// <summary>A qualified name inside a banned namespace is one finding, and the nested names do not repeat it.</summary>
    [Fact]
    public void AQualifiedBannedNameIsOneFinding()
    {
        LintFinding finding = Assert.Single(Scan("public static class A { public static System.Reflection.Assembly? B; }"));
        Assert.Equal("L-REFLECTION", finding.Rule);
        Assert.Equal("System.Reflection.Assembly", finding.Symbol);
    }

    /// <summary>A namespace that only starts with the same letters is not a match. The rule needs the whole segment.</summary>
    [Fact]
    public void ALongerNamespaceOfAnotherFamilyIsNotAMatch()
    {
        Assert.DoesNotContain(Scan("namespace System.ReflectionExtras { public static class A { } }"), finding => finding.Rule == "L-REFLECTION");
    }

    /// <summary>MathF outside the one DetMath file is a finding, whichever member it calls.</summary>
    [Fact]
    public void MathFOutsideDetMathIsAFinding()
    {
        LintFinding finding = Assert.Single(Scan("public static class A { public static float B(float c) => System.MathF.Sqrt(c); }", "WhatYouCarry.Core/Other.cs"));
        Assert.Equal("L-MATHF", finding.Rule);
        Assert.Contains(BannedSymbols.DetMathPath, finding.Detail, StringComparison.Ordinal);
    }

    /// <summary>DetMath.cs may call an exact IEEE operation, and only those.</summary>
    [Theory]
    [InlineData("System.MathF.Sqrt(c)")]
    [InlineData("System.MathF.Abs(c)")]
    [InlineData("System.MathF.Floor(c)")]
    public void DetMathMayCallAnExactOperation(string call)
    {
        Assert.Empty(Scan($"public static class A {{ public static float B(float c) => {call}; }}", BannedSymbols.DetMathPath));
    }

    /// <summary>
    /// DetMath.cs may not call a MathF transcendental. The platform library decides its last bits, which is the
    /// defect that DetMath exists to remove (D-69).
    /// </summary>
    [Theory]
    [InlineData("System.MathF.Sin(c)", "MathF.Sin")]
    [InlineData("System.MathF.Cos(c)", "MathF.Cos")]
    [InlineData("System.MathF.Pow(c, 2f)", "MathF.Pow")]
    [InlineData("System.MathF.Atan2(c, 1f)", "MathF.Atan2")]
    public void DetMathMayNotCallATranscendental(string call, string symbol)
    {
        LintFinding finding = Assert.Single(Scan($"public static class A {{ public static float B(float c) => {call}; }}", BannedSymbols.DetMathPath));
        Assert.Equal("L-MATHF", finding.Rule);
        Assert.Equal(symbol, finding.Symbol);
    }

    /// <summary>A bare MathF, such as one behind a using static, is a finding even inside DetMath.cs.</summary>
    [Fact]
    public void ABareMathFIsAFinding()
    {
        LintFinding finding = Assert.Single(Scan("using static System.MathF;\npublic static class A { }", BannedSymbols.DetMathPath));
        Assert.Equal("L-MATHF", finding.Rule);
    }

    /// <summary>
    /// Only the one canonical DetMath path takes the MathF exemption. A second file with the same name in
    /// another directory does not (F-64).
    /// </summary>
    [Fact]
    public void OnlyTheCanonicalDetMathPathIsExempt()
    {
        const string source = "public static class A { public static float B(float c) => System.MathF.Sqrt(c); }";

        // The review trigger. The old code compared the file name alone and reported no finding.
        LintFinding finding = Assert.Single(Scan(source, "WhatYouCarry.Core/Other/DetMath.cs"));
        Assert.Equal("L-MATHF", finding.Rule);

        Assert.Single(Scan(source, "WhatYouCarry.Core/DetMath.cs"));
        Assert.Empty(Scan(source, BannedSymbols.DetMathPath));

        // A Windows separator names the same file, so the rule must read it too.
        Assert.Empty(Scan(source, BannedSymbols.DetMathPath.Replace('/', '\\')));
    }

    /// <summary>The canonical DetMath path names a file that exists, so the exemption is never dead.</summary>
    [Fact]
    public void TheCanonicalDetMathPathExists()
    {
        Assert.Contains(
            CoreSourceScan.SourceFiles(RepositoryRoot.Find()),
            path => path.Replace('\\', '/').EndsWith(BannedSymbols.DetMathPath, StringComparison.Ordinal));
    }

    /// <summary>
    /// Reflection that never names its namespace is a finding. The compiler gives the symbol, so the scan sees
    /// System.Type behind `typeof`, behind `GetType`, and behind a variable of that type (F-64).
    /// </summary>
    [Theory]
    [InlineData("public static class A { public static object B() => typeof(string).GetMethods(); }")]
    [InlineData("public static class A { public static object B(object c) => c.GetType(); }")]
    [InlineData("public static class A { public static object B(System.Type t) => t.GetEvents(); }")]
    [InlineData("public static class A { public static object B(System.Type t) => t.GetProperties(); }")]
    [InlineData("public static class A { public static object? B() => System.Activator.CreateInstance(typeof(A)); }")]
    public void ReflectionWithoutItsNamespaceIsAFinding(string source)
    {
        Assert.Contains(Scan(source), finding => finding.Rule == "L-REFLECTION");
    }

    /// <summary>
    /// A Core member with the same name as a reflection member is not a finding. The compiler tells the two
    /// symbols apart, and a word list cannot. This is the false report that the review found (F-64).
    /// </summary>
    [Fact]
    public void ACoreMemberThatSharesAReflectionNameIsNotAFinding()
    {
        IReadOnlyList<LintFinding> findings = Scan("""
            public sealed class Probe
            {
                public int Type => 1;
                public object GetMethods() => this;
                public object GetProperties() => this;
            }

            public static class A
            {
                public static object B(Probe probe) => probe.GetMethods();
                public static object C(Probe probe) => probe.GetProperties();
                public static int D(Probe probe) => probe.Type;
            }
            """);

        Assert.Empty(findings);
    }

    /// <summary>
    /// A Core type may carry the name of a banned platform type. PR-7 declares a Core vector, and the scan must
    /// read the symbol and not the word (G-2, F-64).
    /// </summary>
    [Fact]
    public void ACoreTypeThatSharesABannedNameIsNotAFinding()
    {
        IReadOnlyList<LintFinding> findings = Scan("""
            namespace WhatYouCarry.Core.World
            {
                public struct Vector3
                {
                    public float X;
                }

                public static class A
                {
                    public static float B(Vector3 v) => v.X;
                }
            }
            """);

        Assert.Empty(findings);
    }

    /// <summary>
    /// Reflection that sits outside System.Type, System.Activator, and the System.Reflection namespace is a
    /// finding too. `Enum.IsDefined` reads the enum metadata, and session 28 removed it from Core for that
    /// reason (F-64).
    /// </summary>
    [Theory]
    [InlineData("public static bool B() => System.Enum.IsDefined(typeof(Probe), 0);")]
    [InlineData("public static string[] B() => System.Enum.GetNames(typeof(Probe));")]
    [InlineData("public static object B() => System.Enum.GetValues(typeof(Probe));")]
    [InlineData("public static object B() => System.Enum.Parse(typeof(Probe), \"A\");")]
    [InlineData("public static bool B(Probe p) => p.HasFlag(Probe.A);")]
    [InlineData("public static object B(System.Delegate d) => d.Method;")]
    [InlineData("public static object B() => System.AppDomain.CurrentDomain;")]
    [InlineData("public static int B(object o) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(o);")]
    public void ReflectionOutsideTheKnownTypesIsAFinding(string member)
    {
        Assert.Contains(
            Scan($"public enum Probe {{ A = 1 }}\npublic static class A {{ {member} }}"),
            finding => finding.Rule == "L-REFLECTION");
    }

    /// <summary>
    /// `typeof` gives back a System.Type, and no symbol carries its name, so the keyword itself is the rule.
    /// Without it a Core file can hold a type value that reaches a place where no name is banned (F-64).
    /// </summary>
    [Fact]
    public void TypeOfIsAFinding()
    {
        LintFinding finding = Assert.Single(Scan("public static class A { public static object B() { var t = typeof(A); return t; } }"));
        Assert.Equal("L-REFLECTION", finding.Rule);
        Assert.Equal("typeof", finding.Symbol);
    }

    /// <summary>
    /// An ordinary enum stays legal. The ban reads `System.Enum`, which owns the metadata methods, and never the
    /// Core enum that declares the values.
    /// </summary>
    [Fact]
    public void AnOrdinaryEnumIsNotAFinding()
    {
        Assert.Empty(Scan("""
            namespace WhatYouCarry.Core.Determinism
            {
                public enum Stream
                {
                    First = 0,
                    Second = 1,
                }

                public static class A
                {
                    public static bool B(Stream stream) => stream == Stream.First;

                    public static int C(Stream stream) => (int)stream;

                    public static bool D(Stream stream) => stream >= Stream.First && stream <= Stream.Second;
                }
            }
            """));
    }

    /// <summary>
    /// TypeDescriptor reads type metadata beside reflection, so it carries the reflection rule id (F-66).
    /// </summary>
    [Fact]
    public void TypeDescriptorIsAReflectionFinding()
    {
        // The review trigger. It compiles in Core, and the old scan reported nothing.
        Assert.Contains(
            Scan("public static class A { public static object B(object o) => System.ComponentModel.TypeDescriptor.GetProperties(o); }"),
            finding => finding.Rule == "L-REFLECTION");
    }

    /// <summary>
    /// Every approved type stays legal. Core uses no array member and no collection yet, so `System.Array` and
    /// the collection types are absent from the list. The PR that first needs one adds it with a decision
    /// (D-207, G-16).
    /// </summary>
    [Theory]
    [InlineData("public static object B() => new System.InvalidOperationException(\"x\");")]
    [InlineData("public static string B(int i) => i.ToString(System.Globalization.CultureInfo.InvariantCulture);")]
    [InlineData("public static uint B(float f) => System.BitConverter.SingleToUInt32Bits(f);")]
    [InlineData("public static bool B(float f) => float.IsNegative(f);")]
    [InlineData("public static int[] B() => new int[4];")]
    public void AnApprovedTypeIsNotAFinding(string member)
    {
        Assert.Empty(Scan($"public static class A {{ {member} }}"));
    }

    /// <summary>A type of this project is never a namespace finding, at any depth.</summary>
    [Fact]
    public void AProjectNamespaceIsNotAFinding()
    {
        Assert.Empty(Scan("""
            namespace WhatYouCarry.Core.World.Deep
            {
                public struct Block
                {
                    public byte Id;
                }
            }

            namespace WhatYouCarry.Core.Determinism
            {
                public static class A
                {
                    public static byte B(WhatYouCarry.Core.World.Deep.Block block) => block.Id;
                }
            }
            """));
    }

    /// <summary>
    /// A randomness source outside `Rng` is a finding, even inside the approved `System` namespace (G-21, F-67).
    /// </summary>
    [Fact]
    public void AnotherRandomnessSourceIsAFinding()
    {
        // The review trigger. Guid sits in System and outside the old denylist, and it gave no finding.
        Assert.Contains(
            Scan("public static class A { public static object B() => System.Guid.NewGuid(); }"),
            finding => finding.Rule == "L-RANDOM");
    }

    /// <summary>
    /// A type that nobody approved is a finding, whatever it does and whichever namespace holds it (D-207, F-68).
    /// No namespace is approved as a whole, so a machine-dependent type cannot enter beside a needed one.
    /// </summary>
    [Theory]
    [InlineData("public static long B() => System.GC.GetTotalMemory(false);")]
    [InlineData("public static bool B() => System.OperatingSystem.IsMacOS();")]
    [InlineData("public static void B() => System.Console.WriteLine(\"x\");")]
    [InlineData("public static object? B() => System.AppContext.GetData(\"x\");")]
    [InlineData("public static object B() => new System.Uri(\"http://x\");")]
    [InlineData("public static object B() => new System.Text.StringBuilder();")]
    [InlineData("public static object B() => System.Linq.Expressions.Expression.Constant(1);")]
    [InlineData("public static object B(string s) => System.Text.Json.JsonDocument.Parse(s);")]
    [InlineData("public static object B() => new System.Collections.ArrayList();")]
    [InlineData("public static object B() => System.Threading.Thread.CurrentThread;")]
    [InlineData("public static object B() => new System.Collections.Generic.List<int>();")]
    [InlineData("public static int B(uint u) => System.Numerics.BitOperations.PopCount(u);")]
    public void AnUnapprovedTypeIsAFinding(string member)
    {
        Assert.Contains(
            Scan($"public static class A {{ {member} }}"),
            finding => finding.Rule == "L-TYPE");
    }

    /// <summary>
    /// A machine-dependent member reached through an approved wrapper is a finding. The review found the
    /// process-randomized string hash behind `EqualityComparer`, which no member ban on `String` reaches (F-68).
    /// </summary>
    [Fact]
    public void AMachineMemberBehindAWrapperIsAFinding()
    {
        // The review trigger. Three processes gave three hashes for one string.
        Assert.Contains(
            Scan("public static class A { public static int B(string v) => System.Collections.Generic.EqualityComparer<string>.Default.GetHashCode(v); }"),
            finding => finding.Rule == "L-TYPE");

        Assert.Contains(
            Scan("public static class A { public static bool B() => System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported; }"),
            finding => finding.Rule == "L-TYPE");
    }

    /// <summary>
    /// An approved type can still hold a member that reads the user. `CultureInfo` is approved for
    /// `InvariantCulture`, and the culture of the machine is a finding (F-68).
    /// </summary>
    [Fact]
    public void AMachineMemberOfAnApprovedTypeIsAFinding()
    {
        LintFinding finding = Assert.Single(Scan("public static class A { public static object B() => System.Globalization.CultureInfo.CurrentCulture; }"));
        Assert.Equal("L-CLOCK", finding.Rule);

        // The member that Core needs stays clean.
        Assert.Empty(Scan("public static class A { public static object B() => System.Globalization.CultureInfo.InvariantCulture; }"));
    }

    /// <summary>
    /// A hash that reads the machine is a finding, although its type is approved. The default hash of a
    /// reference type is its address, and the string hash takes a new seed in each process (G-21, F-67).
    /// </summary>
    [Theory]
    [InlineData("public static int B(object o) => o.GetHashCode();")]
    [InlineData("public static int B(string s) => s.GetHashCode();")]
    [InlineData("public static int B() => System.HashCode.Combine(1, 2);")]
    public void AHashThatReadsTheMachineIsAFinding(string member)
    {
        Assert.Contains(
            Scan($"public static class A {{ {member} }}"),
            finding => finding.Rule == "L-IDENTITY");
    }

    /// <summary>
    /// Every approved `System` type stays legal, and an ordinary return type never needs an entry. The allowlist
    /// binds the names that Core writes, not the types that flow through it.
    /// </summary>
    [Fact]
    public void AnApprovedSystemTypeIsNotAFinding()
    {
        Assert.Empty(Scan("""
            namespace WhatYouCarry.Core.Determinism
            {
                public static class A
                {
                    public static bool B(float f) => float.IsNegative(f);

                    public static uint C(float f) => System.BitConverter.SingleToUInt32Bits(f);

                    public static (float First, int Second) D() => (1.0f, 2);

                    public static void E(int i)
                    {
                        if (i < 0)
                        {
                            throw new System.ArgumentOutOfRangeException(nameof(i), "x");
                        }
                    }
                }
            }
            """));
    }

    /// <summary>
    /// An import of an unapproved namespace is a finding on its own, with no use of a type in it (D-205, F-67).
    /// </summary>
    [Fact]
    public void AnUnapprovedNamespaceImportIsAFinding()
    {
        // The review trigger. The import check read the denylist alone and reported nothing.
        LintFinding finding = Assert.Single(Scan("using System.Text;\n\npublic static class A { public static int B() => 1; }"));
        Assert.Equal("L-NAMESPACE", finding.Rule);
        Assert.Equal("System.Text", finding.Symbol);
        Assert.Equal(1, finding.Line);

        // An import of System is correct. Its types pass the allowlist of D-206 one at a time.
        Assert.Empty(Scan("using System;\n\npublic static class A { public static int B() => 1; }"));
    }

    /// <summary>
    /// Conditional compilation in Core is a finding (D-204, F-65). The Core build defines the target-framework
    /// symbol, so a `#if NET10_0` branch compiles while a lint that defines no symbol reads an empty branch.
    /// </summary>
    [Fact]
    public void ConditionalCompilationInCoreIsAFinding()
    {
        // The review trigger. The build compiles this call, and the old scan reported nothing.
        IReadOnlyList<LintFinding> findings = Scan("public static class A {\n#if NET10_0\n    public static double B(double c) => System.Math.Sin(c);\n#endif\n}");
        LintFinding finding = Assert.Single(findings);
        Assert.Equal("L-CONDITIONAL", finding.Rule);
        Assert.Equal("#if", finding.Symbol);
        Assert.Equal(2, finding.Line);

        // A branch that the lint symbols would select is a finding too. The rule reads the directive, not the body.
        Assert.Contains(
            Scan("public static class A {\n#if DEBUG\n    public static int B() => 1;\n#else\n    public static int B() => 2;\n#endif\n}"),
            other => other.Rule == "L-CONDITIONAL");
    }

    /// <summary>
    /// A directive that selects no branch stays legal. The rule reads conditional compilation alone (D-204).
    /// </summary>
    [Theory]
    [InlineData("#nullable enable")]
    [InlineData("#region Parts")]
    [InlineData("#pragma warning disable CA1000")]
    public void ADirectiveThatSelectsNoBranchIsNotAFinding(string directive)
    {
        string suffix = directive.StartsWith("#region", StringComparison.Ordinal) ? "\n#endregion" : string.Empty;
        Assert.Empty(Scan($"{directive}\npublic static class A {{ public static int B() => 1; }}{suffix}"));
    }

    /// <summary>
    /// A banned name inside a comment or a string is not a finding. The scan reads symbols, so prose can never
    /// reach a rule (D-202).
    /// </summary>
    [Fact]
    public void ABannedNameInProseIsNotAFinding()
    {
        Assert.Empty(Scan("""
            // Use DetMath and never System.Math.Sin.
            /* Stopwatch and DateTime are banned here. */
            public static class A
            {
                /// <summary>Never call MathF or System.Random.</summary>
                public const string Note = "Math, MathF, Stopwatch, DateTime, System.Reflection";
            }
            """));
    }

    /// <summary>A file that does not compile is a finding, so no rule below it fails in silence (T-2).</summary>
    [Fact]
    public void AFileThatDoesNotParseIsAFinding()
    {
        Assert.Contains(Scan("public static class A { this is not C# "), finding => finding.Rule == "L-PARSE");
    }

    /// <summary>
    /// A Core source that does not compile is a finding on the repository scan. Without it every rule below the
    /// error would resolve no symbol and pass in silence (T-2).
    /// </summary>
    [Fact]
    public void ACoreSourceThatDoesNotCompileIsAFinding()
    {
        using TemporaryCheckout checkout = new();
        checkout.WriteCoreFile("Broken.cs", "public static class A { public static Missing B() => null!; }");

        Assert.Contains(CoreSourceScan.Run(checkout.Root), finding => finding.Rule == "L-PARSE");
    }

    /// <summary>The scan reads every hand-written Core file and leaves the build output out.</summary>
    [Fact]
    public void TheScanLeavesTheBuildOutputOut()
    {
        using TemporaryCheckout checkout = new();
        checkout.WriteCoreFile("Determinism/Good.cs", "public static class A { }");
        checkout.WriteCoreFile("obj/Debug/Generated.cs", "public static class B { public static object C => new System.Random(); }");

        Assert.Single(CoreSourceScan.SourceFiles(checkout.Root));
        Assert.Empty(CoreSourceScan.Run(checkout.Root));
    }

    /// <summary>A checkout with no Core directory is an error that names the path (T-2).</summary>
    [Fact]
    public void ACheckoutWithNoCoreDirectoryIsAnError()
    {
        string root = Path.Combine(Path.GetTempPath(), "wyc-lint-" + Guid.NewGuid().ToString("n"));
        DirectoryNotFoundException error = Assert.Throws<DirectoryNotFoundException>(() => CoreSourceScan.SourceFiles(root));
        Assert.Contains("WhatYouCarry.Core", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Every allowed MathF member is one that DetMath.cs really calls, so the list holds no dead entry.</summary>
    [Fact]
    public void EveryAllowedMathFMemberHasACaller()
    {
        string detMath = RepositoryRoot.ReadFile(BannedSymbols.DetMathPath);
        foreach (string member in BannedSymbols.AllowedMathFMembers)
        {
            Assert.Contains($"MathF.{member}(", detMath, StringComparison.Ordinal);
        }
    }

    private static IReadOnlyList<LintFinding> Scan(string source, string path = "WhatYouCarry.Core/Test.cs")
    {
        return CoreSourceScan.ScanText(source, path);
    }

    /// <summary>A throwaway checkout with a Core directory, for the scans that read files.</summary>
    private sealed class TemporaryCheckout : IDisposable
    {
        public TemporaryCheckout()
        {
            this.Root = Path.Combine(Path.GetTempPath(), "wyc-lint-" + Guid.NewGuid().ToString("n"));
            Directory.CreateDirectory(Path.Combine(this.Root, CoreSourceScan.CoreDirectory));
        }

        public string Root { get; }

        public void WriteCoreFile(string relativePath, string text)
        {
            string full = Path.Combine(this.Root, CoreSourceScan.CoreDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, text);
        }

        public void Dispose()
        {
            if (Directory.Exists(this.Root))
            {
                Directory.Delete(this.Root, recursive: true);
            }
        }
    }
}
