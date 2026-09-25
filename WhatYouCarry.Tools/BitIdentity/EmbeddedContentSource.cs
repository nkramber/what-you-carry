using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WhatYouCarry.Core.Content;

namespace WhatYouCarry.Tools.BitIdentity;

/// <summary>
/// The content of the checkout that the build puts into the tool (F-133, D-219). The project file embeds every JSON
/// file under <c>content/</c> that Core reads, with the logical name <c>content/</c> and the content path. The
/// bit-identity sweep then runs on the real content with no option and no open file, on every platform.
/// </summary>
/// <remarks>
/// The build of each platform reads the one checkout, and the checkout keeps one line ending (D-71), so the three
/// platforms load the same bytes.
/// </remarks>
public sealed class EmbeddedContentSource : IContentSource
{
    /// <summary>The start of the logical name of each embedded content file.</summary>
    public const string Prefix = "content/";

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The tool holds no content file, or a resource of the tool cannot be read (T-2).</exception>
    public IReadOnlyList<ContentFile> Read()
    {
        Assembly tool = typeof(EmbeddedContentSource).Assembly;
        List<ContentFile> files = [];
        foreach (string name in tool.GetManifestResourceNames())
        {
            // The build of Windows writes the directory part of the name with a backslash.
            string logical = name.Replace('\\', '/');
            if (!logical.StartsWith(Prefix, StringComparison.Ordinal))
            {
                continue;
            }

            string contentPath = logical.Substring(Prefix.Length);
            if (ContentLoader.IsAssetPath(contentPath))
            {
                continue;
            }

            using Stream? stream = tool.GetManifestResourceStream(name);
            if (stream is null)
            {
                throw new InvalidOperationException($"The tool lists the content resource '{name}', and it cannot read it.");
            }

            using MemoryStream bytes = new();
            stream.CopyTo(bytes);
            files.Add(new ContentFile(contentPath, bytes.ToArray()));
        }

        if (files.Count == 0)
        {
            throw new InvalidOperationException($"The tool holds no content file under '{Prefix}'. The project file embeds the JSON files of the content directory (F-133).");
        }

        return files;
    }
}
