using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WhatYouCarry.Tools.CodexReview;

/// <summary>
/// One finding of a review record: the id, the severity, the text of its status line, and each effective head at
/// which a review round found it open (D-514). The status line starts with <c>open</c>, <c>fixed</c>,
/// <c>accepted risk</c>, or <c>withdrawn</c> (the <c>findings.md</c> reference of <c>pr-review</c>), and the parser
/// refuses any other status.
/// </summary>
public sealed record ReviewFinding(string Id, int Severity, string Status, IReadOnlyList<string> OpenAt)
{
    public bool IsOpen => Status.StartsWith(ReviewFindings.OpenStatus, StringComparison.Ordinal);
}

/// <summary>Reads the findings from the <c>## Findings</c> section of a review record.</summary>
public static partial class ReviewFindings
{
    public const string SectionHeading = "## Findings";
    public const string StatusPrefix = "Status:";
    public const string OpenAtPrefix = "Open at:";
    public const string OpenStatus = "open";

    /// <summary>The statuses of <c>findings.md</c>, in the words of each form (F-125).</summary>
    public static readonly string[] KnownStatuses = [OpenStatus, "fixed", "accepted risk", "withdrawn"];

    /// <summary>
    /// The complete forms of a status in <c>findings.md</c>: <c>open.</c>, <c>fixed in `&lt;sha&gt;`.</c>,
    /// <c>accepted risk, D-&lt;n&gt;.</c>, and <c>withdrawn.</c>. A closed status names its revision or its decision, so
    /// a status such as <c>fixed.</c> never closes a finding with no evidence (F-125, D-514).
    /// </summary>
    public const string StatusForms = "open. | fixed in `<sha>`. | accepted risk, D-<n>. | withdrawn.";

    /// <summary>The severities of <c>findings.md</c>: P0 to P3.</summary>
    public const int HighestSeverity = 3;

    /// <summary>
    /// Every finding under the section heading, in the order of the file. A finding heading is
    /// <c>### P&lt;severity&gt;-&lt;index&gt;: &lt;title&gt;</c>, with a severity from P0 to P3. Any other line that starts with three
    /// or more hashes in the section is an error that names it, because a skipped finding blocks nothing in silence (T-2, PR #93 P2-1).
    /// A finding with no status line, or with a status outside <see cref="KnownStatuses"/>, is an error that names it too
    /// (F-125). A finding with no <c>Open at:</c> line has an
    /// empty list, and the outcome rule judges that.
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
            // A line of three or more hashes is a heading of level 3 or deeper, with a space or without one.
            if (!heading.Success && line.StartsWith("###", StringComparison.Ordinal))
            {
                throw new FormatException($"The heading '{line}' in the '{SectionHeading}' section is not a finding heading of the form '### P<0 to {HighestSeverity}>-<index>: <title>'.");
            }

            if (heading.Success)
            {
                AddFinding(findings, id, severity, status, openAt);
                id = heading.Groups["id"].Value;
                severity = int.Parse(heading.Groups["severity"].Value, CultureInfo.InvariantCulture);
                if (severity > HighestSeverity)
                {
                    throw new FormatException($"The finding {id} has the severity P{severity}, and a severity is P0 to P{HighestSeverity}.");
                }

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

        // An unknown status is an error, because a closed default lets a finding such as "Open" block nothing (F-125).
        if (!IsKnownStatus(status))
        {
            throw new FormatException($"The finding {id} has the status '{status}', and a status is one of the forms: {StatusForms}");
        }

        findings.Add(new ReviewFinding(id, severity, status, openAt));
    }

    private static bool IsKnownStatus(string status)
    {
        return StatusForm().IsMatch(status);
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

    [GeneratedRegex(@"^### (?<id>P(?<severity>[0-9]{1,3})-[0-9]+):")]
    private static partial Regex FindingHeading();

    [GeneratedRegex("`(?<text>[^`]+)`")]
    private static partial Regex BacktickText();

    [GeneratedRegex(@"^(open|fixed in `[0-9a-f]{7,40}`|accepted risk, D-[0-9]+|withdrawn)\.$")]
    private static partial Regex StatusForm();
}
