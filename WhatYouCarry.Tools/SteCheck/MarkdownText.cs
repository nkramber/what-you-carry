using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WhatYouCarry.Tools.SteCheck;

/// <summary>One line of prose from a Markdown file, with the list marker and the block-quote marker removed.</summary>
public sealed record TextLine(int Line, string Text, bool IsHeading, bool IsNumberedItem, bool InProceduralSection, bool InFrontMatter = false);

/// <summary>
/// Reads the prose lines out of a Markdown file. Fenced code, tables, blank lines, and thematic breaks are not prose.
/// A numbered item under a heading that holds "Sequence" or "Procedure" is a procedural step, with the 20-word limit.
/// The nearest heading above a line, at any level, is its section heading.
/// A file that opens with a "---" line has front matter, which ends at the next "---" line. The front matter is prose
/// that a model reads, so rule 6.3 holds there. No grammar rule reads it, because a description names triggers and not
/// sentences (D-386).
/// </summary>
public static class MarkdownText
{
    private static readonly Regex NumberedMarker = new(@"^\d+[.)]\s+", RegexOptions.Compiled);
    private static readonly Regex BulletMarker = new(@"^[-*+]\s+", RegexOptions.Compiled);
    private static readonly Regex CheckboxMarker = new(@"^\[[ xX]\]\s+", RegexOptions.Compiled);
    private static readonly Regex ThematicBreak = new(@"^\s*([-*_])(\s*\1){2,}\s*$", RegexOptions.Compiled);
    private static readonly Regex Link = new(@"!?\[([^\]]*)\]\([^)]*\)", RegexOptions.Compiled);
    private static readonly Regex Italic = new(@"(?<!\w)\*(?=\S)([^*]+?)(?<=\S)\*(?!\w)", RegexOptions.Compiled);

    /// <summary>The line that opens and closes the front matter of a skill or an agent file.</summary>
    private const string FrontMatterFence = "---";

    public static List<TextLine> Read(string text)
    {
        var lines = new List<TextLine>();
        bool inFence = false;
        bool inProceduralSection = false;
        string[] rawLines = text.Split('\n');
        bool inFrontMatter = HasFrontMatter(rawLines);
        for (int index = 0; index < rawLines.Length; index++)
        {
            string raw = rawLines[index].TrimEnd('\r');
            string trimmed = raw.Trim();
            if (inFrontMatter)
            {
                if (index > 0 && raw == FrontMatterFence)
                {
                    inFrontMatter = false;
                    continue;
                }

                if (index > 0 && trimmed.Length > 0)
                {
                    lines.Add(new TextLine(index + 1, StripMarkup(trimmed), IsHeading: false, IsNumberedItem: false, InProceduralSection: false, InFrontMatter: true));
                }

                continue;
            }

            if (trimmed.StartsWith("```", StringComparison.Ordinal) || trimmed.StartsWith("~~~", StringComparison.Ordinal))
            {
                inFence = !inFence;
                continue;
            }

            if (inFence || trimmed.Length == 0 || trimmed.StartsWith('|') || ThematicBreak.IsMatch(raw))
            {
                continue;
            }

            int lineNumber = index + 1;
            if (trimmed.StartsWith('#'))
            {
                string heading = StripMarkup(trimmed.TrimStart('#').Trim());
                inProceduralSection = IsProceduralHeading(heading);
                lines.Add(new TextLine(lineNumber, heading, IsHeading: true, IsNumberedItem: false, inProceduralSection));
                continue;
            }

            string body = StripBlockQuote(trimmed);
            bool isNumbered = NumberedMarker.IsMatch(body);
            body = StripListMarkers(body);
            lines.Add(new TextLine(lineNumber, StripMarkup(body), IsHeading: false, isNumbered, inProceduralSection));
        }

        return lines;
    }

    /// <summary>
    /// True when the file opens with a front matter block that closes. An open "---" with no close is a thematic
    /// break, not front matter. Without this check the rest of that file reads as front matter, and no grammar rule
    /// sees it.
    /// </summary>
    private static bool HasFrontMatter(string[] rawLines)
    {
        if (rawLines.Length == 0 || rawLines[0].TrimEnd('\r') != FrontMatterFence)
        {
            return false;
        }

        for (int index = 1; index < rawLines.Length; index++)
        {
            if (rawLines[index].TrimEnd('\r') == FrontMatterFence)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsProceduralHeading(string heading)
    {
        return heading.Contains("sequence", StringComparison.OrdinalIgnoreCase)
            || heading.Contains("procedure", StringComparison.OrdinalIgnoreCase);
    }

    private static string StripBlockQuote(string line)
    {
        string result = line;
        while (result.StartsWith('>'))
        {
            result = result[1..].TrimStart();
        }

        return result;
    }

    private static string StripListMarkers(string line)
    {
        string result = NumberedMarker.Replace(line, string.Empty, 1);
        result = BulletMarker.Replace(result, string.Empty, 1);
        return CheckboxMarker.Replace(result, string.Empty, 1);
    }

    /// <summary>Removes the markup that is not text: links keep their text, and bold markers, comment markers, and emphasis go.</summary>
    private static string StripMarkup(string line)
    {
        string result = Link.Replace(line, "$1");
        result = result.Replace("<!--", string.Empty, StringComparison.Ordinal).Replace("-->", string.Empty, StringComparison.Ordinal);
        result = result.Replace("**", string.Empty, StringComparison.Ordinal).Replace("__", string.Empty, StringComparison.Ordinal);
        result = Italic.Replace(result, "$1");
        return result.Trim();
    }
}
