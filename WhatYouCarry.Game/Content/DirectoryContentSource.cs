using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Content;

/// <summary>
/// The content source of one directory on disk (D-219): every JSON file under it, with a forward-slash path
/// relative to the root. The Game layer opens the files, and Core opens none.
/// </summary>
/// <remarks>
/// PR-12 reads the content directory of the checkout, next to the project directory. PR-31 reads the content
/// directory of an exported build. An absent directory is an error that names the path (T-2).
/// </remarks>
public sealed class DirectoryContentSource : IContentSource
{
    /// <summary>The file pattern of a content file (D-91).</summary>
    public const string Pattern = "*.json";

    private const string AbsentDirectory = "The content directory does not exist.";
    private const string DirectoryField = "directory";

    private readonly string root;

    /// <summary>A source that reads one directory.</summary>
    public DirectoryContentSource(string root)
    {
        this.root = root;
    }

    /// <inheritdoc/>
    /// <exception cref="ContextException">The directory does not exist.</exception>
    public IReadOnlyList<ContentFile> Read()
    {
        if (!Directory.Exists(this.root))
        {
            ContextException error = new(AbsentDirectory);
            error.AddContext(DirectoryField, this.root);
            throw error;
        }

        List<ContentFile> files = [];
        foreach (string file in Directory.EnumerateFiles(this.root, Pattern, SearchOption.AllDirectories))
        {
            files.Add(new ContentFile(Path.GetRelativePath(this.root, file).Replace('\\', '/'), File.ReadAllBytes(file)));
        }

        return files;
    }
}
