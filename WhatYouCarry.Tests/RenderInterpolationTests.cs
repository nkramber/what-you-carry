using System;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Render;
using Xunit;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Tests;

/// <summary>The render interpolation of the Game layer (D-73, D-234, D-245; PR-12 exit test 3).</summary>
public sealed class RenderInterpolationTests
{
    /// <summary>PR-12 exit test 3. A render position halfway between two tick positions at half a tick.</summary>
    [Fact]
    public void RenderInterpolates()
    {
        CoreVector3 previous = new(1.0f, 2.0f, 3.0f);
        CoreVector3 current = new(3.0f, 6.0f, 9.0f);

        Assert.Equal(new CoreVector3(2.0f, 4.0f, 6.0f), RenderInterpolation.Between(previous, current, 0.5f));
        Assert.Equal(previous, RenderInterpolation.Between(previous, current, 0.0f));
        Assert.Equal(current, RenderInterpolation.Between(previous, current, 1.0f));
    }

    /// <summary>The frame pose is the orbit pose of the interpolated look, across the wrap of the yaw (D-724).</summary>
    [Fact]
    public void TheFramePoseIsTheOrbitPoseOfTheInterpolatedLook()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        CoreVector3 feet = new(8.0f, 1.0f, 8.0f);

        CameraPose pose = RenderInterpolation.Camera(grid, feet, new TickLook(35000, -4000), new TickLook(1000, 2000), 0.5f);

        Assert.Equal(OrbitCamera.Place(grid, feet, 0, -1000), pose);
    }

    /// <summary>The yaw turns the short way across the wrap, and stays from 0 up to but not including a full turn (D-227).</summary>
    [Fact]
    public void TheYawTurnsTheShortWay()
    {
        Assert.Equal(0, RenderInterpolation.Yaw(35900, 100, 0.5f));
        Assert.Equal(35950, RenderInterpolation.Yaw(35900, 100, 0.25f));
        Assert.Equal(0, RenderInterpolation.Yaw(100, 35900, 0.5f));
        Assert.Equal(50, RenderInterpolation.Yaw(100, 35900, 0.25f));
        Assert.Equal(4000, RenderInterpolation.Yaw(1000, 7000, 0.5f));
        Assert.Equal(0, RenderInterpolation.Yaw(35999, 1, 0.5f));
        Assert.Equal(35999, RenderInterpolation.Yaw(35998, 35999, 1.0f));
        Assert.Equal(35998, RenderInterpolation.Yaw(35998, 35999, 0.0f));
    }

    /// <summary>A turn of exactly half a circle turns counterclockwise seen from above, which raises the yaw (D-234).</summary>
    [Fact]
    public void AHalfTurnTurnsCounterclockwise()
    {
        Assert.Equal(9000, RenderInterpolation.Yaw(0, 18000, 0.5f));
        Assert.Equal(27000, RenderInterpolation.Yaw(18000, 0, 0.5f));
    }

    /// <summary>
    /// PR-99 exit test 1 (D-724, F-202). A turn of 180 degrees in one tick: each frame between the two ticks draws the
    /// camera at the distance of the tick poses from the shoulder point. The interpolation of the two tick poses drew
    /// the frame halfway through the turn at the shoulder point, near the head.
    /// </summary>
    [Fact]
    public void AHalfTurnKeepsTheViewOnTheBoomCircle()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        CoreVector3 feet = new(8.0f, 1.0f, 8.0f);
        TickLook previous = new(0, 0);
        TickLook current = new(18000, 0);
        CameraPose previousPose = OrbitCamera.Place(grid, feet, previous.Yaw, previous.Pitch);
        CameraPose currentPose = OrbitCamera.Place(grid, feet, current.Yaw, current.Pitch);
        float previousDistance = (previousPose.View - previousPose.Shoulder).Length();
        float currentDistance = (currentPose.View - currentPose.Shoulder).Length();
        Assert.InRange(currentDistance, previousDistance - 1e-4f, previousDistance + 1e-4f);

        foreach (float fraction in new[] { 0.25f, 0.5f, 0.75f })
        {
            CameraPose pose = RenderInterpolation.Camera(grid, feet, previous, current, fraction);
            float distance = (pose.View - pose.Shoulder).Length();
            Assert.True(
                MathF.Abs(distance - previousDistance) <= 1e-4f,
                $"At the fraction {fraction} the drawn camera is {distance} meters from the shoulder point, and the tick poses are {previousDistance} meters from it.");
        }
    }

    /// <summary>A Core vector becomes an engine vector with no conversion, because the two share one frame (D-234).</summary>
    [Fact]
    public void ToGodotKeepsEveryComponent()
    {
        Godot.Vector3 vector = RenderInterpolation.ToGodot(new CoreVector3(1.5f, -2.5f, 3.5f));

        Assert.Equal(1.5f, vector.X);
        Assert.Equal(-2.5f, vector.Y);
        Assert.Equal(3.5f, vector.Z);
    }
}
