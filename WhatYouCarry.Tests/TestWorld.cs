using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Tests;

/// <summary>The grid and the spawn point that the loop tests share (D-236). A flat stone floor under open air.</summary>
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
        VoxelGrid grid = new(Side, Height, Side);
        for (int x = 0; x < Side; x++)
        {
            for (int z = 0; z < Side; z++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
            }
        }

        return grid;
    }

    /// <summary>A loop at tick zero on the flat floor.</summary>
    public static SimulationLoop NewLoop(ulong seed)
    {
        return new SimulationLoop(seed, FlatFloor(), Spawn);
    }
}
