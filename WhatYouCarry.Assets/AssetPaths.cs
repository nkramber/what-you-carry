using WhatYouCarry.Core.Content;

namespace WhatYouCarry.Assets;

/// <summary>
/// The places of the asset files under the content directory (D-298, D-299, D-300, D-305). Every path here is
/// relative to the content directory, with forward slashes.
/// </summary>
public static class AssetPaths
{
    /// <summary>The directory of the models and the animations, which every content source skips (D-298).</summary>
    public const string ModelDirectory = ContentLoader.ModelDirectory;

    /// <summary>The directory of the armor overlays, under the model directory (D-300).</summary>
    public const string ArmorDirectory = ModelDirectory + "armor/";

    /// <summary>The shared base body that every armor overlay covers (D-82, D-300, OQ-159).</summary>
    public const string BodyModel = ModelDirectory + "player.bbmodel";

    /// <summary>The file extension that Blockbench writes for a project (OQ-159).</summary>
    public const string ModelExtension = ContentLoader.ModelExtension;

    /// <summary>The file extension of an animation (D-298).</summary>
    public const string AnimationExtension = ContentLoader.AnimationExtension;

    /// <summary>The character between the model stem and the animation name in an animation file name (D-298).</summary>
    public const char AnimationSeparator = '.';

    /// <summary>The directory of the palette, the texture rules, and the atlas, which every content source skips (D-305).</summary>
    public const string TextureDirectory = ContentLoader.TextureDirectory;

    /// <summary>The palette of the atlas: ramps of colors from dark to light (D-304, D-305).</summary>
    public const string PaletteFile = TextureDirectory + "palette.json";

    /// <summary>The directory of the texture rules, one file per material (D-307).</summary>
    public const string RuleDirectory = TextureDirectory + "rules/";

    /// <summary>The atlas image that the texture generator writes and the game reads at boot (D-305).</summary>
    public const string AtlasImage = TextureDirectory + "atlas.png";

    /// <summary>The directory of the audio, which every content source skips (D-453).</summary>
    public const string AudioDirectory = ContentLoader.AudioDirectory;

    /// <summary>The directory of the sound effects: one parameter file and one rendered WAV file per sound (D-453, D-462).</summary>
    public const string SoundDirectory = AudioDirectory + "sfx/";

    /// <summary>The directory of the CC0 recordings that a recording layer plays (D-467).</summary>
    public const string RecordingDirectory = AudioDirectory + "recordings/";

    /// <summary>The file extension of a sound parameter file (D-462).</summary>
    public const string SoundParameterExtension = ".json";

    /// <summary>The file extension of a rendered sound (D-453).</summary>
    public const string SoundExtension = ".wav";

    /// <summary>The path of the rendered sound of one parameter file: <c>audio/sfx/footstep.wav</c> for <c>audio/sfx/footstep.json</c> (D-453).</summary>
    public static string SoundPath(string parameterPath)
    {
        return parameterPath.EndsWith(SoundParameterExtension, System.StringComparison.Ordinal)
            ? parameterPath.Substring(0, parameterPath.Length - SoundParameterExtension.Length) + SoundExtension
            : parameterPath + SoundExtension;
    }

    /// <summary>The path of one animation of one model: <c>models/player.attack.json</c> for the model <c>models/player.bbmodel</c> (D-298).</summary>
    public static string AnimationPath(string modelPath, string animationName)
    {
        return ModelStem(modelPath) + AnimationSeparator + animationName + AnimationExtension;
    }

    /// <summary>The model path without its extension.</summary>
    public static string ModelStem(string modelPath)
    {
        return modelPath.EndsWith(ModelExtension, System.StringComparison.Ordinal)
            ? modelPath.Substring(0, modelPath.Length - ModelExtension.Length)
            : modelPath;
    }
}
