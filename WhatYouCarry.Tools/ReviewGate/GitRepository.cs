using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace WhatYouCarry.Tools.ReviewGate;

/// <summary>A commit and its committer time.</summary>
public sealed record CommitStamp(string Sha, DateTimeOffset CommitTime);

/// <summary>Runs git in one checkout. Every failure carries the command, the exit code, and stderr (T-2).</summary>
public sealed class GitRepository
{
    private readonly string path;

    public GitRepository(string path)
    {
        this.path = path;
    }

    /// <summary>Returns the file content at a revision, or null when the path is absent at that revision.</summary>
    public string? ReadFileOrNull(string revision, string filePath)
    {
        // ls-tree prints nothing for an absent path and exits 0. It still fails on a bad revision.
        string entry = Run(["ls-tree", revision, "--", filePath]);
        if (entry.Trim().Length == 0)
        {
            return null;
        }

        return Run(["show", $"{revision}:{filePath}"]);
    }

    public string MergeBase(string first, string second)
    {
        return Run(["merge-base", first, second]).Trim();
    }

    /// <summary>The newest commit in the range that changes a path outside the excluded paths, or null when no commit does.</summary>
    public CommitStamp? NewestCommitOutside(string mergeBase, string head, IReadOnlyList<string> excludedPaths)
    {
        var args = new List<string> { "log", "-1", "--format=%H %cI", $"{mergeBase}..{head}", "--", "." };
        foreach (string excluded in excludedPaths)
        {
            args.Add($":(exclude){excluded}");
        }

        string line = Run(args).Trim();
        if (line.Length == 0)
        {
            return null;
        }

        string[] parts = line.Split(' ');
        if (parts.Length != 2)
        {
            throw new InvalidOperationException($"git log returned '{line}', and the expected form is '<sha> <ISO 8601 time>'.");
        }

        DateTimeOffset time = DateTimeOffset.Parse(parts[1], CultureInfo.InvariantCulture);
        return new CommitStamp(parts[0], time);
    }

    public IReadOnlyList<string> ChangedPaths(string mergeBase, string head)
    {
        string output = Run(["diff", "--name-only", mergeBase, head]);
        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>Runs git and returns stdout. Throws when git exits with a nonzero code.</summary>
    public string Run(IReadOnlyList<string> args)
    {
        GitResult result = Execute(args);
        result.ThrowIfFailed();
        return result.StandardOutput;
    }

    private GitResult Execute(IReadOnlyList<string> args)
    {
        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = path,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (string arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"git did not start in '{path}'.");
        string standardOutput = process.StandardOutput.ReadToEnd();
        string standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return new GitResult(path, string.Join(' ', args), process.ExitCode, standardOutput, standardError);
    }

    private sealed record GitResult(string Path, string Command, int ExitCode, string StandardOutput, string StandardError)
    {
        public void ThrowIfFailed()
        {
            if (ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"'git {Command}' exited {ExitCode} in '{Path}'. stderr: {StandardError.Trim()}");
            }
        }
    }
}
