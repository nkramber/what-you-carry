using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Pathfinding;

/// <summary>
/// An A* search over the walkable cells of one grid (D-76). Every move is a move of <see cref="GridMoves"/>: one
/// block up, any drop, or a walk along a ramp to a side column, and a diagonal move to a corner column (D-165,
/// D-345, D-486).
/// </summary>
/// <remarks>
/// <para>
/// A side move costs <see cref="SideCost"/> and a diagonal move costs <see cref="DiagonalCost"/>, near the ratio of
/// the square root of two (D-487). The path that the search gives is then the shortest in distance over the ground,
/// and a straight line across open floor takes diagonal moves and not a staircase of side moves (F-107).
/// </para>
/// <para>
/// The estimate to the goal is the octile distance: a diagonal move for each step that X and Z share, and a side
/// move for each step that remains. A move changes X and Z by one at most and the row by any amount, so that
/// estimate never overstates the cost that remains, and it falls by at most the cost of a move. The first pop of
/// the goal then holds a shortest path.
/// </para>
/// <para>
/// The search reads the grid and the two cells alone, and it steps in integers, so one grid and one pair of cells
/// give one path everywhere (G-9). Two cells of one estimate and one cost pop in the order that they entered, so a
/// tie never reads a hash order or an address.
/// </para>
/// <para>
/// One pathfinder holds the arrays of one grid, and every search of that grid reuses them. A fresh array of one
/// slot per cell costs about a megabyte on a floor of 64 by 20 by 64 blocks, and the AI searches many times each
/// second (D-109). A stamp of the search number marks which slots the current search wrote, so a search clears
/// nothing. The simulation runs on one thread, so one pathfinder serves every brain of a floor (G-4).
/// </para>
/// <para>
/// A goal that no move reaches is an answer and not a fault: <see cref="TryFind"/> gives false, and the caller
/// holds its ground. A start or a goal that is no floor cell is a fault, because the caller read a cell that no
/// body stands on (T-2).
/// </para>
/// </remarks>
public sealed class GridPathfinder
{
    /// <summary>The cost of a side move (D-487).</summary>
    public const int SideCost = 10;

    /// <summary>The cost of a diagonal move (D-487).</summary>
    public const int DiagonalCost = 14;

    /// <summary>The value in the parent array of the cell that a search started from.</summary>
    private const int NoParent = -1;

    private readonly VoxelGrid grid;
    private readonly int[] cost;
    private readonly int[] parent;
    private readonly int[] written;
    private readonly int[] closed;
    private readonly OpenSet open;
    private int search;

    /// <summary>A pathfinder for one grid. It holds four arrays of one slot per cell, and every search reuses them.</summary>
    public GridPathfinder(VoxelGrid grid)
    {
        this.grid = grid;
        int cellCount = grid.SizeX * grid.SizeY * grid.SizeZ;
        this.cost = new int[cellCount];
        this.parent = new int[cellCount];
        this.written = new int[cellCount];
        this.closed = new int[cellCount];
        this.open = new OpenSet(cellCount);
    }

    /// <summary>The count of searches that this pathfinder ran. It is the stamp of the newest one.</summary>
    public int Searches => this.search;

    /// <summary>The count of cells that the last search took out of its open set. It measures the cost of a search (G-17).</summary>
    public int LastExpansions { get; private set; }

    /// <summary>
    /// A shortest path of moves from the start to the goal, both cells included, in walk order. Gives false when no
    /// move rule path joins them, and the path is then empty.
    /// </summary>
    /// <exception cref="ContextException">The start or the goal is not a floor cell with two open cells over it.</exception>
    public bool TryFind(Cell start, Cell goal, out IReadOnlyList<Cell> path)
    {
        this.CheckFloor(start, "start");
        this.CheckFloor(goal, "goal");

        if (start == goal)
        {
            path = [start];
            this.LastExpansions = 0;
            return true;
        }

        // The stamp tells a slot of this search from a slot that an older search wrote, so no array needs a clear.
        this.search++;
        this.LastExpansions = 0;
        this.open.Clear();

        int startIndex = this.Index(start.X, start.Y, start.Z);
        int goalIndex = this.Index(goal.X, goal.Y, goal.Z);
        this.written[startIndex] = this.search;
        this.cost[startIndex] = 0;
        this.parent[startIndex] = NoParent;
        this.open.Push(startIndex, Estimate(start, goal));

        while (this.open.TryPop(out int index))
        {
            if (index == goalIndex)
            {
                path = this.Walk(goalIndex);
                return true;
            }

            // A cell enters the open set once for each better cost that the search found for it, so the later
            // entries of a cell that already left it are stale and carry no new path.
            if (this.closed[index] == this.search)
            {
                continue;
            }

            this.closed[index] = this.search;
            this.LastExpansions++;
            int x = index % this.grid.SizeX;
            int z = (index / this.grid.SizeX) % this.grid.SizeZ;
            int y = index / (this.grid.SizeX * this.grid.SizeZ);
            for (int direction = 0; direction < GridMoves.Directions; direction++)
            {
                int neighborX = x + GridMoves.StepX[direction];
                int neighborZ = z + GridMoves.StepZ[direction];
                int landingY = GridMoves.Move(this.grid, x, y, z, neighborX, neighborZ);
                this.Offer(index, new Cell(neighborX, landingY, neighborZ), SideCost, goal);
            }

            // The two orders of a diagonal move can land on two rows, so each order offers its own landing. An order
            // that lands where the other did offers no better cost, and the search drops it (D-486).
            for (int corner = 0; corner < GridMoves.Corners; corner++)
            {
                int stepX = GridMoves.CornerStepX[corner];
                int stepZ = GridMoves.CornerStepZ[corner];
                int alongXFirst = GridMoves.DiagonalMove(this.grid, x, y, z, stepX, stepZ, true);
                this.Offer(index, new Cell(x + stepX, alongXFirst, z + stepZ), DiagonalCost, goal);
                int alongZFirst = GridMoves.DiagonalMove(this.grid, x, y, z, stepX, stepZ, false);
                this.Offer(index, new Cell(x + stepX, alongZFirst, z + stepZ), DiagonalCost, goal);
            }
        }

        path = [];
        return false;
    }

    /// <summary>
    /// The cost that remains at least: the octile distance (D-487). A diagonal move covers one step along X and one
    /// along Z, and a side move covers each step that remains.
    /// </summary>
    public static int Estimate(Cell from, Cell goal)
    {
        int alongX = from.X > goal.X ? from.X - goal.X : goal.X - from.X;
        int alongZ = from.Z > goal.Z ? from.Z - goal.Z : goal.Z - from.Z;
        int shared = alongX < alongZ ? alongX : alongZ;
        int straight = (alongX + alongZ) - (2 * shared);
        return (DiagonalCost * shared) + (SideCost * straight);
    }

    /// <summary>
    /// Offers one move from an open cell to a landing cell. A landing row of <see cref="GridMoves.NoMove"/> offers
    /// nothing. A landing that this search reached at an equal or lower cost keeps its path.
    /// </summary>
    private void Offer(int from, Cell landing, int moveCost, Cell goal)
    {
        if (landing.Y == GridMoves.NoMove)
        {
            return;
        }

        int index = this.Index(landing.X, landing.Y, landing.Z);
        int next = this.cost[from] + moveCost;
        bool known = this.written[index] == this.search;
        if (known && next >= this.cost[index])
        {
            return;
        }

        this.written[index] = this.search;
        this.cost[index] = next;
        this.parent[index] = from;
        this.open.Push(index, next + Estimate(landing, goal));
    }

    /// <summary>The path from the start to one cell, read backward through the parent array and then turned around.</summary>
    private IReadOnlyList<Cell> Walk(int goalIndex)
    {
        List<Cell> backward = [];
        int index = goalIndex;
        while (index != NoParent)
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
    private int Index(int x, int y, int z)
    {
        return x + (this.grid.SizeX * (z + (this.grid.SizeZ * y)));
    }

    /// <summary>Stops a search from or to a cell that no body stands on, and names which of the two it is (T-2).</summary>
    private void CheckFloor(Cell cell, string role)
    {
        if (GridMoves.IsFloor(this.grid, cell))
        {
            return;
        }

        ContextException error = new($"The path {role} {cell} is not a floor cell with two open cells over it, and no body stands there (D-165).");
        error.AddContext("role", role);
        error.AddContext("cell", cell.ToString());
        error.AddContext("sizeX", ((long)this.grid.SizeX).ToString(CultureInfo.InvariantCulture));
        error.AddContext("sizeY", ((long)this.grid.SizeY).ToString(CultureInfo.InvariantCulture));
        error.AddContext("sizeZ", ((long)this.grid.SizeZ).ToString(CultureInfo.InvariantCulture));
        throw error;
    }

    /// <summary>
    /// The open set of the search: a binary heap of cell indexes, lowest estimate first. Two entries of one estimate
    /// pop in the order that they entered, so the search reads no hash order and no address (G-9).
    /// </summary>
    private sealed class OpenSet
    {
        private int[] cells;
        private int[] estimates;
        private int[] arrivals;
        private int count;
        private int arrival;

        /// <summary>An empty set that grows from a first size.</summary>
        internal OpenSet(int firstSize)
        {
            int size = firstSize < 1 ? 1 : firstSize;
            this.cells = new int[size];
            this.estimates = new int[size];
            this.arrivals = new int[size];
        }

        /// <summary>Empties the set for the next search, and keeps the arrays.</summary>
        internal void Clear()
        {
            this.count = 0;
            this.arrival = 0;
        }

        /// <summary>Puts one cell in the set under an estimate of the whole path through it.</summary>
        internal void Push(int cell, int estimate)
        {
            if (this.count == this.cells.Length)
            {
                this.Grow();
            }

            int position = this.count;
            this.cells[position] = cell;
            this.estimates[position] = estimate;
            this.arrivals[position] = this.arrival;
            this.count++;
            this.arrival++;

            while (position > 0)
            {
                int above = (position - 1) / 2;
                if (!this.IsBefore(position, above))
                {
                    break;
                }

                this.Swap(position, above);
                position = above;
            }
        }

        /// <summary>Takes the cell of the lowest estimate out of the set. Gives false when the set is empty.</summary>
        internal bool TryPop(out int cell)
        {
            if (this.count == 0)
            {
                cell = 0;
                return false;
            }

            cell = this.cells[0];
            this.count--;
            this.cells[0] = this.cells[this.count];
            this.estimates[0] = this.estimates[this.count];
            this.arrivals[0] = this.arrivals[this.count];

            int position = 0;
            while (true)
            {
                int left = (2 * position) + 1;
                int right = left + 1;
                int first = position;
                if (left < this.count && this.IsBefore(left, first))
                {
                    first = left;
                }

                if (right < this.count && this.IsBefore(right, first))
                {
                    first = right;
                }

                if (first == position)
                {
                    return true;
                }

                this.Swap(position, first);
                position = first;
            }
        }

        /// <summary>Answers whether the entry at one place pops before the entry at another: the lower estimate, then the earlier arrival.</summary>
        private bool IsBefore(int place, int other)
        {
            if (this.estimates[place] != this.estimates[other])
            {
                return this.estimates[place] < this.estimates[other];
            }

            return this.arrivals[place] < this.arrivals[other];
        }

        private void Swap(int place, int other)
        {
            (this.cells[place], this.cells[other]) = (this.cells[other], this.cells[place]);
            (this.estimates[place], this.estimates[other]) = (this.estimates[other], this.estimates[place]);
            (this.arrivals[place], this.arrivals[other]) = (this.arrivals[other], this.arrivals[place]);
        }

        /// <summary>Doubles the three arrays. A cell enters the set once for each better cost, so the count can pass the cell count.</summary>
        private void Grow()
        {
            int size = this.cells.Length * 2;
            int[] grownCells = new int[size];
            int[] grownEstimates = new int[size];
            int[] grownArrivals = new int[size];
            for (int index = 0; index < this.count; index++)
            {
                grownCells[index] = this.cells[index];
                grownEstimates[index] = this.estimates[index];
                grownArrivals[index] = this.arrivals[index];
            }

            this.cells = grownCells;
            this.estimates = grownEstimates;
            this.arrivals = grownArrivals;
        }
    }
}
