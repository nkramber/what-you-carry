using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Tools.TextureGen;

namespace WhatYouCarry.Tools.TextureTrace;

/// <summary>
/// <c>texture-trace --root &lt;checkout&gt; --spec &lt;name&gt;</c>: the trace of D-612. It reads the spec
/// <c>textures/traces/&lt;name&gt;.json</c>, the palette, the models, and the screenshots, and it writes one recipe of
/// a texel map for each face of the spec. A recipe file that exists already stays as it is, because a hand edit of a
/// map corrects a feature after the trace. Remove the file to trace that face again. Then run <c>texture-gen</c>.
/// </summary>
public static class TextureTraceCommand
{
    /// <summary>The whole run as an exit code: 0 when each face has its recipe, 1 on an input that is bad or unreadable, or on a write failure, 2 on a bad command line.</summary>
    public static int Run(string[] args)
    {
        string? root = null;
        string? spec = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--root" && i + 1 < args.Length)
            {
                root = args[i + 1];
                i++;
                continue;
            }

            if (args[i] == "--spec" && i + 1 < args.Length)
            {
                spec = args[i + 1];
                i++;
                continue;
            }

            Console.Error.WriteLine($"Unexpected argument '{args[i]}'. The options are --root <checkout> and --spec <name>, and every argument must belong to one of them.");
            return 2;
        }

        if (root is null || spec is null)
        {
            Console.Error.WriteLine("Both options are required: --root <checkout> and --spec <name>.");
            return 2;
        }

        try
        {
            (int written, int kept) = Trace(root, spec);
            Console.WriteLine($"texture-trace: wrote {Text(written)} recipe(s), and kept {Text(kept)} that exist already.");
            return 0;
        }
        catch (ContextException error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
    }

    /// <summary>Traces each face of one spec whose recipe file is absent, and writes the file.</summary>
    /// <returns>The count of recipe files written, and the count kept because they exist.</returns>
    /// <exception cref="ContextException">An input is absent, unreadable, or not valid, or a recipe file cannot be written.</exception>
    public static (int Written, int Kept) Trace(string root, string specName)
    {
        string contentRoot = Path.Combine(root, TextureGenCommand.ContentDirectoryName);
        Palette palette = Palette.Parse(AssetPaths.PaletteFile, ReadFile(Path.Combine(contentRoot, AssetPaths.PaletteFile)));
        string specPath = AssetPaths.TraceDirectory + specName + ".json";
        TraceSpec spec = TraceSpecFile.Parse(specPath, ReadFile(Path.Combine(contentRoot, specPath)), palette);

        Dictionary<string, BlockbenchModel> models = [];
        Dictionary<string, ScreenshotImage> images = [];
        int written = 0;
        int kept = 0;
        foreach (TraceFace face in spec.Faces)
        {
            string recipeFile = Path.Combine(contentRoot, AssetPaths.RecipeDirectory, face.Recipe + ".json");
            if (File.Exists(recipeFile))
            {
                Console.WriteLine($"texture-trace: kept {AssetPaths.RecipeDirectory}{face.Recipe}.json, which exists already.");
                kept++;
                continue;
            }

            ModelBox box = BoxOf(contentRoot, models, face);
            (int width, int height) = BoxFaces.CanvasTexels(box, face.Side);
            string imagePath = spec.Images[face.Image];
            if (!images.TryGetValue(face.Image, out ScreenshotImage? image))
            {
                image = ScreenshotPng.Read(imagePath, ReadFile(Path.Combine(root, imagePath)));
                images.Add(face.Image, image);
            }

            MapLayer map = TextureTracer.Trace(image, imagePath, face, width, height, palette);
            WriteFile(recipeFile, TextureTracer.RecipeText(map));
            written++;
        }

        return (written, kept);
    }

    private static ModelBox BoxOf(string contentRoot, Dictionary<string, BlockbenchModel> models, TraceFace face)
    {
        if (!models.TryGetValue(face.Model, out BlockbenchModel? model))
        {
            model = BlockbenchLoader.Parse(face.Model, ReadFile(Path.Combine(contentRoot, face.Model)));
            models.Add(face.Model, model);
        }

        return model.Box(face.Box) ?? throw new ContextException($"The trace names the box '{face.Box}', and the model '{face.Model}' has no box of that name.");
    }

    private static byte[] ReadFile(string file)
    {
        try
        {
            return File.ReadAllBytes(file);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new ContextException($"The file '{file}' could not be read. {error.Message}");
        }
    }

    private static void WriteFile(string file, string text)
    {
        try
        {
            File.WriteAllText(file, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new ContextException($"The recipe '{file}' could not be written. {error.Message}");
        }
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
