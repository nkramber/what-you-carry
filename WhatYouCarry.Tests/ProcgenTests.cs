using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The floor generator, the dig canvas, the chamber budget, the footprint, and the reachability search (D-159, D-165 to D-167, D-252 to D-256; PR-9 exit tests 1 to 7).</summary>
public sealed class ProcgenTests
{
    /// <summary>The environment variable that the night job sets to run the night seed counts (D-116).</summary>
    public const string NightVariable = "WYC_NIGHT_SWEEP";

    /// <summary>The seeds of the reachability sweep per PR (PR-9 exit test 1).</summary>
    public const int ReachabilitySeedsPerPr = 5000;

    /// <summary>The seeds of the reachability sweep each night (D-116).</summary>
    public const int ReachabilitySeedsPerNight = 100000;

    /// <summary>The seeds of the other property sweeps per PR.</summary>
    public const int PropertySeeds = 1000;

    /// <summary>The count of seeds of a sweep: the PR count, or the night count when the night variable is "1".</summary>
    internal static int SweepSeeds(int perPr, int perNight)
    {
        return Environment.GetEnvironmentVariable(NightVariable) == "1" ? perNight : perPr;
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
            if (Reachability.IsFloor(grid, cell) && reach.IsReachable(cell))
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
    /// PR-9 exit test 1. Over five thousand seeds per PR and one hundred thousand each night, across every band,
    /// every floor cell of every chamber that still has its floor is reachable from the spawn, the spawn holds the
    /// player box, and every shaft lands on a reachable floor. A failure names its seed and its floor (D-66, D-116).
    /// </summary>
    [Fact]
    public void EveryChamberReachable()
    {
        int seeds = SweepSeeds(ReachabilitySeedsPerPr, ReachabilitySeedsPerNight);
        for (int seed = 1; seed <= seeds; seed++)
        {
            FloorPlan plan = Plan(seed);
            string context = $"Seed {seed}, floor {plan.Floor}";
            PlayerBody body = new(plan.Grid, plan.Spawn);
            Assert.True(body.IsOnGround(), $"{context}: the body at the spawn {plan.Spawn} is not on the ground.");

            Reachability reach = Reachability.From(plan.Grid, SpawnCell(plan));
            Assert.True(plan.Chambers.Count > 0, $"{context}: the floor has no chamber.");
            foreach (Chamber chamber in plan.Chambers)
            {
                int floorCells = 0;
                foreach (Column column in chamber.Footprint)
                {
                    Cell cell = new(column.X, chamber.FloorRow, column.Z);
                    if (!plan.Grid.IsSolid(cell.X, cell.Y, cell.Z))
                    {
                        continue;
                    }

                    floorCells++;
                    Assert.True(Reachability.IsFloor(plan.Grid, cell), $"{context}: the chamber {chamber.Index} cell {cell} has rock over it.");
                    Assert.True(reach.IsReachable(cell), $"{context}: the chamber {chamber.Index} cell {cell} is not reachable from the spawn {reach.Start}.");
                }

                Assert.True(floorCells > 0, $"{context}: the chamber {chamber.Index} has no floor cell left.");
            }

            foreach (Shaft shaft in plan.Shafts)
            {
                Column landing = shaft.Center;
                int floorRow = shaft.LandingAirRow;
                while (!plan.Grid.IsSolid(landing.X, floorRow, landing.Z))
                {
                    floorRow--;
                }

                Assert.True(reach.IsReachable(new Cell(landing.X, floorRow, landing.Z)), $"{context}: the shaft at {shaft.Center} lands on an unreachable floor at row {floorRow}.");
            }
        }
    }

    /// <summary>PR-9 exit test 2. Over one thousand seeds, no two chambers share a block, and every chamber block is air.</summary>
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
                    Assert.False(plan.Grid.IsSolid(cell.X, cell.Y, cell.Z), $"Seed {seed}, floor {plan.Floor}: the chamber {chamber.Index} cell {cell} is rock.");
                }
            }
        }
    }

    /// <summary>
    /// PR-9 exit test 3. Over one thousand seeds, a path leads from the spawn to the stairwell, the stairwell is a
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
            Assert.True(Reachability.IsFloor(plan.Grid, plan.Stairwell), $"{context}: the stairwell {plan.Stairwell} is not a floor cell.");
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
                if (Reachability.IsFloor(plan.Grid, cell) && reach.IsReachable(cell))
                {
                    Assert.True(reach.Distance(cell) <= stairwellDistance, $"{context}: the cell {cell} of the stairwell chamber lies farther than the stairwell.");
                }
            }
        }
    }

    /// <summary>PR-9 exit test 4. Over one thousand seeds, the sum of chamber weights lies within 10 percent of the floor budget, and the count inside the room count range (D-167).</summary>
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

    /// <summary>PR-9 exit test 5. Over one thousand seeds, every air cell sits inside an air cross-section three blocks wide and three blocks high (D-166).</summary>
    [Fact]
    public void TunnelCrossSection()
    {
        for (int seed = 1; seed <= PropertySeeds; seed++)
        {
            FloorPlan plan = Plan(seed);
            VoxelGrid grid = plan.Grid;
            for (int y = 0; y < grid.SizeY; y++)
            {
                for (int z = 0; z < grid.SizeZ; z++)
                {
                    for (int x = 0; x < grid.SizeX; x++)
                    {
                        if (!grid.IsSolid(x, y, z))
                        {
                            Assert.True(HasCrossSection(grid, x, y, z), $"Seed {seed}, floor {plan.Floor}: the air cell ({x}, {y}, {z}) has no three by three window of air.");
                        }
                    }
                }
            }
        }
    }

    /// <summary>PR-9 exit test 6. Floor 15 is larger than floor 1 for one seed, and each band carries its size (D-252).</summary>
    [Fact]
    public void FloorSizeGrowsWithDepth()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            FloorPlan shallow = FloorGenerator.Generate((ulong)seed, 1, TestWorld.Content);
            FloorPlan middle = FloorGenerator.Generate((ulong)seed, 8, TestWorld.Content);
            FloorPlan deep = FloorGenerator.Generate((ulong)seed, 15, TestWorld.Content);

            long shallowCells = (long)shallow.Grid.SizeX * shallow.Grid.SizeY * shallow.Grid.SizeZ;
            long middleCells = (long)middle.Grid.SizeX * middle.Grid.SizeY * middle.Grid.SizeZ;
            long deepCells = (long)deep.Grid.SizeX * deep.Grid.SizeY * deep.Grid.SizeZ;
            Assert.True(shallowCells < middleCells && middleCells < deepCells, $"Seed {seed}: the floors hold {shallowCells}, {middleCells}, and {deepCells} cells.");

            Assert.Equal(48, shallow.Grid.SizeX);
            Assert.Equal(12, shallow.Grid.SizeY);
            Assert.Equal(96, deep.Grid.SizeX);
            Assert.Equal(20, deep.Grid.SizeY);
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
    private static StateHash GridHash(VoxelGrid grid)
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

        FloorTemplate tight = new("tight", 1, 1, 1, 1, 10, "test", 24, 12, 24);
        ChamberKind heavy = new("heavy", 100, 1, 1, 3, 3);
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
        Assert.Equal(0, Reachability.Landing(grid, 2, 4, 1, 3, 1));

        // From the floor, the ledge is four blocks up, so no move reaches it.
        Reachability fromFloor = Reachability.From(grid, new Cell(4, 0, 1));
        Assert.False(fromFloor.IsReachable(new Cell(2, 4, 1)));

        // A step of one at x = 5 with a ceiling three over the start at x = 4: the jump has no room, and nothing else leads to x = 5.
        grid.Set(4, 3, 1, BlockId.RawStone);
        grid.Set(5, 1, 1, BlockId.RawStone);
        Reachability underCeiling = Reachability.From(grid, new Cell(4, 0, 1));
        Assert.False(underCeiling.IsReachable(new Cell(5, 1, 1)));
        Assert.Equal(-1, Reachability.Landing(grid, 4, 0, 1, 5, 1));
        Assert.Equal(1, Reachability.Landing(grid, 6, 0, 1, 5, 1));
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
}
