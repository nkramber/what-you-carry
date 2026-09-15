using System;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// A test course along one rise (D-345, D-346): a low floor, one ramp across the whole width, and a high floor one
/// block up. The course has its own frame: the distance along the rise, the height, and the distance across. The
/// frame maps to the grid by the rise, so one test runs the same course along all four directions.
/// </summary>
internal sealed class RampCourse
{
    /// <summary>The size of the course along the rise, in blocks.</summary>
    public const int Length = 16;

    /// <summary>The size of the course across the rise, in blocks.</summary>
    public const int Width = 9;

    /// <summary>The height of the grid, in blocks.</summary>
    public const int Height = 8;

    /// <summary>The first cell of the ramp along the rise. The low floor lies before it.</summary>
    public const int RampStart = 6;

    /// <summary>The top of the low floor, in meters.</summary>
    public const float LowTop = 1.0f;

    /// <summary>The top of the high floor, in meters.</summary>
    public const float HighTop = 2.0f;

    /// <summary>The distance across of the middle of the course, in meters.</summary>
    public const float Middle = 4.5f;

    /// <summary>A course with a ramp of one run along one rise. Row 0 is stone, and the ramp and the high floor stand in row 1.</summary>
    public RampCourse(RampRise rise, int run)
    {
        this.Rise = rise;
        this.Run = run;
        this.Grid = rise == RampRise.PlusX || rise == RampRise.MinusX ? new VoxelGrid(Length, Height, Width) : new VoxelGrid(Width, Height, Length);
        for (int along = 0; along < Length; along++)
        {
            for (int across = 0; across < Width; across++)
            {
                Cell ground = this.CellAt(along, 0, across);
                this.Grid.Set(ground.X, ground.Y, ground.Z, BlockId.RawStone);

                Cell upper = this.CellAt(along, 1, across);
                if (along >= RampStart && along < RampStart + run)
                {
                    this.Grid.Set(upper.X, upper.Y, upper.Z, new Ramp(rise, run, along - RampStart).Id);
                }
                else if (along >= RampStart + run)
                {
                    this.Grid.Set(upper.X, upper.Y, upper.Z, BlockId.RawStone);
                }
            }
        }
    }

    /// <summary>The rise of the ramp.</summary>
    public RampRise Rise { get; }

    /// <summary>The run of the ramp.</summary>
    public int Run { get; }

    /// <summary>The grid of the course.</summary>
    public VoxelGrid Grid { get; }

    /// <summary>The distance along the rise where the high floor starts.</summary>
    public int HighStart => RampStart + this.Run;

    /// <summary>The unit horizontal vector toward the rise.</summary>
    public Vector3 Uphill => this.Point(1.0f, 0.0f, 0.0f) - this.Point(0.0f, 0.0f, 0.0f);

    /// <summary>The unit horizontal vector across the rise.</summary>
    public Vector3 Across => this.Point(0.0f, 0.0f, 1.0f) - this.Point(0.0f, 0.0f, 0.0f);

    /// <summary>Every rise with every run of D-346, for a theory.</summary>
    public static TheoryData<RampRise, int> EveryRiseAndRun()
    {
        TheoryData<RampRise, int> data = new();
        foreach (RampRise rise in Enum.GetValues<RampRise>())
        {
            for (int run = Ramp.SteepestRun; run <= Ramp.ShallowestRun; run++)
            {
                data.Add(rise, run);
            }
        }

        return data;
    }

    /// <summary>
    /// Answers whether a point lies in a solid part of the grid: in a block, under the slope of a ramp, or outside
    /// the grid (D-237, D-367). The tests compare the marches and the sweeps with it.
    /// </summary>
    public static bool IsSolidAt(VoxelGrid grid, Vector3 point)
    {
        int x = (int)MathF.Floor(point.X);
        int y = (int)MathF.Floor(point.Y);
        int z = (int)MathF.Floor(point.Z);
        if (grid.TryGetRamp(x, y, z, out Ramp ramp))
        {
            return ramp.HeightOver(x, y, z, point.X, point.Y, point.Z) < 0.0f;
        }

        return grid.IsSolid(x, y, z);
    }

    /// <summary>The grid point of a point of the course frame.</summary>
    public Vector3 Point(float along, float height, float across)
    {
        switch (this.Rise)
        {
            case RampRise.PlusX:
                return new Vector3(along, height, across);
            case RampRise.MinusX:
                return new Vector3(Length - along, height, across);
            case RampRise.PlusZ:
                return new Vector3(across, height, along);
            default:
                return new Vector3(across, height, Length - along);
        }
    }

    /// <summary>The distance along the rise of a grid point.</summary>
    public float AlongOf(Vector3 point)
    {
        switch (this.Rise)
        {
            case RampRise.PlusX:
                return point.X;
            case RampRise.MinusX:
                return Length - point.X;
            case RampRise.PlusZ:
                return point.Z;
            default:
                return Length - point.Z;
        }
    }

    /// <summary>The grid cell of a cell of the course frame.</summary>
    public Cell CellAt(int along, int row, int across)
    {
        switch (this.Rise)
        {
            case RampRise.PlusX:
                return new Cell(along, row, across);
            case RampRise.MinusX:
                return new Cell(Length - 1 - along, row, across);
            case RampRise.PlusZ:
                return new Cell(across, row, along);
            default:
                return new Cell(across, row, Length - 1 - along);
        }
    }
}
