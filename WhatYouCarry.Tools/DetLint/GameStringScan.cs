using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        "Connect",
        "EmitSignal",
        "HasMeta",
        "GetMeta",
        "SetMeta",
        "nameof",
    };

    /// <summary>
    /// The call that loads a resource by its path. Any type can hold a <c>Load</c> method, and <c>h.Load("You
    /// died")</c> can put a text on a node, so the call takes a path only when its receiver is one of
    /// <see cref="ResourceLoaders"/> (F-132).
    /// </summary>
    public const string LoadMethod = "Load";

    /// <summary>The receiver names of the engine calls that load a resource by its path (F-132).</summary>
    public static readonly IReadOnlySet<string> ResourceLoaders = new HashSet<string>
    {
        "GD",
        "ResourceLoader",
    };

    /// <summary>
    /// The members that put a text on the screen. A const that the file declares, assigned to one of these,
    /// is a text that a player reads, and the string table must hold it (F-132).
    /// </summary>
    public static readonly IReadOnlySet<string> VisibleTextMembers = new HashSet<string>
    {
        "Text",
        "TooltipText",
        "PlaceholderText",
    };

    private const string LiteralDetail = $"A string that a player sees needs an id in the string table, and {StringsType}.{StringsGet} reads it (G-8, D-98).";

    private const string InterpolationDetail = $"An interpolated string builds a text outside the string table. Give the text an id in the table, and put the values in it through {StringsType}.{StringsGet} (G-8, D-98, F-132).";

    private const string ConstTextDetail = $"A const string that a text member takes is a text that a player sees, and the string table must hold it. Read it through {StringsType}.{StringsGet} (G-8, D-98, F-132).";

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
    /// <remarks>
    /// Three forms put a text on the screen outside the string table: a string literal, an interpolated string,
    /// and a const string of the file that a text member takes. The compiler lowers the last two to text that no
    /// literal node shows, so each one has its own rule (F-132).
    /// </remarks>
    public static IReadOnlyList<LintFinding> ScanText(string sourceText, string path)
    {
        SyntaxNode root = CSharpSyntaxTree.ParseText(sourceText).GetRoot();
        IReadOnlySet<string> constStrings = ConstStringNames(root);
        List<LintFinding> findings = [];

        foreach (SyntaxNode node in root.DescendantNodes())
        {
            if (node is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                // An empty string names nothing that a player reads.
                if (literal.Token.ValueText.Length > 0 && !StandsInAnAllowedPosition(literal))
                {
                    findings.Add(Create(literal, path, literal.Token.ValueText, LiteralDetail));
                }

                continue;
            }

            // Error text never reaches the screen, so an exception constructor may build it by interpolation.
            if (node is InterpolatedStringExpressionSyntax interpolated)
            {
                if (!StandsInAnAllowedPosition(interpolated) && !IsExceptionText(interpolated))
                {
                    findings.Add(Create(interpolated, path, interpolated.ToString(), InterpolationDetail));
                }

                continue;
            }

            if (node is AssignmentExpressionSyntax assignment && IsVisibleText(assignment.Left))
            {
                AddConstTextFindings(findings, assignment.Right, constStrings, path);
            }
        }

        return findings;
    }

    /// <summary>
    /// The names of the const string fields that one file declares. The scan reads the syntax alone, so a const
    /// of another file is not in the set.
    /// </summary>
    private static IReadOnlySet<string> ConstStringNames(SyntaxNode root)
    {
        HashSet<string> names = [];
        foreach (FieldDeclarationSyntax field in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            if (!field.Modifiers.Any(SyntaxKind.ConstKeyword)
                || field.Declaration.Type is not PredefinedTypeSyntax { Keyword.RawKind: (int)SyntaxKind.StringKeyword })
            {
                continue;
            }

            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
            {
                names.Add(variable.Identifier.ValueText);
            }
        }

        return names;
    }

    /// <summary>Answers whether the target of an assignment is a member that puts a text on the screen, such as <c>label.Text</c>.</summary>
    private static bool IsVisibleText(ExpressionSyntax target)
    {
        return target switch
        {
            MemberAccessExpressionSyntax access => VisibleTextMembers.Contains(access.Name.Identifier.ValueText),
            IdentifierNameSyntax name => VisibleTextMembers.Contains(name.Identifier.ValueText),
            _ => false,
        };
    }

    /// <summary>
    /// Adds a finding for each const string of the file that reaches a text member: the value itself, or a
    /// branch of a <c>?:</c>, a <c>??</c>, or a <c>+</c> in it. A const inside a call, such as the id in
    /// <c>Strings.Get(Id)</c>, does not reach the text as it is.
    /// </summary>
    private static void AddConstTextFindings(List<LintFinding> findings, ExpressionSyntax value, IReadOnlySet<string> constStrings, string path)
    {
        switch (value)
        {
            case ParenthesizedExpressionSyntax parenthesized:
                AddConstTextFindings(findings, parenthesized.Expression, constStrings, path);
                break;
            case ConditionalExpressionSyntax conditional:
                AddConstTextFindings(findings, conditional.WhenTrue, constStrings, path);
                AddConstTextFindings(findings, conditional.WhenFalse, constStrings, path);
                break;
            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AddExpression) || binary.IsKind(SyntaxKind.CoalesceExpression):
                AddConstTextFindings(findings, binary.Left, constStrings, path);
                AddConstTextFindings(findings, binary.Right, constStrings, path);
                break;
            case IdentifierNameSyntax name when constStrings.Contains(name.Identifier.ValueText):
                findings.Add(Create(name, path, name.Identifier.ValueText, ConstTextDetail));
                break;
            case MemberAccessExpressionSyntax access when constStrings.Contains(access.Name.Identifier.ValueText):
                findings.Add(Create(access, path, access.Name.Identifier.ValueText, ConstTextDetail));
                break;
        }
    }

    /// <summary>
    /// Answers whether an expression is an argument of an exception constructor: <c>new XException(...)</c>, or
    /// a target-typed <c>new(...)</c> whose declaration names an exception type. The walk stops at the statement.
    /// </summary>
    private static bool IsExceptionText(ExpressionSyntax expression)
    {
        for (SyntaxNode? above = expression.Parent; above is not null; above = above.Parent)
        {
            if (above is StatementSyntax or MemberDeclarationSyntax)
            {
                return false;
            }

            if (above is not ArgumentSyntax { Parent: ArgumentListSyntax { Parent: SyntaxNode creation } })
            {
                continue;
            }

            TypeSyntax? created = creation switch
            {
                ObjectCreationExpressionSyntax explicitNew => explicitNew.Type,
                ImplicitObjectCreationExpressionSyntax { Parent: EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration } } } => declaration.Type,
                _ => null,
            };

            string typeName = created switch
            {
                QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
                SimpleNameSyntax simple => simple.Identifier.ValueText,
                _ => string.Empty,
            };

            if (typeName.EndsWith("Exception", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Answers whether a literal or an interpolated string stands where the engine needs a name, and not where
    /// a player reads text. The rule reads the call that holds the text, and a constant declaration.
    /// </summary>
    private static bool StandsInAnAllowedPosition(ExpressionSyntax literal)
    {
        // A const string is a name that the code declares once, such as a scene path or a table id. A use of the
        // const at a text member is a finding of its own (F-132).
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

                // A `Load` call takes a path only when its receiver is an engine loader (F-132).
                if (access.Name.Identifier.ValueText == LoadMethod)
                {
                    return access.Expression is SimpleNameSyntax loader && ResourceLoaders.Contains(loader.Identifier.ValueText);
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

    /// <summary>One L-STRING finding at the position of a node. The line and the column both count from one.</summary>
    private static LintFinding Create(SyntaxNode node, string path, string symbol, string detail)
    {
        FileLinePositionSpan span = node.SyntaxTree.GetLineSpan(node.Span);
        return new LintFinding(path, span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1, "L-STRING", symbol, detail);
    }
}
