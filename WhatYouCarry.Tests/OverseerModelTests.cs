using System;
using System.IO;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Render;
using Xunit;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Tests;

/// <summary>The model of the Overseer (PR-93): the model field of D-698, and the boxes of D-686 and D-689 to D-697.</summary>
public sealed class OverseerModelTests
{
    private const string OverseerModel = "models/overseer.bbmodel";

    /// <summary>A model unit in meters: one sixteenth of a meter.</summary>
    private const float Unit = 1.0f / 16.0f;

    /// <summary>The largest difference of two box corners that counts as equal, in meters.</summary>
    private const float Tolerance = 0.0001f;

    private static string ContentDirectory => Path.Combine(RepositoryRoot.Find(), "content");

    /// <summary>The hunter file names the Overseer model, and the Game layer reads it with its feet on the floor (D-698).</summary>
    [Fact]
    public void TheOverseerDrawsWithItsOwnModel()
    {
        HunterDefinition hunter = TestWorld.Content.Hunter;

        EnemyModel model = EnemyModels.Read(ContentDirectory, hunter.Model);

        Assert.Equal(OverseerModel, hunter.Model);
        Assert.Equal(OverseerModel, model.Path);
        Assert.NotNull(model.Model.Box("lamp_box"));
        Assert.Null(model.Model.Box("beard_box"));
        Assert.Equal(0.0f, model.Lowest);
    }

    /// <summary>
    /// The model is 37 units tall, about 2.3 meters, and so taller than the body box of the simulation. The box of
    /// D-165 stays 1.8 meters, and a corridor of 3 blocks clears the model (D-166, D-423, D-686, D-691).
    /// </summary>
    [Fact]
    public void TheOverseerStandsThirtySevenUnitsTall()
    {
        BlockbenchModel overseer = Parse(OverseerModel);

        float top = float.MinValue;
        float bottom = float.MaxValue;
        foreach (ModelBox box in overseer.Boxes)
        {
            top = MathF.Max(top, box.To.Y);
            bottom = MathF.Min(bottom, box.From.Y);
        }

        Assert.Equal(0.0f, bottom, Tolerance);
        Assert.Equal(37.0f * Unit, top, Tolerance);
        Assert.True(top > PlayerBody.Height, $"The model top is {top} meters, and the body box is {PlayerBody.Height} meters tall.");
        Assert.True(top < 3.0f, $"The model top is {top} meters, and a corridor is 3 blocks high (D-166).");
    }

    /// <summary>
    /// Each box has its decided place and bone. The legs are 14 units, the torso 12, the head 8, and the helmet 3
    /// (D-686, D-691). The arms are 13 units from the torso top (D-694). The upper legs are 5 wide and meet at the
    /// middle (D-692). The goggles and the mask stand 1 unit forward of the head, and the cans 2 (D-693). Each toe box
    /// is 4 by 2 by 3 (D-697).
    /// </summary>
    [Theory]
    [InlineData("torso_box", "body", -5.0f, 14.0f, -2.5f, 5.0f, 26.0f, 2.5f)]
    [InlineData("head_box", "head", -4.0f, 26.0f, -4.0f, 4.0f, 34.0f, 4.0f)]
    [InlineData("brim_box", "head", -5.0f, 34.0f, -5.0f, 5.0f, 34.75f, 5.0f)]
    [InlineData("crown_box", "head", -4.0f, 34.75f, -4.0f, 4.0f, 37.0f, 4.0f)]
    [InlineData("lamp_box", "head", -1.5f, 34.75f, -5.0f, 1.5f, 37.0f, -4.0f)]
    [InlineData("goggle_left_box", "head", -4.0f, 31.0f, -5.0f, -0.5f, 33.0f, -4.0f)]
    [InlineData("goggle_right_box", "head", 0.5f, 31.0f, -5.0f, 4.0f, 33.0f, -4.0f)]
    [InlineData("mask_box", "head", -1.5f, 26.0f, -5.0f, 1.5f, 31.0f, -4.0f)]
    [InlineData("can_left_box", "head", -4.0f, 26.5f, -6.0f, -1.5f, 29.0f, -4.0f)]
    [InlineData("can_right_box", "head", 1.5f, 26.5f, -6.0f, 4.0f, 29.0f, -4.0f)]
    [InlineData("arm_left_upper_box", "arm_left", -8.0f, 19.5f, -1.5f, -5.0f, 26.0f, 1.5f)]
    [InlineData("arm_left_lower_box", "arm_left_lower", -8.0f, 13.0f, -1.5f, -5.0f, 19.5f, 1.5f)]
    [InlineData("arm_right_upper_box", "arm_right", 5.0f, 19.5f, -1.5f, 8.0f, 26.0f, 1.5f)]
    [InlineData("arm_right_lower_box", "arm_right_lower", 5.0f, 13.0f, -1.5f, 8.0f, 19.5f, 1.5f)]
    [InlineData("leg_left_upper_box", "leg_left", -5.0f, 7.0f, -2.5f, 0.0f, 14.0f, 2.5f)]
    [InlineData("leg_left_lower_box", "leg_left_lower", -4.5f, 0.0f, -2.0f, -0.5f, 7.0f, 2.0f)]
    [InlineData("toe_left_box", "leg_left_lower", -4.5f, 0.0f, -5.0f, -0.5f, 2.0f, -2.0f)]
    [InlineData("leg_right_upper_box", "leg_right", 0.0f, 7.0f, -2.5f, 5.0f, 14.0f, 2.5f)]
    [InlineData("leg_right_lower_box", "leg_right_lower", 0.5f, 0.0f, -2.0f, 4.5f, 7.0f, 2.0f)]
    [InlineData("toe_right_box", "leg_right_lower", 0.5f, 0.0f, -5.0f, 4.5f, 2.0f, -2.0f)]
    public void TheOverseerBoxesHaveTheirDecidedPlaces(string name, string bone, float fromX, float fromY, float fromZ, float toX, float toY, float toZ)
    {
        BlockbenchModel overseer = Parse(OverseerModel);

        ModelBox box = overseer.Box(name) ?? throw new InvalidOperationException($"The Overseer has no box '{name}'.");

        Assert.Equal(bone, overseer.Bones[box.Bone].Name);
        AssertNear(new CoreVector3(fromX * Unit, fromY * Unit, fromZ * Unit), box.From, name);
        AssertNear(new CoreVector3(toX * Unit, toY * Unit, toZ * Unit), box.To, name);
    }

    /// <summary>
    /// The model holds the twenty boxes of the test above and no other, and the seven attachment points of the body
    /// model, so the sword of D-397 hangs from the right hand (D-694).
    /// </summary>
    [Fact]
    public void TheOverseerHoldsItsBoxesAndTheAttachmentPointsOfTheBody()
    {
        BlockbenchModel body = Parse(AssetPaths.BodyModel);
        BlockbenchModel overseer = Parse(OverseerModel);

        Assert.Equal(20, overseer.Boxes.Count);
        Assert.Equal(body.Attachments.Count, overseer.Attachments.Count);
    }

    /// <summary>A hunter whose model file is absent is an error that names the file, and never an Overseer with no model (T-2, D-698).</summary>
    [Fact]
    public void AnAbsentHunterModelIsAnError()
    {
        ContextException error = Assert.Throws<ContextException>(() => EnemyModels.Read(ContentDirectory, "models/gone.bbmodel"));

        Assert.Contains("gone.bbmodel", error.Message, StringComparison.Ordinal);
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
