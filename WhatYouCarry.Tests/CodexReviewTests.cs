using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WhatYouCarry.Tools.CodexReview;
using WhatYouCarry.Tools.ReviewGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The rules of <c>codex-review</c> (D-511 to D-515): the CLI version, the Codex arguments, the start checks, and
/// the outcome of a round with the three-strike count.
/// </summary>
public sealed class CodexReviewTests
{
    private const string Head = "1111111111111111111111111111111111111111";
    private const string RoundOne = "aaaaaaa";
    private const string RoundTwo = "bbbbbbb";
    private const string ReviewFile = "docs/reviews/pr-93.md";
    private const string Approve = "Ready for owner merge";
    private const string Changes = "Changes required";

    [Theory]
    [InlineData("codex-cli 0.156.1\n", 0, 156, 1, "")]
    [InlineData("codex-cli 0.155.0-alpha.9.2", 0, 155, 0, "alpha.9.2")]
    public void VersionReadsTheNumbersAndThePrerelease(string output, int major, int minor, int patch, string prerelease)
    {
        Assert.Equal(new CodexVersion(major, minor, patch, prerelease), CodexVersion.Parse(output));
    }

    [Theory]
    [InlineData("codex-cli 0.156.1", true)]
    [InlineData("codex-cli 0.156.2", true)]
    [InlineData("codex-cli 0.157.0-alpha.11", true)]
    [InlineData("codex-cli 1.0.0", true)]
    [InlineData("codex-cli 0.156.1-alpha.1", false)]
    [InlineData("codex-cli 0.155.0-alpha.9.2", false)]
    [InlineData("codex-cli 0.39.0", false)]
    public void VersionPassesTheMinimumOrNewer(string output, bool passes)
    {
        // D-512: 0.156.1 ran the model probe on 2026-09-23. The Homebrew 0.39.0 and the app bundle build are older.
        Assert.Equal(passes, CodexVersion.Parse(output).IsAtLeast(CodexReviewSettings.MinimumVersion));
    }

    [Theory]
    [InlineData("codex 0.156.1")]
    [InlineData("codex-cli 0.156")]
    [InlineData("codex-cli 0.x.1")]
    public void VersionRefusesAnotherForm(string output)
    {
        FormatException exception = Assert.Throws<FormatException>(() => CodexVersion.Parse(output));
        Assert.Contains("version", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewRunNamesTheModelTheEffortAndTheSandbox()
    {
        // D-511: the effort comes from the command line, and never from the user configuration.
        IReadOnlyList<string> args = CodexReviewSettings.ReviewArguments("/tmp/worktree", "/tmp/last.md", "Review PR #93.");

        Assert.Equal("exec", args[0]);
        AssertPair(args, "-m", "gpt-6-luna");
        AssertPair(args, "-s", "danger-full-access");
        AssertPair(args, "-C", "/tmp/worktree");
        AssertPair(args, "-o", "/tmp/last.md");
        Assert.Contains("model_reasoning_effort=\"medium\"", args);
        Assert.Contains("approval_policy=\"never\"", args);
        Assert.Contains("forced_login_method=\"chatgpt\"", args);
        Assert.Contains("--json", args);
        Assert.Equal("Review PR #93.", args[^1]);
    }

    [Fact]
    public void ProbeNamesTheSameModelAndEffortWithNoWriteAccess()
    {
        IReadOnlyList<string> args = CodexReviewSettings.ProbeArguments("/tmp/probe");

        AssertPair(args, "-m", CodexReviewSettings.Model);
        AssertPair(args, "-s", "read-only");
        Assert.Contains("model_reasoning_effort=\"medium\"", args);
        Assert.Contains("forced_login_method=\"chatgpt\"", args);
        Assert.Contains("--skip-git-repo-check", args);
        Assert.Contains("--ephemeral", args);
    }

    [Fact]
    public void ReviewPromptIsTheOwnerTextWithTheSkillAndThePush()
    {
        string prompt = CodexReviewSettings.ReviewPrompt(93, "feat/pr-78-codex-review");

        Assert.StartsWith("Review PR #93.\n", prompt, StringComparison.Ordinal);
        Assert.Contains("`.claude/skills/pr-review/SKILL.md`", prompt, StringComparison.Ordinal);
        Assert.Contains("`git push origin HEAD:feat/pr-78-codex-review`", prompt, StringComparison.Ordinal);
        Assert.Contains("(D-182)", prompt, StringComparison.Ordinal);
    }

    [Fact]
    public void AnApprovingRecordOfTheEffectiveHeadApproves()
    {
        ReviewOutcome outcome = Judge(Record(Head, Approve));

        Assert.Equal(CodexReviewExit.Approve, outcome.Exit);
        Assert.Empty(outcome.OpenFindingIds);
    }

    [Fact]
    public void AFindingOpenInRoundsOneAndTwoDoesNotStop()
    {
        ReviewOutcome outcome = Judge(Record(Head, Changes, Finding("P1-1", "open", RoundOne, Head)));

        Assert.Equal(CodexReviewExit.ChangesRequired, outcome.Exit);
        Assert.Equal(["P1-1"], outcome.OpenFindingIds);
        Assert.Empty(outcome.StrikeFindingIds);
    }

    [Fact]
    public void AFindingOpenInRoundThreeStops()
    {
        // D-513: the third round in which one id is open stops the fix loop.
        ReviewOutcome outcome = Judge(Record(Head, Changes, Finding("P2-1", "open", RoundOne, RoundTwo, Head), Finding("P1-2", "open", Head)));

        Assert.Equal(CodexReviewExit.ThreeStrikes, outcome.Exit);
        Assert.Equal(["P2-1"], outcome.StrikeFindingIds);
        Assert.Equal(["P2-1", "P1-2"], outcome.OpenFindingIds);
    }

    [Fact]
    public void AFixedAndReopenedFindingCountsEachOpenRound()
    {
        // D-514: the same id counts one time for each round in which it is open. Round three fixed it, and round
        // four reopened it, so round four is the third open round.
        ReviewOutcome third = Judge(Record(Head, Changes, Finding("P1-1", "open", RoundOne, RoundTwo, Head)));
        ReviewOutcome second = Judge(Record(Head, Changes, Finding("P1-1", "open", RoundOne, Head)));

        Assert.Equal(CodexReviewExit.ThreeStrikes, third.Exit);
        Assert.Equal(CodexReviewExit.ChangesRequired, second.Exit);
    }

    [Fact]
    public void AP3FindingNeverStops()
    {
        // D-515: a P3 never blocks the merge, so it never drives the fix loop.
        ReviewOutcome outcome = Judge(Record(Head, Changes, Finding("P3-1", "open", RoundOne, RoundTwo, Head), Finding("P2-1", "open", Head)));

        Assert.Equal(CodexReviewExit.ChangesRequired, outcome.Exit);
        Assert.Empty(outcome.StrikeFindingIds);
    }

    [Fact]
    public void AnApprovalWinsOverAnOpenCount()
    {
        ReviewOutcome outcome = Judge(Record(Head, Approve, Finding("P3-1", "open", RoundOne, RoundTwo, Head)));

        Assert.Equal(CodexReviewExit.Approve, outcome.Exit);
        Assert.Equal(["P3-1"], outcome.OpenFindingIds);
    }

    [Theory]
    [InlineData("P0-1")]
    [InlineData("P1-1")]
    [InlineData("P2-1")]
    public void AnApprovalWithAnOpenBlockingFindingIsAFault(string id)
    {
        // PR #93 review P1-1: an approval needs no blocking finding, so the command never reports it as an approval.
        ReviewOutcome outcome = Judge(Record(Head, Approve, Finding(id, "open", Head)));

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains(id, outcome.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("### P4-1: A defect", "severity P4")]
    [InlineData("### P10-1: A defect", "severity P10")]
    [InlineData("### P1: A defect", "not a finding heading")]
    [InlineData("### Notes", "not a finding heading")]
    [InlineData("#### P1-1: A defect", "not a finding heading")]
    [InlineData("###P1-1: A defect", "not a finding heading")]
    public void AFindingHeadingOutsideTheFormatIsAFault(string heading, string expected)
    {
        // PR #93 review P2-1: a severity outside P0 to P3, or a heading that the parser skips, never passes as
        // a nonblocking finding under an approval.
        string record = Record(Head, Approve, heading + "\n\nStatus: open.\n\nOpen at: `" + Head + "`.\n");

        ReviewOutcome outcome = Judge(record);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains(expected, outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnApprovalWithAnAcceptedRiskApproves()
    {
        ReviewOutcome outcome = Judge(Record(Head, Approve, Finding("P2-1", "accepted risk, D-524", RoundOne, RoundTwo)));

        Assert.Equal(CodexReviewExit.Approve, outcome.Exit);
    }

    [Fact]
    public void ABlockedRecordAsksForChanges()
    {
        Assert.Equal(CodexReviewExit.ChangesRequired, Judge(Record(Head, "Blocked")).Exit);
    }

    [Fact]
    public void AClosedFindingNeedsNoHeadOfThisRound()
    {
        ReviewOutcome outcome = Judge(Record(Head, Approve, Finding("P1-1", "fixed in `2222222`", RoundOne, RoundTwo)));

        Assert.Equal(CodexReviewExit.Approve, outcome.Exit);
    }

    [Fact]
    public void NoRecordIsAFault()
    {
        ReviewOutcome outcome = ReviewOutcomeRules.Judge(null, ReviewFile, Head);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains(ReviewFile, outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStaleHeadIsAFault()
    {
        ReviewOutcome outcome = Judge(Record("2222222", Approve));

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains("stale", outcome.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("no 'Open at:' line")]
    [InlineData("does not list the head of this review")]
    [InlineData("two times")]
    [InlineData("seven characters")]
    public void AnOpenFindingWithAWrongHeadListIsAFault(string expected)
    {
        // T-2: a count that a reviewer forgot to extend is wrong in silence, so the rule stops.
        string finding = expected switch
        {
            "no 'Open at:' line" => Finding("P1-1", "open"),
            "does not list the head of this review" => Finding("P1-1", "open", RoundOne, RoundTwo),
            "two times" => Finding("P1-1", "open", RoundOne, "aaaaaaaaaa", Head),
            _ => Finding("P1-1", "open", "abc", Head),
        };

        ReviewOutcome outcome = Judge(Record(Head, Changes, finding));

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains("P1-1", outcome.Message, StringComparison.Ordinal);
        Assert.Contains(expected, outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFindingWithNoStatusIsAFault()
    {
        string record = Record(Head, Changes, "### P1-1: A defect\n\nFile: `a.cs:1`.\n");

        ReviewOutcome outcome = Judge(record);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains("P1-1", outcome.Message, StringComparison.Ordinal);
        Assert.Contains("Status:", outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFindingHeadingOutsideTheFindingsSectionDoesNotCount()
    {
        string record = Record(Head, Approve) + "\n## Earlier notes\n\n### P1-9: Old\n\nStatus: open.\n";

        ReviewOutcome outcome = Judge(record);

        Assert.Equal(CodexReviewExit.Approve, outcome.Exit);
        Assert.Empty(outcome.OpenFindingIds);
    }

    [Fact]
    public void StartChecksPassWhenEveryConditionHolds()
    {
        Assert.Empty(StartChecks.Problems(GoodFacts(), skipGitarReview: false));
    }

    public static TheoryData<string, string> StartProblems()
    {
        return new TheoryData<string, string>
        {
            { "old-cli", "minimum is 0.156.1" },
            { "api-login", "never uses API pricing" },
            { "no-login", "never uses API pricing" },
            { "closed", "is MERGED" },
            { "other-branch", "The checkout is on 'main'" },
            { "local-ahead", "differs from origin" },
            { "github-behind", "GitHub gives the PR head" },
            { "dirty", "The working tree is dirty" },
            { "no-gitar-check", "has a Gitar check run" },
            { "gitar-running", "is 'in_progress'" },
            { "no-dashboard", "has no Gitar dashboard comment" },
            { "stale-dashboard", "is not current" },
            { "open-thread", "2 unresolved review thread(s)" },
        };
    }

    [Theory]
    [MemberData(nameof(StartProblems))]
    public void StartChecksRefuseEachFailedCondition(string change, string expected)
    {
        StartFacts good = GoodFacts();
        DateTimeOffset started = good.GitarChecks[0].StartedAt;
        StartFacts facts = change switch
        {
            "old-cli" => With(good, version: CodexVersion.Parse("codex-cli 0.155.0-alpha.9.2")),
            "api-login" => With(good, loginStatus: "Logged in using an API key - sk-proj-***\n"),
            "no-login" => With(good, loginStatus: "Not logged in\n"),
            "closed" => With(good, state: "MERGED"),
            "other-branch" => With(good, localBranch: "main"),
            "local-ahead" => With(good, localHead: "3333333333333333333333333333333333333333"),
            "github-behind" => With(good, pullRequestHead: "4444444444444444444444444444444444444444"),
            "dirty" => With(good, status: " M Makefile\n"),
            "no-gitar-check" => With(good, gitarChecks: []),
            "gitar-running" => With(good, gitarChecks: [new GitarCheck(Head, "in_progress", started)]),
            "no-dashboard" => With(good, dashboard: null, clearDashboard: true),
            "stale-dashboard" => With(good, dashboard: started.AddSeconds(-1)),
            "open-thread" => With(good, unresolved: 2),
            _ => throw new ArgumentException($"Unknown change '{change}'."),
        };

        IReadOnlyList<string> problems = StartChecks.Problems(facts, skipGitarReview: false);

        string problem = Assert.Single(problems);
        Assert.Contains(expected, problem, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryApiCredentialVariableLeavesTheCodexEnvironment()
    {
        // D-523: the CLI reads a credential from each of these three variables. Each Codex process loses all three.
        Assert.Equal(["OPENAI_API_KEY", "CODEX_API_KEY", "CODEX_ACCESS_TOKEN"], CodexReviewSettings.ApiCredentialVariables);
    }

    [Fact]
    public void TheChildLosesARemovedVariableAndTheParentKeepsIt()
    {
        // A unique name, so no other test sees the variable.
        string name = "WYC_TEST_CREDENTIAL_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(name, "fake-value");
        try
        {
            string stripped = ReadChildEnvironment([name]);
            string kept = ReadChildEnvironment([]);

            Assert.DoesNotContain(name, stripped, StringComparison.Ordinal);
            Assert.Contains(name + "=fake-value", kept, StringComparison.Ordinal);
            Assert.Equal("fake-value", Environment.GetEnvironmentVariable(name));
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Fact]
    public void TheLoginStatusReadsTheStderrOfTheCli()
    {
        // PR #93 automated pass: `codex login status` writes its status line to stderr, and stdout stays empty.
        ProcessResult result = OperatingSystem.IsWindows()
            ? ExternalProcess.Run("cmd.exe", ["/c", "echo Logged in using ChatGPT 1>&2"], System.IO.Path.GetTempPath())
            : ExternalProcess.Run("sh", ["-c", "echo 'Logged in using ChatGPT' >&2"], System.IO.Path.GetTempPath());

        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.StartsWith(CodexReviewSettings.ChatGptLoginStatus, CodexReviewSettings.LoginStatusText(result), StringComparison.Ordinal);
        Assert.Empty(StartChecks.Problems(With(GoodFacts(), loginStatus: CodexReviewSettings.LoginStatusText(result)), skipGitarReview: false));
    }

    /// <summary>The environment that a child process prints, with the removed variables.</summary>
    private static string ReadChildEnvironment(IReadOnlyList<string> removed)
    {
        string directory = System.IO.Path.GetTempPath();
        ProcessResult result = OperatingSystem.IsWindows()
            ? ExternalProcess.Run("cmd.exe", ["/c", "set"], directory, removed)
            : ExternalProcess.Run("env", [], directory, removed);
        return result.RequireSuccess();
    }

    [Fact]
    public void ADocumentsOnlyPullRequestIsRefused()
    {
        // D-540: no commit lies outside the skip set, so the review has nothing to approve. The problem names the label.
        IReadOnlyList<string> problems = StartChecks.Problems(With(GoodFacts(), clearEffectiveHead: true), skipGitarReview: false);

        string problem = Assert.Single(problems);
        Assert.Contains(ReviewGateRules.OverrideLabel, problem, StringComparison.Ordinal);
        Assert.Contains("D-540", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void AMetadataOnlyPullRequestIsRefusedWithOneProblem()
    {
        // A PR of metadata alone has no work head either. The Gitar checks need a work head, so they add nothing.
        IReadOnlyList<string> problems = StartChecks.Problems(With(GoodFacts(), clearEffectiveHead: true, clearWorkHead: true, gitarChecks: []), skipGitarReview: false);

        string problem = Assert.Single(problems);
        Assert.Contains(ReviewGateRules.OverrideLabel, problem, StringComparison.Ordinal);
    }

    [Fact]
    public void TheGitarProblemNamesTheWorkHead()
    {
        // D-534: the Gitar pass keeps the metadata set of D-184, so a missing check names the work head.
        const string work = "6666666666666666666666666666666666666666";
        IReadOnlyList<string> problems = StartChecks.Problems(With(GoodFacts(), workHead: work, gitarChecks: []), skipGitarReview: false);

        Assert.Contains(problems, problem => problem.Contains($"the work head {work}", StringComparison.Ordinal));
    }

    [Fact]
    public void AMetadataTipKeepsThePassOfTheWorkHead()
    {
        // D-184: a paused Gitar attaches a check to a metadata tip and edits no dashboard. The pass of the work head
        // stays current, so the later check does not refuse the round.
        StartFacts good = GoodFacts();
        DateTimeOffset dashboard = good.DashboardEditedAt!.Value;
        StartFacts facts = With(good, gitarChecks: [good.GitarChecks[0], new GitarCheck("5555555555555555555555555555555555555555", "completed", dashboard.AddMinutes(5))]);

        Assert.Empty(StartChecks.Problems(facts, skipGitarReview: false));
    }

    [Theory]
    [InlineData("no-gitar-check")]
    [InlineData("gitar-running")]
    [InlineData("no-dashboard")]
    [InlineData("stale-dashboard")]
    public void TheSkipFlagDropsEachGitarProblem(string change)
    {
        // D-543: with --skip-gitar-review, no Gitar check run and no Gitar dashboard refuses the round.
        StartFacts good = GoodFacts();
        DateTimeOffset started = good.GitarChecks[0].StartedAt;
        StartFacts facts = change switch
        {
            "no-gitar-check" => With(good, gitarChecks: []),
            "gitar-running" => With(good, gitarChecks: [new GitarCheck(Head, "in_progress", started)]),
            "no-dashboard" => With(good, clearDashboard: true),
            "stale-dashboard" => With(good, dashboard: started.AddSeconds(-1)),
            _ => throw new ArgumentException($"Unknown change '{change}'."),
        };

        Assert.NotEmpty(StartChecks.Problems(facts, skipGitarReview: false));
        Assert.Empty(StartChecks.Problems(facts, skipGitarReview: true));
    }

    [Fact]
    public void TheSkipFlagKeepsTheThreadCheck()
    {
        // D-522, D-543: the ruleset of main requires resolved threads, so an open thread refuses the round with the flag.
        StartFacts facts = With(GoodFacts(), gitarChecks: [], clearDashboard: true, unresolved: 1);

        string problem = Assert.Single(StartChecks.Problems(facts, skipGitarReview: true));
        Assert.Contains("1 unresolved review thread(s)", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSkipFlagKeepsEveryOtherCheck()
    {
        // D-543: the flag drops the Gitar checks alone. A dirty tree and an old CLI still refuse the round.
        StartFacts facts = With(GoodFacts(), version: CodexVersion.Parse("codex-cli 0.155.0"), status: " M Makefile\n", gitarChecks: []);

        IReadOnlyList<string> problems = StartChecks.Problems(facts, skipGitarReview: true);

        Assert.Equal(2, problems.Count);
        Assert.Contains(problems, problem => problem.Contains("minimum is 0.156.1", StringComparison.Ordinal));
        Assert.Contains(problems, problem => problem.Contains("The working tree is dirty", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(new[] { "--root", ".", "--pr", "96", "--codex", "codex" }, false)]
    [InlineData(new[] { "--root", ".", "--pr", "96", "--codex", "codex", "--skip-gitar-review" }, true)]
    [InlineData(new[] { "--skip-gitar-review", "--root", ".", "--pr", "96", "--codex", "codex" }, true)]
    [InlineData(new[] { "--root", ".", "--skip-gitar-review", "--pr", "96", "--codex", "codex" }, true)]
    public void TheOptionsReadTheSkipFlagInAnyPlace(string[] args, bool skip)
    {
        // D-543: `make codex-review PR=<n> -- --skip-gitar-review` puts the flag after the three options.
        CodexReviewOptions? options = CodexReviewCommand.ParseOptions(args, out string problem);

        Assert.NotNull(options);
        Assert.Equal(string.Empty, problem);
        Assert.Equal(new CodexReviewOptions(".", 96, "codex", skip), options);
    }

    [Theory]
    [InlineData(new[] { "--root", ".", "--pr", "96", "--codex", "codex", "--skip-gitar" }, "Unknown option '--skip-gitar'.")]
    [InlineData(new[] { "--root", ".", "--pr", "96", "--codex" }, "The option '--codex' needs a value.")]
    [InlineData(new[] { "--root", ".", "--pr", "x96", "--codex", "codex" }, "Found --pr 'x96'.")]
    [InlineData(new[] { "--root", ".", "--codex", "codex", "--skip-gitar-review" }, "Found --pr ''.")]
    public void TheOptionsRefuseAnUnknownOrIncompleteForm(string[] args, string expected)
    {
        CodexReviewOptions? options = CodexReviewCommand.ParseOptions(args, out string problem);

        Assert.Null(options);
        Assert.Contains(expected, problem, StringComparison.Ordinal);
    }

    private static ReviewOutcome Judge(string record)
    {
        return ReviewOutcomeRules.Judge(record, ReviewFile, Head);
    }

    /// <summary>A record with the skeleton of <c>review-record.md</c>: the Identity list, the findings, and one verdict.</summary>
    private static string Record(string head, string verdict, params string[] findings)
    {
        var text = new StringBuilder();
        text.Append("# PR-93 review\n\nDate: 2026-09-23\n\n## Identity\n\n- PR: 93\n");
        text.Append(CultureInfo.InvariantCulture, $"- Head: `{head}`\n\n## Findings\n\n");
        text.Append(findings.Length == 0 ? "No finding.\n" : string.Join("\n", findings));
        text.Append(CultureInfo.InvariantCulture, $"\n## Out of scope\n\nNone.\n\n## Verdict\n\n**{verdict}.** This verdict applies to head `{head}`.\n");
        return text.ToString();
    }

    /// <summary>A finding in the format of <c>findings.md</c>. No head gives no <c>Open at:</c> line.</summary>
    private static string Finding(string id, string status, params string[] openAt)
    {
        var text = new StringBuilder();
        text.Append(CultureInfo.InvariantCulture, $"### {id}: A defect\n\nStatus: {status}.\n\n");
        if (openAt.Length > 0)
        {
            text.Append("Open at: ");
            text.Append(string.Join(", ", Array.ConvertAll(openAt, head => $"`{head}`")));
            text.Append(".\n\n");
        }

        text.Append("File: `a.cs:1`.\n");
        return text.ToString();
    }

    private static StartFacts GoodFacts()
    {
        DateTimeOffset started = DateTimeOffset.Parse("2026-09-23T10:00:00Z", CultureInfo.InvariantCulture);
        return new StartFacts
        {
            PullRequestNumber = 93,
            Version = CodexVersion.Parse("codex-cli 0.156.1"),
            LoginStatus = "Logged in using ChatGPT\n",
            PullRequestState = "OPEN",
            PullRequestBranch = "feat/pr-78-codex-review",
            PullRequestHead = Head,
            LocalBranch = "feat/pr-78-codex-review",
            LocalHead = Head,
            OriginHead = Head,
            WorkingTreeStatus = string.Empty,
            EffectiveHead = Head,
            WorkHead = Head,
            GitarChecks = [new GitarCheck(Head, "completed", started)],
            DashboardEditedAt = started.AddMinutes(2),
            UnresolvedThreadCount = 0,
        };
    }

    private static StartFacts With(
        StartFacts facts,
        CodexVersion? version = null,
        string? loginStatus = null,
        string? state = null,
        string? localBranch = null,
        string? localHead = null,
        string? pullRequestHead = null,
        string? status = null,
        IReadOnlyList<GitarCheck>? gitarChecks = null,
        DateTimeOffset? dashboard = null,
        bool clearDashboard = false,
        int? unresolved = null,
        string? workHead = null,
        bool clearEffectiveHead = false,
        bool clearWorkHead = false)
    {
        return new StartFacts
        {
            PullRequestNumber = facts.PullRequestNumber,
            Version = version ?? facts.Version,
            LoginStatus = loginStatus ?? facts.LoginStatus,
            PullRequestState = state ?? facts.PullRequestState,
            PullRequestBranch = facts.PullRequestBranch,
            PullRequestHead = pullRequestHead ?? facts.PullRequestHead,
            LocalBranch = localBranch ?? facts.LocalBranch,
            LocalHead = localHead ?? facts.LocalHead,
            OriginHead = facts.OriginHead,
            WorkingTreeStatus = status ?? facts.WorkingTreeStatus,
            EffectiveHead = clearEffectiveHead ? null : facts.EffectiveHead,
            WorkHead = clearWorkHead ? null : workHead ?? facts.WorkHead,
            GitarChecks = gitarChecks ?? facts.GitarChecks,
            DashboardEditedAt = clearDashboard ? null : dashboard ?? facts.DashboardEditedAt,
            UnresolvedThreadCount = unresolved ?? facts.UnresolvedThreadCount,
        };
    }

    private static void AssertPair(IReadOnlyList<string> args, string option, string value)
    {
        int index = -1;
        for (int i = 0; i < args.Count; i++)
        {
            if (args[i] == option)
            {
                index = i;
                break;
            }
        }

        Assert.True(index >= 0 && index + 1 < args.Count, $"The arguments hold no option '{option}': {string.Join(' ', args)}");
        Assert.Equal(value, args[index + 1]);
    }
}
