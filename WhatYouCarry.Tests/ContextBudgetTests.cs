using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WhatYouCarry.Tools.HandoffRotate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The context budget (D-382, D-384). An agent session loads the agent file on each model call, and it reads the
/// skills and the newest handoff entry early. Each byte there costs tokens on every later call of the session. These
/// ceilings hold the sizes that the token audit of 2026-09-16 measured, with room for a short correction. A larger
/// ceiling needs a decision. D-384 ends the separate ceiling of the review skill, and it holds every file under
/// <c>.claude/skills/</c> to one ceiling, a reference file included.
/// </summary>
[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
public sealed class ContextBudgetTests
{
    private const int AgentFileByteCeiling = 15000;

    private const int SkillByteCeiling = 12000;

    private const int NewestHandoffEntryByteCeiling = 7000;

    private const int HandoffFileByteCeiling = 60000;

    /// <summary>
    /// The agent files quote the six tenets in full (D-122), and section 6 of the design doc holds the same six lines word
    /// for word. The two copies drifted apart in four tenets before this test, and a session read the older text first
    /// (F-136).
    /// </summary>
    [Fact]
    public void TheDesignDocHoldsTheTenetsOfTheAgentFiles()
    {
        string[] agentTenets = TenetLines(RepositoryRoot.ReadFile("AGENTS.md"));
        string[] designTenets = TenetLines(RepositoryRoot.ReadFile("docs/design.md"));

        Assert.Equal(6, agentTenets.Length);
        Assert.Equal(agentTenets, designTenets);
    }

    /// <summary>Each line of a text that starts a tenet, in order.</summary>
    private static string[] TenetLines(string text)
    {
        return Array.FindAll(text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'), static line => line.StartsWith("- **T-", StringComparison.Ordinal));
    }

    [Fact]
    public void AgentFilesStayUnderTheCeiling()
    {
        foreach (string path in new[] { "CLAUDE.md", "AGENTS.md" })
        {
            AssertUnder(path, ByteCount(RepositoryRoot.ReadFile(path)), AgentFileByteCeiling);
        }
    }

    [Fact]
    public void EachSkillFileStaysUnderTheCeiling()
    {
        // D-384: every .md file under .claude/skills/ takes one ceiling, a reference file included. A skill that
        // cannot hold its detail moves that detail to a reference file, which a session loads at the step that needs it.
        string root = RepositoryRoot.Find();
        string directory = Path.Combine(root, ".claude", "skills");
        string[] files = Directory.GetFiles(directory, "*.md", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/'))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.Contains(".claude/skills/pr-review/SKILL.md", files);
        Assert.Contains(files, path => path.Contains("/references/", StringComparison.Ordinal));
        foreach (string path in files)
        {
            AssertUnder(path, ByteCount(RepositoryRoot.ReadFile(path)), SkillByteCeiling);
        }
    }

    [Fact]
    public void HandoffAndItsNewestEntryStayUnderTheCeilings()
    {
        string handoff = RepositoryRoot.ReadFile(HandoffRotateRules.HandoffPath);
        IReadOnlyList<HandoffEntry> entries = HandoffRotateRules.Parse(handoff).Entries;
        Assert.True(entries.Count > 0, $"'{HandoffRotateRules.HandoffPath}' has no '## Session <number>' entry.");

        AssertUnder(HandoffRotateRules.HandoffPath, ByteCount(handoff), HandoffFileByteCeiling);
        AssertUnder($"{HandoffRotateRules.HandoffPath}, Session {entries[0].Number}", ByteCount(entries[0].Text), NewestHandoffEntryByteCeiling);
    }

    private static int ByteCount(string text)
    {
        return Encoding.UTF8.GetByteCount(text);
    }

    private static void AssertUnder(string source, int bytes, int ceiling)
    {
        Assert.True(bytes <= ceiling,
            $"'{source}' holds {bytes} bytes, and the ceiling is {ceiling} bytes (D-382, D-384). Move detail to a reference file or a register, or ask the owner for a new ceiling.");
    }
}
