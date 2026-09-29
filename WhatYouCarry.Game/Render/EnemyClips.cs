using System;
using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Content;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The swing clip of each enemy family and of the Overseer (D-739, D-741). An enemy plays the clip that its weapon
/// names, on the bones of its own model: the scavenger plays the swing clip of the player, and the Overseer the clip
/// of its pick. A clip plays tick for tick, so it lasts the swing ticks of its weapon (D-87).
/// </summary>
/// <param name="Families">The swing clip of each family, by family id.</param>
/// <param name="Hunter">The swing clip of the Overseer.</param>
public sealed record EnemyClips(IReadOnlyDictionary<string, AnimationClip> Families, AnimationClip Hunter)
{
    private const string NoWeapon = "The enemy names a weapon that the content set does not hold (D-397).";
    private const string NoFamilyClip = "The enemy family has no loaded swing clip (D-741).";
    private const string WrongLength = "The swing clip does not last the swing ticks of its weapon, and a clip plays tick for tick (D-87).";
    private const string NotABone = "The swing clip turns a bone that the enemy model does not have (D-741).";
    private const string FileField = "file";
    private const string WeaponField = "weapon";
    private const string HolderField = "holder";
    private const string FamilyField = "family";
    private const string ModelField = "model";
    private const string BoneField = "bone";
    private const string ClipTicksField = "clipTicks";
    private const string SwingTicksField = "swingTicks";

    /// <summary>The swing clip of each family and of the Overseer, each checked against its weapon and its model.</summary>
    /// <exception cref="ContextException">A weapon is absent, or a clip is absent, is not valid, does not last its swing, or turns a bone that its model does not have.</exception>
    public static EnemyClips Load(string contentDirectory, ContentSet content, IReadOnlyDictionary<string, EnemyModel> familyModels, EnemyModel hunterModel)
    {
        Dictionary<string, AnimationClip> families = new(StringComparer.Ordinal);
        foreach (EnemyDefinition family in content.Enemies)
        {
            WeaponDefinition weapon = WeaponById(content.Weapons, family.Weapon, family.Id);
            families.Add(family.Id, Swing(contentDirectory, weapon, EnemyModels.Of(familyModels, family)));
        }

        WeaponDefinition pick = WeaponById(content.Weapons, content.Hunter.Weapon, content.Hunter.Id);
        return new EnemyClips(families, Swing(contentDirectory, pick, hunterModel));
    }

    /// <summary>The swing clip of one family.</summary>
    /// <exception cref="ContextException">The set holds no clip of the family.</exception>
    public AnimationClip Of(EnemyDefinition family)
    {
        if (this.Families.TryGetValue(family.Id, out AnimationClip? clip))
        {
            return clip;
        }

        ContextException error = new(NoFamilyClip);
        error.AddContext(FamilyField, family.Id);
        throw error;
    }

    /// <summary>
    /// The clip that one weapon names, for one enemy model. The clip lasts the swing ticks of the weapon, and each track
    /// turns a bone of the model. The clip can name another model with the same bones, as the swing clip of the player
    /// does for the scavenger (D-82, D-741).
    /// </summary>
    /// <exception cref="ContextException">The clip is absent, is not valid, does not last the swing, or turns a bone that the model does not have.</exception>
    public static AnimationClip Swing(string contentDirectory, WeaponDefinition weapon, EnemyModel model)
    {
        AnimationClip clip = AnimationLoader.Parse(weapon.Animation, AssetFile.Read(contentDirectory, weapon.Animation));
        if (clip.Length != weapon.SwingTicks)
        {
            ContextException error = new(WrongLength);
            error.AddContext(FileField, weapon.Animation);
            error.AddContext(WeaponField, weapon.Id);
            error.AddContext(ClipTicksField, clip.Length.ToString(CultureInfo.InvariantCulture));
            error.AddContext(SwingTicksField, weapon.SwingTicks.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        foreach (BoneTrack track in clip.Tracks)
        {
            if (model.Model.BoneIndex(track.Bone) == ModelBone.NoParent)
            {
                ContextException error = new(NotABone);
                error.AddContext(FileField, weapon.Animation);
                error.AddContext(ModelField, model.Path);
                error.AddContext(BoneField, track.Bone);
                throw error;
            }
        }

        return clip;
    }

    /// <summary>The weapon of the set with an id. The content loader checks each id, so an absent one is a defect (T-2).</summary>
    private static WeaponDefinition WeaponById(IReadOnlyList<WeaponDefinition> weapons, string id, string holder)
    {
        foreach (WeaponDefinition weapon in weapons)
        {
            if (weapon.Id == id)
            {
                return weapon;
            }
        }

        ContextException error = new(NoWeapon);
        error.AddContext(WeaponField, id);
        error.AddContext(HolderField, holder);
        throw error;
    }
}
