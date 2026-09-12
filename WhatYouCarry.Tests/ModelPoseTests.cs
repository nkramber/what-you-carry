using System;
using System.Collections.Generic;
using System.Text;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The rotation matrix, the bone transforms, and the posed boxes of the Assets project (D-298, D-299).</summary>
public sealed class ModelPoseTests
{
    private const float Tolerance = 0.00001f;

    /// <summary>
    /// The euler order is the order of Blockbench: a vector turns about X first, then Y, then Z (D-298). The
    /// point (0, 1, 0) under (90, 90, 0) goes to (0, 0, 1) about X, then to (1, 0, 0) about Y. The other order
    /// leaves it at (0, 1, 0) about Y and then puts it at (0, 0, 1).
    /// </summary>
    [Fact]
    public void RotationOrderIsBlockbenchOrder()
    {
        RotationMatrix rotation = RotationMatrix.FromEulerDegrees(new Vector3(90.0f, 90.0f, 0.0f));

        AssertClose(new Vector3(1.0f, 0.0f, 0.0f), rotation.Apply(new Vector3(0.0f, 1.0f, 0.0f)));
    }

    /// <summary>Each single-axis turn follows the right-hand rule of the frame of D-234.</summary>
    [Theory]
    [InlineData(90.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f)]
    [InlineData(0.0f, 90.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f)]
    [InlineData(0.0f, 0.0f, 90.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f)]
    public void SingleAxisTurnsFollowTheRightHandRule(float aboutX, float aboutY, float aboutZ, float inX, float inY, float inZ, float outX, float outY, float outZ)
    {
        RotationMatrix rotation = RotationMatrix.FromEulerDegrees(new Vector3(aboutX, aboutY, aboutZ));

        AssertClose(new Vector3(outX, outY, outZ), rotation.Apply(new Vector3(inX, inY, inZ)));
    }

    /// <summary>The columns of a rotation are the images of the three axes, and the identity changes nothing.</summary>
    [Fact]
    public void ColumnsAreTheImagesOfTheAxes()
    {
        RotationMatrix rotation = RotationMatrix.FromEulerDegrees(new Vector3(30.0f, -50.0f, 120.0f));

        AssertClose(rotation.Apply(new Vector3(1.0f, 0.0f, 0.0f)), rotation.AxisX());
        AssertClose(rotation.Apply(new Vector3(0.0f, 1.0f, 0.0f)), rotation.AxisY());
        AssertClose(rotation.Apply(new Vector3(0.0f, 0.0f, 1.0f)), rotation.AxisZ());
        Assert.Equal(new Vector3(3.0f, -2.0f, 7.0f), RotationMatrix.Identity.Apply(new Vector3(3.0f, -2.0f, 7.0f)));
    }

    /// <summary>An angle that is not finite is an error.</summary>
    [Fact]
    public void RotationRejectsANonFiniteAngle()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RotationMatrix.FromEulerDegrees(new Vector3(float.NaN, 0.0f, 0.0f)));
    }

    /// <summary>A bone turns about its pivot: the pivot stays, and a point one unit above it swings out.</summary>
    [Fact]
    public void BoneTurnsAboutItsPivot()
    {
        Vector3 pivot = new(4.0f, 16.0f, 0.0f);
        BoneTransform transform = BoneTransform.AboutPivot(RotationMatrix.FromEulerDegrees(new Vector3(0.0f, 0.0f, -90.0f)), pivot);

        AssertClose(pivot, transform.Apply(pivot));
        AssertClose(new Vector3(5.0f, 16.0f, 0.0f), transform.Apply(new Vector3(4.0f, 17.0f, 0.0f)));
    }

    /// <summary>A child bone turns with its parent, and then about its own pivot.</summary>
    [Fact]
    public void ChildBoneTurnsWithItsParent()
    {
        BlockbenchModel model = BlockbenchLoader.Parse("models/rig.bbmodel", Encoding.UTF8.GetBytes(ModelJson.JointRig()));
        Dictionary<string, Vector3> rotations = new()
        {
            ["torso_bone"] = new Vector3(0.0f, 0.0f, 90.0f),
            ["arm_bone"] = new Vector3(0.0f, 0.0f, -90.0f),
        };

        IReadOnlyList<BoneTransform> transforms = ModelPose.BoneTransforms("models/rig.bbmodel", model, rotations);

        // The torso turns the arm pivot, (4, 16, 0) units or (0.25, 1, 0) meters, about the origin by 90 degrees to (-1, 0.25, 0).
        AssertClose(new Vector3(-1.0f, 0.25f, 0.0f), transforms[model.BoneIndex("arm_bone")].Apply(new Vector3(0.25f, 1.0f, 0.0f)));

        // A point one unit below the arm pivot turns by minus 90 degrees about that pivot to one unit left of it, and then with the torso.
        AssertClose(new Vector3(-1.0f, 0.1875f, 0.0f), transforms[model.BoneIndex("arm_bone")].Apply(new Vector3(0.25f, 0.9375f, 0.0f)));
        Assert.Equal(BoneTransform.Rest, transforms[model.BoneIndex("body")]);
    }

    /// <summary>A rotation on a name that is not a bone is an error that names it (T-2).</summary>
    [Fact]
    public void PoseRejectsAnUnknownBone()
    {
        BlockbenchModel model = BlockbenchLoader.Parse("models/rig.bbmodel", Encoding.UTF8.GetBytes(ModelJson.SiblingRig()));
        Dictionary<string, Vector3> rotations = new() { ["wing"] = new Vector3(0.0f, 0.0f, 10.0f) };

        ContextException error = Assert.Throws<ContextException>(() => ModelPose.BoneTransforms("models/rig.attack.json", model, rotations));

        Assert.Contains("wing", error.Message, StringComparison.Ordinal);
        Assert.Contains("models/rig.attack.json", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A posed box keeps its half extents, and its center follows the transform of its bone.</summary>
    [Fact]
    public void PlacedBoxFollowsItsBone()
    {
        BlockbenchModel model = BlockbenchLoader.Parse("models/rig.bbmodel", Encoding.UTF8.GetBytes(ModelJson.SiblingRig()));
        ModelBox arm = Assert.Single(model.Boxes, box => box.Name == "arm");
        Dictionary<string, Vector3> rotations = new() { ["arm_bone"] = new Vector3(0.0f, 0.0f, -90.0f) };
        IReadOnlyList<BoneTransform> transforms = ModelPose.BoneTransforms("models/rig.bbmodel", model, rotations);

        PosedBox posed = ModelPose.Place(arm, transforms[arm.Bone]);

        // The arm center at rest is (6, 12, 0) units: 2 right of the shoulder and 4 below it. The turn of minus 90 degrees
        // about Z puts it 4 left of the shoulder and 2 below it, at (0, 14, 0) units, which is (0, 0.875, 0) meters.
        Assert.Equal(new Vector3(0.125f, 0.25f, 0.125f), posed.HalfExtents);
        AssertClose(new Vector3(0.0f, 0.875f, 0.0f), posed.Center);
        Assert.Equal(arm.Bone, posed.Bone);
    }

    private static void AssertClose(Vector3 expected, Vector3 actual)
    {
        Assert.True(Math.Abs(expected.X - actual.X) < Tolerance && Math.Abs(expected.Y - actual.Y) < Tolerance && Math.Abs(expected.Z - actual.Z) < Tolerance,
            $"Expected {expected} and got {actual}.");
    }
}
