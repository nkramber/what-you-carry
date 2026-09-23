using System;
using System.Collections.Generic;
using WhatYouCarry.Tools.ReviewGate;

namespace WhatYouCarry.Tools.CodexReview;

/// <summary>One check run of the Gitar app, and the commit that it reads.</summary>
public sealed record GitarCheck(string Sha, string Status, DateTimeOffset StartedAt);

/// <summary>Everything the start checks read, gathered once from the CLI, git, and GitHub. The rules do no I/O.</summary>
public sealed class StartFacts
{
    public required int PullRequestNumber { get; init; }

    public required CodexVersion Version { get; init; }

    /// <summary>The output of <c>codex login status</c>, with every API credential variable removed (D-523).</summary>
    public required string LoginStatus { get; init; }

    /// <summary>The state of the PR on GitHub: <c>OPEN</c>, <c>CLOSED</c>, or <c>MERGED</c>.</summary>
    public required string PullRequestState { get; init; }

    public required string PullRequestBranch { get; init; }

    /// <summary>The head of the PR on GitHub.</summary>
    public required string PullRequestHead { get; init; }

    /// <summary>The branch of the local checkout, or <c>HEAD</c> when the checkout is detached.</summary>
    public required string LocalBranch { get; init; }

    public required string LocalHead { get; init; }

    /// <summary>The branch head on origin after the fetch.</summary>
    public required string OriginHead { get; init; }

    /// <summary>The output of <c>git status --porcelain</c>. Empty text means a clean tree.</summary>
    public required string WorkingTreeStatus { get; init; }

    /// <summary>The newest commit outside the skip set of D-475, or null when the PR changes documents alone (D-534).</summary>
    public required string? EffectiveHead { get; init; }

    /// <summary>The newest commit outside the metadata set, or null when the PR changes metadata alone (D-184).</summary>
    public required string? WorkHead { get; init; }

    /// <summary>
    /// Every Gitar check run on the commits from the work head to the PR head. A metadata commit keeps the pass of
    /// the work head current (D-184, D-534), so a check on a later commit is not required.
    /// </summary>
    public required IReadOnlyList<GitarCheck> GitarChecks { get; init; }

    /// <summary>The last edit time of the newest Gitar dashboard comment, or null when the PR has none.</summary>
    public required DateTimeOffset? DashboardEditedAt { get; init; }

    public required int UnresolvedThreadCount { get; init; }
}

/// <summary>
/// The conditions that must hold before a review round starts (D-511). Each problem names the fact that failed
/// and the value found (T-2). The command refuses the round when the list is not empty.
/// </summary>
public static class StartChecks
{
    public const string GitarCompleted = "completed";
    public const string OpenState = "OPEN";

    public static string NotOpenProblem(int pullRequestNumber, string state)
    {
        return $"PR #{pullRequestNumber} is {state}, and a review needs an open PR.";
    }

    public static IReadOnlyList<string> Problems(StartFacts facts)
    {
        var problems = new List<string>();
        if (!facts.Version.IsAtLeast(CodexReviewSettings.MinimumVersion))
        {
            problems.Add($"The Codex CLI is {facts.Version}, and the minimum is {CodexReviewSettings.MinimumVersion} (D-512). Run `npm install -g @openai/codex@latest`.");
        }

        if (!facts.LoginStatus.StartsWith(CodexReviewSettings.ChatGptLoginStatus, StringComparison.Ordinal))
        {
            problems.Add($"`codex login status` gives '{facts.LoginStatus.Trim()}', and a review needs '{CodexReviewSettings.ChatGptLoginStatus}', so it never uses API pricing (D-523). Run `codex login` and choose ChatGPT.");
        }

        if (facts.PullRequestState != OpenState)
        {
            problems.Add(NotOpenProblem(facts.PullRequestNumber, facts.PullRequestState));
        }

        if (facts.EffectiveHead is null)
        {
            problems.Add($"No commit of PR #{facts.PullRequestNumber} changes a path outside the skip set of D-475, so the review has nothing to approve. The '{ReviewGateRules.OverrideLabel}' label covers such a PR (D-540).");
        }

        AddCheckoutProblems(facts, problems);
        AddGitarProblems(facts, problems);
        return problems;
    }

    /// <summary>The local checkout is the PR branch, clean, and at the head that origin and GitHub hold.</summary>
    private static void AddCheckoutProblems(StartFacts facts, List<string> problems)
    {
        if (facts.LocalBranch != facts.PullRequestBranch)
        {
            problems.Add($"The checkout is on '{facts.LocalBranch}', and PR #{facts.PullRequestNumber} has the branch '{facts.PullRequestBranch}'.");
        }

        if (facts.LocalHead != facts.OriginHead)
        {
            problems.Add($"The local head {facts.LocalHead} differs from origin/{facts.PullRequestBranch} at {facts.OriginHead}. Push or pull first.");
        }

        if (facts.OriginHead != facts.PullRequestHead)
        {
            problems.Add($"origin/{facts.PullRequestBranch} is at {facts.OriginHead}, and GitHub gives the PR head {facts.PullRequestHead}.");
        }

        if (facts.WorkingTreeStatus.Trim().Length > 0)
        {
            problems.Add($"The working tree is dirty. git status --porcelain:\n{facts.WorkingTreeStatus.TrimEnd()}");
        }
    }

    /// <summary>
    /// The facts of a current and answered Gitar pass that a machine can read (D-250, D-374). The
    /// <c>gitar-review</c> skill holds the full proof, and the author runs it before this command.
    /// </summary>
    private static void AddGitarProblems(StartFacts facts, List<string> problems)
    {
        if (facts.WorkHead is null)
        {
            // The effective head problem already names the label, and a metadata commit gives Gitar no work.
            return;
        }

        GitarCheck? earliest = null;
        foreach (GitarCheck check in facts.GitarChecks)
        {
            if (check.Status != GitarCompleted)
            {
                problems.Add($"The Gitar check run on {check.Sha} is '{check.Status}'. Wait until it completes.");
            }

            if (earliest is null || check.StartedAt < earliest.StartedAt)
            {
                earliest = check;
            }
        }

        if (earliest is null)
        {
            problems.Add($"No commit from the work head {facts.WorkHead} to the PR head {facts.PullRequestHead} has a Gitar check run. Follow the gitar-review skill.");
        }

        if (facts.DashboardEditedAt is null)
        {
            problems.Add($"PR #{facts.PullRequestNumber} has no Gitar dashboard comment.");
        }
        else if (earliest is not null && facts.DashboardEditedAt <= earliest.StartedAt)
        {
            problems.Add($"The Gitar dashboard comment has its last edit at {facts.DashboardEditedAt:O}, not after the Gitar check run on {earliest.Sha} started at {earliest.StartedAt:O}. The review of the work head is not current. Follow the gitar-review skill.");
        }

        if (facts.UnresolvedThreadCount > 0)
        {
            problems.Add($"PR #{facts.PullRequestNumber} has {facts.UnresolvedThreadCount} unresolved review thread(s). Answer and resolve each one (D-250, D-522).");
        }
    }
}
