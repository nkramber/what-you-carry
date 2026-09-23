using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using WhatYouCarry.Tools.ReviewGate;

namespace WhatYouCarry.Tools.CodexReview;

/// <summary>The facts of a PR that <c>gh pr view</c> gives.</summary>
public sealed record PullRequestView(string State, string Branch, string Head, string BaseBranch);

/// <summary>
/// <c>codex-review --root . --pr &lt;n&gt; --codex &lt;path&gt;</c> (D-511). The command checks the start conditions,
/// probes the model, runs one Codex review round in a detached worktree at the PR head, and judges the review
/// record that the round pushed. The exit code names the outcome (<see cref="CodexReviewExit"/>).
/// </summary>
public static class CodexReviewCommand
{
    public const string Usage = "Options: --root <path> --pr <number> --codex <path>.";
    public const string GitarApp = "gitar-bot";
    public const string GitarLogin = "gitar-bot[bot]";
    public const string DashboardMarker = "<b>Code Review</b>";

    public static int Run(string[] args)
    {
        string? root = null;
        string? pullRequestText = null;
        string? codex = null;
        for (int i = 0; i + 1 < args.Length; i += 2)
        {
            switch (args[i])
            {
                case "--root":
                    root = args[i + 1];
                    break;
                case "--pr":
                    pullRequestText = args[i + 1];
                    break;
                case "--codex":
                    codex = args[i + 1];
                    break;
                default:
                    Console.Error.WriteLine($"Unknown option '{args[i]}'. {Usage}");
                    return 2;
            }
        }

        if (root is null || codex is null || !int.TryParse(pullRequestText, NumberStyles.None, CultureInfo.InvariantCulture, out int pullRequest))
        {
            Console.Error.WriteLine($"Each option needs a value, and --pr needs a whole number. Found --pr '{pullRequestText}'. {Usage}");
            return 2;
        }

        try
        {
            return (int)Review(Path.GetFullPath(root), pullRequest, codex);
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException or IOException or JsonException)
        {
            Console.Error.WriteLine($"codex-review: {CodexReviewExit.Fault} (exit {(int)CodexReviewExit.Fault}). {exception.Message}");
            return (int)CodexReviewExit.Fault;
        }
    }

    private static CodexReviewExit Review(string root, int pullRequest, string codex)
    {
        var git = new GitRepository(root);
        ProcessResult versionResult;
        try
        {
            versionResult = ExternalProcess.Run(codex, ["--version"], root, CodexReviewSettings.ApiCredentialVariables);
        }
        catch (InvalidOperationException exception)
        {
            return Refuse(pullRequest, [$"The Codex CLI is missing. {exception.Message}. Run `npm install -g @openai/codex@latest` (D-512)."]);
        }

        CodexVersion version = CodexVersion.Parse(versionResult.RequireSuccess());
        PullRequestView view = ReadPullRequest(root, pullRequest);
        if (view.State != StartChecks.OpenState)
        {
            // GitHub can delete the branch of a closed PR, so the refusal comes before the fetch.
            return Refuse(pullRequest, [StartChecks.NotOpenProblem(pullRequest, view.State)]);
        }

        string loginStatus = ExternalProcess.Run(codex, CodexReviewSettings.LoginStatusArguments, root, CodexReviewSettings.ApiCredentialVariables).StandardOutput;
        StartFacts facts = GatherStartFacts(root, git, pullRequest, view, version, loginStatus);
        var problems = new List<string>(StartChecks.Problems(facts));
        if (problems.Count == 0)
        {
            string? probeProblem = ProbeModel(codex);
            if (probeProblem is not null)
            {
                problems.Add(probeProblem);
            }
        }

        if (problems.Count > 0)
        {
            return Refuse(pullRequest, problems);
        }

        string effectiveBefore = facts.EffectiveHead;
        string workDirectory = Path.Combine(Path.GetTempPath(), "wyc-codex-review", $"pr-{pullRequest}-{DateTimeOffset.UtcNow:yyyyMMddTHHmmssZ}");
        Directory.CreateDirectory(workDirectory);
        string worktree = Path.Combine(workDirectory, "worktree");
        string transcript = Path.Combine(workDirectory, "transcript.jsonl");
        string errorLog = Path.Combine(workDirectory, "codex-stderr.txt");
        string lastMessage = Path.Combine(workDirectory, "last-message.md");
        git.Run(["worktree", "add", "--detach", worktree, facts.OriginHead]);
        Console.WriteLine($"codex-review: PR #{pullRequest}, effective head {effectiveBefore}, model {CodexReviewSettings.Model} at effort {CodexReviewSettings.ReasoningEffort}, CLI {version}.");
        Console.WriteLine($"Transcript: {transcript}");

        IReadOnlyList<string> arguments = CodexReviewSettings.ReviewArguments(worktree, lastMessage, CodexReviewSettings.ReviewPrompt(pullRequest, view.Branch));
        int codexExit = ExternalProcess.RunToFiles(codex, arguments, worktree, transcript, errorLog, CodexReviewSettings.ApiCredentialVariables);
        if (codexExit != 0)
        {
            Console.Error.WriteLine($"codex-review: {CodexReviewExit.Fault} (exit {(int)CodexReviewExit.Fault}). Codex exited {codexExit}. Read the transcript {transcript} and the log {errorLog}. The worktree stays at {worktree}.");
            return CodexReviewExit.Fault;
        }

        git.Run(["worktree", "remove", "--force", worktree]);
        FetchBranch(git, view.Branch);
        FetchBranch(git, view.BaseBranch);
        ReviewOutcome outcome = JudgeRound(git, pullRequest, view, facts.OriginHead, effectiveBefore);
        Print(outcome, effectiveBefore, transcript, lastMessage);
        return outcome.Exit;
    }

    /// <summary>
    /// Judges the round from the branch on origin after the fetch. The round must push a new commit, and that commit
    /// must keep the effective head, because a reviewer pushes a metadata commit alone (D-182, D-184).
    /// </summary>
    public static ReviewOutcome JudgeRound(GitRepository git, int pullRequest, PullRequestView view, string headBefore, string effectiveBefore)
    {
        string headAfter = git.Run(["rev-parse", $"refs/remotes/origin/{view.Branch}"]).Trim();
        if (headAfter == headBefore)
        {
            return new ReviewOutcome(CodexReviewExit.Fault, "none", [], [], $"origin/{view.Branch} is still at {headBefore}. The review pushed no commit.");
        }

        string effectiveAfter = EffectiveHead(git, view, headAfter);
        if (effectiveAfter != effectiveBefore)
        {
            return new ReviewOutcome(CodexReviewExit.Fault, "none", [], [], $"The effective head moved from {effectiveBefore} to {effectiveAfter} during the review. A commit outside the metadata set arrived (D-184).");
        }

        string reviewFile = ReviewGateRules.ReviewFilePath(pullRequest);
        return ReviewOutcomeRules.Judge(git.ReadFileOrNull(headAfter, reviewFile), reviewFile, effectiveAfter);
    }

    /// <summary>The newest commit from the merge base to the head that changes a path outside the metadata set (D-184).</summary>
    public static string EffectiveHead(GitRepository git, PullRequestView view, string head)
    {
        string mergeBase = git.MergeBase($"refs/remotes/origin/{view.BaseBranch}", head);
        CommitStamp? effective = git.NewestCommitOutside(mergeBase, head, ReviewGateRules.MetadataPaths);
        return effective?.Sha
            ?? throw new InvalidOperationException($"No commit from {mergeBase} to {head} changes a path outside the metadata set, so the review has nothing to approve. The '{ReviewGateRules.OverrideLabel}' label covers such a PR (D-190).");
    }

    private static PullRequestView ReadPullRequest(string root, int pullRequest)
    {
        string json = ExternalProcess.Run("gh", ["pr", "view", pullRequest.ToString(CultureInfo.InvariantCulture), "--json", "state,headRefName,headRefOid,baseRefName"], root).RequireSuccess();
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement pr = document.RootElement;
        return new PullRequestView(
            RequiredString(pr, "state", pullRequest),
            RequiredString(pr, "headRefName", pullRequest),
            RequiredString(pr, "headRefOid", pullRequest),
            RequiredString(pr, "baseRefName", pullRequest));
    }

    private static StartFacts GatherStartFacts(string root, GitRepository git, int pullRequest, PullRequestView view, CodexVersion version, string loginStatus)
    {
        FetchBranch(git, view.Branch);
        FetchBranch(git, view.BaseBranch);
        string repository = ExternalProcess.Run("gh", ["repo", "view", "--json", "nameWithOwner", "--jq", ".nameWithOwner"], root).RequireSuccess().Trim();
        string originHead = git.Run(["rev-parse", $"refs/remotes/origin/{view.Branch}"]).Trim();
        string effectiveHead = EffectiveHead(git, view, originHead);
        var gitarChecks = new List<GitarCheck>();
        foreach (string sha in CommitsFrom(git, effectiveHead, originHead))
        {
            gitarChecks.AddRange(ReadGitarChecks(root, repository, sha));
        }

        return new StartFacts
        {
            PullRequestNumber = pullRequest,
            Version = version,
            LoginStatus = loginStatus,
            PullRequestState = view.State,
            PullRequestBranch = view.Branch,
            PullRequestHead = view.Head,
            LocalBranch = git.Run(["rev-parse", "--abbrev-ref", "HEAD"]).Trim(),
            LocalHead = git.Run(["rev-parse", "HEAD"]).Trim(),
            OriginHead = originHead,
            WorkingTreeStatus = git.Run(["status", "--porcelain"]),
            EffectiveHead = effectiveHead,
            GitarChecks = gitarChecks,
            DashboardEditedAt = ReadDashboardEditTime(root, repository, pullRequest),
            UnresolvedThreadCount = CountUnresolvedThreads(root, repository, pullRequest),
        };
    }

    /// <summary>The effective head and each later commit up to the head.</summary>
    private static List<string> CommitsFrom(GitRepository git, string effectiveHead, string head)
    {
        var commits = new List<string> { effectiveHead };
        commits.AddRange(git.Run(["rev-list", $"{effectiveHead}..{head}"]).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return commits;
    }

    /// <summary>Every Gitar check run on the commit.</summary>
    private static List<GitarCheck> ReadGitarChecks(string root, string repository, string sha)
    {
        string lines = ExternalProcess.Run(
            "gh",
            ["api", "--paginate", $"repos/{repository}/commits/{sha}/check-runs?per_page=100", "--jq", $".check_runs[] | select(.app.slug == \"{GitarApp}\") | \"\\(.status) \\(.started_at)\""],
            root).RequireSuccess();
        var checks = new List<GitarCheck>();
        foreach (string line in lines.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] parts = line.Split(' ');
            if (parts.Length != 2)
            {
                throw new FormatException($"A Gitar check run on {sha} gave '{line}', and the expected form is '<status> <start time>'.");
            }

            checks.Add(new GitarCheck(sha, parts[0], DateTimeOffset.Parse(parts[1], CultureInfo.InvariantCulture)));
        }

        return checks;
    }

    /// <summary>The last edit time of the newest Gitar dashboard comment, or null when the PR has none.</summary>
    private static DateTimeOffset? ReadDashboardEditTime(string root, string repository, int pullRequest)
    {
        string lines = ExternalProcess.Run(
            "gh",
            ["api", "--paginate", $"repos/{repository}/issues/{pullRequest}/comments?per_page=100", "--jq", $".[] | select(.user.login == \"{GitarLogin}\") | select(.body | contains(\"{DashboardMarker}\")) | .updated_at"],
            root).RequireSuccess();
        string[] times = lines.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return times.Length == 0 ? null : DateTimeOffset.Parse(times[^1], CultureInfo.InvariantCulture);
    }

    private static int CountUnresolvedThreads(string root, string repository, int pullRequest)
    {
        string owner = repository[..repository.IndexOf('/')];
        string name = repository[(repository.IndexOf('/') + 1)..];
        const string query = "query($owner: String!, $name: String!, $number: Int!, $endCursor: String) { repository(owner: $owner, name: $name) { pullRequest(number: $number) { reviewThreads(first: 100, after: $endCursor) { pageInfo { hasNextPage endCursor } nodes { isResolved } } } } }";
        string lines = ExternalProcess.Run(
            "gh",
            ["api", "graphql", "--paginate", "-F", $"owner={owner}", "-F", $"name={name}", "-F", $"number={pullRequest}", "-f", $"query={query}", "--jq", ".data.repository.pullRequest.reviewThreads.nodes[] | .isResolved"],
            root).RequireSuccess();
        int unresolved = 0;
        foreach (string line in lines.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line == "false")
            {
                unresolved++;
            }
        }

        return unresolved;
    }

    /// <summary>One short call with the model of the review. Returns null when the model answers, or the problem with the output.</summary>
    private static string? ProbeModel(string codex)
    {
        string directory = Path.Combine(Path.GetTempPath(), "wyc-codex-probe-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        ProcessResult result = ExternalProcess.Run(codex, CodexReviewSettings.ProbeArguments(directory), directory, CodexReviewSettings.ApiCredentialVariables);
        Directory.Delete(directory, recursive: true);
        if (result.ExitCode == 0 && result.StandardOutput.Contains("agent_message", StringComparison.Ordinal))
        {
            return null;
        }

        return $"The model probe of '{CodexReviewSettings.Model}' failed with exit {result.ExitCode} (D-512). stdout: {result.StandardOutput.Trim()} stderr: {result.StandardError.Trim()}";
    }

    private static CodexReviewExit Refuse(int pullRequest, IReadOnlyList<string> problems)
    {
        Console.Error.WriteLine($"codex-review: {CodexReviewExit.Refused} (exit {(int)CodexReviewExit.Refused}). PR #{pullRequest} does not meet the start conditions:");
        foreach (string problem in problems)
        {
            Console.Error.WriteLine($"- {problem}");
        }

        return CodexReviewExit.Refused;
    }

    private static void FetchBranch(GitRepository git, string branch)
    {
        git.Run(["fetch", "--quiet", "origin", $"+refs/heads/{branch}:refs/remotes/origin/{branch}"]);
    }

    private static string RequiredString(JsonElement element, string property, int pullRequest)
    {
        if (!element.TryGetProperty(property, out JsonElement value) || value.ValueKind != JsonValueKind.String)
        {
            throw new FormatException($"gh pr view {pullRequest} gave no text field '{property}'.");
        }

        return value.GetString() ?? throw new FormatException($"gh pr view {pullRequest} gave a null '{property}'.");
    }

    private static void Print(ReviewOutcome outcome, string effectiveHead, string transcript, string lastMessage)
    {
        Console.WriteLine($"codex-review: {outcome.Exit} (exit {(int)outcome.Exit}).");
        Console.WriteLine($"Verdict: {outcome.Verdict}");
        Console.WriteLine($"Effective head: {effectiveHead}");
        Console.WriteLine($"Open findings: {JoinOrNone(outcome.OpenFindingIds)}");
        Console.WriteLine($"Three-strike findings: {JoinOrNone(outcome.StrikeFindingIds)}");
        Console.WriteLine(outcome.Message);
        Console.WriteLine($"Transcript: {transcript}");
        Console.WriteLine($"Last message: {lastMessage}");
    }

    private static string JoinOrNone(IReadOnlyList<string> ids)
    {
        return ids.Count == 0 ? "none" : string.Join(", ", ids);
    }
}
