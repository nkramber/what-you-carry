using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// The walkable cells that a body reaches from one floor cell, with the count of moves to each (D-165, D-256).
/// </summary>
/// <remarks>
/// <para>
/// A floor cell is a rock cell with two air cells above it, so the box of D-165 stands on it. A move goes to one
/// of the four neighbor columns. It is a step of at most one block up, or a drop of any depth. A step up needs
/// the block of the step, two air cells over it, and a third air cell over the start for the jump. A flat move
/// or a drop needs two air cells in the neighbor column at the height of the start, and the body then lands on
/// the first rock below.
/// </para>
/// <para>
/// The search is a breadth-first walk over an array queue, so the distance of a cell is the count of moves of
/// a shortest path. It reads the grid alone and steps in integers, so one grid gives one answer everywhere.
/// A body can cut a corner that this search does not, so the search reaches no cell that a body cannot.
/// </para>
/// </remarks>
public sealed class Reachability
{
    private const int Unreached = -1;

    private readonly VoxelGrid grid;
    private readonly int[] distance;
    private readonly int[] parent;

    private Reachability(VoxelGrid grid, Cell start, int[] distance, int[] parent)
    {
        this.grid = grid;
        this.Start = start;
        this.distance = distance;
        this.parent = parent;
    }

    /// <summary>The floor cell that the search started from.</summary>
    public Cell Start { get; }

    /// <summary>Answers whether a body stands on the cell: rock, with two air cells above it. A cell outside the grid is no floor.</summary>
    public static bool IsFloor(VoxelGrid grid, Cell cell)
    {
        return grid.Contains(cell.X, cell.Y, cell.Z)
            && grid.IsSolid(cell.X, cell.Y, cell.Z)
            && !grid.IsSolid(cell.X, cell.Y + 1, cell.Z)
            && !grid.IsSolid(cell.X, cell.Y + 2, cell.Z);
    }

    /// <summary>The search from one floor cell over the whole grid.</summary>
    /// <exception cref="ContextException">The start is not a floor cell.</exception>
    public static Reachability From(VoxelGrid grid, Cell start)
    {
        if (!IsFloor(grid, start))
        {
            ContextException error = new($"The reachability search starts at {start}, and that is not a floor cell with two air cells above it.");
            error.AddContext("start", start.ToString());
            throw error;
        }

        int cellCount = grid.SizeX * grid.SizeY * grid.SizeZ;
        int[] distance = new int[cellCount];
        int[] parent = new int[cellCount];
        for (int index = 0; index < cellCount; index++)
        {
            distance[index] = Unreached;
            parent[index] = Unreached;
        }

        // Each cell enters the queue at most once, so an array of one slot per cell never fills.
        int[] queue = new int[cellCount];
        int head = 0;
        int tail = 0;
        int startIndex = Index(grid, start.X, start.Y, start.Z);
        distance[startIndex] = 0;
        queue[tail++] = startIndex;

        int[] stepX = [1, -1, 0, 0];
        int[] stepZ = [0, 0, 1, -1];
        while (head < tail)
        {
            int index = queue[head++];
            int x = index % grid.SizeX;
            int z = (index / grid.SizeX) % grid.SizeZ;
            int y = index / (grid.SizeX * grid.SizeZ);
            for (int direction = 0; direction < 4; direction++)
            {
                int landingY = Landing(grid, x, y, z, x + stepX[direction], z + stepZ[direction]);
                if (landingY == Unreached)
                {
                    continue;
                }

                int landing = Index(grid, x + stepX[direction], landingY, z + stepZ[direction]);
                if (distance[landing] != Unreached)
                {
                    continue;
                }

                distance[landing] = distance[index] + 1;
                parent[landing] = index;
                queue[tail++] = landing;
            }
        }

        return new Reachability(grid, start, distance, parent);
    }

    /// <summary>Answers whether the search reached the cell.</summary>
    public bool IsReachable(Cell cell)
    {
        return this.grid.Contains(cell.X, cell.Y, cell.Z) && this.distance[Index(this.grid, cell.X, cell.Y, cell.Z)] != Unreached;
    }

    /// <summary>The count of moves of a shortest path from the start to the cell.</summary>
    /// <exception cref="ContextException">The search did not reach the cell.</exception>
    public int Distance(Cell cell)
    {
        this.CheckReached(cell);
        return this.distance[Index(this.grid, cell.X, cell.Y, cell.Z)];
    }

    /// <summary>A shortest path from the start to the cell, both included, as the floor cells in walk order.</summary>
    /// <exception cref="ContextException">The search did not reach the cell.</exception>
    public IReadOnlyList<Cell> PathTo(Cell cell)
    {
        this.CheckReached(cell);

        List<Cell> backward = [];
        int index = Index(this.grid, cell.X, cell.Y, cell.Z);
        while (index != Unreached)
        {
            backward.Add(new Cell(index % this.grid.SizeX, index / (this.grid.SizeX * this.grid.SizeZ), (index / this.grid.SizeX) % this.grid.SizeZ));
            index = this.parent[index];
        }

        List<Cell> path = [];
        for (int position = backward.Count - 1; position >= 0; position--)
        {
            path.Add(backward[position]);
        }

        return path;
    }

    /// <summary>
    /// The floor row that a body reaches in the neighbor column from a floor cell, or minus one when no move
    /// leads there. A step up comes first, then a flat move or a drop.
    /// </summary>
    public static int Landing(VoxelGrid grid, int x, int y, int z, int neighborX, int neighborZ)
    {
        bool stepUp = grid.IsSolid(neighborX, y + 1, neighborZ)
            && !grid.IsSolid(neighborX, y + 2, neighborZ)
            && !grid.IsSolid(neighborX, y + 3, neighborZ)
            && !grid.IsSolid(x, y + 3, z);
        if (stepUp)
        {
            return y + 1;
        }

        if (grid.IsSolid(neighborX, y + 1, neighborZ) || grid.IsSolid(neighborX, y + 2, neighborZ))
        {
            return Unreached;
        }

        // The body falls to the first rock at or below the height of the start. The cells on the way down are
        // air by the scan, so the landing has its two air cells.
        int landingY = y;
        while (landingY >= 0 && !grid.IsSolid(neighborX, landingY, neighborZ))
        {
            landingY--;
        }

        return landingY;
    }

    /// <summary>The array index of one cell: x fastest, then z, then y, as the grid stores it.</summary>
    private static int Index(VoxelGrid grid, int x, int y, int z)
    {
        return x + (grid.SizeX * (z + (grid.SizeZ * y)));
    }

    /// <summary>Stops a read of a cell that the search did not reach (T-2).</summary>
    private void CheckReached(Cell cell)
    {
        if (this.IsReachable(cell))
        {
            return;
        }

        ContextException error = new($"The reachability search from {this.Start} did not reach {cell}.");
        error.AddContext("start", this.Start.ToString());
        error.AddContext("cell", cell.ToString());
        error.AddContext("sizeX", ((long)this.grid.SizeX).ToString(CultureInfo.InvariantCulture));
        error.AddContext("sizeY", ((long)this.grid.SizeY).ToString(CultureInfo.InvariantCulture));
        error.AddContext("sizeZ", ((long)this.grid.SizeZ).ToString(CultureInfo.InvariantCulture));
        throw error;
    }
}
