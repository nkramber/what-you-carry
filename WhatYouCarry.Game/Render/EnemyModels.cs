using System;
using System.Collections.Generic;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Animation;
using WhatYouCarry.Game.Content;

namespace WhatYouCarry.Game.Render;

/// <summary>One model that an enemy draws with: its path, the model, and the lowest box corner of its rest pose.</summary>
/// <param name="Path">The content path of the model file.</param>
/// <param name="Model">The model that the tree of the enemy builds from.</param>
/// <param name="Lowest">The lowest box corner of the rest pose. The root offset reads it, so the feet stand on the floor.</param>
public sealed record EnemyModel(string Path, BlockbenchModel Model, float Lowest);

/// <summary>
/// The models that the enemies draw with (D-673): the model that each family names, and the body model of the player
/// for the Overseer until PR-93 (D-660). An enemy of PR-16 plays no clip, so each model stands in its rest pose (D-401).
/// </summary>
public static class EnemyModels
{
    private const string NoLoadedModel = "The enemy family names a model that the loaded enemy models do not hold (D-673).";
    private const string FamilyField = "family";
    private const string ModelField = "model";

    /// <summary>The model of each family, by model path. Each path that a family names loads once.</summary>
    /// <exception cref="ContextException">A model file is absent or not valid. The error names the file.</exception>
    public static IReadOnlyDictionary<string, EnemyModel> Load(string contentDirectory, IReadOnlyList<EnemyDefinition> families)
    {
        Dictionary<string, EnemyModel> models = new(StringComparer.Ordinal);
        foreach (EnemyDefinition family in families)
        {
            if (!models.ContainsKey(family.Model))
            {
                models.Add(family.Model, Read(contentDirectory, family.Model));
            }
        }

        return models;
    }

    /// <summary>One model at its rest pose.</summary>
    /// <exception cref="ContextException">The model file is absent or not valid. The error names the file.</exception>
    public static EnemyModel Read(string contentDirectory, string path)
    {
        BlockbenchModel model = BlockbenchLoader.Parse(path, AssetFile.Read(contentDirectory, path));
        return new EnemyModel(path, model, ModelPose.LowestPoint(path, model, BodyPose.RestRotations()));
    }

    /// <summary>The model of one family.</summary>
    /// <exception cref="ContextException">The set holds no model of the path that the family names.</exception>
    public static EnemyModel Of(IReadOnlyDictionary<string, EnemyModel> models, EnemyDefinition family)
    {
        if (models.TryGetValue(family.Model, out EnemyModel? model))
        {
            return model;
        }

        ContextException error = new(NoLoadedModel);
        error.AddContext(FamilyField, family.Id);
        error.AddContext(ModelField, family.Model);
        throw error;
    }
}
