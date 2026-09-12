using WhatYouCarry.Core.World;

namespace WhatYouCarry.Game.World;

/// <summary>
/// How a floor divides into chunks (D-291): 16 by 32 by 16 blocks, so a maximum floor of D-164 gives 64 chunks
/// in an 8 by 8 layout, and every floor fits in one chunk along Y. Each chunk is one mesh and one draw.
/// </summary>
public static class ChunkLayout
{
    /// <summary>The blocks of a chunk along X (D-291).</summary>
    public const int SizeX = 16;

    /// <summary>The blocks of a chunk along Y (D-291). The grid limit of D-164 is the same, so a chunk holds every row.</summary>
    public const int SizeY = VoxelGrid.MaxSizeY;

    /// <summary>The blocks of a chunk along Z (D-291).</summary>
    public const int SizeZ = 16;

    /// <summary>The budget of world meshes on one floor (D-291). The entity meshes come on top of it, one per entity.</summary>
    public const int WorldMeshBudget = (VoxelGrid.MaxSizeX / SizeX) * (VoxelGrid.MaxSizeZ / SizeZ);

    /// <summary>The count of chunks along X of a grid. The last chunk is partial when the size is not a multiple.</summary>
    public static int CountX(VoxelGrid grid)
    {
        return (grid.SizeX + SizeX - 1) / SizeX;
    }

    /// <summary>The count of chunks along Z of a grid.</summary>
    public static int CountZ(VoxelGrid grid)
    {
        return (grid.SizeZ + SizeZ - 1) / SizeZ;
    }

    /// <summary>The count of chunks of a grid, which is the count of world meshes of its floor.</summary>
    public static int Count(VoxelGrid grid)
    {
        return CountX(grid) * CountZ(grid);
    }
}
