using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhatYouCarry.Tools.CiSkip;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The CI skip of a head that changes documents alone (PR-71, D-473 to D-477). The rules skip the heavy jobs of a PR
/// whose paths are all documents, or of a push of documents after a previous head whose run passed. Every other case
/// runs every job.
/// </summary>
[Collection(ConsoleCollection.Name)]
[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
public sealed class CiSkipTests
{
    /// <summary>The four workflows whose jobs skip a documents head, each with the ids of its jobs that skip (D-474, D-477).</summary>
    private static readonly (string File, string[] Jobs)[] HeavyWorkflows =
    [
        ("ci.yml", ["linux-x64", "linux-x64-sweeps", "windows-x64", "windows-x64-sweeps", "macos-arm64"]),
        ("smoke.yml", ["linux-x64", "windows-x64", "macos-arm64"]),
        ("bit-identity.yml", ["linux-x64", "windows-x64", "macos-arm64", "compare"]),
        ("bots.yml", ["bots"]),
    ];

    /// <summary>The workflows that run on each head (D-472). None of them reads the skip.</summary>
    private static readonly string[] CheapWorkflows = ["ste-check.yml", "doc-gate.yml", "review-gate.yml", "night-gate.yml", "det-lint.yml", "asset-qa.yml"];

    private const string SkipCondition = "if: ${{ !cancelled() && needs.ci-skip.outputs.skip != 'true' }}";

    private static readonly string[] Documents = ["docs/design.md", "docs/reviews/pr-9.md", "CLAUDE.md", "AGENTS.md", ".claude/skills/ste-writing/SKILL.md", "README.md", "LICENSE"];

    private static readonly DateTimeOffset Noon = DateTimeOffset.Parse("2026-09-22T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture);

    [Fact]
    public void PushToMainNeverSkips()
    {
        CiSkipDecision decision = CiSkipRules.Decide(new CiSkipFacts { EventName = "push", PullRequestPaths = null, PushPaths = null, PreviousRuns = null });

        Assert.False(decision.Skip);
        Assert.Contains("D-473", decision.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void PullRequestOfDocumentsAloneSkips()
    {
        CiSkipDecision decision = CiSkipRules.Decide(PullRequest(Documents, push: null, runs: null));

        Assert.True(decision.Skip);
        Assert.StartsWith("Rule 1", decision.Reason, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("content/audio/sfx/sword-swing.json")]
    [InlineData(".github/workflows/ci.yml")]
    [InlineData(".github/pull_request_template.md")]
    [InlineData("WhatYouCarry.Core/Simulation/Simulation.cs")]
    [InlineData("WhatYouCarry.Tests/CiSkipTests.cs")]
    [InlineData(".claude/settings.json")]
    [InlineData("global.json")]
    [InlineData("Makefile")]
    [InlineData("CLAUDE.md.orig")]
    [InlineData("LICENSE.txt")]
    [InlineData("docs")]
    [InlineData("Docs/design.md")]
    public void PathOutsideTheSetRunsEveryJob(string path)
    {
        Assert.False(CiSkipRules.IsDocument(path));

        CiSkipDecision decision = CiSkipRules.Decide(PullRequest([.. Documents, path], push: null, runs: null));

        Assert.False(decision.Skip);
        Assert.Contains($"'{path}'", decision.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void PushOfDocumentsAfterAPassedRunSkips()
    {
        CiSkipDecision decision = CiSkipRules.Decide(PullRequest(["WhatYouCarry.Core/A.cs", "docs/design.md"], push: ["docs/session-handoff.md"], runs: [Run(7, "completed", "success", 0)]));

        Assert.True(decision.Skip);
        Assert.StartsWith("Rule 2", decision.Reason, StringComparison.Ordinal);
        Assert.Contains("run 7", decision.Reason, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("completed", "failure")]
    [InlineData("completed", "cancelled")]
    [InlineData("completed", "skipped")]
    [InlineData("in_progress", null)]
    [InlineData("queued", null)]
    public void PushAfterARunThatDidNotPassRunsEveryJob(string status, string? conclusion)
    {
        CiSkipDecision decision = CiSkipRules.Decide(PullRequest(["WhatYouCarry.Core/A.cs"], push: ["docs/design.md"], runs: [Run(7, status, conclusion, 0)]));

        Assert.False(decision.Skip);
        Assert.Contains($"'{status}'", decision.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void NewestRunOfThePreviousHeadDecides()
    {
        string[] pullRequest = ["WhatYouCarry.Core/A.cs"];
        string[] push = ["docs/design.md"];

        CiSkipDecision laterFailure = CiSkipRules.Decide(PullRequest(pullRequest, push, [Run(8, "completed", "failure", 60), Run(7, "completed", "success", 0)]));
        CiSkipDecision laterSuccess = CiSkipRules.Decide(PullRequest(pullRequest, push, [Run(7, "completed", "failure", 0), Run(8, "completed", "success", 60)]));
        CiSkipDecision sameTimeHigherId = CiSkipRules.Decide(PullRequest(pullRequest, push, [Run(9, "completed", "cancelled", 0), Run(8, "completed", "success", 0)]));

        Assert.False(laterFailure.Skip);
        Assert.True(laterSuccess.Skip);
        Assert.False(sameTimeHigherId.Skip);
    }

    [Fact]
    public void PushWithNoRunOfThePreviousHeadRunsEveryJob()
    {
        CiSkipDecision decision = CiSkipRules.Decide(PullRequest(["WhatYouCarry.Core/A.cs"], push: ["docs/design.md"], runs: []));

        Assert.False(decision.Skip);
        Assert.Contains("no run", decision.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void PushOfCodeRunsEveryJob()
    {
        CiSkipDecision decision = CiSkipRules.Decide(PullRequest(["WhatYouCarry.Core/A.cs"], push: ["docs/design.md", "content/floors/tiers.json"], runs: [Run(7, "completed", "success", 0)]));

        Assert.False(decision.Skip);
        Assert.Contains("'content/floors/tiers.json'", decision.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void CodePullRequestWithNoPreviousHeadRunsEveryJob()
    {
        CiSkipDecision decision = CiSkipRules.Decide(PullRequest(["WhatYouCarry.Core/A.cs"], push: null, runs: null));

        Assert.False(decision.Skip);
        Assert.Contains("no previous head", decision.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void AbsentFactsAreErrors()
    {
        InvalidOperationException noPaths = Assert.Throws<InvalidOperationException>(() => CiSkipRules.Decide(PullRequest(null, push: null, runs: null)));
        InvalidOperationException noRuns = Assert.Throws<InvalidOperationException>(() => CiSkipRules.Decide(PullRequest(["WhatYouCarry.Core/A.cs"], push: ["docs/design.md"], runs: null)));

        Assert.Contains("paths of the PR", noPaths.Message, StringComparison.Ordinal);
        Assert.Contains("runs of the workflow", noRuns.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GatherReadsThePushFromThePreviousHead()
    {
        using var repo = new TemporaryGitRepository();
        using var runs = new TemporaryRunsFile("{\"id\":7,\"status\":\"completed\",\"conclusion\":\"success\",\"created_at\":\"2026-09-22T12:00:00Z\"}\n");
        string baseCommit = repo.Commit("chore: base", Files(("README.md", "base")));
        repo.CreateBranch("feature");
        string code = repo.Commit("feat: code", Files(("WhatYouCarry.Core/A.cs", "// a"), ("docs/design.md", "design")));
        string handoff = repo.Commit("docs: handoff", Files(("docs/session-handoff.md", "entry")));

        CiSkipFacts facts = CiSkipFacts.Gather(repo.Path, "pull_request", "synchronize", baseCommit, handoff, code, runs.Path);

        Assert.Equal(["WhatYouCarry.Core/A.cs", "docs/design.md", "docs/session-handoff.md"], facts.PullRequestPaths);
        Assert.Equal(["docs/session-handoff.md"], facts.PushPaths);
        WorkflowRun run = Assert.Single(facts.PreviousRuns!);
        Assert.Equal(7, run.Id);
        Assert.True(CiSkipRules.Decide(facts).Skip);
    }

    /// <summary>A code file that moves into the skip set changes a code path too: git lists both sides of the move (PR #89 review).</summary>
    [Fact]
    public void CodeMovedIntoTheSkipSetRunsEveryJob()
    {
        using var repo = new TemporaryGitRepository();
        using var runs = new TemporaryRunsFile("{\"id\":7,\"status\":\"completed\",\"conclusion\":\"success\",\"created_at\":\"2026-09-22T12:00:00Z\"}\n");
        string source = string.Join('\n', Enumerable.Range(1, 40).Select(line => $"// line {line} of the source"));
        string baseCommit = repo.Commit("chore: base", Files(("WhatYouCarry.Core/Moved.cs", source)));
        repo.CreateBranch("feature");
        string previous = repo.Commit("docs: design", Files(("docs/design.md", "design")));
        repo.Git(["mv", "WhatYouCarry.Core/Moved.cs", "docs/Moved.cs"]);
        string head = repo.Commit("docs: move", Files());

        CiSkipFacts facts = CiSkipFacts.Gather(repo.Path, "pull_request", "synchronize", baseCommit, head, previous, runs.Path);

        Assert.Contains("WhatYouCarry.Core/Moved.cs", facts.PullRequestPaths!);
        Assert.Contains("WhatYouCarry.Core/Moved.cs", facts.PushPaths!);
        Assert.False(CiSkipRules.Decide(facts).Skip);
    }

    [Fact]
    public void ForcePushNewPullRequestAndAbsentCommitGiveNoPreviousHead()
    {
        using var repo = new TemporaryGitRepository();
        string baseCommit = repo.Commit("chore: base", Files(("README.md", "base")));
        repo.CreateBranch("replaced");
        string replaced = repo.Commit("feat: replaced", Files(("WhatYouCarry.Core/A.cs", "// old")));
        repo.Git(["checkout", "-q", "main"]);
        repo.CreateBranch("feature");
        string head = repo.Commit("feat: code", Files(("WhatYouCarry.Core/A.cs", "// new")));

        // The runs file is absent, so a read of it would throw. No case below reads it.
        string noRuns = Path.Combine(repo.Path, "absent-runs.jsonl");
        CiSkipFacts forcePush = CiSkipFacts.Gather(repo.Path, "pull_request", "synchronize", baseCommit, head, replaced, noRuns);
        CiSkipFacts opened = CiSkipFacts.Gather(repo.Path, "pull_request", "opened", baseCommit, head, string.Empty, noRuns);
        CiSkipFacts noCommit = CiSkipFacts.Gather(repo.Path, "pull_request", "synchronize", baseCommit, head, CiSkipFacts.NoCommit, noRuns);
        CiSkipFacts unknown = CiSkipFacts.Gather(repo.Path, "pull_request", "synchronize", baseCommit, head, "1234567890123456789012345678901234567890", noRuns);

        foreach (CiSkipFacts facts in new[] { forcePush, opened, noCommit, unknown })
        {
            Assert.Null(facts.PushPaths);
            Assert.False(CiSkipRules.Decide(facts).Skip);
        }
    }

    [Fact]
    public void AbsentRunsFileWithAPreviousHeadFailsAndNamesIt()
    {
        using var repo = new TemporaryGitRepository();
        string baseCommit = repo.Commit("chore: base", Files(("README.md", "base")));
        string previous = repo.Commit("feat: code", Files(("WhatYouCarry.Core/A.cs", "// a")));
        string head = repo.Commit("docs: design", Files(("docs/design.md", "design")));
        string noRuns = Path.Combine(repo.Path, "absent-runs.jsonl");

        FileNotFoundException error = Assert.Throws<FileNotFoundException>(() => CiSkipFacts.Gather(repo.Path, "pull_request", "synchronize", baseCommit, head, previous, noRuns));

        Assert.Contains(noRuns, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseRunsReadsTheApiLines()
    {
        string text = "{\"id\":7,\"status\":\"completed\",\"conclusion\":\"success\",\"created_at\":\"2026-09-22T12:00:00Z\"}\n\n" +
            "{\"id\":8,\"status\":\"in_progress\",\"conclusion\":null,\"created_at\":\"2026-09-22T12:01:00Z\"}\n";

        IReadOnlyList<WorkflowRun> runs = CiSkipFacts.ParseRuns("runs.jsonl", text);

        Assert.Equal([Run(7, "completed", "success", 0), Run(8, "in_progress", null, 60)], runs);
    }

    [Theory]
    [InlineData("{\"id\":7,\"status\":\"completed\",\"created_at\":\"2026-09-22T12:00:00Z\"}", "'conclusion' is absent")]
    [InlineData("{\"id\":\"7\",\"status\":\"completed\",\"conclusion\":null,\"created_at\":\"2026-09-22T12:00:00Z\"}", "'id' is a String")]
    [InlineData("{\"id\":7,\"status\":\"completed\",\"conclusion\":null,\"created_at\":\"noon\"}", "'created_at' holds 'noon'")]
    [InlineData("{\"id\":7,\"status\":\"completed\",\"conclusion\":1,\"created_at\":\"2026-09-22T12:00:00Z\"}", "'conclusion' is a Number")]
    [InlineData("[7]", "a run is an object")]
    [InlineData("{\"id\":7", "not JSON")]
    public void ParseRunsNamesTheLineAndTheFieldOfADefect(string line, string expected)
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => CiSkipFacts.ParseRuns("runs.jsonl", "\n" + line + "\n"));

        Assert.StartsWith("runs.jsonl:2: ", error.Message, StringComparison.Ordinal);
        Assert.Contains(expected, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CommandWritesTheOutputAndRejectsAWrongCall()
    {
        using var repo = new TemporaryGitRepository();
        string baseCommit = repo.Commit("chore: base", Files(("README.md", "base")));
        repo.CreateBranch("feature");
        string head = repo.Commit("docs: design", Files(("docs/design.md", "design")));
        string output = Path.Combine(repo.Path, "github-output.txt");
        string runs = Path.Combine(repo.Path, "runs.jsonl");

        int pullRequest = CiSkipCommand.Run(["--root", repo.Path, "--event", "pull_request", "--action", "opened", "--base", baseCommit, "--head", head, "--before", "", "--runs", runs, "--output", output]);
        int push = CiSkipCommand.Run(["--root", repo.Path, "--event", "push", "--action", "", "--base", "", "--head", "", "--before", baseCommit, "--runs", runs, "--output", output]);
        int noHead = CiSkipCommand.Run(["--root", repo.Path, "--event", "pull_request", "--action", "opened", "--base", baseCommit, "--head", "", "--before", "", "--runs", runs, "--output", output]);
        int missing = CiSkipCommand.Run(["--root", repo.Path, "--event", "push"]);

        Assert.Equal(0, pullRequest);
        Assert.Equal(0, push);
        Assert.Equal(2, noHead);
        Assert.Equal(2, missing);
        Assert.Equal("skip=true\nskip=false\n", File.ReadAllText(output));
    }

    [Fact]
    public void EveryHeavyWorkflowTakesTheSkipJob()
    {
        foreach ((string file, string[] jobs) in HeavyWorkflows)
        {
            string workflow = RepositoryRoot.ReadFile($".github/workflows/{file}");
            string skipJob = WorkflowText.JobText(workflow, "ci-skip");
            Assert.Contains("fetch-depth: 0", skipJob, StringComparison.Ordinal);
            Assert.Contains("actions: read", skipJob, StringComparison.Ordinal);
            Assert.Contains("uses: ./.github/actions/ci-skip", skipJob, StringComparison.Ordinal);
            Assert.Contains($"workflow-file: {file}\n", skipJob, StringComparison.Ordinal);

            foreach (string job in jobs)
            {
                string text = WorkflowText.JobText(workflow, job);
                Assert.True(text.Contains("needs: ci-skip\n", StringComparison.Ordinal) || text.Contains("needs: [ci-skip,", StringComparison.Ordinal), $"The job '{job}' of '{file}' does not need ci-skip.");
                Assert.True(text.Contains(SkipCondition, StringComparison.Ordinal), $"The job '{job}' of '{file}' lacks the skip condition.");
            }

            Assert.Equal(jobs.Length, CountOf(workflow, SkipCondition));
        }
    }

    [Fact]
    public void CheapWorkflowsNeverSkip()
    {
        foreach (string file in CheapWorkflows)
        {
            string workflow = RepositoryRoot.ReadFile($".github/workflows/{file}");
            Assert.DoesNotContain("ci-skip", workflow, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void SkipActionPassesTheEventThroughTheEnvironment()
    {
        string action = RepositoryRoot.ReadFile(".github/actions/ci-skip/action.yml");

        Assert.Contains("using: composite", action, StringComparison.Ordinal);
        Assert.Contains("actions/workflows/$WORKFLOW_FILE/runs?head_sha=$BEFORE_SHA&event=pull_request", action, StringComparison.Ordinal);
        Assert.Contains("-- ci-skip --root \"$GITHUB_WORKSPACE\"", action, StringComparison.Ordinal);
        Assert.Contains("--output \"$GITHUB_OUTPUT\"", action, StringComparison.Ordinal);

        // A value of the event in the text of a run block would run as shell text.
        string[] blocks = action.Split("run: |", StringSplitOptions.None)[1..];
        Assert.Equal(2, blocks.Length);
        foreach (string block in blocks)
        {
            int nextStep = block.IndexOf("\n    - ", StringComparison.Ordinal);
            string script = nextStep < 0 ? block : block[..nextStep];
            Assert.DoesNotContain("${{", script, StringComparison.Ordinal);
        }
    }

    private static CiSkipFacts PullRequest(IReadOnlyList<string>? paths, IReadOnlyList<string>? push, IReadOnlyList<WorkflowRun>? runs)
    {
        return new CiSkipFacts { EventName = "pull_request", PullRequestPaths = paths, PushPaths = push, PreviousRuns = runs };
    }

    private static WorkflowRun Run(long id, string status, string? conclusion, int secondsAfterNoon)
    {
        return new WorkflowRun(id, status, conclusion, Noon.AddSeconds(secondsAfterNoon));
    }

    private static Dictionary<string, string> Files(params (string Path, string Content)[] files)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach ((string path, string content) in files)
        {
            result[path] = content;
        }

        return result;
    }

    private static int CountOf(string text, string value)
    {
        int count = 0;
        int index = text.IndexOf(value, StringComparison.Ordinal);
        while (index >= 0)
        {
            count++;
            index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal);
        }

        return count;
    }

    /// <summary>A runs file under the temp directory. Dispose deletes it.</summary>
    private sealed class TemporaryRunsFile : IDisposable
    {
        public string Path { get; }

        public TemporaryRunsFile(string text)
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "wyc-ci-skip-" + Guid.NewGuid().ToString("N") + ".jsonl");
            File.WriteAllText(Path, text);
        }

        public void Dispose()
        {
            File.Delete(Path);
        }
    }
}
