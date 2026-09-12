using WhatYouCarry.Core.Content;

namespace WhatYouCarry.Assets;

/// <summary>
/// The places of the asset files under the content directory (D-298, D-299, D-300). Every path here is relative
/// to the content directory, with forward slashes.
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
    public const string ModelExtension = ".bbmodel";

    /// <summary>The file extension of an animation (D-298).</summary>
    public const string AnimationExtension = ".json";

    /// <summary>The character between the model stem and the animation name in an animation file name (D-298).</summary>
    public const char AnimationSeparator = '.';

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
