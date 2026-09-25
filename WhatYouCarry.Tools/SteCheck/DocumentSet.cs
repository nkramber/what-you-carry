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

    /// <summary>The extensions of a Markdown file. The match ignores the letter case, on each platform (F-140).</summary>
    public static readonly string[] MarkdownExtensions = [".md", ".markdown"];

    /// <summary>Build output and tool caches, skipped at any depth. No hand-written file lives under one of these names.</summary>
    public static readonly string[] SkippedDirectoryNames = [".git", "bin", "obj", ".godot"];

    /// <summary>
    /// Directories skipped by their path from the root. A worktree under the checkout holds a copy of each document,
    /// and the check of its own checkout reads it (F-140).
    /// </summary>
    public static readonly string[] SkippedDirectoryPaths = [".claude/worktrees"];

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

    /// <summary>True when the file name ends in a Markdown extension, in any letter case.</summary>
    public static bool IsMarkdown(string fileName)
    {
        string extension = Path.GetExtension(fileName);
        return Array.Exists(MarkdownExtensions, markdown => extension.Equals(markdown, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Every non-exempt Markdown file under the root, as a sorted list of repository-relative paths with forward slashes.</summary>
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
        foreach (string file in Directory.GetFiles(directory))
        {
            string relativePath = ToRelativePath(root, file);
            if (IsMarkdown(file) && !IsExempt(relativePath))
            {
                paths.Add(relativePath);
            }
        }

        foreach (string child in Directory.GetDirectories(directory))
        {
            bool skippedByName = Array.IndexOf(SkippedDirectoryNames, Path.GetFileName(child)) >= 0;
            bool skippedByPath = Array.IndexOf(SkippedDirectoryPaths, ToRelativePath(root, child)) >= 0;
            if (skippedByName || skippedByPath)
            {
                continue;
            }

            Collect(root, child, paths);
        }
    }
}
