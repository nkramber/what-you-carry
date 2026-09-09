using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Core.Content;

namespace WhatYouCarry.Tests;

/// <summary>The content source of this checkout, which reads the real `content/` directory.</summary>
internal sealed class RepositoryContentSource : IContentSource
{
    public IReadOnlyList<ContentFile> Read()
    {
        string root = Path.Combine(RepositoryRoot.Find(), "content");
        List<ContentFile> files = [];
        foreach (string file in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories))
        {
            files.Add(new ContentFile(Path.GetRelativePath(root, file).Replace('\\', '/'), File.ReadAllBytes(file)));
        }

        return files;
    }
}
