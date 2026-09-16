using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WhatYouCarry.Tools.DocGate;

/// <summary>The outcome of the documentation gate: whether the PR passes, and one line for each problem (T-2).</summary>
public sealed record DocGateResult(bool Passes, IReadOnlyList<string> Problems);

/// <summary>One required line of the documents matrix: the label that the PR description writes, and the paths it covers.</summary>
public sealed record DocumentCategory(string Label, IReadOnlyList<string> Paths);

/// <summary>
/// The rules of the documentation gate (D-375, D-376). A PR carries its own handoff entry, a documents matrix that
/// accounts for every required category and agrees with the diff, and no promise of documents in later work. A PR
/// that only records the merge of an earlier PR fails. The rules read text and paths alone, and they do no I/O.
/// </summary>
public static class DocGateRules
{
    /// <summary>The name of the job and the command.</summary>
    public const string JobName = "doc-gate";

    /// <summary>The handoff file. It is in the metadata set of D-184, so its commit never moves the effective head.</summary>
    public const string HandoffPath = "docs/session-handoff.md";

    /// <summary>The heading of the documents matrix in the PR description.</summary>
    public const string MatrixHeading = "## Documents";

    /// <summary>The three dispositions of a matrix line. Each one starts the text after the label.</summary>
    public const string Changed = "Changed:";
    public const string NoChangeNeeded = "Reviewed; no change needed:";
    public const string NotApplicable = "Not applicable:";

    /// <summary>The fewest words that a reason can have. A shorter reason cannot name a document and a cause.</summary>
    public const int MinimumReasonWords = 5;

    /// <summary>The required categories, in the order of the PR template. A path prefix ends with a slash.</summary>
    public static readonly DocumentCategory[] Categories =
    [
        new("`docs/design.md`", ["docs/design.md"]),
        new("`docs/decisions.md`", ["docs/decisions.md"]),
        new("`docs/questions.md`", ["docs/questions.md"]),
        new("`docs/roadmaps/`", ["docs/roadmaps/"]),
        new("`docs/runbooks/`", ["docs/runbooks/"]),
        new("`docs/session-handoff.md`", [HandoffPath, "docs/session-handoff-archive.md"]),
        new("`CLAUDE.md` and `AGENTS.md`", ["CLAUDE.md", "AGENTS.md"]),
        new("`.claude/skills/`", [".claude/skills/"]),
    ];

    /// <summary>Text that puts documents off to later work, or names a PR that only records an earlier one.</summary>
    private static readonly Regex[] DeferralPatterns =
    [
        new(@"\b(follow-?up|later|next|second|separate|another) (docs|documentation) PR\b", RegexOptions.IgnoreCase),
        new(@"\b(docs|documentation) PR (records|will record|to record|that records|after)\b", RegexOptions.IgnoreCase),
        new(@"\b(follows|comes|lands) in a (docs|documentation) PR\b", RegexOptions.IgnoreCase),
        new(@"\bwill update\b[^.]{0,40}\b(docs?|documents?|documentation|design|roadmaps?|handoff|decisions|questions|register|skills?)\b", RegexOptions.IgnoreCase),
        new(@"\b(update|record|write)\b[^.]{0,40}\b(after the merge|after merge|in a later PR)\b", RegexOptions.IgnoreCase),
        new(@"\b(TBD|TODO)\b"),
    ];

    /// <summary>A title or a branch of a PR that records the merge of an earlier PR.</summary>
    private static readonly Regex MergeRecordPattern = new(@"merge[- ]record|\brecord the (PR-\d+ )?merge\b", RegexOptions.IgnoreCase);

    private static readonly Regex HtmlComment = new(@"<!--.*?-->", RegexOptions.Singleline);

    private static readonly string[] GenericReasons = ["no documentation impact", "no docs impact"];

    public static DocGateResult Evaluate(DocGateFacts facts)
    {
        var problems = new List<string>();
        if (MergeRecordPattern.IsMatch(facts.Title) || MergeRecordPattern.IsMatch(facts.Branch))
        {
            problems.Add($"The title '{facts.Title}' or the branch '{facts.Branch}' names a merge record. A PR carries its own documents, and no PR records an earlier merge (D-375).");
        }

        CheckHandoff(facts, problems);
        string body = HtmlComment.Replace(facts.Body, string.Empty);
        CheckMatrix(body, facts.ChangedPaths, problems);
        CheckDeferral("the PR description", body, problems);
        if (facts.NewestHandoffEntry is not null)
        {
            CheckDeferral("the newest handoff entry", facts.NewestHandoffEntry, problems);
        }

        return new DocGateResult(problems.Count == 0, problems);
    }

    /// <summary>The first <c>## Session</c> entry of the handoff text, up to the next one, or null when the text holds none.</summary>
    public static string? NewestHandoffEntry(string handoffText)
    {
        int start = handoffText.IndexOf("\n## Session ", StringComparison.Ordinal);
        if (start < 0)
        {
            return null;
        }

        int end = handoffText.IndexOf("\n## Session ", start + 1, StringComparison.Ordinal);
        return end < 0 ? handoffText[(start + 1)..] : handoffText[(start + 1)..end];
    }

    private static void CheckHandoff(DocGateFacts facts, List<string> problems)
    {
        if (!facts.ChangedPaths.Contains(HandoffPath))
        {
            problems.Add($"The PR does not change {HandoffPath}. Each PR carries its own handoff entry (D-146, D-375).");
            return;
        }

        if (facts.NewestHandoffEntry is null)
        {
            problems.Add($"{HandoffPath} at the head holds no '## Session' entry.");
            return;
        }

        string branchMark = $"Branch `{facts.Branch}`";
        if (!facts.NewestHandoffEntry.Contains(branchMark, StringComparison.Ordinal))
        {
            problems.Add($"The newest handoff entry does not name {branchMark}. The entry must describe the work of this PR (D-375).");
        }
    }

    private static void CheckMatrix(string body, IReadOnlyList<string> changedPaths, List<string> problems)
    {
        List<string> lines = MatrixLines(body);
        if (lines.Count == 0)
        {
            problems.Add($"The PR description has no '{MatrixHeading}' section with lines. The section gives one line for each category (D-376).");
            return;
        }

        foreach (DocumentCategory category in Categories)
        {
            string prefix = $"- {category.Label}:";
            List<string> matches = lines.FindAll(candidate => candidate.StartsWith(prefix, StringComparison.Ordinal));
            if (matches.Count == 0)
            {
                problems.Add($"The documents matrix has no line for {category.Label}.");
                continue;
            }

            if (matches.Count > 1)
            {
                problems.Add($"The documents matrix has {matches.Count} lines for {category.Label}, and a category has one line. Two lines make its disposition ambiguous.");
                continue;
            }

            string text = matches[0][prefix.Length..].Trim();
            string? disposition = Array.Find([Changed, NoChangeNeeded, NotApplicable], value => text.StartsWith(value, StringComparison.Ordinal));
            if (disposition is null)
            {
                problems.Add($"The line for {category.Label} does not start with '{Changed}', '{NoChangeNeeded}', or '{NotApplicable}': '{text}'.");
                continue;
            }

            string reason = text[disposition.Length..].Trim();
            int words = reason.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            if (words < MinimumReasonWords || Array.Exists(GenericReasons, generic => reason.Contains(generic, StringComparison.OrdinalIgnoreCase)))
            {
                problems.Add($"The line for {category.Label} gives a generic reason, or fewer than {MinimumReasonWords} words: '{reason}'. Name the document and the cause.");
            }

            bool changed = HasChangedPath(category, changedPaths);
            if (disposition == Changed && !changed)
            {
                problems.Add($"The line for {category.Label} says '{Changed}', and the diff changes no path of that category.");
            }

            if (disposition != Changed && changed)
            {
                problems.Add($"The line for {category.Label} says '{disposition}', and the diff changes a path of that category.");
            }
        }
    }

    /// <summary>The list lines of the documents matrix: each line that starts with "- " between its heading and the next "## " heading.</summary>
    private static List<string> MatrixLines(string body)
    {
        var result = new List<string>();
        bool inMatrix = false;
        foreach (string rawLine in body.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                inMatrix = line.Trim() == MatrixHeading;
                continue;
            }

            if (inMatrix && line.StartsWith("- ", StringComparison.Ordinal))
            {
                result.Add(line);
            }
        }

        return result;
    }

    private static bool HasChangedPath(DocumentCategory category, IReadOnlyList<string> changedPaths)
    {
        foreach (string changedPath in changedPaths)
        {
            foreach (string path in category.Paths)
            {
                bool matches = path.EndsWith('/') ? changedPath.StartsWith(path, StringComparison.Ordinal) : changedPath == path;
                if (matches)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void CheckDeferral(string source, string text, List<string> problems)
    {
        foreach (Regex pattern in DeferralPatterns)
        {
            Match match = pattern.Match(text);
            if (match.Success)
            {
                problems.Add($"{char.ToUpperInvariant(source[0])}{source[1..]} defers documents to later work: '{match.Value}'. The PR carries all of its documents (D-375).");
            }
        }
    }
}
