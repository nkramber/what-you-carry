using WhatYouCarry.Core.Camera;
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

    /// <summary>Every field of a pose interpolates on its own.</summary>
    [Fact]
    public void PoseInterpolatesEveryField()
    {
        CameraPose previous = new(new CoreVector3(0.0f, 0.0f, 0.0f), new CoreVector3(0.0f, 0.0f, -1.0f), new CoreVector3(1.0f, 0.0f, 0.0f), new CoreVector3(0.0f, 1.0f, 0.0f), new CoreVector3(0.0f, 2.0f, 0.0f));
        CameraPose current = new(new CoreVector3(2.0f, 0.0f, 0.0f), new CoreVector3(0.0f, 0.0f, 1.0f), new CoreVector3(-1.0f, 0.0f, 0.0f), new CoreVector3(0.0f, 1.0f, 0.0f), new CoreVector3(2.0f, 2.0f, 0.0f));

        CameraPose middle = RenderInterpolation.Between(previous, current, 0.5f);

        Assert.Equal(new CoreVector3(1.0f, 0.0f, 0.0f), middle.Position);
        Assert.Equal(new CoreVector3(0.0f, 0.0f, 0.0f), middle.Forward);
        Assert.Equal(new CoreVector3(0.0f, 0.0f, 0.0f), middle.Right);
        Assert.Equal(new CoreVector3(0.0f, 1.0f, 0.0f), middle.Up);
        Assert.Equal(new CoreVector3(1.0f, 2.0f, 0.0f), middle.Shoulder);
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
