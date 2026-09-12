using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Core.Content;

namespace WhatYouCarry.Tests;

/// <summary>The content source of this checkout, which reads the real `content/` directory and skips the model directory (D-298).</summary>
internal sealed class RepositoryContentSource : IContentSource
{
    public IReadOnlyList<ContentFile> Read()
    {
        string root = Path.Combine(RepositoryRoot.Find(), "content");
        List<ContentFile> files = [];
        foreach (string file in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories))
        {
            string contentPath = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (contentPath.StartsWith(ContentLoader.ModelDirectory, System.StringComparison.Ordinal))
            {
                continue;
            }

            files.Add(new ContentFile(contentPath, File.ReadAllBytes(file)));
        }

        return files;
    }
}
