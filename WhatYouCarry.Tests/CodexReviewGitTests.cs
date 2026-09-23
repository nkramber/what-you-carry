using System;
using System.Collections.Generic;
using WhatYouCarry.Tools.CodexReview;
using WhatYouCarry.Tools.ReviewGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The judge of one review round against real commits (D-511). The origin refs stand in for a fetch: the test
/// writes them with update-ref.
/// </summary>
public sealed class CodexReviewGitTests
{
    private const int PullRequestNumber = 93;
    private const string Branch = "feat/pr-78-codex-review";
    private const string ReviewFile = "docs/reviews/pr-93.md";
    private static readonly PullRequestView View = new("OPEN", Branch, "unused", "main");

    [Fact]
    public void ARoundThatPushesAnApprovingRecordApproves()
    {
        using var repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        string record = repo.Commit("docs: review", Files((ReviewFile, Record(reviewed)), ("docs/session-handoff.md", "entry")));
        SetOrigin(repo, Branch, record);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Path), PullRequestNumber, View, reviewed, reviewed);

        Assert.Equal(CodexReviewExit.Approve, outcome.Exit);
    }

    [Fact]
    public void ARoundThatPushesNoCommitIsAFault()
    {
        using var repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        SetOrigin(repo, Branch, reviewed);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Path), PullRequestNumber, View, reviewed, reviewed);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains("pushed no commit", outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACodeCommitDuringTheRoundIsAFault()
    {
        // D-182, D-184: the reviewer pushes a metadata commit alone, so the work head must not move.
        using var repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        repo.Commit("docs: review", Files((ReviewFile, Record(reviewed))));
        string code = repo.Commit("fix: a change", Files(("WhatYouCarry.Core/B.cs", "// b")));
        SetOrigin(repo, Branch, code);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Path), PullRequestNumber, View, reviewed, reviewed);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains(code, outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADocumentsCommitDuringTheRoundIsAFault()
    {
        // D-182, D-534: a documents commit keeps the effective head, and it moves the work head. The reviewer pushes
        // a metadata commit alone, so the round fails.
        using var repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        repo.Commit("docs: review", Files((ReviewFile, Record(reviewed))));
        string documents = repo.Commit("docs: a design change", Files(("docs/design.md", "text")));
        SetOrigin(repo, Branch, documents);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Path), PullRequestNumber, View, reviewed, reviewed);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains(documents, outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordOfTheEffectiveHeadApprovesAfterADocumentsCommit()
    {
        // D-534: the round starts after a documents commit. The record names the code commit, and that commit is the
        // effective head.
        using var repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        string documents = repo.Commit("docs: the roadmap mark", Files(("docs/roadmaps/phase-2.md", "mark")));
        string record = repo.Commit("docs: review", Files((ReviewFile, Record(reviewed)), ("docs/session-handoff.md", "entry")));
        SetOrigin(repo, Branch, record);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Path), PullRequestNumber, View, documents, documents);

        Assert.Equal(CodexReviewExit.Approve, outcome.Exit);
    }

    [Fact]
    public void TheEffectiveHeadSkipsTheMetadataCommits()
    {
        using var repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        string metadata = repo.Commit("docs: handoff", Files(("docs/session-handoff.md", "entry"), ("docs/reviews/pr-93-response.md", "answer")));

        Assert.Equal(reviewed, CodexReviewCommand.EffectiveHead(new GitRepository(repo.Path), View, metadata));
        Assert.Equal(reviewed, CodexReviewCommand.WorkHead(new GitRepository(repo.Path), View, metadata));
    }

    [Fact]
    public void TheEffectiveHeadSkipsTheDocumentsCommitsAndTheWorkHeadDoesNot()
    {
        // D-534: the review follows the skip set of D-475, and the Gitar pass follows the metadata set of D-184.
        using var repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        string documents = repo.Commit("docs: a skill and the agent files", Files((".claude/skills/pr-review/SKILL.md", "skill"), ("AGENTS.md", "agent")));
        var git = new GitRepository(repo.Path);

        Assert.Equal(reviewed, CodexReviewCommand.EffectiveHead(git, View, documents));
        Assert.Equal(documents, CodexReviewCommand.WorkHead(git, View, documents));
    }

    [Fact]
    public void ADocumentsOnlyPullRequestHasNoEffectiveHead()
    {
        // D-540: the start checks refuse such a PR and name the label.
        using var repo = new TemporaryGitRepository();
        string root = repo.Commit("chore: root", Files(("README.md", "root")));
        SetOrigin(repo, "main", root);
        repo.CreateBranch(Branch);
        string documents = repo.Commit("docs: a design change", Files(("docs/design.md", "text")));
        var git = new GitRepository(repo.Path);

        Assert.Null(CodexReviewCommand.EffectiveHead(git, View, documents));
        Assert.Equal(documents, CodexReviewCommand.WorkHead(git, View, documents));
    }

    /// <summary>A root commit on main, then one code commit on the PR branch. Returns the code commit.</summary>
    private static string StartBranch(TemporaryGitRepository repo)
    {
        string root = repo.Commit("chore: root", Files(("README.md", "root")));
        SetOrigin(repo, "main", root);
        repo.CreateBranch(Branch);
        return repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a")));
    }

    private static void SetOrigin(TemporaryGitRepository repo, string branch, string sha)
    {
        repo.Git(["update-ref", $"refs/remotes/origin/{branch}", sha]);
    }

    private static string Record(string head)
    {
        return $"# PR-93 review\n\n## Identity\n\n- Head: `{head}`\n\n## Findings\n\nNo finding.\n\n## Verdict\n\n**Ready for owner merge.** This verdict applies to head `{head}`.\n";
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
}
