using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WhatYouCarry.Tools.DetLint;

/// <summary>
/// Scans the Game source for a string that a player sees (G-8, D-98, D-222). Every such string has an id in the
/// string table, and `Strings.Get` reads it.
/// </summary>
/// <remarks>
/// <para>
/// The Game rules and the Core rules never mix. Game has an engine dependency and needs no allowlist, and Core
/// needs no string rule, because no player sees a Core string.
/// </para>
/// <para>
/// The scan reads the syntax alone. A string literal is a finding unless it stands in one of the places that
/// <see cref="AllowedStringPositions"/> names, such as an argument to `Strings.Get` or a node path.
/// </para>
/// </remarks>
public static class GameStringScan
{
    /// <summary>The directory of the Game project, relative to the checkout root.</summary>
    public const string GameDirectory = "WhatYouCarry.Game";

    /// <summary>The call that reads a string from the table. A literal inside it is the id, and never the text.</summary>
    public const string StringsGet = "Get";

    /// <summary>The type that owns <see cref="StringsGet"/>.</summary>
    public const string StringsType = "Strings";

    /// <summary>
    /// The receiver names that stand for the string table. A `Get` call takes an id and not a text only when
    /// its receiver is one of these (D-222, F-79).
    /// </summary>
    /// <remarks>
    /// The rule reads the syntax. The Game project has an engine dependency, so a compilation of it needs the
    /// engine assemblies, and the project holds no source file yet. The PR that first writes Game code can add
    /// a symbol read, and this list holds the boundary until then.
    /// </remarks>
    public static readonly IReadOnlySet<string> StringTableReceivers = new HashSet<string>
    {
        "Strings",
        "strings",
    };

    /// <summary>
    /// The method names whose string arguments name a thing in the engine, and not a thing a player reads.
    /// A new name joins this list with a test. `Get` is absent, because any type can hold a `Get` method, and
    /// <see cref="StringTableReceivers"/> covers the one call that takes an id (F-79).
    /// </summary>
    public static readonly IReadOnlySet<string> AllowedStringPositions = new HashSet<string>
    {
        "GetNode",
        "GetNodeOrNull",
        "FindChild",
        "Load",
        "Connect",
        "EmitSignal",
        "HasMeta",
        "GetMeta",
        "SetMeta",
        "nameof",
    };

    /// <summary>Every Game source file under the checkout, sorted, with the build output left out.</summary>
    /// <exception cref="DirectoryNotFoundException">The checkout holds no Game directory.</exception>
    public static IReadOnlyList<string> SourceFiles(string checkoutRoot)
    {
        string gameRoot = Path.Combine(checkoutRoot, GameDirectory);
        if (!Directory.Exists(gameRoot))
        {
            throw new DirectoryNotFoundException($"The checkout holds no Game directory. The path is '{gameRoot}'.");
        }

        List<string> files = [];
        foreach (string file in Directory.EnumerateFiles(gameRoot, "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(gameRoot, file);
            if (relative.StartsWith("obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || relative.StartsWith("bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || relative.StartsWith(".godot" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                continue;
            }

            files.Add(file);
        }

        files.Sort(StringComparer.Ordinal);
        return files;
    }

    /// <summary>Every finding in the Game source of one checkout, in file order.</summary>
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

    /// <summary>Every finding in one Game source text.</summary>
    public static IReadOnlyList<LintFinding> ScanText(string sourceText, string path)
    {
        SyntaxNode root = CSharpSyntaxTree.ParseText(sourceText).GetRoot();
        List<LintFinding> findings = [];

        foreach (SyntaxNode node in root.DescendantNodes())
        {
            if (node is not LiteralExpressionSyntax literal || !literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                continue;
            }

            // An empty string names nothing that a player reads.
            if (literal.Token.ValueText.Length == 0)
            {
                continue;
            }

            if (StandsInAnAllowedPosition(literal))
            {
                continue;
            }

            findings.Add(Create(literal, path, "L-STRING", literal.Token.ValueText, $"A string that a player sees needs an id in the string table, and {StringsType}.{StringsGet} reads it (G-8, D-98)."));
        }

        return findings;
    }

    /// <summary>
    /// Answers whether a literal stands where the engine needs a name, and not where a player reads text.
    /// The rule reads the call that holds the literal, and a constant declaration.
    /// </summary>
    private static bool StandsInAnAllowedPosition(LiteralExpressionSyntax literal)
    {
        // A const string is a name that the code declares once, such as a scene path or a table id.
        for (SyntaxNode? above = literal.Parent; above is not null; above = above.Parent)
        {
            if (above is FieldDeclarationSyntax field && field.Modifiers.ToString().Contains("const", StringComparison.Ordinal))
            {
                return true;
            }

            if (above is AttributeSyntax or EnumMemberDeclarationSyntax)
            {
                return true;
            }

            if (above is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax access)
                {
                    return AllowedStringPositions.Contains(invocation.Expression.ToString());
                }

                // A `Get` call takes an id only when its receiver names the string table. Any type can hold a
                // `Get` method, and `inventory.Get("You died")` carries a text that a player reads (F-79).
                if (access.Name.Identifier.ValueText == StringsGet)
                {
                    return access.Expression is SimpleNameSyntax receiver && StringTableReceivers.Contains(receiver.Identifier.ValueText)
                        || access.Expression is MemberAccessExpressionSyntax inner && StringTableReceivers.Contains(inner.Name.Identifier.ValueText);
                }

                return AllowedStringPositions.Contains(access.Name.Identifier.ValueText);
            }

            if (above is StatementSyntax or MemberDeclarationSyntax)
            {
                break;
            }
        }

        return false;
    }

    /// <summary>One finding at the position of a node. The line and the column both count from one.</summary>
    private static LintFinding Create(SyntaxNode node, string path, string rule, string symbol, string detail)
    {
        FileLinePositionSpan span = node.SyntaxTree.GetLineSpan(node.Span);
        return new LintFinding(path, span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1, rule, symbol, detail);
    }
}
