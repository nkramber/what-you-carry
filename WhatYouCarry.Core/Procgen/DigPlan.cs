using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// The mine dig plan of one floor (D-253): the walkers that dig the gallery, the drifts, the ramps, the chambers,
/// and the shafts on a <see cref="DigCanvas"/>, with the dig sizes of the floor template (D-341, D-342).
/// </summary>
/// <remarks>
/// <para>
/// The plan is a queue of dig jobs. A job is one walker: it starts on a cell of the dug network, faces one of
/// the four axis directions, and digs one stamp per step with a square brush of a radius and a height. The gallery
/// walker takes the gallery width and height of the template, and a drift walker takes the drift width and height.
/// On each step the walker can turn, dig a ramp of one-block steps up or down, start a drift as a new job, or dig
/// the next chamber where it stands. A walker that can dig in no direction ends its job. When the queue runs dry
/// before every chamber is dug, a new drift starts from a random cell of the trail, so the plan always has a place
/// to dig from.
/// </para>
/// <para>
/// Every unit attaches at a cell the walker stands on, and the canvas refuses a unit that breaks a floor, so
/// every dug cell stays reachable from the first chamber. The shafts come last, one try per chamber: a three by
/// three hole in a chamber floor whose ring of chamber floor stays, over air that the plan dug before.
/// </para>
/// <para>
/// The plan keeps, for the detail pass, which walker dug each air cell and which walkers have a dependent: a
/// chamber, a drift, or a later walker on their trail. A collapse fills the end of a walker with no dependent,
/// in cells that walker alone dug, so no path to a chamber goes with it (D-253). The plan also keeps the center
/// of every stamp and every ramp landing of a walker, so a check reads each tunnel against the sizes of its template.
/// </para>
/// <para>
/// Every draw comes from the one Procgen stream of the floor, in program order, so one seed gives one plan
/// (D-159). Every number is an integer.
/// </para>
/// </remarks>
public sealed class DigPlan
{
    /// <summary>The smallest rise of one ramp, in blocks (D-347).</summary>
    public const int RampRiseMin = 1;

    /// <summary>
    /// The largest rise of one ramp, in blocks. A rise of three at the shallowest slope of D-346 takes twelve cells
    /// of tunnel, and a longer ramp fills a drift of a small floor.
    /// </summary>
    public const int RampRiseMax = 3;

    /// <summary>The half side of a shaft hole. The hole is three by three.</summary>
    public const int ShaftRadius = 1;

    /// <summary>The count of drift generations under the gallery. A drift at this depth starts no drift.</summary>
    public const int MaxBranchDepth = 3;

    /// <summary>
    /// The count of jobs of one dig before the floor generator digs the floor again (D-359). The last chamber of a
    /// crowded floor finds room only after many walkers end at once (F-92). The measurement of 2026-09-14 dug the 675000
    /// floors of F-98 on the sizes of D-341: one floor in 5500 needs more than 1000 jobs, and 7 ran 10000 jobs with a
    /// chamber in rock (F-98). A heavy floor digs 1000 jobs in about half a second, and the next floor digs inside one
    /// tick, so the budget also bounds the stop of the game at a descend. It replaces the job cap of D-279.
    /// </summary>
    public const int JobBudget = 1000;

    /// <summary>The cells that a drift of the shaft pass walks to reach the chamber over it (F-103).</summary>
    public const int ShaftRouteReach = 28;

    /// <summary>The drifts that the shaft pass digs for one chamber before it leaves that chamber without a landing (F-103).</summary>
    public const int ShaftRouteTries = 6;

    /// <summary>The blocks that a drift of the shaft pass descends by a ramp to pass under its chamber (F-103).</summary>
    public const int ShaftRouteDrop = 6;

    /// <summary>The count of anchors tried for the first chamber before the floor is an error.</summary>
    public const int MaxFirstChamberTries = 100;

    // One in this many steps turns, ramps, or branches.
    private const int TurnChance = 8;
    private const int RampChance = 12;
    private const int BranchChance = 10;

    // The job index of the gallery. The first chamber queues the gallery before any drift.
    private const int GalleryJob = 0;

    private readonly Rng rng;
    private readonly DigCanvas canvas;
    private readonly FloorTemplate template;
    private readonly IReadOnlyList<ChamberKind> kinds;
    private readonly int galleryRadius;
    private readonly int driftRadius;
    private readonly int tallestHeight;
    private readonly List<Chamber> chambers = [];
    private readonly List<IReadOnlyList<ChamberBox>> chamberBoxes = [];
    private readonly List<Shaft> shafts = [];
    private readonly List<TunnelStamp> tunnels = [];
    private readonly List<DugRamp> ramps = [];
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

    /// <summary>A plan that digs one chamber per kind, in order, on the canvas, with the dig sizes of the template (D-342).</summary>
    /// <exception cref="ContextException">A tunnel width of the template is even, which the validator rejects (D-352).</exception>
    public DigPlan(Rng rng, DigCanvas canvas, FloorTemplate template, IReadOnlyList<ChamberKind> kinds)
    {
        this.rng = rng;
        this.canvas = canvas;
        this.template = template;
        this.kinds = kinds;
        this.galleryRadius = RadiusOf(template, "galleryWidth", template.GalleryWidth);
        this.driftRadius = RadiusOf(template, "driftWidth", template.DriftWidth);
        this.tallestHeight = TallestHeight(template);
        this.chamberOf = new int[canvas.Grid.SizeX * canvas.Grid.SizeY * canvas.Grid.SizeZ];
        this.dugBy = new int[this.chamberOf.Length];
    }

    /// <summary>The chambers dug so far, in dig order.</summary>
    public IReadOnlyList<Chamber> Chambers => this.chambers;

    /// <summary>The shafts dug so far, in dig order.</summary>
    public IReadOnlyList<Shaft> Shafts => this.shafts;

    /// <summary>The stamps of the gallery and the drifts so far, in dig order: the center of each step and each ramp landing.</summary>
    public IReadOnlyList<TunnelStamp> Tunnels => this.tunnels;

    /// <summary>The ramps so far, in dig order: the tunnel ramps of D-347, and then the tier ramps of D-348.</summary>
    public IReadOnlyList<DugRamp> Ramps => this.ramps;

    /// <summary>The walkers that ended in rock, with the cell, the brush radius, and the height of their last stamp. The detail pass fills them with rubble.</summary>
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
        int highestRow = this.canvas.HighestFloorRow(this.tallestHeight);
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
            this.AddJob(new DigJob(start, StepX(direction), StepZ(direction), this.galleryRadius, this.template.GalleryHeight, this.canvas.Grid.SizeX + this.canvas.Grid.SizeZ, 0), -1);
            return;
        }

        ContextException error = new($"No anchor of {MaxFirstChamberTries} takes the first chamber of kind '{this.kinds[0].Id}' on a grid of {this.canvas.Grid.SizeX} by {this.canvas.Grid.SizeY} by {this.canvas.Grid.SizeZ} blocks.");
        error.AddContext("chamberKind", this.kinds[0].Id);
        throw error;
    }

    /// <summary>
    /// Runs dig jobs until every chamber is dug, or until <see cref="JobBudget"/> jobs ran. Gives true when every
    /// chamber is dug, and false when the budget ran out with a chamber still in rock, so the floor generator digs the
    /// floor again (D-359). The count of jobs that ran comes out in both cases.
    /// </summary>
    public bool TryDigUntilComplete(out int jobCount)
    {
        jobCount = 0;
        while (this.chambers.Count < this.kinds.Count)
        {
            if (jobCount >= JobBudget)
            {
                return false;
            }

            if (this.nextJob >= this.jobs.Count)
            {
                int trailIndex = this.rng.NextInt(this.trail.Count);
                int direction = this.rng.NextInt(4);
                this.AddJob(new DigJob(this.trail[trailIndex], StepX(direction), StepZ(direction), this.driftRadius, this.template.DriftHeight, this.DriftLength(), 0), this.trailJobs[trailIndex]);
            }

            DigJob job = this.jobs[this.nextJob];
            int jobIndex = this.nextJob;
            this.nextJob++;
            jobCount++;
            this.RunJob(job, jobIndex);
        }

        return true;
    }

    /// <summary>
    /// Builds the tier of each chamber that draws one, in dig order (D-348, D-350). A floor whose draws build none
    /// then takes one tier in the first chamber that fits (D-392). The pass runs after the whole dig, because a tier
    /// fills the two rows over the chamber floor of its own columns, and a walker that met one mid-dig found its way
    /// barred. Every tunnel is dug here, so the plan reads the true entries of each chamber (D-391).
    /// </summary>
    public void BuildTiers()
    {
        bool built = false;
        for (int index = 0; index < this.chambers.Count; index++)
        {
            ChamberSpace space = this.SpaceOf(index);
            ChamberTier? tier = TierPlan.TryPlan(this.rng, this.template, this.chambers[index].Kind, space, out bool drawn);
            this.chambers[index] = this.chambers[index] with { TierDrawn = drawn };
            built = this.Raise(index, tier) || built;
        }

        if (built)
        {
            return;
        }

        // Every floor holds one tier when a chamber of it can take one (D-392). The draws of the tier chance give
        // one tier about every five floors, because a tunnel that crosses a chamber keeps its cross-section and
        // takes no tier. The first chamber that fits takes this one, so the high ground of D-348 reaches every floor.
        for (int index = 0; index < this.chambers.Count; index++)
        {
            // A kind of tier chance zero is a small room, and the high ground belongs to the large kinds (D-350).
            if (this.chambers[index].Kind.TierChance == 0)
            {
                continue;
            }

            if (this.Raise(index, TierPlan.Force(this.rng, this.template, this.SpaceOf(index))))
            {
                return;
            }
        }
    }

    /// <summary>The room that one carved chamber gives a tier, with the columns that must stay open marked.</summary>
    private ChamberSpace SpaceOf(int index)
    {
        Chamber chamber = this.chambers[index];
        ChamberSpace space = new(chamber.FloorRow, chamber.Height, chamber.Anchor, chamber.Footprint, this.chamberBoxes[index]);
        space.Block(this.OpenColumns(space, chamber.FloorRow));
        space.Slope(this.SlopedColumns(space, chamber.FloorRow));
        return space;
    }

    /// <summary>Writes the blocks of a tier on the chamber and keeps its ramp, or answers false when the tier is null.</summary>
    private bool Raise(int index, ChamberTier? tier)
    {
        if (tier is null)
        {
            return false;
        }

        Chamber chamber = this.chambers[index];
        TierPlan.Build(this.canvas, tier, chamber.FloorRow);
        this.ramps.Add(tier.Ramp);
        this.chambers[index] = chamber with { Tier = tier };
        return true;
    }

    /// <summary>
    /// Digs a drift under a chamber, so a shaft of that chamber has a landing (F-103). The pass runs after the whole
    /// dig and before the tiers. A shaft needs dug space under a chamber floor, and the dig of a floor stacks two
    /// spaces by chance alone: the measurement of 2026-09-20 found a landing under 4 floors of 2000.
    /// </summary>
    /// <remarks>
    /// The drift starts at a cell of the trail, so a walker stood there and the network holds it. The body that
    /// drops through the shaft therefore lands in a space that leads on, and the drop is the one-way move of D-253
    /// and never a trap. The pass digs no chamber, so the budget draw of D-167 stands.
    /// </para>
    /// <para>
    /// A cell of the trail over the room that the drift needs takes a ramp down first, of the steepest slope of the
    /// template (D-346). The drift then walks under the chamber at the lower row, and the shaft joins the two.
    /// </remarks>
    public void DigShaftRoutes()
    {
        foreach (Chamber chamber in this.chambers)
        {
            // The first pass takes the cells of the trail that already lie low enough, and the second lets a cell
            // over that room descend by a ramp. A drift with no ramp costs less room, so it comes first.
            if (!this.RouteUnder(chamber, false))
            {
                this.RouteUnder(chamber, true);
            }
        }
    }

    /// <summary>
    /// Digs drifts under one chamber until a shaft of that chamber has a landing, or until the tries run out. Gives
    /// true when the chamber has a landing at the end. A descending pass takes a ramp down from a cell of the trail
    /// that lies over the room that the drift needs.
    /// </summary>
    private bool RouteUnder(Chamber chamber, bool descending)
    {
        if (this.HasAShaftSite(chamber))
        {
            return true;
        }

        int lowest = chamber.FloorRow - this.template.DriftHeight - 1;
        int tries = 0;
        for (int cell = 0; cell < this.trail.Count && tries < ShaftRouteTries; cell++)
        {
            Cell start = this.trail[cell];
            int drop = start.Y - lowest;
            bool needsARamp = drop > 0;
            if (start.Y < DigCanvas.LowestFloorRow || needsARamp != descending || drop > ShaftRouteDrop)
            {
                continue;
            }

            int alongX = chamber.Anchor.X - start.X;
            int alongZ = chamber.Anchor.Z - start.Z;
            int reach = (alongX < 0 ? -alongX : alongX) + (alongZ < 0 ? -alongZ : alongZ);
            if (reach > ShaftRouteReach || reach < this.driftRadius)
            {
                continue;
            }

            int stepX = alongX > 0 ? 1 : -1;
            DigJob job = new(start, stepX, 0, this.driftRadius, this.template.DriftHeight, reach + this.driftRadius, MaxBranchDepth);
            Cell head = start;
            if (needsARamp)
            {
                this.AddJob(job, this.trailJobs[cell]);
                if (!this.TryCarveRamp(start, stepX, 0, job, this.jobs.Count - 1, this.template.RampSlopeRuns[0], drop, -1, out head))
                {
                    continue;
                }

                this.AddTrail(head, this.jobs.Count - 1);
            }
            else
            {
                this.AddJob(job, this.trailJobs[cell]);
            }

            tries++;
            this.RunRoute(job with { Start = head }, this.jobs.Count - 1, chamber.Anchor);
            if (this.HasAShaftSite(chamber))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tries one shaft per chamber, at a random column of a random chamber each time. A floor that takes no shaft
    /// from those tries then reads every chamber column in scan order, so a floor with a landing keeps its shaft
    /// (F-103).
    /// </summary>
    public void DigShafts()
    {
        int tries = this.chambers.Count;
        for (int attempt = 0; attempt < tries; attempt++)
        {
            Chamber chamber = this.chambers[this.rng.NextInt(this.chambers.Count)];
            Column center = chamber.Footprint[this.rng.NextInt(chamber.Footprint.Count)];
            this.TryDigShaft(chamber, center);
        }

        if (this.shafts.Count > 0)
        {
            return;
        }

        foreach (Chamber chamber in this.chambers)
        {
            foreach (Column center in chamber.Footprint)
            {
                if (this.TryDigShaft(chamber, center))
                {
                    return;
                }
            }
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

    /// <summary>
    /// The brush radius of a tunnel width: the columns on each side of the walker. The validator rejects an even width
    /// (D-352), so an even one here comes from a template that no validator read, and it is an error and never a
    /// narrower tunnel (T-2).
    /// </summary>
    /// <exception cref="ContextException">The width is even.</exception>
    private static int RadiusOf(FloorTemplate template, string name, int width)
    {
        if (width % 2 == 0)
        {
            ContextException error = new($"The floor template '{template.Id}' has an even {name} of {width}, and the brush of a walker is centered on its cell with an odd width alone (D-352).");
            error.AddContext("floorTemplate", template.Id);
            error.AddContext(name, ((long)width).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        return (width - 1) / 2;
    }

    /// <summary>
    /// The tallest dig height of a template: the highest chamber, or a tunnel that stands taller. A floor row under this
    /// height has room for every unit that a walker can dig from that row.
    /// </summary>
    private static int TallestHeight(FloorTemplate template)
    {
        int tallest = template.ChamberHeightMax;
        if (template.GalleryHeight > tallest)
        {
            tallest = template.GalleryHeight;
        }

        if (template.DriftHeight > tallest)
        {
            tallest = template.DriftHeight;
        }

        return tallest;
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

    /// <summary>
    /// Adds a walker position to the trail, with the walker that stood there. The position of a walker is the center of
    /// a stamp or a ramp landing that it dug, so the tunnel list keeps it too. The anchor of the first chamber has no walker.
    /// </summary>
    private void AddTrail(Cell position, int jobIndex)
    {
        this.trail.Add(position);
        this.trailJobs.Add(jobIndex);
        if (jobIndex >= 0)
        {
            this.tunnels.Add(new TunnelStamp(position, jobIndex == GalleryJob));
        }
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

    /// <summary>
    /// Walks one drift of the shaft pass toward a column, one flat step at a time (F-103). The walk digs no ramp,
    /// starts no drift, and digs no chamber, because every chamber of the draw is dug when this pass runs. It aims
    /// at the column over it, and it ends when the walker stands under that column or when a step finds no room.
    /// </summary>
    private void RunRoute(DigJob job, int jobIndex, Column target)
    {
        Cell position = job.Start;
        int directionX = job.DirectionX;
        int directionZ = job.DirectionZ;
        for (int step = 0; step < job.Length; step++)
        {
            if (position.X == target.X && position.Z == target.Z)
            {
                return;
            }

            // The walker takes the axis with the longer distance left, so it turns the corner once on its way.
            int alongX = target.X - position.X;
            int alongZ = target.Z - position.Z;
            bool alongTheXAxis = (alongX < 0 ? -alongX : alongX) >= (alongZ < 0 ? -alongZ : alongZ);
            directionX = alongTheXAxis ? (alongX > 0 ? 1 : -1) : 0;
            directionZ = alongTheXAxis ? 0 : (alongZ > 0 ? 1 : -1);
            if (!this.TryDigStep(position, ref directionX, ref directionZ, job, jobIndex, out Cell next))
            {
                return;
            }

            position = next;
            this.AddTrail(position, jobIndex);
        }
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

            if (stamped && this.rng.NextInt(RampChance) == 0 && this.TryDigRamp(position, directionX, directionZ, job, jobIndex, out Cell afterRamp))
            {
                position = afterRamp;
                this.AddTrail(position, jobIndex);
                continue;
            }

            if (!this.TryDigStep(position, ref directionX, ref directionZ, job, jobIndex, out Cell next))
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
                this.AddJob(new DigJob(position, branchX, branchZ, this.driftRadius, this.template.DriftHeight, this.DriftLength(), job.Depth + 1), jobIndex);
            }
        }

        // The walker ended where it stands, in a stamp of its own or in a chamber. A chamber cell is never a dead end.
        if (stamped && this.chamberOf[this.Index(position.X, position.Y + 1, position.Z)] == 0)
        {
            this.deadEnds.Add(new DeadEnd(position, job.Radius, job.Height, jobIndex));
        }
    }

    /// <summary>
    /// Digs one flat step in the facing direction, or after a turn when the facing direction is blocked: left,
    /// then right, then back. Gives false when every direction is blocked.
    /// </summary>
    private bool TryDigStep(Cell position, ref int directionX, ref int directionZ, DigJob job, int jobIndex, out Cell next)
    {
        int[] quarterTurns = [0, 1, 3, 2];
        foreach (int quarter in quarterTurns)
        {
            int stepX = quarter == 0 ? directionX : quarter == 1 ? -directionZ : quarter == 2 ? -directionX : directionZ;
            int stepZ = quarter == 0 ? directionZ : quarter == 1 ? directionX : quarter == 2 ? -directionZ : -directionX;
            Column center = new(position.X + stepX, position.Z + stepZ);
            List<DigColumn> unit = Stamp(center, position.Y, job.Radius, job.Height);
            if (this.canvas.CanCarveWithoutStep(unit))
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
    /// Digs one ramp ahead of the walker, up or down (D-345, D-347). The slope starts past the stamp around the
    /// walker, and a landing follows it that reaches one brush radius past the new position on every side. The
    /// walker then stands in a landing that has the shape of a flat stamp, so the next step or ramp meets the new
    /// floor row alone and leaves no gap.
    /// </summary>
    /// <remarks>
    /// The Procgen stream draws the slope from the list of the template, and then the rise in blocks and the
    /// direction (D-159, D-346, D-390). A ramp of a run and a rise takes run times rise cells of slope: each group
    /// of run cells rises one block, and the places inside a group run from the low face to the high face. The
    /// floor row of a group is the row whose block the slope of that group replaces, so an upward ramp starts one
    /// row over the walker and a downward ramp starts in the row of the walker.
    /// </remarks>
    private bool TryDigRamp(Cell position, int directionX, int directionZ, DigJob job, int jobIndex, out Cell after)
    {
        int run = this.template.RampSlopeRuns[this.rng.NextInt(this.template.RampSlopeRuns.Count)];
        int rise = RampRiseMin + this.rng.NextInt(RampRiseMax - RampRiseMin + 1);
        int stepY = this.rng.NextInt(2) == 0 ? -1 : 1;
        int endRow = position.Y + (stepY * rise);
        int highestRow = this.canvas.HighestFloorRow(this.tallestHeight);
        if (endRow < DigCanvas.LowestFloorRow || endRow > highestRow)
        {
            stepY = -stepY;
        }

        return this.TryCarveRamp(position, directionX, directionZ, job, jobIndex, run, rise, stepY, out after);
    }

    /// <summary>
    /// Carves one ramp of a run, a rise, and a direction along Y, or gives false when it does not fit. The caller
    /// owns every draw, so the shaft pass carves a ramp down of the rise that it needs (F-103).
    /// </summary>
    private bool TryCarveRamp(Cell position, int directionX, int directionZ, DigJob job, int jobIndex, int run, int rise, int stepY, out Cell after)
    {
        int endRow = position.Y + (stepY * rise);
        int highestRow = this.canvas.HighestFloorRow(this.tallestHeight);
        if (endRow < DigCanvas.LowestFloorRow || endRow > highestRow)
        {
            after = position;
            return false;
        }

        RampRise riseDirection = RiseOf(stepY * directionX, stepY * directionZ);
        int slopeCells = rise * run;
        int radius = job.Radius;
        int landingCenter = (2 * radius) + slopeCells + 1;
        int landingEnd = landingCenter + radius;
        List<DigColumn> unit = [];
        List<DigColumn> landing = [];
        List<Cell> slope = [];
        for (int distance = radius + 1; distance <= landingEnd; distance++)
        {
            int slopeStep = distance - radius;
            bool onSlope = slopeStep <= slopeCells;
            int group = (slopeStep - 1) / run;
            int floorRow = onSlope ? RampRow(position.Y, stepY, group) : endRow;
            int alongX = position.X + (directionX * distance);
            int alongZ = position.Z + (directionZ * distance);
            for (int across = -radius; across <= radius; across++)
            {
                // Across is at a right angle to the direction: it runs along Z on an X walk, and along X on a Z walk.
                DigColumn column = new(alongX + (directionZ * across), alongZ + (directionX * across), floorRow, job.Height);
                unit.Add(column);
                if (onSlope)
                {
                    slope.Add(new Cell(column.X, floorRow, column.Z));
                }
                else
                {
                    landing.Add(column);
                }
            }
        }

        // A ramp cell replaces the block of its own row with a slope, so it digs into solid rock alone. A cell
        // with air over it is the floor of a space that an earlier unit dug, and a slope there would take that
        // floor away, which the two carve rules of the canvas forbid for every other unit (D-253).
        foreach (Cell cell in slope)
        {
            if (this.canvas.IsAir(cell.X, cell.Y + 1, cell.Z))
            {
                after = position;
                return false;
            }
        }

        // The landing is flat ground, so it takes the step rule like every other stamp (D-347). The slope takes the
        // two rules alone, because the step that it makes with the floor at each of its ends is the ramp itself.
        if (!this.canvas.CanCarve(unit) || !this.canvas.CanCarveWithoutStep(landing))
        {
            after = position;
            return false;
        }

        this.MarkDug(unit, jobIndex);
        this.canvas.Carve(unit);
        for (int index = 0; index < slope.Count; index++)
        {
            Cell cell = slope[index];
            int place = RampPlace(stepY, (index / ((2 * radius) + 1)) % run, run);
            this.canvas.Grid.Set(cell.X, cell.Y, cell.Z, new Ramp(riseDirection, run, place).Id);
        }

        after = new Cell(position.X + (directionX * landingCenter), endRow, position.Z + (directionZ * landingCenter));
        Cell low = stepY > 0 ? position : after;
        Cell high = stepY > 0 ? after : position;
        this.ramps.Add(new DugRamp(run, rise, slope, low, high));
        return true;
    }

    /// <summary>
    /// The floor row of one group of a ramp of <paramref name="rise"/> groups from a walker at <paramref name="walkerRow"/>.
    /// An upward ramp replaces the rock one row over the walker, and a downward ramp replaces the rock in the row of
    /// the walker, because the slope of a cell fills the block of its own row.
    /// </summary>
    private static int RampRow(int walkerRow, int stepY, int group)
    {
        return stepY > 0 ? walkerRow + 1 + group : walkerRow - group;
    }

    /// <summary>
    /// The place of a ramp cell inside its group, from the walker outward. An upward ramp meets the walker at the
    /// low face, so the places rise. A downward ramp meets the walker at the high face, so they fall.
    /// </summary>
    private static int RampPlace(int stepY, int inGroup, int run)
    {
        return stepY > 0 ? inGroup : run - 1 - inGroup;
    }

    /// <summary>The rise direction of one axis step. The caller gives the step of the rise, and not the step of the walk.</summary>
    /// <exception cref="ContextException">The step is not one of the four axis directions.</exception>
    private static RampRise RiseOf(int stepX, int stepZ)
    {
        if (stepX == 1)
        {
            return RampRise.PlusX;
        }

        if (stepX == -1)
        {
            return RampRise.MinusX;
        }

        if (stepZ == 1)
        {
            return RampRise.PlusZ;
        }

        if (stepZ == -1)
        {
            return RampRise.MinusZ;
        }

        ContextException error = new($"A ramp rises along one of the four axis directions, and the step ({stepX}, {stepZ}) is none of them (D-345).");
        error.AddContext("stepX", ((long)stepX).ToString(CultureInfo.InvariantCulture));
        error.AddContext("stepZ", ((long)stepZ).ToString(CultureInfo.InvariantCulture));
        throw error;
    }

    /// <summary>
    /// Digs the next chamber around an anchor at a floor row, when its whole unit passes the rules and touches no
    /// other chamber. A chamber of a kind with a tier chance can take a tier, which the dig builds last (D-350).
    /// </summary>
    private bool TryDigChamber(Column anchor, int floorRow)
    {
        ChamberKind kind = this.kinds[this.chambers.Count];
        IReadOnlyList<Column> footprint = ChamberFootprint.Make(this.rng, kind, anchor, out IReadOnlyList<ChamberBox> boxes);
        int height = this.template.ChamberHeightMin + this.rng.NextInt(this.template.ChamberHeightMax - this.template.ChamberHeightMin + 1);

        List<DigColumn> unit = [];
        foreach (Column column in footprint)
        {
            unit.Add(new DigColumn(column.X, column.Z, floorRow, height));
        }

        if (!this.canvas.CanCarveWithoutStep(unit))
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

        this.chambers.Add(new Chamber(index, kind, floorRow, height, anchor, footprint, null, false));
        this.chamberBoxes.Add(boxes);
        return true;
    }

    /// <summary>
    /// The columns of a chamber floor that must stay open chamber floor. A tier fills the two rows over the chamber
    /// floor of its own columns, so it takes none of these (D-348).
    /// </summary>
    /// <remarks>
    /// The first kind is a column that a tunnel joins the chamber through: a column with a neighbor outside the
    /// footprint that holds air in one of those two rows. A tier over one of them would wall the chamber in. The
    /// second kind is a column inside the brush of a tunnel stamp that holds one of those two rows, because a
    /// tunnel keeps the cross-section of its template over its whole length (D-342, D-352).
    /// </remarks>
    private IReadOnlyList<Column> OpenColumns(ChamberSpace space, int floorRow)
    {
        List<Column> open = [];
        foreach (Column column in space.Footprint)
        {
            Column[] neighbors = [new(column.X + 1, column.Z), new(column.X - 1, column.Z), new(column.X, column.Z + 1), new(column.X, column.Z - 1)];
            foreach (Column neighbor in neighbors)
            {
                bool air = this.canvas.IsAir(neighbor.X, floorRow + 1, neighbor.Z) || this.canvas.IsAir(neighbor.X, floorRow + ChamberTier.Rise, neighbor.Z);
                if (!space.InFootprint(neighbor) && air)
                {
                    open.Add(column);
                    break;
                }
            }
        }

        foreach (Column column in space.Footprint)
        {

            Column[] neighbors = [new(column.X + 1, column.Z), new(column.X - 1, column.Z), new(column.X, column.Z + 1), new(column.X, column.Z - 1)];
            foreach (Column neighbor in neighbors)
            {
                int tierRow = floorRow + ChamberTier.Rise;
                if (this.IsPlainFloorAt(neighbor, tierRow + 1) || this.IsPlainFloorAt(neighbor, tierRow - 1))
                {
                    open.Add(column);
                    break;
                }
            }
        }

        foreach (TunnelStamp stamp in this.tunnels)
        {
            int radius = stamp.Gallery ? this.galleryRadius : this.driftRadius;
            int height = stamp.Gallery ? this.template.GalleryHeight : this.template.DriftHeight;
            if (stamp.Center.Y + height <= floorRow || stamp.Center.Y + 1 > floorRow + ChamberTier.Rise)
            {
                continue;
            }

            for (int z = stamp.Center.Z - radius; z <= stamp.Center.Z + radius; z++)
            {
                for (int x = stamp.Center.X - radius; x <= stamp.Center.X + radius; x++)
                {
                    Column column = new(x, z);
                    if (space.InFootprint(column))
                    {
                        open.Add(column);
                    }
                }
            }
        }

        return open;
    }

    /// <summary>
    /// The columns of a chamber floor that hold the slope of a ramp. A chamber takes the cells of a tunnel ramp
    /// into its floor, because a ramp cell is solid. A tier over such a column stands on a slope, and a tier ramp
    /// that starts beside one meets a slope where it needs a flat floor (D-345, D-348).
    /// </summary>
    private IReadOnlyList<Column> SlopedColumns(ChamberSpace space, int floorRow)
    {
        List<Column> sloped = [];
        foreach (Column column in space.Footprint)
        {
            if (this.canvas.Grid.TryGetRamp(column.X, floorRow, column.Z, out _))
            {
                sloped.Add(column);
            }
        }

        return sloped;
    }

    /// <summary>Answers whether the cell of a column is a block with air over it, and no ramp: the floor of a space that a body walks on (D-345).</summary>
    private bool IsPlainFloorAt(Column column, int row)
    {
        return !this.canvas.IsAir(column.X, row, column.Z)
            && this.canvas.IsAir(column.X, row + 1, column.Z)
            && !this.canvas.Grid.TryGetRamp(column.X, row, column.Z, out _);
    }

    /// <summary>
    /// Digs a shaft under a chamber column when the five by five block around it is chamber floor, the anchor is
    /// outside the block, and the nine columns of the hole open onto one air row of dug space with two air rows
    /// over each landing floor.
    /// </summary>
    private bool TryDigShaft(Chamber chamber, Column center)
    {
        if (!this.CanDigShaft(chamber, center, out int landingAirRow))
        {
            return false;
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

    /// <summary>Answers whether a shaft fits under a chamber column, and gives the air row that it lands on.</summary>
    private bool CanDigShaft(Chamber chamber, Column center, out int landingAirRow)
    {
        landingAirRow = -1;
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

                // A tier and its ramp fill the rows over the chamber floor. A hole under one would take the floor
                // of the tier away and leave it over air, so the ring takes open chamber floor alone (D-348).
                bool openFloor = this.canvas.IsAir(x, chamber.FloorRow + 1, z);
                if (!inChamber || !floorStands || isAnchor || !openFloor)
                {
                    return false;
                }
            }
        }

        // The hole takes the floor of every column of it away. A ramp that ends on one of them loses the floor that
        // its slope meets, and the way up or down goes with it (D-345). The detail pass keeps a pillar out of a
        // shaft for the same reason (F-101).
        for (int z = center.Z - ShaftRadius; z <= center.Z + ShaftRadius; z++)
        {
            for (int x = center.X - ShaftRadius; x <= center.X + ShaftRadius; x++)
            {
                if (this.IsRampColumn(new Column(x, z)))
                {
                    return false;
                }
            }
        }

        landingAirRow = this.canvas.TopAirRowBelow(center, chamber.FloorRow);
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

        return true;
    }

    /// <summary>Answers whether a ramp holds the column, or one of the two floor cells at its ends stands on it (D-345).</summary>
    private bool IsRampColumn(Column column)
    {
        foreach (DugRamp ramp in this.ramps)
        {
            if ((ramp.LowEnd.X == column.X && ramp.LowEnd.Z == column.Z) || (ramp.HighEnd.X == column.X && ramp.HighEnd.Z == column.Z))
            {
                return true;
            }

            foreach (Cell cell in ramp.Cells)
            {
                if (cell.X == column.X && cell.Z == column.Z)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Answers whether one column of the chamber takes a shaft.</summary>
    private bool HasAShaftSite(Chamber chamber)
    {
        foreach (Column center in chamber.Footprint)
        {
            if (this.CanDigShaft(chamber, center, out _))
            {
                return true;
            }
        }

        return false;
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

/// <summary>The end of a walker in rock: the cell it stood on, the radius and the height of its brush, and its job index.</summary>
public readonly record struct DeadEnd(Cell End, int Radius, int Height, int Job);

/// <summary>One walker: where it starts, which axis direction it faces, its brush radius and height, its length in steps, and its drift generation.</summary>
public readonly record struct DigJob(Cell Start, int DirectionX, int DirectionZ, int Radius, int Height, int Length, int Depth);
