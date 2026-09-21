using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace WhatYouCarry.Tools.HandoffRotate;

/// <summary>One <c>## Session N</c> entry: its number and its text from the heading to the next heading.</summary>
public sealed record HandoffEntry(int Number, string Text);

/// <summary>A handoff file split into the text before the first entry and the entries, in file order.</summary>
public sealed record HandoffFile(string Preamble, IReadOnlyList<HandoffEntry> Entries);

/// <summary>
/// The two new file texts, the numbers of the moved entries, the numbers of the entries that the sort put back in
/// place, and the next session number (D-187, D-406).
/// </summary>
public sealed record HandoffRotation(string Handoff, string Archive, IReadOnlyList<int> Moved, IReadOnlyList<int> Reordered, int NextSession);

/// <summary>
/// The handoff rotation (D-146, D-379). The handoff keeps the 10 newest entries, newest first. Each older entry moves,
/// with its text intact, to the top of the archive.
/// </summary>
/// <remarks>
/// <para>
/// An entry that sits under an older one goes back to its place by number, with its text intact, and the rotation
/// names it (D-406). A session that adds its entry at the end of the file then leaves the file in order. Four
/// sessions put an entry at the end before this rule, and each one failed the three build legs of its branch
/// (F-106).
/// </para>
/// <para>
/// The sort is not a silent repair. <see cref="HandoffRotation.Reordered"/> names every entry that moved, the
/// command prints it, and `RepositoryFilesHoldTheRule` asserts that the committed file needs no sort (T-2).
/// </para>
/// <para>
/// Two entries of one number stay an error, and nothing moves. A number is the identity of a session, so the tool
/// cannot know which of the two is newer (D-187).
/// </para>
/// </remarks>
public static class HandoffRotateRules
{
    public const int KeepCount = 10;

    public const string HandoffPath = "docs/session-handoff.md";

    public const string ArchivePath = "docs/session-handoff-archive.md";

    private static readonly Regex SessionHeading = new(@"^## Session (\d+)\b", RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>Splits the text at each <c>## Session N</c> heading that starts a line.</summary>
    public static HandoffFile Parse(string text)
    {
        MatchCollection matches = SessionHeading.Matches(text);
        var entries = new List<HandoffEntry>();
        for (int index = 0; index < matches.Count; index++)
        {
            Match match = matches[index];
            int end = index + 1 < matches.Count ? matches[index + 1].Index : text.Length;
            int number = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            entries.Add(new HandoffEntry(number, text[match.Index..end]));
        }

        string preamble = matches.Count == 0 ? text : text[..matches[0].Index];
        return new HandoffFile(preamble, entries);
    }

    /// <summary>
    /// Moves every handoff entry after the tenth to the top of the archive. When the handoff holds 10 entries or
    /// fewer, both texts come back unchanged. Moved entries and the kept entries end with one blank line between
    /// entries, and each file ends with one line break. The archive text below its first old entry never changes.
    /// </summary>
    public static HandoffRotation Rotate(string handoffText, string archiveText)
    {
        HandoffFile handoff = Parse(handoffText);
        if (handoff.Entries.Count == 0)
        {
            throw new InvalidOperationException($"'{HandoffPath}' has no '## Session <number>' heading, so the newest session is unknown.");
        }

        CheckNoDuplicate(handoff.Entries);
        IReadOnlyList<HandoffEntry> ordered = SortNewestFirst(handoff.Entries, out IReadOnlyList<int> reordered);
        int nextSession = ordered[0].Number + 1;
        if (ordered.Count <= KeepCount)
        {
            string sorted = reordered.Count == 0 ? handoffText : handoff.Preamble + JoinEntries(ordered) + "\n";
            return new HandoffRotation(sorted, archiveText, [], reordered, nextSession);
        }

        List<HandoffEntry> kept = ordered.Take(KeepCount).ToList();
        List<HandoffEntry> moved = ordered.Skip(KeepCount).ToList();
        HandoffFile archive = Parse(archiveText);
        if (archive.Entries.Count > 0 && archive.Entries[0].Number >= moved[^1].Number)
        {
            throw new InvalidOperationException(
                $"'{ArchivePath}' starts with Session {archive.Entries[0].Number}, and the handoff moves Session {moved[^1].Number} to the archive. " +
                "The archive top must be older than every moved entry. A rotation that stopped after the archive write leaves this state. " +
                $"Remove the copies of the moved entries from the top of '{ArchivePath}', then run the command again.");
        }

        string newHandoff = handoff.Preamble + JoinEntries(kept) + "\n";
        string movedText = JoinEntries(moved);
        string newArchive;
        if (archive.Entries.Count == 0)
        {
            newArchive = WithBlankLineEnd(archiveText) + movedText + "\n";
        }
        else
        {
            string oldEntries = archiveText[archive.Preamble.Length..];
            newArchive = WithBlankLineEnd(archive.Preamble) + movedText + "\n\n" + oldEntries;
        }

        return new HandoffRotation(newHandoff, newArchive, moved.Select(entry => entry.Number).ToList(), reordered, nextSession);
    }

    /// <summary>
    /// The entries by number, newest first, with the text of each one intact. Names every entry whose place the
    /// sort changed, in the order that the file held them (D-406). A swap of two entries names both.
    /// </summary>
    public static IReadOnlyList<HandoffEntry> SortNewestFirst(IReadOnlyList<HandoffEntry> entries, out IReadOnlyList<int> reordered)
    {
        List<HandoffEntry> ordered = entries.OrderByDescending(entry => entry.Number).ToList();
        List<int> moved = [];
        for (int index = 0; index < entries.Count; index++)
        {
            if (entries[index].Number != ordered[index].Number)
            {
                moved.Add(entries[index].Number);
            }
        }

        reordered = moved;
        return ordered;
    }

    /// <summary>Stops a file that holds one session number two times, because a number is the identity of a session (D-187, T-2).</summary>
    private static void CheckNoDuplicate(IReadOnlyList<HandoffEntry> entries)
    {
        var seen = new HashSet<int>();
        foreach (HandoffEntry entry in entries)
        {
            if (!seen.Add(entry.Number))
            {
                throw new InvalidOperationException(
                    $"'{HandoffPath}' holds Session {entry.Number} two times (D-187). Give the newer entry the next free number, then run the command again.");
            }
        }
    }

    private static string JoinEntries(IEnumerable<HandoffEntry> entries)
    {
        return string.Join("\n\n", entries.Select(entry => entry.Text.TrimEnd()));
    }

    private static string WithBlankLineEnd(string text)
    {
        string trimmed = text.TrimEnd();
        return trimmed.Length == 0 ? "" : trimmed + "\n\n";
    }
}
