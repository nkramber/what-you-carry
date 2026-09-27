using System;
using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Render;
using Xunit;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Tests;

/// <summary>The model of each enemy family (PR-76): the model field of D-673, and the scavenger of D-661 to D-675.</summary>
public sealed class EnemyModelTests
{
    private const string ScavengerModel = "models/scavenger.bbmodel";

    /// <summary>A model unit in meters: one sixteenth of a meter.</summary>
    private const float Unit = 1.0f / 16.0f;

    /// <summary>The largest difference of two box corners that counts as equal, in meters.</summary>
    private const float Tolerance = 0.0001f;

    /// <summary>The boxes of the shared base body that the scavenger keeps (D-82, D-662).</summary>
    private static readonly string[] BodyBoxes =
    [
        "head_box", "torso_box", "arm_left_upper_box", "arm_left_lower_box", "arm_right_upper_box", "arm_right_lower_box",
        "leg_left_upper_box", "leg_left_lower_box", "leg_right_upper_box", "leg_right_lower_box", "toe_left_box", "toe_right_box",
    ];

    private static string ContentDirectory => Path.Combine(RepositoryRoot.Find(), "content");

    /// <summary>The scavenger family draws with its own model, and not with the body model of the player (D-673).</summary>
    [Fact]
    public void TheScavengerDrawsWithItsOwnModel()
    {
        EnemyDefinition scavenger = Family("scavenger");
        IReadOnlyDictionary<string, EnemyModel> models = EnemyModels.Load(ContentDirectory, TestWorld.Content.Enemies);

        EnemyModel model = EnemyModels.Of(models, scavenger);

        Assert.Equal(ScavengerModel, scavenger.Model);
        Assert.Equal(ScavengerModel, model.Path);
        Assert.NotNull(model.Model.Box("hood_top_box"));
        Assert.Null(model.Model.Box("brow_box"));
        EnemyModel body = EnemyModels.Read(ContentDirectory, AssetPaths.BodyModel);
        Assert.Equal(body.Lowest, model.Lowest);
    }

    /// <summary>
    /// The scavenger keeps each box of the shared base body at its place and bone, and the weapon point, and it drops
    /// the brow, the nose, and the beard (D-82, D-662, D-670).
    /// </summary>
    [Fact]
    public void TheScavengerKeepsTheBaseBody()
    {
        BlockbenchModel body = Parse(AssetPaths.BodyModel);
        BlockbenchModel scavenger = Parse(ScavengerModel);

        foreach (string name in BodyBoxes)
        {
            ModelBox expected = body.Box(name) ?? throw new InvalidOperationException($"The body has no box '{name}'.");
            ModelBox actual = scavenger.Box(name) ?? throw new InvalidOperationException($"The scavenger has no box '{name}'.");
            AssertNear(expected.From, actual.From, name);
            AssertNear(expected.To, actual.To, name);
            Assert.Equal(body.Bones[expected.Bone].Name, scavenger.Bones[actual.Bone].Name);
        }

        Assert.Null(scavenger.Box("brow_box"));
        Assert.Null(scavenger.Box("nose_box"));
        Assert.Null(scavenger.Box("beard_box"));
        Assert.Equal(body.Attachments.Count, scavenger.Attachments.Count);
    }

    /// <summary>
    /// The hood is four plates of 1 unit on the head bone, the top and side plates 1 unit forward of the head front
    /// (D-666, D-674). The scarf is 8 by 3.5 by 1 on the head front (D-670). The sack is 9.5 by 2.5 by 3.5 on the torso
    /// bone, its top 1 unit over the torso, and its back 1 unit behind the hood (D-667, D-675).
    /// </summary>
    [Theory]
    [InlineData("hood_top_box", "head", -5.0f, 28.8f, -5.0f, 5.0f, 29.8f, 5.0f)]
    [InlineData("hood_left_box", "head", -5.0f, 20.8f, -5.0f, -4.0f, 28.8f, 4.0f)]
    [InlineData("hood_right_box", "head", 4.0f, 20.8f, -5.0f, 5.0f, 28.8f, 4.0f)]
    [InlineData("hood_back_box", "head", -5.0f, 20.8f, 4.0f, 5.0f, 28.8f, 5.0f)]
    [InlineData("scarf_box", "head", -4.0f, 20.8f, -5.0f, 4.0f, 24.3f, -4.0f)]
    [InlineData("sack_box", "body", -4.75f, 19.3f, 2.5f, 4.75f, 21.8f, 6.0f)]
    public void TheScavengerFeatureBoxesHaveTheirDecidedPlaces(string name, string bone, float fromX, float fromY, float fromZ, float toX, float toY, float toZ)
    {
        BlockbenchModel scavenger = Parse(ScavengerModel);

        ModelBox box = scavenger.Box(name) ?? throw new InvalidOperationException($"The scavenger has no box '{name}'.");

        Assert.Equal(bone, scavenger.Bones[box.Bone].Name);
        AssertNear(new CoreVector3(fromX * Unit, fromY * Unit, fromZ * Unit), box.From, name);
        AssertNear(new CoreVector3(toX * Unit, toY * Unit, toZ * Unit), box.To, name);
    }

    /// <summary>A family whose model file is absent is an error that names the file, and never a body with no model (T-2, D-673).</summary>
    [Fact]
    public void AnAbsentFamilyModelIsAnError()
    {
        EnemyDefinition gone = Family("scavenger") with { Model = "models/gone.bbmodel" };

        ContextException error = Assert.Throws<ContextException>(() => EnemyModels.Load(ContentDirectory, [gone]));

        Assert.Contains("gone.bbmodel", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A family whose model the loaded set does not hold is an error that names the family and the path (T-2).</summary>
    [Fact]
    public void AFamilyWithNoLoadedModelIsAnError()
    {
        EnemyDefinition scavenger = Family("scavenger");

        ContextException error = Assert.Throws<ContextException>(() => EnemyModels.Of(new Dictionary<string, EnemyModel>(), scavenger));

        Assert.Contains("scavenger", error.Message, StringComparison.Ordinal);
        Assert.Contains(ScavengerModel, error.Message, StringComparison.Ordinal);
    }

    private static EnemyDefinition Family(string id)
    {
        foreach (EnemyDefinition family in TestWorld.Content.Enemies)
        {
            if (family.Id == id)
            {
                return family;
            }
        }

        throw new InvalidOperationException($"The repository content holds no enemy family '{id}'.");
    }

    private static BlockbenchModel Parse(string path)
    {
        return BlockbenchLoader.Parse(path, File.ReadAllBytes(Path.Combine(ContentDirectory, path)));
    }

    private static void AssertNear(CoreVector3 expected, CoreVector3 actual, string name)
    {
        bool near = MathF.Abs(expected.X - actual.X) <= Tolerance && MathF.Abs(expected.Y - actual.Y) <= Tolerance && MathF.Abs(expected.Z - actual.Z) <= Tolerance;
        Assert.True(near, $"The box '{name}' has the corner {actual}, and the expected corner is {expected}.");
    }
}
