using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Core.Content;

namespace WhatYouCarry.Tests;

/// <summary>The content source of this checkout, which reads the real `content/` directory and skips the model and texture directories (D-298, D-305).</summary>
internal sealed class RepositoryContentSource : IContentSource
{
    public IReadOnlyList<ContentFile> Read()
    {
        string root = Path.Combine(RepositoryRoot.Find(), "content");
        List<ContentFile> files = [];
        foreach (string file in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories))
        {
            string contentPath = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (ContentLoader.IsAssetPath(contentPath))
            {
                continue;
            }

            files.Add(new ContentFile(contentPath, File.ReadAllBytes(file)));
        }

        return files;
    }
}
