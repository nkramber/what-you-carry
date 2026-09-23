using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WhatYouCarry.Tools.CodexReview;

/// <summary>
/// One finding of a review record: the id, the severity, the text of its status line, and each effective head at
/// which a review round found it open (D-514). The status line starts with <c>open</c>, <c>fixed</c>,
/// <c>accepted risk</c>, or <c>withdrawn</c> (the <c>findings.md</c> reference of <c>pr-review</c>).
/// </summary>
public sealed record ReviewFinding(string Id, int Severity, string Status, IReadOnlyList<string> OpenAt)
{
    public bool IsOpen => Status.StartsWith("open", StringComparison.Ordinal);
}

/// <summary>Reads the findings from the <c>## Findings</c> section of a review record.</summary>
public static partial class ReviewFindings
{
    public const string SectionHeading = "## Findings";
    public const string StatusPrefix = "Status:";
    public const string OpenAtPrefix = "Open at:";

    /// <summary>
    /// Every finding under the section heading, in the order of the file. A finding heading is
    /// <c>### P&lt;severity&gt;-&lt;index&gt;: &lt;title&gt;</c>. A finding with no status line is an error that names it (T-2).
    /// A finding with no <c>Open at:</c> line has an empty list, and the outcome rule judges that.
    /// </summary>
    public static IReadOnlyList<ReviewFinding> Parse(string recordText)
    {
        string[] lines = recordText.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        int start = Array.FindIndex(lines, static line => line.TrimEnd() == SectionHeading);
        if (start < 0)
        {
            throw new FormatException($"The review record has no '{SectionHeading}' section.");
        }

        var findings = new List<ReviewFinding>();
        string? id = null;
        int severity = 0;
        string? status = null;
        List<string> openAt = [];
        for (int index = start + 1; index < lines.Length && !lines[index].StartsWith("## ", StringComparison.Ordinal); index++)
        {
            string line = lines[index].Trim();
            Match heading = FindingHeading().Match(line);
            if (heading.Success)
            {
                AddFinding(findings, id, severity, status, openAt);
                id = heading.Groups["id"].Value;
                severity = int.Parse(heading.Groups["severity"].Value, CultureInfo.InvariantCulture);
                status = null;
                openAt = [];
                continue;
            }

            if (id is null)
            {
                continue;
            }

            if (status is null && line.StartsWith(StatusPrefix, StringComparison.Ordinal))
            {
                status = line[StatusPrefix.Length..].Trim();
            }
            else if (openAt.Count == 0 && line.StartsWith(OpenAtPrefix, StringComparison.Ordinal))
            {
                openAt = ReadHashes(line[OpenAtPrefix.Length..]);
            }
        }

        AddFinding(findings, id, severity, status, openAt);
        return findings;
    }

    private static void AddFinding(List<ReviewFinding> findings, string? id, int severity, string? status, List<string> openAt)
    {
        if (id is null)
        {
            return;
        }

        if (status is null)
        {
            throw new FormatException($"The finding {id} in the '{SectionHeading}' section has no '{StatusPrefix}' line.");
        }

        findings.Add(new ReviewFinding(id, severity, status, openAt));
    }

    /// <summary>Each text in backticks on the line, in order.</summary>
    private static List<string> ReadHashes(string text)
    {
        var hashes = new List<string>();
        foreach (Match match in BacktickText().Matches(text))
        {
            hashes.Add(match.Groups["text"].Value.Trim());
        }

        return hashes;
    }

    [GeneratedRegex(@"^### (?<id>P(?<severity>[0-9])-[0-9]+):")]
    private static partial Regex FindingHeading();

    [GeneratedRegex("`(?<text>[^`]+)`")]
    private static partial Regex BacktickText();
}
