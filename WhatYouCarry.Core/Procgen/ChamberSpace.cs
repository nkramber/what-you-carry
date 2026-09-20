using System.Collections.Generic;

namespace WhatYouCarry.Core.Procgen;

/// <summary>One box of the union that builds a chamber footprint (D-253), by its lowest and highest column.</summary>
/// <param name="MinX">The lowest X of the box.</param>
/// <param name="MaxX">The highest X of the box.</param>
/// <param name="MinZ">The lowest Z of the box.</param>
/// <param name="MaxZ">The highest Z of the box.</param>
public readonly record struct ChamberBox(int MinX, int MaxX, int MinZ, int MaxZ)
{
    /// <summary>Answers whether the box holds the column.</summary>
    public bool Holds(Column column)
    {
        return column.X >= this.MinX && column.X <= this.MaxX && column.Z >= this.MinZ && column.Z <= this.MaxZ;
    }
}

/// <summary>
/// The room that one carved chamber gives a tier (D-348, D-388): its footprint inside a window of columns, the
/// boxes that built it, its floor row, its air height, and its anchor. The tier plan reads it, and it changes
/// nothing on the grid.
/// </summary>
/// <remarks>
/// <para>
/// The window is the bounding box of the footprint with one column of margin on each side, so a neighbor of an
/// edge column lies inside it. A mask is one truth per column of the window, in scan order: X inner, Z outer.
/// </para>
/// <para>
/// Some columns of the chamber floor must stay open: the anchor, and every column that a tunnel joins the chamber
/// through. A tier over one of them would wall the chamber in, so the tier plan takes none of them (D-348). The
/// dig plan reads the grid and marks them with <see cref="Block"/>.
/// </para>
/// </remarks>
public sealed class ChamberSpace
{
    private readonly bool[] footprint;
    private bool[] blocked;
    private bool[] sloped;

    /// <summary>The space of one carved chamber.</summary>
    public ChamberSpace(int floorRow, int height, Column anchor, IReadOnlyList<Column> footprint, IReadOnlyList<ChamberBox> boxes)
    {
        this.FloorRow = floorRow;
        this.Height = height;
        this.Anchor = anchor;
        this.Footprint = footprint;
        this.Boxes = boxes;

        int minX = footprint[0].X;
        int maxX = footprint[0].X;
        int minZ = footprint[0].Z;
        int maxZ = footprint[0].Z;
        foreach (Column column in footprint)
        {
            minX = column.X < minX ? column.X : minX;
            maxX = column.X > maxX ? column.X : maxX;
            minZ = column.Z < minZ ? column.Z : minZ;
            maxZ = column.Z > maxZ ? column.Z : maxZ;
        }

        this.MinX = minX - 1;
        this.MaxX = maxX + 1;
        this.MinZ = minZ - 1;
        this.MaxZ = maxZ + 1;
        this.Width = this.MaxX - this.MinX + 1;
        this.Depth = this.MaxZ - this.MinZ + 1;
        this.footprint = new bool[this.Width * this.Depth];
        foreach (Column column in footprint)
        {
            this.footprint[this.Index(column)] = true;
        }

        this.blocked = this.EmptyMask();
        this.Set(this.blocked, anchor);
        this.sloped = this.EmptyMask();
    }

    /// <summary>The floor row of the chamber.</summary>
    public int FloorRow { get; }

    /// <summary>The air rows of the chamber over its floor row.</summary>
    public int Height { get; }

    /// <summary>The anchor of the chamber. The spawn of the floor stands on the anchor of the first chamber (D-256).</summary>
    public Column Anchor { get; }

    /// <summary>Every column of the chamber, in scan order.</summary>
    public IReadOnlyList<Column> Footprint { get; }

    /// <summary>The boxes that built the footprint, in the order the union drew them (D-253).</summary>
    public IReadOnlyList<ChamberBox> Boxes { get; }

    /// <summary>The lowest X of the window: one column outside the footprint.</summary>
    public int MinX { get; }

    /// <summary>The highest X of the window: one column outside the footprint.</summary>
    public int MaxX { get; }

    /// <summary>The lowest Z of the window: one column outside the footprint.</summary>
    public int MinZ { get; }

    /// <summary>The highest Z of the window: one column outside the footprint.</summary>
    public int MaxZ { get; }

    /// <summary>The columns of the window along X.</summary>
    public int Width { get; }

    /// <summary>The columns of the window along Z.</summary>
    public int Depth { get; }

    /// <summary>A mask of the window with no column marked.</summary>
    public bool[] EmptyMask()
    {
        return new bool[this.Width * this.Depth];
    }

    /// <summary>A mask of the window that holds every column of a list.</summary>
    public bool[] Mark(IReadOnlyList<Column> columns)
    {
        bool[] mask = this.EmptyMask();
        foreach (Column column in columns)
        {
            if (this.Inside(column))
            {
                mask[this.Index(column)] = true;
            }
        }

        return mask;
    }

    /// <summary>Answers whether a mask holds the column. A column outside the window is not in any mask.</summary>
    public bool Holds(bool[] mask, Column column)
    {
        return this.Inside(column) && mask[this.Index(column)];
    }

    /// <summary>Marks one column of a mask. A column outside the window changes nothing.</summary>
    public void Set(bool[] mask, Column column)
    {
        if (this.Inside(column))
        {
            mask[this.Index(column)] = true;
        }
    }

    /// <summary>Clears one column of a mask. A column outside the window changes nothing.</summary>
    public void Clear(bool[] mask, Column column)
    {
        if (this.Inside(column))
        {
            mask[this.Index(column)] = false;
        }
    }

    /// <summary>Answers whether the chamber holds the column.</summary>
    public bool InFootprint(Column column)
    {
        return this.Holds(this.footprint, column);
    }

    /// <summary>Answers whether the column is the anchor of the chamber.</summary>
    public bool IsAnchor(Column column)
    {
        return column == this.Anchor;
    }

    /// <summary>Marks the columns of the chamber floor that must stay open, beside the anchor, which is always one.</summary>
    public void Block(IReadOnlyList<Column> columns)
    {
        bool[] marked = this.Mark(columns);
        this.Set(marked, this.Anchor);
        this.blocked = marked;
    }

    /// <summary>Marks the columns whose chamber floor cell holds a ramp of a tunnel (D-345).</summary>
    public void Slope(IReadOnlyList<Column> columns)
    {
        this.sloped = this.Mark(columns);
    }

    /// <summary>
    /// Answers whether the chamber floor of the column is the slope of a ramp. A tier stands on no slope, and the
    /// low end of the ramp of a tier meets a flat floor, so neither takes such a column (D-345, D-348).
    /// </summary>
    public bool IsSloped(Column column)
    {
        return this.Holds(this.sloped, column);
    }

    /// <summary>Answers whether the column must stay open chamber floor: the anchor, or a column that a tunnel joins the chamber through.</summary>
    public bool MustStayOpen(Column column)
    {
        return this.Holds(this.blocked, column);
    }

    /// <summary>Answers whether a tier can take the column: the chamber holds it, and it does not have to stay open.</summary>
    public bool CanTakeATier(Column column)
    {
        return this.InFootprint(column) && !this.MustStayOpen(column) && !this.IsSloped(column);
    }

    /// <summary>Every column of a mask, in scan order: X inner, Z outer.</summary>
    public IReadOnlyList<Column> Columns(bool[] mask)
    {
        List<Column> columns = [];
        for (int z = this.MinZ; z <= this.MaxZ; z++)
        {
            for (int x = this.MinX; x <= this.MaxX; x++)
            {
                Column column = new(x, z);
                if (mask[this.Index(column)])
                {
                    columns.Add(column);
                }
            }
        }

        return columns;
    }

    /// <summary>Answers whether the column lies inside the window.</summary>
    private bool Inside(Column column)
    {
        return column.X >= this.MinX && column.X <= this.MaxX && column.Z >= this.MinZ && column.Z <= this.MaxZ;
    }

    /// <summary>The index of one column of the window: X inner, Z outer.</summary>
    private int Index(Column column)
    {
        return (column.X - this.MinX) + (this.Width * (column.Z - this.MinZ));
    }
}
