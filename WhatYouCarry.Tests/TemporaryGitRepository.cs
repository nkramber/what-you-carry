using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace WhatYouCarry.Tests;

/// <summary>A throwaway git repository under the temp directory. Dispose deletes it.</summary>
public sealed class TemporaryGitRepository : IDisposable
{
    public string Path { get; }

    public TemporaryGitRepository()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "wyc-review-gate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
        Git(["init", "-q", "-b", "main"]);
    }

    /// <summary>Writes each file, stages everything, and commits. Returns the new commit hash.</summary>
    public string Commit(string message, IReadOnlyDictionary<string, string> files, DateTimeOffset? time = null)
    {
        foreach ((string relativePath, string content) in files)
        {
            string fullPath = System.IO.Path.Combine(Path, relativePath);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath)
                ?? throw new InvalidOperationException($"'{fullPath}' has no directory."));
            File.WriteAllText(fullPath, content);
        }

        Git(["add", "--all"]);
        string stamp = (time ?? DateTimeOffset.Parse("2026-09-07T10:00:00Z", System.Globalization.CultureInfo.InvariantCulture)).ToString("O");
        Git(["commit", "-q", "--allow-empty", "-m", message], stamp);
        return Git(["rev-parse", "HEAD"]).Trim();
    }

    public void CreateBranch(string name)
    {
        Git(["checkout", "-q", "-b", name]);
    }

    public string Git(IReadOnlyList<string> args, string? commitTime = null)
    {
        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = Path,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (string configuration in new[] { "user.name=Test", "user.email=test@example.invalid", "commit.gpgsign=false", "core.autocrlf=false" })
        {
            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add(configuration);
        }

        foreach (string arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        if (commitTime is not null)
        {
            startInfo.Environment["GIT_AUTHOR_DATE"] = commitTime;
            startInfo.Environment["GIT_COMMITTER_DATE"] = commitTime;
        }

        using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("git did not start.");
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"'git {string.Join(' ', args)}' exited {process.ExitCode} in '{Path}'. stderr: {error.Trim()}");
        }

        return output;
    }

    public void Dispose()
    {
        if (!Directory.Exists(Path))
        {
            return;
        }

        // Git marks its object files read-only, and Directory.Delete refuses a read-only file on Windows.
        foreach (string file in Directory.EnumerateFiles(Path, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(Path, recursive: true);
    }
}
