using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WhatYouCarry.Tools.HandoffRotate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The context budget (D-382). An agent session loads the agent file on each model call, and it reads the skills and
/// the newest handoff entry early. Each byte there costs tokens on every later call of the session. These ceilings
/// hold the sizes that the token audit of 2026-09-16 measured, with room for a short correction. A larger ceiling
/// needs a decision.
/// </summary>
public sealed class ContextBudgetTests
{
    private const int AgentFileByteCeiling = 15000;

    private const int SkillByteCeiling = 12000;

    private const int ReviewSkillByteCeiling = 31000;

    private const int NewestHandoffEntryByteCeiling = 7000;

    private const int HandoffFileByteCeiling = 60000;

    [Fact]
    public void AgentFilesStayUnderTheCeiling()
    {
        foreach (string path in new[] { "CLAUDE.md", "AGENTS.md" })
        {
            AssertUnder(path, ByteCount(RepositoryRoot.ReadFile(path)), AgentFileByteCeiling);
        }
    }

    [Fact]
    public void EachSkillStaysUnderItsCeiling()
    {
        string directory = Path.Combine(RepositoryRoot.Find(), ".claude", "skills");
        string[] skills = Directory.GetDirectories(directory).Select(Path.GetFileName).OfType<string>().OrderBy(name => name, StringComparer.Ordinal).ToArray();
        Assert.Contains("pr-review", skills);
        foreach (string name in skills)
        {
            string path = $".claude/skills/{name}/SKILL.md";
            int ceiling = name == "pr-review" ? ReviewSkillByteCeiling : SkillByteCeiling;
            AssertUnder(path, ByteCount(RepositoryRoot.ReadFile(path)), ceiling);
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
            $"'{source}' holds {bytes} bytes, and the ceiling is {ceiling} bytes (D-382). Move detail to a skill or a register, or ask the owner for a new ceiling.");
    }
}
