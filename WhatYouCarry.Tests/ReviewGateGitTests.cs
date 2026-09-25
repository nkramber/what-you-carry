using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WhatYouCarry.Tools.ReviewGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The review gate against real commits: the effective head, the mode source, and the command output.</summary>
[Collection(ConsoleCollection.Name)]
public sealed class ReviewGateGitTests
{
    private const int PullRequestNumber = 7;
    private const string ReviewFile = "docs/reviews/pr-7.md";
    private static readonly DateTimeOffset LabelTime = DateTimeOffset.Parse("2026-09-07T12:00:00Z", CultureInfo.InvariantCulture);

    [Fact]
    public void ReviewGateFailsOnStaleHead()
    {
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string reviewed = repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a")));
        repo.Commit("docs: review", Files((ReviewFile, ReviewFixture.Text(reviewed, "Ready for owner merge"))));
        string later = repo.Commit("feat: second", Files(("WhatYouCarry.Core/B.cs", "// b")));

        ReviewGateResult result = Evaluate(repo, later);

        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains(later, result.Summary, StringComparison.Ordinal);
        Assert.Contains(reviewed, result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateIgnoresMetadataCommit()
    {
        // D-184: a commit that changes only the metadata paths does not change the effective head.
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string reviewed = repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a")));
        string metadata = repo.Commit("docs: review and handoff", Files(
            (ReviewFile, ReviewFixture.Text(reviewed, "Ready for owner merge")),
            ("docs/session-handoff.md", "entry"),
            ("docs/session-handoff-archive.md", "archive")));

        ReviewGateResult result = Evaluate(repo, metadata);

        Assert.Equal(ReviewGateResult.Success, result.Conclusion);
        Assert.Contains(reviewed, result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateKeepsAnApprovalAfterADocumentsCommit()
    {
        // D-534: a commit whose paths all lie in the skip set of D-475 does not move the effective head, so the
        // approval stays green, and no new review is due.
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string reviewed = repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a")));
        repo.Commit("docs: review", Files((ReviewFile, ReviewFixture.Text(reviewed, "Ready for owner merge"))));
        string documents = repo.Commit("docs: the roadmap mark and the agent files", Files(
            ("docs/design.md", "mark"),
            ("docs/decisions.md", "row"),
            (".claude/skills/pr-review/SKILL.md", "skill"),
            ("CLAUDE.md", "agent"),
            ("AGENTS.md", "agent"),
            ("README.md", "readme"),
            ("LICENSE", "license")));

        ReviewGateResult result = Evaluate(repo, documents);

        Assert.Equal(ReviewGateResult.Success, result.Conclusion);
        Assert.Contains($"Effective head: {reviewed}", result.Summary, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(".github/pull_request_template.md")]
    [InlineData("WhatYouCarry.Game/README.md")]
    [InlineData("content/audio/sfx/sword-swing.json")]
    [InlineData("docs.md")]
    public void ReviewGateFailsOnACommitOutsideTheSkipSetAfterTheReview(string path)
    {
        // D-475: a path outside the skip set moves the effective head, also when it is a Markdown file.
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string reviewed = repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a")));
        repo.Commit("docs: review", Files((ReviewFile, ReviewFixture.Text(reviewed, "Ready for owner merge"))));
        string later = repo.Commit("chore: a path outside the skip set", Files((path, "text")));

        ReviewGateResult result = Evaluate(repo, later);

        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains(later, result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateFailsOnCodeMovedIntoTheSkipSet()
    {
        // A move deletes the code path, so the commit changes a path outside the skip set (PR #89 review).
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string reviewed = repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a")));
        repo.Commit("docs: review", Files((ReviewFile, ReviewFixture.Text(reviewed, "Ready for owner merge"))));
        repo.Git(["mv", "WhatYouCarry.Core/A.cs", "docs/A.cs"]);
        string move = repo.Commit("docs: move the code into docs", Files());

        ReviewGateResult result = Evaluate(repo, move);

        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains(move, result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateFailsOnAnApprovalOfADocumentsOnlyPullRequestInGit()
    {
        // D-540: every commit changes documents alone, so the review path has nothing to approve.
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string documents = repo.Commit("docs: a skill", Files((".claude/skills/pr-review/SKILL.md", "skill")));
        string head = repo.Commit("docs: review", Files((ReviewFile, ReviewFixture.Text(documents, "Ready for owner merge"))));

        ReviewGateResult result = Evaluate(repo, head);

        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains(ReviewGateRules.OverrideLabel, result.Summary, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("LICENSE")]
    [InlineData("README.md")]
    public void ReviewGateFailsOnADirectoryWithTheNameOfARootDocument(string path)
    {
        // F-125: the skip set names the root file alone. A change of the file keeps the approval, and a directory of
        // the same name holds code, so a commit into it moves the effective head.
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string reviewed = repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a"), (path, "text")));
        repo.Commit("docs: review", Files((ReviewFile, ReviewFixture.Text(reviewed, "Ready for owner merge"))));
        string document = repo.Commit("docs: the root document", Files((path, "new text")));
        Assert.Equal(ReviewGateResult.Success, Evaluate(repo, document).Conclusion);

        repo.Git(["rm", "-q", path]);
        string directory = repo.Commit("chore: a directory in place of the root document", Files((path + "/Evil.cs", "// code")));
        ReviewGateResult result = Evaluate(repo, directory);

        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains(directory, result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void TheWorkHeadMovesOnADirectoryWithTheNameOfAMetadataFile()
    {
        // F-125: the metadata set names docs/session-handoff.md as a file. A path under a directory of that name is
        // outside the set, and a change of the file alone is inside it.
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string mergeBase = repo.Git(["rev-parse", "HEAD"]).Trim();
        string code = repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a"), ("docs/session-handoff.md", "entry")));
        string handoff = repo.Commit("docs: handoff", Files(("docs/session-handoff.md", "next entry")));
        var git = new GitRepository(repo.Path);
        Assert.Equal(code, git.NewestCommitOutside(mergeBase, handoff, ReviewGateRules.MetadataPaths)?.Sha);

        repo.Git(["rm", "-q", "docs/session-handoff.md"]);
        string directory = repo.Commit("docs: a directory in place of the handoff", Files(("docs/session-handoff.md/entry.md", "entry")));

        Assert.Equal(directory, git.NewestCommitOutside(mergeBase, directory, ReviewGateRules.MetadataPaths)?.Sha);
    }

    [Fact]
    public void TheEffectiveHeadRefusesTwoLinesThatItCannotOrder()
    {
        // F-125: a merge joins a line with code and a line with a directory of the name of a root document. Neither
        // candidate is an ancestor of the other, so the gate stops with both commits and never picks one in silence.
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string mergeBase = repo.Git(["rev-parse", "HEAD"]).Trim();
        repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a")));
        repo.CreateBranch("side");
        string side = repo.Commit("chore: a directory of a root document name", Files(("LICENSE/Evil.cs", "// code")));
        repo.Git(["checkout", "-q", "feature"]);
        string code = repo.Commit("feat: second", Files(("WhatYouCarry.Core/B.cs", "// b")));
        repo.Git(["merge", "-q", "--no-ff", "-m", "merge side", "side"]);
        string head = repo.Git(["rev-parse", "HEAD"]).Trim();
        var git = new GitRepository(repo.Path);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => git.NewestCommitOutside(mergeBase, head, ReviewGateRules.SkipPaths));

        Assert.Contains(side, error.Message, StringComparison.Ordinal);
        Assert.Contains(code, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GitReadsALargeStandardErrorWithoutADeadlock()
    {
        // F-125: git blocks when its stderr pipe is full. A read of stdout to its end before stderr then never
        // ends. Each pathspec that matches no file gives one error line, so 5000 of them fill the pipe many times.
        using var repo = new TemporaryGitRepository();
        repo.Commit("chore: root", Files(("README.md", "root")));
        var names = new List<string>();
        for (int index = 0; index < 5000; index++)
        {
            names.Add($"absent/path-number-{index:D5}.txt");
        }

        string pathspecFile = Path.Combine(repo.Path, "pathspecs.txt");
        File.WriteAllLines(pathspecFile, names);
        var git = new GitRepository(repo.Path);

        Task<string> run = Task.Run(() => Assert.Throws<InvalidOperationException>(() => git.Run(["checkout", $"--pathspec-from-file={pathspecFile}"])).Message);

        Task finished = await Task.WhenAny(run, Task.Delay(TimeSpan.FromSeconds(60)));
        Assert.True(finished == run, "git did not end inside 60 s, so the read of its two streams is in a deadlock.");
        string message = await run;
        Assert.Contains("absent/path-number-00000.txt", message, StringComparison.Ordinal);
        Assert.Contains("absent/path-number-04999.txt", message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateNamesARewriteOfTheReviewFile()
    {
        // D-198: a later commit that rewrites the review file keeps the effective head, and the output names that commit.
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string reviewed = repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a")));
        repo.Commit("docs: review", Files((ReviewFile, ReviewFixture.Text(reviewed, "Changes required"))));
        string rewrite = repo.Commit("docs: rewrite the review", Files((ReviewFile, ReviewFixture.Text(reviewed, "Ready for owner merge"))));

        ReviewGateResult result = Evaluate(repo, rewrite);

        Assert.Equal(ReviewGateResult.Success, result.Conclusion);
        Assert.Contains($"Review file last changed by: {rewrite} \"docs: rewrite the review\"", result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateFailsOnHandoffPlusCodeCommit()
    {
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string reviewed = repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a")));
        string mixed = repo.Commit("docs: review plus a code change", Files(
            (ReviewFile, ReviewFixture.Text(reviewed, "Ready for owner merge")),
            ("docs/session-handoff.md", "entry"),
            ("WhatYouCarry.Core/B.cs", "// b")));

        ReviewGateResult result = Evaluate(repo, mixed);

        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains(mixed, result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateReadsModeFromBase()
    {
        // D-185: a PR that changes the mode file does not change its own mode.
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string head = repo.Commit("chore: enforce", Files((ReviewGateRules.ModeFilePath, "enforced\n")));

        ReviewGateResult result = Evaluate(repo, head);

        Assert.Equal(ReviewGateResult.Neutral, result.Conclusion);
    }

    [Fact]
    public void ReviewGateFailsOnAbsentModeFileAtBase()
    {
        using var repo = new TemporaryGitRepository();
        repo.Commit("chore: root without a mode file", Files(("README.md", "root")));
        repo.CreateBranch("feature");
        string head = repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a")));

        ReviewGateResult result = Evaluate(repo, head);

        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains(ReviewGateRules.ModeFilePath, result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateFailsOnOverrideLabelBeforeNewCommitInGit()
    {
        // D-539: the label reads the work head, so a documents commit after the label needs the label again.
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string head = repo.Commit("docs: after the label", Files(("docs/design.md", "text")), LabelTime.AddMinutes(5));

        ReviewGateResult result = Evaluate(repo, head, overrideLabel: true);

        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains(head, result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGatePassesOnOverrideLabelForMetadataOnlyChange()
    {
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string head = repo.Commit("docs: handoff only", Files(("docs/session-handoff.md", "entry")), LabelTime.AddMinutes(-5));

        ReviewGateResult result = Evaluate(repo, head, overrideLabel: true);

        Assert.Equal(ReviewGateResult.Success, result.Conclusion);
    }

    [Theory]
    [InlineData("README.md")]
    [InlineData("LICENSE")]
    public void ReviewGatePassesOnOverrideLabelForARootDocument(string path)
    {
        // PR #95 review P2-1, D-541: a PR of one root document has no effective head (D-540), so the label covers it.
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string head = repo.Commit("docs: a root document", Files((path, "text")), LabelTime.AddMinutes(-5));

        ReviewGateResult result = Evaluate(repo, head, overrideLabel: true);

        Assert.Equal(ReviewGateResult.Success, result.Conclusion);
    }

    [Fact]
    public void ReviewGateCommandWritesCheckRunPayload()
    {
        using var repo = new TemporaryGitRepository();
        StartBranch(repo);
        string reviewed = repo.Commit("feat: first", Files(("WhatYouCarry.Core/A.cs", "// a")));
        string head = repo.Commit("docs: review", Files((ReviewFile, ReviewFixture.Text(reviewed[..7], "Ready for owner merge"))));
        string inputPath = Path.Combine(repo.Path, "request.json");
        string outputPath = Path.Combine(repo.Path, "check-run.json");
        File.WriteAllText(inputPath, JsonSerializer.Serialize(Request(repo, head, overrideLabel: false)));

        int exitCode = ReviewGateCommand.Run(["--input", inputPath, "--output", outputPath]);

        Assert.Equal(0, exitCode);
        using JsonDocument payload = JsonDocument.Parse(File.ReadAllText(outputPath));
        JsonElement root = payload.RootElement;
        Assert.Equal("review-gate", root.GetProperty("name").GetString());
        Assert.Equal(head, root.GetProperty("head_sha").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal("success", root.GetProperty("conclusion").GetString());
        Assert.Contains(reviewed, root.GetProperty("output").GetProperty("summary").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateCommandRejectsARequestWithAMissingField()
    {
        // T-2: an absent field is an error, never a default.
        using var repo = new TemporaryGitRepository();
        string inputPath = Path.Combine(repo.Path, "request.json");
        File.WriteAllText(inputPath, """{"repositoryPath": ".", "pullRequestNumber": 7}""");

        Assert.Throws<JsonException>(() => ReviewGateCommand.Evaluate(inputPath));
    }

    /// <summary>A root commit on main with the advisory mode file, then a feature branch.</summary>
    private static void StartBranch(TemporaryGitRepository repo)
    {
        repo.Commit("chore: root", Files((ReviewGateRules.ModeFilePath, "advisory\n"), ("README.md", "root")));
        repo.CreateBranch("feature");
    }

    private static ReviewGateResult Evaluate(TemporaryGitRepository repo, string head, bool overrideLabel = false)
    {
        ReviewGateFacts facts = ReviewGateFacts.Gather(Request(repo, head, overrideLabel));
        return ReviewGateRules.Evaluate(facts);
    }

    private static ReviewGateRequest Request(TemporaryGitRepository repo, string head, bool overrideLabel)
    {
        return new ReviewGateRequest
        {
            RepositoryPath = repo.Path,
            PullRequestNumber = PullRequestNumber,
            HeadSha = head,
            BaseRef = "main",
            Labels = overrideLabel ? ["review-override"] : [],
            OverrideLabelEvents = overrideLabel
                ? [new LabelEvent { CreatedAt = LabelTime.ToString("O"), Actor = "owner-login" }]
                : [],
        };
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
