using System.IO;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The texture layout file of every chunk and every model (D-505): the place of each block canvas and each face canvas
/// in the atlas, which the texture generator writes beside the atlas. The game reads it at boot, next to the atlas.
/// </summary>
/// <remarks>An absent file and a file that does not parse are each an error that names the file, and never a layout that places nothing (T-2).</remarks>
public static class TextureLayoutFile
{
    private const string AbsentFile = "The texture layout file does not exist. The texture-gen command of Tools writes it.";
    private const string FileField = "file";

    /// <summary>The texture layout from one content directory.</summary>
    /// <exception cref="ContextException">The file is absent, or it is not a valid layout.</exception>
    public static TextureLayout Load(string contentDirectory)
    {
        string file = Path.Combine(contentDirectory, AssetPaths.LayoutFile);
        if (!File.Exists(file))
        {
            ContextException absent = new(AbsentFile);
            absent.AddContext(FileField, file);
            throw absent;
        }

        return TextureLayout.Parse(AssetPaths.LayoutFile, File.ReadAllBytes(file));
    }
}
