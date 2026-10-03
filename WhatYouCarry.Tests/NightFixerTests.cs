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
    // fix PRs, and a file read-fails makes each read fail. A dispatch of notify.yml fails with the file notify-fails. Its
    // run is 777, and that run fails with the file notify-run-fails. Each call goes to calls.log.
    private const string FakeGh = """
        #!/usr/bin/env bash
        dir="$(dirname "$0")"
        echo "$*" >> "$dir/calls.log"
        if [ -f "$dir/read-fails" ]; then echo "HTTP 502" >&2; exit 1; fi
        case "$1 $2" in
          "run list")
            case "$*" in *notify.yml*) echo 777; exit 0 ;; esac
            if [ -f "$dir/run.txt" ]; then cat "$dir/run.txt"; fi ;;
          "run watch") if [ -f "$dir/notify-run-fails" ]; then exit 1; fi ;;
          "pr list") cat "$dir/open.txt" 2>/dev/null || echo 0 ;;
          "workflow run") if [ -f "$dir/notify-fails" ]; then echo "HTTP 422" >&2; exit 1; fi; echo "$*" >> "$dir/notices.log" ;;
          *) echo "fake gh: unknown call $*" >&2; exit 9 ;;
        esac
        """;

    // The fake session writes each call to claude-calls.log, and the three session settings to claude-env.log. The prompt
    // of the test checkout is END_FILE alone, so the first call names the path of the end mark. The file claude-exit holds
    // the exit code of each call. The file claude-ends-at holds the number of the call that writes the end mark.
    private const string FakeClaude = """
        #!/usr/bin/env bash
        dir="$(dirname "$0")"
        echo "$*" >> "$dir/claude-calls.log"
        echo "background=${CLAUDE_CODE_DISABLE_BACKGROUND_TASKS:-} default=${BASH_DEFAULT_TIMEOUT_MS:-} max=${BASH_MAX_TIMEOUT_MS:-}" >> "$dir/claude-env.log"
        if [ "$3" = "--session-id" ]; then printf '%s' "$2" > "$dir/end-path"; fi
        if [ -f "$dir/claude-exit" ]; then exit "$(cat "$dir/claude-exit")"; fi
        calls=$(( $(wc -l < "$dir/claude-calls.log") ))
        if [ -f "$dir/claude-ends-at" ] && [ "$calls" -ge "$(cat "$dir/claude-ends-at")" ]; then
          echo "ready to merge https://github.com/nkramber/what-you-carry/pull/1" > "$(cat "$dir/end-path")"
        fi
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
        string live = Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RunCase(new() { ["run.txt"] = $"47 completed failure {Sha}" }, (result, state) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Contains($"the session of poll {live} runs", result.Output, StringComparison.Ordinal);
            Assert.Equal(live, new FileInfo(Path.Combine(state, "lock")).LinkTarget);
        }, lockLink: live);
    }

    /// <summary>A lock of a process that ended goes, and the poll takes the lock and reads on.</summary>
    [Fact]
    public void AStaleLockGoes()
    {
        RunCase(new() { ["run.txt"] = $"48 completed failure {Sha}", ["open.txt"] = "0" }, (result, state) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.False(Directory.Exists(Path.Combine(state, "lock.reap")), "The poll removes its guard after the removal of the stale lock.");
            Assert.Contains("the lock of poll 999999 stays after its end", result.Output, StringComparison.Ordinal);
            Assert.Contains("a session would start for the night 48", result.Output, StringComparison.Ordinal);
        }, lockLink: "999999");
    }

    /// <summary>
    /// PR #109 automated pass. A poll that finds another poll in the removal of a stale lock steps back and leaves the lock.
    /// The check of the target and the removal were two open steps, so a second poll could remove a lock that the first
    /// poll took between them.
    /// </summary>
    [Fact]
    public void AGuardOfAnotherPollLeavesTheStaleLock()
    {
        RunCase(new() { ["run.txt"] = $"54 completed failure {Sha}", ["open.txt"] = "0" }, (result, state) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Contains("another poll removes the stale lock, so this poll starts none", result.Output, StringComparison.Ordinal);
            Assert.Equal("999999", new FileInfo(Path.Combine(state, "lock")).LinkTarget);
        }, lockLink: "999999", guardAgeMinutes: 0);
    }

    /// <summary>PR #109 automated pass. A guard that stays for more than 10 minutes stops each poll with an error, so a stopped poll never blocks the fixer in silence.</summary>
    [Fact]
    public void AnOldGuardStopsThePoll()
    {
        RunCase(new() { ["run.txt"] = $"55 completed failure {Sha}", ["open.txt"] = "0" }, (result, _) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains("stays for more than 10 minutes, so this poll stops", result.Errors, StringComparison.Ordinal);
        }, lockLink: "999999", guardAgeMinutes: 11);
    }

    /// <summary>
    /// PR #109 review P2-3. A lock with no process id is the state of the old lock between its directory and its pid
    /// file, while a live poll writes it. The poll stops with an error and leaves the lock. The old poll removed such a
    /// lock as stale, and two polls then passed it. The new lock names its process id from its first moment.
    /// </summary>
    [Fact]
    public void ALockWithNoProcessIdStaysAndStopsThePoll()
    {
        RunCase(new() { ["run.txt"] = $"51 completed failure {Sha}", ["open.txt"] = "0" }, (result, state) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains("is no link, so it names no process id, and this poll stops", result.Errors, StringComparison.Ordinal);
            Assert.Empty(Directory.GetFileSystemEntries(Path.Combine(state, "lock")));
            Assert.True(Directory.Exists(Path.Combine(state, "lock")), "The poll keeps a lock that names no process id.");
            Assert.DoesNotContain("a session would start", result.Output, StringComparison.Ordinal);
        }, lockDirectory: true);
    }

    /// <summary>PR #109 review P2-3. A lock whose target is no number stops the poll, and the lock stays.</summary>
    [Fact]
    public void ALockOfAnotherFormStopsThePoll()
    {
        RunCase(new() { ["run.txt"] = $"52 completed failure {Sha}", ["open.txt"] = "0" }, (result, state) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Equal("not-a-pid", new FileInfo(Path.Combine(state, "lock")).LinkTarget);
        }, lockLink: "not-a-pid");
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
            Assert.Contains("-f id=", notices, StringComparison.Ordinal);
            Assert.Contains("run watch 777 --repo nkramber/what-you-carry --exit-status", File.ReadAllText(Path.Combine(Path.GetDirectoryName(state)!, "calls.log")), StringComparison.Ordinal);
            Assert.Contains("the run 777 delivered the notice", result.Output, StringComparison.Ordinal);
            Assert.Equal("49\n", File.ReadAllText(Path.Combine(state, "handled")));
        }, []);
    }

    /// <summary>PR #109 automated pass. A failed setup whose notice fails at the dispatch keeps the night open, so the next poll tries again.</summary>
    [Fact]
    public void AFailedSetupWithNoNoticeKeepsTheNightOpen()
    {
        RunCase(new() { ["run.txt"] = $"50 completed failure {Sha}", ["open.txt"] = "0", ["notify-fails"] = "yes" }, (result, state) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains("the notice 'What You Carry: the night fixer stopped' did not reach the owner", result.Errors, StringComparison.Ordinal);
            Assert.Equal(string.Empty, File.ReadAllText(Path.Combine(state, "handled")));
        }, []);
    }

    /// <summary>
    /// PR #109 review P2-2. A dispatch that GitHub accepts, whose run of notify.yml then fails, is no notice. The night
    /// stays open, so the next poll tries again. The old poll counted the accepted dispatch as the notice.
    /// </summary>
    [Fact]
    public void AFailedNoticeRunKeepsTheNightOpen()
    {
        RunCase(new() { ["run.txt"] = $"53 completed failure {Sha}", ["open.txt"] = "0", ["notify-run-fails"] = "yes" }, (result, state) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains("the run 777 of notify.yml failed, so the notice", result.Errors, StringComparison.Ordinal);
            Assert.Equal(string.Empty, File.ReadAllText(Path.Combine(state, "handled")));
        }, []);
    }

    /// <summary>
    /// D-769, the night 37015330351. A session that exits 0 with no end mark resumes 3 times under its own session id,
    /// and then the poll sends a notice and exits 1. The old poll took the exit 0 as the end, and sent no notice.
    /// </summary>
    [Fact]
    public void ASessionWithNoEndMarkResumesThreeTimesAndThenNotifiesTheOwner()
    {
        RunCase(new() { ["run.txt"] = $"56 completed failure {Sha}", ["open.txt"] = "0" }, (result, state) =>
        {
            Assert.Equal(1, result.Exit);
            string directory = Path.GetDirectoryName(state)!;
            string[] calls = File.ReadAllLines(Path.Combine(directory, "claude-calls.log"));
            Assert.Equal(4, calls.Length);
            Assert.EndsWith("--session-id 00000000-0000-4000-8000-000000000056 --dangerously-skip-permissions", calls[0], StringComparison.Ordinal);
            for (int resume = 1; resume < calls.Length; resume++)
            {
                Assert.StartsWith("-p Your last reply ended this session before its end mark", calls[resume], StringComparison.Ordinal);
                Assert.EndsWith("--resume 00000000-0000-4000-8000-000000000056 --dangerously-skip-permissions", calls[resume], StringComparison.Ordinal);
            }

            Assert.Contains("resume 3 of 3 starts", result.Output, StringComparison.Ordinal);
            Assert.Contains("ended with no end mark after 3 resumes", result.Errors, StringComparison.Ordinal);
            string notices = File.ReadAllText(Path.Combine(directory, "notices.log"));
            Assert.Contains("The session for the night 56 ended with no end mark after 3 resumes", notices, StringComparison.Ordinal);
            Assert.Equal("56\n", File.ReadAllText(Path.Combine(state, "handled")));
        }, [], withCheckout: true);
    }

    /// <summary>A session that writes its end mark in its first resume ends the poll with that mark, and runs with no background tasks and a command limit of 6 hours (D-769).</summary>
    [Fact]
    public void ASessionThatWritesTheEndMarkEndsThePoll()
    {
        RunCase(new() { ["run.txt"] = $"57 completed failure {Sha}", ["open.txt"] = "0", ["claude-ends-at"] = "2" }, (result, state) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            string directory = Path.GetDirectoryName(state)!;
            Assert.Equal(2, File.ReadAllLines(Path.Combine(directory, "claude-calls.log")).Length);
            Assert.Contains("the session for the night 57 ended: ready to merge https://github.com/nkramber/what-you-carry/pull/1", result.Output, StringComparison.Ordinal);
            Assert.Equal(Path.Combine(state, "end-57"), File.ReadAllText(Path.Combine(directory, "end-path")));
            foreach (string line in File.ReadAllLines(Path.Combine(directory, "claude-env.log")))
            {
                Assert.Equal("background=1 default=21600000 max=21600000", line);
            }

            Assert.False(File.Exists(Path.Combine(directory, "notices.log")), "A session with its end mark sends no notice from the poll.");
        }, [], withCheckout: true);
    }

    /// <summary>A session that exits with an error resumes no time, and the poll sends the notice of its exit code (D-645).</summary>
    [Fact]
    public void ASessionThatFailsNotifiesTheOwnerWithNoResume()
    {
        RunCase(new() { ["run.txt"] = $"58 completed failure {Sha}", ["open.txt"] = "0", ["claude-exit"] = "3" }, (result, state) =>
        {
            Assert.Equal(3, result.Exit);
            string directory = Path.GetDirectoryName(state)!;
            Assert.Single(File.ReadAllLines(Path.Combine(directory, "claude-calls.log")));
            Assert.Contains("The session for the night 58 ended with exit code 3", File.ReadAllText(Path.Combine(directory, "notices.log")), StringComparison.Ordinal);
        }, [], withCheckout: true);
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
        Assert.Contains("env \"${session_env[@]}\" claude -p \"$prompt\" --session-id \"$session\" --dangerously-skip-permissions", script, StringComparison.Ordinal);
        Assert.Contains("bash \"$(dirname \"$0\")/notify-owner.sh\"", script, StringComparison.Ordinal);
        Assert.Contains("--workflow night.yml --branch main --limit 1", script, StringComparison.Ordinal);
        Assert.Contains("ln -sn \"$$\" \"$lock\"", script, StringComparison.Ordinal);

        // A notice counts only when its run of notify.yml succeeds (PR #109 review P2-2).
        string notice = RepositoryRoot.ReadFile(".github/scripts/notify-owner.sh");
        Assert.Contains("gh workflow run notify.yml --repo \"$repo\" --ref main", notice, StringComparison.Ordinal);
        Assert.Contains("-f id=\"$id\"", notice, StringComparison.Ordinal);
        Assert.Contains("gh run watch \"$run\" --repo \"$repo\" --exit-status", notice, StringComparison.Ordinal);
        Assert.Contains("run-name: Notify ${{ inputs.id }}", RepositoryRoot.ReadFile(".github/workflows/notify.yml"), StringComparison.Ordinal);
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
            "Never end a reply while work is in flight.",
            "Run each command in the foreground, and wait for its end.",
            "Write the end mark as the last step of the session, after the last notice.",
            "RUN_ID",
            "RUN_SHA",
            "FIX_BRANCH",
            "END_FILE",
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

    /// <summary>
    /// Runs the poll with --dry-run under bash, with the fakes first on the path and the state in a new directory. A lock
    /// link names its target, and a lock directory is the old form with no process id. A case with a checkout gets a git
    /// repository whose origin/main holds a prompt of the one word END_FILE, so the setup passes. Windows skips it.
    /// </summary>
    private static void RunCase(Dictionary<string, string> files, Action<(int Exit, string Output, string Errors), string> check, string[]? arguments = null, string? lockLink = null, bool lockDirectory = false, int? guardAgeMinutes = null, bool withCheckout = false)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        string directory = Path.Combine(Path.GetTempPath(), $"wyc-night-fixer-{Guid.NewGuid():N}");
        string state = Path.Combine(directory, "state");
        Directory.CreateDirectory(state);
        using TemporaryGitRepository? checkout = withCheckout ? new TemporaryGitRepository() : null;
        try
        {
            WriteFake(directory, "gh", FakeGh);
            WriteFake(directory, "claude", FakeClaude);
            if (checkout is not null)
            {
                checkout.Commit("prompt", new Dictionary<string, string> { ["docs/runbooks/night-fixer-prompt.md"] = "END_FILE\n" });
                checkout.Git(["remote", "add", "origin", checkout.Path]);
            }

            foreach ((string name, string text) in files)
            {
                string path = Path.Combine(directory, name);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, text + "\n");
            }

            if (lockLink is not null)
            {
                File.CreateSymbolicLink(Path.Combine(state, "lock"), lockLink);
            }

            if (lockDirectory)
            {
                Directory.CreateDirectory(Path.Combine(state, "lock"));
            }

            if (guardAgeMinutes is int age)
            {
                string guard = Path.Combine(state, "lock.reap");
                Directory.CreateDirectory(guard);
                Directory.SetLastWriteTimeUtc(guard, DateTime.UtcNow.AddMinutes(-age));
            }

            check(RunScript(directory, state, arguments ?? ["--dry-run"], checkout?.Path ?? directory), state);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [System.Runtime.Versioning.UnsupportedOSPlatform("windows")]
    private static void WriteFake(string directory, string name, string text)
    {
        string path = Path.Combine(directory, name);
        File.WriteAllText(path, text.Replace("\r\n", "\n", StringComparison.Ordinal) + "\n");
        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    }

    private static (int Exit, string Output, string Errors) RunScript(string fakeDirectory, string state, string[] arguments, string checkout)
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
        start.Environment["WYC_FIXER_REPO"] = checkout;
        start.Environment["WYC_NOTIFY_TRIES"] = "2";
        start.Environment["WYC_NOTIFY_PAUSE"] = "0";

        using Process process = Process.Start(start) ?? throw new InvalidOperationException("bash did not start.");
        System.Threading.Tasks.Task<string> errorsTask = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        string errors = errorsTask.Result;
        process.WaitForExit();
        return (process.ExitCode, output, errors);
    }
}
