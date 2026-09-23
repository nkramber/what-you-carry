using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The register lookup command of <c>AGENTS.md</c> (D-378). Review P2-1 of PR #78: the command held fixed ids and no
/// place for the ids of the task. These tests read the command from the agent file and apply its patterns to the
/// committed registers.
/// </summary>
[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
public sealed class RegisterLookupTests
{
    private static readonly Regex VariableLine = new(@"^d='[0-9|]+'; q='[0-9|]+'$");

    private static readonly Regex GrepLine = new("^grep -n -E \"(?<pattern>[^\"]+)\" (?<file>\\S+)(?: \\| grep -E '(?<filter>[^']+)')?");

    [Fact]
    public void LookupCommandTakesTheIdsOfTheTask()
    {
        List<string> block = LookupBlock();

        Assert.Matches(VariableLine, block[0]);
        List<string> greps = block.Skip(1).ToList();
        Assert.Equal(3, greps.Count);
        foreach (string line in greps)
        {
            Assert.True(line.Contains("($d)", StringComparison.Ordinal) || line.Contains("($q)", StringComparison.Ordinal), $"The lookup line '{line}' does not read the ids from d or q.");
            Assert.DoesNotMatch(new Regex(@"(D|OQ)-\([0-9]"), line);
        }
    }

    [Fact]
    public void LookupCommandFindsTheRowsOfNewDecisionsAndQuestions()
    {
        // The regression check of P2-1: the ids of this PR and two open questions.
        List<string> decisions = Run("docs/decisions.md", 0, "379|380|381|382");
        List<string> questions = Run("docs/questions.md", 2, "9|44");

        foreach (string id in new[] { "D-379", "D-380", "D-381", "D-382" })
        {
            Assert.Single(decisions, line => line.StartsWith($"| {id} |", StringComparison.Ordinal));
        }

        Assert.Equal(2, questions.Count);
        Assert.Contains(questions, line => line.StartsWith("9. **OQ-9.", StringComparison.Ordinal));
        Assert.Contains(questions, line => line.StartsWith("44. **OQ-44.", StringComparison.Ordinal));
    }

    [Fact]
    public void LookupCommandFindsEachRevision()
    {
        // D-186: D-377 revises D-187 in part, and D-381 revises D-374 in part. The revision line finds both revisers.
        List<string> revisions = Run("docs/decisions.md", 1, "187|374");

        Assert.Contains(revisions, line => line.StartsWith("| D-377 |", StringComparison.Ordinal));
        Assert.Contains(revisions, line => line.StartsWith("| D-381 |", StringComparison.Ordinal));
    }

    /// <summary>Applies grep line <paramref name="index"/> of the command, with its ids, to the register it names.</summary>
    private static List<string> Run(string expectedFile, int index, string ids)
    {
        string line = LookupBlock().Skip(1).ElementAt(index);
        Match grep = GrepLine.Match(line);
        Assert.True(grep.Success, $"The lookup line '{line}' does not have the form grep -n -E \"<pattern>\" <file>.");
        Assert.Equal(expectedFile, grep.Groups["file"].Value);

        var pattern = new Regex(grep.Groups["pattern"].Value.Replace("$d", ids, StringComparison.Ordinal).Replace("$q", ids, StringComparison.Ordinal));
        Regex? filter = grep.Groups["filter"].Success ? new Regex(grep.Groups["filter"].Value) : null;
        return RepositoryRoot.ReadFile(expectedFile).Split('\n')
            .Where(text => pattern.IsMatch(text) && (filter is null || filter.IsMatch(text)))
            .ToList();
    }

    /// <summary>The lines of the fenced block after the D-378 sentence of <c>AGENTS.md</c>.</summary>
    private static List<string> LookupBlock()
    {
        string[] lines = RepositoryRoot.ReadFile("AGENTS.md").Split('\n');
        int sentence = Array.FindIndex(lines, line => line.Contains("(D-378)", StringComparison.Ordinal) && line.Contains("docs/decisions.md", StringComparison.Ordinal));
        Assert.True(sentence >= 0, "AGENTS.md has no register lookup sentence that cites D-378.");
        int open = Array.IndexOf(lines, "```", sentence);
        int close = open < 0 ? -1 : Array.IndexOf(lines, "```", open + 1);
        Assert.True(open > sentence && close > open, "AGENTS.md has no fenced block after the register lookup sentence.");
        return lines[(open + 1)..close].ToList();
    }
}
