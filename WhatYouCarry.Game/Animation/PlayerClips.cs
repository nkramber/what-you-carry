using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Content;

namespace WhatYouCarry.Game.Animation;

/// <summary>
/// The three clips of the player body (D-331): the swing of the main weapon, the roll, and the stagger. The swing clip
/// comes from the path in the weapon file (D-334), and the other two stand next to the body model (D-298).
/// </summary>
/// <param name="Swing">The swing clip, with the windup, active, and recovery phases of the weapon (D-87).</param>
/// <param name="Dodge">The roll clip, over the ticks of the roll (D-327).</param>
/// <param name="Stagger">The stagger clip, over the ticks of the stagger (D-326).</param>
public sealed record PlayerClips(AnimationClip Swing, AnimationClip Dodge, AnimationClip Stagger)
{
    /// <summary>The animation name of the roll clip.</summary>
    public const string DodgeName = "dodge";

    /// <summary>The animation name of the stagger clip.</summary>
    public const string StaggerName = "stagger";

    private const string NotABodyClip = "The clip does not animate the body model.";
    private const string FileField = "file";
    private const string ModelField = "model";

    /// <summary>The content path of the roll clip.</summary>
    public static string DodgePath => AssetPaths.AnimationPath(AssetPaths.BodyModel, DodgeName);

    /// <summary>The content path of the stagger clip.</summary>
    public static string StaggerPath => AssetPaths.AnimationPath(AssetPaths.BodyModel, StaggerName);

    /// <summary>The three clips under the content directory, for the main weapon.</summary>
    /// <exception cref="ContextException">A clip file does not exist, is not valid, or animates another model.</exception>
    public static PlayerClips Load(string contentDirectory, WeaponDefinition weapon)
    {
        return new PlayerClips(
            BodyClip(contentDirectory, weapon.Animation),
            BodyClip(contentDirectory, DodgePath),
            BodyClip(contentDirectory, StaggerPath));
    }

    /// <summary>One clip of the body model. A clip of another model is an error that names both paths (T-2).</summary>
    private static AnimationClip BodyClip(string contentDirectory, string contentPath)
    {
        AnimationClip clip = AnimationLoader.Parse(contentPath, AssetFile.Read(contentDirectory, contentPath));
        if (clip.Model != AssetPaths.BodyModel)
        {
            ContextException error = new(NotABodyClip);
            error.AddContext(FileField, contentPath);
            error.AddContext(ModelField, clip.Model);
            throw error;
        }

        return clip;
    }
}
