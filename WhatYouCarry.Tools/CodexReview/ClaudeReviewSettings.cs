using System;
using System.Collections.Generic;
using System.Text.Json;

namespace WhatYouCarry.Tools.CodexReview;

/// <summary>
/// The configuration of the cross-provider review through the Claude Code CLI, for a PR that Codex writes (D-649).
/// Each call names the model on the command line, so the model does not come from the user settings. The CLI has no
/// option for the directory, so the command starts each review run in the detached worktree.
/// </summary>
public static class ClaudeReviewSettings
{
    public const string Model = "claude-opus-5-5";

    /// <summary>The reviewer runs <c>gh</c>, <c>git push</c>, and the build with no person to approve each tool call.</summary>
    public const string ReviewPermissionMode = "bypassPermissions";

    public const string ProbePrompt = CodexReviewSettings.ProbePrompt;

    /// <summary>
    /// The environment variables that give the CLI a credential or another provider. The command removes each one from
    /// every Claude process that it starts, so no review uses API pricing (D-649, as D-523 for Codex). Verified
    /// 2026-09-27 on 2.1.283 with <c>claude auth status</c>: a fake <c>ANTHROPIC_AUTH_TOKEN</c> or
    /// <c>CLAUDE_CODE_OAUTH_TOKEN</c> changed the method to <c>oauth_token</c>, and each <c>CLAUDE_CODE_USE_*</c>
    /// switch changed the provider to a cloud platform with its own pricing. A fake <c>ANTHROPIC_API_KEY</c> left the
    /// status at <c>claude.ai</c>, so the status alone does not prove the login of a run. A print run reads that key.
    /// </summary>
    public static readonly string[] ApiCredentialVariables =
    [
        "ANTHROPIC_API_KEY",
        "ANTHROPIC_AUTH_TOKEN",
        "CLAUDE_CODE_OAUTH_TOKEN",
        "CLAUDE_CODE_USE_BEDROCK",
        "CLAUDE_CODE_USE_VERTEX",
        "CLAUDE_CODE_USE_FOUNDRY",
    ];

    public static readonly string[] LoginStatusArguments = ["auth", "status", "--json"];

    /// <summary>The three fields of <c>claude auth status --json</c> for the login of a Claude account, verified 2026-09-27 on 2.1.283.</summary>
    public const string AccountAuthMethod = "claude.ai";

    public const string FirstPartyProvider = "firstParty";

    /// <summary>The CLI on this machine on 2026-09-27, the first CLI of the command (D-649). The owner keeps the CLI updated.</summary>
    public static readonly Version MinimumVersion = new(2, 1, 283);

    /// <summary>The end of the first line of <c>claude --version</c>, such as <c>2.1.283 (Claude Code)</c>.</summary>
    public const string VersionSuffix = " (Claude Code)";

    /// <summary>Reads the first line of the version output. Throws with the text when the line has another form (T-2).</summary>
    public static Version ParseVersion(string versionOutput)
    {
        string line = versionOutput.Split('\n')[0].Trim();
        if (!line.EndsWith(VersionSuffix, StringComparison.Ordinal))
        {
            throw new FormatException($"The Claude Code version output '{line}' does not end with '{VersionSuffix}'.");
        }

        string number = line[..^VersionSuffix.Length];
        if (!Version.TryParse(number, out Version? version) || version.Build < 0 || version.Revision >= 0)
        {
            throw new FormatException($"The Claude Code version '{number}' does not have the form <major>.<minor>.<patch>.");
        }

        return version;
    }

    /// <summary>
    /// The problem of the output of <c>claude auth status --json</c>, or null for the login of a Claude account with the
    /// first-party provider (D-649). The problem gives the three fields alone, because the output also holds the email.
    /// </summary>
    public static string? LoginProblem(string statusOutput)
    {
        const string help = "A review needs loggedIn true, authMethod 'claude.ai', and apiProvider 'firstParty', so it never uses API pricing (D-649). Run `claude auth login` with the Claude account.";
        JsonElement status;
        try
        {
            using JsonDocument document = JsonDocument.Parse(statusOutput);
            status = document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            return $"`claude auth status --json` gave output that is not JSON ({exception.Message}): '{statusOutput.Trim()}'. {help}";
        }

        string loggedIn = FieldText(status, "loggedIn");
        string authMethod = FieldText(status, "authMethod");
        string apiProvider = FieldText(status, "apiProvider");
        if (loggedIn == "true" && authMethod == AccountAuthMethod && apiProvider == FirstPartyProvider)
        {
            return null;
        }

        return $"`claude auth status --json` gives loggedIn {loggedIn}, authMethod '{authMethod}', and apiProvider '{apiProvider}'. {help}";
    }

    /// <summary>The prompt of <see cref="CodexReviewSettings.ReviewPrompt"/>, with the name of this command.</summary>
    public static string ReviewPrompt(int pullRequestNumber, string branch)
    {
        return CodexReviewSettings.ReviewPromptOf(pullRequestNumber, branch, "claude-review");
    }

    /// <summary>
    /// The arguments of the review run. The transcript is the stream of JSON events on stdout, and the stream needs the
    /// verbose option in a print run. The process starts in the worktree, because the CLI has no option for the directory.
    /// </summary>
    public static IReadOnlyList<string> ReviewArguments(string prompt)
    {
        return
        [
            "-p", prompt,
            "--model", Model,
            "--permission-mode", ReviewPermissionMode,
            "--output-format", "stream-json",
            "--verbose",
        ];
    }

    /// <summary>The arguments of the model probe: one short call, which the command starts in a directory outside a repository.</summary>
    public static IReadOnlyList<string> ProbeArguments()
    {
        return
        [
            "-p", ProbePrompt,
            "--model", Model,
            "--output-format", "json",
        ];
    }

    /// <summary>
    /// The problem of the probe output, or null when the model answers OK. The comparison ignores the case, the white
    /// space, and a period at the end. The output is one JSON object with the answer in the field <c>result</c>.
    /// </summary>
    public static string? ProbeProblem(ProcessResult probe)
    {
        string problem = $"The model probe of '{Model}' failed with exit {probe.ExitCode} (D-649). stdout: {probe.StandardOutput.Trim()} stderr: {probe.StandardError.Trim()}";
        if (probe.ExitCode != 0)
        {
            return problem;
        }

        string answer;
        try
        {
            using JsonDocument document = JsonDocument.Parse(probe.StandardOutput);
            answer = FieldText(document.RootElement, "result");
        }
        catch (JsonException exception)
        {
            return $"{problem} The output is not JSON: {exception.Message}";
        }

        return string.Equals(answer.Trim().TrimEnd('.'), "OK", StringComparison.OrdinalIgnoreCase) ? null : problem;
    }

    /// <summary>
    /// The last message of a review run: the field <c>result</c> of the last event of the type <c>result</c> in the
    /// stream on stdout. The CLI has no option that writes the last message to a file.
    /// </summary>
    /// <exception cref="FormatException">A line is not JSON, the stream holds no result event, or the last result event has no text field <c>result</c>. The message names the transcript (T-2).</exception>
    public static string LastMessage(string transcriptText, string transcriptPath)
    {
        string? lastMessage = null;
        int resultLine = 0;
        string[] lines = transcriptText.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(line);
            }
            catch (JsonException exception)
            {
                throw new FormatException($"Line {i + 1} of the transcript '{transcriptPath}' is not JSON: {exception.Message}", exception);
            }

            using (document)
            {
                JsonElement root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object || FieldText(root, "type") != "result")
                {
                    continue;
                }

                resultLine = i + 1;
                lastMessage = root.TryGetProperty("result", out JsonElement result) && result.ValueKind == JsonValueKind.String
                    ? result.GetString()
                    : null;
            }
        }

        if (resultLine == 0)
        {
            throw new FormatException($"The transcript '{transcriptPath}' holds no event of the type 'result', so the review run gave no last message.");
        }

        return lastMessage
            ?? throw new FormatException($"The result event on line {resultLine} of the transcript '{transcriptPath}' has no text field 'result'.");
    }

    /// <summary>The text of a field of a JSON object: a string, <c>true</c>, <c>false</c>, or <c>absent</c>.</summary>
    private static string FieldText(JsonElement element, string property)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(property, out JsonElement value))
        {
            return "absent";
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? "absent",
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => value.GetRawText(),
        };
    }
}
