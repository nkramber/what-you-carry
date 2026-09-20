using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// One dug floor (D-253, D-256): the grid, the spawn point, the stairwell cell, the chambers, the tunnels, and the shafts.
/// </summary>
/// <param name="Floor">The floor number, from one (D-3).</param>
/// <param name="Template">The floor template of the band that holds the floor (D-252).</param>
/// <param name="Grid">The grid, in the blocks of D-259.</param>
/// <param name="Spawn">The feet center of the player at tick zero: the center of the anchor cell of the first chamber (D-256).</param>
/// <param name="Stairwell">The floor cell of the stairwell: the cell with the longest walkable path from the spawn, in the chamber that lies farthest (D-256).</param>
/// <param name="Chambers">The chambers, in dig order. The first one holds the spawn.</param>
/// <param name="Tunnels">The stamps of the gallery and the drifts, in dig order (D-341, D-342).</param>
/// <param name="Shafts">The shafts, in dig order.</param>
/// <param name="Ramps">The ramps, in dig order: the tunnel ramps and then the tier ramps (D-345, D-348).</param>
/// <param name="Detail">The pools, the pillars, and the collapses of the detail pass (D-254).</param>
public sealed record FloorPlan(int Floor, FloorTemplate Template, VoxelGrid Grid, Vector3 Spawn, Cell Stairwell, IReadOnlyList<Chamber> Chambers, IReadOnlyList<TunnelStamp> Tunnels, IReadOnlyList<Shaft> Shafts, IReadOnlyList<DugRamp> Ramps, DetailResult Detail);

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
/// <param name="Tier">The tier of the chamber, or null when the chamber holds none (D-348, D-350, D-391).</param>
/// <param name="TierDrawn">True when the draw of the tier chance hit. A chamber with a drawn tier that fits no shape holds no tier (D-350, D-391).</param>
public sealed record Chamber(int Index, ChamberKind Kind, int FloorRow, int Height, Column Anchor, IReadOnlyList<Column> Footprint, ChamberTier? Tier, bool TierDrawn)
{
    /// <summary>The air cells of the chamber: every footprint column over its lowest air row, up to the top of the chamber.</summary>
    public IReadOnlyList<Cell> AirCells()
    {
        List<Cell> cells = [];
        foreach (Column column in this.Footprint)
        {
            for (int row = this.LowestAirRow(column); row <= this.FloorRow + this.Height; row++)
            {
                cells.Add(new Cell(column.X, row, column.Z));
            }
        }

        return cells;
    }

    /// <summary>
    /// The lowest air row of one column of the chamber: the row over the chamber floor, or the row over the tier
    /// floor or the ramp cell that fills the column (D-348, D-349).
    /// </summary>
    public int LowestAirRow(Column column)
    {
        if (this.Tier is null)
        {
            return this.FloorRow + 1;
        }

        foreach (Column tierColumn in this.Tier.Floor)
        {
            if (tierColumn == column)
            {
                return this.Tier.FloorRow + 1;
            }
        }

        foreach (Cell rampCell in this.Tier.Ramp.Cells)
        {
            if (rampCell.X == column.X && rampCell.Z == column.Z)
            {
                return rampCell.Y + 1;
            }
        }

        return this.FloorRow + 1;
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

/// <summary>
/// One stamp of a tunnel (D-253, D-341): the cell that a walker stood on after a step or a ramp landing. The air of the
/// stamp fills the square of the tunnel width around that cell, over the tunnel height above it (D-342).
/// </summary>
/// <param name="Center">The floor cell under the walker.</param>
/// <param name="Gallery">True for a stamp of the gallery walker, and false for a stamp of a drift.</param>
public readonly record struct TunnelStamp(Cell Center, bool Gallery);

/// <summary>
/// One ramp of a dug floor (D-345, D-346): the cells of its slope, and the flat floor cells at its two ends. The
/// floor rises one block over <paramref name="Run"/> cells, and the whole ramp rises <paramref name="Rise"/> blocks.
/// </summary>
/// <param name="Run">The cells over which the floor rises one block: 2, 3, or 4, from the list of the template (D-346, D-390).</param>
/// <param name="Rise">The blocks from the low end to the high end.</param>
/// <param name="Cells">Every cell that holds a ramp block, in the order the dig carved them.</param>
/// <param name="LowEnd">The floor cell at the low end, whose top the low face of the slope meets.</param>
/// <param name="HighEnd">The floor cell at the high end, whose top the high face of the slope meets.</param>
public sealed record DugRamp(int Run, int Rise, IReadOnlyList<Cell> Cells, Cell LowEnd, Cell HighEnd);

/// <summary>
/// One tier of a chamber (D-348, D-349): a raised floor 2 blocks over the chamber floor, inside the footprint of
/// the chamber, with one ramp up to it. A body reaches a tier by its ramp alone, because a jump clears one block
/// (D-165).
/// </summary>
/// <param name="Shape">The shape that the generator drew for the tier (D-388).</param>
/// <param name="FloorRow">The row of the floor cells of the tier: the chamber floor row and <see cref="Rise"/>.</param>
/// <param name="Floor">Every column of the tier, in scan order.</param>
/// <param name="Ramp">The ramp that joins the chamber floor to the tier (D-390).</param>
public sealed record ChamberTier(TierShape Shape, int FloorRow, IReadOnlyList<Column> Floor, DugRamp Ramp)
{
    /// <summary>The blocks that a tier stands over its chamber floor (D-349).</summary>
    public const int Rise = 2;
}

/// <summary>The four shapes that a tier can take inside the footprint of its chamber (D-388).</summary>
public enum TierShape
{
    /// <summary>The largest rectangle inside the footprint against one side of it.</summary>
    Rectangle = 0,

    /// <summary>Every column of the footprint past a line across it.</summary>
    CutLine = 1,

    /// <summary>The part of the footprint that one box of its union holds.</summary>
    RaisedBox = 2,

    /// <summary>A rectangle inside the footprint that touches no wall, with chamber floor around it.</summary>
    Island = 3,
}
