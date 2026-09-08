using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WhatYouCarry.Tools.DetLint;

/// <summary>
/// Scans the Core source for the symbols that <see cref="BannedSymbols"/> lists (D-67, G-2, G-21).
/// </summary>
/// <remarks>
/// <para>
/// The scan compiles the sources with the C# compiler API and asks the compiler what each name means (D-202).
/// It reads symbols and never source words. A name in a comment or a string is not a symbol, and a member that
/// a Core type declares for itself is a different symbol from the reflection member of the same name (F-64).
/// </para>
/// <para>
/// A word list cannot do this. `probe.GetMethods()` on a Core type and `type.GetMethods()` on System.Type share
/// every letter, and `Vector3` can name the Core type that a later PR declares. The compiler tells them apart.
/// </para>
/// </remarks>
public static class CoreSourceScan
{
    /// <summary>The directory of the Core project, relative to the checkout root.</summary>
    public const string CoreDirectory = "WhatYouCarry.Core";

    // The C# version of Directory.Build.props. A source that the lint cannot parse is a finding, never a pass.
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

    private static IReadOnlyList<MetadataReference>? cachedReferences;

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

    /// <summary>
    /// Every finding in the Core source of one checkout, in file order. The whole project compiles together, so
    /// one Core file may name a type that another one declares.
    /// </summary>
    public static IReadOnlyList<LintFinding> Run(string checkoutRoot)
    {
        List<(string Path, string Text)> sources = [];
        foreach (string file in SourceFiles(checkoutRoot))
        {
            string relativePath = Path.GetRelativePath(checkoutRoot, file).Replace('\\', '/');
            sources.Add((relativePath, File.ReadAllText(file)));
        }

        // Core must compile for the rules to mean anything. A source that does not compile resolves no symbol,
        // and every rule below it would pass in silence, so the whole project reports its errors here (T-2).
        return Scan(sources, reportEveryCompilerError: true);
    }

    /// <summary>
    /// Every finding in one source text. The path names the file in each finding and selects the DetMath rule.
    /// </summary>
    /// <remarks>
    /// One text alone often names a type that it does not declare. An unresolved name gives no finding, because
    /// the scan cannot know what it means. <see cref="Run"/> reports a Core source that does not compile.
    /// </remarks>
    public static IReadOnlyList<LintFinding> ScanText(string sourceText, string path)
    {
        return Scan([(path, sourceText)], reportEveryCompilerError: false);
    }

    /// <summary>Compiles the sources together, then reads every name through the compiler.</summary>
    private static IReadOnlyList<LintFinding> Scan(IReadOnlyList<(string Path, string Text)> sources, bool reportEveryCompilerError)
    {
        List<SyntaxTree> trees = [];
        foreach ((string path, string text) in sources)
        {
            trees.Add(CSharpSyntaxTree.ParseText(text, ParseOptions, path));
        }

        CSharpCompilation compilation = CSharpCompilation.Create(
            "WhatYouCarry.Core.Lint",
            trees,
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Without the runtime references every name resolves to nothing, and the scan would report no finding
        // for any file. That is the one failure this tool must never have in silence (T-2).
        if (compilation.GetTypeByMetadataName("System.Math") is null
            || compilation.GetTypeByMetadataName("System.Reflection.Assembly") is null)
        {
            throw new InvalidOperationException(
                "The lint compilation resolved no runtime reference, so no rule can read a symbol. The scan would pass every file. Check TRUSTED_PLATFORM_ASSEMBLIES.");
        }

        List<LintFinding> findings = [];
        foreach (SyntaxTree tree in trees)
        {
            string path = tree.FilePath;
            AddCompilerErrors(findings, reportEveryCompilerError ? compilation.GetDiagnostics() : tree.GetDiagnostics(), path);

            // A conditional directive is trivia, so the node walk below never reaches it. The rule reads `#if`
            // alone, which gives one finding for each conditional block (D-204).
            foreach (SyntaxNode directive in tree.GetRoot().DescendantNodes(descendIntoTrivia: true))
            {
                if (directive is IfDirectiveTriviaSyntax)
                {
                    findings.Add(Create(directive, path, "L-CONDITIONAL", "#if", BannedSymbols.ConditionalDetail));
                }
            }

            SemanticModel model = compilation.GetSemanticModel(tree);
            bool isDetMath = path.Replace('\\', '/').Equals(BannedSymbols.DetMathPath, StringComparison.Ordinal);

            foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
            {
                if (node is UsingDirectiveSyntax usingDirective)
                {
                    AddImportFinding(findings, model, usingDirective, path);
                    continue;
                }

                // `typeof` is a keyword, so no symbol names it and no Core type can carry its name. It gives back
                // a System.Type value that can reach a place where no name is banned, so the keyword is the rule.
                if (node is TypeOfExpressionSyntax typeOfExpression)
                {
                    findings.Add(Create(typeOfExpression, path, "L-REFLECTION", "typeof", BannedSymbols.TypeOfDetail));
                    continue;
                }

                // Only the last name of a dotted chain. `System.Math.Sin` holds three names for one call, and the
                // last one carries the member, so the earlier names would repeat the same finding.
                if (node is SimpleNameSyntax name && !IsAccessTarget(name))
                {
                    AddNameFinding(findings, model, name, path, isDetMath);
                }
            }
        }

        return findings;
    }

    /// <summary>Adds one finding for each compiler error, so no rule fails in silence (T-2).</summary>
    private static void AddCompilerErrors(List<LintFinding> findings, IEnumerable<Diagnostic> diagnostics, string path)
    {
        foreach (Diagnostic diagnostic in diagnostics)
        {
            if (diagnostic.Severity != DiagnosticSeverity.Error || diagnostic.Location.SourceTree?.FilePath != path)
            {
                continue;
            }

            FileLinePositionSpan span = diagnostic.Location.GetLineSpan();
            findings.Add(new LintFinding(
                path,
                span.StartLinePosition.Line + 1,
                span.StartLinePosition.Character + 1,
                "L-PARSE",
                diagnostic.Id,
                $"The file does not compile, so no other rule can read its symbols. {diagnostic.GetMessage(CultureInfo.InvariantCulture)}"));
        }
    }

    /// <summary>Adds a finding when a using directive imports a banned namespace.</summary>
    private static void AddImportFinding(List<LintFinding> findings, SemanticModel model, UsingDirectiveSyntax usingDirective, string path)
    {
        if (usingDirective.Name is null || model.GetSymbolInfo(usingDirective.Name).Symbol is not INamespaceSymbol imported)
        {
            return;
        }

        string full = imported.ToDisplayString();
        if (BannedNamespace(full) is BannedSymbols.BannedName banned)
        {
            findings.Add(Create(usingDirective, path, banned.Rule, full, banned.Detail));
        }
    }

    /// <summary>Adds a finding when a name resolves to a banned type, to a member of one, or to a banned namespace.</summary>
    private static void AddNameFinding(List<LintFinding> findings, SemanticModel model, SimpleNameSyntax name, string path, bool isDetMath)
    {
        // `var` resolves to the type that the compiler inferred, and the source already names that type on the
        // right of the assignment. Without this the scan reports the same use twice.
        if (name is IdentifierNameSyntax identifier && identifier.IsVar)
        {
            return;
        }

        ISymbol? symbol = model.GetSymbolInfo(name).Symbol;
        if (symbol is null)
        {
            return;
        }

        // `dynamic` hides the call that runs, and the compiler names it a type of its own kind.
        if (symbol.Kind == SymbolKind.DynamicType)
        {
            findings.Add(Create(name, path, "L-DYNAMIC", "dynamic", "Dynamic dispatch hides the call that runs. Core is explicit (G-2, T-1)."));
            return;
        }

        // A use of the type itself, or a use of one of its members. Both belong to the same owner.
        INamedTypeSymbol? owner = symbol is INamedTypeSymbol type ? type.OriginalDefinition : symbol.ContainingType?.OriginalDefinition;
        if (owner is null)
        {
            return;
        }

        string ownerName = FullName(owner);
        bool namesTheTypeItself = symbol is INamedTypeSymbol;
        string used = namesTheTypeItself ? owner.Name : $"{owner.Name}.{symbol.Name}";
        string fullUsed = namesTheTypeItself ? ownerName : $"{ownerName}.{symbol.Name}";

        if (ownerName.Equals(BannedSymbols.MathFType, StringComparison.Ordinal))
        {
            AddMathFFinding(findings, symbol, name, path, isDetMath, used);
            return;
        }

        if (BannedSymbols.Types.TryGetValue(ownerName, out BannedSymbols.BannedName bannedType))
        {
            findings.Add(Create(name, path, bannedType.Rule, used, bannedType.Detail));
            return;
        }

        if (BannedNamespace(owner.ContainingNamespace.ToDisplayString()) is BannedSymbols.BannedName bannedNamespace)
        {
            findings.Add(Create(name, path, bannedNamespace.Rule, fullUsed, bannedNamespace.Detail));
            return;
        }

        // The allowlist is the last word on the owner. A namespace that nobody approved is a finding, so a
        // surface that no denylist names cannot reach Core (D-205).
        if (!IsAllowedNamespace(owner.ContainingNamespace))
        {
            findings.Add(Create(name, path, "L-NAMESPACE", fullUsed, BannedSymbols.NamespaceDetail));
            return;
        }

        AddProducedTypeFinding(findings, symbol, name, path, used);
    }

    /// <summary>
    /// Adds a finding when a call gives back a banned type. `object.GetType()` belongs to System.Object, so the
    /// owner rule cannot see it, and the type it gives back is System.Type (F-64).
    /// </summary>
    /// <remarks>
    /// The rule reads a method and a property alone. A local, a parameter, and a field each name their type in
    /// the source, and the owner rule reports that name, so a second rule here would report one use many times.
    /// </remarks>
    private static void AddProducedTypeFinding(List<LintFinding> findings, ISymbol symbol, SimpleNameSyntax name, string path, string used)
    {
        ITypeSymbol? produced = symbol switch
        {
            IMethodSymbol method => method.ReturnType,
            IPropertySymbol property => property.Type,
            _ => null,
        };

        if (produced is not INamedTypeSymbol producedType)
        {
            return;
        }

        string producedName = FullName(producedType.OriginalDefinition);
        if (BannedSymbols.Types.TryGetValue(producedName, out BannedSymbols.BannedName bannedType))
        {
            findings.Add(Create(name, path, bannedType.Rule, used, $"{used} gives back {producedName}. {bannedType.Detail}"));
            return;
        }

        if (BannedNamespace(producedType.ContainingNamespace.ToDisplayString()) is BannedSymbols.BannedName bannedNamespace)
        {
            findings.Add(Create(name, path, bannedNamespace.Rule, used, $"{used} gives back {producedName}. {bannedNamespace.Detail}"));
            return;
        }

        if (!IsAllowedNamespace(producedType.ContainingNamespace))
        {
            findings.Add(Create(name, path, "L-NAMESPACE", used, $"{used} gives back {producedName}. {BannedSymbols.NamespaceDetail}"));
        }
    }

    /// <summary>
    /// Answers whether Core may use a namespace. Each approved entry matches one namespace and never its
    /// children, so `System` does not approve `System.ComponentModel` (D-205).
    /// </summary>
    private static bool IsAllowedNamespace(INamespaceSymbol space)
    {
        // A type with no namespace comes from the compilation itself, never from the class library.
        if (space.IsGlobalNamespace)
        {
            return true;
        }

        string full = space.ToDisplayString();
        return full.StartsWith(BannedSymbols.ProjectNamespacePrefix, StringComparison.Ordinal)
            || BannedSymbols.AllowedNamespaces.Contains(full);
    }

    /// <summary>Adds a MathF finding unless the file is the one DetMath file and the member is an exact operation.</summary>
    private static void AddMathFFinding(List<LintFinding> findings, ISymbol symbol, SimpleNameSyntax name, string path, bool isDetMath, string used)
    {
        if (!isDetMath)
        {
            findings.Add(Create(name, path, "L-MATHF", used, $"Only {BannedSymbols.DetMathPath} may use MathF (G-2)."));
            return;
        }

        // The bare type gives no member, as `using static System.MathF` does, so the rule cannot clear it.
        if (symbol is INamedTypeSymbol || !BannedSymbols.AllowedMathFMembers.Contains(symbol.Name))
        {
            findings.Add(Create(name, path, "L-MATHF", used, $"{BannedSymbols.DetMathPath} may call only an exact IEEE operation: {string.Join(", ", BannedSymbols.AllowedMathFMembers)} (G-2)."));
        }
    }

    /// <summary>The banned rule for a namespace, or null. A whole segment must match, so System.ReflectionExtras does not.</summary>
    private static BannedSymbols.BannedName? BannedNamespace(string full)
    {
        foreach (KeyValuePair<string, BannedSymbols.BannedName> banned in BannedSymbols.Namespaces)
        {
            if (full.Equals(banned.Key, StringComparison.Ordinal) || full.StartsWith(banned.Key + ".", StringComparison.Ordinal))
            {
                return banned.Value;
            }
        }

        return null;
    }

    /// <summary>The namespace and the name of a type, without its type arguments. `Vector128&lt;float&gt;` gives the plain name.</summary>
    private static string FullName(INamedTypeSymbol type)
    {
        return type.ContainingNamespace.IsGlobalNamespace
            ? type.Name
            : $"{type.ContainingNamespace.ToDisplayString()}.{type.Name}";
    }

    /// <summary>
    /// Answers whether a name stands left of a dot in a dotted chain. In <c>System.Math.Sin</c> the names
    /// <c>System</c> and <c>Math</c> do, and <c>Sin</c> does not. The scan reads the last name alone, because it
    /// names the member and the earlier names would repeat the finding.
    /// </summary>
    private static bool IsAccessTarget(SimpleNameSyntax name)
    {
        SyntaxNode current = name;
        while (current.Parent is MemberAccessExpressionSyntax access)
        {
            if (access.Expression == current)
            {
                return true;
            }

            current = access;
        }

        return false;
    }

    /// <summary>One finding at the position of a node. The line and the column both count from one, as a compiler reports them.</summary>
    private static LintFinding Create(SyntaxNode node, string path, string rule, string symbol, string detail)
    {
        FileLinePositionSpan span = node.SyntaxTree.GetLineSpan(node.Span);
        return new LintFinding(path, span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1, rule, symbol, detail);
    }

    /// <summary>The runtime assemblies that the host runs on. The lint compiles Core against the same set.</summary>
    private static IReadOnlyList<MetadataReference> References()
    {
        if (cachedReferences is not null)
        {
            return cachedReferences;
        }

        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is not string assemblies || assemblies.Length == 0)
        {
            throw new InvalidOperationException("The host gave no TRUSTED_PLATFORM_ASSEMBLIES list, so the lint cannot compile Core against the runtime.");
        }

        List<MetadataReference> references = [];
        foreach (string assembly in assemblies.Split(Path.PathSeparator))
        {
            if (assembly.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && File.Exists(assembly))
            {
                references.Add(MetadataReference.CreateFromFile(assembly));
            }
        }

        cachedReferences = references;
        return references;
    }
}
