using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// Digs one floor from the run seed, the floor number, and the content set (D-252 to D-256). Floor n comes from
/// those three alone, so a replay digs the same floor from the record header (D-151, D-236).
/// </summary>
/// <remarks>
/// <para>
/// The template is the one whose depth range holds the floor number. The size of the grid and the dig sizes come
/// from it (D-342), the chamber kinds come from the budget draw, and the <see cref="DigPlan"/> digs the chambers,
/// the tunnels, the ramps, and the shafts. The <see cref="DetailPass"/> then adds the blocks of the band, the pools, the pillars,
/// and the collapses (D-254). The spawn is the center of the anchor cell of the first chamber. The stairwell is
/// the floor cell with the longest walkable path from the spawn after the detail, in the chamber whose nearest
/// floor cell lies farthest (D-256). <see cref="EnemyPlacement"/> then fills every chamber past the first with
/// enemies for its weight (D-167, D-398).
/// </para>
/// <para>
/// A dig that runs the job budget of <see cref="DigPlan.JobBudget"/> with a chamber still in rock starts again on an
/// empty grid: a new budget draw and a new plan, from the next draws of the same Procgen stream (D-359, D-361). The
/// floor still comes from the seed and the floor number alone. A floor with no complete dig in <see cref="MaxDigs"/>
/// digs is an error (D-360).
/// </para>
/// <para>
/// The generator confirms its own construction: every chamber has a reachable floor cell, and the spawn reaches the
/// landing of every shaft and every floor cell of every tier, or the floor is an error that names the seed, the
/// floor, and the chamber, the shaft, or the tier (D-112, F-101, T-2). PR-9 exit test 1 asserts the same from outside over thousands of seeds.
/// </para>
/// </remarks>
public static class FloorGenerator
{
    /// <summary>
    /// The count of digs of one floor before the floor is an error (D-360). The measurement of 2026-09-14 dug the
    /// 675000 floors of F-98, and each of the 123 floors that ran out of the job budget dug every chamber on its second
    /// dig. When each dig fails on its own, four failures in a row come about once in 900 trillion floors.
    /// </summary>
    public const int MaxDigs = 4;

    /// <summary>The dug floor.</summary>
    /// <exception cref="ContextException">The floor is below one, no template or two templates cover it, the content holds no chamber kind, or no dig of <see cref="MaxDigs"/> digs every chamber. The error names the seed and the floor.</exception>
    public static FloorPlan Generate(ulong runSeed, int floor, ContentSet content)
    {
        try
        {
            return Dig(runSeed, floor, content);
        }
        catch (ContextException error)
        {
            error.AddContext("seed", runSeed.ToString(CultureInfo.InvariantCulture));
            error.AddContext("floor", ((long)floor).ToString(CultureInfo.InvariantCulture));
            throw;
        }
    }

    /// <summary>The one template whose depth range holds the floor. The error names the floor in its text, and <see cref="Generate"/> adds it as a field.</summary>
    /// <exception cref="ContextException">The floor is below one, or the count of templates that cover it is not one.</exception>
    public static FloorTemplate TemplateFor(int floor, ContentSet content)
    {
        if (floor < 1)
        {
            throw new ContextException($"The floor number is {floor}, and every run starts at floor 1 (D-3).");
        }

        List<FloorTemplate> covering = [];
        foreach (FloorTemplate template in content.Floors)
        {
            if (template.MinDepth <= floor && floor <= template.MaxDepth)
            {
                covering.Add(template);
            }
        }

        if (covering.Count != 1)
        {
            ContextException error = new($"The content set has {covering.Count} floor templates that cover floor {floor}, and a floor needs exactly one (D-252).");
            error.AddContext("templates", ((long)covering.Count).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        return covering[0];
    }

    /// <summary>The whole dig, inside the context of <see cref="Generate"/>.</summary>
    private static FloorPlan Dig(ulong runSeed, int floor, ContentSet content)
    {
        FloorTemplate template = TemplateFor(floor, content);
        Rng rng = Rng.ForStream(runSeed, RngStream.Procgen, floor);
        DigPlan plan = DigChambers(rng, template, content, out DigCanvas canvas);
        plan.DigShaftRoutes();
        plan.BuildTiers();
        plan.DigShafts();
        DetailResult detail = DetailPass.Apply(rng, canvas, template, plan);

        VoxelGrid grid = canvas.Grid;
        Chamber first = plan.Chambers[0];
        Cell spawnCell = new(first.Anchor.X, first.FloorRow, first.Anchor.Z);
        Reachability reach = Reachability.From(grid, spawnCell);
        Cell stairwell = FarthestChamberCell(plan.Chambers, grid, reach);
        CheckShaftLandings(plan.Shafts, grid, reach);
        CheckTiers(plan.Chambers, grid, reach);
        Vector3 spawn = new(first.Anchor.X + 0.5f, first.FloorRow + 1.0f, first.Anchor.Z + 0.5f);
        IReadOnlyList<EnemySpawn> enemies = EnemyPlacement.Place(grid, plan.Chambers, reach, floor, content.Enemies);
        return new FloorPlan(floor, template, grid, spawn, stairwell, plan.Chambers, plan.Tunnels, plan.Shafts, plan.Ramps, detail, enemies);
    }

    /// <summary>
    /// Draws the chamber kinds and digs them on an empty grid. A dig that runs out of its job budget starts again from
    /// the next draws of the stream (D-359, D-361). Gives the first plan that digs every chamber, with its canvas.
    /// </summary>
    /// <exception cref="ContextException">The budget draw fails, the first chamber of a dig finds no anchor, or no dig of <see cref="MaxDigs"/> digs every chamber (D-360).</exception>
    private static DigPlan DigChambers(Rng rng, FloorTemplate template, ContentSet content, out DigCanvas canvas)
    {
        int chambersDug = 0;
        int chambersNeeded = 0;
        for (int dig = 0; dig < MaxDigs; dig++)
        {
            IReadOnlyList<ChamberKind> kinds = ChamberBudget.Draw(rng, template, content.Chambers);
            canvas = new DigCanvas(new VoxelGrid(template.SizeX, template.SizeY, template.SizeZ));
            DigPlan plan = new(rng, canvas, template, kinds);
            plan.DigFirstChamber();
            if (plan.TryDigUntilComplete(out _))
            {
                return plan;
            }

            chambersDug = plan.Chambers.Count;
            chambersNeeded = kinds.Count;
        }

        ContextException error = new($"Each of {MaxDigs} digs ran {DigPlan.JobBudget} jobs with a chamber still in rock, and the last dug {chambersDug} of {chambersNeeded} chambers (D-360).");
        error.AddContext("digs", ((long)MaxDigs).ToString(CultureInfo.InvariantCulture));
        error.AddContext("jobBudget", ((long)DigPlan.JobBudget).ToString(CultureInfo.InvariantCulture));
        error.AddContext("chambersDug", ((long)chambersDug).ToString(CultureInfo.InvariantCulture));
        error.AddContext("chambersNeeded", ((long)chambersNeeded).ToString(CultureInfo.InvariantCulture));
        throw error;
    }

    /// <summary>
    /// The stairwell cell (D-256): in the chamber whose nearest floor cell lies farthest from the spawn, the
    /// floor cell that lies farthest. A tie takes the earlier chamber, and the earlier cell in scan order.
    /// </summary>
    /// <exception cref="ContextException">A chamber has no reachable floor cell, which the dig rules make impossible (D-112).</exception>
    private static Cell FarthestChamberCell(IReadOnlyList<Chamber> chambers, VoxelGrid grid, Reachability reach)
    {
        int farthestChamber = -1;
        int farthestNearest = -1;
        Cell stairwell = reach.Start;
        foreach (Chamber chamber in chambers)
        {
            int nearest = -1;
            int farthest = -1;
            Cell farthestCell = reach.Start;
            foreach (Column column in chamber.Footprint)
            {
                Cell cell = new(column.X, chamber.FloorRow, column.Z);
                if (!GridMoves.IsFloor(grid, cell) || !reach.IsReachable(cell))
                {
                    continue;
                }

                int distance = reach.Distance(cell);
                if (nearest < 0 || distance < nearest)
                {
                    nearest = distance;
                }

                if (distance > farthest)
                {
                    farthest = distance;
                    farthestCell = cell;
                }
            }

            if (nearest < 0)
            {
                ContextException error = new($"Chamber {chamber.Index} of kind '{chamber.Kind.Id}' has no floor cell that the spawn reaches, and the dig rules make that impossible (D-253).");
                error.AddContext("chamber", ((long)chamber.Index).ToString(CultureInfo.InvariantCulture));
                error.AddContext("chamberKind", chamber.Kind.Id);
                throw error;
            }

            if (nearest > farthestNearest)
            {
                farthestNearest = nearest;
                farthestChamber = chamber.Index;
                stairwell = farthestCell;
            }
        }

        if (farthestChamber < 0)
        {
            throw new ContextException("The floor has no chamber, and every floor holds at least one (D-167).");
        }

        return stairwell;
    }

    /// <summary>
    /// Confirms that the spawn reaches every floor cell of every tier. A tier takes one connected region of the
    /// chamber floor with a ramp up to it, and the detail pass keeps that ramp and its two ends clear, so a tier
    /// that no body reaches is a defect of the construction and never a floor that ships (D-112, D-348, T-2).
    /// </summary>
    /// <exception cref="ContextException">A tier holds a cell that is no floor cell, or that the spawn does not reach.</exception>
    private static void CheckTiers(IReadOnlyList<Chamber> chambers, VoxelGrid grid, Reachability reach)
    {
        foreach (Chamber chamber in chambers)
        {
            if (chamber.Tier is null)
            {
                continue;
            }

            foreach (Column column in chamber.Tier.Floor)
            {
                Cell cell = new(column.X, chamber.Tier.FloorRow, column.Z);
                if (GridMoves.IsFloor(grid, cell) && reach.IsReachable(cell))
                {
                    continue;
                }

                string shape = TierShapes.NameOf(chamber.Tier.Shape);
                ContextException error = new($"The tier of chamber {chamber.Index}, of the shape '{shape}', holds the cell ({cell.X}, {cell.Y}, {cell.Z}) that the spawn does not reach, and the ramp of the tier makes that impossible (D-348, D-391).");
                error.AddContext("chamber", ((long)chamber.Index).ToString(CultureInfo.InvariantCulture));
                error.AddContext("tierShape", shape);
                error.AddContext("tierX", ((long)cell.X).ToString(CultureInfo.InvariantCulture));
                error.AddContext("tierY", ((long)cell.Y).ToString(CultureInfo.InvariantCulture));
                error.AddContext("tierZ", ((long)cell.Z).ToString(CultureInfo.InvariantCulture));
                throw error;
            }
        }
    }

    /// <summary>
    /// Confirms that the spawn reaches the landing of every shaft: the floor under the top air row of the space below
    /// the hole. The dig proves two air rows over each landing before it carves, and the detail pass keeps them free,
    /// so an unreachable landing is a defect of the construction and never a floor that ships (D-112, F-101, T-2).
    /// </summary>
    /// <exception cref="ContextException">A shaft lands on a cell that is no floor cell, or that the spawn does not reach.</exception>
    private static void CheckShaftLandings(IReadOnlyList<Shaft> shafts, VoxelGrid grid, Reachability reach)
    {
        foreach (Shaft shaft in shafts)
        {
            int landingRow = shaft.LandingAirRow;
            while (landingRow >= 0 && !grid.IsSolid(shaft.Center.X, landingRow, shaft.Center.Z))
            {
                landingRow--;
            }

            Cell landing = new(shaft.Center.X, landingRow, shaft.Center.Z);
            bool onFloor = GridMoves.IsFloor(grid, landing);
            if (onFloor && reach.IsReachable(landing))
            {
                continue;
            }

            // The two causes need two messages. A pillar or a heap of rubble takes the landing away, and the cell
            // is then no floor cell. A landing that stands alone keeps its two air rows, and the search misses it.
            string cause = onFloor ? "the spawn does not reach that cell" : "that cell is no floor cell with two air rows over it";
            ContextException error = new($"The shaft of chamber {shaft.ChamberIndex} at the column ({shaft.Center.X}, {shaft.Center.Z}) lands at row {landingRow}, and {cause} (D-253, F-101).");
            error.AddContext("cause", onFloor ? "unreachable" : "noFloorCell");
            error.AddContext("chamber", ((long)shaft.ChamberIndex).ToString(CultureInfo.InvariantCulture));
            error.AddContext("shaftX", ((long)shaft.Center.X).ToString(CultureInfo.InvariantCulture));
            error.AddContext("shaftZ", ((long)shaft.Center.Z).ToString(CultureInfo.InvariantCulture));
            error.AddContext("landingRow", ((long)landingRow).ToString(CultureInfo.InvariantCulture));
            throw error;
        }
    }
}
