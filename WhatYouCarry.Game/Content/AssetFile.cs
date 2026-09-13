using System.IO;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Content;

/// <summary>
/// Reads one asset file under the content directory of the checkout: a model or an animation (D-298, D-330). An absent
/// file is an error that names the path (T-2).
/// </summary>
public static class AssetFile
{
    private const string AbsentFile = "The asset file does not exist.";
    private const string FileField = "file";

    /// <summary>The bytes of one file under the content directory.</summary>
    /// <exception cref="ContextException">The file does not exist.</exception>
    public static byte[] Read(string contentDirectory, string contentPath)
    {
        string file = Path.Combine(contentDirectory, contentPath);
        if (!File.Exists(file))
        {
            ContextException error = new(AbsentFile);
            error.AddContext(FileField, file);
            throw error;
        }

        return File.ReadAllBytes(file);
    }
}
