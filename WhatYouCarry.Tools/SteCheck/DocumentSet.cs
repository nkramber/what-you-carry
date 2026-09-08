using System;
using System.Collections.Generic;
using System.IO;

namespace WhatYouCarry.Tools.SteCheck;

/// <summary>The hand-written Markdown files that the checks read, and the paths that are exempt (D-139).</summary>
public static class DocumentSet
{
    public const string DecisionsPath = "docs/decisions.md";
    public const string HandoffPath = "docs/session-handoff.md";

    /// <summary>Dated records. A file under one of these paths is history, and no check reads it.</summary>
    public static readonly string[] ExemptPaths = ["docs/reviews/", "docs/session-handoff.md", "docs/session-handoff-archive.md", "docs/archive/"];

    /// <summary>Build output and tool caches. No hand-written file lives under one of these names.</summary>
    private static readonly string[] SkippedDirectoryNames = [".git", "bin", "obj", ".godot"];

    public static bool IsExempt(string relativePath)
    {
        foreach (string exempt in ExemptPaths)
        {
            if (exempt.EndsWith('/') && relativePath.StartsWith(exempt, StringComparison.Ordinal))
            {
                return true;
            }

            if (relativePath == exempt)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Every non-exempt <c>.md</c> file under the root, as a sorted list of repository-relative paths with forward slashes.</summary>
    public static List<string> Enumerate(string root)
    {
        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException($"The root directory '{root}' does not exist.");
        }

        var paths = new List<string>();
        Collect(root, root, paths);
        paths.Sort(StringComparer.Ordinal);
        return paths;
    }

    public static string ToRelativePath(string root, string fullPath)
    {
        return Path.GetRelativePath(root, fullPath).Replace(Path.DirectorySeparatorChar, '/');
    }

    private static void Collect(string root, string directory, List<string> paths)
    {
        foreach (string file in Directory.GetFiles(directory, "*.md"))
        {
            string relativePath = ToRelativePath(root, file);
            if (!IsExempt(relativePath))
            {
                paths.Add(relativePath);
            }
        }

        foreach (string child in Directory.GetDirectories(directory))
        {
            if (Array.IndexOf(SkippedDirectoryNames, Path.GetFileName(child)) >= 0)
            {
                continue;
            }

            Collect(root, child, paths);
        }
    }
}
