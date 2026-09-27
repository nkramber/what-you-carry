using System;
using System.Collections.Generic;
using System.IO;

namespace WhatYouCarry.Tools.CodexReview;

/// <summary>
/// The reviewer CLI of one review command (D-649). <c>codex-review</c> runs the Codex CLI for a PR that Claude Code
/// writes, and <c>claude-review</c> runs the Claude Code CLI for a PR that Codex writes (T-4). The two commands share
/// the start checks of the PR, the detached worktree, and the verdict judge. Each member here is a fact or a rule of
/// one CLI.
/// </summary>
public interface IReviewer
{
    /// <summary>The name of the command and of the make target, such as <c>codex-review</c>.</summary>
    string CommandName { get; }

    /// <summary>The option that names the CLI binary, such as <c>--codex</c>.</summary>
    string CliOption { get; }

    /// <summary>The short name of the CLI in file names, such as <c>codex</c>.</summary>
    string CliName { get; }

    /// <summary>The provider of the reviewer, as the <c>Author:</c> field of a handoff entry writes it.</summary>
    string ProviderName { get; }

    /// <summary>The provider that must write the PR: the other provider (T-4).</summary>
    string PullRequestAuthor { get; }

    /// <summary>The variables that the command removes from the environment of each process of the CLI (D-523, D-649).</summary>
    IReadOnlyList<string> CredentialVariables { get; }

    /// <summary>The model, and the effort when the CLI takes one, for the start line.</summary>
    string ModelText { get; }

    IReadOnlyList<string> LoginStatusArguments { get; }

    /// <summary>The refusal when the CLI does not start. <paramref name="startError"/> is the error of the start.</summary>
    string MissingCliProblem(string startError);

    /// <summary>The version in the output of the version call.</summary>
    /// <exception cref="FormatException">The output has another form. The message holds the text (T-2).</exception>
    string VersionText(string versionOutput);

    /// <summary>The problems of the CLI version and of the login status (D-512, D-523, D-649).</summary>
    /// <exception cref="FormatException">The version output has another form. The message holds the text (T-2).</exception>
    IReadOnlyList<string> CliProblems(string versionOutput, ProcessResult loginStatus);

    /// <summary>The arguments of the model probe in <paramref name="directory"/>, a directory outside a repository.</summary>
    IReadOnlyList<string> ProbeArguments(string directory);

    /// <summary>The problem of the probe, or null when the model answers.</summary>
    string? ProbeProblem(ProcessResult probe);

    /// <summary>The arguments of the review run. The process starts in <paramref name="worktree"/>.</summary>
    IReadOnlyList<string> ReviewArguments(string worktree, string lastMessagePath, int pullRequest, string branch);

    /// <summary>Makes sure that <paramref name="lastMessagePath"/> holds the last message of the review run.</summary>
    /// <exception cref="FormatException">The transcript gives no last message. The message names the transcript (T-2).</exception>
    void SaveLastMessage(string transcriptPath, string lastMessagePath);
}

/// <summary>The Codex CLI as the reviewer of a PR that Claude Code writes (D-511, D-512, D-523).</summary>
public sealed class CodexReviewer : IReviewer
{
    public string CommandName => "codex-review";

    public string CliOption => "--codex";

    public string CliName => "codex";

    public string ProviderName => "Codex";

    public string PullRequestAuthor => "Claude Code";

    public IReadOnlyList<string> CredentialVariables => CodexReviewSettings.ApiCredentialVariables;

    public string ModelText => $"model {CodexReviewSettings.Model} at effort {CodexReviewSettings.ReasoningEffort}";

    public IReadOnlyList<string> LoginStatusArguments => CodexReviewSettings.LoginStatusArguments;

    public string MissingCliProblem(string startError)
    {
        return $"The Codex CLI is missing. {startError}. Run `npm install -g @openai/codex@latest` (D-512).";
    }

    public string VersionText(string versionOutput)
    {
        return CodexVersion.Parse(versionOutput).ToString();
    }

    public IReadOnlyList<string> CliProblems(string versionOutput, ProcessResult loginStatus)
    {
        var problems = new List<string>();
        CodexVersion version = CodexVersion.Parse(versionOutput);
        if (!version.IsAtLeast(CodexReviewSettings.MinimumVersion))
        {
            problems.Add($"The Codex CLI is {version}, and the minimum is {CodexReviewSettings.MinimumVersion} (D-512). Run `npm install -g @openai/codex@latest`.");
        }

        string status = CodexReviewSettings.LoginStatusText(loginStatus);
        if (!status.StartsWith(CodexReviewSettings.ChatGptLoginStatus, StringComparison.Ordinal))
        {
            problems.Add($"`codex login status` gives '{status}', and a review needs '{CodexReviewSettings.ChatGptLoginStatus}', so it never uses API pricing (D-523). Run `codex login` and choose ChatGPT.");
        }

        return problems;
    }

    public IReadOnlyList<string> ProbeArguments(string directory)
    {
        return CodexReviewSettings.ProbeArguments(directory);
    }

    public string? ProbeProblem(ProcessResult probe)
    {
        if (probe.ExitCode == 0 && probe.StandardOutput.Contains("agent_message", StringComparison.Ordinal))
        {
            return null;
        }

        return $"The model probe of '{CodexReviewSettings.Model}' failed with exit {probe.ExitCode} (D-512). stdout: {probe.StandardOutput.Trim()} stderr: {probe.StandardError.Trim()}";
    }

    public IReadOnlyList<string> ReviewArguments(string worktree, string lastMessagePath, int pullRequest, string branch)
    {
        return CodexReviewSettings.ReviewArguments(worktree, lastMessagePath, CodexReviewSettings.ReviewPrompt(pullRequest, branch));
    }

    public void SaveLastMessage(string transcriptPath, string lastMessagePath)
    {
        // The review run writes the last message itself, through the option -o.
    }
}

/// <summary>The Claude Code CLI as the reviewer of a PR that Codex writes (D-649).</summary>
public sealed class ClaudeReviewer : IReviewer
{
    public string CommandName => "claude-review";

    public string CliOption => "--claude";

    public string CliName => "claude";

    public string ProviderName => "Claude Code";

    public string PullRequestAuthor => "Codex";

    public IReadOnlyList<string> CredentialVariables => ClaudeReviewSettings.ApiCredentialVariables;

    public string ModelText => $"model {ClaudeReviewSettings.Model}";

    public IReadOnlyList<string> LoginStatusArguments => ClaudeReviewSettings.LoginStatusArguments;

    public string MissingCliProblem(string startError)
    {
        return $"The Claude Code CLI is missing. {startError}. Install it, or name it with `make claude-review PR=<n> CLAUDE=<path>` (D-649).";
    }

    public string VersionText(string versionOutput)
    {
        return ClaudeReviewSettings.ParseVersion(versionOutput).ToString();
    }

    public IReadOnlyList<string> CliProblems(string versionOutput, ProcessResult loginStatus)
    {
        var problems = new List<string>();
        Version version = ClaudeReviewSettings.ParseVersion(versionOutput);
        if (version < ClaudeReviewSettings.MinimumVersion)
        {
            problems.Add($"The Claude Code CLI is {version}, and the minimum is {ClaudeReviewSettings.MinimumVersion} (D-649). Update the CLI.");
        }

        if (loginStatus.ExitCode != 0)
        {
            problems.Add($"`claude auth status --json` exited {loginStatus.ExitCode}, and a review needs the login of a Claude account (D-649). stderr: {loginStatus.StandardError.Trim()}");
            return problems;
        }

        string? loginProblem = ClaudeReviewSettings.LoginProblem(loginStatus.StandardOutput);
        if (loginProblem is not null)
        {
            problems.Add(loginProblem);
        }

        return problems;
    }

    public IReadOnlyList<string> ProbeArguments(string directory)
    {
        // The CLI has no option for the directory, and the command starts the probe in it.
        return ClaudeReviewSettings.ProbeArguments();
    }

    public string? ProbeProblem(ProcessResult probe)
    {
        return ClaudeReviewSettings.ProbeProblem(probe);
    }

    public IReadOnlyList<string> ReviewArguments(string worktree, string lastMessagePath, int pullRequest, string branch)
    {
        // The CLI has no option for the directory or for the last message. The command starts the run in the worktree,
        // and SaveLastMessage reads the last message from the transcript.
        return ClaudeReviewSettings.ReviewArguments(ClaudeReviewSettings.ReviewPrompt(pullRequest, branch));
    }

    public void SaveLastMessage(string transcriptPath, string lastMessagePath)
    {
        string lastMessage = ClaudeReviewSettings.LastMessage(File.ReadAllText(transcriptPath), transcriptPath);
        File.WriteAllText(lastMessagePath, lastMessage);
    }
}
