using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.HandoffRotate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The handoff rotation (D-146, D-379): the move of each entry after the tenth to the archive top, the text
/// conservation of every entry, the order errors, and the exit codes of the command.
/// </summary>
[Collection(ConsoleCollection.Name)]
public sealed class HandoffRotateTests
{
    private const string HandoffPreamble = "# Session handoff\n\nRule (D-146): the rule text.\n\n";

    private const string ArchivePreamble = "# Session handoff archive\n\n";

    [Fact]
    public void TwelveEntriesMoveTheTwoOldestToTheArchiveTop()
    {
        string handoff = HandoffPreamble + Entries(112, 101);
        string archive = ArchivePreamble + Entries(100, 99);

        HandoffRotation rotation = HandoffRotateRules.Rotate(handoff, archive);

        Assert.Equal([102, 101], rotation.Moved);
        Assert.Equal(113, rotation.NextSession);
        Assert.Equal(HandoffPreamble + Entries(112, 103), rotation.Handoff);
        Assert.Equal(ArchivePreamble + Entries(102, 99), rotation.Archive);
    }

    [Fact]
    public void TenEntriesChangeNothing()
    {
        string handoff = HandoffPreamble + Entries(20, 11) + "\n\n";
        string archive = ArchivePreamble + Entries(10, 9);

        HandoffRotation rotation = HandoffRotateRules.Rotate(handoff, archive);

        Assert.Empty(rotation.Moved);
        Assert.Equal(21, rotation.NextSession);
        Assert.Same(handoff, rotation.Handoff);
        Assert.Same(archive, rotation.Archive);
    }

    [Fact]
    public void EveryEntryKeepsItsTextOverSeeds()
    {
        // D-66: a seed loop over random entry counts, entry bodies, and trailing blank lines.
        for (int seed = 1; seed <= 300; seed++)
        {
            var random = new Random(seed);
            int handoffCount = random.Next(1, 26);
            int archiveCount = random.Next(0, 6);
            int newest = 1000 + random.Next(0, 50);
            string handoff = HandoffPreamble + RandomEntries(random, newest, handoffCount);
            string archive = ArchivePreamble + RandomEntries(random, newest - handoffCount - random.Next(0, 3), archiveCount);

            HandoffRotation rotation = HandoffRotateRules.Rotate(handoff, archive);

            List<string> before = TrimmedEntries(handoff).Concat(TrimmedEntries(archive)).ToList();
            List<string> after = TrimmedEntries(rotation.Handoff).Concat(TrimmedEntries(rotation.Archive)).ToList();
            Assert.True(before.SequenceEqual(after, StringComparer.Ordinal), $"Seed {seed}: the entry texts changed or changed order.");
            Assert.True(HandoffRotateRules.Parse(rotation.Handoff).Entries.Count == Math.Min(handoffCount, HandoffRotateRules.KeepCount), $"Seed {seed}: the handoff holds the wrong count.");
            Assert.True(rotation.Handoff.StartsWith(HandoffPreamble, StringComparison.Ordinal), $"Seed {seed}: the handoff preamble changed.");
            Assert.True(rotation.Archive.StartsWith(ArchivePreamble, StringComparison.Ordinal), $"Seed {seed}: the archive preamble changed.");
            Assert.True(rotation.Moved.Count == Math.Max(0, handoffCount - HandoffRotateRules.KeepCount), $"Seed {seed}: the moved count is wrong.");
            HandoffFile oldArchive = HandoffRotateRules.Parse(archive);
            if (oldArchive.Entries.Count > 0)
            {
                string oldEntries = archive[oldArchive.Preamble.Length..];
                Assert.True(rotation.Archive.EndsWith(oldEntries, StringComparison.Ordinal), $"Seed {seed}: the old archive entries changed.");
            }

            HandoffRotation second = HandoffRotateRules.Rotate(rotation.Handoff, rotation.Archive);
            Assert.True(second.Moved.Count == 0 && second.Handoff == rotation.Handoff, $"Seed {seed}: a second rotation changed the files.");
        }
    }

    [Fact]
    public void RepositoryFilesHoldTheRule()
    {
        // The committed handoff holds 10 entries or fewer, newest first, above an older archive top.
        string handoff = RepositoryRoot.ReadFile(HandoffRotateRules.HandoffPath);
        string archive = RepositoryRoot.ReadFile(HandoffRotateRules.ArchivePath);

        HandoffRotation rotation = HandoffRotateRules.Rotate(handoff, archive);

        Assert.Empty(rotation.Moved);

        // The tool puts an entry that it finds out of order back in place, so the committed file must already hold
        // the order. Without this line a file that breaks D-146 would reach the trunk and no check would name it
        // (D-406, F-106).
        Assert.Empty(rotation.Reordered);
        Assert.Equal(handoff, rotation.Handoff);
        Assert.True(HandoffRotateRules.Parse(archive).Entries[0].Number < HandoffRotateRules.Parse(handoff).Entries[^1].Number);
    }

    [Fact]
    public void DuplicateSessionNumberFailsAndNamesIt()
    {
        string handoff = HandoffPreamble + Entry(12) + "\n\n" + Entry(12) + "\n\n" + Entries(11, 1);

        var error = Assert.Throws<InvalidOperationException>(() => HandoffRotateRules.Rotate(handoff, ArchivePreamble));

        Assert.Contains("Session 12 two times (D-187)", error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// An entry that sits under an older one goes back to its place by number, with its text intact, and the
    /// rotation names it (D-146, D-406, F-106). A session that adds its entry at the end of the file then leaves
    /// the file in order.
    /// </summary>
    [Fact]
    public void OlderEntryAboveNewerGoesBackInPlace()
    {
        string handoff = HandoffPreamble + Entry(5) + "\n\n" + Entry(6) + "\n";

        HandoffRotation rotation = HandoffRotateRules.Rotate(handoff, ArchivePreamble);

        // Both entries changed place, so the rotation names both.
        Assert.Equal([5, 6], rotation.Reordered);
        Assert.Empty(rotation.Moved);
        Assert.Equal(7, rotation.NextSession);
        Assert.Equal(HandoffPreamble + Entry(6) + "\n\n" + Entry(5) + "\n", rotation.Handoff);

        // A file that already holds the order needs no sort, and its text does not change.
        HandoffRotation again = HandoffRotateRules.Rotate(rotation.Handoff, ArchivePreamble);
        Assert.Empty(again.Reordered);
        Assert.Equal(rotation.Handoff, again.Handoff);
    }

    /// <summary>An entry at the end of a full handoff goes back in place, and the rotation then moves the right entries (D-406).</summary>
    [Fact]
    public void AnEntryAtTheEndOfAFullHandoffGoesBackInPlace()
    {
        // Eleven entries, newest first, with the newest one moved to the end of the file.
        string ordered = HandoffPreamble + Entries(111, 101);
        HandoffFile parsed = HandoffRotateRules.Parse(ordered);
        string outOfOrder = parsed.Preamble + string.Join("\n\n", parsed.Entries.Skip(1).Select(entry => entry.Text.TrimEnd())) + "\n\n" + parsed.Entries[0].Text.TrimEnd() + "\n";

        HandoffRotation rotation = HandoffRotateRules.Rotate(outOfOrder, ArchivePreamble);

        Assert.Equal(112, rotation.NextSession);
        Assert.Equal([101], rotation.Moved);
        Assert.Contains(111, rotation.Reordered);
        Assert.Equal(HandoffRotateRules.Rotate(ordered, ArchivePreamble).Handoff, rotation.Handoff);
    }

    [Fact]
    public void ArchiveTopThatIsNotOlderFails()
    {
        // A rotation that stopped after the archive write leaves the moved entries at the archive top.
        string handoff = HandoffPreamble + Entries(111, 101);
        string archive = ArchivePreamble + Entries(101, 99);

        var error = Assert.Throws<InvalidOperationException>(() => HandoffRotateRules.Rotate(handoff, archive));

        Assert.Contains("starts with Session 101", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void HandoffWithNoEntryFails()
    {
        var error = Assert.Throws<InvalidOperationException>(() => HandoffRotateRules.Rotate(HandoffPreamble, ArchivePreamble));

        Assert.Contains("has no '## Session <number>' heading", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CommandMovesEntriesAndExitsZeroOneAndTwo()
    {
        string root = Path.Combine(Path.GetTempPath(), "wyc-handoff-rotate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "docs"));
        string handoffPath = Path.Combine(root, HandoffRotateRules.HandoffPath);
        string archivePath = Path.Combine(root, HandoffRotateRules.ArchivePath);
        try
        {
            File.WriteAllText(handoffPath, HandoffPreamble + Entries(11, 1));
            File.WriteAllText(archivePath, ArchivePreamble);
            Assert.Equal(0, Program.Main(["handoff-rotate", "--root", root]));
            Assert.Equal(HandoffPreamble + Entries(11, 2), File.ReadAllText(handoffPath));
            Assert.Equal(ArchivePreamble + Entries(1, 1), File.ReadAllText(archivePath));

            // A second run moves nothing and changes nothing.
            Assert.Equal(0, Program.Main(["handoff-rotate", "--root", root]));
            Assert.Equal(HandoffPreamble + Entries(11, 2), File.ReadAllText(handoffPath));

            // An order error exits 1 and leaves both files as they were.
            string broken = HandoffPreamble + Entry(3) + "\n\n" + Entry(3) + "\n";
            File.WriteAllText(handoffPath, broken);
            Assert.Equal(1, Program.Main(["handoff-rotate", "--root", root]));
            Assert.Equal(broken, File.ReadAllText(handoffPath));
            Assert.Equal(ArchivePreamble + Entries(1, 1), File.ReadAllText(archivePath));

            File.Delete(archivePath);
            Assert.Equal(1, Program.Main(["handoff-rotate", "--root", root]));

            Assert.Equal(2, Program.Main(["handoff-rotate"]));
            Assert.Equal(2, Program.Main(["handoff-rotate", "--root"]));
            Assert.Equal(2, Program.Main(["handoff-rotate", "--root", root, "extra"]));
            Assert.Equal(2, Program.Main(["handoff-rotate", "--file", root]));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string Entry(int number)
    {
        return $"## Session {number}: 2026-09-16, Codex\n\nAuthor: Codex\n\n### What this session did, and why\n\n- Work of session {number}.";
    }

    /// <summary>Entries from <paramref name="newest"/> down to <paramref name="oldest"/>, one blank line apart, with a final line break.</summary>
    private static string Entries(int newest, int oldest)
    {
        IEnumerable<string> entries = Enumerable.Range(oldest, newest - oldest + 1).Reverse().Select(Entry);
        return string.Join("\n\n", entries) + "\n";
    }

    private static string RandomEntries(Random random, int newest, int count)
    {
        var text = new StringBuilder();
        for (int index = 0; index < count; index++)
        {
            text.Append("## Session ").Append(newest - index).Append(": 2026-09-16, Claude Code\n");
            int lines = random.Next(0, 6);
            for (int line = 0; line < lines; line++)
            {
                text.Append(random.Next(0, 3) == 0 ? "\n" : $"- Line {random.Next()} with ## Session {random.Next(0, 9)} inside.\n");
            }

            text.Append('\n', random.Next(0, 4));
        }

        return text.ToString();
    }

    private static List<string> TrimmedEntries(string text)
    {
        return HandoffRotateRules.Parse(text).Entries.Select(entry => entry.Text.TrimEnd()).ToList();
    }
}
