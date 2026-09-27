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
/// The options of one run. <see cref="Cli"/> is the path of the reviewer CLI. <see cref="SkipGitarReview"/> drops
/// the Gitar start checks alone (D-543).
/// </summary>
public sealed record CodexReviewOptions(string Root, int PullRequest, string Cli, bool SkipGitarReview);

/// <summary>
/// <c>codex-review --root . --pr &lt;n&gt; --codex &lt;path&gt;</c> (D-511) and
/// <c>claude-review --root . --pr &lt;n&gt; --claude &lt;path&gt;</c> (D-649). The command checks the start
/// conditions, probes the model, runs one review round of the reviewer CLI (<see cref="IReviewer"/>) in a detached
/// worktree at the PR head, and judges the review record that the round pushed. The exit code names the outcome
/// (<see cref="CodexReviewExit"/>).
/// </summary>
public static class CodexReviewCommand
{
    public const string SkipGitarReviewFlag = "--skip-gitar-review";
    public const string GitarApp = "gitar-bot";
    public const string GitarLogin = "gitar-bot[bot]";
    public const string DashboardMarker = "<b>Code Review</b>";

    public static string Usage(IReviewer reviewer)
    {
        return $"Options: --root <path> --pr <number> {reviewer.CliOption} <path> [{SkipGitarReviewFlag}].";
    }

    public static int Run(string[] args, IReviewer reviewer)
    {
        CodexReviewOptions? options = ParseOptions(args, reviewer, out string usageProblem);
        if (options is null)
        {
            Console.Error.WriteLine($"{usageProblem} {Usage(reviewer)}");
            return 2;
        }

        try
        {
            return (int)Review(reviewer, Path.GetFullPath(options.Root), options.PullRequest, options.Cli, options.SkipGitarReview);
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException or IOException or JsonException)
        {
            Console.Error.WriteLine($"{reviewer.CommandName}: {CodexReviewExit.Fault} (exit {(int)CodexReviewExit.Fault}). {exception.Message}");
            return (int)CodexReviewExit.Fault;
        }
    }

    /// <summary>
    /// Reads the options. Each option takes one value, except the flag <see cref="SkipGitarReviewFlag"/>, which takes
    /// none. The option of the CLI is the one of the reviewer, such as <c>--codex</c>. Returns null, with the problem,
    /// on an unknown option, a missing value, or a PR that is not a whole number.
    /// </summary>
    public static CodexReviewOptions? ParseOptions(IReadOnlyList<string> args, IReviewer reviewer, out string problem)
    {
        string? root = null;
        string? pullRequestText = null;
        string? cli = null;
        bool skipGitarReview = false;
        int i = 0;
        while (i < args.Count)
        {
            string option = args[i];
            if (option == SkipGitarReviewFlag)
            {
                skipGitarReview = true;
                i++;
                continue;
            }

            if (option != "--root" && option != "--pr" && option != reviewer.CliOption)
            {
                problem = $"Unknown option '{option}'.";
                return null;
            }

            if (i + 1 >= args.Count)
            {
                problem = $"The option '{option}' needs a value.";
                return null;
            }

            string value = args[i + 1];
            if (option == "--root")
            {
                root = value;
            }
            else if (option == "--pr")
            {
                pullRequestText = value;
            }
            else
            {
                cli = value;
            }

            i += 2;
        }

        if (root is null || cli is null || !int.TryParse(pullRequestText, NumberStyles.None, CultureInfo.InvariantCulture, out int pullRequest))
        {
            problem = $"Each of --root, --pr, and {reviewer.CliOption} needs a value, and --pr needs a whole number. Found --pr '{pullRequestText}'.";
            return null;
        }

        problem = string.Empty;
        return new CodexReviewOptions(root, pullRequest, cli, skipGitarReview);
    }

    private static CodexReviewExit Review(IReviewer reviewer, string root, int pullRequest, string cli, bool skipGitarReview)
    {
        var git = new GitRepository(root);
        ProcessResult versionResult;
        try
        {
            versionResult = ExternalProcess.Run(cli, ["--version"], root, reviewer.CredentialVariables);
        }
        catch (InvalidOperationException exception)
        {
            return Refuse(reviewer, pullRequest, [reviewer.MissingCliProblem(exception.Message)]);
        }

        string versionOutput = versionResult.RequireSuccess();
        string version = reviewer.VersionText(versionOutput);
        PullRequestView view = ReadPullRequest(root, pullRequest);
        if (view.State != StartChecks.OpenState)
        {
            // GitHub can delete the branch of a closed PR, so the refusal comes before the fetch.
            return Refuse(reviewer, pullRequest, [StartChecks.NotOpenProblem(pullRequest, view.State)]);
        }

        ProcessResult loginStatus = ExternalProcess.Run(cli, reviewer.LoginStatusArguments, root, reviewer.CredentialVariables);
        StartFacts facts = GatherStartFacts(root, git, pullRequest, view, skipGitarReview);
        var problems = new List<string>(reviewer.CliProblems(versionOutput, loginStatus));
        problems.AddRange(StartChecks.Problems(facts, skipGitarReview));
        string? providerProblem = ProviderGate.Problem(git.ReadFileOrNull(facts.OriginHead, ProviderGate.HandoffPath), view.Branch, reviewer.PullRequestAuthor);
        if (providerProblem is not null)
        {
            problems.Add(providerProblem);
        }

        if (problems.Count == 0)
        {
            string? probeProblem = ProbeModel(reviewer, cli);
            if (probeProblem is not null)
            {
                problems.Add(probeProblem);
            }
        }

        if (problems.Count > 0)
        {
            return Refuse(reviewer, pullRequest, problems);
        }

        // The start checks refuse a PR with no effective head, and a PR with an effective head has a work head too.
        string effectiveBefore = facts.EffectiveHead ?? throw new InvalidOperationException($"PR #{pullRequest} passed the start checks with no effective head.");
        string workBefore = facts.WorkHead ?? throw new InvalidOperationException($"PR #{pullRequest} passed the start checks with no work head.");
        string workDirectory = Path.Combine(Path.GetTempPath(), $"wyc-{reviewer.CommandName}", $"pr-{pullRequest}-{DateTimeOffset.UtcNow:yyyyMMddTHHmmssZ}");
        Directory.CreateDirectory(workDirectory);
        string worktree = Path.Combine(workDirectory, "worktree");
        string transcript = Path.Combine(workDirectory, "transcript.jsonl");
        string errorLog = Path.Combine(workDirectory, $"{reviewer.CliName}-stderr.txt");
        string lastMessage = Path.Combine(workDirectory, "last-message.md");
        git.Run(["worktree", "add", "--detach", worktree, facts.OriginHead]);
        Console.WriteLine($"{reviewer.CommandName}: PR #{pullRequest}, effective head {effectiveBefore}, {reviewer.ModelText}, CLI {version}.");
        if (skipGitarReview)
        {
            Console.WriteLine($"{reviewer.CommandName}: {SkipGitarReviewFlag} skipped the Gitar check run and the dashboard. The unresolved thread check ran (D-543).");
        }

        Console.WriteLine($"Transcript: {transcript}");

        IReadOnlyList<string> arguments = reviewer.ReviewArguments(worktree, lastMessage, pullRequest, view.Branch);
        int reviewExit = ExternalProcess.RunToFiles(cli, arguments, worktree, transcript, errorLog, reviewer.CredentialVariables, ProcessLimit.Review);
        if (reviewExit != 0)
        {
            Console.Error.WriteLine($"{reviewer.CommandName}: {CodexReviewExit.Fault} (exit {(int)CodexReviewExit.Fault}). {reviewer.ProviderName} exited {reviewExit}. Read the transcript {transcript} and the log {errorLog}. The worktree stays at {worktree}.");
            return CodexReviewExit.Fault;
        }

        try
        {
            reviewer.SaveLastMessage(transcript, lastMessage);
        }
        catch (FormatException exception)
        {
            // A transcript with no last message is a fault, and the worktree stays for the diagnosis (D-649).
            Console.Error.WriteLine($"{reviewer.CommandName}: {CodexReviewExit.Fault} (exit {(int)CodexReviewExit.Fault}). {exception.Message} The worktree stays at {worktree}.");
            return CodexReviewExit.Fault;
        }

        git.Run(["worktree", "remove", "--force", worktree]);
        FetchBranch(git, view.Branch);
        FetchBranch(git, view.BaseBranch);
        ReviewOutcome outcome = JudgeRound(git, pullRequest, view, facts.OriginHead, workBefore);
        Print(reviewer, outcome, effectiveBefore, transcript, lastMessage);
        return outcome.Exit;
    }

    /// <summary>
    /// Judges the round from the branch on origin after the fetch. The round must push a new commit, and that commit
    /// must keep the work head, because a reviewer pushes a metadata commit alone (D-182, D-184). The record must
    /// name the effective head (D-534).
    /// </summary>
    public static ReviewOutcome JudgeRound(GitRepository git, int pullRequest, PullRequestView view, string headBefore, string workBefore)
    {
        string headAfter = git.Run(["rev-parse", $"refs/remotes/origin/{view.Branch}"]).Trim();
        if (headAfter == headBefore)
        {
            return new ReviewOutcome(CodexReviewExit.Fault, "none", [], [], $"origin/{view.Branch} is still at {headBefore}. The review pushed no commit.");
        }

        string? workAfter = WorkHead(git, view, headAfter);
        if (workAfter != workBefore)
        {
            return new ReviewOutcome(CodexReviewExit.Fault, "none", [], [], $"The work head moved from {workBefore} to {workAfter ?? "none"} during the review. A commit outside the metadata set arrived (D-182, D-184).");
        }

        // The work head stayed, so every new commit is a metadata commit, and the effective head stayed too.
        string effectiveAfter = EffectiveHead(git, view, headAfter)
            ?? throw new InvalidOperationException($"origin/{view.Branch} at {headAfter} has the work head {workAfter} and no effective head.");
        string reviewFile = ReviewGateRules.ReviewFilePath(pullRequest);
        return ReviewOutcomeRules.Judge(git.ReadFileOrNull(headAfter, reviewFile), reviewFile, effectiveAfter);
    }

    /// <summary>
    /// The newest commit from the merge base to the head that changes a path outside the skip set of D-475, or null
    /// when every commit changes documents alone. The review record names it (D-534).
    /// </summary>
    public static string? EffectiveHead(GitRepository git, PullRequestView view, string head)
    {
        string mergeBase = git.MergeBase($"refs/remotes/origin/{view.BaseBranch}", head);
        return git.NewestCommitOutside(mergeBase, head, ReviewGateRules.SkipPaths)?.Sha;
    }

    /// <summary>
    /// The newest commit from the merge base to the head that changes a path outside the metadata set, or null when
    /// every commit is a metadata commit. The Gitar pass reads it (D-184, D-534).
    /// </summary>
    public static string? WorkHead(GitRepository git, PullRequestView view, string head)
    {
        string mergeBase = git.MergeBase($"refs/remotes/origin/{view.BaseBranch}", head);
        return git.NewestCommitOutside(mergeBase, head, ReviewGateRules.MetadataPaths)?.Sha;
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

    /// <summary>
    /// Reads the start facts once. <paramref name="skipGitarReview"/> leaves the Gitar check runs and the dashboard
    /// unread, because the start checks then do not judge them (D-543, F-125).
    /// </summary>
    private static StartFacts GatherStartFacts(string root, GitRepository git, int pullRequest, PullRequestView view, bool skipGitarReview)
    {
        FetchBranch(git, view.Branch);
        FetchBranch(git, view.BaseBranch);
        string repository = ExternalProcess.Run("gh", ["repo", "view", "--json", "nameWithOwner", "--jq", ".nameWithOwner"], root).RequireSuccess().Trim();
        string originHead = git.Run(["rev-parse", $"refs/remotes/origin/{view.Branch}"]).Trim();
        string? workHead = WorkHead(git, view, originHead);
        List<GitarCheck>? gitarChecks = null;
        DateTimeOffset? dashboardEditedAt = null;
        if (!skipGitarReview)
        {
            gitarChecks = [];
            if (workHead is not null)
            {
                foreach (string sha in CommitsFrom(git, workHead, originHead))
                {
                    gitarChecks.AddRange(ReadGitarChecks(root, repository, sha));
                }
            }

            dashboardEditedAt = ReadDashboardEditTime(root, repository, pullRequest);
        }

        return new StartFacts
        {
            PullRequestNumber = pullRequest,
            PullRequestState = view.State,
            PullRequestBranch = view.Branch,
            PullRequestHead = view.Head,
            LocalBranch = git.Run(["rev-parse", "--abbrev-ref", "HEAD"]).Trim(),
            LocalHead = git.Run(["rev-parse", "HEAD"]).Trim(),
            OriginHead = originHead,
            WorkingTreeStatus = git.Run(["status", "--porcelain"]),
            EffectiveHead = EffectiveHead(git, view, originHead),
            WorkHead = workHead,
            GitarChecks = gitarChecks,
            DashboardEditedAt = dashboardEditedAt,
            UnresolvedThreadCount = CountUnresolvedThreads(root, repository, pullRequest),
        };
    }

    /// <summary>The work head and each later commit up to the head.</summary>
    private static List<string> CommitsFrom(GitRepository git, string workHead, string head)
    {
        var commits = new List<string> { workHead };
        commits.AddRange(git.Run(["rev-list", $"{workHead}..{head}"]).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return commits;
    }

    /// <summary>Every Gitar check run on the commit.</summary>
    private static List<GitarCheck> ReadGitarChecks(string root, string repository, string sha)
    {
        string lines = ExternalProcess.Run(
            "gh",
            ["api", "--paginate", $"repos/{repository}/commits/{sha}/check-runs?per_page=100", "--jq", $".check_runs[] | select(.app.slug == \"{GitarApp}\") | \"\\(.status) \\(.started_at)\""],
            root).RequireSuccess();
        return ParseGitarChecks(sha, lines);
    }

    /// <summary>
    /// Reads the lines <c>&lt;status&gt; &lt;start time&gt;</c> of the Gitar check runs on the commit. The jq text of
    /// a start time that GitHub does not give is <c>null</c>, and the check run then has no start time (F-125).
    /// </summary>
    /// <exception cref="FormatException">A line has another form, or a start time that is not a time. The message names the commit and the line.</exception>
    public static List<GitarCheck> ParseGitarChecks(string sha, string lines)
    {
        var checks = new List<GitarCheck>();
        foreach (string line in lines.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] parts = line.Split(' ');
            if (parts.Length != 2)
            {
                throw new FormatException($"A Gitar check run on {sha} gave '{line}', and the expected form is '<status> <start time>'.");
            }

            if (parts[1] == "null")
            {
                checks.Add(new GitarCheck(sha, parts[0], null));
                continue;
            }

            if (!DateTimeOffset.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset startedAt))
            {
                throw new FormatException($"A Gitar check run on {sha} gave '{line}', and '{parts[1]}' is not a start time.");
            }

            checks.Add(new GitarCheck(sha, parts[0], startedAt));
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

    /// <summary>
    /// One short call with the model of the review, in a new directory outside any repository. Returns null when the
    /// model answers, or the problem with the output.
    /// </summary>
    private static string? ProbeModel(IReviewer reviewer, string cli)
    {
        string directory = Path.Combine(Path.GetTempPath(), $"wyc-{reviewer.CliName}-probe-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        ProcessResult result = ExternalProcess.Run(cli, reviewer.ProbeArguments(directory), directory, reviewer.CredentialVariables);
        Directory.Delete(directory, recursive: true);
        return reviewer.ProbeProblem(result);
    }

    private static CodexReviewExit Refuse(IReviewer reviewer, int pullRequest, IReadOnlyList<string> problems)
    {
        Console.Error.WriteLine($"{reviewer.CommandName}: {CodexReviewExit.Refused} (exit {(int)CodexReviewExit.Refused}). PR #{pullRequest} does not meet the start conditions:");
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

    private static void Print(IReviewer reviewer, ReviewOutcome outcome, string effectiveHead, string transcript, string lastMessage)
    {
        Console.WriteLine($"{reviewer.CommandName}: {outcome.Exit} (exit {(int)outcome.Exit}).");
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
