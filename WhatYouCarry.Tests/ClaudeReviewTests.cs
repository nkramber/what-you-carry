using System;
using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.CodexReview;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The rules of <c>claude-review</c> (D-649): the Claude Code arguments, the removed credential variables, the last
/// message from the result event, the CLI checks, the options, the provider gate of both commands, and the make target.
/// The class sets process environment variables and calls the program, so it runs in the console collection.
/// </summary>
[Collection(ConsoleCollection.Name)]
public sealed class ClaudeReviewTests
{
    private const string Branch = "feat/pr-94-codex-work";
    private const string TranscriptPath = "/tmp/wyc-claude-review/transcript.jsonl";
    private const string GoodStatus = "{\"loggedIn\": true, \"authMethod\": \"claude.ai\", \"apiProvider\": \"firstParty\", \"email\": \"owner@example.com\"}";

    [Fact]
    public void ReviewRunIsThePrintRunWithTheModelThePermissionModeAndTheEventStream()
    {
        IReadOnlyList<string> args = new ClaudeReviewer().ReviewArguments("/tmp/worktree", "/tmp/last.md", 94, Branch);

        Assert.Equal(
            ["-p", ClaudeReviewSettings.ReviewPrompt(94, Branch), "--model", "claude-opus-5-5", "--permission-mode", "bypassPermissions", "--output-format", "stream-json", "--verbose"],
            args);

        // The CLI has no option for the directory or the last message, so neither path is an argument.
        Assert.DoesNotContain("/tmp/worktree", args);
        Assert.DoesNotContain("/tmp/last.md", args);
    }

    [Fact]
    public void ProbeIsOneJsonPrintRunWithTheSameModel()
    {
        Assert.Equal(
            ["-p", "Reply with the single word OK.", "--model", "claude-opus-5-5", "--output-format", "json"],
            new ClaudeReviewer().ProbeArguments("/tmp/probe"));
    }

    [Fact]
    public void ReviewPromptIsTheCodexPromptWithTheNameOfThisCommand()
    {
        string prompt = ClaudeReviewSettings.ReviewPrompt(94, Branch);

        Assert.Equal(CodexReviewSettings.ReviewPrompt(94, Branch).Replace("make codex-review", "make claude-review", StringComparison.Ordinal), prompt);
        Assert.Contains("The command `make claude-review` started this review", prompt, StringComparison.Ordinal);
        Assert.Contains($"`git push origin HEAD:{Branch}`", prompt, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryCredentialVariableAndProviderSwitchLeavesTheClaudeEnvironment()
    {
        // D-649, as D-523 for Codex: two API credentials, the subscription token that bypasses the checked login, and
        // the three switches to a cloud platform with its own pricing.
        Assert.Equal(
            ["ANTHROPIC_API_KEY", "ANTHROPIC_AUTH_TOKEN", "CLAUDE_CODE_OAUTH_TOKEN", "CLAUDE_CODE_USE_BEDROCK", "CLAUDE_CODE_USE_VERTEX", "CLAUDE_CODE_USE_FOUNDRY"],
            new ClaudeReviewer().CredentialVariables);
    }

    [Fact]
    public void TheClaudeChildLosesEachCredentialVariableAndTheParentKeepsIt()
    {
        IReadOnlyList<string> variables = new ClaudeReviewer().CredentialVariables;
        var saved = new Dictionary<string, string?>();
        foreach (string variable in variables)
        {
            saved[variable] = Environment.GetEnvironmentVariable(variable);
            Environment.SetEnvironmentVariable(variable, "fake-value");
        }

        try
        {
            string directory = Path.GetTempPath();
            ProcessResult result = OperatingSystem.IsWindows()
                ? ExternalProcess.Run("cmd.exe", ["/c", "set"], directory, variables)
                : ExternalProcess.Run("env", [], directory, variables);
            string childEnvironment = result.RequireSuccess();

            foreach (string variable in variables)
            {
                Assert.DoesNotContain(variable + "=", childEnvironment, StringComparison.Ordinal);
                Assert.Equal("fake-value", Environment.GetEnvironmentVariable(variable));
            }
        }
        finally
        {
            foreach (string variable in variables)
            {
                Environment.SetEnvironmentVariable(variable, saved[variable]);
            }
        }
    }

    [Fact]
    public void TheLastMessageIsTheTextOfTheResultEvent()
    {
        string transcript =
            "{\"type\":\"system\",\"subtype\":\"init\",\"model\":\"claude-opus-5-5\"}\n"
            + "{\"type\":\"assistant\",\"message\":{\"content\":[{\"type\":\"text\",\"text\":\"Working.\"}]}}\n"
            + "\n"
            + "{\"type\":\"result\",\"subtype\":\"success\",\"is_error\":false,\"result\":\"Verdict: Ready for owner merge.\"}\n";

        Assert.Equal("Verdict: Ready for owner merge.", ClaudeReviewSettings.LastMessage(transcript, TranscriptPath));
    }

    [Fact]
    public void AStreamWithNoResultEventIsAFault()
    {
        string transcript = "{\"type\":\"system\",\"subtype\":\"init\"}\n{\"type\":\"assistant\",\"message\":{}}\n";

        FormatException error = Assert.Throws<FormatException>(() => ClaudeReviewSettings.LastMessage(transcript, TranscriptPath));

        Assert.Contains("no event of the type 'result'", error.Message, StringComparison.Ordinal);
        Assert.Contains(TranscriptPath, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("{\"type\":\"result\",\"subtype\":\"error_max_turns\",\"is_error\":true}\n", "on line 1 of the transcript")]
    [InlineData("{\"type\":\"system\"}\nnot json\n", "Line 2 of the transcript")]
    public void AResultWithNoTextOrALineThatIsNotJsonIsAFault(string transcript, string expected)
    {
        FormatException error = Assert.Throws<FormatException>(() => ClaudeReviewSettings.LastMessage(transcript, TranscriptPath));

        Assert.Contains(expected, error.Message, StringComparison.Ordinal);
        Assert.Contains(TranscriptPath, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheReviewerSavesTheLastMessageFromTheTranscript()
    {
        string directory = Path.Combine(Path.GetTempPath(), "wyc-claude-review-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string transcript = Path.Combine(directory, "transcript.jsonl");
            string lastMessage = Path.Combine(directory, "last-message.md");
            File.WriteAllText(transcript, "{\"type\":\"result\",\"subtype\":\"success\",\"result\":\"Done.\"}\n");

            new ClaudeReviewer().SaveLastMessage(transcript, lastMessage);

            Assert.Equal("Done.", File.ReadAllText(lastMessage));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void TheVersionReadsTheNumbersBeforeTheName()
    {
        Assert.Equal(new Version(2, 1, 283), ClaudeReviewSettings.ParseVersion("2.1.283 (Claude Code)\n"));
        Assert.Equal("2.1.283", new ClaudeReviewer().VersionText("2.1.283 (Claude Code)"));
    }

    [Theory]
    [InlineData("codex-cli 0.156.1")]
    [InlineData("2.1 (Claude Code)")]
    [InlineData("2.1.x (Claude Code)")]
    [InlineData("2.1.283.4 (Claude Code)")]
    public void TheVersionRefusesAnotherForm(string output)
    {
        FormatException error = Assert.Throws<FormatException>(() => ClaudeReviewSettings.ParseVersion(output));

        Assert.Contains("version", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheClaudeCliChecksPassForTheMinimumAndAnAccountLogin()
    {
        Assert.Empty(new ClaudeReviewer().CliProblems("2.1.283 (Claude Code)", LoginResult(0, GoodStatus)));
        Assert.Empty(new ClaudeReviewer().CliProblems("2.2.0 (Claude Code)", LoginResult(0, GoodStatus)));
    }

    public static TheoryData<string, int, string, string> ClaudeCliProblems()
    {
        return new TheoryData<string, int, string, string>
        {
            { "2.1.282 (Claude Code)", 0, GoodStatus, "minimum is 2.1.283" },
            { "2.1.283 (Claude Code)", 0, "{\"loggedIn\": true, \"authMethod\": \"oauth_token\", \"apiProvider\": \"firstParty\"}", "authMethod 'oauth_token'" },
            { "2.1.283 (Claude Code)", 0, "{\"loggedIn\": true, \"authMethod\": \"third_party\", \"apiProvider\": \"bedrock\"}", "apiProvider 'bedrock'" },
            { "2.1.283 (Claude Code)", 0, "{\"loggedIn\": false}", "loggedIn false" },
            { "2.1.283 (Claude Code)", 0, "Not logged in", "not JSON" },
            { "2.1.283 (Claude Code)", 1, string.Empty, "exited 1" },
        };
    }

    [Theory]
    [MemberData(nameof(ClaudeCliProblems))]
    public void TheClaudeCliChecksRefuseAnOldCliOrAnotherLogin(string versionOutput, int loginExit, string loginOutput, string expected)
    {
        IReadOnlyList<string> problems = new ClaudeReviewer().CliProblems(versionOutput, LoginResult(loginExit, loginOutput));

        string problem = Assert.Single(problems);
        Assert.Contains(expected, problem, StringComparison.Ordinal);
        Assert.Contains("D-649", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLoginProblemDoesNotRepeatTheEmail()
    {
        string? problem = ClaudeReviewSettings.LoginProblem("{\"loggedIn\": true, \"authMethod\": \"oauth_token\", \"apiProvider\": \"firstParty\", \"email\": \"owner@example.com\"}");

        Assert.NotNull(problem);
        Assert.DoesNotContain("owner@example.com", problem, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, "{\"type\":\"result\",\"subtype\":\"success\",\"is_error\":false,\"result\":\"OK\"}", true)]
    [InlineData(0, "{\"type\":\"result\",\"result\":\" ok.\\n\"}", true)]
    [InlineData(0, "{\"type\":\"result\",\"result\":\"Hello\"}", false)]
    [InlineData(0, "{\"type\":\"result\",\"is_error\":true}", false)]
    [InlineData(0, "OK", false)]
    [InlineData(1, "{\"type\":\"result\",\"result\":\"OK\"}", false)]
    public void TheProbeNeedsTheAnswerOk(int exitCode, string output, bool answers)
    {
        string? problem = new ClaudeReviewer().ProbeProblem(new ProcessResult("claude -p", "/tmp/probe", exitCode, output, "the probe stderr"));

        if (answers)
        {
            Assert.Null(problem);
            return;
        }

        Assert.NotNull(problem);
        Assert.Contains("claude-opus-5-5", problem, StringComparison.Ordinal);
        Assert.Contains($"exit {exitCode}", problem, StringComparison.Ordinal);
        Assert.Contains("the probe stderr", problem, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(new[] { "--root", ".", "--pr", "96", "--claude", "claude" }, false)]
    [InlineData(new[] { "--skip-gitar-review", "--root", ".", "--pr", "96", "--claude", "claude" }, true)]
    public void TheClaudeOptionsReadTheClaudePath(string[] args, bool skip)
    {
        CodexReviewOptions? options = CodexReviewCommand.ParseOptions(args, new ClaudeReviewer(), out string problem);

        Assert.Equal(string.Empty, problem);
        Assert.Equal(new CodexReviewOptions(".", 96, "claude", skip), options);
    }

    [Theory]
    [InlineData(new[] { "--root", ".", "--pr", "96", "--codex", "codex" }, "Unknown option '--codex'.")]
    [InlineData(new[] { "--root", ".", "--pr", "96" }, "Each of --root, --pr, and --claude needs a value")]
    [InlineData(new[] { "--root", ".", "--pr", "9x", "--claude", "claude" }, "Found --pr '9x'.")]
    [InlineData(new[] { "--root", ".", "--pr", "96", "--claude" }, "The option '--claude' needs a value.")]
    public void TheClaudeOptionsRefuseTheCodexOptionAndAnIncompleteForm(string[] args, string expected)
    {
        CodexReviewOptions? options = CodexReviewCommand.ParseOptions(args, new ClaudeReviewer(), out string problem);

        Assert.Null(options);
        Assert.Contains(expected, problem, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCodexOptionsRefuseTheClaudeOption()
    {
        CodexReviewOptions? options = CodexReviewCommand.ParseOptions(["--root", ".", "--pr", "96", "--claude", "claude"], new CodexReviewer(), out string problem);

        Assert.Null(options);
        Assert.Contains("Unknown option '--claude'.", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void TheProgramRunsClaudeReviewWithItsUsage()
    {
        TextWriter savedError = Console.Error;
        try
        {
            var errors = new StringWriter();
            Console.SetError(errors);

            int exitCode = Program.Main(["claude-review", "--root", ".", "--pr", "96", "--codex", "codex"]);

            Assert.Equal(2, exitCode);
            Assert.Contains("Unknown option '--codex'.", errors.ToString(), StringComparison.Ordinal);
            Assert.Contains("--claude <path>", errors.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(savedError);
        }
    }

    [Fact]
    public void EachReviewerNamesTheOtherProviderAsTheAuthor()
    {
        // T-4: the provider that wrote the PR does not review it.
        Assert.Equal("Claude Code", new CodexReviewer().PullRequestAuthor);
        Assert.Equal("Codex", new ClaudeReviewer().PullRequestAuthor);
        Assert.Equal("Codex", new CodexReviewer().ProviderName);
        Assert.Equal("Claude Code", new ClaudeReviewer().ProviderName);
    }

    [Fact]
    public void TheProviderGateReadsTheAuthorEntriesAndSkipsTheReviewerEntries()
    {
        // The newest entry of the branch is a reviewer entry of Claude Code, and the author entries name Codex.
        string handoff = Handoff(
            Entry(12, "Claude Code", $"PR-94, reviewer. Branch `{Branch}`."),
            Entry(11, "Codex", $"PR-94, author, merge. Branch `{Branch}`. PR #120."),
            Entry(10, "Claude Code", "PR-93, author. Branch `feat/pr-93-other`."),
            Entry(9, "Codex", $"PR-94, author. Branch `{Branch}`."));

        Assert.Equal(
            [new AuthorEntry("## Session 11: 2026-09-27, Codex", "Codex"), new AuthorEntry("## Session 9: 2026-09-27, Codex", "Codex")],
            ProviderGate.AuthorEntries(handoff, Branch));
        Assert.Null(ProviderGate.Problem(handoff, EmptyArchive, Branch, new ClaudeReviewer().PullRequestAuthor));

        string? codexProblem = ProviderGate.Problem(handoff, EmptyArchive, Branch, new CodexReviewer().PullRequestAuthor);
        Assert.NotNull(codexProblem);
        Assert.Contains("`Author: Codex`", codexProblem, StringComparison.Ordinal);
        Assert.Contains("needs `Author: Claude Code`", codexProblem, StringComparison.Ordinal);
    }

    /// <summary>The archive of a checkout with no rotated entry.</summary>
    private const string EmptyArchive = "# Session handoff archive\n";

    /// <summary>
    /// PR #116 automated pass. The rotation moves each entry after the tenth to the archive (D-379), so the first author
    /// entry of a long PR can live there alone. The gate reads it, and a PR that both providers wrote stays refused.
    /// </summary>
    [Fact]
    public void TheProviderGateReadsTheAuthorEntriesOfTheArchive()
    {
        string handoff = Handoff(Entry(21, "Codex", $"PR-94, author. Branch `{Branch}`."));
        string archive = Handoff(Entry(9, "Claude Code", $"PR-94, author. Branch `{Branch}`."));
        string? mixed = ProviderGate.Problem(handoff, archive, Branch, "Codex");
        Assert.NotNull(mixed);
        Assert.Contains("## Session 9", mixed, StringComparison.Ordinal);

        string onlyArchived = Handoff(Entry(21, "Claude Code", $"PR-94, reviewer. Branch `{Branch}`."));
        string codexArchive = Handoff(Entry(9, "Codex", $"PR-94, author. Branch `{Branch}`."));
        Assert.Null(ProviderGate.Problem(onlyArchived, codexArchive, Branch, "Codex"));

        string? noArchive = ProviderGate.Problem(handoff, null, Branch, "Codex");
        Assert.NotNull(noArchive);
        Assert.Contains(ProviderGate.ArchivePath, noArchive, StringComparison.Ordinal);
    }

    [Fact]
    public void TheProviderGateRefusesAPullRequestThatBothProvidersWrote()
    {
        string handoff = Handoff(
            Entry(11, "Codex", $"PR-94, author. Branch `{Branch}`."),
            Entry(10, "Claude Code", $"PR-94, author. Branch `{Branch}`."));

        string? problem = ProviderGate.Problem(handoff, EmptyArchive, Branch, "Codex");

        Assert.NotNull(problem);
        Assert.Contains("## Session 10", problem, StringComparison.Ordinal);
        Assert.Contains("`Author: Claude Code`", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void TheProviderGateRefusesABranchWithNoAuthorEntry()
    {
        string handoff = Handoff(
            Entry(11, "Claude Code", $"PR-94, reviewer. Branch `{Branch}`."),
            Entry(10, "Codex", "PR-93, author. Branch `feat/pr-93-other`."));

        string? problem = ProviderGate.Problem(handoff, EmptyArchive, Branch, "Codex");

        Assert.NotNull(problem);
        Assert.Contains($"No author entry of '{ProviderGate.HandoffPath}' or '{ProviderGate.ArchivePath}' names Branch `{Branch}`", problem, StringComparison.Ordinal);
        Assert.Contains("`Author:` field", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void TheProviderGateRefusesAnAuthorEntryWithNoAuthorFieldOrNoHandoff()
    {
        string handoff = "# Session handoff\n\n## Session 11: 2026-09-27, Codex\n\nSession: PR-94, author. Branch `" + Branch + "`.\n";

        string? noField = ProviderGate.Problem(handoff, EmptyArchive, Branch, "Codex");
        string? noHandoff = ProviderGate.Problem(null, EmptyArchive, Branch, "Codex");

        Assert.NotNull(noField);
        Assert.Contains("has no `Author:` field", noField, StringComparison.Ordinal);
        Assert.NotNull(noHandoff);
        Assert.Contains(ProviderGate.HandoffPath, noHandoff, StringComparison.Ordinal);
    }

    [Fact]
    public void TheMakeTargetRunsClaudeReviewWithTheClaudePath()
    {
        // D-649: the target stays thin, and the owner keeps the CLI updated, so it installs nothing.
        string makefile = RepositoryRoot.ReadFile("Makefile");
        int start = makefile.IndexOf("\nclaude-review: ## ", StringComparison.Ordinal);
        Assert.True(start > 0, "The Makefile has no claude-review target with a help line.");
        int end = makefile.IndexOf("\n\n", start + 1, StringComparison.Ordinal);
        string target = end < 0 ? makefile[start..] : makefile[start..end];

        Assert.Contains("make claude-review PR=93", target, StringComparison.Ordinal);
        Assert.Contains("(D-649)", target, StringComparison.Ordinal);
        Assert.Contains("\t$(TOOLS) claude-review --root . --pr $(PR) --claude $(CLAUDE) $(CODEX_REVIEW_FLAGS)", target, StringComparison.Ordinal);
        Assert.DoesNotContain("npm install", target, StringComparison.Ordinal);
        Assert.Contains("\nCLAUDE ?= $(shell command -v claude)\n", makefile, StringComparison.Ordinal);
        Assert.Matches(@"\n\.PHONY:[^\n]* claude-review(\n| )", makefile);
    }

    private static ProcessResult LoginResult(int exitCode, string standardOutput)
    {
        return new ProcessResult("claude auth status --json", "/tmp", exitCode, standardOutput, exitCode == 0 ? string.Empty : "not logged in");
    }

    private static string Handoff(params string[] entries)
    {
        return "# Session handoff\n\n" + string.Join(string.Empty, entries);
    }

    /// <summary>One handoff entry in the form of the handoff: the heading, the author field, and the session line.</summary>
    private static string Entry(int number, string author, string session)
    {
        return $"## Session {number}: 2026-09-27, {author}\n\nAuthor: {author}\nSession: {session}\n\n### What this session did, and why\n\n- Work.\n\n";
    }
}
