using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace WhatYouCarry.Tools.DetLint;

/// <summary>
/// Scans the Core source files for the symbols that <see cref="BannedSymbols"/> lists (D-67, G-2, G-21).
/// </summary>
/// <remarks>
/// The scan parses each file with the C# compiler API (D-202). A parse tells an identifier from a comment and
/// from a string, so a banned name inside prose is never a finding, and a name that a text search cannot see,
/// such as one behind a <c>using static</c>, still is.
/// </remarks>
public static class CoreSourceScan
{
    /// <summary>The directory of the Core project, relative to the checkout root.</summary>
    public const string CoreDirectory = "WhatYouCarry.Core";

    /// <summary>Every Core source file under the checkout, sorted, with the build output left out.</summary>
    /// <exception cref="DirectoryNotFoundException">The checkout holds no Core directory.</exception>
    public static IReadOnlyList<string> SourceFiles(string checkoutRoot)
    {
        string coreRoot = Path.Combine(checkoutRoot, CoreDirectory);
        if (!Directory.Exists(coreRoot))
        {
            throw new DirectoryNotFoundException($"The checkout holds no Core directory. The path is '{coreRoot}'.");
        }

        List<string> files = [];
        foreach (string file in Directory.EnumerateFiles(coreRoot, "*.cs", SearchOption.AllDirectories))
        {
            // The build writes generated sources into obj and bin. They are not hand-written Core code.
            string relative = Path.GetRelativePath(coreRoot, file);
            if (relative.StartsWith("obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || relative.StartsWith("bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                continue;
            }

            files.Add(file);
        }

        files.Sort(StringComparer.Ordinal);
        return files;
    }

    /// <summary>Every finding in the Core source of one checkout, in file order.</summary>
    public static IReadOnlyList<LintFinding> Run(string checkoutRoot)
    {
        List<LintFinding> findings = [];
        foreach (string file in SourceFiles(checkoutRoot))
        {
            string relativePath = Path.GetRelativePath(checkoutRoot, file).Replace('\\', '/');
            findings.AddRange(ScanText(File.ReadAllText(file), relativePath));
        }

        return findings;
    }

    /// <summary>Every finding in one source text. The path names the file in each finding and selects the DetMath rule.</summary>
    public static IReadOnlyList<LintFinding> ScanText(string sourceText, string path)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);
        SyntaxNode root = tree.GetRoot();
        bool isDetMath = Path.GetFileName(path).Equals(BannedSymbols.DetMathFileName, StringComparison.Ordinal);

        List<LintFinding> findings = [];

        // A file that does not parse hides every rule below it. That is a finding and never a silent pass (T-2).
        foreach (Diagnostic diagnostic in tree.GetDiagnostics())
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                FileLinePositionSpan errorSpan = diagnostic.Location.GetLineSpan();
                findings.Add(new LintFinding(
                    path,
                    errorSpan.StartLinePosition.Line + 1,
                    errorSpan.StartLinePosition.Character + 1,
                    "L-PARSE",
                    diagnostic.Id,
                    $"The file does not parse, so no other rule can read it. {diagnostic.GetMessage(CultureInfo.InvariantCulture)}"));
            }
        }

        // The namespace pass runs first, over the using directives and the outermost qualified names. An inner
        // qualified name spells a prefix of the same text, so only the outermost one can report.
        List<TextSpan> reportedSpans = [];
        foreach (SyntaxNode node in root.DescendantNodes())
        {
            string? name = node switch
            {
                UsingDirectiveSyntax usingDirective => usingDirective.Name?.ToString(),
                QualifiedNameSyntax qualifiedName when qualifiedName.Parent is not QualifiedNameSyntax
                    && qualifiedName.Parent is not UsingDirectiveSyntax => qualifiedName.ToString(),
                _ => null,
            };

            if (name is not null && AddNamespaceFinding(findings, name, node, path))
            {
                reportedSpans.Add(node.Span);
            }
        }

        // The name pass reads every simple name, which covers a plain identifier and a generic name such as
        // Vector128<float>. A name inside a span that the namespace pass reported is part of that one finding.
        foreach (SyntaxNode node in root.DescendantNodes())
        {
            if (node is not SimpleNameSyntax simpleName || IsInside(reportedSpans, simpleName.Span))
            {
                continue;
            }

            AddNameFinding(findings, simpleName, path, isDetMath);
        }

        return findings;
    }

    /// <summary>Adds a finding when a name starts with a banned namespace. It answers whether it added one.</summary>
    private static bool AddNamespaceFinding(List<LintFinding> findings, string name, SyntaxNode node, string path)
    {
        foreach (KeyValuePair<string, BannedSymbols.BannedName> banned in BannedSymbols.Namespaces)
        {
            // The dot keeps a longer namespace of another family, such as System.ReflectionExtras, out of the match.
            if (name.Equals(banned.Key, StringComparison.Ordinal) || name.StartsWith(banned.Key + ".", StringComparison.Ordinal))
            {
                findings.Add(Create(node, path, banned.Value.Rule, name, banned.Value.Detail));
                return true;
            }
        }

        return false;
    }

    /// <summary>Answers whether a span sits inside any of the spans that the namespace pass reported.</summary>
    private static bool IsInside(List<TextSpan> reportedSpans, TextSpan span)
    {
        foreach (TextSpan reported in reportedSpans)
        {
            if (reported.Contains(span))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Adds a finding when an identifier is a banned name. The MathF rule reads the member and the file.</summary>
    private static void AddNameFinding(List<LintFinding> findings, SimpleNameSyntax identifier, string path, bool isDetMath)
    {
        string name = identifier.Identifier.ValueText;
        if (!BannedSymbols.Names.TryGetValue(name, out BannedSymbols.BannedName banned))
        {
            return;
        }

        string? member = MemberAfter(identifier);

        if (name.Equals("MathF", StringComparison.Ordinal))
        {
            if (!isDetMath)
            {
                findings.Add(Create(identifier, path, banned.Rule, "MathF", $"Only {BannedSymbols.DetMathFileName} may name MathF (G-2)."));
                return;
            }

            if (member is null || !BannedSymbols.AllowedMathFMembers.Contains(member))
            {
                string called = member is null ? "MathF" : $"MathF.{member}";
                findings.Add(Create(identifier, path, banned.Rule, called, $"{BannedSymbols.DetMathFileName} may call only an exact IEEE operation: {string.Join(", ", BannedSymbols.AllowedMathFMembers)} (G-2)."));
            }

            return;
        }

        string symbol = member is null ? name : $"{name}.{member}";
        findings.Add(Create(identifier, path, banned.Rule, symbol, banned.Detail));
    }

    /// <summary>
    /// The member that a banned type names, or null when the type stands alone. The type is the left side of a
    /// member access in <c>Math.Sin</c>, and it is the name of an inner access in <c>System.Math.Sin</c>.
    /// A bare name, such as one behind a <c>using static</c>, gives null.
    /// </summary>
    private static string? MemberAfter(SimpleNameSyntax identifier)
    {
        if (identifier.Parent is not MemberAccessExpressionSyntax access)
        {
            return null;
        }

        if (access.Expression == identifier)
        {
            return access.Name.Identifier.ValueText;
        }

        // The type is the right side of an access, so the member is one level above: System.Math then .Sin.
        if (access.Name == identifier && access.Parent is MemberAccessExpressionSyntax outer && outer.Expression == access)
        {
            return outer.Name.Identifier.ValueText;
        }

        return null;
    }

    /// <summary>One finding at the position of a node. The line and the column both count from one, as a compiler reports them.</summary>
    private static LintFinding Create(SyntaxNode node, string path, string rule, string symbol, string detail)
    {
        FileLinePositionSpan span = node.SyntaxTree.GetLineSpan(node.Span);
        return new LintFinding(path, span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1, rule, symbol, detail);
    }
}
