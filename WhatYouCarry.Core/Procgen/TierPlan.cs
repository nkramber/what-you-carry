using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// The tier of one chamber (D-348 to D-350, D-388 to D-391): a raised floor 2 blocks over the chamber floor, with
/// one ramp up to it. The plan builds the tier on the carved chamber, and the chamber keeps its air over the tier.
/// </summary>
/// <remarks>
/// <para>
/// The tier chance of the kind decides whether the chamber gets a tier (D-350). The Procgen stream then draws the
/// share of the footprint that the tier takes, from 25 to 40 percent (D-389). The plan reads the candidates of each
/// of the four shapes of D-388 at that share, and it takes the first of each shape that fits: a floor of at least
/// <see cref="SmallestTierCells"/> cells inside the footprint, with room beside it for a ramp of a slope of the
/// template. When no shape fits, the plan lowers the share toward the smallest tier and tries again, and it gives
/// no tier when even the smallest does not fit (D-391).
/// </para>
/// <para>
/// The stream then draws one of the shapes that fit, and one of the slopes that fit that shape (D-388, D-390). A
/// ramp of a run rises 2 blocks over twice that run of cells, so a run of 4 needs 8 cells of chamber floor behind
/// the tier. The ramp takes the widest cross-section that fits, from <see cref="WidestRamp"/> down to
/// <see cref="NarrowestRamp"/> cells (D-166, D-393).
/// </para>
/// <para>
/// The tier fills the two rows over the chamber floor of its own columns, so a body reaches it by its ramp alone
/// (D-165, D-349). The anchor of the chamber stays clear, because the spawn of the floor stands on the anchor of
/// the first chamber (D-256).
/// </para>
/// </remarks>
public static class TierPlan
{
    /// <summary>The shortest side of the smallest tier floor, in cells (D-391).</summary>
    public const int SmallestTierSideShort = 2;

    /// <summary>The longest side of the smallest tier floor, in cells (D-391).</summary>
    public const int SmallestTierSideLong = 3;

    /// <summary>The cells of the smallest tier floor (D-391).</summary>
    public const int SmallestTierCells = SmallestTierSideShort * SmallestTierSideLong;

    /// <summary>The smallest share of the chamber floor that a tier takes, in percent (D-389).</summary>
    public const int ShareMin = 25;

    /// <summary>The largest share of the chamber floor that a tier takes, in percent (D-389).</summary>
    public const int ShareMax = 40;

    /// <summary>The widest tier ramp, in cells: the cross-section of D-166.</summary>
    public const int WidestRamp = 3;

    /// <summary>
    /// The narrowest tier ramp, in cells (D-393). The generator takes the widest that fits, and a chamber with no
    /// room for the widest takes a narrower one before it gives up its tier.
    /// </summary>
    public const int NarrowestRamp = 2;

    /// <summary>The air rows that a chamber keeps over a tier, so a body stands on it (D-166, D-349).</summary>
    public const int AirOverTier = 3;

    /// <summary>The count of shares that the plan tries before it gives no tier: the drawn share, one between, and the smallest tier (D-391).</summary>
    public const int ShrinkSteps = 3;

    /// <summary>The count of shapes of D-388.</summary>
    private const int ShapeCount = 4;

    /// <summary>
    /// The tier of one carved chamber, or null when the draw gives none and when no shape fits (D-350, D-391). The
    /// caller writes the blocks with <see cref="Build"/>. The draw of the tier chance comes out too, so a test reads
    /// the rate of the draw apart from the rate of the fit (PR-66 exit test 4).
    /// </summary>
    public static ChamberTier? TryPlan(Rng rng, FloorTemplate template, ChamberKind kind, ChamberSpace space, out bool drawn)
    {
        drawn = rng.NextInt(100) < kind.TierChance;
        return drawn ? Plan(rng, template, space) : null;
    }

    /// <summary>
    /// The tier of one carved chamber with the draw of the tier chance passed over, or null when no shape fits
    /// (D-392). The floor pass calls it for the first chamber of a floor that built no tier from the draws.
    /// </summary>
    public static ChamberTier? Force(Rng rng, FloorTemplate template, ChamberSpace space)
    {
        return Plan(rng, template, space);
    }

    /// <summary>The tier of one carved chamber, with no draw of the tier chance.</summary>
    private static ChamberTier? Plan(Rng rng, FloorTemplate template, ChamberSpace space)
    {
        if (space.Height < ChamberTier.Rise + AirOverTier || space.Footprint.Count <= SmallestTierCells)
        {
            return null;
        }

        int share = ShareMin + rng.NextInt(ShareMax - ShareMin + 1);
        int target = space.Footprint.Count * share / 100;
        for (int step = 0; step < ShrinkSteps; step++)
        {
            int shrunk = Shrink(target, step);
            List<ChamberTier> fitting = [];
            List<IReadOnlyList<int>> runs = [];
            for (int shape = 0; shape < ShapeCount; shape++)
            {
                foreach (IReadOnlyList<Column> floor in Candidates(space, (TierShape)shape, shrunk))
                {
                    if (floor.Count < SmallestTierCells || CoversAnOpenColumn(space, floor) || !IsWhole(space, floor))
                    {
                        continue;
                    }

                    IReadOnlyList<int> fits = FittingRuns(template, space, floor);
                    if (fits.Count == 0)
                    {
                        continue;
                    }

                    fitting.Add(new ChamberTier((TierShape)shape, space.FloorRow + ChamberTier.Rise, floor, EmptyRamp));
                    runs.Add(fits);
                    break;
                }
            }

            if (fitting.Count == 0)
            {
                continue;
            }

            int pick = rng.NextInt(fitting.Count);
            IReadOnlyList<int> picked = runs[pick];
            int run = picked[rng.NextInt(picked.Count)];
            ChamberTier chosen = fitting[pick];
            DugRamp ramp = FirstRamp(space, chosen.Floor, run);
            return chosen with { Ramp = ramp };
        }

        return null;
    }

    /// <summary>
    /// Writes the blocks of a tier on the grid: the two rows over the chamber floor of every tier column, the fill
    /// under the upper half of the ramp, and the ramp cells. The chamber carved the air over all of them.
    /// </summary>
    public static void Build(DigCanvas canvas, ChamberTier tier, int chamberFloorRow)
    {
        foreach (Column column in tier.Floor)
        {
            for (int row = chamberFloorRow + 1; row <= tier.FloorRow; row++)
            {
                canvas.Grid.Set(column.X, row, column.Z, BlockId.RawStone);
            }
        }

        foreach (Cell cell in tier.Ramp.Cells)
        {
            for (int row = chamberFloorRow + 1; row < cell.Y; row++)
            {
                canvas.Grid.Set(cell.X, row, cell.Z, BlockId.RawStone);
            }
        }

        int run = tier.Ramp.Run;
        for (int index = 0; index < tier.Ramp.Cells.Count; index++)
        {
            Cell cell = tier.Ramp.Cells[index];
            int along = index / (tier.Ramp.Cells.Count / (run * ChamberTier.Rise));
            int place = run - 1 - (along % run);
            canvas.Grid.Set(cell.X, cell.Y, cell.Z, new Ramp(RiseToward(tier.Ramp), run, place).Id);
        }
    }

    /// <summary>The share of one shrink step: the drawn share first, the smallest tier last, and one step between them (D-391).</summary>
    private static int Shrink(int target, int step)
    {
        if (step == 0)
        {
            return target;
        }

        if (step + 1 >= ShrinkSteps)
        {
            return SmallestTierCells;
        }

        int middle = (target + SmallestTierCells) / 2;
        return middle < SmallestTierCells ? SmallestTierCells : middle;
    }

    /// <summary>The candidate floors of one shape at one target count, nearest the target first.</summary>
    private static IReadOnlyList<IReadOnlyList<Column>> Candidates(ChamberSpace space, TierShape shape, int target)
    {
        switch (shape)
        {
            case TierShape.Rectangle:
                return TierShapes.AgainstOneSide(space, target);
            case TierShape.CutLine:
                return TierShapes.PastACutLine(space, target);
            case TierShape.RaisedBox:
                return TierShapes.OneBoxOfTheUnion(space, target);
            default:
                return TierShapes.AnIsland(space, target);
        }
    }

    /// <summary>
    /// The runs of the template that take a ramp beside the candidate floor, in the order of the template (D-390).
    /// A run fits when one placement of <see cref="FirstRamp"/> holds.
    /// </summary>
    private static IReadOnlyList<int> FittingRuns(FloorTemplate template, ChamberSpace space, IReadOnlyList<Column> floor)
    {
        List<int> fits = [];
        foreach (int run in template.RampSlopeRuns)
        {
            if (FindRamp(space, floor, run, out _))
            {
                fits.Add(run);
            }
        }

        return fits;
    }

    /// <summary>The first placement of a ramp of one run beside the floor, in the order of <see cref="FindRamp"/>.</summary>
    /// <exception cref="Logging.ContextException">No placement holds. The caller reads <see cref="FittingRuns"/> first, so this is a defect of the caller (T-2).</exception>
    private static DugRamp FirstRamp(ChamberSpace space, IReadOnlyList<Column> floor, int run)
    {
        if (!FindRamp(space, floor, run, out DugRamp ramp))
        {
            Logging.ContextException error = new($"No ramp of run {run} fits beside a tier of {floor.Count} cells, and the plan read the run from the runs that fit (D-390).");
            error.AddContext("tierCells", ((long)floor.Count).ToString(System.Globalization.CultureInfo.InvariantCulture));
            error.AddContext("rampRun", ((long)run).ToString(System.Globalization.CultureInfo.InvariantCulture));
            throw error;
        }

        return ramp;
    }

    /// <summary>The widest ramp of one run that joins the chamber floor to the tier floor, or false when no width of D-393 fits.</summary>
    private static bool FindRamp(ChamberSpace space, IReadOnlyList<Column> floor, int run, out DugRamp ramp)
    {
        for (int width = WidestRamp; width >= NarrowestRamp; width--)
        {
            if (FindRampOfWidth(space, floor, run, width, out ramp))
            {
                return true;
            }
        }

        ramp = EmptyRamp;
        return false;
    }

    /// <summary>
    /// The first ramp of one run and one width that joins the chamber floor to the tier floor, or false when none
    /// fits. The search runs over the four directions in turn, and over the tier columns in scan order, so one
    /// chamber gives one ramp. The corridor of the ramp holds the width across and twice the run along, and one
    /// more cell of chamber floor at the low end. Every cell of it lies in the footprint and outside the tier.
    /// </summary>
    private static bool FindRampOfWidth(ChamberSpace space, IReadOnlyList<Column> floor, int run, int width, out DugRamp ramp)
    {
        int half = width / 2;
        int acrossLow = -half;
        int acrossHigh = width - 1 - half;
        bool[] tier = space.Mark(floor);
        int length = ChamberTier.Rise * run;
        for (int direction = 0; direction < 4; direction++)
        {
            int stepX = StepX(direction);
            int stepZ = StepZ(direction);
            foreach (Column top in floor)
            {
                if (!EdgeFacesAway(space, tier, top, stepX, stepZ, acrossLow, acrossHigh) || !CorridorIsClear(space, tier, top, stepX, stepZ, length, acrossLow, acrossHigh))
                {
                    continue;
                }

                if (!GroundIsWhole(space, tier, top, stepX, stepZ, length, acrossLow, acrossHigh))
                {
                    continue;
                }

                List<Cell> cells = [];
                for (int along = 1; along <= length; along++)
                {
                    int row = space.FloorRow + ChamberTier.Rise - ((along - 1) / run);
                    for (int across = acrossLow; across <= acrossHigh; across++)
                    {
                        Column column = Offset(top, -(stepX * along) + (stepZ * across), -(stepZ * along) + (stepX * across));
                        cells.Add(new Cell(column.X, row, column.Z));
                    }
                }

                Column low = Offset(top, -(stepX * (length + 1)), -(stepZ * (length + 1)));
                ramp = new DugRamp(run, ChamberTier.Rise, cells, new Cell(low.X, space.FloorRow, low.Z), new Cell(top.X, space.FloorRow + ChamberTier.Rise, top.Z));
                return true;
            }
        }

        ramp = EmptyRamp;
        return false;
    }

    /// <summary>Answers whether the three tier cells across the top of a ramp are tier, and the cell behind the middle one is not.</summary>
    private static bool EdgeFacesAway(ChamberSpace space, bool[] tier, Column top, int stepX, int stepZ, int acrossLow, int acrossHigh)
    {
        for (int across = acrossLow; across <= acrossHigh; across++)
        {
            Column column = Offset(top, stepZ * across, stepX * across);
            if (!space.Holds(tier, column))
            {
                return false;
            }
        }

        return !space.Holds(tier, Offset(top, -stepX, -stepZ));
    }

    /// <summary>
    /// Answers whether the corridor of the ramp, and one cell of chamber floor past its low end, lie in the
    /// footprint and outside the tier. The ramp fills the two rows over the chamber floor of its own columns, so
    /// those columns take the same rule as a tier column. The cell past the low end stays open chamber floor, so a
    /// column that must stay open holds it.
    /// </summary>
    private static bool CorridorIsClear(ChamberSpace space, bool[] tier, Column top, int stepX, int stepZ, int length, int acrossLow, int acrossHigh)
    {
        for (int along = 1; along <= length + 1; along++)
        {
            for (int across = acrossLow; across <= acrossHigh; across++)
            {
                Column column = Offset(top, -(stepX * along) + (stepZ * across), -(stepZ * along) + (stepX * across));
                if (!space.InFootprint(column) || space.Holds(tier, column) || space.IsSloped(column))
                {
                    return false;
                }

                if (along <= length && space.MustStayOpen(column))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Answers whether a candidate floor is one connected region. A cut line and a box of the union can each take
    /// two parts of a footprint that meet nowhere, and the ramp reaches one of the two parts alone.
    /// </summary>
    private static bool IsWhole(ChamberSpace space, IReadOnlyList<Column> floor)
    {
        bool[] left = space.Mark(floor);
        List<Column> queue = [floor[0]];
        space.Clear(left, floor[0]);
        int reached = 1;
        for (int index = 0; index < queue.Count; index++)
        {
            Column column = queue[index];
            Column[] neighbors = [new(column.X + 1, column.Z), new(column.X - 1, column.Z), new(column.X, column.Z + 1), new(column.X, column.Z - 1)];
            foreach (Column neighbor in neighbors)
            {
                if (space.Holds(left, neighbor))
                {
                    space.Clear(left, neighbor);
                    queue.Add(neighbor);
                    reached++;
                }
            }
        }

        return reached == floor.Count;
    }

    /// <summary>Answers whether a candidate floor covers a column of the chamber floor that must stay open.</summary>
    private static bool CoversAnOpenColumn(ChamberSpace space, IReadOnlyList<Column> floor)
    {
        foreach (Column column in floor)
        {
            if (space.MustStayOpen(column) || space.IsSloped(column))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Answers whether the chamber floor that the tier and its ramp leave is one connected region. A tier that cuts
    /// the floor in two takes the way to one of the parts away, and the chamber then holds floor that no body reaches.
    /// </summary>
    private static bool GroundIsWhole(ChamberSpace space, bool[] tier, Column top, int stepX, int stepZ, int length, int acrossLow, int acrossHigh)
    {
        bool[] ground = space.Mark(space.Footprint);
        int count = space.Footprint.Count;
        count -= Clear(space, ground, tier);
        for (int along = 1; along <= length; along++)
        {
            for (int across = acrossLow; across <= acrossHigh; across++)
            {
                Column column = Offset(top, -(stepX * along) + (stepZ * across), -(stepZ * along) + (stepX * across));
                if (space.Holds(ground, column))
                {
                    space.Clear(ground, column);
                    count--;
                }
            }
        }

        if (count <= 0)
        {
            return false;
        }

        Column start = space.Columns(ground)[0];
        List<Column> queue = [start];
        space.Clear(ground, start);
        int reached = 1;
        for (int index = 0; index < queue.Count; index++)
        {
            Column column = queue[index];
            Column[] neighbors = [new(column.X + 1, column.Z), new(column.X - 1, column.Z), new(column.X, column.Z + 1), new(column.X, column.Z - 1)];
            foreach (Column neighbor in neighbors)
            {
                if (space.Holds(ground, neighbor))
                {
                    space.Clear(ground, neighbor);
                    queue.Add(neighbor);
                    reached++;
                }
            }
        }

        return reached == count;
    }

    /// <summary>Clears from a mask every column that a second mask holds, and gives the count cleared.</summary>
    private static int Clear(ChamberSpace space, bool[] mask, bool[] remove)
    {
        int cleared = 0;
        foreach (Column column in space.Columns(remove))
        {
            if (space.Holds(mask, column))
            {
                space.Clear(mask, column);
                cleared++;
            }
        }

        return cleared;
    }

    /// <summary>The rise direction of a tier ramp: from its low end toward its high end.</summary>
    private static RampRise RiseToward(DugRamp ramp)
    {
        int stepX = ramp.HighEnd.X - ramp.LowEnd.X;
        int stepZ = ramp.HighEnd.Z - ramp.LowEnd.Z;
        if (stepX > 0)
        {
            return RampRise.PlusX;
        }

        if (stepX < 0)
        {
            return RampRise.MinusX;
        }

        return stepZ > 0 ? RampRise.PlusZ : RampRise.MinusZ;
    }

    /// <summary>One column at an offset from another.</summary>
    private static Column Offset(Column column, int alongX, int alongZ)
    {
        return new Column(column.X + alongX, column.Z + alongZ);
    }

    /// <summary>The X step of one of the four axis directions.</summary>
    private static int StepX(int direction)
    {
        return direction == 0 ? 1 : direction == 1 ? -1 : 0;
    }

    /// <summary>The Z step of one of the four axis directions.</summary>
    private static int StepZ(int direction)
    {
        return direction == 2 ? 1 : direction == 3 ? -1 : 0;
    }

    /// <summary>The ramp of a candidate that has no ramp yet. No tier of a floor plan ever holds it.</summary>
    private static readonly DugRamp EmptyRamp = new(Ramp.SteepestRun, ChamberTier.Rise, [], default, default);
}
