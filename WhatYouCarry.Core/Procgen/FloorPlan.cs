using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// One dug floor (D-253, D-256): the grid, the spawn point, the stairwell cell, the chambers, and the shafts.
/// </summary>
/// <param name="Floor">The floor number, from one (D-3).</param>
/// <param name="Template">The floor template of the band that holds the floor (D-252).</param>
/// <param name="Grid">The grid, in the blocks of D-259.</param>
/// <param name="Spawn">The feet center of the player at tick zero: the center of the anchor cell of the first chamber (D-256).</param>
/// <param name="Stairwell">The floor cell of the stairwell: the cell with the longest walkable path from the spawn, in the chamber that lies farthest (D-256).</param>
/// <param name="Chambers">The chambers, in dig order. The first one holds the spawn.</param>
/// <param name="Shafts">The shafts, in dig order.</param>
/// <param name="Detail">The pools, the pillars, and the collapses of the detail pass (D-254).</param>
public sealed record FloorPlan(int Floor, FloorTemplate Template, VoxelGrid Grid, Vector3 Spawn, Cell Stairwell, IReadOnlyList<Chamber> Chambers, IReadOnlyList<Shaft> Shafts, DetailResult Detail);

/// <summary>
/// One chamber of a floor (D-253, D-255). Its air fills the rows above <paramref name="FloorRow"/> over every
/// column of the footprint, <paramref name="Height"/> rows high.
/// </summary>
/// <param name="Index">The position in the dig order, from zero.</param>
/// <param name="Kind">The kind that gave the chamber its weight and its shape.</param>
/// <param name="FloorRow">The row of the floor cells. The air starts one row above it.</param>
/// <param name="Height">The count of air rows over the floor.</param>
/// <param name="Anchor">The column that the first box was centered on. The spawn stands on it in the first chamber.</param>
/// <param name="Footprint">Every column of the chamber, in scan order.</param>
public sealed record Chamber(int Index, ChamberKind Kind, int FloorRow, int Height, Column Anchor, IReadOnlyList<Column> Footprint)
{
    /// <summary>The air cells of the chamber: every footprint column over every air row.</summary>
    public IReadOnlyList<Cell> AirCells()
    {
        List<Cell> cells = [];
        foreach (Column column in this.Footprint)
        {
            for (int row = this.FloorRow + 1; row <= this.FloorRow + this.Height; row++)
            {
                cells.Add(new Cell(column.X, row, column.Z));
            }
        }

        return cells;
    }
}

/// <summary>
/// One shaft (D-253): a three by three hole in a chamber floor, down to the top air row of the dug space below
/// it. A body drops through it and lands on the floor of that space, and no path leads back up it.
/// </summary>
/// <param name="ChamberIndex">The chamber whose floor holds the hole.</param>
/// <param name="Center">The center column of the hole.</param>
/// <param name="TopRow">The floor row of the chamber, which is the highest row the shaft removed.</param>
/// <param name="LandingAirRow">The top air row of the space below, which the shaft joins.</param>
public sealed record Shaft(int ChamberIndex, Column Center, int TopRow, int LandingAirRow);
