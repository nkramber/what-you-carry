using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WhatYouCarry.Tools.SteCheck;

/// <summary>One line of prose from a Markdown file, with the list marker and the block-quote marker removed.</summary>
public sealed record TextLine(int Line, string Text, bool IsHeading, bool IsNumberedItem, bool InProceduralSection);

/// <summary>
/// Reads the prose lines out of a Markdown file. Fenced code, tables, blank lines, and thematic breaks are not prose.
/// A numbered item under a heading that holds "Sequence" or "Procedure" is a procedural step, with the 20-word limit.
/// The nearest heading above a line, at any level, is its section heading.
/// </summary>
public static class MarkdownText
{
    private static readonly Regex NumberedMarker = new(@"^\d+[.)]\s+", RegexOptions.Compiled);
    private static readonly Regex BulletMarker = new(@"^[-*+]\s+", RegexOptions.Compiled);
    private static readonly Regex CheckboxMarker = new(@"^\[[ xX]\]\s+", RegexOptions.Compiled);
    private static readonly Regex ThematicBreak = new(@"^\s*([-*_])(\s*\1){2,}\s*$", RegexOptions.Compiled);
    private static readonly Regex Link = new(@"!?\[([^\]]*)\]\([^)]*\)", RegexOptions.Compiled);
    private static readonly Regex Italic = new(@"(?<!\w)\*(?=\S)([^*]+?)(?<=\S)\*(?!\w)", RegexOptions.Compiled);

    public static List<TextLine> Read(string text)
    {
        var lines = new List<TextLine>();
        bool inFence = false;
        bool inProceduralSection = false;
        string[] rawLines = text.Split('\n');
        for (int index = 0; index < rawLines.Length; index++)
        {
            string raw = rawLines[index].TrimEnd('\r');
            string trimmed = raw.Trim();
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
