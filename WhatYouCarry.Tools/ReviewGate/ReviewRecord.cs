using System;

namespace WhatYouCarry.Tools.ReviewGate;

/// <summary>The two machine-read parts of a review file: the head in the Identity list and the verdict (D-179).</summary>
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

        string? verdict = FindVerdict(text);
        if (verdict is null)
        {
            error = $"The review file has no '{VerdictHeading}' section that names one of: {string.Join(", ", VerdictNames)}.";
            return null;
        }

        error = string.Empty;
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

    private static string? FindVerdict(string text)
    {
        int sectionStart = text.IndexOf(VerdictHeading, StringComparison.Ordinal);
        if (sectionStart < 0)
        {
            return null;
        }

        string section = text[(sectionStart + VerdictHeading.Length)..];
        int nextHeading = section.IndexOf("\n## ", StringComparison.Ordinal);
        if (nextHeading >= 0)
        {
            section = section[..nextHeading];
        }

        string? found = null;
        int foundAt = int.MaxValue;
        foreach (string name in VerdictNames)
        {
            int at = section.IndexOf(name, StringComparison.Ordinal);
            if (at >= 0 && at < foundAt)
            {
                found = name;
                foundAt = at;
            }
        }

        return found;
    }
}
