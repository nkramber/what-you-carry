using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>
/// <c>texture-gen --root &lt;checkout&gt;</c>: the texture generator (D-65, D-85, D-305). It reads the palette and every
/// rule file under the content directory, paints the atlas, and writes <c>textures/atlas.png</c>. A test holds the
/// committed atlas equal to the output, so run the command after each change to the palette or to a rule.
/// </summary>
public static class TextureGenCommand
{
    /// <summary>The content directory under the checkout root.</summary>
    public const string ContentDirectoryName = "content";

    /// <summary>The file pattern of a rule file.</summary>
    public const string RulePattern = "*.json";

    /// <summary>The whole run as an exit code: 0 when the atlas is written, 1 on a palette or a rule that is bad or unreadable, or on a write failure, 2 on a bad command line.</summary>
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
        byte[] atlas;
        try
        {
            atlas = AtlasBytes(contentRoot);
        }
        catch (ContextException error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }

        string output = Path.Combine(contentRoot, AssetPaths.AtlasImage);
        try
        {
            File.WriteAllBytes(output, atlas);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            // A path that the user cannot write raises the second kind, and it is not an IOException.
            Console.Error.WriteLine($"The atlas could not be written to '{output}'. {error.Message}");
            return 1;
        }

        Console.WriteLine($"texture-gen: wrote {AssetPaths.AtlasImage}, {Text(AtlasLayout.AtlasPixels)} by {Text(AtlasLayout.AtlasPixels)} pixels, {Text(atlas.Length)} bytes.");
        return 0;
    }

    /// <summary>The bytes of the atlas PNG that the palette and the rules under one content directory give.</summary>
    /// <exception cref="ContextException">The palette or the rule directory is absent, the user cannot read an input, or a file is not valid.</exception>
    public static byte[] AtlasBytes(string contentRoot)
    {
        Palette palette = Palette.Parse(AssetPaths.PaletteFile, ReadFile(contentRoot, AssetPaths.PaletteFile));
        IReadOnlyList<TextureRule> rules = ReadRules(contentRoot, palette);
        byte[] pixels = TextureGenerator.Paint(palette, rules);
        return PngWriter.Write(AtlasLayout.AtlasPixels, AtlasLayout.AtlasPixels, palette.Colors, pixels);
    }

    /// <summary>Every rule file of the rule directory, in ordinal order of the file name.</summary>
    /// <exception cref="ContextException">The directory is absent, the user cannot read it or a rule in it, it holds no rule file, or a rule is not valid.</exception>
    public static IReadOnlyList<TextureRule> ReadRules(string contentRoot, Palette palette)
    {
        string directory = Path.Combine(contentRoot, AssetPaths.RuleDirectory);
        if (!Directory.Exists(directory))
        {
            throw new ContextException($"The rule directory '{directory}' does not exist.");
        }

        try
        {
            string[] files = Directory.GetFiles(directory, RulePattern);
            Array.Sort(files, StringComparer.Ordinal);
            if (files.Length == 0)
            {
                throw new ContextException($"The rule directory '{directory}' holds no rule file, and the atlas needs one rule per tile that it paints (D-307).");
            }

            List<TextureRule> rules = [];
            foreach (string file in files)
            {
                string contentPath = AssetPaths.RuleDirectory + Path.GetFileName(file);
                rules.Add(TextureRule.Parse(contentPath, File.ReadAllBytes(file), palette));
            }

            return rules;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            // A directory or a file that the user cannot read raises the second kind, and it is not an IOException.
            // The message of the platform names the exact path, and the directory listing and each rule read share
            // this one boundary (PR #56 review P2-1).
            throw new ContextException($"The rule directory '{directory}' or a rule in it could not be read. {error.Message}", error);
        }
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
