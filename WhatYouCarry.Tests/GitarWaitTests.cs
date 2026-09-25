using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The Gitar wait after a push (D-575): 60 seconds, then a read of the Gitar check runs every 30 seconds, one
/// <c>Gitar review</c> comment when no check run shows up by 360 seconds, and a stop at 900 seconds. Each behavior test
/// puts a fake <c>gh</c> first on the path and shortens the four times. The Windows leg has no such fake, so it checks
/// the script text and the Makefile alone, and the two other legs of the same gate run the behavior.
/// </summary>
public sealed class GitarWaitTests
{
    private const string Head = "0123456789abcdef0123456789abcdef01234567";
    private const string Completed = "completed success 2026-09-25T04:16:45Z 2026-09-25T04:22:56Z";
    private const string InProgress = "in_progress null 2026-09-25T04:16:45Z null";

    // The fake reads its state from files beside it. A file runs-<n>.txt holds the check runs of read n, and
    // runs.txt holds them for each later read. No file means no Gitar check run.
    private const string FakeGh = """
        #!/usr/bin/env bash
        dir="$(dirname "$0")"
        echo "$*" >> "$dir/calls.log"
        case "$1 $2" in
          "repo view") echo "owner/name" ;;
          "pr view") echo "0123456789abcdef0123456789abcdef01234567" ;;
          "pr comment") echo "$*" >> "$dir/comments.log" ;;
          api*)
            if [ -f "$dir/api-fails" ]; then echo "HTTP 502" >&2; exit 1; fi
            n=$(( $(cat "$dir/reads" 2>/dev/null || echo 0) + 1 ))
            echo "$n" > "$dir/reads"
            if [ -f "$dir/runs-$n.txt" ]; then cat "$dir/runs-$n.txt"; elif [ -f "$dir/runs.txt" ]; then cat "$dir/runs.txt"; fi ;;
          *) echo "fake gh: unknown call $*" >&2; exit 9 ;;
        esac
        """;

    [Fact]
    public void TheScriptAndTheMakefileHoldTheTimesOfD575()
    {
        string script = RepositoryRoot.ReadFile(".github/scripts/gitar-wait.sh");
        Assert.Contains("first=\"${GITAR_WAIT_FIRST:-60}\"", script, StringComparison.Ordinal);
        Assert.Contains("poll=\"${GITAR_WAIT_POLL:-30}\"", script, StringComparison.Ordinal);
        Assert.Contains("request=\"${GITAR_WAIT_REQUEST:-360}\"", script, StringComparison.Ordinal);
        Assert.Contains("limit=\"${GITAR_WAIT_LIMIT:-900}\"", script, StringComparison.Ordinal);
        Assert.Contains("\tbash .github/scripts/gitar-wait.sh $(PR)", RepositoryRoot.ReadFile("Makefile"), StringComparison.Ordinal);
    }

    [Fact]
    public void ACompletedCheckRunEndsTheWaitWithNoRequest()
    {
        RunCase(new Dictionary<string, string> { ["runs.txt"] = Completed }, (result, directory) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Contains($"each Gitar check run on {Head} of PR #7 completed", result.Output, StringComparison.Ordinal);
            Assert.Contains(Completed, result.Output, StringComparison.Ordinal);
            Assert.Contains($"api --paginate repos/owner/name/commits/{Head}/check-runs?per_page=100", File.ReadAllText(Path.Combine(directory, "calls.log")), StringComparison.Ordinal);
            Assert.False(File.Exists(Path.Combine(directory, "comments.log")), "A completed check run needs no Gitar review comment.");
        });
    }

    [Fact]
    public void ARunningCheckRunKeepsTheWaitUntilItCompletes()
    {
        Dictionary<string, string> files = new()
        {
            ["runs-1.txt"] = InProgress,
            ["runs-2.txt"] = $"{Completed}\n{InProgress}",
            ["runs.txt"] = $"{Completed}\n{Completed}",
        };
        RunCase(files, (result, directory) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Equal("3", File.ReadAllText(Path.Combine(directory, "reads")).Trim());
            Assert.False(File.Exists(Path.Combine(directory, "comments.log")), "A running check run needs no Gitar review comment.");
        });
    }

    [Fact]
    public void NoCheckRunPostsOneRequestAndStopsAtTheLimit()
    {
        RunCase(new Dictionary<string, string>(), (result, directory) =>
        {
            Assert.Equal(1, result.Exit);
            string[] comments = File.ReadAllLines(Path.Combine(directory, "comments.log"));
            Assert.Equal(new[] { "pr comment 7 --body Gitar review" }, comments);
            Assert.Contains("Posted one Gitar review comment on PR #7", result.Output, StringComparison.Ordinal);
            Assert.Contains("Stop, and tell the owner (D-575)", result.Errors, StringComparison.Ordinal);
            Assert.Contains("no Gitar check run", result.Errors, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void ACheckRunThatNeverCompletesStopsAtTheLimitWithNoRequest()
    {
        RunCase(new Dictionary<string, string> { ["runs.txt"] = InProgress }, (result, directory) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains(InProgress, result.Errors, StringComparison.Ordinal);
            Assert.False(File.Exists(Path.Combine(directory, "comments.log")), "A running check run needs no Gitar review comment.");
        });
    }

    [Fact]
    public void AFailedReadStopsTheWaitWithItsContext()
    {
        // T-2: a failed read of the check runs is never the same as no check run, so it posts no request.
        RunCase(new Dictionary<string, string> { ["api-fails"] = string.Empty }, (result, directory) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains($"the read of the check runs on {Head} of PR #7 failed", result.Errors, StringComparison.Ordinal);
            Assert.False(File.Exists(Path.Combine(directory, "comments.log")), "A failed read needs no Gitar review comment.");
        });
    }

    [Fact]
    public void TheWaitNeedsOnePrNumber()
    {
        RunCase(new Dictionary<string, string>(), (result, _) => Assert.Equal(2, result.Exit), arguments: []);
    }

    /// <summary>
    /// Runs the script against the fake in a new directory that holds the named state files, with the times shortened
    /// to 0, 1, 1, and 4 seconds. The check reads the result and the directory. Windows skips it.
    /// </summary>
    private static void RunCase(Dictionary<string, string> files, Action<(int Exit, string Output, string Errors), string> check, string[]? arguments = null)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        string directory = Path.Combine(Path.GetTempPath(), $"wyc-gitar-wait-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            string gh = Path.Combine(directory, "gh");
            File.WriteAllText(gh, FakeGh.Replace("\r\n", "\n", StringComparison.Ordinal) + "\n");
            File.SetUnixFileMode(gh, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            foreach ((string name, string text) in files)
            {
                File.WriteAllText(Path.Combine(directory, name), text + "\n");
            }

            check(RunScript(directory, arguments ?? ["7"]), directory);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>Runs the wait under bash with the fake first on the path, and returns the exit code and both outputs.</summary>
    private static (int Exit, string Output, string Errors) RunScript(string fakeDirectory, string[] arguments)
    {
        string script = Path.Combine(RepositoryRoot.Find(), ".github", "scripts", "gitar-wait.sh");
        ProcessStartInfo start = new("bash") { RedirectStandardError = true, RedirectStandardOutput = true, UseShellExecute = false };
        start.ArgumentList.Add(script);
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        string path = Environment.GetEnvironmentVariable("PATH") ?? throw new InvalidOperationException("The test process has no PATH variable.");
        start.Environment["PATH"] = fakeDirectory + Path.PathSeparator + path;
        start.Environment["GITAR_WAIT_FIRST"] = "0";
        start.Environment["GITAR_WAIT_POLL"] = "1";
        start.Environment["GITAR_WAIT_REQUEST"] = "1";
        start.Environment["GITAR_WAIT_LIMIT"] = "4";

        using Process process = Process.Start(start) ?? throw new InvalidOperationException($"bash did not start for the script {script}.");
        System.Threading.Tasks.Task<string> errors = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output, errors.Result);
    }
}
