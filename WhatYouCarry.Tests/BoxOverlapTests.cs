using System;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Physics;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The separating axis test of two posed boxes (D-135, D-301).</summary>
public sealed class BoxOverlapTests
{
    private const float Tolerance = 0.00001f;

    /// <summary>Two boxes with a gap are apart, and the depth is minus the gap.</summary>
    [Fact]
    public void ApartBoxesHaveANegativeDepth()
    {
        PosedBox first = Aligned(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
        PosedBox second = Aligned(new Vector3(2.5f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));

        Assert.Equal(-0.5f, BoxOverlap.Depth(first, second), Tolerance);
        Assert.False(BoxOverlap.Clips(first, second));
    }

    /// <summary>Two boxes that share a face touch: the depth is zero, and a touch is not a clip.</summary>
    [Fact]
    public void TouchingBoxesDoNotClip()
    {
        PosedBox first = Aligned(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
        PosedBox second = Aligned(new Vector3(2.0f, 0.5f, -0.5f), new Vector3(1.0f, 1.0f, 1.0f));

        Assert.Equal(0.0f, BoxOverlap.Depth(first, second), Tolerance);
        Assert.False(BoxOverlap.Clips(first, second));
    }

    /// <summary>Two aligned boxes that overlap have the depth of the least overlap over the three axes.</summary>
    [Fact]
    public void OverlappingBoxesHaveTheLeastOverlapAsDepth()
    {
        PosedBox first = Aligned(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
        PosedBox second = Aligned(new Vector3(1.7f, 0.2f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));

        Assert.Equal(0.3f, BoxOverlap.Depth(first, second), Tolerance);
        Assert.True(BoxOverlap.Clips(first, second));
        Assert.Equal(BoxOverlap.Depth(first, second), BoxOverlap.Depth(second, first), Tolerance);
    }

    /// <summary>A box that another box encloses clips it by the full width of the small box.</summary>
    [Fact]
    public void AnEnclosedBoxClipsByItsOwnWidth()
    {
        PosedBox large = Aligned(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(2.0f, 2.0f, 2.0f));
        PosedBox small = Aligned(new Vector3(0.5f, 0.0f, 0.0f), new Vector3(0.5f, 0.5f, 0.5f));

        Assert.Equal(2.0f, BoxOverlap.Depth(large, small), Tolerance);
    }

    /// <summary>
    /// A box turned by 45 degrees about Z reaches the square root of two along X, so it clips a box that an
    /// aligned box of the same size would not reach.
    /// </summary>
    [Fact]
    public void ATurnedBoxReachesItsCorner()
    {
        PosedBox aligned = Aligned(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
        RotationMatrix turned = RotationMatrix.FromEulerDegrees(new Vector3(0.0f, 0.0f, 45.0f));
        PosedBox near = new("near", 0, new Vector3(2.2f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f), turned);
        PosedBox far = new("far", 0, new Vector3(2.5f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f), turned);

        Assert.Equal(1.0f + MathF.Sqrt(2.0f) - 2.2f, BoxOverlap.Depth(aligned, near), Tolerance);
        Assert.True(BoxOverlap.Clips(aligned, near));
        Assert.False(BoxOverlap.Clips(aligned, far));
        Assert.Equal(1.0f + MathF.Sqrt(2.0f) - 2.5f, BoxOverlap.Depth(aligned, far), Tolerance);
    }

    /// <summary>
    /// Two boxes that only a cross-product axis separates: an edge of one points at an edge of the other, and
    /// no face axis of either box shows the gap.
    /// </summary>
    [Fact]
    public void ACrossAxisSeparatesEdgeToEdge()
    {
        PosedBox first = new("first", 0, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f), RotationMatrix.FromEulerDegrees(new Vector3(0.0f, 45.0f, 0.0f)));
        PosedBox second = new("second", 0, new Vector3(2.4f, 0.0f, 2.4f), new Vector3(1.0f, 1.0f, 1.0f), RotationMatrix.FromEulerDegrees(new Vector3(45.0f, 0.0f, 0.0f)));

        // Each face axis projects the other box wide enough to overlap. The cross axes show the gap between the two edges.
        Assert.True(BoxOverlap.Depth(first, second) < 0.0f, $"The depth is {BoxOverlap.Depth(first, second)}.");
    }

    /// <summary>An overlap under the touch epsilon is float noise and not a clip (D-301).</summary>
    [Fact]
    public void NoiseUnderTheEpsilonIsNotAClip()
    {
        PosedBox first = Aligned(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
        PosedBox second = Aligned(new Vector3(2.0f - (BoxOverlap.TouchEpsilon / 2.0f), 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));

        Assert.True(BoxOverlap.Depth(first, second) > 0.0f);
        Assert.False(BoxOverlap.Clips(first, second));
    }

    private static PosedBox Aligned(Vector3 center, Vector3 halfExtents)
    {
        return new PosedBox("box", 0, center, halfExtents, RotationMatrix.Identity);
    }
}
