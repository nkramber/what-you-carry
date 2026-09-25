using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WhatYouCarry.Tools.SteCheck;

/// <summary>
/// The reference check (D-178, D-186). It reads the Effect column of the decision register for every
/// <c>Superseded by D-N</c> marker. A line in another file that cites a superseded decision is a finding, unless the
/// sentence of the citation holds a revision word or the line names the superseding decision. A decision marked
/// <c>Revised in part by</c> stays citable.
/// </summary>
public static class ReferenceCheck
{
    public const string Rule = "D-178";

    private static readonly Regex SupersededMarker = new(@"Superseded by (D-\d+)", RegexOptions.Compiled);
    private static readonly Regex DecisionId = new(@"\bD-\d+\b", RegexOptions.Compiled);
    private static readonly Regex RevisionWord = new(@"\b(supersed\w*|revis\w*)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>Maps each superseded decision id to the id that supersedes it.</summary>
    public static Dictionary<string, string> SupersededDecisions(string decisionsText)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string rawLine in decisionsText.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            if (!line.StartsWith("| D-", StringComparison.Ordinal))
            {
                continue;
            }

            string[] cells = line.Split('|');
            if (cells.Length != 7)
            {
                throw new InvalidOperationException($"The decision row '{line}' has {cells.Length - 2} cells, and a row has 5.");
            }

            string id = cells[1].Trim();
            Match marker = SupersededMarker.Match(cells[5]);
            if (marker.Success)
            {
                result[id] = marker.Groups[1].Value;
            }
        }

        if (result.Count == 0)
        {
            throw new InvalidOperationException("The decision register has no 'Superseded by' marker. The register holds superseded decisions, so the parse failed.");
        }

        return result;
    }

    public static List<Finding> Check(string relativePath, string text, IReadOnlyDictionary<string, string> superseded)
    {
        var findings = new List<Finding>();
        string[] lines = text.Split('\n');
        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index].TrimEnd('\r');
            foreach (string sentence in SentencesOf(line))
            {
                if (RevisionWord.IsMatch(sentence))
                {
                    continue;
                }

                foreach (Match match in DecisionId.Matches(sentence))
                {
                    if (!superseded.TryGetValue(match.Value, out string? superseder) || NamesDecision(line, superseder))
                    {
                        continue;
                    }

                    findings.Add(new Finding(relativePath, index + 1, Rule, $"cites {match.Value}, which {superseder} supersedes", line.Trim()));
                }
            }
        }

        return findings;
    }

    /// <summary>
    /// The sentences of one line, as <see cref="SentenceText"/> splits them for the STE rules. A paragraph is one line,
    /// so a revision word exempts only the citations of its own sentence. A parenthesis stays whole inside its sentence.
    /// </summary>
    private static List<string> SentencesOf(string line)
    {
        var result = new List<string>();
        var textLine = new TextLine(0, line, IsHeading: false, IsNumberedItem: false, InProceduralSection: false);
        foreach (Sentence sentence in SentenceText.Split(textLine))
        {
            result.Add(sentence.Text);
        }

        return result;
    }

    private static bool NamesDecision(string line, string id)
    {
        foreach (Match match in DecisionId.Matches(line))
        {
            if (match.Value == id)
            {
                return true;
            }
        }

        return false;
    }
}
