using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// The grid of one floor while the dig plan carves it (D-253). It starts as solid rock with the floor generator's
/// shell around it, and every carve goes through the two rules that keep the dug network walkable.
/// </summary>
/// <remarks>
/// <para>
/// A dig unit is a set of columns, each with a floor row and a height: the air goes in the rows above the floor
/// row. The first rule: the floor row of every column is rock, so the new air has a floor. The second rule: no
/// carved cell is rock with air above it, because such a cell is the floor of air that an earlier unit dug, and
/// a body that stood there would fall. A unit that breaks a rule is refused whole, and nothing of it is carved.
/// </para>
/// <para>
/// Under the two rules, air only grows and no floor ever goes, so every cell that a unit made walkable stays
/// walkable, and every unit attaches to the network at the cell that the walker stands on. That is why every
/// chamber is reachable by construction, and the reachability search of PR-9 exit test 1 confirms it.
/// </para>
/// <para>
/// A shaft is the one carve that removes a floor on purpose: a three by three hole in a chamber floor, ringed by
/// chamber floor on every side, over air that an earlier unit dug. <see cref="CarveShaft"/> takes it apart from
/// the rules, and the dig plan proves the ring before it calls.
/// </para>
/// </remarks>
public sealed class DigCanvas
{
    /// <summary>The count of rock rows and columns that stay around the floor on every side.</summary>
    public const int Shell = 1;

    /// <summary>The lowest row that a floor cell can take. Row zero is the base of the shell.</summary>
    public const int LowestFloorRow = 1;

    /// <summary>A canvas over a grid, with every cell set to rock.</summary>
    public DigCanvas(VoxelGrid grid)
    {
        this.Grid = grid;
        for (int y = 0; y < grid.SizeY; y++)
        {
            for (int z = 0; z < grid.SizeZ; z++)
            {
                for (int x = 0; x < grid.SizeX; x++)
                {
                    grid.Set(x, y, z, BlockId.RawStone);
                }
            }
        }
    }

    /// <summary>The grid under the canvas.</summary>
    public VoxelGrid Grid { get; }

    /// <summary>The highest row that a floor cell can take under air of the given height, so the top row of the shell stays rock.</summary>
    public int HighestFloorRow(int height)
    {
        return this.Grid.SizeY - 1 - Shell - height;
    }

    /// <summary>Answers whether the column is inside the shell on X and on Z.</summary>
    public bool IsInsideShell(Column column)
    {
        return column.X >= Shell && column.X <= this.Grid.SizeX - 1 - Shell && column.Z >= Shell && column.Z <= this.Grid.SizeZ - 1 - Shell;
    }

    /// <summary>Answers whether the cell is air. A cell outside the grid is rock.</summary>
    public bool IsAir(int x, int y, int z)
    {
        return !this.Grid.IsSolid(x, y, z);
    }

    /// <summary>Answers whether every column of the unit passes the two carve rules and lies inside the shell.</summary>
    public bool CanCarve(IReadOnlyList<DigColumn> unit)
    {
        foreach (DigColumn column in unit)
        {
            if (!this.IsInsideShell(new Column(column.X, column.Z)))
            {
                return false;
            }

            if (column.Floor < LowestFloorRow || column.Floor > this.HighestFloorRow(column.Height))
            {
                return false;
            }

            // The first rule: the new air needs a floor of rock.
            if (this.IsAir(column.X, column.Floor, column.Z))
            {
                return false;
            }

            // The second rule: a rock cell with air above it is the floor of an earlier unit, and it stays.
            for (int row = column.Floor + 1; row <= column.Floor + column.Height; row++)
            {
                if (!this.IsAir(column.X, row, column.Z) && this.IsAir(column.X, row + 1, column.Z))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>Carves the air of every column of a unit that <see cref="CanCarve"/> accepted.</summary>
    /// <exception cref="ContextException">The unit fails a carve rule. The caller checks first, so this is a defect of the caller (T-2).</exception>
    public void Carve(IReadOnlyList<DigColumn> unit)
    {
        if (!this.CanCarve(unit))
        {
            ContextException error = new($"The dig unit of {unit.Count} columns fails a carve rule, and the caller did not check it first.");
            error.AddContext("columns", ((long)unit.Count).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        foreach (DigColumn column in unit)
        {
            for (int row = column.Floor + 1; row <= column.Floor + column.Height; row++)
            {
                this.Grid.Set(column.X, row, column.Z, BlockId.Air);
            }
        }
    }

    /// <summary>
    /// Carves one column of a shaft: every row from <paramref name="fromRow"/> up to <paramref name="toRow"/>
    /// becomes air, the top row included, which is the chamber floor cell that the shaft removes.
    /// </summary>
    public void CarveShaft(Column column, int fromRow, int toRow)
    {
        for (int row = fromRow; row <= toRow; row++)
        {
            this.Grid.Set(column.X, row, column.Z, BlockId.Air);
        }
    }

    /// <summary>
    /// The top air row of the dug space below a floor cell, or minus one when only rock lies below it. The
    /// scan starts one row under the floor cell and stops at the first air cell.
    /// </summary>
    public int TopAirRowBelow(Column column, int floorRow)
    {
        for (int row = floorRow - 1; row >= 0; row--)
        {
            if (this.IsAir(column.X, row, column.Z))
            {
                return row;
            }
        }

        return -1;
    }

    /// <summary>The floor row under an air cell: the first rock row below it. The base row of the shell is rock, so the scan always ends.</summary>
    public int FloorRowBelow(Column column, int airRow)
    {
        int row = airRow;
        while (this.IsAir(column.X, row, column.Z))
        {
            row--;
        }

        return row;
    }
}

/// <summary>One column of a dig unit: the air goes in the <paramref name="Height"/> rows above <paramref name="Floor"/>.</summary>
public readonly record struct DigColumn(int X, int Z, int Floor, int Height);
