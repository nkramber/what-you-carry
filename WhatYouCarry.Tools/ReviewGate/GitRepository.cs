using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace WhatYouCarry.Tools.ReviewGate;

/// <summary>A commit and its committer time.</summary>
public sealed record CommitStamp(string Sha, DateTimeOffset CommitTime);

/// <summary>A commit and its subject line.</summary>
public sealed record CommitSubject(string Sha, string Subject);

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

    /// <summary>
    /// The newest commit in the range that changes a path outside the excluded paths, or null when no commit does. An
    /// entry that ends in a slash excludes each path under it, and any other entry excludes the one file of that name,
    /// as <c>CiSkipRules.IsDocument</c> reads the entries (F-125).
    /// </summary>
    /// <exception cref="InvalidOperationException">The two candidate commits lie on separate lines of the history, so neither is the newer one.</exception>
    public CommitStamp? NewestCommitOutside(string mergeBase, string head, IReadOnlyList<string> excludedPaths)
    {
        // A git pathspec of a file name also matches a directory of that name, so ':(exclude)LICENSE' hides the path
        // 'LICENSE/evil.cs' too. A second walk finds the paths under a directory that has the name of an excluded file.
        var outsidePathspecs = new List<string> { "." };
        var fileNamedDirectoryPathspecs = new List<string>();
        var directoryExclusions = new List<string>();
        foreach (string excluded in excludedPaths)
        {
            outsidePathspecs.Add($":(exclude){excluded}");
            if (excluded.EndsWith('/'))
            {
                directoryExclusions.Add($":(exclude){excluded}");
            }
            else
            {
                fileNamedDirectoryPathspecs.Add(excluded + "/");
            }
        }

        CommitStamp? outside = NewestCommitIn(mergeBase, head, outsidePathspecs);
        if (fileNamedDirectoryPathspecs.Count == 0)
        {
            return outside;
        }

        fileNamedDirectoryPathspecs.AddRange(directoryExclusions);
        CommitStamp? underFileNamedDirectory = NewestCommitIn(mergeBase, head, fileNamedDirectoryPathspecs);
        return Newer(outside, underFileNamedDirectory, mergeBase, head);
    }

    /// <summary>The newest commit in the range that changes a path of the pathspecs, or null when no commit does.</summary>
    private CommitStamp? NewestCommitIn(string mergeBase, string head, IReadOnlyList<string> pathspecs)
    {
        var args = new List<string> { "log", "-1", "--format=%H %cI", $"{mergeBase}..{head}", "--" };
        args.AddRange(pathspecs);
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

    /// <summary>
    /// The later of two commits of the range by ancestry, or the one that is not null. The commit time does not decide,
    /// because a rebase can keep the time of an older commit.
    /// </summary>
    private CommitStamp? Newer(CommitStamp? first, CommitStamp? second, string mergeBase, string head)
    {
        if (first is null || second is null)
        {
            return first ?? second;
        }

        if (IsAncestor(first.Sha, second.Sha))
        {
            return second;
        }

        if (IsAncestor(second.Sha, first.Sha))
        {
            return first;
        }

        throw new InvalidOperationException(
            $"The commits {first.Sha} and {second.Sha} lie on separate lines of the history from {mergeBase} to {head}, so neither is the newest commit outside the excluded paths.");
    }

    /// <summary>The newest commit up to the head that changes the path, with its subject, or null when no commit does.</summary>
    public CommitSubject? NewestCommitThatChanged(string head, string filePath)
    {
        string line = Run(["log", "-1", "--format=%H %s", head, "--", filePath]).Trim();
        if (line.Length == 0)
        {
            return null;
        }

        int space = line.IndexOf(' ');
        string sha = space < 0 ? line : line[..space];
        string subject = space < 0 ? string.Empty : line[(space + 1)..];
        return new CommitSubject(sha, subject);
    }

    /// <summary>
    /// Every path that changes from the first revision to the second. A move lists both paths, because rename
    /// detection would hide the old path, and a code file moved into <c>docs/</c> would then read as a document.
    /// </summary>
    public IReadOnlyList<string> ChangedPaths(string mergeBase, string head)
    {
        string output = Run(["diff", "--name-only", "--no-renames", mergeBase, head]);
        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>True when the checkout holds the commit. git exits 1 for an absent object and 128 for an absent name, and both mean no.</summary>
    public bool HasCommit(string sha)
    {
        GitResult result = Execute(["cat-file", "-e", $"{sha}^{{commit}}"]);
        result.ThrowUnless(0, 1, 128);
        return result.ExitCode == 0;
    }

    /// <summary>
    /// True when the remote has the branch. git exits 2 when no ref matches, and any other failure is an error (T-2).
    /// The full ref name keeps a branch such as <c>archive/night-results</c> from a match on its last part (F-125).
    /// </summary>
    public bool HasRemoteBranch(string remote, string branch)
    {
        GitResult result = Execute(["ls-remote", "--exit-code", "--heads", remote, $"refs/heads/{branch}"]);
        result.ThrowUnless(0, 2);
        return result.ExitCode == 0;
    }

    /// <summary>
    /// Fetches one branch of a remote into FETCH_HEAD. Any failure is an error with the command and stderr (T-2). The
    /// full ref name fetches the branch, because git reads a short name as a tag first, and a tag of the same name
    /// would take the place of the branch (F-125).
    /// </summary>
    public void Fetch(string remote, string branch)
    {
        Run(["fetch", "--quiet", remote, $"refs/heads/{branch}"]);
    }

    /// <summary>True when the commit is the revision or an ancestor of it. git exits 1 when it is not.</summary>
    public bool IsAncestor(string commit, string revision)
    {
        GitResult result = Execute(["merge-base", "--is-ancestor", commit, revision]);
        result.ThrowUnless(0, 1);
        return result.ExitCode == 0;
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
        // The two streams are read at the same time. git blocks on a full stderr pipe, so a read of stdout to its end
        // first never ends (F-125).
        Task<string> standardError = process.StandardError.ReadToEndAsync();
        string standardOutput = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return new GitResult(path, string.Join(' ', args), process.ExitCode, standardOutput, standardError.Result);
    }

    private sealed record GitResult(string Path, string Command, int ExitCode, string StandardOutput, string StandardError)
    {
        public void ThrowIfFailed()
        {
            ThrowUnless(0);
        }

        /// <summary>Throws with the command, the exit code, and stderr when the exit code is not one of the given codes (T-2).</summary>
        public void ThrowUnless(params int[] allowedExitCodes)
        {
            if (Array.IndexOf(allowedExitCodes, ExitCode) < 0)
            {
                throw new InvalidOperationException(
                    $"'git {Command}' exited {ExitCode} in '{Path}'. stderr: {StandardError.Trim()}");
            }
        }
    }
}
