using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.World;
using WhatYouCarry.Tools.NightGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The floor generator, the dig canvas, the chamber budget, the footprint, and the reachability search (D-159, D-165 to D-167, D-252 to D-256, D-341 to D-344, D-352; PR-9 exit tests 1 to 7, PR-63 exit tests 1 and 2).</summary>
[Trait("Category", SweepScope.SweepCategory)]
public sealed class ProcgenTests
{
    /// <summary>The environment variable that the night job sets to run the night seed counts (D-116).</summary>
    public const string NightVariable = "WYC_NIGHT_SWEEP";

    /// <summary>The environment variable that holds the seed list of the night reachability sweep, from <c>night-seeds</c> (D-564).</summary>
    public const string NightSeedsVariable = "WYC_NIGHT_SEEDS";

    /// <summary>The environment variable that names the failures file of the night, which takes the failure line of the sweep (D-567).</summary>
    public const string NightFailuresVariable = "WYC_NIGHT_FAILURES";

    /// <summary>The seeds of the reachability sweep on main (PR-9 exit test 1, D-277). A pull request runs one fifth (D-480).</summary>
    public const int ReachabilitySeeds = 5000;

    /// <summary>The seeds of the reachability sweep each night (D-116).</summary>
    public const int ReachabilitySeedsPerNight = 100000;

    /// <summary>The seeds of the other property sweeps on main. A pull request runs one fifth (D-480).</summary>
    public static readonly int PropertySeeds = SweepScope.Seeds(1000);

    /// <summary>The count of seeds of a sweep: the night count when the night variable is "1", or else the count of <see cref="SweepScope"/>.</summary>
    internal static int SweepSeeds(int onMain, int perNight)
    {
        return Environment.GetEnvironmentVariable(NightVariable) == "1" ? perNight : SweepScope.Seeds(onMain);
    }

    /// <summary>
    /// The seeds of the reachability sweep. Off the night, the count of <see cref="SweepSeeds"/> from seed 1. On the
    /// night, the seed list of <see cref="NightSeedsVariable"/>: the fixed range, the slice, and the extra and carried
    /// seeds (D-564). A night variable with no list gives the fixed range alone, for a night sweep by hand.
    /// </summary>
    /// <exception cref="InvalidOperationException">The list is malformed, or it holds a seed past the largest int.</exception>
    internal static List<int> ReachabilitySeedList(string? night, string? list)
    {
        List<int> seeds = [];
        if (night != "1" || list is null)
        {
            int count = night == "1" ? ReachabilitySeedsPerNight : SweepScope.Seeds(ReachabilitySeeds);
            for (int seed = 1; seed <= count; seed++)
            {
                seeds.Add(seed);
            }

            return seeds;
        }

        List<SeedRange> ranges = NightSeeds.TryParseList(list, out string error)
            ?? throw new InvalidOperationException($"The variable {NightSeedsVariable} is wrong: {error}.");
        foreach (SeedRange range in ranges)
        {
            if (range.To >= int.MaxValue)
            {
                throw new InvalidOperationException($"The variable {NightSeedsVariable} holds the range {range}, past the largest seed of the sweep, {int.MaxValue}.");
            }

            for (int seed = (int)range.From; seed <= (int)range.To; seed++)
            {
                seeds.Add(seed);
            }
        }

        return seeds;
    }

    /// <summary>The floor of a sweep seed: one to fifteen in turn, so every band takes one third of the seeds.</summary>
    private static int FloorOf(int seed)
    {
        return 1 + (seed % 15);
    }

    private static FloorPlan Plan(int seed)
    {
        return FloorGenerator.Generate((ulong)seed, FloorOf(seed), TestWorld.Content);
    }

    /// <summary>The floor cell under the spawn point.</summary>
    private static Cell SpawnCell(FloorPlan plan)
    {
        return new Cell((int)MathF.Floor(plan.Spawn.X), (int)MathF.Floor(plan.Spawn.Y) - 1, (int)MathF.Floor(plan.Spawn.Z));
    }

    /// <summary>The count of moves to the nearest reachable floor cell of a chamber, or minus one.</summary>
    private static int NearestDistance(Chamber chamber, VoxelGrid grid, Reachability reach)
    {
        int nearest = -1;
        foreach (Column column in chamber.Footprint)
        {
            Cell cell = new(column.X, chamber.FloorRow, column.Z);
            if (GridMoves.IsFloor(grid, cell) && reach.IsReachable(cell))
            {
                int distance = reach.Distance(cell);
                if (nearest < 0 || distance < nearest)
                {
                    nearest = distance;
                }
            }
        }

        return nearest;
    }

    /// <summary>Answers whether an air cell sits inside a three-wide, three-high window of air along X or along Z (D-166).</summary>
    private static bool HasCrossSection(VoxelGrid grid, int x, int y, int z)
    {
        for (int lowY = y - 2; lowY <= y; lowY++)
        {
            for (int lowX = x - 2; lowX <= x; lowX++)
            {
                if (!grid.IsAnySolid(lowX, lowX + 2, lowY, lowY + 2, z, z))
                {
                    return true;
                }
            }

            for (int lowZ = z - 2; lowZ <= z; lowZ++)
            {
                if (!grid.IsAnySolid(x, x, lowY, lowY + 2, lowZ, lowZ + 2))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// The one sweep of five thousand seeds per PR, and one hundred thousand and a slice each night, that PR-9 exit
    /// test 1 and PR-59 exit test 1 both read (D-116, D-564). It digs each floor once and keeps one failure list per
    /// test, so the two tests cost one dig per seed. The seeds cover every band in turn. On the night, it writes the
    /// failure line of the sweep to the failures file, with each seed of a chamber or detail failure (D-567).
    /// </summary>
    private static readonly Lazy<SweepReport> ReachabilitySweep = new(RunReachabilitySweep);

    private sealed record SweepReport(
        IReadOnlyList<string> ChamberFailures,
        IReadOnlyList<string> DetailFailures,
        IReadOnlyList<string> RampFailures,
        IReadOnlyList<string> TierFailures,
        int Pillars,
        int Pools,
        int Collapses,
        IReadOnlyDictionary<int, int> RampRuns,
        IReadOnlyDictionary<TierShape, int> TierShapes,
        IReadOnlyDictionary<string, TierTally> TiersByKind,
        int Floors,
        int FloorsWithATier,
        IReadOnlyDictionary<int, int> RampWidths,
        int Shafts,
        int FloorsWithAShaft,
        IReadOnlyList<string> ShaftFailures);

    /// <summary>The chambers of one kind over a sweep: how many the sweep dug, how many drew a tier, and how many took one (D-350, D-391).</summary>
    private sealed class TierTally
    {
        public int Chambers { get; set; }

        public int Drawn { get; set; }

        public int Built { get; set; }
    }

    private static SweepReport RunReachabilitySweep()
    {
        List<int> seeds = ReachabilitySeedList(Environment.GetEnvironmentVariable(NightVariable), Environment.GetEnvironmentVariable(NightSeedsVariable));
        return Sweep(seeds, TestWorld.Content, Environment.GetEnvironmentVariable(NightFailuresVariable));
    }

    /// <summary>
    /// Digs one floor for each seed with one content set, and reads every check of the sweep. With a failures file,
    /// it appends the failure line of the sweep to the file (D-567).
    /// </summary>
    private static SweepReport Sweep(IReadOnlyList<int> seeds, ContentSet content, string? failuresFile)
    {
        List<ulong> failedSeeds = [];
        List<string> chamberFailures = [];
        List<string> detailFailures = [];
        List<string> rampFailures = [];
        List<string> tierFailures = [];
        int pillars = 0;
        int pools = 0;
        int collapses = 0;
        Dictionary<int, int> rampRuns = [];
        Dictionary<TierShape, int> tierShapes = [];
        Dictionary<string, TierTally> tiersByKind = [];
        Dictionary<int, int> rampWidths = [];
        List<string> shaftFailures = [];
        int shaftCount = 0;
        int floorsWithAShaft = 0;
        int floorsWithATier = 0;
        foreach (int seed in seeds)
        {
            int failuresBefore = chamberFailures.Count + detailFailures.Count;
            FloorPlan plan;
            try
            {
                plan = FloorGenerator.Generate((ulong)seed, FloorOf(seed), content);
            }
            catch (ContextException error)
            {
                // A dig that throws fails its seed. The sweep names the seed and reads the next one, so the failure line
                // and the night record carry the seed (D-565, D-567, F-117).
                chamberFailures.Add($"Seed {seed}, floor {FloorOf(seed)}: the dig threw. {error.Message}");
                failedSeeds.Add((ulong)seed);
                continue;
            }

            string context = $"Seed {seed}, floor {plan.Floor}";
            PlayerBody body = new(plan.Grid, plan.Spawn);
            if (!body.IsOnGround())
            {
                chamberFailures.Add($"{context}: the body at the spawn {plan.Spawn} is not on the ground.");
            }

            Reachability reach = Reachability.From(plan.Grid, SpawnCell(plan));
            if (plan.Chambers.Count == 0)
            {
                chamberFailures.Add($"{context}: the floor has no chamber.");
            }

            foreach (Chamber chamber in plan.Chambers)
            {
                int floorCells = 0;
                foreach (Column column in chamber.Footprint)
                {
                    Cell cell = new(column.X, chamber.FloorRow, column.Z);
                    if (!plan.Grid.IsSolid(cell.X, cell.Y, cell.Z) || plan.Detail.Pillars.Contains(cell))
                    {
                        continue;
                    }

                    // A tier and its ramp stand on the chamber floor, so those columns hold no chamber floor cell.
                    // The tier check below reads the floor of the tier itself (D-348, D-349).
                    if (chamber.LowestAirRow(column) != chamber.FloorRow + 1)
                    {
                        continue;
                    }

                    floorCells++;
                    if (!GridMoves.IsFloor(plan.Grid, cell))
                    {
                        chamberFailures.Add($"{context}: the chamber {chamber.Index} cell {cell} has rock over it.");
                    }
                    else if (!reach.IsReachable(cell))
                    {
                        chamberFailures.Add($"{context}: the chamber {chamber.Index} cell {cell} is not reachable from the spawn {reach.Start}.");
                    }
                }

                if (floorCells == 0)
                {
                    chamberFailures.Add($"{context}: the chamber {chamber.Index} has no floor cell left.");
                }
            }

            floorsWithAShaft += plan.Shafts.Count > 0 ? 1 : 0;
            foreach (Shaft shaft in plan.Shafts)
            {
                shaftCount++;
                Column landing = shaft.Center;
                int floorRow = shaft.LandingAirRow;
                while (!plan.Grid.IsSolid(landing.X, floorRow, landing.Z))
                {
                    floorRow--;
                }

                if (!reach.IsReachable(new Cell(landing.X, floorRow, landing.Z)))
                {
                    chamberFailures.Add($"{context}: the shaft at {shaft.Center} lands on an unreachable floor at row {floorRow}.");
                }

                // F-101: a pillar in a column of the hole fills the landing, and no path leads to the top of it.
                foreach (Cell pillar in plan.Detail.Pillars)
                {
                    int alongX = pillar.X - shaft.Center.X;
                    int alongZ = pillar.Z - shaft.Center.Z;
                    if (alongX >= -DigPlan.ShaftRadius && alongX <= DigPlan.ShaftRadius && alongZ >= -DigPlan.ShaftRadius && alongZ <= DigPlan.ShaftRadius)
                    {
                        shaftFailures.Add($"{context}: the pillar at {pillar} stands in the hole of the shaft at {shaft.Center}.");
                    }
                }
            }

            CheckRamps(plan, context, reach, rampFailures, rampRuns);
            floorsWithATier += CheckTiers(plan, context, reach, tierFailures, tierShapes, tiersByKind, rampWidths) ? 1 : 0;
            pillars += plan.Detail.Pillars.Count;
            pools += plan.Detail.Pools.Count;
            collapses += plan.Detail.Collapses.Count;
            BlockId pillarBlock = DetailPass.PillarBlock(plan.Template.Band);
            foreach (Cell pillar in plan.Detail.Pillars)
            {
                if (plan.Grid.Get(pillar.X, pillar.Y + 1, pillar.Z) != pillarBlock || !plan.Grid.IsSolid(pillar.X, pillar.Y, pillar.Z))
                {
                    detailFailures.Add($"{context}: the pillar at {pillar} is not a {pillarBlock} column on rock.");
                }
            }

            foreach (Cell pool in plan.Detail.Pools)
            {
                if (plan.Grid.Get(pool.X, pool.Y, pool.Z) != BlockId.StillWater || !plan.Grid.IsSolid(pool.X, pool.Y - 1, pool.Z))
                {
                    detailFailures.Add($"{context}: the pool cell {pool} is not still water over rock.");
                }
            }

            foreach (Cell rubble in plan.Detail.Collapses)
            {
                if (plan.Grid.Get(rubble.X, rubble.Y, rubble.Z) != BlockId.Rubble)
                {
                    detailFailures.Add($"{context}: the collapse cell {rubble} is not rubble.");
                }
            }

            if (chamberFailures.Count + detailFailures.Count > failuresBefore)
            {
                failedSeeds.Add((ulong)seed);
            }
        }

        if (failuresFile is not null)
        {
            File.AppendAllText(failuresFile, NightSeeds.FailureLine(NightSeeds.ReachabilitySweep, failedSeeds), new UTF8Encoding(false));
        }

        return new SweepReport(chamberFailures, detailFailures, rampFailures, tierFailures, pillars, pools, collapses, rampRuns, tierShapes, tiersByKind, seeds.Count, floorsWithATier, rampWidths, shaftCount, floorsWithAShaft, shaftFailures);
    }

    /// <summary>
    /// Reads every ramp of one floor against the slopes of its template (PR-66 exit test 3). Each ramp cell holds a
    /// ramp block of the run of the ramp, the places rise from the low end to the high end, and the grid holds no
    /// ramp block outside a ramp of the plan.
    /// </summary>
    private static void CheckRamps(FloorPlan plan, string context, Reachability reach, List<string> failures, Dictionary<int, int> runs)
    {
        int cells = 0;
        foreach (DugRamp ramp in plan.Ramps)
        {
            cells += ramp.Cells.Count;
            runs[ramp.Run] = runs.TryGetValue(ramp.Run, out int seen) ? seen + 1 : 1;
            if (!plan.Template.RampSlopeRuns.Contains(ramp.Run))
            {
                failures.Add($"{context}: the ramp at {ramp.LowEnd} has the run {ramp.Run}, and the template '{plan.Template.Id}' lists {string.Join(", ", plan.Template.RampSlopeRuns)}.");
                continue;
            }

            if (ramp.Cells.Count % (ramp.Run * ramp.Rise) != 0)
            {
                failures.Add($"{context}: the ramp at {ramp.LowEnd} holds {ramp.Cells.Count} cells, and a ramp of the run {ramp.Run} and the rise {ramp.Rise} holds a multiple of {ramp.Run * ramp.Rise}.");
            }

            foreach (Cell cell in ramp.Cells)
            {
                if (!plan.Grid.TryGetRamp(cell.X, cell.Y, cell.Z, out Ramp block))
                {
                    failures.Add($"{context}: the ramp cell {cell} holds the block {(int)plan.Grid.Get(cell.X, cell.Y, cell.Z)}, which is no ramp.");
                    continue;
                }

                if (block.Run != ramp.Run)
                {
                    failures.Add($"{context}: the ramp cell {cell} has the run {block.Run}, and its ramp has the run {ramp.Run}.");
                }

                if (!GridMoves.IsFloor(plan.Grid, cell))
                {
                    failures.Add($"{context}: the ramp cell {cell} has no two open cells over it.");
                }
                else if (!reach.IsReachable(cell))
                {
                    failures.Add($"{context}: the ramp cell {cell} is not reachable from the spawn.");
                }
            }

            if (GridMoves.IsFloor(plan.Grid, ramp.LowEnd) && reach.IsReachable(ramp.LowEnd) && !reach.IsReachable(ramp.HighEnd))
            {
                failures.Add($"{context}: the ramp from {ramp.LowEnd} reaches the spawn, and its high end {ramp.HighEnd} does not.");
            }
        }

        int blocks = 0;
        for (int y = 0; y < plan.Grid.SizeY; y++)
        {
            for (int z = 0; z < plan.Grid.SizeZ; z++)
            {
                for (int x = 0; x < plan.Grid.SizeX; x++)
                {
                    blocks += Ramp.IsRamp(plan.Grid.Get(x, y, z)) ? 1 : 0;
                }
            }
        }

        if (blocks != cells)
        {
            failures.Add($"{context}: the grid holds {blocks} ramp blocks, and the plan holds {cells} ramp cells.");
        }
    }

    /// <summary>
    /// Reads every tier of one floor (PR-66 exit tests 4 and 5): its floor stands two blocks over the chamber floor,
    /// its ramp joins the two, every tier cell is a reachable floor cell, and the tally holds the draw of each kind.
    /// Gives true when the floor holds one tier or more (D-392).
    /// </summary>
    private static bool CheckTiers(FloorPlan plan, string context, Reachability reach, List<string> failures, Dictionary<TierShape, int> shapes, Dictionary<string, TierTally> byKind, Dictionary<int, int> widths)
    {
        int built = 0;
        foreach (Chamber chamber in plan.Chambers)
        {
            if (!byKind.TryGetValue(chamber.Kind.Id, out TierTally? tally))
            {
                tally = new TierTally();
                byKind[chamber.Kind.Id] = tally;
            }

            tally.Chambers++;
            tally.Drawn += chamber.TierDrawn ? 1 : 0;
            if (chamber.TierDrawn && chamber.Kind.TierChance == 0)
            {
                failures.Add($"{context}: the chamber {chamber.Index} of kind '{chamber.Kind.Id}' drew a tier, and the kind has the tier chance 0.");
            }

            ChamberTier? tier = chamber.Tier;
            if (tier is null)
            {
                continue;
            }

            tally.Built++;
            built++;
            int width = tier.Ramp.Cells.Count / (tier.Ramp.Run * tier.Ramp.Rise);
            widths[width] = widths.TryGetValue(width, out int seenWidth) ? seenWidth + 1 : 1;
            if (chamber.Kind.TierChance == 0)
            {
                failures.Add($"{context}: the chamber {chamber.Index} of kind '{chamber.Kind.Id}' holds a tier, and the kind has the tier chance 0.");
            }

            shapes[tier.Shape] = shapes.TryGetValue(tier.Shape, out int seen) ? seen + 1 : 1;
            if (tier.FloorRow != chamber.FloorRow + ChamberTier.Rise)
            {
                failures.Add($"{context}: the tier of chamber {chamber.Index} stands at row {tier.FloorRow}, and its chamber floor is row {chamber.FloorRow}.");
            }

            if (tier.Ramp.LowEnd.Y != chamber.FloorRow || tier.Ramp.HighEnd.Y != tier.FloorRow || tier.Ramp.Rise != ChamberTier.Rise)
            {
                failures.Add($"{context}: the ramp of the tier of chamber {chamber.Index} runs from {tier.Ramp.LowEnd} to {tier.Ramp.HighEnd} with the rise {tier.Ramp.Rise}.");
            }

            foreach (Column column in tier.Floor)
            {
                Cell cell = new(column.X, tier.FloorRow, column.Z);
                if (!GridMoves.IsFloor(plan.Grid, cell))
                {
                    failures.Add($"{context}: the tier cell {cell} of chamber {chamber.Index} is no floor cell.");
                }
                else if (!reach.IsReachable(cell))
                {
                    failures.Add($"{context}: the tier cell {cell} of chamber {chamber.Index} is not reachable from the spawn.");
                }
            }
        }

        return built > 0;
    }

    /// <summary>
    /// F-117. A seed whose dig throws is a failure of the sweep: the report names it, the sweep reads every later
    /// seed, and the failure line carries it. The old sweep let the exception leave, so it wrote no failure line, and
    /// the night record lost the seed. Here the deep template stops at floor 14, so each seed of floor 15 throws.
    /// </summary>
    [Fact]
    public void TheSweepNamesASeedWhoseDigThrows()
    {
        List<FloorTemplate> floors = [];
        foreach (FloorTemplate template in TestWorld.Content.Floors)
        {
            floors.Add(template.MaxDepth == 15 ? template with { MaxDepth = 14 } : template);
        }

        ContentSet shallow = TestWorld.Content with { Floors = floors };
        List<int> seeds = [.. Enumerable.Range(100001, 20)];
        List<ulong> deepest = [.. seeds.Where(seed => FloorOf(seed) == 15).Select(seed => (ulong)seed)];
        Assert.Equal([100004UL, 100019UL], deepest);

        string failures = Path.Combine(Path.GetTempPath(), "wyc-sweep-failures-" + Guid.NewGuid().ToString("N") + ".txt");
        try
        {
            SweepReport report = Sweep(seeds, shallow, failures);

            Assert.Equal(seeds.Count, report.Floors);
            Assert.Equal(NightSeeds.FailureLine(NightSeeds.ReachabilitySweep, deepest), File.ReadAllText(failures));
            Assert.Equal(2, report.ChamberFailures.Count(failure => failure.Contains("the dig threw", StringComparison.Ordinal)));
            Assert.Contains(report.ChamberFailures, failure => failure.StartsWith("Seed 100019, floor 15: the dig threw.", StringComparison.Ordinal));
        }
        finally
        {
            File.Delete(failures);
        }
    }

    /// <summary>
    /// PR-9 exit test 1. Over five thousand seeds per PR and one hundred thousand each night, across every band,
    /// every floor cell of every chamber that still has its floor is reachable from the spawn, the spawn holds the
    /// player box, and every shaft lands on a reachable floor. A failure names its seed and its floor (D-66, D-116).
    /// </summary>
    [Fact]
    public void EveryChamberReachable()
    {
        SweepReport report = ReachabilitySweep.Value;
        Assert.True(report.ChamberFailures.Count == 0, string.Join("\n", report.ChamberFailures.Take(10)));
    }

    /// <summary>
    /// PR-59 exit test 1. Over the same seeds, the reachability of PR-9 holds with the detail on, every pillar
    /// stands on rock inside its chamber, every pool is still water over rock, and every rubble cell is rubble.
    /// The sweep places some of each. A failure names its seed and its floor (D-66, D-116).
    /// </summary>
    [Fact]
    public void DetailKeepsEveryChamberReachable()
    {
        SweepReport report = ReachabilitySweep.Value;
        Assert.True(report.ChamberFailures.Count == 0, string.Join("\n", report.ChamberFailures.Take(10)));
        Assert.True(report.DetailFailures.Count == 0, string.Join("\n", report.DetailFailures.Take(10)));
        Assert.True(report.Pillars > 0 && report.Pools > 0 && report.Collapses > 0, $"The sweep placed {report.Pillars} pillars, {report.Pools} pools, and {report.Collapses} rubble cells.");
    }

    /// <summary>
    /// PR-66 exit test 3. Over the sweep, every ramp has a slope that its floor template lists, every cell of it
    /// holds a ramp block of that run with two open cells over it, and the grid holds no ramp block outside a ramp
    /// of the plan. Each of the three slopes of D-346 appears.
    /// </summary>
    [Fact]
    public void RampsUseTheTemplateSlopes()
    {
        SweepReport report = ReachabilitySweep.Value;
        Assert.True(report.RampFailures.Count == 0, string.Join("\n", report.RampFailures.Take(10)));
        foreach (int run in new[] { Ramp.SteepestRun, 3, Ramp.ShallowestRun })
        {
            Assert.True(report.RampRuns.TryGetValue(run, out int count) && count > 0, $"The sweep dug no ramp of the run {run}. It dug {Tally(report.RampRuns)}.");
        }
    }

    /// <summary>
    /// PR-66 exit test 4. Over the sweep, the share of the chambers of each kind that draw a tier lands near the
    /// tier chance of that kind, and no chamber of a kind of chance 0 draws one (D-350). A chamber that draws a
    /// tier and fits no shape holds none, so the share of the chambers that take one is lower (D-391).
    /// </summary>
    [Fact]
    public void TierChanceMatchesTheKind()
    {
        SweepReport report = ReachabilitySweep.Value;
        Assert.True(report.TierFailures.Count == 0, string.Join("\n", report.TierFailures.Take(10)));
        foreach (ChamberKind kind in TestWorld.Content.Chambers)
        {
            if (!report.TiersByKind.TryGetValue(kind.Id, out TierTally? tally) || tally.Chambers < TierRateChambers)
            {
                continue;
            }

            int drawn = 100 * tally.Drawn / tally.Chambers;
            int low = kind.TierChance - TierRateTolerance;
            int high = kind.TierChance + TierRateTolerance;
            Assert.True(drawn >= low && drawn <= high, $"The kind '{kind.Id}' has the tier chance {kind.TierChance}, and {tally.Drawn} of {tally.Chambers} chambers drew a tier, which is {drawn} percent.");
            if (kind.TierChance > 0)
            {
                Assert.True(tally.Built > 0, $"The kind '{kind.Id}' has the tier chance {kind.TierChance}, and none of its {tally.Chambers} chambers took a tier.");
            }
        }
    }

    /// <summary>
    /// PR-66 exit test 5. Over the sweep, every tier floor stands two blocks over its chamber floor, a ramp joins
    /// the two, and every tier cell is a floor cell that the spawn reaches (D-348, D-349). Each of the four shapes
    /// of D-388 appears.
    /// </summary>
    [Fact]
    public void TierIsTwoBlocksUp()
    {
        SweepReport report = ReachabilitySweep.Value;
        Assert.True(report.TierFailures.Count == 0, string.Join("\n", report.TierFailures.Take(10)));
        foreach (TierShape shape in new[] { TierShape.Rectangle, TierShape.CutLine, TierShape.RaisedBox, TierShape.Island })
        {
            Assert.True(report.TierShapes.TryGetValue(shape, out int count) && count > 0, $"The sweep built no tier of the shape {shape}. It built {Tally(report.TierShapes)}.");
        }
    }

    /// <summary>
    /// PR-66 exit test 6 (D-392, D-393). Over the sweep, a floor holds a tier far more often than the tier chances
    /// alone give, because a floor with no tier from the draws takes one in the first chamber of a tiered kind that
    /// fits. Every tier ramp is 3 or 2 cells wide, and the widest that fits comes first.
    /// </summary>
    /// <remarks>
    /// The measurement of 2026-09-19 over 1000 floors gives 524 tiers, 51 percent of floors with one, and the widths
    /// 329 of 3 cells and 195 of 2. The draws alone gave 184 tiers and 18 percent of floors. The floor of the test
    /// leaves room under the measured rate, because the rate reads the shapes of every chamber of every band.
    /// </remarks>
    [Fact]
    public void EveryFloorTakesATierWhenOneFits()
    {
        SweepReport report = ReachabilitySweep.Value;
        Assert.True(report.TierFailures.Count == 0, string.Join("\n", report.TierFailures.Take(10)));
        int share = 100 * report.FloorsWithATier / report.Floors;
        Assert.True(share >= FloorsWithATierFloor, $"{report.FloorsWithATier} of {report.Floors} floors hold a tier, which is {share} percent, and the test reads {FloorsWithATierFloor} percent.");
        foreach (KeyValuePair<int, int> width in report.RampWidths)
        {
            Assert.True(width.Key <= TierPlan.WidestRamp && width.Key >= TierPlan.NarrowestRamp, $"The sweep built {width.Value} tier ramps of {width.Key} cells across, and a tier ramp is {TierPlan.NarrowestRamp} to {TierPlan.WidestRamp} cells (D-393).");
        }

        Assert.True(report.RampWidths.TryGetValue(TierPlan.WidestRamp, out int widest) && widest > 0, $"The sweep built no tier ramp of {TierPlan.WidestRamp} cells across. It built {Tally(report.RampWidths)}.");
    }

    /// <summary>
    /// PR-66 exit test 7 (F-103, D-394). Over the sweep, the share of floors with a shaft holds over the floor that
    /// D-394 sets. The dig routes a drift under a chamber, so a shaft of that chamber has a landing.
    /// </summary>
    /// <remarks>
    /// The measurement of 2026-09-20 over 3000 floors gives 76 shafts and 2.5 percent of floors with one. The base
    /// `27db615` gives 9 shafts over 20000 floors, which is 0.045 percent. The floor of the test leaves room under
    /// the measured share.
    /// </remarks>
    [Fact]
    public void EveryFloorTakesAShaftWhenOneFits()
    {
        SweepReport report = ReachabilitySweep.Value;
        Assert.True(report.ShaftFailures.Count == 0, string.Join("\n", report.ShaftFailures.Take(10)));
        int share = 100 * report.FloorsWithAShaft / report.Floors;
        Assert.True(share >= FloorsWithAShaftFloor, $"{report.FloorsWithAShaft} of {report.Floors} floors hold a shaft, which is {share} percent, and the test reads {FloorsWithAShaftFloor} percent.");
    }

    /// <summary>The lowest share of floors with a shaft that the sweep accepts, in percent (D-394).</summary>
    private const int FloorsWithAShaftFloor = 2;

    /// <summary>The lowest share of floors with a tier that the sweep accepts, in percent (D-392).</summary>
    private const int FloorsWithATierFloor = 40;

    /// <summary>The count of chambers of one kind that a rate test needs before it reads the rate.</summary>
    private const int TierRateChambers = 200;

    /// <summary>The tolerance of the tier rate of one kind, in points of percent.</summary>
    private const int TierRateTolerance = 6;

    /// <summary>One tally as text, for the message of a failure.</summary>
    private static string Tally<TKey>(IReadOnlyDictionary<TKey, int> counts)
    {
        List<string> parts = [];
        foreach (KeyValuePair<TKey, int> pair in counts.OrderBy(pair => pair.Key?.ToString(), StringComparer.Ordinal))
        {
            parts.Add($"{pair.Key}: {pair.Value}");
        }

        return parts.Count == 0 ? "none" : string.Join(", ", parts);
    }

    /// <summary>Answers whether a body stands on the cell and the cell holds no ramp: a one-block step up to it is a step and not a walk (D-345, D-347).</summary>
    private static bool IsPlainFloor(VoxelGrid grid, Cell cell)
    {
        return GridMoves.IsFloor(grid, cell) && !grid.TryGetRamp(cell.X, cell.Y, cell.Z, out _);
    }

    /// <summary>The grid of one floor after the dig and the tiers, and before the shafts and the detail pass.</summary>
    private static VoxelGrid DugGrid(int seed, int floor)
    {
        FloorTemplate template = FloorGenerator.TemplateFor(floor, TestWorld.Content);
        Rng rng = Rng.ForStream((ulong)seed, RngStream.Procgen, floor);
        for (int dig = 0; dig < FloorGenerator.MaxDigs; dig++)
        {
            DigCanvas canvas = new(new VoxelGrid(template.SizeX, template.SizeY, template.SizeZ));
            DigPlan plan = new(rng, canvas, template, ChamberBudget.Draw(rng, template, TestWorld.Content.Chambers));
            plan.DigFirstChamber();
            if (plan.TryDigUntilComplete(out _))
            {
                plan.BuildTiers();
                return canvas.Grid;
            }
        }

        throw new ContextException($"No dig of seed {seed}, floor {floor} dug every chamber.");
    }

    /// <summary>
    /// PR-68 exit test 1 (F-101). No pillar stands in a column of the hole of a shaft, over every shaft of the
    /// sweep. A pillar there fills the landing of the shaft, so a body that drops through it stands on the top of
    /// the pillar and no path leads back (D-253).
    /// </summary>
    /// <remarks>
    /// The night of 2026-09-15 found the case at seed 79146, floor 7. The dig of PR-66 changed every floor, and
    /// that seed digs no shaft now, so the test reads the rule over the shafts of the sweep in place of that one
    /// floor. The measurement of 2026-09-20 finds about one shaft in 3300 floors, so the sweep of main reads few,
    /// the sweep of a pull request reads fewer (D-480), and the sweep of a night reads about thirty (D-116).
    /// </remarks>
    [Fact]
    public void NoPillarStandsInTheHoleOfAShaft()
    {
        Assert.Empty(Plan(79146).Shafts);

        SweepReport report = ReachabilitySweep.Value;
        Assert.True(report.ShaftFailures.Count == 0, string.Join("\n", report.ShaftFailures.Take(10)));
        Assert.True(report.ChamberFailures.Count == 0, string.Join("\n", report.ChamberFailures.Take(10)));
    }

    /// <summary>The count of cells that a three by three window reaches past its cell on each axis.</summary>
    private const int WindowReach = 2;

    /// <summary>The array index of one cell: x fastest, then z, then y, as the grid stores it.</summary>
    private static int CellIndex(VoxelGrid grid, int x, int y, int z)
    {
        return x + (grid.SizeX * (z + (grid.SizeZ * y)));
    }

    /// <summary>Marks one cell as outside the cross-section check. A cell past the grid needs no mark.</summary>
    private static void MarkOutside(bool[] outside, VoxelGrid grid, int x, int y, int z)
    {
        if (grid.Contains(x, y, z))
        {
            outside[x + (grid.SizeX * (z + (grid.SizeZ * y)))] = true;
        }
    }

    /// <summary>PR-59 exit test 2. A floor of each band holds the blocks of its band and none of another band. Rubble and raw stone belong to every band (D-210, D-259).</summary>
    [Fact]
    public void EveryBandUsesItsBlocks()
    {
        for (int seed = 1; seed <= 100; seed++)
        {
            foreach (FloorTemplate template in TestWorld.Content.Floors)
            {
                FloorPlan plan = FloorGenerator.Generate((ulong)seed, (int)template.MinDepth, TestWorld.Content);
                int[] counts = new int[Ramp.LastId + 1];
                for (int y = 0; y < plan.Grid.SizeY; y++)
                {
                    for (int z = 0; z < plan.Grid.SizeZ; z++)
                    {
                        for (int x = 0; x < plan.Grid.SizeX; x++)
                        {
                            counts[(int)plan.Grid.Get(x, y, z)]++;
                        }
                    }
                }

                string context = $"Seed {seed}, band '{template.Band}'";
                bool working = template.Band == DetailPass.WorkingMine;
                bool older = template.Band == DetailPass.OlderWorkings;
                bool deep = template.Band == DetailPass.Deep;
                Assert.True(working || older || deep, $"{context}: the band is not one of D-210.");
                Assert.True((counts[(int)BlockId.TimberBeam] > 0) == working, $"{context}: {counts[(int)BlockId.TimberBeam]} timber beams.");
                Assert.True((counts[(int)BlockId.Plank] > 0) == working, $"{context}: {counts[(int)BlockId.Plank]} planks.");
                Assert.True((counts[(int)BlockId.HewnStone] > 0) == older, $"{context}: {counts[(int)BlockId.HewnStone]} hewn stone.");
                Assert.True((counts[(int)BlockId.StillWater] > 0) == older || (older && plan.Detail.Pools.Count == 0), $"{context}: {counts[(int)BlockId.StillWater]} water.");
                Assert.True((counts[(int)BlockId.OreVein] > 0) == deep, $"{context}: {counts[(int)BlockId.OreVein]} ore veins.");
            }
        }
    }

    /// <summary>
    /// PR-59 exit test 4. One seed and one floor give one grid with the detail on, twice over, and the bit-identity
    /// sweep folds a floor of each band, so the three platforms assert the same.
    /// </summary>
    [Fact]
    public void DetailIsDeterministic()
    {
        for (int seed = 1; seed <= 30; seed++)
        {
            FloorPlan first = Plan(seed);
            FloorPlan second = Plan(seed);
            Assert.True(GridHash(first.Grid).Value == GridHash(second.Grid).Value, $"Seed {seed}, floor {first.Floor}: two digs with the detail on give two grids.");
            Assert.Equal(first.Detail.Pools.Count, second.Detail.Pools.Count);
            Assert.Equal(first.Detail.Pillars.Count, second.Detail.Pillars.Count);
            Assert.Equal(first.Detail.Collapses.Count, second.Detail.Collapses.Count);
            Assert.Equal(first.Stairwell, second.Stairwell);
        }

        ContentSet sweep = WhatYouCarry.Tools.BitIdentity.BitIdentitySweep.SweepContent();
        Assert.Equal(3, sweep.Floors.Count);
    }

    /// <summary>A template of a band that D-210 does not name is an error that names the band (T-2).</summary>
    [Fact]
    public void AnUnknownBandIsAnError()
    {
        FloorTemplate odd = FloorGenerator.TemplateFor(1, TestWorld.Content) with { Id = "odd", Band = "sunlit-meadow" };
        List<FloorTemplate> floors = [odd];
        ContentSet content = TestWorld.Content with { Floors = floors };
        ContextException error = Assert.Throws<ContextException>(() => FloorGenerator.Generate(1UL, 1, content));
        Assert.Contains("band=sunlit-meadow", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A template that no validator read, with an even tunnel width, stops the dig with an error that names the width, and never digs a narrower tunnel (D-352, T-2).</summary>
    [Fact]
    public void AnEvenTunnelWidthStopsThePlan()
    {
        FloorTemplate even = FloorGenerator.TemplateFor(1, TestWorld.Content) with { Id = "even", DriftWidth = 6 };
        List<FloorTemplate> floors = [even];
        ContentSet content = TestWorld.Content with { Floors = floors };
        ContextException error = Assert.Throws<ContextException>(() => FloorGenerator.Generate(1UL, 1, content));
        Assert.Contains("driftWidth=6", error.Message, StringComparison.Ordinal);
        Assert.Contains("floorTemplate=even", error.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-9 exit test 6, as D-343 revises it. Floors 1 to 15 dig one grid of 64 by 20 by 64 for each seed, and the grid takes the size of its template.</summary>
    [Fact]
    public void EveryBandHasOneFloorSize()
    {
        for (int seed = 1; seed <= 4; seed++)
        {
            for (int floor = 1; floor <= 15; floor++)
            {
                FloorPlan plan = FloorGenerator.Generate((ulong)seed, floor, TestWorld.Content);
                string context = $"Seed {seed}, floor {floor}, template '{plan.Template.Id}'";
                Assert.True(plan.Grid.SizeX == 64 && plan.Grid.SizeY == 20 && plan.Grid.SizeZ == 64, $"{context}: the grid is {plan.Grid.SizeX} by {plan.Grid.SizeY} by {plan.Grid.SizeZ}.");
                Assert.True(plan.Grid.SizeX == plan.Template.SizeX && plan.Grid.SizeY == plan.Template.SizeY && plan.Grid.SizeZ == plan.Template.SizeZ, $"{context}: the grid does not take the size of its template.");
            }
        }
    }

    /// <summary>
    /// PR-9 exit test 7. One seed and one floor give one grid, one spawn, and one stairwell, twice over. Another
    /// seed or another floor gives another grid. The bit-identity sweep folds a dug floor, so the three platforms
    /// assert the same.
    /// </summary>
    [Fact]
    public void GenerationIsDeterministic()
    {
        for (int seed = 1; seed <= 50; seed++)
        {
            FloorPlan first = Plan(seed);
            FloorPlan second = Plan(seed);
            Assert.True(GridHash(first.Grid).Value == GridHash(second.Grid).Value, $"Seed {seed}, floor {first.Floor}: two digs give two grids.");
            Assert.Equal(first.Spawn, second.Spawn);
            Assert.Equal(first.Stairwell, second.Stairwell);
            Assert.Equal(first.Chambers.Count, second.Chambers.Count);
            Assert.Equal(first.Shafts.Count, second.Shafts.Count);

            FloorPlan otherSeed = FloorGenerator.Generate((ulong)seed + 1000000UL, first.Floor, TestWorld.Content);
            Assert.True(GridHash(first.Grid).Value != GridHash(otherSeed.Grid).Value, $"Seed {seed}, floor {first.Floor}: another seed gives the same grid.");

            FloorPlan otherFloor = FloorGenerator.Generate((ulong)seed, first.Floor == 15 ? 14 : first.Floor + 1, TestWorld.Content);
            Assert.True(GridHash(first.Grid).Value != GridHash(otherFloor.Grid).Value, $"Seed {seed}, floor {first.Floor}: the next floor gives the same grid.");
        }
    }

    /// <summary>The hash of every block of a grid, in array order.</summary>
    internal static StateHash GridHash(VoxelGrid grid)
    {
        StateHash hash = StateHash.Start();
        hash.Add(grid.SizeX);
        hash.Add(grid.SizeY);
        hash.Add(grid.SizeZ);
        for (int y = 0; y < grid.SizeY; y++)
        {
            for (int z = 0; z < grid.SizeZ; z++)
            {
                for (int x = 0; x < grid.SizeX; x++)
                {
                    hash.Add((byte)grid.Get(x, y, z));
                }
            }
        }

        return hash;
    }

    /// <summary>The canvas starts as rock, and it refuses a unit whose floor cell is air, a unit that removes a floor, and a unit that reaches the shell.</summary>
    [Fact]
    public void TheCanvasKeepsEveryFloor()
    {
        DigCanvas canvas = new(new VoxelGrid(24, 12, 24));
        Assert.False(canvas.IsAir(5, 5, 5));

        // A tunnel at floor row 4, three high: air in rows 5 to 7.
        List<DigColumn> tunnel = [new(10, 10, 4, 3), new(11, 10, 4, 3), new(12, 10, 4, 3)];
        Assert.True(canvas.CanCarve(tunnel));
        canvas.Carve(tunnel);
        Assert.True(canvas.IsAir(11, 5, 10) && canvas.IsAir(11, 7, 10));
        Assert.False(canvas.IsAir(11, 4, 10) || canvas.IsAir(11, 8, 10));

        // The same floor row joins the tunnel, and a stamp over an air floor cell is refused.
        Assert.True(canvas.CanCarve([new(13, 10, 4, 3)]));
        Assert.False(canvas.CanCarve([new(11, 10, 5, 3)]));

        // A unit one row lower would carve the floor of the tunnel, and a unit whose top touches it too.
        Assert.False(canvas.CanCarve([new(11, 10, 3, 3)]));
        Assert.False(canvas.CanCarve([new(11, 10, 1, 3)]));

        // A unit that starts right over the tunnel ceiling stands on it, and a unit far below passes under it.
        Assert.True(canvas.CanCarve([new(11, 10, 8, 2)]));
        Assert.True(canvas.CanCarve([new(11, 10, 1, 2)]));

        // The shell: a column at the edge, a floor row of zero, and a top row that would open the roof.
        Assert.False(canvas.CanCarve([new(0, 10, 4, 3)]));
        Assert.False(canvas.CanCarve([new(10, 23, 4, 3)]));
        Assert.False(canvas.CanCarve([new(10, 10, 0, 3)]));
        Assert.False(canvas.CanCarve([new(5, 5, 8, 3)]));
        Assert.True(canvas.CanCarve([new(5, 5, 7, 3)]));

        ContextException error = Assert.Throws<ContextException>(() => canvas.Carve([new(11, 10, 3, 3)]));
        Assert.Contains("carve rule", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The budget draw fills the window of each band of the checkout over one thousand seeds, and it names the floor when no draw can (D-167).</summary>
    [Fact]
    public void TheBudgetDrawFillsEveryBand()
    {
        foreach (FloorTemplate floor in TestWorld.Content.Floors)
        {
            for (int seed = 1; seed <= 1000; seed++)
            {
                Rng rng = Rng.ForStream((ulong)seed, RngStream.Procgen, (int)floor.MinDepth);
                IReadOnlyList<ChamberKind> drawn = ChamberBudget.Draw(rng, floor, TestWorld.Content.Chambers);
                long sum = 0;
                foreach (ChamberKind kind in drawn)
                {
                    sum += kind.Weight;
                }

                Assert.True(sum >= ChamberBudget.WindowBottom(floor.DifficultyBudget) && sum <= ChamberBudget.WindowTop(floor.DifficultyBudget), $"Seed {seed}, template '{floor.Id}': the sum is {sum}.");
                Assert.True(drawn.Count >= floor.RoomCountMin && drawn.Count <= floor.RoomCountMax, $"Seed {seed}, template '{floor.Id}': {drawn.Count} chambers.");
            }
        }

        FloorTemplate tight = new("tight", 1, 1, 1, 1, 10, "test", 24, 12, 24, 7, 5, 5, 4, 5, 8, [2, 3, 4], 180, 120, 30, 12);
        ChamberKind heavy = new("heavy", 100, 1, 1, 3, 3, 0);
        ContextException error = Assert.Throws<ContextException>(() => ChamberBudget.Draw(Rng.ForStream(1UL, RngStream.Procgen, 1), tight, [heavy]));
        Assert.Contains("floorTemplate=tight", error.Message, StringComparison.Ordinal);
        Assert.Contains("windowBottom=9", error.Message, StringComparison.Ordinal);

        ContextException empty = Assert.Throws<ContextException>(() => ChamberBudget.Draw(Rng.ForStream(1UL, RngStream.Procgen, 1), tight, []));
        Assert.Contains("no chamber kind", empty.Message, StringComparison.Ordinal);
    }

    /// <summary>A footprint holds its anchor, every cell lies in one four-connected shape, and every cell has a run of three along X or Z (D-166, D-253).</summary>
    [Fact]
    public void TheFootprintIsOneConnectedShape()
    {
        foreach (ChamberKind kind in TestWorld.Content.Chambers)
        {
            for (int seed = 1; seed <= 200; seed++)
            {
                Column anchor = new(100, 100);
                IReadOnlyList<Column> footprint = ChamberFootprint.Make(Rng.ForStream((ulong)seed, RngStream.Procgen, 1), kind, anchor);
                HashSet<Column> cells = [.. footprint];
                Assert.Contains(anchor, cells);
                Assert.Equal(footprint.Count, cells.Count);

                Queue<Column> queue = new();
                HashSet<Column> seen = [anchor];
                queue.Enqueue(anchor);
                while (queue.Count > 0)
                {
                    Column cell = queue.Dequeue();
                    Column[] neighbors = [new(cell.X + 1, cell.Z), new(cell.X - 1, cell.Z), new(cell.X, cell.Z + 1), new(cell.X, cell.Z - 1)];
                    foreach (Column neighbor in neighbors)
                    {
                        if (cells.Contains(neighbor) && seen.Add(neighbor))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                Assert.True(seen.Count == cells.Count, $"Seed {seed}, kind '{kind.Id}': the anchor reaches {seen.Count} of {cells.Count} cells.");

                foreach (Column cell in footprint)
                {
                    bool runX = false;
                    bool runZ = false;
                    for (int low = -2; low <= 0; low++)
                    {
                        runX |= cells.Contains(new(cell.X + low, cell.Z)) && cells.Contains(new(cell.X + low + 1, cell.Z)) && cells.Contains(new(cell.X + low + 2, cell.Z));
                        runZ |= cells.Contains(new(cell.X, cell.Z + low)) && cells.Contains(new(cell.X, cell.Z + low + 1)) && cells.Contains(new(cell.X, cell.Z + low + 2));
                    }

                    Assert.True(runX || runZ, $"Seed {seed}, kind '{kind.Id}': the cell {cell} has no run of three.");
                }
            }
        }
    }

    /// <summary>The search steps one block up, drops any depth, and stops at two blocks up and at a wall, and it walks around a wall (D-165).</summary>
    [Fact]
    public void TheSearchFollowsTheBodyRule()
    {
        // A floor at row 0. Along z = 2: a step of one at x = 3, a step of two at x = 5, a low ceiling at x = 7, and a wall at x = 9.
        VoxelGrid grid = new(12, 8, 5);
        for (int x = 0; x < 12; x++)
        {
            for (int z = 0; z < 5; z++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
            }
        }

        grid.Set(3, 1, 2, BlockId.RawStone);
        grid.Set(5, 1, 2, BlockId.RawStone);
        grid.Set(5, 2, 2, BlockId.RawStone);
        grid.Set(7, 3, 2, BlockId.RawStone);
        grid.Set(7, 4, 2, BlockId.RawStone);
        for (int y = 1; y < 8; y++)
        {
            grid.Set(9, y, 2, BlockId.RawStone);
        }

        Reachability reach = Reachability.From(grid, new Cell(1, 0, 2));
        Assert.Equal(0, reach.Distance(new Cell(1, 0, 2)));
        Assert.Equal(1, reach.Distance(new Cell(2, 0, 2)));
        Assert.Equal(2, reach.Distance(new Cell(3, 1, 2)));
        Assert.Equal(3, reach.Distance(new Cell(4, 0, 2)));
        Assert.Equal(new Cell(3, 1, 2), reach.PathTo(new Cell(4, 0, 2))[2]);
        Assert.Equal(4, reach.PathTo(new Cell(4, 0, 2)).Count);

        // The step of two is no floor cell to reach, and the walk along z = 2 stops at it, so x = 6 comes around by z = 1.
        Assert.False(reach.IsReachable(new Cell(5, 2, 2)));
        Assert.Equal(7, reach.Distance(new Cell(6, 0, 2)));

        // Under the low ceiling the cell has its two air cells, so the walk passes through it.
        Assert.Equal(8, reach.Distance(new Cell(7, 0, 2)));

        // The wall stands on the floor cell, so that cell is no floor, and the walk goes around it.
        Assert.False(reach.IsReachable(new Cell(9, 0, 2)));
        Assert.Equal(11, reach.Distance(new Cell(10, 0, 2)));

        ContextException unreached = Assert.Throws<ContextException>(() => reach.Distance(new Cell(5, 2, 2)));
        Assert.Contains("did not reach", unreached.Message, StringComparison.Ordinal);

        ContextException rock = Assert.Throws<ContextException>(() => Reachability.From(grid, new Cell(5, 1, 2)));
        Assert.Contains("not a floor cell", rock.Message, StringComparison.Ordinal);
    }

    /// <summary>A drop lands on the first rock below, a walk passes under a ledge, and a step up needs a third air cell over the start.</summary>
    [Fact]
    public void TheSearchDropsAndJumps()
    {
        VoxelGrid grid = new(8, 10, 3);
        for (int x = 0; x < 8; x++)
        {
            grid.Set(x, 0, 1, BlockId.RawStone);
        }

        // A ledge at row 4 over x = 1 and 2, with the drop at x = 3.
        grid.Set(1, 4, 1, BlockId.RawStone);
        grid.Set(2, 4, 1, BlockId.RawStone);
        Reachability fromLedge = Reachability.From(grid, new Cell(1, 4, 1));
        Assert.Equal(2, fromLedge.Distance(new Cell(3, 0, 1)));
        Assert.Equal(3, fromLedge.Distance(new Cell(2, 0, 1)));
        Assert.Equal(0, GridMoves.Landing(grid, 2, 4, 1, 3, 1));

        // From the floor, the ledge is four blocks up, so no move reaches it.
        Reachability fromFloor = Reachability.From(grid, new Cell(4, 0, 1));
        Assert.False(fromFloor.IsReachable(new Cell(2, 4, 1)));

        // A step of one at x = 5 with a ceiling three over the start at x = 4: the jump has no room, and nothing else leads to x = 5.
        grid.Set(4, 3, 1, BlockId.RawStone);
        grid.Set(5, 1, 1, BlockId.RawStone);
        Reachability underCeiling = Reachability.From(grid, new Cell(4, 0, 1));
        Assert.False(underCeiling.IsReachable(new Cell(5, 1, 1)));
        Assert.Equal(-1, GridMoves.Landing(grid, 4, 0, 1, 5, 1));
        Assert.Equal(1, GridMoves.Landing(grid, 6, 0, 1, 5, 1));
    }

    /// <summary>A floor below one, a floor that no template covers, and a floor that two templates cover are errors that name the floor (D-252, T-2).</summary>
    [Fact]
    public void AFloorWithoutOneTemplateIsAnError()
    {
        ContextException below = Assert.Throws<ContextException>(() => FloorGenerator.Generate(1UL, 0, TestWorld.Content));
        Assert.Contains("floor=0", below.Message, StringComparison.Ordinal);

        ContextException beyond = Assert.Throws<ContextException>(() => FloorGenerator.Generate(1UL, 16, TestWorld.Content));
        Assert.Contains("templates=0", beyond.Message, StringComparison.Ordinal);
        Assert.Contains("seed=1", beyond.Message, StringComparison.Ordinal);

        // The template that covers floor 1, and not the first of the list: the list is in path order, and that puts the deep band first.
        List<FloorTemplate> doubled = [.. TestWorld.Content.Floors, FloorGenerator.TemplateFor(1, TestWorld.Content) with { Id = "again" }];
        ContentSet twice = TestWorld.Content with { Floors = doubled };
        ContextException two = Assert.Throws<ContextException>(() => FloorGenerator.Generate(1UL, 1, twice));
        Assert.Contains("templates=2", two.Message, StringComparison.Ordinal);

        ContentSet noKinds = TestWorld.Content with { Chambers = [] };
        ContextException none = Assert.Throws<ContextException>(() => FloorGenerator.Generate(1UL, 1, noKinds));
        Assert.Contains("no chamber kind", none.Message, StringComparison.Ordinal);
    }

    /// <summary>The stairwell sweep, a class of its own, so it runs beside the other sweeps (D-478). xUnit runs the tests of one class in sequence, and each nested class is a class of its own.</summary>
    [Trait("Category", SweepScope.SweepCategory)]
    public sealed class StairwellSweep
    {
        /// <summary>
        /// PR-9 exit test 3. Over the property seeds, a path leads from the spawn to the stairwell, the stairwell is a
        /// floor cell of one chamber, and no chamber lies farther from the spawn than that chamber (D-256).
        /// </summary>
        [Fact]
        public void StairwellReachable()
        {
            for (int seed = 1; seed <= PropertySeeds; seed++)
            {
                FloorPlan plan = Plan(seed);
                string context = $"Seed {seed}, floor {plan.Floor}";
                Reachability reach = Reachability.From(plan.Grid, SpawnCell(plan));
                Assert.True(GridMoves.IsFloor(plan.Grid, plan.Stairwell), $"{context}: the stairwell {plan.Stairwell} is not a floor cell.");
                Assert.True(reach.IsReachable(plan.Stairwell), $"{context}: the stairwell {plan.Stairwell} is not reachable from {reach.Start}.");
                Assert.NotEqual(reach.Start, plan.Stairwell);

                Chamber? holder = null;
                foreach (Chamber chamber in plan.Chambers)
                {
                    foreach (Column column in chamber.Footprint)
                    {
                        if (column.X == plan.Stairwell.X && column.Z == plan.Stairwell.Z && chamber.FloorRow == plan.Stairwell.Y)
                        {
                            holder = chamber;
                        }
                    }
                }

                Assert.True(holder is not null, $"{context}: the stairwell {plan.Stairwell} lies in no chamber.");
                int holderDistance = NearestDistance(holder!, plan.Grid, reach);
                foreach (Chamber chamber in plan.Chambers)
                {
                    int distance = NearestDistance(chamber, plan.Grid, reach);
                    Assert.True(distance <= holderDistance, $"{context}: chamber {chamber.Index} lies {distance} moves away, past the stairwell chamber {holder!.Index} at {holderDistance}.");
                }

                int stairwellDistance = reach.Distance(plan.Stairwell);
                foreach (Column column in holder!.Footprint)
                {
                    Cell cell = new(column.X, holder.FloorRow, column.Z);
                    if (GridMoves.IsFloor(plan.Grid, cell) && reach.IsReachable(cell))
                    {
                        Assert.True(reach.Distance(cell) <= stairwellDistance, $"{context}: the cell {cell} of the stairwell chamber lies farther than the stairwell.");
                    }
                }
            }
        }
    }

    /// <summary>The tunnel sweeps, a class of their own, so they run beside the other sweeps (D-478). xUnit runs the tests of one class in sequence, and each nested class is a class of its own.</summary>
    [Trait("Category", SweepScope.SweepCategory)]
    public sealed class TunnelSweeps
    {
        /// <summary>
        /// PR-66 exit test 2 (D-347). Over the seeds of a property sweep, no dug floor holds two walkable cells in
        /// neighboring columns one row apart that are both plain blocks. A tunnel changes height by a ramp or by a
        /// shaft alone, and a jump clears one block for rubble and ledges alone, so the check reads the dug floor
        /// before the detail pass (D-165, D-258).
        /// </summary>
        [Fact]
        public void TunnelsHaveNoStep()
        {
            List<string> failures = [];
            for (int seed = 1; seed <= PropertySeeds && failures.Count < 10; seed++)
            {
                int floor = FloorOf(seed);
                VoxelGrid grid = DugGrid(seed, floor);
                for (int y = 1; y < grid.SizeY - 2; y++)
                {
                    for (int z = 1; z < grid.SizeZ - 1; z++)
                    {
                        for (int x = 1; x < grid.SizeX - 1; x++)
                        {
                            Cell low = new(x, y, z);
                            if (!IsPlainFloor(grid, low))
                            {
                                continue;
                            }

                            Cell[] neighbors = [new(x + 1, y + 1, z), new(x - 1, y + 1, z), new(x, y + 1, z + 1), new(x, y + 1, z - 1)];
                            foreach (Cell high in neighbors)
                            {
                                if (IsPlainFloor(grid, high))
                                {
                                    failures.Add($"Seed {seed}, floor {floor}: the cell {low} and the cell {high} are plain floor cells one row apart.");
                                }
                            }
                        }
                    }
                }
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures.Take(10)));
        }

        /// <summary>
        /// PR-63 exit test 2 and PR-9 exit test 5. Over the property seeds, every stamp of the gallery is air over the
        /// gallery width and height of its template, and every stamp of a drift is air over the drift width and height
        /// (D-341, D-342). Every tunnel air cell also sits inside an air cross-section three blocks wide and three blocks
        /// high (D-166). A pillar and the rubble of a collapse fill cells on purpose, so both checks step over them (PR-59).
        /// </summary>
        [Fact]
        public void TunnelCrossSection()
        {
            int galleryStamps = 0;
            int driftStamps = 0;
            for (int seed = 1; seed <= PropertySeeds; seed++)
            {
                FloorPlan plan = Plan(seed);
                VoxelGrid grid = plan.Grid;
                FloorTemplate template = plan.Template;
                HashSet<Cell> collapses = [.. plan.Detail.Collapses];
                HashSet<Column> pillarColumns = [];
                foreach (Cell pillar in plan.Detail.Pillars)
                {
                    pillarColumns.Add(new Column(pillar.X, pillar.Z));
                }

                // The size of each stamp comes from the template, and not from the plan, so a plan that digs another size fails.
                BlockId pillarBlock = DetailPass.PillarBlock(template.Band);
                foreach (TunnelStamp stamp in plan.Tunnels)
                {
                    string tunnel = stamp.Gallery ? "gallery" : "drift";
                    int width = stamp.Gallery ? template.GalleryWidth : template.DriftWidth;
                    int height = stamp.Gallery ? template.GalleryHeight : template.DriftHeight;
                    galleryStamps += stamp.Gallery ? 1 : 0;
                    driftStamps += stamp.Gallery ? 0 : 1;
                    for (int z = stamp.Center.Z - (width / 2); z <= stamp.Center.Z + (width / 2); z++)
                    {
                        for (int x = stamp.Center.X - (width / 2); x <= stamp.Center.X + (width / 2); x++)
                        {
                            for (int y = stamp.Center.Y + 1; y <= stamp.Center.Y + height; y++)
                            {
                                BlockId block = grid.Get(x, y, z);
                                if (block == BlockId.Air || collapses.Contains(new Cell(x, y, z)) || (block == pillarBlock && pillarColumns.Contains(new Column(x, z))))
                                {
                                    continue;
                                }

                                Assert.Fail($"Seed {seed}, floor {plan.Floor}: the {tunnel} stamp at {stamp.Center} holds {block} at ({x}, {y}, {z}), inside its {width} by {height} blocks.");
                            }
                        }
                    }
                }

                // No window of a cell reads past the window reach, so a rubble cell cuts the windows of the cells within
                // that reach alone. A pillar cuts the windows of the columns around it, and a pool cell is water under chamber air.
                bool[] outside = new bool[grid.SizeX * grid.SizeY * grid.SizeZ];
                foreach (Chamber chamber in plan.Chambers)
                {
                    foreach (Cell cell in chamber.AirCells())
                    {
                        MarkOutside(outside, grid, cell.X, cell.Y, cell.Z);
                    }
                }

                foreach (Cell cell in plan.Detail.Collapses)
                {
                    for (int dz = -WindowReach; dz <= WindowReach; dz++)
                    {
                        for (int dx = -WindowReach; dx <= WindowReach; dx++)
                        {
                            for (int dy = -WindowReach; dy <= WindowReach; dy++)
                            {
                                MarkOutside(outside, grid, cell.X + dx, cell.Y + dy, cell.Z + dz);
                            }
                        }
                    }
                }

                foreach (Cell pillar in plan.Detail.Pillars)
                {
                    for (int dz = -WindowReach; dz <= WindowReach; dz++)
                    {
                        for (int dx = -WindowReach; dx <= WindowReach; dx++)
                        {
                            for (int y = 0; y < grid.SizeY; y++)
                            {
                                MarkOutside(outside, grid, pillar.X + dx, y, pillar.Z + dz);
                            }
                        }
                    }
                }

                for (int y = 0; y < grid.SizeY; y++)
                {
                    for (int z = 0; z < grid.SizeZ; z++)
                    {
                        for (int x = 0; x < grid.SizeX; x++)
                        {
                            bool tunnelAir = !grid.IsSolid(x, y, z) && grid.Get(x, y, z) != BlockId.StillWater && !outside[CellIndex(grid, x, y, z)];
                            if (tunnelAir && !HasCrossSection(grid, x, y, z))
                            {
                                Assert.Fail($"Seed {seed}, floor {plan.Floor}: the air cell ({x}, {y}, {z}) has no three by three window of air.");
                            }
                        }
                    }
                }
            }

            Assert.True(galleryStamps > 0 && driftStamps > 0, $"The sweep saw {galleryStamps} gallery stamps and {driftStamps} drift stamps.");
        }
    }

    /// <summary>The chamber sweeps, a class of their own, so they run beside the other sweeps (D-478). xUnit runs the tests of one class in sequence, and each nested class is a class of its own.</summary>
    [Trait("Category", SweepScope.SweepCategory)]
    public sealed class ChamberSweeps
    {
        /// <summary>PR-9 exit test 2. Over the property seeds, no two chambers share a block, and every chamber block is air.</summary>
        [Fact]
        public void NoChamberOverlap()
        {
            for (int seed = 1; seed <= PropertySeeds; seed++)
            {
                FloorPlan plan = Plan(seed);
                HashSet<Cell> taken = [];
                foreach (Chamber chamber in plan.Chambers)
                {
                    foreach (Cell cell in chamber.AirCells())
                    {
                        Assert.True(taken.Add(cell), $"Seed {seed}, floor {plan.Floor}: the cell {cell} lies in chamber {chamber.Index} and in an earlier chamber.");
                        bool pillar = plan.Detail.Pillars.Contains(new Cell(cell.X, chamber.FloorRow, cell.Z));
                        Assert.True(pillar || !plan.Grid.IsSolid(cell.X, cell.Y, cell.Z), $"Seed {seed}, floor {plan.Floor}: the chamber {chamber.Index} cell {cell} is rock.");
                    }
                }
            }
        }

        /// <summary>PR-9 exit test 4. Over the property seeds, the sum of chamber weights lies within 10 percent of the floor budget, and the count inside the room count range (D-167).</summary>
        [Fact]
        public void BudgetWithinTolerance()
        {
            for (int seed = 1; seed <= PropertySeeds; seed++)
            {
                FloorPlan plan = Plan(seed);
                string context = $"Seed {seed}, floor {plan.Floor}";
                long sum = 0;
                foreach (Chamber chamber in plan.Chambers)
                {
                    sum += chamber.Kind.Weight;
                    Assert.Contains(chamber.Kind, TestWorld.Content.Chambers);
                }

                long budget = plan.Template.DifficultyBudget;
                Assert.True(sum >= ChamberBudget.WindowBottom(budget) && sum <= ChamberBudget.WindowTop(budget), $"{context}: the weights sum to {sum}, outside the window of the budget {budget}.");
                Assert.True(plan.Chambers.Count >= plan.Template.RoomCountMin && plan.Chambers.Count <= plan.Template.RoomCountMax, $"{context}: {plan.Chambers.Count} chambers, outside {plan.Template.RoomCountMin} to {plan.Template.RoomCountMax}.");
            }
        }

        /// <summary>
        /// PR-59 exit test 6. Over the property seeds, every water cell has a reachable dry floor cell beside it, one
        /// block up, so the search that reads water as air holds for a body (D-258).
        /// </summary>
        [Fact]
        public void EveryPoolHasAWayOut()
        {
            int poolsSeen = 0;
            for (int seed = 1; seed <= PropertySeeds; seed++)
            {
                FloorPlan plan = Plan(seed);
                if (plan.Detail.Pools.Count == 0)
                {
                    continue;
                }

                Reachability reach = Reachability.From(plan.Grid, SpawnCell(plan));
                foreach (Cell pool in plan.Detail.Pools)
                {
                    poolsSeen++;
                    int row = pool.Y;
                    Cell below = new(pool.X, row - 1, pool.Z);
                    Assert.True(GridMoves.IsFloor(plan.Grid, below), $"Seed {seed}, floor {plan.Floor}: the pool cell {pool} has no floor under its water.");
                    Assert.True(reach.IsReachable(below), $"Seed {seed}, floor {plan.Floor}: the floor under the pool cell {pool} is not reachable.");

                    bool wayOut = false;
                    Column[] neighbors = [new(pool.X + 1, pool.Z), new(pool.X - 1, pool.Z), new(pool.X, pool.Z + 1), new(pool.X, pool.Z - 1)];
                    foreach (Column neighbor in neighbors)
                    {
                        Cell floor = new(neighbor.X, row, neighbor.Z);
                        if (plan.Grid.Get(floor.X, floor.Y, floor.Z) != BlockId.StillWater && GridMoves.IsFloor(plan.Grid, floor) && reach.IsReachable(floor))
                        {
                            wayOut = true;
                        }
                    }

                    Assert.True(wayOut, $"Seed {seed}, floor {plan.Floor}: the pool cell {pool} has no dry floor cell beside it.");
                }
            }

            Assert.True(poolsSeen > 0, "The sweep saw no pool.");
        }
    }
}
