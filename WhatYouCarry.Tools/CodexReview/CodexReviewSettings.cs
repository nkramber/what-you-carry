using System.Collections.Generic;

namespace WhatYouCarry.Tools.CodexReview;

/// <summary>
/// The configuration of the cross-provider review through the Codex CLI (D-511, D-512). Each Codex call names the
/// model, the reasoning effort, the approval policy, and the sandbox on the command line, so no value comes from
/// the user configuration file.
/// </summary>
public static class CodexReviewSettings
{
    public const string Model = "gpt-6-luna";
    public const string ReasoningEffort = "medium";
    public const string ApprovalPolicy = "never";

    /// <summary>The review sandbox. The reviewer runs <c>gh</c>, <c>git push</c>, and the build, so it needs the network and the caches.</summary>
    public const string ReviewSandbox = "danger-full-access";

    /// <summary>The probe runs no command, so it needs no write access.</summary>
    public const string ProbeSandbox = "read-only";

    public const string ProbePrompt = "Reply with the single word OK.";

    /// <summary>The oldest CLI that ran the model probe, 2026-09-23 (D-512).</summary>
    public static readonly CodexVersion MinimumVersion = new(0, 156, 1, string.Empty);

    public const string ReviewSkillPath = ".claude/skills/pr-review/SKILL.md";

    /// <summary>The prompt that the owner typed in the desktop app, with the two facts of an automated start (D-511).</summary>
    public static string ReviewPrompt(int pullRequestNumber, string branch)
    {
        return $"Review PR #{pullRequestNumber}.\n"
            + $"Load and follow `{ReviewSkillPath}`.\n"
            + "The command `make codex-review` started this review in a detached worktree at the PR head.\n"
            + $"Push the review record and your session handoff entry as one metadata commit (D-182) with `git push origin HEAD:{branch}`.\n";
    }

    /// <summary>The arguments of the review run. The transcript is the JSON event stream on stdout.</summary>
    public static IReadOnlyList<string> ReviewArguments(string worktree, string lastMessagePath, string prompt)
    {
        return
        [
            "exec",
            "-m", Model,
            "-c", $"model_reasoning_effort=\"{ReasoningEffort}\"",
            "-c", $"approval_policy=\"{ApprovalPolicy}\"",
            "-s", ReviewSandbox,
            "-C", worktree,
            "--json",
            "-o", lastMessagePath,
            prompt,
        ];
    }

    /// <summary>The arguments of the model probe: one short call outside a repository, with no saved session.</summary>
    public static IReadOnlyList<string> ProbeArguments(string directory)
    {
        return
        [
            "exec",
            "-m", Model,
            "-c", $"model_reasoning_effort=\"{ReasoningEffort}\"",
            "-c", $"approval_policy=\"{ApprovalPolicy}\"",
            "-s", ProbeSandbox,
            "-C", directory,
            "--skip-git-repo-check",
            "--ephemeral",
            "--json",
            ProbePrompt,
        ];
    }
}
