using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// The walkable cells that a body reaches from one floor cell, with the count of moves to each (D-165, D-256,
/// D-345).
/// </summary>
/// <remarks>
/// <para>
/// The move rule is <see cref="GridMoves"/>: one block up, any drop, or a walk along a ramp. The generator reads
/// this search to place the stairwell and to confirm that a body reaches every chamber, every tier, and every
/// shaft landing. <see cref="GridPathfinder"/> searches the same rule for the AI.
/// </para>
/// <para>
/// The search is a breadth-first walk over an array queue, so the distance of a cell is the count of moves of
/// a shortest path. It reads the grid alone and steps in integers, so one grid gives one answer everywhere.
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

    /// <summary>The search from one floor cell over the whole grid.</summary>
    /// <exception cref="ContextException">The start is not a floor cell.</exception>
    public static Reachability From(VoxelGrid grid, Cell start)
    {
        if (!GridMoves.IsFloor(grid, start))
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

        while (head < tail)
        {
            int index = queue[head++];
            int x = index % grid.SizeX;
            int z = (index / grid.SizeX) % grid.SizeZ;
            int y = index / (grid.SizeX * grid.SizeZ);
            for (int direction = 0; direction < GridMoves.Directions; direction++)
            {
                int neighborX = x + GridMoves.StepX[direction];
                int neighborZ = z + GridMoves.StepZ[direction];
                int landingY = GridMoves.Move(grid, x, y, z, neighborX, neighborZ);
                if (landingY == GridMoves.NoMove)
                {
                    continue;
                }

                int landing = Index(grid, neighborX, landingY, neighborZ);
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
