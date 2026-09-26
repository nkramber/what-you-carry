using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// Each action that a workflow or a local action uses has a dependency entry in the decision register (G-16, D-621).
/// PR-85 added <c>actions/download-artifact</c> with no entry, and <c>actions/checkout</c> had none (F-154).
/// </summary>
[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
public sealed class ActionDecisionTests
{
    /// <summary>The start of a step that uses an action, after the indent and an optional list dash.</summary>
    private const string UsesKey = "uses: ";

    /// <summary>The start of a local action path, which is code of this repository and no dependency.</summary>
    private const string LocalPrefix = "./";

    /// <summary>Each action of the repository and the decision that is its dependency entry.</summary>
    private static readonly Dictionary<string, string> DecisionOfAction = new(StringComparer.Ordinal)
    {
        ["actions/checkout"] = "D-620",
        ["actions/setup-dotnet"] = "D-189",
        ["actions/upload-artifact"] = "D-280",
        ["actions/download-artifact"] = "D-619",
        ["actions/cache"] = "D-294",
    };

    /// <summary>Every action that a file under <c>.github/</c> uses maps to a decision, and each mapped decision names its action.</summary>
    [Fact]
    public void EachActionHasADependencyDecision()
    {
        string root = RepositoryRoot.Find();
        List<string> defects = [];
        HashSet<string> used = new(StringComparer.Ordinal);
        foreach (string file in Directory.GetFiles(Path.Combine(root, ".github"), "*.yml", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            foreach (string action in ActionsOf(File.ReadAllText(file)))
            {
                used.Add(action);
                if (!DecisionOfAction.ContainsKey(action))
                {
                    defects.Add($"{relative} uses '{action}', and no decision is its dependency entry (G-16).");
                }
            }
        }

        string decisions = RepositoryRoot.ReadFile("docs/decisions.md");
        foreach (KeyValuePair<string, string> entry in DecisionOfAction)
        {
            string? row = DecisionRow(decisions, entry.Value);
            if (row is null || !row.Contains($"`{entry.Key}`", StringComparison.Ordinal))
            {
                defects.Add($"The decision {entry.Value} does not name '{entry.Key}'.");
            }

            if (!used.Contains(entry.Key))
            {
                defects.Add($"No file under .github/ uses '{entry.Key}', so its map entry is dead.");
            }
        }

        Assert.True(defects.Count == 0, string.Join(Environment.NewLine, defects));
    }

    /// <summary>The scan reads an action with a list dash or without one, drops the version, and skips a local action and a remark.</summary>
    [Fact]
    public void TheScanReadsEachUsesLine()
    {
        const string Workflow = """
            steps:
              - uses: actions/checkout@v5
              - name: Cache
                uses: actions/cache@v4
              - uses: ./.github/actions/ci-skip
              # uses: actions/other@v1
            """;

        Assert.Equal(["actions/checkout", "actions/cache"], ActionsOf(Workflow));
    }

    /// <summary>The actions that one workflow text uses, in file order, with no version and no local action.</summary>
    private static List<string> ActionsOf(string workflow)
    {
        List<string> actions = [];
        foreach (string rawLine in workflow.Split('\n'))
        {
            string line = rawLine.Trim();
            if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                line = line[2..].TrimStart();
            }

            if (!line.StartsWith(UsesKey, StringComparison.Ordinal))
            {
                continue;
            }

            string value = line[UsesKey.Length..].Trim();
            if (value.StartsWith(LocalPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            int at = value.IndexOf('@', StringComparison.Ordinal);
            actions.Add(at < 0 ? value : value[..at]);
        }

        return actions;
    }

    /// <summary>The table row of one decision id, or null when the register holds none.</summary>
    private static string? DecisionRow(string decisions, string id)
    {
        foreach (string line in decisions.Split('\n'))
        {
            if (line.StartsWith($"| {id} |", StringComparison.Ordinal))
            {
                return line;
            }
        }

        return null;
    }
}
