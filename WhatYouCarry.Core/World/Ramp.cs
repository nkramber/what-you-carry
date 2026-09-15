using System.Globalization;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.World;

/// <summary>The direction along which the floor of a ramp rises (D-345, D-367). The value is part of the block id.</summary>
public enum RampRise : byte
{
    /// <summary>The floor rises toward plus X.</summary>
    PlusX = 0,

    /// <summary>The floor rises toward minus X.</summary>
    MinusX = 1,

    /// <summary>The floor rises toward plus Z.</summary>
    PlusZ = 2,

    /// <summary>The floor rises toward minus Z.</summary>
    MinusZ = 3,
}

/// <summary>
/// One ramp cell (D-345, D-346, D-367): a sloped floor that rises one block over <see cref="Run"/> cells along
/// <see cref="Rise"/>. The cell at <see cref="Place"/> holds the part of the rise from Place / Run to
/// (Place + 1) / Run, so place 0 is the low end.
/// </summary>
/// <remarks>
/// <para>
/// The part of the cell under the slope is solid, and the part over it is air. The distance along the rise runs
/// from 0 at the low face of the cell to 1 at its high face, so the slope over a cell in row y is
/// y + (Place + along) / Run. A ramp of the last place meets the top of a block in its row, and a ramp of place 0
/// meets the top of a block in the row below.
/// </para>
/// <para>
/// The block id of a ramp is the first id of its run, plus the rise times the run, plus the place (D-367). The
/// runs of 2, 3, and 4 take ids 8 to 15, 16 to 27, and 28 to 43.
/// </para>
/// </remarks>
public readonly record struct Ramp
{
    /// <summary>The first block id of a ramp (D-367).</summary>
    public const int FirstId = 8;

    /// <summary>The last block id of a ramp (D-367).</summary>
    public const int LastId = 43;

    /// <summary>The run of the steepest slope, 1:2 (D-346). A slope rises at most one block over this many.</summary>
    public const int SteepestRun = 2;

    /// <summary>The run of the shallowest slope, 1:4 (D-346).</summary>
    public const int ShallowestRun = 4;

    /// <summary>A ramp cell of a rise, a run, and a place.</summary>
    /// <exception cref="ContextException">The rise is not one of the four, the run is not 2, 3, or 4, or the place is outside the run.</exception>
    public Ramp(RampRise rise, int run, int place)
    {
        if (rise > RampRise.MinusZ || run < SteepestRun || run > ShallowestRun || place < 0 || place >= run)
        {
            ContextException error = new($"A ramp rises along one of four directions over a run of 2, 3, or 4 cells, with a place inside the run (D-346, D-367). The rise is {(int)rise}, the run is {run}, and the place is {place}.");
            error.AddContext("rise", ((long)rise).ToString(CultureInfo.InvariantCulture));
            error.AddContext("run", ((long)run).ToString(CultureInfo.InvariantCulture));
            error.AddContext("place", ((long)place).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        this.Rise = rise;
        this.Run = run;
        this.Place = place;
    }

    /// <summary>The direction along which the floor rises.</summary>
    public RampRise Rise { get; }

    /// <summary>The count of cells over which the floor rises one block: 2, 3, or 4.</summary>
    public int Run { get; }

    /// <summary>The place of the cell along the run, from 0 at the low end.</summary>
    public int Place { get; }

    /// <summary>The block id of the cell (D-367).</summary>
    public BlockId Id => (BlockId)(FirstIdOfRun(this.Run) + ((int)this.Rise * this.Run) + this.Place);

    /// <summary>Answers whether the floor rises along X. Otherwise it rises along Z.</summary>
    public bool RisesAlongX => this.Rise == RampRise.PlusX || this.Rise == RampRise.MinusX;

    /// <summary>Answers whether a block id is a ramp cell (D-367).</summary>
    public static bool IsRamp(BlockId block)
    {
        return (int)block >= FirstId && (int)block <= LastId;
    }

    /// <summary>The ramp cell of a block id (D-367).</summary>
    /// <exception cref="ContextException">The id is not a ramp.</exception>
    public static Ramp FromId(BlockId block)
    {
        if (!IsRamp(block))
        {
            ContextException error = new($"The block id {(int)block} is not a ramp. The ramp ids are {FirstId} to {LastId} (D-367).");
            error.AddContext("block", ((long)block).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        int offset = (int)block - FirstId;
        int run;
        int firstOffset;
        if (offset < FirstIdOfRun(3) - FirstId)
        {
            run = 2;
            firstOffset = 0;
        }
        else if (offset < FirstIdOfRun(4) - FirstId)
        {
            run = 3;
            firstOffset = FirstIdOfRun(3) - FirstId;
        }
        else
        {
            run = 4;
            firstOffset = FirstIdOfRun(4) - FirstId;
        }

        int inRun = offset - firstOffset;
        return new Ramp((RampRise)(inRun / run), run, inRun % run);
    }

    /// <summary>The height of the slope over a cell in <paramref name="row"/>, at a distance along the rise from 0 at the low face to 1 at the high face.</summary>
    public float SlopeAt(int row, float along)
    {
        return row + ((this.Place + along) / this.Run);
    }

    /// <summary>
    /// The distance along the rise at which the slope over a cell in <paramref name="row"/> reaches a height. A height
    /// at or under the low end gives zero or less, and a height at or over the high end gives one or more.
    /// </summary>
    public float MeetAt(int row, float height)
    {
        return (this.Run * (height - row)) - this.Place;
    }

    /// <summary>The distance along the rise of a point over the cell: 0 at the low face and 1 at the high face. A point outside the cell gives a value outside [0, 1].</summary>
    public float Along(int cellX, int cellZ, float x, float z)
    {
        switch (this.Rise)
        {
            case RampRise.PlusX:
                return x - cellX;
            case RampRise.MinusX:
                return (cellX + 1) - x;
            case RampRise.PlusZ:
                return z - cellZ;
            default:
                return (cellZ + 1) - z;
        }
    }

    /// <summary>The height of a point over the slope of the cell. A point under the slope gives a value below zero.</summary>
    public float HeightOver(int cellX, int row, int cellZ, float x, float y, float z)
    {
        return y - this.SlopeAt(row, this.Along(cellX, cellZ, x, z));
    }

    /// <summary>
    /// The highest point of the slope under the part of a footprint that covers the cell. The slope rises along
    /// one axis, so the highest point lies at the edge of that part nearest the high face.
    /// </summary>
    public float HighestUnder(int cellX, int row, int cellZ, float minX, float maxX, float minZ, float maxZ)
    {
        float along;
        switch (this.Rise)
        {
            case RampRise.PlusX:
                along = (maxX < cellX + 1 ? maxX : cellX + 1) - cellX;
                break;
            case RampRise.MinusX:
                along = (cellX + 1) - (minX > cellX ? minX : cellX);
                break;
            case RampRise.PlusZ:
                along = (maxZ < cellZ + 1 ? maxZ : cellZ + 1) - cellZ;
                break;
            default:
                along = (cellZ + 1) - (minZ > cellZ ? minZ : cellZ);
                break;
        }

        return this.SlopeAt(row, DetMath.Clamp(along, 0.0f, 1.0f));
    }

    /// <summary>The first block id of the ramps of one run: 8 for 2, 16 for 3, and 28 for 4 (D-367).</summary>
    private static int FirstIdOfRun(int run)
    {
        if (run == 2)
        {
            return FirstId;
        }

        if (run == 3)
        {
            return FirstId + (4 * 2);
        }

        return FirstId + (4 * 2) + (4 * 3);
    }
}
