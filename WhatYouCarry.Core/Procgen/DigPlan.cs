using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// The mine dig plan of one floor (D-253): the walkers that dig the gallery, the drifts, the ramps, the chambers,
/// and the shafts on a <see cref="DigCanvas"/>.
/// </summary>
/// <remarks>
/// <para>
/// The plan is a queue of dig jobs. A job is one walker: it starts on a cell of the dug network, faces one of
/// the four axis directions, and digs one stamp per step with a brush of a radius, three rows high. The gallery
/// walker takes the wide brush, and a drift walker takes the three by three brush. On each step the walker can
/// turn, dig a ramp of one-block steps up or down, start a drift as a new job, or dig the next chamber where it
/// stands. A walker that can dig in no direction ends its job. When the queue runs dry before every chamber is
/// dug, a new drift starts from a random cell of the trail, so the plan always has a place to dig from.
/// </para>
/// <para>
/// Every unit attaches at a cell the walker stands on, and the canvas refuses a unit that breaks a floor, so
/// every dug cell stays reachable from the first chamber. The shafts come last, one try per chamber: a three by
/// three hole in a chamber floor whose ring of chamber floor stays, over air that the plan dug before.
/// </para>
/// <para>
/// The plan keeps, for the detail pass, which walker dug each air cell and which walkers have a dependent: a
/// chamber, a drift, or a later walker on their trail. A collapse fills the end of a walker with no dependent,
/// in cells that walker alone dug, so no path to a chamber goes with it (D-253).
/// </para>
/// <para>
/// Every draw comes from the one Procgen stream of the floor, in program order, so one seed gives one plan
/// (D-159). Every number is an integer.
/// </para>
/// </remarks>
public sealed class DigPlan
{
    /// <summary>The brush radius of the gallery, so the gallery is five blocks wide.</summary>
    public const int GalleryRadius = 2;

    /// <summary>The brush radius of a drift, so a drift is three blocks wide (D-166).</summary>
    public const int DriftRadius = 1;

    /// <summary>The height of every tunnel, in air rows (D-166).</summary>
    public const int TunnelHeight = 3;

    /// <summary>The lowest chamber height, in air rows.</summary>
    public const int ChamberHeightMin = 3;

    /// <summary>The highest chamber height, in air rows.</summary>
    public const int ChamberHeightMax = 4;

    /// <summary>The shortest ramp, in one-block steps.</summary>
    public const int RampLengthMin = 2;

    /// <summary>The longest ramp, in one-block steps.</summary>
    public const int RampLengthMax = 5;

    /// <summary>The half side of a shaft hole. The hole is three by three.</summary>
    public const int ShaftRadius = 1;

    /// <summary>The count of drift generations under the gallery. A drift at this depth starts no drift.</summary>
    public const int MaxBranchDepth = 3;

    /// <summary>The count of jobs before a floor that still lacks a chamber is an error.</summary>
    public const int MaxJobs = 400;

    /// <summary>The count of anchors tried for the first chamber before the floor is an error.</summary>
    public const int MaxFirstChamberTries = 100;

    // One in this many steps turns, ramps, or branches.
    private const int TurnChance = 8;
    private const int RampChance = 12;
    private const int BranchChance = 10;

    private readonly Rng rng;
    private readonly DigCanvas canvas;
    private readonly IReadOnlyList<ChamberKind> kinds;
    private readonly List<Chamber> chambers = [];
    private readonly List<Shaft> shafts = [];
    private readonly int[] chamberOf;
    private readonly int[] dugBy;
    private readonly List<Cell> trail = [];
    private readonly List<DigJob> jobs = [];
    private readonly List<DeadEnd> deadEnds = [];
    private readonly List<bool> hasDependent = [];
    private readonly List<int> trailJobs = [];
    private int nextJob;

    // The mark of a cell that two walkers dug, or a chamber, in the dug-by map. A job mark is the job index plus one.
    private const int Shared = -1;

    /// <summary>A plan that digs one chamber per kind, in order, on the canvas.</summary>
    public DigPlan(Rng rng, DigCanvas canvas, IReadOnlyList<ChamberKind> kinds)
    {
        this.rng = rng;
        this.canvas = canvas;
        this.kinds = kinds;
        this.chamberOf = new int[canvas.Grid.SizeX * canvas.Grid.SizeY * canvas.Grid.SizeZ];
        this.dugBy = new int[this.chamberOf.Length];
    }

    /// <summary>The chambers dug so far, in dig order.</summary>
    public IReadOnlyList<Chamber> Chambers => this.chambers;

    /// <summary>The shafts dug so far, in dig order.</summary>
    public IReadOnlyList<Shaft> Shafts => this.shafts;

    /// <summary>The walkers that ended in rock, with the cell and the brush radius of their last stamp. The detail pass fills them with rubble.</summary>
    public IReadOnlyList<DeadEnd> DeadEnds => this.deadEnds;

    /// <summary>Answers whether one walker alone dug the air cell, and no chamber holds it. The detail pass fills such a cell of a dead end.</summary>
    public bool IsDugByJobAlone(int x, int y, int z, int job)
    {
        return this.dugBy[this.Index(x, y, z)] == job + 1;
    }

    /// <summary>
    /// Answers whether anything depends on the cells of a walker: it dug a chamber, it started a drift, or a later
    /// walker started from its trail. A collapse never touches such a walker, so no path to a chamber goes.
    /// </summary>
    public bool HasDependent(int job)
    {
        return this.hasDependent[job];
    }

    /// <summary>Digs the first chamber at a random anchor away from the shell, and queues the gallery from it.</summary>
    /// <exception cref="ContextException">No anchor of <see cref="MaxFirstChamberTries"/> takes the first chamber.</exception>
    public void DigFirstChamber()
    {
        int marginX = this.canvas.Grid.SizeX / 4;
        int marginZ = this.canvas.Grid.SizeZ / 4;
        int lowestRow = DigCanvas.LowestFloorRow;
        int highestRow = this.canvas.HighestFloorRow(ChamberHeightMax);
        for (int attempt = 0; attempt < MaxFirstChamberTries; attempt++)
        {
            Column anchor = new(
                marginX + this.rng.NextInt(this.canvas.Grid.SizeX - (2 * marginX)),
                marginZ + this.rng.NextInt(this.canvas.Grid.SizeZ - (2 * marginZ)));
            int floorRow = lowestRow + this.rng.NextInt(highestRow - lowestRow + 1);
            if (!this.TryDigChamber(anchor, floorRow))
            {
                continue;
            }

            Cell start = new(anchor.X, floorRow, anchor.Z);
            this.AddTrail(start, -1);
            int direction = this.rng.NextInt(4);
            this.AddJob(new DigJob(start, StepX(direction), StepZ(direction), GalleryRadius, this.canvas.Grid.SizeX + this.canvas.Grid.SizeZ, 0), -1);
            return;
        }

        ContextException error = new($"No anchor of {MaxFirstChamberTries} takes the first chamber of kind '{this.kinds[0].Id}' on a grid of {this.canvas.Grid.SizeX} by {this.canvas.Grid.SizeY} by {this.canvas.Grid.SizeZ} blocks.");
        error.AddContext("chamberKind", this.kinds[0].Id);
        throw error;
    }

    /// <summary>Runs dig jobs until every chamber is dug.</summary>
    /// <exception cref="ContextException"><see cref="MaxJobs"/> jobs ran and a chamber is still not dug.</exception>
    public void DigUntilComplete()
    {
        int jobCount = 0;
        while (this.chambers.Count < this.kinds.Count)
        {
            if (jobCount >= MaxJobs)
            {
                ContextException error = new($"The dig plan ran {MaxJobs} jobs and dug {this.chambers.Count} of {this.kinds.Count} chambers.");
                error.AddContext("chambersDug", ((long)this.chambers.Count).ToString(CultureInfo.InvariantCulture));
                error.AddContext("chambersNeeded", ((long)this.kinds.Count).ToString(CultureInfo.InvariantCulture));
                throw error;
            }

            if (this.nextJob >= this.jobs.Count)
            {
                int trailIndex = this.rng.NextInt(this.trail.Count);
                int direction = this.rng.NextInt(4);
                this.AddJob(new DigJob(this.trail[trailIndex], StepX(direction), StepZ(direction), DriftRadius, this.DriftLength(), 0), this.trailJobs[trailIndex]);
            }

            DigJob job = this.jobs[this.nextJob];
            int jobIndex = this.nextJob;
            this.nextJob++;
            jobCount++;
            this.RunJob(job, jobIndex);
        }
    }

    /// <summary>Tries one shaft per chamber, at a random column of a random chamber each time.</summary>
    public void DigShafts()
    {
        int tries = this.chambers.Count;
        for (int attempt = 0; attempt < tries; attempt++)
        {
            Chamber chamber = this.chambers[this.rng.NextInt(this.chambers.Count)];
            Column center = chamber.Footprint[this.rng.NextInt(chamber.Footprint.Count)];
            this.TryDigShaft(chamber, center);
        }
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

    /// <summary>The square stamp of one walker step: every column within the radius, at one floor row.</summary>
    private static List<DigColumn> Stamp(Column center, int floorRow, int radius, int height)
    {
        List<DigColumn> unit = [];
        for (int z = center.Z - radius; z <= center.Z + radius; z++)
        {
            for (int x = center.X - radius; x <= center.X + radius; x++)
            {
                unit.Add(new DigColumn(x, z, floorRow, height));
            }
        }

        return unit;
    }

    /// <summary>Queues a job, and marks the walker whose trail it starts from as one with a dependent.</summary>
    private void AddJob(DigJob job, int parentJob)
    {
        this.jobs.Add(job);
        this.hasDependent.Add(false);
        if (parentJob >= 0)
        {
            this.hasDependent[parentJob] = true;
        }
    }

    /// <summary>Adds a walker position to the trail, with the walker that stood there.</summary>
    private void AddTrail(Cell position, int jobIndex)
    {
        this.trail.Add(position);
        this.trailJobs.Add(jobIndex);
    }

    /// <summary>The length of one drift, in steps.</summary>
    private int DriftLength()
    {
        return 8 + this.rng.NextInt(this.canvas.Grid.SizeX / 2);
    }

    /// <summary>The count of steps before the walker tries the next chamber.</summary>
    private int ChamberSpacing()
    {
        return 6 + this.rng.NextInt(9);
    }

    /// <summary>Walks one job to its length, or until the walker is stuck, or until every chamber is dug. A walker that ends in rock leaves a dead end.</summary>
    private void RunJob(DigJob job, int jobIndex)
    {
        Cell position = job.Start;
        int directionX = job.DirectionX;
        int directionZ = job.DirectionZ;
        int untilChamber = this.ChamberSpacing();

        // A ramp starts past the stamp around the walker, so it needs a stamp of this brush around the position
        // first. The start of a job is a cell of another walker, whose brush can be narrower.
        bool stamped = false;
        for (int step = 0; step < job.Length && this.chambers.Count < this.kinds.Count; step++)
        {
            if (this.rng.NextInt(TurnChance) == 0)
            {
                bool left = this.rng.NextInt(2) == 0;
                int turnedX = left ? -directionZ : directionZ;
                int turnedZ = left ? directionX : -directionX;
                directionX = turnedX;
                directionZ = turnedZ;
            }

            if (stamped && this.rng.NextInt(RampChance) == 0 && this.TryDigRamp(position, directionX, directionZ, job.Radius, jobIndex, out Cell afterRamp))
            {
                position = afterRamp;
                this.AddTrail(position, jobIndex);
                continue;
            }

            if (!this.TryDigStep(position, ref directionX, ref directionZ, job.Radius, jobIndex, out Cell next))
            {
                break;
            }

            position = next;
            stamped = true;
            this.AddTrail(position, jobIndex);

            untilChamber--;
            if (untilChamber <= 0)
            {
                bool dug = this.TryDigChamber(new Column(position.X, position.Z), position.Y);
                untilChamber = dug ? this.ChamberSpacing() : 3;
                if (dug)
                {
                    this.hasDependent[jobIndex] = true;
                }
            }

            if (job.Depth < MaxBranchDepth && this.rng.NextInt(BranchChance) == 0)
            {
                bool left = this.rng.NextInt(2) == 0;
                int branchX = left ? -directionZ : directionZ;
                int branchZ = left ? directionX : -directionX;
                this.AddJob(new DigJob(position, branchX, branchZ, DriftRadius, this.DriftLength(), job.Depth + 1), jobIndex);
            }
        }

        // The walker ended where it stands, in a stamp of its own or in a chamber. A chamber cell is never a dead end.
        if (stamped && this.chamberOf[this.Index(position.X, position.Y + 1, position.Z)] == 0)
        {
            this.deadEnds.Add(new DeadEnd(position, job.Radius, jobIndex));
        }
    }

    /// <summary>
    /// Digs one flat step in the facing direction, or after a turn when the facing direction is blocked: left,
    /// then right, then back. Gives false when every direction is blocked.
    /// </summary>
    private bool TryDigStep(Cell position, ref int directionX, ref int directionZ, int radius, int jobIndex, out Cell next)
    {
        int[] quarterTurns = [0, 1, 3, 2];
        foreach (int quarter in quarterTurns)
        {
            int stepX = quarter == 0 ? directionX : quarter == 1 ? -directionZ : quarter == 2 ? -directionX : directionZ;
            int stepZ = quarter == 0 ? directionZ : quarter == 1 ? directionX : quarter == 2 ? -directionZ : -directionX;
            Column center = new(position.X + stepX, position.Z + stepZ);
            List<DigColumn> unit = Stamp(center, position.Y, radius, TunnelHeight);
            if (this.canvas.CanCarve(unit))
            {
                this.MarkDug(unit, jobIndex);
                this.canvas.Carve(unit);
                directionX = stepX;
                directionZ = stepZ;
                next = new Cell(center.X, position.Y, center.Z);
                return true;
            }
        }

        next = position;
        return false;
    }

    /// <summary>
    /// Digs a ramp of one-block steps ahead of the walker: the slope starts past the stamp around the walker,
    /// and a landing follows it that reaches one brush radius past the new position on every side. The walker
    /// then stands in a landing that has the shape of a flat stamp, so the next step or ramp meets the new floor
    /// row alone and leaves no gap.
    /// </summary>
    private bool TryDigRamp(Cell position, int directionX, int directionZ, int radius, int jobIndex, out Cell after)
    {
        int length = RampLengthMin + this.rng.NextInt(RampLengthMax - RampLengthMin + 1);
        int stepY = this.rng.NextInt(2) == 0 ? -1 : 1;
        int endRow = position.Y + (stepY * length);
        if (endRow < DigCanvas.LowestFloorRow || endRow > this.canvas.HighestFloorRow(ChamberHeightMax))
        {
            stepY = -stepY;
            endRow = position.Y + (stepY * length);
        }

        if (endRow < DigCanvas.LowestFloorRow || endRow > this.canvas.HighestFloorRow(ChamberHeightMax))
        {
            after = position;
            return false;
        }

        List<DigColumn> unit = [];
        int landingCenter = (2 * radius) + length + 1;
        int landingEnd = landingCenter + radius;
        for (int distance = radius + 1; distance <= landingEnd; distance++)
        {
            int slopeStep = distance - radius;
            int floorRow = slopeStep <= length ? position.Y + (stepY * slopeStep) : endRow;
            int alongX = position.X + (directionX * distance);
            int alongZ = position.Z + (directionZ * distance);
            for (int across = -radius; across <= radius; across++)
            {
                // Across is at a right angle to the direction: it runs along Z on an X walk, and along X on a Z walk.
                unit.Add(new DigColumn(alongX + (directionZ * across), alongZ + (directionX * across), floorRow, TunnelHeight));
            }
        }

        if (!this.canvas.CanCarve(unit))
        {
            after = position;
            return false;
        }

        this.MarkDug(unit, jobIndex);
        this.canvas.Carve(unit);
        after = new Cell(position.X + (directionX * landingCenter), endRow, position.Z + (directionZ * landingCenter));
        return true;
    }

    /// <summary>Digs the next chamber around an anchor at a floor row, when its whole unit passes the rules and touches no other chamber.</summary>
    private bool TryDigChamber(Column anchor, int floorRow)
    {
        ChamberKind kind = this.kinds[this.chambers.Count];
        IReadOnlyList<Column> footprint = ChamberFootprint.Make(this.rng, kind, anchor);
        int height = ChamberHeightMin + this.rng.NextInt(ChamberHeightMax - ChamberHeightMin + 1);

        List<DigColumn> unit = [];
        foreach (Column column in footprint)
        {
            unit.Add(new DigColumn(column.X, column.Z, floorRow, height));
        }

        if (!this.canvas.CanCarve(unit))
        {
            return false;
        }

        foreach (DigColumn column in unit)
        {
            for (int row = floorRow + 1; row <= floorRow + height; row++)
            {
                if (this.chamberOf[this.Index(column.X, row, column.Z)] != 0)
                {
                    return false;
                }
            }
        }

        this.canvas.Carve(unit);
        int index = this.chambers.Count;
        foreach (DigColumn column in unit)
        {
            for (int row = floorRow + 1; row <= floorRow + height; row++)
            {
                this.chamberOf[this.Index(column.X, row, column.Z)] = index + 1;
                this.dugBy[this.Index(column.X, row, column.Z)] = Shared;
            }
        }

        this.chambers.Add(new Chamber(index, kind, floorRow, height, anchor, footprint));
        return true;
    }

    /// <summary>
    /// Digs a shaft under a chamber column when the five by five block around it is chamber floor, the anchor is
    /// outside the block, and the nine columns of the hole open onto one air row of dug space with two air rows
    /// over each landing floor.
    /// </summary>
    private bool TryDigShaft(Chamber chamber, Column center)
    {
        int ring = ShaftRadius + 1;
        for (int z = center.Z - ring; z <= center.Z + ring; z++)
        {
            for (int x = center.X - ring; x <= center.X + ring; x++)
            {
                if (!this.canvas.Grid.Contains(x, chamber.FloorRow, z))
                {
                    return false;
                }

                bool inChamber = this.chamberOf[this.Index(x, chamber.FloorRow + 1, z)] == chamber.Index + 1;
                bool floorStands = !this.canvas.IsAir(x, chamber.FloorRow, z);
                bool isAnchor = x == chamber.Anchor.X && z == chamber.Anchor.Z;
                if (!inChamber || !floorStands || isAnchor)
                {
                    return false;
                }
            }
        }

        int landingAirRow = this.canvas.TopAirRowBelow(center, chamber.FloorRow);
        if (landingAirRow < 0)
        {
            return false;
        }

        for (int z = center.Z - ShaftRadius; z <= center.Z + ShaftRadius; z++)
        {
            for (int x = center.X - ShaftRadius; x <= center.X + ShaftRadius; x++)
            {
                Column column = new(x, z);
                if (this.canvas.TopAirRowBelow(column, chamber.FloorRow) != landingAirRow)
                {
                    return false;
                }

                if (landingAirRow - this.canvas.FloorRowBelow(column, landingAirRow) < 2)
                {
                    return false;
                }
            }
        }

        for (int z = center.Z - ShaftRadius; z <= center.Z + ShaftRadius; z++)
        {
            for (int x = center.X - ShaftRadius; x <= center.X + ShaftRadius; x++)
            {
                this.canvas.CarveShaft(new Column(x, z), landingAirRow + 1, chamber.FloorRow);
            }
        }

        this.shafts.Add(new Shaft(chamber.Index, center, chamber.FloorRow, landingAirRow));
        return true;
    }

    /// <summary>
    /// Marks the air of a unit with the walker that digs it, before the carve. A cell that was air before, from
    /// another walker or a chamber, becomes shared, so no collapse ever fills it.
    /// </summary>
    private void MarkDug(IReadOnlyList<DigColumn> unit, int jobIndex)
    {
        foreach (DigColumn column in unit)
        {
            for (int row = column.Floor + 1; row <= column.Floor + column.Height; row++)
            {
                int index = this.Index(column.X, row, column.Z);
                if (!this.canvas.IsAir(column.X, row, column.Z))
                {
                    this.dugBy[index] = jobIndex + 1;
                }
                else if (this.dugBy[index] != jobIndex + 1)
                {
                    this.dugBy[index] = Shared;
                }
            }
        }
    }

    /// <summary>The array index of one cell: x fastest, then z, then y, as the grid stores it.</summary>
    private int Index(int x, int y, int z)
    {
        return x + (this.canvas.Grid.SizeX * (z + (this.canvas.Grid.SizeZ * y)));
    }
}

/// <summary>The end of a walker in rock: the cell it stood on, the radius of its brush, and its job index.</summary>
public readonly record struct DeadEnd(Cell End, int Radius, int Job);

/// <summary>One walker: where it starts, which axis direction it faces, its brush radius, its length in steps, and its drift generation.</summary>
public readonly record struct DigJob(Cell Start, int DirectionX, int DirectionZ, int Radius, int Length, int Depth);
