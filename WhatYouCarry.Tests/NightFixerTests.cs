using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The night fixer of D-643 to D-645: the poll that starts one session for a failed night on main, the prompt that binds
/// that session, and the notices to the owner. Each behavior test runs the poll with <c>--dry-run</c> and a fake
/// <c>gh</c> first on the path, so no session starts. The Windows leg has no such fake, so it checks the text alone.
/// </summary>
[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
public sealed class NightFixerTests
{
    private const string Sha = "0123456789abcdef0123456789abcdef01234567";

    // The fake reads its state from files beside it. run.txt holds the newest night on main, open.txt the count of open
    // fix PRs, and a file read-fails makes each read fail. Each call goes to calls.log.
    private const string FakeGh = """
        #!/usr/bin/env bash
        dir="$(dirname "$0")"
        echo "$*" >> "$dir/calls.log"
        if [ -f "$dir/read-fails" ]; then echo "HTTP 502" >&2; exit 1; fi
        case "$1 $2" in
          "run list") if [ -f "$dir/run.txt" ]; then cat "$dir/run.txt"; fi ;;
          "pr list") cat "$dir/open.txt" 2>/dev/null || echo 0 ;;
          "workflow run") if [ -f "$dir/notify-fails" ]; then echo "HTTP 422" >&2; exit 1; fi; echo "$*" >> "$dir/notices.log" ;;
          *) echo "fake gh: unknown call $*" >&2; exit 9 ;;
        esac
        """;

    [Fact]
    public void ARunningNightStartsNoSession()
    {
        RunCase(new() { ["run.txt"] = $"41 in_progress  {Sha}" }, (result, _) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Contains("the night 41 is in_progress", result.Output, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void APassedNightStartsNoSession()
    {
        RunCase(new() { ["run.txt"] = $"42 completed success {Sha}" }, (result, _) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Contains("the night 42 passed", result.Output, StringComparison.Ordinal);
        });
    }

    /// <summary>A failed night that no session took, with no fix PR open, starts a session. The dry run names it and marks nothing.</summary>
    [Fact]
    public void ANewFailedNightStartsASession()
    {
        RunCase(new() { ["run.txt"] = $"43 completed failure {Sha}", ["open.txt"] = "0" }, (result, state) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Contains($"a session would start for the night 43 at {Sha}", result.Output, StringComparison.Ordinal);
            Assert.Equal(string.Empty, File.ReadAllText(Path.Combine(state, "handled")));
            Assert.False(Directory.Exists(Path.Combine(state, "lock")), "The poll removes its lock at its end.");
        });
    }

    [Fact]
    public void ACancelledNightCountsAsFailed()
    {
        RunCase(new() { ["run.txt"] = $"44 completed cancelled {Sha}", ["open.txt"] = "0" }, (result, _) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Contains("a session would start for the night 44", result.Output, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void AHandledNightStartsNoSecondSession()
    {
        RunCase(new() { ["run.txt"] = $"45 completed failure {Sha}", ["state/handled"] = "45" }, (result, _) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Contains("a session already took the night 45", result.Output, StringComparison.Ordinal);
        });
    }

    /// <summary>An open fix PR holds a new failed night in the file queued, one time, and starts no session (D-643).</summary>
    [Fact]
    public void AnOpenFixPrQueuesTheNight()
    {
        RunCase(new() { ["run.txt"] = $"46 completed failure {Sha}", ["open.txt"] = "1" }, (result, state) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Contains("a fix PR is open, so the night 46 waits", result.Output, StringComparison.Ordinal);
            Assert.Equal("46\n", File.ReadAllText(Path.Combine(state, "queued")));
        });
    }

    /// <summary>A lock that a live process holds stops the poll, and the lock stays (D-643).</summary>
    [Fact]
    public void ALiveLockStartsNoSession()
    {
        RunCase(new() { ["run.txt"] = $"47 completed failure {Sha}", ["state/lock/pid"] = Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture) }, (result, state) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Contains($"the session of poll {Environment.ProcessId} runs", result.Output, StringComparison.Ordinal);
            Assert.True(Directory.Exists(Path.Combine(state, "lock")), "The poll keeps the lock of a live session.");
        });
    }

    /// <summary>A lock of a process that ended goes, and the poll reads on.</summary>
    [Fact]
    public void AStaleLockGoes()
    {
        RunCase(new() { ["run.txt"] = $"48 completed failure {Sha}", ["open.txt"] = "0", ["state/lock/pid"] = "999999" }, (result, _) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Contains("the lock of poll '999999' stays after its end", result.Output, StringComparison.Ordinal);
            Assert.Contains("a session would start for the night 48", result.Output, StringComparison.Ordinal);
        });
    }

    /// <summary>
    /// PR #109 automated pass. A setup that fails, here a fetch in a folder with no git checkout, sends a notice and then
    /// marks the night handled. The old poll marked the night first, and a failed setup then left it with no session
    /// and no notice (T-2).
    /// </summary>
    [Fact]
    public void AFailedSetupNotifiesTheOwnerAndThenMarksTheNight()
    {
        RunCase(new() { ["run.txt"] = $"49 completed failure {Sha}", ["open.txt"] = "0" }, (result, state) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains("the setup of the session for the night 49 failed", result.Errors, StringComparison.Ordinal);
            string notices = File.ReadAllText(Path.Combine(Path.GetDirectoryName(state)!, "notices.log"));
            Assert.Contains("workflow run notify.yml --repo nkramber/what-you-carry --ref main", notices, StringComparison.Ordinal);
            Assert.Contains("The setup of the session for the night 49 failed", notices, StringComparison.Ordinal);
            Assert.Equal("49\n", File.ReadAllText(Path.Combine(state, "handled")));
        }, []);
    }

    /// <summary>PR #109 automated pass. A failed setup whose notice also fails keeps the night open, so the next poll tries again.</summary>
    [Fact]
    public void AFailedSetupWithNoNoticeKeepsTheNightOpen()
    {
        RunCase(new() { ["run.txt"] = $"50 completed failure {Sha}", ["open.txt"] = "0", ["notify-fails"] = "yes" }, (result, state) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains("the notice 'What You Carry: the night fixer stopped' did not start", result.Errors, StringComparison.Ordinal);
            Assert.Equal(string.Empty, File.ReadAllText(Path.Combine(state, "handled")));
        }, []);
    }

    [Fact]
    public void AFailedReadStopsThePollWithItsContext()
    {
        RunCase(new() { ["read-fails"] = "yes" }, (result, _) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains("the read of the newest night on main failed", result.Errors, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void AnUnknownArgumentIsAUsageError()
    {
        RunCase([], (result, _) => Assert.Equal(2, result.Exit), ["--start"]);
    }

    /// <summary>The poll starts the session from the prompt of its new worktree, and a failed session sends a notice (D-643, D-645).</summary>
    [Fact]
    public void ThePollStartsTheSessionFromThePrompt()
    {
        string script = RepositoryRoot.ReadFile(".github/scripts/night-fixer.sh");
        Assert.Contains("git -C \"$checkout\" worktree add --quiet -b \"$branch\" \"$work\" origin/main", script, StringComparison.Ordinal);
        Assert.Contains("\"$work/docs/runbooks/night-fixer-prompt.md\"", script, StringComparison.Ordinal);
        Assert.Contains("claude -p \"$prompt\" --dangerously-skip-permissions", script, StringComparison.Ordinal);
        Assert.Contains("gh workflow run notify.yml --repo \"$repo\" --ref main", script, StringComparison.Ordinal);
        Assert.Contains("--workflow night.yml --branch main --limit 1", script, StringComparison.Ordinal);
    }

    /// <summary>The prompt forbids the merge and each change of a setting, and names each stop and the notice at the end (D-643 to D-645).</summary>
    [Fact]
    public void ThePromptHoldsTheRulesOfTheFixer()
    {
        string prompt = RepositoryRoot.ReadFile("docs/runbooks/night-fixer-prompt.md");
        string[] rules =
        [
            "Never merge a PR. Never run `gh pr merge`.",
            "Never change a secret, a permission, the ruleset, or a repository setting.",
            "Never apply the label `review-override`",
            "Session answer under D-644, for the owner to confirm",
            "\"three branch nights failed\"",
            "\"the Codex three-strike stop\"",
            "\"ready to merge\"",
            "RUN_ID",
            "RUN_SHA",
            "FIX_BRANCH",
        ];
        foreach (string rule in rules)
        {
            Assert.Contains(rule, prompt, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// The workflow notify.yml sends one notice on request through the Pushover action, from the repository secrets (D-645).
    /// The action checks both keys and the priority, and a failed send fails the step (T-2).
    /// </summary>
    [Fact]
    public void TheNoticeWorkflowUsesTheSecretsThroughTheAction()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/notify.yml");
        Assert.Contains("  workflow_dispatch:\n", workflow, StringComparison.Ordinal);
        Assert.Contains("permissions:\n  contents: read\n", workflow, StringComparison.Ordinal);
        Assert.Contains("uses: ./.github/actions/pushover", workflow, StringComparison.Ordinal);
        Assert.Contains("user-key: ${{ secrets.PUSHOVER_USER_KEY }}", workflow, StringComparison.Ordinal);
        Assert.Contains("api-token: ${{ secrets.PUSHOVER_API_TOKEN }}", workflow, StringComparison.Ordinal);

        string action = RepositoryRoot.ReadFile(".github/actions/pushover/action.yml");
        Assert.Contains("curl -sS --fail-with-body", action, StringComparison.Ordinal);
        Assert.Contains("--form-string \"token=${PUSHOVER_API_TOKEN}\"", action, StringComparison.Ordinal);
        Assert.Contains("--form-string \"user=${PUSHOVER_USER_KEY}\"", action, StringComparison.Ordinal);
        Assert.Contains("--form-string \"message=${NOTICE_MESSAGE}\"", action, StringComparison.Ordinal);
        Assert.Contains("if [ \"${NOTICE_PRIORITY}\" != \"0\" ] && [ \"${NOTICE_PRIORITY}\" != \"1\" ]; then", action, StringComparison.Ordinal);
        Assert.DoesNotContain("${{ inputs.message }}\"", action, StringComparison.Ordinal);
    }

    /// <summary>Runs the poll with --dry-run under bash, with the fake first on the path and the state in a new directory. Windows skips it.</summary>
    private static void RunCase(Dictionary<string, string> files, Action<(int Exit, string Output, string Errors), string> check, string[]? arguments = null)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        string directory = Path.Combine(Path.GetTempPath(), $"wyc-night-fixer-{Guid.NewGuid():N}");
        string state = Path.Combine(directory, "state");
        Directory.CreateDirectory(state);
        try
        {
            string gh = Path.Combine(directory, "gh");
            File.WriteAllText(gh, FakeGh.Replace("\r\n", "\n", StringComparison.Ordinal) + "\n");
            File.SetUnixFileMode(gh, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            foreach ((string name, string text) in files)
            {
                string path = Path.Combine(directory, name);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, text + "\n");
            }

            check(RunScript(directory, state, arguments ?? ["--dry-run"]), state);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static (int Exit, string Output, string Errors) RunScript(string fakeDirectory, string state, string[] arguments)
    {
        string script = Path.Combine(RepositoryRoot.Find(), ".github", "scripts", "night-fixer.sh");
        ProcessStartInfo start = new("bash") { RedirectStandardError = true, RedirectStandardOutput = true, UseShellExecute = false };
        start.ArgumentList.Add(script);
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        string path = Environment.GetEnvironmentVariable("PATH") ?? throw new InvalidOperationException("The test process has no PATH variable.");
        start.Environment["PATH"] = fakeDirectory + Path.PathSeparator + path;
        start.Environment["WYC_FIXER_STATE"] = state;
        start.Environment["WYC_FIXER_REPO"] = fakeDirectory;

        using Process process = Process.Start(start) ?? throw new InvalidOperationException("bash did not start.");
        string output = process.StandardOutput.ReadToEnd();
        string errors = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output, errors);
    }
}
