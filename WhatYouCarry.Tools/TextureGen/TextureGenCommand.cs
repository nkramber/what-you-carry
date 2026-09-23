using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>The two files of one generator run: the atlas PNG and the text of the texture layout.</summary>
public sealed record GeneratorOutput(byte[] Atlas, string Layout);

/// <summary>
/// <c>texture-gen --root &lt;checkout&gt;</c>: the texture generator (D-65, D-85, D-305, D-505). It reads the palette, every
/// recipe, the block file, and the paint file of each model under the content directory. It paints the atlas and
/// writes <c>textures/atlas.png</c> and <c>textures/layout.json</c>. A test holds both committed files equal to the
/// output, so run the command after each change to the palette, a recipe, a binding, or a model.
/// </summary>
public static class TextureGenCommand
{
    /// <summary>The content directory under the checkout root.</summary>
    public const string ContentDirectoryName = "content";

    /// <summary>The file pattern of a recipe file.</summary>
    public const string RecipePattern = "*.json";

    /// <summary>The whole run as an exit code: 0 when both files are written, 1 on an input that is bad or unreadable, or on a write failure, 2 on a bad command line.</summary>
    public static int Run(string[] args)
    {
        string? root = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--root" && i + 1 < args.Length)
            {
                root = args[i + 1];
                i++;
                continue;
            }

            Console.Error.WriteLine($"Unexpected argument '{args[i]}'. The only option is --root <checkout>, and every argument must belong to it.");
            return 2;
        }

        if (root is null)
        {
            Console.Error.WriteLine("The option is required: --root <checkout>.");
            return 2;
        }

        string contentRoot = Path.Combine(root, ContentDirectoryName);
        GeneratorOutput output;
        try
        {
            output = Generate(contentRoot);
        }
        catch (ContextException error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }

        string atlasFile = Path.Combine(contentRoot, AssetPaths.AtlasImage);
        string layoutFile = Path.Combine(contentRoot, AssetPaths.LayoutFile);
        try
        {
            File.WriteAllBytes(atlasFile, output.Atlas);
            File.WriteAllText(layoutFile, output.Layout, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            // A path that the user cannot write raises the second kind, and it is not an IOException.
            Console.Error.WriteLine($"The atlas '{atlasFile}' or the layout '{layoutFile}' could not be written. {error.Message}");
            return 1;
        }

        Console.WriteLine($"texture-gen: wrote {AssetPaths.AtlasImage}, {Text(AtlasLayout.AtlasPixels)} by {Text(AtlasLayout.AtlasPixels)} pixels, {Text(output.Atlas.Length)} bytes, and {AssetPaths.LayoutFile}.");
        return 0;
    }

    /// <summary>The atlas PNG and the layout text that the inputs under one content directory give.</summary>
    /// <exception cref="ContextException">An input is absent, the user cannot read it, or a file is not valid.</exception>
    public static GeneratorOutput Generate(string contentRoot)
    {
        Palette palette = Palette.Parse(AssetPaths.PaletteFile, ReadFile(contentRoot, AssetPaths.PaletteFile));
        IReadOnlyDictionary<string, Recipe> recipes = RecipeFile.ReadAll(ReadRecipeFiles(contentRoot), palette);
        IReadOnlyList<BlockPaint> blocks = PaintFile.ReadBlocks(AssetPaths.BlockPaintFile, ReadFile(contentRoot, AssetPaths.BlockPaintFile), recipes);
        IReadOnlyList<ModelPaint> models = ReadModelPaints(contentRoot, recipes);
        AtlasResult result = TextureGenerator.Generate(palette, recipes, blocks, models);
        byte[] atlas = PngWriter.Write(AtlasLayout.AtlasPixels, AtlasLayout.AtlasPixels, palette.Colors, result.Pixels);
        return new GeneratorOutput(atlas, result.Layout.ToJson());
    }

    /// <summary>Every recipe file of the recipe directory, with its content path, in ordinal order of the file name.</summary>
    /// <exception cref="ContextException">The directory is absent, the user cannot read it or a file in it, or it holds no recipe file.</exception>
    public static IReadOnlyList<(string Path, byte[] Bytes)> ReadRecipeFiles(string contentRoot)
    {
        string directory = Path.Combine(contentRoot, AssetPaths.RecipeDirectory);
        if (!Directory.Exists(directory))
        {
            throw new ContextException($"The recipe directory '{directory}' does not exist.");
        }

        try
        {
            string[] files = Directory.GetFiles(directory, RecipePattern);
            Array.Sort(files, StringComparer.Ordinal);
            if (files.Length == 0)
            {
                throw new ContextException($"The recipe directory '{directory}' holds no recipe file, and each block and each face names a recipe (D-505).");
            }

            List<(string Path, byte[] Bytes)> read = [];
            foreach (string file in files)
            {
                read.Add((AssetPaths.RecipeDirectory + Path.GetFileName(file), File.ReadAllBytes(file)));
            }

            return read;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            // A directory or a file that the user cannot read raises the second kind, and it is not an IOException.
            // The message of the platform names the exact path (PR #56 review P2-1).
            throw new ContextException($"The recipe directory '{directory}' or a recipe in it could not be read. {error.Message}", error);
        }
    }

    /// <summary>
    /// The paint of each model file at the top of the model directory, in ordinal order of the path. Each model has a
    /// paint file, and each paint file has a model (D-508).
    /// </summary>
    /// <exception cref="ContextException">A model has no paint file, a paint file has no model, or a file is absent, unreadable, or not valid.</exception>
    public static IReadOnlyList<ModelPaint> ReadModelPaints(string contentRoot, IReadOnlyDictionary<string, Recipe> recipes)
    {
        string directory = Path.Combine(contentRoot, AssetPaths.ModelDirectory);
        string[] models;
        string[] paints;
        try
        {
            models = Directory.Exists(directory) ? Directory.GetFiles(directory, "*" + AssetPaths.ModelExtension) : [];
            paints = Directory.Exists(directory) ? Directory.GetFiles(directory, "*" + AssetPaths.PaintExtension) : [];
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new ContextException($"The model directory '{directory}' could not be read. {error.Message}", error);
        }

        Array.Sort(models, StringComparer.Ordinal);
        HashSet<string> paintPaths = [];
        foreach (string paint in paints)
        {
            paintPaths.Add(AssetPaths.ModelDirectory + Path.GetFileName(paint));
        }

        List<ModelPaint> read = [];
        foreach (string file in models)
        {
            string modelPath = AssetPaths.ModelDirectory + Path.GetFileName(file);
            string paintPath = AssetPaths.PaintPath(modelPath);
            if (!paintPaths.Remove(paintPath))
            {
                throw new ContextException($"The model '{modelPath}' has no paint file '{paintPath}', and each model names the recipe of each box (D-508).");
            }

            BlockbenchModel model = BlockbenchLoader.Parse(modelPath, ReadFile(contentRoot, modelPath));
            read.Add(PaintFile.ReadModel(paintPath, ReadFile(contentRoot, paintPath), model, recipes));
        }

        if (paintPaths.Count > 0)
        {
            string[] orphans = [.. paintPaths];
            Array.Sort(orphans, StringComparer.Ordinal);
            throw new ContextException($"The paint file '{orphans[0]}' names no model file of the model directory. Remove it, or add the model (D-508).");
        }

        return read;
    }

    /// <summary>The bytes of one file under the content directory. An absent file, and a file that the user cannot read, are each an error that names the path (T-2).</summary>
    private static byte[] ReadFile(string contentRoot, string contentPath)
    {
        string file = Path.Combine(contentRoot, contentPath);
        if (!File.Exists(file))
        {
            throw new ContextException($"The file '{file}' does not exist.");
        }

        try
        {
            return File.ReadAllBytes(file);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            // A file that the user cannot read raises the second kind, and it is not an IOException (PR #56 review P2-1).
            throw new ContextException($"The file '{file}' could not be read. {error.Message}", error);
        }
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
