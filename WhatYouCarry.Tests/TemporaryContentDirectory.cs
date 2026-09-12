using System;
using System.IO;

namespace WhatYouCarry.Tests;

/// <summary>A throwaway content directory under the temp directory. Dispose deletes it.</summary>
public sealed class TemporaryContentDirectory : IDisposable
{
    /// <summary>The checkout root: the content directory is <c>content/</c> under it.</summary>
    public string Root { get; }

    /// <summary>The content directory.</summary>
    public string Content { get; }

    public TemporaryContentDirectory()
    {
        Root = Path.Combine(Path.GetTempPath(), "wyc-asset-qa-" + Guid.NewGuid().ToString("N"));
        Content = Path.Combine(Root, "content");
        Directory.CreateDirectory(Content);
    }

    /// <summary>Writes one file under the content directory. The path uses forward slashes.</summary>
    public void Write(string contentPath, string text)
    {
        string fullPath = Path.Combine(Content, contentPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException($"'{fullPath}' has no directory."));
        File.WriteAllText(fullPath, text);
    }

    public void Dispose()
    {
        Directory.Delete(Root, recursive: true);
    }
}
