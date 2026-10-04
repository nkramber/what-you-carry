using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// Each action that a workflow or a local action uses has a dependency entry in the decision register (G-16, D-621).
/// PR-85 added <c>actions/download-artifact</c> with no entry, and <c>actions/checkout</c> had none (F-154). Each use
/// names the full commit of the action and its release in a remark, because a tag can move (D-636, F-171).
/// </summary>
[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
public sealed class ActionDecisionTests
{
    /// <summary>The start of a step that uses an action, after the indent and an optional list dash.</summary>
    private const string UsesKey = "uses: ";

    /// <summary>The start of a local action path, which is code of this repository and no dependency.</summary>
    private const string LocalPrefix = "./";

    /// <summary>The pinned use of <c>actions/checkout</c> (D-620, D-636, D-768).</summary>
    public const string Checkout = "actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1";

    /// <summary>The pinned use of <c>actions/setup-dotnet</c> (D-189, D-636, D-768).</summary>
    public const string SetupDotnet = "actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68 # v6.0.0";

    /// <summary>The pinned use of <c>actions/cache</c> (D-294, D-636, D-768).</summary>
    public const string Cache = "actions/cache@55cc8345863c7cc4c66a329aec7e433d2d1c52a9 # v6.1.0";

    /// <summary>The pinned use of <c>actions/upload-artifact</c> (D-280, D-636, D-768).</summary>
    public const string UploadArtifact = "actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a # v7.0.1";

    /// <summary>The pinned use of <c>actions/download-artifact</c> (D-619, D-636, D-768).</summary>
    public const string DownloadArtifact = "actions/download-artifact@3e5f45b2cfb9172054b4087a40e8e0b5a5461e7c # v8.0.1";

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

    /// <summary>
    /// F-171. Each use of an action names a full commit of 40 hexadecimal digits and its release in a remark, and each use
    /// of one action names one commit (D-636). The old uses named a major tag, which the owner of the action can move.
    /// </summary>
    [Fact]
    public void EachActionUseNamesAPinnedCommit()
    {
        string root = RepositoryRoot.Find();
        List<string> defects = [];
        Dictionary<string, string> pinOfAction = new(StringComparer.Ordinal);
        foreach (string file in Directory.GetFiles(Path.Combine(root, ".github"), "*.yml", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            foreach (string use in UsesOf(File.ReadAllText(file)))
            {
                string? defect = PinDefect(use);
                if (defect is not null)
                {
                    defects.Add($"{relative}: {defect}");
                    continue;
                }

                string action = use[..use.IndexOf('@', StringComparison.Ordinal)];
                if (pinOfAction.TryGetValue(action, out string? pin) && pin != use)
                {
                    defects.Add($"{relative}: '{use}' and '{pin}' pin one action to two commits.");
                }

                pinOfAction[action] = use;
            }
        }

        Assert.True(defects.Count == 0, string.Join(Environment.NewLine, defects));
        Assert.Equal([Cache, Checkout, DownloadArtifact, SetupDotnet, UploadArtifact], pinOfAction.Values.Order(StringComparer.Ordinal));
    }

    /// <summary>F-171. Dependabot proposes the pins of the actions of the workflows and of the local actions each week (D-636).</summary>
    [Fact]
    public void DependabotProposesThePins()
    {
        string config = RepositoryRoot.ReadFile(".github/dependabot.yml");
        Assert.Contains("version: 2", config, StringComparison.Ordinal);
        Assert.Contains("  - package-ecosystem: github-actions", config, StringComparison.Ordinal);
        Assert.Contains("      - \"/\"", config, StringComparison.Ordinal);
        Assert.Contains("      - \"/.github/actions/*\"", config, StringComparison.Ordinal);
        Assert.Contains("      interval: weekly", config, StringComparison.Ordinal);
    }

    /// <summary>
    /// One Dependabot PR bumps one action in every directory (D-777). The bumps #112 and #115 moved setup-dotnet in two PRs,
    /// and the merge of #115 alone left the action on two pins.
    /// </summary>
    [Fact]
    public void DependabotBumpsOneActionInEveryDirectoryInOnePr()
    {
        const string Group = """
                groups:
                  each-action:
                    patterns:
                      - "*"
                    group-by: dependency-name
            """;

        string config = RepositoryRoot.ReadFile(".github/dependabot.yml").Replace("\r\n", "\n", StringComparison.Ordinal);
        Assert.Contains(Group.Replace("\r\n", "\n", StringComparison.Ordinal), config, StringComparison.Ordinal);
    }

    /// <summary>
    /// One Dependabot security PR moves each action with an advisory in every directory (D-779). The group of
    /// <c>group-by: dependency-name</c> applies to version updates alone, so a security update needs a group of its own.
    /// </summary>
    [Fact]
    public void DependabotGroupsSecurityUpdatesInEveryDirectory()
    {
        const string Group = """
                  security:
                    applies-to: security-updates
                    patterns:
                      - "*"
            """;

        string config = RepositoryRoot.ReadFile(".github/dependabot.yml").Replace("\r\n", "\n", StringComparison.Ordinal);
        Assert.Contains(Group.Replace("\r\n", "\n", StringComparison.Ordinal), config, StringComparison.Ordinal);
    }

    /// <summary>F-171. The pin check takes a full commit with its release remark alone.</summary>
    [Theory]
    [InlineData("actions/checkout@v5", false)]
    [InlineData("actions/checkout@fbc6f3992d24b796d5a048ff273f7fcc4a7b6c09", false)]
    [InlineData("actions/checkout@fbc6f39 # v5.1.0", false)]
    [InlineData("actions/checkout@FBC6F3992D24B796D5A048FF273F7FCC4A7B6C09 # v5.1.0", false)]
    [InlineData(Checkout, true)]
    public void ThePinCheckTakesAFullCommitWithItsRelease(string use, bool pinned)
    {
        Assert.Equal(pinned, PinDefect(use) is null);
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

    /// <summary>The defect of one use of an action, or null when it names a full commit and its release in a remark (D-636).</summary>
    private static string? PinDefect(string use)
    {
        int at = use.IndexOf('@', StringComparison.Ordinal);
        int remark = use.IndexOf(" # v", StringComparison.Ordinal);
        if (at < 0 || remark < 0 || remark - at - 1 != 40)
        {
            return $"'{use}' names no full commit with its release in a remark (D-636).";
        }

        foreach (char digit in use[(at + 1)..remark])
        {
            if (!char.IsAsciiHexDigitLower(digit))
            {
                return $"'{use}' names a commit that is not 40 lowercase hexadecimal digits (D-636).";
            }
        }

        return null;
    }

    /// <summary>The whole value of each <c>uses:</c> line of one workflow text that names no local action.</summary>
    private static List<string> UsesOf(string workflow)
    {
        List<string> uses = [];
        foreach (string rawLine in workflow.Split('\n'))
        {
            string line = rawLine.Trim();
            if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                line = line[2..].TrimStart();
            }

            if (line.StartsWith(UsesKey, StringComparison.Ordinal) && !line[UsesKey.Length..].StartsWith(LocalPrefix, StringComparison.Ordinal))
            {
                uses.Add(line[UsesKey.Length..].Trim());
            }
        }

        return uses;
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
