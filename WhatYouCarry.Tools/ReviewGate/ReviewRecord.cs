using System;

namespace WhatYouCarry.Tools.ReviewGate;

/// <summary>The two machine-read parts of a review file: the head in the Identity list and the verdict (D-179, D-269).</summary>
/// <remarks>
/// The Verdict section starts at the line that is exactly the Verdict heading and ends at the next heading. It
/// holds one verdict name. A heading that starts with the same words, such as a history of earlier verdicts, is
/// another section, and a section with two names is an error that names both, never the first one (F-89).
/// </remarks>
public sealed record ReviewRecord(string RecordedHead, string Verdict)
{
    public const string HeadPrefix = "- Head: ";
    public const string VerdictHeading = "## Verdict";
    public static readonly string[] VerdictNames = ["Ready for owner merge", "Changes required", "Blocked"];

    /// <summary>Parses a review file. Returns null and an error that names the missing part when the file does not hold both parts.</summary>
    public static ReviewRecord? TryParse(string text, out string error)
    {
        string? head = FindHead(text);
        if (head is null)
        {
            error = $"The review file has no line that starts with '{HeadPrefix}' and holds a hash in backticks.";
            return null;
        }

        string? verdict = FindVerdict(text, out error);
        if (verdict is null)
        {
            return null;
        }

        return new ReviewRecord(head, verdict);
    }

    private static string? FindHead(string text)
    {
        foreach (string rawLine in text.Split('\n'))
        {
            string line = rawLine.Trim();
            if (!line.StartsWith(HeadPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            string rest = line[HeadPrefix.Length..];
            int open = rest.IndexOf('`');
            int close = open < 0 ? -1 : rest.IndexOf('`', open + 1);
            if (open < 0 || close <= open + 1)
            {
                return null;
            }

            return rest[(open + 1)..close].Trim();
        }

        return null;
    }

    /// <summary>
    /// The one verdict name of the Verdict section, or null with an error: no section with the exact heading,
    /// no verdict name in it, or more than one.
    /// </summary>
    private static string? FindVerdict(string text, out string error)
    {
        string[] lines = text.Split('\n');
        int headingLine = -1;
        for (int index = 0; index < lines.Length; index++)
        {
            if (lines[index].TrimEnd() == VerdictHeading)
            {
                headingLine = index;
                break;
            }
        }

        if (headingLine < 0)
        {
            error = $"The review file has no '{VerdictHeading}' section that names one of: {string.Join(", ", VerdictNames)}.";
            return null;
        }

        System.Text.StringBuilder section = new();
        for (int index = headingLine + 1; index < lines.Length && !lines[index].StartsWith("## ", StringComparison.Ordinal); index++)
        {
            section.Append(lines[index]);
            section.Append('\n');
        }

        string body = section.ToString();
        // Every verdict name in the section, in the order that the section holds them.
        System.Collections.Generic.List<(int At, string Name)> hits = [];
        foreach (string name in VerdictNames)
        {
            int at = body.IndexOf(name, StringComparison.Ordinal);
            while (at >= 0)
            {
                hits.Add((at, name));
                at = body.IndexOf(name, at + name.Length, StringComparison.Ordinal);
            }
        }

        hits.Sort(static (first, second) => first.At.CompareTo(second.At));
        System.Collections.Generic.List<string> found = [];
        foreach ((int At, string Name) hit in hits)
        {
            found.Add(hit.Name);
        }

        if (found.Count == 0)
        {
            error = $"The review file has no '{VerdictHeading}' section that names one of: {string.Join(", ", VerdictNames)}.";
            return null;
        }

        if (found.Count > 1)
        {
            error = $"The '{VerdictHeading}' section of the review file names {found.Count} verdicts: {string.Join(", ", found)}. It must name one. An earlier verdict belongs in a section whose heading starts with another word (D-269).";
            return null;
        }

        error = string.Empty;
        return found[0];
    }
}
