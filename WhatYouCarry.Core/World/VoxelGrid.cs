using System.Globalization;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.World;

/// <summary>
/// The voxel grid of one floor: a flat array of one-byte block ids, at most 128 by 32 by 128 (D-78, D-164).
/// </summary>
/// <remarks>
/// <para>
/// The frame is the frame of Godot (D-234): right-handed, Y up, meters. Block (x, y, z) fills the cube from
/// (x, y, z) to (x + 1, y + 1, z + 1). The array runs x fastest, then z, then y, so one row of the floor plan is
/// one contiguous run.
/// </para>
/// <para>
/// A cell outside the grid is solid for collision, so the edge of the world is a wall and a body can never leave
/// the grid (D-237). A direct read outside the grid is still an error, because an absent block is not a block
/// (T-2). Only <see cref="IsSolid"/> treats the outside as rock.
/// </para>
/// </remarks>
public sealed class VoxelGrid
{
    /// <summary>The largest size of a floor along X, in blocks (D-164).</summary>
    public const int MaxSizeX = 128;

    /// <summary>The largest size of a floor along Y, in blocks (D-164).</summary>
    public const int MaxSizeY = 32;

    /// <summary>The largest size of a floor along Z, in blocks (D-164).</summary>
    public const int MaxSizeZ = 128;

    private readonly byte[] blocks;

    /// <summary>A grid of the given size, with every cell air.</summary>
    /// <exception cref="ContextException">A size is below one or above its limit.</exception>
    public VoxelGrid(int sizeX, int sizeY, int sizeZ)
    {
        CheckSize("sizeX", sizeX, MaxSizeX);
        CheckSize("sizeY", sizeY, MaxSizeY);
        CheckSize("sizeZ", sizeZ, MaxSizeZ);

        this.SizeX = sizeX;
        this.SizeY = sizeY;
        this.SizeZ = sizeZ;
        this.blocks = new byte[sizeX * sizeY * sizeZ];
    }

    /// <summary>The count of blocks along X.</summary>
    public int SizeX { get; }

    /// <summary>The count of blocks along Y.</summary>
    public int SizeY { get; }

    /// <summary>The count of blocks along Z.</summary>
    public int SizeZ { get; }

    /// <summary>Answers whether the cell is inside the grid.</summary>
    public bool Contains(int x, int y, int z)
    {
        return x >= 0 && x < this.SizeX && y >= 0 && y < this.SizeY && z >= 0 && z < this.SizeZ;
    }

    /// <summary>The block in one cell.</summary>
    /// <exception cref="ContextException">The cell is outside the grid.</exception>
    public BlockId Get(int x, int y, int z)
    {
        this.CheckInside(x, y, z);
        return (BlockId)this.blocks[this.Index(x, y, z)];
    }

    /// <summary>Puts one block in one cell.</summary>
    /// <exception cref="ContextException">The cell is outside the grid, or the id is not a declared block.</exception>
    public void Set(int x, int y, int z, BlockId block)
    {
        this.CheckInside(x, y, z);

        // An explicit bound, because Enum.IsDefined reads the enum through reflection, and Core has none (G-2).
        // `EveryDeclaredBlockIsAccepted` walks the declared values, so a new value fails the test until this
        // bound names it.
        if (block != BlockId.Air && block != BlockId.RawStone)
        {
            ContextException error = new($"The block id {(int)block} is not a declared block. The declared ids are 0 for air and 1 for raw stone (D-239).");
            error.AddContext("block", ((long)block).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        this.blocks[this.Index(x, y, z)] = (byte)block;
    }

    /// <summary>
    /// Answers whether a body stops at the cell. Every block but air is solid, and every cell outside the grid is
    /// solid, so the edge of the world is a wall (D-237).
    /// </summary>
    public bool IsSolid(int x, int y, int z)
    {
        if (!this.Contains(x, y, z))
        {
            return true;
        }

        return this.blocks[this.Index(x, y, z)] != (byte)BlockId.Air;
    }

    /// <summary>
    /// Answers whether any cell of an inclusive range is solid. A range that reaches outside the grid holds a
    /// solid cell, because the outside is solid (D-237). An empty range holds no cell.
    /// </summary>
    public bool IsAnySolid(int lowX, int highX, int lowY, int highY, int lowZ, int highZ)
    {
        if (lowX > highX || lowY > highY || lowZ > highZ)
        {
            return false;
        }

        if (lowX < 0 || highX >= this.SizeX || lowY < 0 || highY >= this.SizeY || lowZ < 0 || highZ >= this.SizeZ)
        {
            return true;
        }

        for (int y = lowY; y <= highY; y++)
        {
            for (int z = lowZ; z <= highZ; z++)
            {
                for (int x = lowX; x <= highX; x++)
                {
                    if (this.blocks[this.Index(x, y, z)] != (byte)BlockId.Air)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>The array index of one cell: x fastest, then z, then y.</summary>
    private int Index(int x, int y, int z)
    {
        return x + (this.SizeX * (z + (this.SizeZ * y)));
    }

    /// <summary>Stops a read or a write outside the grid, and names the cell and the size (T-2).</summary>
    private void CheckInside(int x, int y, int z)
    {
        if (this.Contains(x, y, z))
        {
            return;
        }

        ContextException error = new($"The cell ({x}, {y}, {z}) is outside the grid of {this.SizeX} by {this.SizeY} by {this.SizeZ} blocks.");
        error.AddContext("x", ((long)x).ToString(CultureInfo.InvariantCulture));
        error.AddContext("y", ((long)y).ToString(CultureInfo.InvariantCulture));
        error.AddContext("z", ((long)z).ToString(CultureInfo.InvariantCulture));
        error.AddContext("sizeX", ((long)this.SizeX).ToString(CultureInfo.InvariantCulture));
        error.AddContext("sizeY", ((long)this.SizeY).ToString(CultureInfo.InvariantCulture));
        error.AddContext("sizeZ", ((long)this.SizeZ).ToString(CultureInfo.InvariantCulture));
        throw error;
    }

    /// <summary>Stops a size outside [1, limit], and names the axis (D-164, T-2).</summary>
    private static void CheckSize(string name, int size, int limit)
    {
        if (size >= 1 && size <= limit)
        {
            return;
        }

        ContextException error = new($"The grid size {name} must be from 1 to {limit} blocks (D-164), and it is {size}.");
        error.AddContext("axis", name);
        error.AddContext("size", ((long)size).ToString(CultureInfo.InvariantCulture));
        error.AddContext("limit", ((long)limit).ToString(CultureInfo.InvariantCulture));
        throw error;
    }
}
