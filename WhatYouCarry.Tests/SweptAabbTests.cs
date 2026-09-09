using System;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The swept box against the grid (D-80, D-165, D-235, D-237; PR-7 exit test 1).</summary>
public sealed class SweptAabbTests
{
    private const float Skin = SweptAabb.ContactSkin;

    /// <summary>The width and the height of the player box (D-165).</summary>
    private static readonly Vector3 PlayerSize = new(0.6f, 1.8f, 0.6f);

    /// <summary>A box of the player size with its min corner at a point.</summary>
    private static Aabb PlayerBoxAt(float x, float y, float z)
    {
        return new Aabb(new Vector3(x, y, z), new Vector3(x + PlayerSize.X, y + PlayerSize.Y, z + PlayerSize.Z));
    }

    /// <summary>A grid of the given size with one row of stone at the bottom.</summary>
    private static VoxelGrid FloorGrid(int side, int height)
    {
        VoxelGrid grid = new(side, height, side);
        for (int x = 0; x < side; x++)
        {
            for (int z = 0; z < side; z++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
            }
        }

        return grid;
    }

    /// <summary>A twelve by eight by twelve room with a one-block wall across it at x = 6, from the floor to the top.</summary>
    private static VoxelGrid WalledRoom()
    {
        VoxelGrid grid = FloorGrid(12, 8);
        for (int y = 1; y < 8; y++)
        {
            for (int z = 0; z < 12; z++)
            {
                grid.Set(6, y, z, BlockId.RawStone);
            }
        }

        return grid;
    }

    /// <summary>The skin of D-235 is 2^-10 meters, exact in float, and at least 64 ulps at the far edge of the largest grid.</summary>
    [Fact]
    public void TheSkinIsAPowerOfTwo()
    {
        Assert.Equal(1.0f / 1024.0f, Skin);
        float ulpAtTheEdge = MathF.BitIncrement(128.0f) - 128.0f;
        Assert.True(Skin >= 64.0f * ulpAtTheEdge, $"The skin {Skin} is under 64 ulps of {ulpAtTheEdge} at the far edge.");
    }

    /// <summary>
    /// PR-7 exit test 1. Over ten thousand random directions at a speed far above any real one, a box in the
    /// west half of a walled room never ends inside a solid block, and never crosses the one-block wall. Each
    /// direction runs five sweeps from a random start. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void NoTunnelAtMaxSpeed()
    {
        VoxelGrid room = WalledRoom();
        for (int seed = 1; seed <= 10000; seed++)
        {
            Random random = new(seed);
            Aabb box = PlayerBoxAt(
                0.1f + (float)(random.NextDouble() * 5.2),
                1.0f + (float)(random.NextDouble() * 5.2),
                0.1f + (float)(random.NextDouble() * 11.2));
            Assert.False(SweptAabb.Overlaps(room, box), $"Seed {seed}: the start box {box} overlaps rock, and the test setup is wrong.");

            float dx = (float)((random.NextDouble() * 2.0) - 1.0);
            float dy = (float)((random.NextDouble() * 2.0) - 1.0);
            float dz = (float)((random.NextDouble() * 2.0) - 1.0);
            float length = MathF.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
            if (length == 0.0f)
            {
                continue;
            }

            // Up to twenty meters in one tick. The fastest real body moves under one meter per tick.
            float speed = (float)(random.NextDouble() * 20.0);
            Vector3 delta = new(dx / length * speed, dy / length * speed, dz / length * speed);

            for (int step = 0; step < 5; step++)
            {
                SweepResult result = SweptAabb.Sweep(room, box, delta);
                box = box.Moved(result.Allowed);
                Assert.False(SweptAabb.Overlaps(room, box), $"Seed {seed}, step {step}: the box {box} ends inside rock after the move {delta}.");
                Assert.True(box.Max.X <= 6.0f, $"Seed {seed}, step {step}: the box {box} crossed the wall at x = 6 after the move {delta}.");
                Assert.True(box.Min.Y >= 1.0f, $"Seed {seed}, step {step}: the box {box} fell into the floor after the move {delta}.");
            }
        }
    }

    /// <summary>A move into a face stops one skin before it, and the sweep names the axis it cut.</summary>
    [Fact]
    public void AMoveIntoAFaceStopsOneSkinBeforeIt()
    {
        VoxelGrid room = WalledRoom();
        Aabb box = PlayerBoxAt(3.0f, 1.5f, 3.0f);

        SweepResult result = SweptAabb.Sweep(room, box, new Vector3(5.0f, 0.0f, 0.0f));

        Assert.True(result.BlockedX);
        Assert.False(result.BlockedY);
        Assert.False(result.BlockedZ);
        Assert.Equal(0.0f, result.Allowed.Y);
        Assert.Equal(0.0f, result.Allowed.Z);
        Aabb moved = box.Moved(result.Allowed);
        Assert.InRange(moved.Max.X, 6.0f - Skin - 1e-5f, 6.0f - Skin + 1e-5f);
        Assert.False(SweptAabb.Overlaps(room, moved));
    }

    /// <summary>The mirror image: a move in the negative direction stops one skin past the far face of the block.</summary>
    [Fact]
    public void ANegativeMoveIntoAFaceStopsOneSkinBeforeIt()
    {
        VoxelGrid room = WalledRoom();
        Aabb box = PlayerBoxAt(9.0f, 1.5f, 3.0f);

        SweepResult result = SweptAabb.Sweep(room, box, new Vector3(-5.0f, -5.0f, 0.0f));

        Assert.True(result.BlockedX);
        Assert.True(result.BlockedY);
        Aabb moved = box.Moved(result.Allowed);
        Assert.InRange(moved.Min.X, 7.0f + Skin - 1e-5f, 7.0f + Skin + 1e-5f);
        Assert.InRange(moved.Min.Y, 1.0f + Skin - 1e-5f, 1.0f + Skin + 1e-5f);
    }

    /// <summary>A face inside the skin, or on the block face, does not move toward the block, and the sweep reports the cut.</summary>
    [Theory]
    [InlineData(6.0f - SweptAabb.ContactSkin)]
    [InlineData(6.0f - (SweptAabb.ContactSkin / 2.0f))]
    [InlineData(6.0f)]
    public void AFaceInsideTheSkinDoesNotMoveIntoTheBlock(float maxX)
    {
        VoxelGrid room = WalledRoom();
        Aabb box = new(new Vector3(maxX - 0.6f, 1.5f, 3.0f), new Vector3(maxX, 3.3f, 3.6f));
        Assert.False(SweptAabb.Overlaps(room, box));

        SweepResult result = SweptAabb.Sweep(room, box, new Vector3(0.01f, 0.0f, 0.0f));

        Assert.True(result.BlockedX);
        Assert.Equal(0.0f, result.Allowed.X);
        Assert.Equal(box, box.Moved(result.Allowed));
    }

    /// <summary>A move that would end inside the skin of a face stops at the skin, and not inside it.</summary>
    [Fact]
    public void AMoveThatEndsInsideTheSkinStopsAtTheSkin()
    {
        VoxelGrid room = WalledRoom();
        Aabb box = PlayerBoxAt(5.0f, 1.5f, 3.0f);

        // The face is at 5.6, and the move of 0.3995 ends at 5.9995, inside the skin of the wall at 6.
        SweepResult result = SweptAabb.Sweep(room, box, new Vector3(0.3995f, 0.0f, 0.0f));

        Assert.True(result.BlockedX);
        Assert.InRange(box.Moved(result.Allowed).Max.X, 6.0f - Skin - 1e-5f, 6.0f - Skin + 1e-6f);
    }

    /// <summary>A move that stops short of the skin runs in full and is not blocked.</summary>
    [Fact]
    public void AMoveThatStopsShortOfTheBlockRunsInFull()
    {
        VoxelGrid room = WalledRoom();
        Aabb box = PlayerBoxAt(3.0f, 1.5f, 3.0f);

        SweepResult result = SweptAabb.Sweep(room, box, new Vector3(2.0f, 1.0f, -1.5f));

        Assert.False(result.BlockedX);
        Assert.False(result.BlockedY);
        Assert.False(result.BlockedZ);
        Assert.Equal(new Vector3(2.0f, 1.0f, -1.5f), result.Allowed);
    }

    /// <summary>A wall cuts one axis, and the move on the other two runs in full, so a body slides along a wall.</summary>
    [Fact]
    public void ACutAxisLeavesTheOthersFree()
    {
        VoxelGrid room = WalledRoom();
        Aabb box = PlayerBoxAt(5.0f, 1.5f, 3.0f);

        SweepResult result = SweptAabb.Sweep(room, box, new Vector3(1.0f, 0.5f, 2.0f));

        Assert.True(result.BlockedX);
        Assert.False(result.BlockedY);
        Assert.False(result.BlockedZ);
        Assert.Equal(0.5f, result.Allowed.Y);
        Assert.Equal(2.0f, result.Allowed.Z);
        Assert.InRange(result.Allowed.X, 0.4f - Skin - 1e-5f, 0.4f - Skin + 1e-5f);
    }

    /// <summary>
    /// The sweep runs Y first, then X, then Z. A box that moves down and sideways toward a one-block step falls
    /// beside the step and stops at its wall. The other order would carry it over the step and land it on top.
    /// </summary>
    [Fact]
    public void TheOrderIsYThenXThenZ()
    {
        VoxelGrid grid = FloorGrid(8, 8);
        grid.Set(3, 1, 5, BlockId.RawStone);
        Aabb box = PlayerBoxAt(2.0f, 2.5f, 5.0f);

        SweepResult result = SweptAabb.Sweep(grid, box, new Vector3(1.0f, -1.0f, 0.0f));

        Assert.False(result.BlockedY);
        Assert.True(result.BlockedX);
        Assert.Equal(-1.0f, result.Allowed.Y);
        Aabb moved = box.Moved(result.Allowed);
        Assert.InRange(moved.Max.X, 3.0f - Skin - 1e-5f, 3.0f - Skin + 1e-5f);
        Assert.Equal(1.5f, moved.Min.Y);
    }

    /// <summary>A move of any size stops at the first block. One thousand meters in one tick still ends one skin before the wall.</summary>
    [Fact]
    public void AMoveOfAnySizeStopsAtTheFirstBlock()
    {
        VoxelGrid room = WalledRoom();
        Aabb box = PlayerBoxAt(1.0f, 1.5f, 3.0f);

        SweepResult result = SweptAabb.Sweep(room, box, new Vector3(1000.0f, 1000.0f, -1000.0f));

        Assert.True(result.BlockedX);
        Assert.True(result.BlockedY);
        Assert.True(result.BlockedZ);
        Aabb moved = box.Moved(result.Allowed);
        Assert.InRange(moved.Max.X, 6.0f - Skin - 1e-5f, 6.0f - Skin + 1e-5f);
        Assert.InRange(moved.Max.Y, 8.0f - Skin - 1e-5f, 8.0f - Skin + 1e-5f);
        Assert.InRange(moved.Min.Z, Skin - 1e-5f, Skin + 1e-5f);
    }

    /// <summary>The edge of the grid is a wall on every side, and a box never leaves the grid (D-237).</summary>
    [Fact]
    public void TheEdgeOfTheGridIsAWall()
    {
        VoxelGrid grid = new(4, 4, 4);
        Aabb box = PlayerBoxAt(1.7f, 1.0f, 1.7f);

        Aabb east = box.Moved(SweptAabb.Sweep(grid, box, new Vector3(9.0f, 0.0f, 0.0f)).Allowed);
        Aabb west = box.Moved(SweptAabb.Sweep(grid, box, new Vector3(-9.0f, 0.0f, 0.0f)).Allowed);
        Aabb up = box.Moved(SweptAabb.Sweep(grid, box, new Vector3(0.0f, 9.0f, 0.0f)).Allowed);
        Aabb down = box.Moved(SweptAabb.Sweep(grid, box, new Vector3(0.0f, -9.0f, 0.0f)).Allowed);
        Aabb south = box.Moved(SweptAabb.Sweep(grid, box, new Vector3(0.0f, 0.0f, 9.0f)).Allowed);
        Aabb north = box.Moved(SweptAabb.Sweep(grid, box, new Vector3(0.0f, 0.0f, -9.0f)).Allowed);

        Assert.InRange(east.Max.X, 4.0f - Skin - 1e-5f, 4.0f - Skin + 1e-5f);
        Assert.InRange(west.Min.X, Skin - 1e-5f, Skin + 1e-5f);
        Assert.InRange(up.Max.Y, 4.0f - Skin - 1e-5f, 4.0f - Skin + 1e-5f);
        Assert.InRange(down.Min.Y, Skin - 1e-5f, Skin + 1e-5f);
        Assert.InRange(south.Max.Z, 4.0f - Skin - 1e-5f, 4.0f - Skin + 1e-5f);
        Assert.InRange(north.Min.Z, Skin - 1e-5f, Skin + 1e-5f);
    }

    /// <summary>A move of zero on every axis moves nothing and cuts nothing.</summary>
    [Fact]
    public void AZeroMoveMovesNothing()
    {
        VoxelGrid room = WalledRoom();
        Aabb box = PlayerBoxAt(5.4f - Skin, 1.0f + Skin, 3.0f);

        SweepResult result = SweptAabb.Sweep(room, box, new Vector3(0.0f, -0.0f, 0.0f));

        Assert.Equal(new Vector3(0.0f, 0.0f, 0.0f), result.Allowed);
        Assert.False(result.BlockedX);
        Assert.False(result.BlockedY);
        Assert.False(result.BlockedZ);
    }

    /// <summary>A face on a block face is contact and not overlap. A face one ulp past it is overlap. A box past the grid overlaps the outside.</summary>
    [Fact]
    public void OverlapReadsTheOpenInterval()
    {
        VoxelGrid room = WalledRoom();

        Assert.False(SweptAabb.Overlaps(room, PlayerBoxAt(5.4f, 1.0f, 3.0f)));
        Assert.True(SweptAabb.Overlaps(room, new Aabb(new Vector3(5.4f, 1.0f, 3.0f), new Vector3(MathF.BitIncrement(6.0f), 2.8f, 3.6f))));
        Assert.True(SweptAabb.Overlaps(room, PlayerBoxAt(5.5f, 1.0f, 3.0f)));
        Assert.True(SweptAabb.Overlaps(room, PlayerBoxAt(2.0f, MathF.BitDecrement(1.0f), 3.0f)));

        Assert.False(SweptAabb.Overlaps(room, PlayerBoxAt(7.0f, 1.0f, 3.0f)));
        Assert.True(SweptAabb.Overlaps(room, PlayerBoxAt(6.5f, 1.0f, 3.0f)));

        Assert.False(SweptAabb.Overlaps(room, PlayerBoxAt(11.4f, 6.2f, 11.4f)));
        Assert.True(SweptAabb.Overlaps(room, PlayerBoxAt(11.5f, 1.0f, 3.0f)));
        Assert.True(SweptAabb.Overlaps(room, PlayerBoxAt(-0.1f, 1.0f, 3.0f)));
        Assert.True(SweptAabb.Overlaps(room, PlayerBoxAt(3.0f, 6.3f, 3.0f)));
        Assert.True(SweptAabb.Overlaps(room, PlayerBoxAt(3.0f, 1.0f, float.NaN)));
    }

    /// <summary>A box inside rock has no correct move, and the sweep reports it instead of a number (T-2).</summary>
    [Fact]
    public void AnOverlappingBoxIsAnError()
    {
        VoxelGrid room = WalledRoom();
        Aabb box = PlayerBoxAt(5.8f, 1.5f, 3.0f);

        ContextException error = Assert.Throws<ContextException>(() => SweptAabb.Sweep(room, box, new Vector3(0.0f, -1.0f, 0.0f)));
        Assert.Contains("overlaps a solid cell", error.Message, StringComparison.Ordinal);
        Assert.Contains("min=", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A coordinate that is not a number, and a box with no volume, are errors that name the part at fault (T-2).</summary>
    [Fact]
    public void ABadBoxOrMoveIsAnError()
    {
        VoxelGrid room = WalledRoom();
        Aabb box = PlayerBoxAt(3.0f, 1.5f, 3.0f);

        ContextException nan = Assert.Throws<ContextException>(() => SweptAabb.Sweep(room, box, new Vector3(0.0f, float.NaN, 0.0f)));
        Assert.Contains("name=delta", nan.Message, StringComparison.Ordinal);

        ContextException infinite = Assert.Throws<ContextException>(() => SweptAabb.Sweep(room, box, new Vector3(float.PositiveInfinity, 0.0f, 0.0f)));
        Assert.Contains("name=delta", infinite.Message, StringComparison.Ordinal);

        Aabb badCorner = new(new Vector3(float.NaN, 1.5f, 3.0f), box.Max);
        ContextException corner = Assert.Throws<ContextException>(() => SweptAabb.Sweep(room, badCorner, new Vector3(0.0f, 0.0f, 0.0f)));
        Assert.Contains("name=box.Min", corner.Message, StringComparison.Ordinal);

        Aabb flat = new(new Vector3(3.0f, 1.5f, 3.0f), new Vector3(3.6f, 1.5f, 3.6f));
        ContextException volume = Assert.Throws<ContextException>(() => SweptAabb.Sweep(room, flat, new Vector3(0.0f, 0.0f, 0.0f)));
        Assert.Contains("no volume", volume.Message, StringComparison.Ordinal);
    }

    /// <summary>The vector sum and the moved box read component by component.</summary>
    [Fact]
    public void VectorSumAndMovedBox()
    {
        Vector3 sum = new Vector3(1.0f, 2.0f, 3.0f) + new Vector3(0.5f, -2.0f, 4.0f);
        Assert.Equal(new Vector3(1.5f, 0.0f, 7.0f), sum);

        Aabb moved = PlayerBoxAt(1.0f, 2.0f, 3.0f).Moved(new Vector3(1.0f, 1.0f, 1.0f));
        Assert.Equal(PlayerBoxAt(2.0f, 3.0f, 4.0f), moved);
    }
}
