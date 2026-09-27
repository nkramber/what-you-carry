using System.Globalization;
using System.IO;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The atlas file of every chunk and every model (D-85, D-305): the PNG file that the texture generator writes under
/// the content directory, read at boot into one image. The file stores the color of each pixel (D-598), and every
/// color comes from the palette of D-304, so every material samples the colors of that palette. The models read the
/// image as one texture with mipmaps (D-677). The chunks read a copy of the block canvases (<see cref="BlockAtlas"/>).
/// </summary>
/// <remarks>
/// An absent file, a file that the engine cannot decode, an image of another size, and a failure of the mipmaps are
/// each an error that names the file, and never a blank atlas that renders in silence (T-2). The smoke session reads the atlas at boot on
/// every platform.
/// </remarks>
public static class AtlasFile
{
    private const string AbsentFile = "The atlas file does not exist. The texture-gen command of Tools writes it.";
    private const string NotDecoded = "The engine could not decode the atlas file as a PNG image.";
    private const string WrongSize = "The atlas image does not have the size of the atlas layout.";
    private const string NoMipmaps = "The engine could not make the mipmaps of the atlas image.";
    private const string FileField = "file";
    private const string ErrorField = "error";
    private const string WidthField = "width";
    private const string HeightField = "height";
    private const string ExpectedField = "expected";

    /// <summary>The atlas image from one content directory, with no mipmaps.</summary>
    /// <exception cref="ContextException">The file is absent, it does not decode, or its size is not the size of the atlas layout.</exception>
    public static Image Load(string contentDirectory)
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

        return image;
    }

    /// <summary>
    /// The texture of every model: a copy of the atlas image with mipmaps, so a distant face does not shimmer (D-677).
    /// The gutter of each canvas repeats its edge, so mipmap level 1 keeps each canvas apart. A model face reads level 2
    /// only past about 30 meters at the 800 rows of the Deck, far past the lantern range (D-678).
    /// </summary>
    /// <exception cref="ContextException">The engine could not make the mipmaps.</exception>
    public static ImageTexture ModelTexture(Image atlas, string contentDirectory)
    {
        Image copy = (Image)atlas.Duplicate();
        Error result = copy.GenerateMipmaps();
        if (result != Error.Ok)
        {
            ContextException failed = new(NoMipmaps);
            failed.AddContext(FileField, Path.Combine(contentDirectory, AssetPaths.AtlasImage));
            failed.AddContext(ErrorField, result.ToString());
            throw failed;
        }

        return ImageTexture.CreateFromImage(copy);
    }
}
