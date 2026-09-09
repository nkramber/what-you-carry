using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// Digs one floor from the run seed, the floor number, and the content set (D-252 to D-256). Floor n comes from
/// those three alone, so a replay digs the same floor from the record header (D-151, D-236).
/// </summary>
/// <remarks>
/// <para>
/// The template is the one whose depth range holds the floor number. The size of the grid comes from it, the
/// chamber kinds come from the budget draw, and the <see cref="DigPlan"/> digs the chambers, the tunnels, the
/// ramps, and the shafts. The spawn is the center of the anchor cell of the first chamber. The stairwell is the
/// floor cell with the longest walkable path from the spawn, in the chamber whose nearest floor cell lies
/// farthest (D-256).
/// </para>
/// <para>
/// The generator confirms its own construction: every chamber has a reachable floor cell, or the floor is an
/// error that names the seed, the floor, and the chamber (D-112, T-2). PR-9 exit test 1 asserts the same from
/// outside over thousands of seeds.
/// </para>
/// </remarks>
public static class FloorGenerator
{
    /// <summary>The dug floor.</summary>
    /// <exception cref="ContextException">The floor is below one, no template or two templates cover it, the content holds no chamber kind, or the dig fails. The error names the seed and the floor.</exception>
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
        IReadOnlyList<ChamberKind> kinds = ChamberBudget.Draw(rng, template, content.Chambers);

        VoxelGrid grid = new(template.SizeX, template.SizeY, template.SizeZ);
        DigPlan plan = new(rng, new DigCanvas(grid), kinds);
        plan.DigFirstChamber();
        plan.DigUntilComplete();
        plan.DigShafts();

        Chamber first = plan.Chambers[0];
        Cell spawnCell = new(first.Anchor.X, first.FloorRow, first.Anchor.Z);
        Reachability reach = Reachability.From(grid, spawnCell);
        Cell stairwell = FarthestChamberCell(plan.Chambers, grid, reach);
        Vector3 spawn = new(first.Anchor.X + 0.5f, first.FloorRow + 1.0f, first.Anchor.Z + 0.5f);
        return new FloorPlan(floor, template, grid, spawn, stairwell, plan.Chambers, plan.Shafts);
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
                if (!Reachability.IsFloor(grid, cell) || !reach.IsReachable(cell))
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
}
