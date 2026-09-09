using System;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The ray march through the grid (D-246, D-237; PR-8).</summary>
public sealed class GridRayTests
{
    /// <summary>A sixteen by eight by sixteen room over a stone floor, with a one-block wall across it at x = 6 from the floor to the top.</summary>
    private static VoxelGrid WalledRoom()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        for (int y = 1; y < 8; y++)
        {
            for (int z = 0; z < 16; z++)
            {
                grid.Set(6, y, z, BlockId.RawStone);
            }
        }

        return grid;
    }

    /// <summary>A segment through air has no hit, and its distance is the whole length.</summary>
    [Fact]
    public void ASegmentThroughAirHasNoHit()
    {
        RayHit hit = GridRay.FirstSolid(TestWorld.FlatFloor(16, 8), new Vector3(2.5f, 2.5f, 2.5f), new Vector3(5.5f, 3.5f, 5.5f));
        Assert.False(hit.Hit);
        Assert.InRange(hit.Distance, MathF.Sqrt(19.0f) - 1e-5f, MathF.Sqrt(19.0f) + 1e-5f);
    }

    /// <summary>A segment into a wall hits at the face, in both directions along the axis.</summary>
    [Fact]
    public void ASegmentIntoAWallHitsAtTheFace()
    {
        VoxelGrid room = WalledRoom();

        RayHit east = GridRay.FirstSolid(room, new Vector3(3.5f, 2.5f, 4.5f), new Vector3(7.5f, 2.5f, 4.5f));
        Assert.True(east.Hit);
        Assert.InRange(east.Distance, 2.5f - 1e-5f, 2.5f + 1e-5f);

        RayHit west = GridRay.FirstSolid(room, new Vector3(7.5f, 2.5f, 4.5f), new Vector3(3.5f, 2.5f, 4.5f));
        Assert.True(west.Hit);
        Assert.InRange(west.Distance, 0.5f - 1e-5f, 0.5f + 1e-5f);
    }

    /// <summary>A segment down hits the top of the floor, and a segment up hits the edge of the grid (D-237).</summary>
    [Fact]
    public void ASegmentAlongYHitsTheFloorAndTheEdge()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);

        RayHit down = GridRay.FirstSolid(grid, new Vector3(3.5f, 3.0f, 3.5f), new Vector3(3.5f, -1.0f, 3.5f));
        Assert.True(down.Hit);
        Assert.InRange(down.Distance, 2.0f - 1e-5f, 2.0f + 1e-5f);

        RayHit up = GridRay.FirstSolid(grid, new Vector3(3.5f, 3.0f, 3.5f), new Vector3(3.5f, 30.0f, 3.5f));
        Assert.True(up.Hit);
        Assert.InRange(up.Distance, 5.0f - 1e-5f, 5.0f + 1e-5f);
    }

    /// <summary>A segment of any length ends at the edge of the grid at the latest, because the outside is solid (D-237).</summary>
    [Fact]
    public void ASegmentOfAnyLengthEndsAtTheEdge()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        RayHit hit = GridRay.FirstSolid(grid, new Vector3(3.5f, 2.5f, 3.5f), new Vector3(3.5f, 2.5f, 100000.0f));
        Assert.True(hit.Hit);
        Assert.InRange(hit.Distance, 12.5f - 1e-3f, 12.5f + 1e-3f);
    }

    /// <summary>A diagonal through a corner passes one cell at a time, so it hits the block at the corner and never skips it.</summary>
    [Fact]
    public void ADiagonalThroughACornerHitsTheCornerBlock()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        grid.Set(4, 2, 4, BlockId.RawStone);

        RayHit hit = GridRay.FirstSolid(grid, new Vector3(3.2f, 2.5f, 3.2f), new Vector3(5.8f, 2.5f, 5.8f));
        Assert.True(hit.Hit);
        float expected = 0.8f * MathF.Sqrt(2.0f);
        Assert.InRange(hit.Distance, expected - 1e-4f, expected + 1e-4f);
    }

    /// <summary>A segment that starts on a cell boundary reads the cell on the far side of the boundary in the direction of travel.</summary>
    [Fact]
    public void AStartOnABoundaryReadsTheCellAhead()
    {
        VoxelGrid room = WalledRoom();
        RayHit toward = GridRay.FirstSolid(room, new Vector3(5.0f, 2.0f, 4.0f), new Vector3(6.5f, 2.0f, 4.0f));
        Assert.True(toward.Hit);
        Assert.InRange(toward.Distance, 1.0f - 1e-5f, 1.0f + 1e-5f);

        RayHit away = GridRay.FirstSolid(room, new Vector3(7.0f, 2.0f, 4.0f), new Vector3(5.0f, 2.0f, 4.0f));
        Assert.True(away.Hit);
        Assert.InRange(away.Distance, 0.0f, 1e-5f);
    }

    /// <summary>A segment with no length has no hit and no distance.</summary>
    [Fact]
    public void AZeroLengthSegmentHasNoHit()
    {
        RayHit hit = GridRay.FirstSolid(TestWorld.FlatFloor(16, 8), new Vector3(3.5f, 2.5f, 3.5f), new Vector3(3.5f, 2.5f, 3.5f));
        Assert.Equal(new RayHit(false, 0.0f), hit);
    }

    /// <summary>A start inside a solid cell, and a coordinate that is not a number, are errors that name the part at fault (T-2).</summary>
    [Fact]
    public void ABadStartIsAnError()
    {
        VoxelGrid room = WalledRoom();

        ContextException inside = Assert.Throws<ContextException>(() => GridRay.FirstSolid(room, new Vector3(6.5f, 2.5f, 4.5f), new Vector3(3.5f, 2.5f, 4.5f)));
        Assert.Contains("starts inside a solid cell", inside.Message, StringComparison.Ordinal);

        ContextException outside = Assert.Throws<ContextException>(() => GridRay.FirstSolid(room, new Vector3(-0.5f, 2.5f, 4.5f), new Vector3(3.5f, 2.5f, 4.5f)));
        Assert.Contains("starts inside a solid cell", outside.Message, StringComparison.Ordinal);

        ContextException nan = Assert.Throws<ContextException>(() => GridRay.FirstSolid(room, new Vector3(3.5f, 2.5f, 4.5f), new Vector3(float.NaN, 2.5f, 4.5f)));
        Assert.Contains("name=end", nan.Message, StringComparison.Ordinal);

        ContextException infinite = Assert.Throws<ContextException>(() => GridRay.FirstSolid(room, new Vector3(float.PositiveInfinity, 2.5f, 4.5f), new Vector3(3.5f, 2.5f, 4.5f)));
        Assert.Contains("name=start", infinite.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Over one thousand seeds in random grids, the march agrees with a walk along the segment in one-millimeter
    /// steps. No sample before the hit sits in rock, and the first sample in rock sits at the hit or, at a
    /// corner, just after it. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void TheMarchAgreesWithAFineWalk()
    {
        for (int seed = 1; seed <= 1000; seed++)
        {
            Random random = new(seed);
            (VoxelGrid grid, Vector3 spawn) = TestWorld.RandomWorld(random);
            Vector3 start = spawn + new Vector3((float)(random.NextDouble() - 0.5) * 0.9f, 0.1f + ((float)random.NextDouble() * 1.5f), (float)(random.NextDouble() - 0.5) * 0.9f);
            Vector3 end = new((float)(random.NextDouble() * 14.0) - 2.0f, (float)(random.NextDouble() * 12.0) - 2.0f, (float)(random.NextDouble() * 14.0) - 2.0f);

            RayHit hit = GridRay.FirstSolid(grid, start, end);
            Vector3 delta = end - start;
            float length = delta.Length();

            float firstRock = -1.0f;
            for (float distance = 0.0f; distance <= length; distance += 0.001f)
            {
                Vector3 point = start + (delta * (distance / length));
                if (grid.IsSolid((int)MathF.Floor(point.X), (int)MathF.Floor(point.Y), (int)MathF.Floor(point.Z)))
                {
                    firstRock = distance;
                    break;
                }
            }

            if (hit.Hit)
            {
                Assert.True(firstRock < 0.0f || hit.Distance <= firstRock + 1e-3f, $"Seed {seed}: the march hit at {hit.Distance}, and the walk found rock at {firstRock}.");
                Assert.True(firstRock < 0.0f || firstRock >= hit.Distance - 1e-3f, $"Seed {seed}: the walk found rock at {firstRock}, before the march hit at {hit.Distance}.");
            }
            else
            {
                Assert.True(firstRock < 0.0f, $"Seed {seed}: the march found no rock, and the walk found rock at {firstRock}.");
                Assert.True(hit.Distance == length, $"Seed {seed}: a miss carries the length {length}, and the march gave {hit.Distance}.");
            }
        }
    }

    /// <summary>The vector operations read component by component, and the length is the IEEE square root of the dot product.</summary>
    [Fact]
    public void VectorOperations()
    {
        Vector3 a = new(1.0f, 2.0f, 3.0f);
        Vector3 b = new(4.0f, -5.0f, 6.0f);
        Assert.Equal(new Vector3(-3.0f, 7.0f, -3.0f), a - b);
        Assert.Equal(new Vector3(2.0f, 4.0f, 6.0f), a * 2.0f);
        Assert.Equal(12.0f, Vector3.Dot(a, b));
        Assert.Equal(new Vector3(27.0f, 6.0f, -13.0f), Vector3.Cross(a, b));
        Assert.Equal(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Cross(new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f)));
        Assert.Equal(5.0f, new Vector3(3.0f, 0.0f, 4.0f).Length());
    }
}
