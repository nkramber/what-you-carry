using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The launch scripts of README, "Launch the game": <c>launch/what-you-carry.sh</c> on the Steam Deck and
/// <c>launch/what-you-carry.ps1</c> on Windows. Each script updates the checkout to the newest main, builds, imports,
/// and starts the game, and it stops at the first step that fails. Each behavior test copies the script into a temporary
/// checkout and puts fakes of git, dotnet, and Godot first on the path. The bash tests run on Linux and macOS, and
/// the PowerShell tests run on the Windows leg alone, because each fake set needs its own shell.
/// </summary>
public sealed class LauncherTests
{
    private const string ShellScript = "launch/what-you-carry.sh";
    private const string PowerShellScript = "launch/what-you-carry.ps1";

    // Each fake writes its name and its arguments to calls.log. The fake git prints status.txt for a status read, and
    // the fake Godot exits with code 3 from the import when the file import-fails exists.
    private const string FakeShell = """
        #!/usr/bin/env bash
        dir="$(dirname "$0")"
        echo "$(basename "$0") $*" >> "$dir/calls.log"
        if [ "$(basename "$0")" = git ] && [ "$3" = status ] && [ -f "$dir/status.txt" ]; then cat "$dir/status.txt"; fi
        if [ "$(basename "$0")" = godot ] && [ "$1" = --headless ] && [ -f "$dir/import-fails" ]; then exit 3; fi
        exit 0
        """;

    private const string FakeCommand = """
        @echo off
        echo %~n0 %*>>"%~dp0calls.log"
        if "%~n0"=="git" if "%3"=="status" if exist "%~dp0status.txt" type "%~dp0status.txt"
        if "%~n0"=="godot" if "%1"=="--headless" if exist "%~dp0import-fails" exit /b 3
        exit /b 0
        """;

    /// <summary>Both scripts start the Godot release that the smoke workflow pins, and the Windows script checks its pinned SHA-512 (D-626).</summary>
    [Fact]
    public void TheScriptsPinTheGodotOfTheSmokeWorkflow()
    {
        string smoke = RepositoryRoot.ReadFile(".github/workflows/smoke.yml");
        string version = WorkflowValue(smoke, "GODOT_VERSION");
        string sha512 = WorkflowValue(smoke, "GODOT_SHA512_WINDOWS");
        Assert.Contains($"GODOT_VERSION=\"{version}\"", RepositoryRoot.ReadFile(ShellScript), StringComparison.Ordinal);
        string powerShell = RepositoryRoot.ReadFile(PowerShellScript);
        Assert.Contains($"$GodotVersion = '{version}'", powerShell, StringComparison.Ordinal);
        Assert.Contains($"$GodotSha512 = '{sha512}'", powerShell, StringComparison.Ordinal);
    }

    /// <summary>The Windows install writes a command file that starts the script from any directory, with no change to the execution policy.</summary>
    [Fact]
    public void TheWindowsInstallWritesACommandOnThePath()
    {
        string powerShell = RepositoryRoot.ReadFile(PowerShellScript);
        Assert.Contains("$CommandFile = Join-Path $env:LOCALAPPDATA 'Microsoft\\WindowsApps\\what-you-carry.cmd'", powerShell, StringComparison.Ordinal);
        Assert.Contains("powershell.exe -NoProfile -ExecutionPolicy Bypass -File `\"$script`\" %*", powerShell, StringComparison.Ordinal);
    }

    [Fact]
    public void TheShellScriptUpdatesBuildsImportsAndPlays()
    {
        RunShellCase([], new Dictionary<string, string>(), (result, repo, fakes, home) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Equal(UpdateCalls(repo).Concat(BuildCalls(repo)), Calls(fakes));
        });
    }

    [Fact]
    public void TheShellScriptStopsBeforeTheCheckoutWhenATrackedFileHasAChange()
    {
        RunShellCase([], new Dictionary<string, string> { ["status.txt"] = " M Makefile" }, (result, repo, fakes, home) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains($"a tracked file in {repo} has a change", result.Errors, StringComparison.Ordinal);
            Assert.Contains("--no-update", result.Errors, StringComparison.Ordinal);
            Assert.Equal(UpdateCalls(repo).Take(2), Calls(fakes));
        });
    }

    [Fact]
    public void TheShellScriptWithNoUpdateBuildsAndPlaysTheCheckoutAsItIs()
    {
        RunShellCase(["--no-update"], new Dictionary<string, string>(), (result, repo, fakes, home) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            Assert.Equal(BuildCalls(repo), Calls(fakes));
        });
    }

    [Fact]
    public void TheShellScriptStopsWithTheCodeOfAFailedImport()
    {
        RunShellCase(["--no-update"], new Dictionary<string, string> { ["import-fails"] = "" }, (result, repo, fakes, home) =>
        {
            Assert.Equal(3, result.Exit);
            Assert.Contains("the step 'import' failed with exit code 3", result.Errors, StringComparison.Ordinal);
            Assert.Equal(BuildCalls(repo).Take(2), Calls(fakes));
        });
    }

    [Fact]
    public void TheShellScriptStopsWhenNoGodotBinaryExists()
    {
        RunShellCase(["--no-update"], new Dictionary<string, string>(), (result, repo, fakes, home) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains("no Godot .NET binary at", result.Errors, StringComparison.Ordinal);
            Assert.Contains("WYC_GODOT", result.Errors, StringComparison.Ordinal);
            Assert.Empty(Calls(fakes));
        }, godotExists: false);
    }

    [Fact]
    public void TheShellScriptRefusesAnUnknownArgument()
    {
        RunShellCase(["--fast"], new Dictionary<string, string>(), (result, repo, fakes, home) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains("unknown argument '--fast'", result.Errors, StringComparison.Ordinal);
            Assert.Empty(Calls(fakes));
        });
    }

    [Fact]
    public void TheShellInstallAddsTheCommandAndTheDesktopShortcut()
    {
        RunShellCase(["--install"], new Dictionary<string, string>(), (result, repo, fakes, home) =>
        {
            Assert.True(result.Exit == 0, result.Errors);
            string command = Path.Combine(home, ".local", "bin", "what-you-carry");
            Assert.Equal(Path.Combine(repo, "launch", "what-you-carry.sh"), new FileInfo(command).LinkTarget);
            string shortcut = File.ReadAllText(Path.Combine(home, "Desktop", "what-you-carry.desktop"));
            Assert.Contains($"Exec={command}\n", shortcut, StringComparison.Ordinal);
            Assert.Contains($"Path={repo}\n", shortcut, StringComparison.Ordinal);
            Assert.Contains("Terminal=true\n", shortcut, StringComparison.Ordinal);
            Assert.Empty(Calls(fakes));
        });
    }

    [Fact]
    public void ThePowerShellScriptUpdatesBuildsImportsAndPlays()
    {
        RunPowerShellCase([], new Dictionary<string, string>(), (result, repo, fakes) =>
        {
            Assert.True(result.Exit == 0, result.Output + result.Errors);
            Assert.Equal(UpdateCalls(repo).Concat(BuildCalls(repo)), Calls(fakes));
        });
    }

    [Fact]
    public void ThePowerShellScriptStopsBeforeTheCheckoutWhenATrackedFileHasAChange()
    {
        RunPowerShellCase([], new Dictionary<string, string> { ["status.txt"] = " M Makefile" }, (result, repo, fakes) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains("has a change", result.Output + result.Errors, StringComparison.Ordinal);
            Assert.Equal(UpdateCalls(repo).Take(2), Calls(fakes));
        });
    }

    [Fact]
    public void ThePowerShellScriptWithNoUpdateBuildsAndPlaysTheCheckoutAsItIs()
    {
        RunPowerShellCase(["-NoUpdate"], new Dictionary<string, string>(), (result, repo, fakes) =>
        {
            Assert.True(result.Exit == 0, result.Output + result.Errors);
            Assert.Equal(BuildCalls(repo), Calls(fakes));
        });
    }

    [Fact]
    public void ThePowerShellScriptStopsAtAFailedImport()
    {
        RunPowerShellCase(["-NoUpdate"], new Dictionary<string, string> { ["import-fails"] = "" }, (result, repo, fakes) =>
        {
            Assert.Equal(1, result.Exit);
            Assert.Contains("the step 'import' failed with exit code 3", result.Output + result.Errors, StringComparison.Ordinal);
            Assert.Equal(BuildCalls(repo).Take(2), Calls(fakes));
        });
    }

    /// <summary>The git calls of the update, in order: the fetch, the status read, the checkout, the pull, and the log line.</summary>
    private static List<string> UpdateCalls(string repo) =>
    [
        $"git -C {repo} fetch origin main",
        $"git -C {repo} status --porcelain --untracked-files=no",
        $"git -C {repo} checkout main",
        $"git -C {repo} pull --ff-only origin main",
        $"git -C {repo} log -1 --oneline",
    ];

    /// <summary>The calls after the update, in order: the build, the import, and the play session.</summary>
    private static List<string> BuildCalls(string repo)
    {
        string game = Path.Combine(repo, "WhatYouCarry.Game");
        return
        [
            $"dotnet build {Path.Combine(repo, "WhatYouCarry.slnx")}",
            $"godot --headless --editor --path {game} --build-solutions --quit",
            $"godot --path {game}",
        ];
    }

    /// <summary>The lines of calls.log, or no line when no fake ran.</summary>
    private static List<string> Calls(string fakes)
    {
        string log = Path.Combine(fakes, "calls.log");
        return File.Exists(log) ? [.. File.ReadAllLines(log).Select(line => line.TrimEnd())] : [];
    }

    /// <summary>The value of one key of the env block of a workflow, as in <c>  GODOT_VERSION: 4.7.2-stable</c>.</summary>
    private static string WorkflowValue(string workflow, string key)
    {
        string prefix = $"  {key}: ";
        string? line = workflow.Split('\n').FirstOrDefault(text => text.StartsWith(prefix, StringComparison.Ordinal));
        Assert.True(line is not null, $"The smoke workflow has no line '{prefix.Trim()}'.");
        return line[prefix.Length..].Trim();
    }

    /// <summary>Copies the bash script into a temporary checkout, runs it with the fakes, and deletes the directory.</summary>
    private static void RunShellCase(string[] arguments, Dictionary<string, string> files, Action<(int Exit, string Output, string Errors), string, string, string> check, bool godotExists = true)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        string directory = CaseDirectory();
        try
        {
            string repo = Path.Combine(directory, "repo");
            string fakes = Path.Combine(directory, "fakes");
            string home = Path.Combine(directory, "home");
            string script = CopyScript(ShellScript, repo);
            File.SetUnixFileMode(script, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            Directory.CreateDirectory(fakes);
            Directory.CreateDirectory(home);
            foreach (string name in new[] { "git", "dotnet", "godot" })
            {
                string fake = Path.Combine(fakes, name);
                File.WriteAllText(fake, FakeShell.Replace("\r\n", "\n", StringComparison.Ordinal) + "\n");
                File.SetUnixFileMode(fake, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }

            WriteFiles(fakes, files);
            string godot = godotExists ? Path.Combine(fakes, "godot") : Path.Combine(fakes, "no-godot");
            ProcessStartInfo start = new("bash");
            start.ArgumentList.Add(script);
            check(Run(start, arguments, fakes, godot, home), PhysicalPath(repo), fakes, home);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>Copies the PowerShell script into a temporary checkout, runs it with the fakes, and deletes the directory.</summary>
    private static void RunPowerShellCase(string[] arguments, Dictionary<string, string> files, Action<(int Exit, string Output, string Errors), string, string> check)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        string directory = CaseDirectory();
        try
        {
            string repo = Path.Combine(directory, "repo");
            string fakes = Path.Combine(directory, "fakes");
            string script = CopyScript(PowerShellScript, repo);
            Directory.CreateDirectory(fakes);
            foreach (string name in new[] { "git", "dotnet", "godot" })
            {
                File.WriteAllText(Path.Combine(fakes, $"{name}.cmd"), FakeCommand.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "\r\n", StringComparison.Ordinal) + "\r\n");
            }

            WriteFiles(fakes, files);
            ProcessStartInfo start = new("powershell.exe");
            foreach (string part in new[] { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", script })
            {
                start.ArgumentList.Add(part);
            }

            check(Run(start, arguments, fakes, Path.Combine(fakes, "godot.cmd"), directory), Path.GetFullPath(repo), fakes);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    /// A new directory under the test output. The temporary directory of macOS sits behind a symlink, and the one of
    /// Windows can have a short 8.3 name, so a path there differs from the path that each script reads.
    /// </summary>
    private static string CaseDirectory()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "launcher-cases", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    /// <summary>The path with each symlink resolved, as the bash script reads its own checkout through <c>readlink -f</c>.</summary>
    private static string PhysicalPath(string directory)
    {
        ProcessStartInfo start = new("bash") { RedirectStandardOutput = true, UseShellExecute = false };
        foreach (string part in new[] { "-c", "cd \"$1\" && pwd -P", "bash", directory })
        {
            start.ArgumentList.Add(part);
        }

        using Process process = Process.Start(start) ?? throw new InvalidOperationException($"bash did not start to resolve '{directory}'.");
        string physical = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        Assert.True(process.ExitCode == 0 && physical.Length > 0, $"bash did not resolve the path '{directory}'.");
        return physical;
    }

    /// <summary>Copies one script of the checkout to the same relative path under a temporary checkout, and returns the copy.</summary>
    private static string CopyScript(string relativePath, string repo)
    {
        string copy = Path.Combine(repo, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(copy) ?? throw new InvalidOperationException($"The path '{copy}' has no directory."));
        File.Copy(Path.Combine(RepositoryRoot.Find(), relativePath), copy);
        return copy;
    }

    private static void WriteFiles(string directory, Dictionary<string, string> files)
    {
        foreach ((string name, string text) in files)
        {
            File.WriteAllText(Path.Combine(directory, name), text + "\n");
        }
    }

    /// <summary>Runs one script with the fakes first on the path and the fake Godot in WYC_GODOT, and returns the exit code and both outputs.</summary>
    private static (int Exit, string Output, string Errors) Run(ProcessStartInfo start, string[] arguments, string fakes, string godot, string home)
    {
        start.RedirectStandardError = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardInput = true;
        start.UseShellExecute = false;
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        string path = Environment.GetEnvironmentVariable("PATH") ?? throw new InvalidOperationException("The test process has no PATH variable.");
        start.Environment["PATH"] = fakes + Path.PathSeparator + path;
        start.Environment["WYC_GODOT"] = godot;
        start.Environment["HOME"] = home;
        start.Environment["LOCALAPPDATA"] = home;

        using Process process = Process.Start(start) ?? throw new InvalidOperationException($"The shell '{start.FileName}' did not start.");
        process.StandardInput.Close();
        System.Threading.Tasks.Task<string> errors = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output, errors.Result);
    }
}
