using System;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The voxel grid (D-78, D-164, D-237, D-239; PR-7).</summary>
public sealed class VoxelGridTests
{
    /// <summary>The limits of D-164 hold, and a size outside them is an error that names the axis (T-2).</summary>
    [Theory]
    [InlineData(0, 1, 1, "sizeX")]
    [InlineData(129, 1, 1, "sizeX")]
    [InlineData(1, 0, 1, "sizeY")]
    [InlineData(1, 33, 1, "sizeY")]
    [InlineData(1, 1, 0, "sizeZ")]
    [InlineData(1, 1, 129, "sizeZ")]
    public void ASizeOutsideTheLimitsIsAnError(int sizeX, int sizeY, int sizeZ, string axis)
    {
        ContextException error = Assert.Throws<ContextException>(() => new VoxelGrid(sizeX, sizeY, sizeZ));
        Assert.Contains($"axis={axis}", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-164", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The largest grid of D-164 fits, and its far corner reads and writes.</summary>
    [Fact]
    public void TheLargestGridFits()
    {
        VoxelGrid grid = new(VoxelGrid.MaxSizeX, VoxelGrid.MaxSizeY, VoxelGrid.MaxSizeZ);
        Assert.Equal(128, grid.SizeX);
        Assert.Equal(32, grid.SizeY);
        Assert.Equal(128, grid.SizeZ);

        grid.Set(127, 31, 127, BlockId.RawStone);
        Assert.Equal(BlockId.RawStone, grid.Get(127, 31, 127));
        Assert.Equal(BlockId.Air, grid.Get(0, 0, 0));
    }

    /// <summary>A new grid is all air, a write reaches one cell alone, and a read gives it back.</summary>
    [Fact]
    public void ACellRoundTrips()
    {
        VoxelGrid grid = new(4, 3, 5);
        Assert.Equal(BlockId.Air, grid.Get(1, 2, 3));

        grid.Set(1, 2, 3, BlockId.RawStone);
        Assert.Equal(BlockId.RawStone, grid.Get(1, 2, 3));
        Assert.Equal(BlockId.Air, grid.Get(1, 2, 4));
        Assert.Equal(BlockId.Air, grid.Get(2, 2, 3));
        Assert.Equal(BlockId.Air, grid.Get(1, 1, 3));

        grid.Set(1, 2, 3, BlockId.Air);
        Assert.Equal(BlockId.Air, grid.Get(1, 2, 3));
    }

    /// <summary>Two cells that differ on one axis alone never share one array slot, on every axis and at every edge.</summary>
    [Fact]
    public void EveryCellHasItsOwnSlot()
    {
        VoxelGrid grid = new(3, 4, 5);
        for (int y = 0; y < 4; y++)
        {
            for (int z = 0; z < 5; z++)
            {
                for (int x = 0; x < 3; x++)
                {
                    grid.Set(x, y, z, BlockId.RawStone);
                    int count = 0;
                    for (int otherY = 0; otherY < 4; otherY++)
                    {
                        for (int otherZ = 0; otherZ < 5; otherZ++)
                        {
                            for (int otherX = 0; otherX < 3; otherX++)
                            {
                                if (grid.Get(otherX, otherY, otherZ) == BlockId.RawStone)
                                {
                                    count++;
                                }
                            }
                        }
                    }

                    Assert.True(count == 1, $"Cell ({x}, {y}, {z}) shares a slot: {count} cells read as stone.");
                    grid.Set(x, y, z, BlockId.Air);
                }
            }
        }
    }

    /// <summary>A read or a write outside the grid is an error that names the cell and the size (T-2).</summary>
    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(4, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 3, 0)]
    [InlineData(0, 0, -1)]
    [InlineData(0, 0, 5)]
    public void ACellOutsideTheGridIsAnError(int x, int y, int z)
    {
        VoxelGrid grid = new(4, 3, 5);
        Assert.False(grid.Contains(x, y, z));

        ContextException read = Assert.Throws<ContextException>(() => grid.Get(x, y, z));
        Assert.Contains($"x={x}", read.Message, StringComparison.Ordinal);
        Assert.Contains($"y={y}", read.Message, StringComparison.Ordinal);
        Assert.Contains($"z={z}", read.Message, StringComparison.Ordinal);
        Assert.Contains("sizeX=4", read.Message, StringComparison.Ordinal);

        Assert.Throws<ContextException>(() => grid.Set(x, y, z, BlockId.RawStone));
    }

    /// <summary>Every cell outside the grid is solid for collision, and a cell inside is solid only with a block in it (D-237).</summary>
    [Fact]
    public void OutsideTheGridIsSolid()
    {
        VoxelGrid grid = new(4, 3, 5);
        grid.Set(2, 1, 2, BlockId.RawStone);

        Assert.True(grid.IsSolid(-1, 1, 2));
        Assert.True(grid.IsSolid(4, 1, 2));
        Assert.True(grid.IsSolid(2, -1, 2));
        Assert.True(grid.IsSolid(2, 3, 2));
        Assert.True(grid.IsSolid(2, 1, -1));
        Assert.True(grid.IsSolid(2, 1, 5));

        Assert.True(grid.IsSolid(2, 1, 2));
        Assert.False(grid.IsSolid(2, 2, 2));
        Assert.False(grid.IsSolid(0, 0, 0));
    }

    /// <summary>Every declared block id is accepted, so the explicit bound in the grid names each one.</summary>
    [Fact]
    public void EveryDeclaredBlockIsAccepted()
    {
        VoxelGrid grid = new(1, 1, 1);
        foreach (BlockId block in Enum.GetValues<BlockId>())
        {
            grid.Set(0, 0, 0, block);
            Assert.Equal(block, grid.Get(0, 0, 0));
        }
    }

    /// <summary>The ids of D-239 and D-259 hold, and an id that no PR declared is an error that names it (T-2).</summary>
    [Fact]
    public void AnUnknownBlockIdIsAnError()
    {
        Assert.Equal(0, (int)BlockId.Air);
        Assert.Equal(1, (int)BlockId.RawStone);
        Assert.Equal(2, (int)BlockId.HewnStone);
        Assert.Equal(3, (int)BlockId.TimberBeam);
        Assert.Equal(4, (int)BlockId.OreVein);
        Assert.Equal(5, (int)BlockId.StillWater);
        Assert.Equal(6, (int)BlockId.Rubble);
        Assert.Equal(7, (int)BlockId.Plank);

        VoxelGrid grid = new(1, 1, 1);
        ContextException error = Assert.Throws<ContextException>(() => grid.Set(0, 0, 0, (BlockId)8));
        Assert.Contains("block=8", error.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => grid.Set(0, 0, 0, (BlockId)255));
        Assert.Equal(BlockId.Air, grid.Get(0, 0, 0));
    }

    /// <summary>PR-59 exit test 3. Every declared id has a solid rule: air and still water let a body through, and every other block stops it (D-239, D-258).</summary>
    [Fact]
    public void EveryBlockIdIsDeclared()
    {
        VoxelGrid grid = new(1, 1, 1);
        foreach (BlockId block in Enum.GetValues<BlockId>())
        {
            grid.Set(0, 0, 0, block);
            bool passable = block == BlockId.Air || block == BlockId.StillWater;
            Assert.Equal(!passable, grid.IsSolid(0, 0, 0));
            Assert.Equal(!passable, VoxelGrid.IsSolidBlock(block));
            Assert.Equal(!passable, grid.IsAnySolid(0, 0, 0, 0, 0, 0));
        }

        Assert.Throws<ContextException>(() => grid.Set(0, 0, 0, (BlockId)8));
    }

    /// <summary>The range query reads every cell of the range, treats the outside as solid, and holds no cell for an empty range.</summary>
    [Fact]
    public void TheRangeQueryReadsTheRange()
    {
        VoxelGrid grid = new(4, 3, 5);
        Assert.False(grid.IsAnySolid(0, 3, 0, 2, 0, 4));

        grid.Set(3, 2, 4, BlockId.RawStone);
        Assert.True(grid.IsAnySolid(0, 3, 0, 2, 0, 4));
        Assert.True(grid.IsAnySolid(3, 3, 2, 2, 4, 4));
        Assert.False(grid.IsAnySolid(0, 2, 0, 2, 0, 4));
        Assert.False(grid.IsAnySolid(0, 3, 0, 1, 0, 4));
        Assert.False(grid.IsAnySolid(0, 3, 0, 2, 0, 3));

        // A range that reaches outside holds a solid cell, because the outside is solid (D-237).
        Assert.True(grid.IsAnySolid(-1, 0, 0, 0, 0, 0));
        Assert.True(grid.IsAnySolid(0, 0, 0, 0, 4, 5));
        Assert.True(grid.IsAnySolid(-1, -1, 0, 0, 0, 0));

        // An empty range holds no cell, inside or outside.
        Assert.False(grid.IsAnySolid(2, 1, 0, 2, 0, 4));
        Assert.False(grid.IsAnySolid(0, 3, 2, 1, 0, 4));
        Assert.False(grid.IsAnySolid(-1, -2, 0, 0, 0, 0));
    }
}
