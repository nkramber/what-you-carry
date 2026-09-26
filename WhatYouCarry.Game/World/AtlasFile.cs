using System.Globalization;
using System.IO;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The atlas file of every chunk and every model (D-85, D-305): the PNG file that the texture generator writes under
/// the content directory, read at boot into one texture. The file stores the color of each pixel (D-598), and every
/// color comes from the palette of D-304, so every material samples the colors of that palette.
/// </summary>
/// <remarks>
/// An absent file, a file that the engine cannot decode, and an image of another size are each an error that names
/// the file, and never a blank atlas that renders in silence (T-2). The smoke session reads the atlas at boot on
/// every platform.
/// </remarks>
public static class AtlasFile
{
    private const string AbsentFile = "The atlas file does not exist. The texture-gen command of Tools writes it.";
    private const string NotDecoded = "The engine could not decode the atlas file as a PNG image.";
    private const string WrongSize = "The atlas image does not have the size of the atlas layout.";
    private const string FileField = "file";
    private const string ErrorField = "error";
    private const string WidthField = "width";
    private const string HeightField = "height";
    private const string ExpectedField = "expected";

    /// <summary>The atlas texture from one content directory.</summary>
    /// <exception cref="ContextException">The file is absent, it does not decode, or its size is not the size of the atlas layout.</exception>
    public static ImageTexture Load(string contentDirectory)
    {
        string file = Path.Combine(contentDirectory, AssetPaths.AtlasImage);
        if (!File.Exists(file))
        {
            ContextException absent = new(AbsentFile);
            absent.AddContext(FileField, file);
            throw absent;
        }

        Image image = new();
        Error result = image.LoadPngFromBuffer(File.ReadAllBytes(file));
        if (result != Error.Ok)
        {
            ContextException failed = new(NotDecoded);
            failed.AddContext(FileField, file);
            failed.AddContext(ErrorField, result.ToString());
            throw failed;
        }

        if (image.GetWidth() != AtlasLayout.AtlasPixels || image.GetHeight() != AtlasLayout.AtlasPixels)
        {
            ContextException wrong = new(WrongSize);
            wrong.AddContext(FileField, file);
            wrong.AddContext(WidthField, image.GetWidth().ToString(CultureInfo.InvariantCulture));
            wrong.AddContext(HeightField, image.GetHeight().ToString(CultureInfo.InvariantCulture));
            wrong.AddContext(ExpectedField, AtlasLayout.AtlasPixels.ToString(CultureInfo.InvariantCulture));
            throw wrong;
        }

        return ImageTexture.CreateFromImage(image);
    }
}
