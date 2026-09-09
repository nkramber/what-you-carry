using System;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Tests;

/// <summary>The grids and the spawn points that the loop tests share (D-236): a flat stone floor under open air, and a random world.</summary>
internal static class TestWorld
{
    /// <summary>The side of the flat floor, in blocks.</summary>
    public const int Side = 8;

    /// <summary>The height of the flat floor grid, in blocks.</summary>
    public const int Height = 6;

    /// <summary>The top face of the floor, which is the lowest feet position of a resting body.</summary>
    public const float FloorTop = 1.0f;

    /// <summary>The feet center over the middle of the floor.</summary>
    public static readonly Vector3 Spawn = new(4.5f, FloorTop, 4.5f);

    /// <summary>A grid with one row of stone at the bottom and air above it.</summary>
    public static VoxelGrid FlatFloor()
    {
        return FlatFloor(Side, Height);
    }

    /// <summary>A grid of the given side and height with one row of stone at the bottom and air above it.</summary>
    public static VoxelGrid FlatFloor(int side, int height)
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

    /// <summary>
    /// A random grid: a stone floor, then stone in one quarter of the cells of the five rows above it. The spawn
    /// is a column with two air cells over the floor, so the body fits. A failure of the search is a defect of
    /// the test and never a pass.
    /// </summary>
    public static (VoxelGrid Grid, Vector3 Spawn) RandomWorld(Random random)
    {
        VoxelGrid grid = new(10, 8, 10);
        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
                for (int y = 1; y <= 5; y++)
                {
                    if (random.Next(4) == 0)
                    {
                        grid.Set(x, y, z, BlockId.RawStone);
                    }
                }
            }
        }

        for (int attempt = 0; attempt < 1000; attempt++)
        {
            int x = random.Next(10);
            int z = random.Next(10);
            if (!grid.IsSolid(x, 1, z) && !grid.IsSolid(x, 2, z))
            {
                return (grid, new Vector3(x + 0.5f, 1.0f, z + 0.5f));
            }
        }

        throw new InvalidOperationException("No column of the random grid has two air cells over the floor.");
    }

    /// <summary>A loop at tick zero on the flat floor.</summary>
    public static SimulationLoop NewLoop(ulong seed)
    {
        return new SimulationLoop(seed, FlatFloor(), Spawn);
    }
}
