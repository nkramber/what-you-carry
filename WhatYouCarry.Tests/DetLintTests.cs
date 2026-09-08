using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhatYouCarry.Tools.DetLint;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The determinism lint tool (D-67, G-2, G-21; PR-3 exit tests 6 and 7).</summary>
public sealed class DetLintTests
{
    /// <summary>PR-3 exit test 6. A file that calls System.Math.Sin gives one finding.</summary>
    [Fact]
    public void LintFailsSystemMath()
    {
        LintFinding finding = Assert.Single(Scan("public static class A { public static float B(float c) => System.Math.Sin(c); }"));
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

    /// <summary>Each banned name gives a finding with its rule id.</summary>
    [Theory]
    [InlineData("var a = new System.Random();", "L-RANDOM")]
    [InlineData("var a = System.DateTime.Now;", "L-CLOCK")]
    [InlineData("var a = System.Environment.TickCount;", "L-CLOCK")]
    [InlineData("var a = Stopwatch.StartNew();", "L-CLOCK")]
    [InlineData("var a = new System.Numerics.Vector3();", "L-SIMD")]
    [InlineData("var a = Vector128<float>.Zero;", "L-SIMD")]
    [InlineData("dynamic a = 1;", "L-DYNAMIC")]
    public void EachBannedNameGivesItsRule(string statement, string rule)
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

    /// <summary>
    /// A namespace that only starts with the same letters is not a match. The rule needs the whole segment.
    /// </summary>
    [Fact]
    public void ALongerNamespaceOfAnotherFamilyIsNotAMatch()
    {
        Assert.Empty(Scan("using System.ReflectionExtras;\npublic static class A { }"));
    }

    /// <summary>MathF outside DetMath.cs is a finding, whichever member it calls.</summary>
    [Fact]
    public void MathFOutsideDetMathIsAFinding()
    {
        LintFinding finding = Assert.Single(Scan("public static class A { public static float B(float c) => MathF.Sqrt(c); }", "WhatYouCarry.Core/Other.cs"));
        Assert.Equal("L-MATHF", finding.Rule);
        Assert.Contains("DetMath.cs", finding.Detail, StringComparison.Ordinal);
    }

    /// <summary>DetMath.cs may call an exact IEEE operation, and only those.</summary>
    [Theory]
    [InlineData("MathF.Sqrt(c)")]
    [InlineData("MathF.Abs(c)")]
    [InlineData("MathF.Floor(c)")]
    public void DetMathMayCallAnExactOperation(string call)
    {
        Assert.Empty(Scan($"public static class A {{ public static float B(float c) => {call}; }}", "WhatYouCarry.Core/Determinism/DetMath.cs"));
    }

    /// <summary>
    /// DetMath.cs may not call a MathF transcendental. The platform library decides its last bits, which is the
    /// defect that DetMath exists to remove (D-69).
    /// </summary>
    [Theory]
    [InlineData("MathF.Sin(c)", "MathF.Sin")]
    [InlineData("MathF.Cos(c)", "MathF.Cos")]
    [InlineData("MathF.Pow(c, 2f)", "MathF.Pow")]
    [InlineData("MathF.Atan2(c, 1f)", "MathF.Atan2")]
    public void DetMathMayNotCallATranscendental(string call, string symbol)
    {
        LintFinding finding = Assert.Single(Scan($"public static class A {{ public static float B(float c) => {call}; }}", "WhatYouCarry.Core/Determinism/DetMath.cs"));
        Assert.Equal("L-MATHF", finding.Rule);
        Assert.Equal(symbol, finding.Symbol);
    }

    /// <summary>A bare MathF, such as one behind a using static, is a finding even inside DetMath.cs.</summary>
    [Fact]
    public void ABareMathFIsAFinding()
    {
        LintFinding finding = Assert.Single(Scan("using static System.MathF;\npublic static class A { }", "WhatYouCarry.Core/Determinism/DetMath.cs"));
        Assert.Equal("L-MATHF", finding.Rule);
    }

    /// <summary>
    /// A banned name inside a comment or a string is not a finding. A parse reads the code alone, which is the
    /// reason the tool uses the compiler API and not a text search (D-202).
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

    /// <summary>A member named like a banned type is not a finding. Only the type on the left of the dot counts.</summary>
    [Fact]
    public void AMemberNamedLikeABannedTypeIsNotAFinding()
    {
        Assert.Empty(Scan("public static class A { public static int Environment => 1; public static int B(A a) => 2; }"));
    }

    /// <summary>A file that does not parse is a finding, so no rule below it fails in silence (T-2).</summary>
    [Fact]
    public void AFileThatDoesNotParseIsAFinding()
    {
        IReadOnlyList<LintFinding> findings = Scan("public static class A { this is not C# ");
        Assert.Contains(findings, finding => finding.Rule == "L-PARSE");
    }

    /// <summary>The scan reads every hand-written Core file and leaves the build output out.</summary>
    [Fact]
    public void TheScanLeavesTheBuildOutputOut()
    {
        string root = Path.Combine(Path.GetTempPath(), "wyc-lint-" + Guid.NewGuid().ToString("n"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "WhatYouCarry.Core", "Determinism"));
            Directory.CreateDirectory(Path.Combine(root, "WhatYouCarry.Core", "obj", "Debug"));
            File.WriteAllText(Path.Combine(root, "WhatYouCarry.Core", "Determinism", "Good.cs"), "public static class A { }");
            File.WriteAllText(Path.Combine(root, "WhatYouCarry.Core", "obj", "Debug", "Generated.cs"), "public static class B { public static object C => new System.Random(); }");

            Assert.Single(CoreSourceScan.SourceFiles(root));
            Assert.Empty(CoreSourceScan.Run(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
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
        string detMath = RepositoryRoot.ReadFile("WhatYouCarry.Core/Determinism/DetMath.cs");
        foreach (string member in BannedSymbols.AllowedMathFMembers)
        {
            Assert.Contains($"MathF.{member}(", detMath, StringComparison.Ordinal);
        }
    }

    private static IReadOnlyList<LintFinding> Scan(string source, string path = "WhatYouCarry.Core/Test.cs")
    {
        return CoreSourceScan.ScanText(source, path);
    }
}
