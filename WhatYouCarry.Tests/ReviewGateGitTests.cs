using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
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
