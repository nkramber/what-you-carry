using System;
using System.Collections.Generic;

namespace WhatYouCarry.Tools.CiSkip;

/// <summary>The answer of the rules: skip the heavy jobs of one workflow or run them, and the reason.</summary>
public sealed record CiSkipDecision(bool Skip, string Reason);

/// <summary>
/// Decides whether the heavy jobs of one workflow skip a PR head that changes documents alone (D-474, D-475). The
/// rules do no I/O. A fact that the rules cannot read runs every job, and never skips one (T-2).
/// </summary>
/// <remarks>
/// Rule 1: a PR whose every path from the merge base is a document skips. Rule 2: a push to a PR skips when every
/// path from the previous head is a document, and the newest run of the same workflow on the previous head completed
/// with success. A run whose heavy jobs skipped completes with success, so a second documents push skips too. A push
/// to <c>main</c> never skips (D-473).
/// </remarks>
public static class CiSkipRules
{
    public const string PullRequestEvent = "pull_request";

    public const string CompletedStatus = "completed";

    public const string SuccessConclusion = "success";

    /// <summary>A path is a document when it starts with an entry that ends in a slash, or equals any other entry (D-475).</summary>
    public static readonly string[] DocumentPaths = ["docs/", ".claude/skills/", "CLAUDE.md", "AGENTS.md", "README.md", "LICENSE"];

    public static bool IsDocument(string path)
    {
        foreach (string document in DocumentPaths)
        {
            if (document.EndsWith('/') && path.StartsWith(document, StringComparison.Ordinal))
            {
                return true;
            }

            if (path == document)
            {
                return true;
            }
        }

        return false;
    }

    public static CiSkipDecision Decide(CiSkipFacts facts)
    {
        if (facts.EventName != PullRequestEvent)
        {
            return new CiSkipDecision(false, $"The event '{facts.EventName}' runs every job. A push to main never skips (D-473).");
        }

        if (facts.PullRequestPaths is null)
        {
            throw new InvalidOperationException($"The event '{PullRequestEvent}' needs the paths of the PR, and the facts hold none.");
        }

        string? pullRequestCode = FirstCodePath(facts.PullRequestPaths);
        if (pullRequestCode is null)
        {
            return new CiSkipDecision(true, $"Rule 1: each of the {facts.PullRequestPaths.Count} path(s) of the PR is a document (D-474).");
        }

        if (facts.PushPaths is null)
        {
            return new CiSkipDecision(false, $"The PR changes '{pullRequestCode}', and the event gives no previous head that is an ancestor of the head.");
        }

        string? pushCode = FirstCodePath(facts.PushPaths);
        if (pushCode is not null)
        {
            return new CiSkipDecision(false, $"The push changes '{pushCode}' after the previous head.");
        }

        if (facts.PreviousRuns is null)
        {
            throw new InvalidOperationException("A push with a previous head needs the runs of the workflow on that head, and the facts hold none.");
        }

        WorkflowRun? newest = Newest(facts.PreviousRuns);
        if (newest is null)
        {
            return new CiSkipDecision(false, "The previous head has no run of this workflow.");
        }

        if (newest.Status != CompletedStatus || newest.Conclusion != SuccessConclusion)
        {
            return new CiSkipDecision(false, $"Run {newest.Id} of the previous head has the status '{newest.Status}' and the conclusion '{newest.Conclusion ?? "none"}'.");
        }

        return new CiSkipDecision(true, $"Rule 2: each of the {facts.PushPaths.Count} path(s) of the push is a document, and run {newest.Id} of the previous head passed (D-474).");
    }

    private static string? FirstCodePath(IReadOnlyList<string> paths)
    {
        foreach (string path in paths)
        {
            if (!IsDocument(path))
            {
                return path;
            }
        }

        return null;
    }

    /// <summary>The run with the latest creation time. The higher id wins a tie, because GitHub gives ids in order.</summary>
    private static WorkflowRun? Newest(IReadOnlyList<WorkflowRun> runs)
    {
        WorkflowRun? newest = null;
        foreach (WorkflowRun run in runs)
        {
            if (newest is null || run.CreatedAt > newest.CreatedAt || (run.CreatedAt == newest.CreatedAt && run.Id > newest.Id))
            {
                newest = run;
            }
        }

        return newest;
    }
}
