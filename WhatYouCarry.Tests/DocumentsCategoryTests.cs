using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The Documents category holds each test class that reads a document of the checkout (D-476). The documents job of
/// <c>ci.yml</c> runs the category on each head, so a head that skips the three build jobs still runs these tests.
/// </summary>
[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
public sealed class DocumentsCategoryTests
{
    public const string DocumentsCategory = "Documents";

    /// <summary>The line that tags a class with the category. It stands on the line above the class.</summary>
    public const string CategoryLine = "[Trait(\"Category\", DocumentsCategoryTests.DocumentsCategory)]";

    /// <summary>
    /// A literal that names a path of the skip set (D-475). A test that reads the checkout and holds one of these
    /// reads a document. The check reads text, so a path that a test builds in another way escapes it.
    /// </summary>
    private static readonly string[] DocumentLiterals = ["\"docs/", "\"docs\"", "\"CLAUDE.md\"", "\"AGENTS.md\"", "\".claude", "\"README.md\"", "\"LICENSE\""];

    private const string CheckoutRead = "RepositoryRoot.";

    [Fact]
    public void EveryTestThatReadsADocumentTakesTheCategory()
    {
        string directory = Path.Combine(RepositoryRoot.Find(), "WhatYouCarry.Tests");
        List<string> tagged = [];
        foreach (string path in Directory.GetFiles(directory, "*.cs").OrderBy(path => path, StringComparer.Ordinal))
        {
            string source = File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
            string name = Path.GetFileName(path);
            if (!ReadsADocument(source))
            {
                continue;
            }

            Assert.True(source.Contains(CategoryLine + "\npublic sealed class ", StringComparison.Ordinal), $"The file '{name}' reads a document of the checkout, and its class lacks the line {CategoryLine} (D-476).");
            tagged.Add(name);
        }

        // The classes of 2026-09-22. A new class joins the list. A class that leaves it needs a reason.
        string[] expected =
        [
            "CiSkipTests.cs", "ContextBudgetTests.cs", "DocumentsCategoryTests.cs", "HandoffRotateTests.cs", "RegisterLookupTests.cs",
            "RepositoryShapeTests.cs", "ReviewGateRulesTests.cs", "SteCheckTests.cs", "UserArgumentsTests.cs",
        ];
        Assert.Equal(expected, tagged);
    }

    [Fact]
    public void ReadsADocumentFindsTheLiteralAndTheRead()
    {
        Assert.True(ReadsADocument("string text = RepositoryRoot.ReadFile(\"AGENTS.md\");"));
        Assert.True(ReadsADocument("string root = RepositoryRoot.Find();\nstring path = Path.Combine(root, \".claude\", \"skills\");"));
        Assert.False(ReadsADocument("string text = RepositoryRoot.ReadFile(\"content/floors/tiers.json\");"));
        Assert.False(ReadsADocument("Finding finding = SteChecker.Check(\"docs/x.md\", text);"));
    }

    [Fact]
    public void DocumentsJobRunsTheCategoryOnEachHead()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/ci.yml");
        string job = WorkflowText.JobText(workflow, "documents");

        Assert.Contains($"dotnet test WhatYouCarry.slnx --no-build --filter \"Category={DocumentsCategory}\"", job, StringComparison.Ordinal);
        Assert.DoesNotContain("needs:", job, StringComparison.Ordinal);
        Assert.DoesNotContain("if:", job, StringComparison.Ordinal);
    }

    private static bool ReadsADocument(string source)
    {
        return source.Contains(CheckoutRead, StringComparison.Ordinal)
            && DocumentLiterals.Any(literal => source.Contains(literal, StringComparison.Ordinal));
    }
}
