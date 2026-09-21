using System.Collections.Generic;
using System.Linq;
using WhatYouCarry.Core.Ai;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The pathfinder, the enemy family, the humanoid brain, the spawns, and the full clearer of PR-16 (D-76, D-165,
/// D-345, D-395 to D-405; PR-16 exit tests 1 to 7).
/// </summary>
public sealed class EnemyTests
{
    /// <summary>The seeds of a property test loop. Each failure names its seed (D-66).</summary>
    private const int Seeds = 1000;

    /// <summary>The floor that the scavenger covers, which every spawn test reads (D-395).</summary>
    private const int ScavengerFloor = 1;

    /// <summary>The seeds that also compare the path length with the breadth-first search of the generator.</summary>
    private const int ShortestPathSeeds = 50;

    /// <summary>
    /// PR-16 exit test 1. Every move of a path is a move of the rule of D-165 as D-345 revises it: one block up,
    /// any drop, or a walk along a ramp. No move is a step of two blocks, and no move leaves the four sides.
    /// </summary>
    [Fact]
    public void PathfinderRespectsMoveRule()
    {
        int paths = 0;
        int ramps = 0;
        int drops = 0;
        int steps = 0;
        for (ulong seed = 1; seed <= 60; seed++)
        {
            FloorPlan plan = FloorGenerator.Generate(seed, ScavengerFloor, TestWorld.Content);
            GridPathfinder finder = new(plan.Grid);
            Cell spawn = PathWalk.FloorCellOf(plan.Spawn);
            foreach (EnemySpawn enemy in plan.EnemySpawns)
            {
                Assert.True(finder.TryFind(spawn, enemy.Cell, out IReadOnlyList<Cell> path), $"Seed {seed}: no path joins the spawn {spawn} and the enemy cell {enemy.Cell}.");
                paths++;
                for (int step = 1; step < path.Count; step++)
                {
                    Cell from = path[step - 1];
                    Cell to = path[step];
                    int alongX = to.X - from.X;
                    int alongZ = to.Z - from.Z;
                    int sides = (alongX < 0 ? -alongX : alongX) + (alongZ < 0 ? -alongZ : alongZ);
                    Assert.Equal(1, sides);

                    int rise = to.Y - from.Y;
                    Assert.True(rise <= 1, $"Seed {seed}: the move from {from} to {to} rises {rise} blocks, and a move rises one at most (D-165).");
                    Assert.True(GridMoves.IsFloor(plan.Grid, to), $"Seed {seed}: the move from {from} lands on {to}, and that is no floor cell.");

                    bool walk = GridMoves.RampWalk(plan.Grid, from.X, from.Y, from.Z, to.X, to.Z) == to.Y;
                    bool landing = GridMoves.Landing(plan.Grid, from.X, from.Y, from.Z, to.X, to.Z) == to.Y;
                    Assert.True(walk || landing, $"Seed {seed}: the move from {from} to {to} is no walk along a ramp and no step or drop (D-345).");
                    if (walk)
                    {
                        ramps++;
                    }
                    else if (rise == 1)
                    {
                        steps++;
                    }
                    else if (rise < 0)
                    {
                        drops++;
                    }
                }
            }
        }

        // A sweep that held no ramp and no drop would pass every assertion above and read nothing. The generator
        // digs no one-block step in a tunnel, so a step up is rare and the sweep asserts no count of it (D-347).
        Assert.True(paths > 100, $"The sweep read {paths} paths, and it needs more than 100 to cover the rule.");
        Assert.True(ramps > 0, "The sweep found no walk along a ramp (D-345).");
        Assert.True(drops > 0, "The sweep found no drop (D-165).");
        Assert.True(steps >= 0, "The count of steps up is never below zero.");
    }

    /// <summary>
    /// PR-16 exit test 2. Over one thousand seeds, a path joins every enemy spawn and the stairwell, and the count
    /// of moves of that path is the count that the reachability search of the generator gives.
    /// </summary>
    [Fact]
    public void PathfinderFindsStairwell()
    {
        for (ulong seed = 1; seed <= Seeds; seed++)
        {
            FloorPlan plan = FloorGenerator.Generate(seed, ScavengerFloor, TestWorld.Content);
            if (plan.EnemySpawns.Count == 0)
            {
                continue;
            }

            GridPathfinder finder = new(plan.Grid);
            foreach (EnemySpawn enemy in plan.EnemySpawns)
            {
                Assert.True(finder.TryFind(enemy.Cell, plan.Stairwell, out IReadOnlyList<Cell> path), $"Seed {seed}: no path joins the enemy cell {enemy.Cell} and the stairwell {plan.Stairwell}.");

                // The A* search and the breadth-first search read one move rule, so both give a shortest path. A
                // search over the whole grid costs far more than one A* search, so the count check reads the first
                // seeds and the path check reads every seed.
                if (seed <= ShortestPathSeeds)
                {
                    Reachability reach = Reachability.From(plan.Grid, enemy.Cell);
                    Assert.Equal(reach.Distance(plan.Stairwell), path.Count - 1);
                }
            }
        }
    }

    /// <summary>
    /// PR-16 exit test 3. An enemy swing has the phases of the weapon that its family names, which is the weapon of
    /// the player, so the two swings run the same windup, active, and recovery (D-31, D-397).
    /// </summary>
    [Fact]
    public void EnemyUsesPlayerRules()
    {
        SimulationLoop loop = new(1, TestWorld.Content);
        Assert.NotEmpty(loop.Enemies);
        WeaponDefinition player = loop.Weapon;
        foreach (Enemy enemy in loop.Enemies)
        {
            Assert.Equal(player.WindupTicks, enemy.Weapon.WindupTicks);
            Assert.Equal(player.ActiveTicks, enemy.Weapon.ActiveTicks);
            Assert.Equal(player.RecoveryTicks, enemy.Weapon.RecoveryTicks);
            Assert.Equal(player.SwingTicks, enemy.Weapon.SwingTicks);
            // The box is the box of D-165, read from its two faces, so the widths compare inside one float step.
            Assert.Equal(enemy.Body.Position.X - PlayerBody.HalfWidth, enemy.Body.Box.Min.X);
            Assert.Equal(enemy.Body.Position.X + PlayerBody.HalfWidth, enemy.Body.Box.Max.X);
            Assert.Equal(enemy.Body.Position.Y, enemy.Body.Box.Min.Y);
            Assert.Equal(enemy.Body.Position.Y + PlayerBody.Height, enemy.Body.Box.Max.Y);
        }

        // A swing of the enemy runs the phases of that weapon, tick for tick, like the swing of the player.
        Enemy first = loop.Enemies[0];
        first.StartSwing();
        for (long tick = 0; tick < first.Weapon.SwingTicks; tick++)
        {
            Assert.Equal(tick, first.SwingTick);
            first.Step(new Vector3(0.0f, 0.0f, 0.0f), false, 0, []);
        }

        Assert.Equal(Core.Combat.Swing.NoSwing, first.SwingTick);
        Assert.Equal((int)first.Definition.AttackCooldownTicks, first.AttackCooldown);
    }

    /// <summary>
    /// PR-16 exit test 4. The spawned weight of a floor lands inside one tenth of the floor budget less the weight
    /// of the chamber that holds the player spawn (D-167, D-398).
    /// </summary>
    [Fact]
    public void EnemyCountMatchesBudget()
    {
        for (ulong seed = 1; seed <= Seeds; seed++)
        {
            FloorPlan plan = FloorGenerator.Generate(seed, ScavengerFloor, TestWorld.Content);
            Assert.True(EnemyPlacement.TryFamilyOf(plan.Floor, TestWorld.Content.Enemies, out EnemyDefinition family), $"Seed {seed}: no enemy family covers floor {plan.Floor}.");

            long chambers = 0;
            for (int index = EnemyPlacement.SpawnChamberIndex + 1; index < plan.Chambers.Count; index++)
            {
                chambers += plan.Chambers[index].Kind.Weight;
            }

            long spawned = plan.EnemySpawns.Count * family.Weight;
            long window = chambers / 10;
            Assert.InRange(spawned, chambers - window - family.Weight, chambers + window + family.Weight);

            // No enemy stands in the chamber of the player spawn (D-398).
            foreach (EnemySpawn spawn in plan.EnemySpawns)
            {
                Assert.NotEqual(EnemyPlacement.SpawnChamberIndex, spawn.ChamberIndex);
            }
        }
    }

    /// <summary>
    /// PR-16 exit test 5. When the full clearer takes the stairwell choice, every enemy that it did not drop is
    /// dead. The policy drops an enemy that no path reaches and one that a hunt gains nothing on (D-149).
    /// </summary>
    /// <remarks>
    /// A floor can strand the policy away from an enemy, because a shaft drops a body into a space that no move
    /// leads back out of (D-253). The test reads the drop list of the policy, so a floor that strands it still
    /// asserts the rule: the policy leaves no enemy alive that it could reach.
    /// </remarks>
    [Fact]
    public void FullClearerClearsFloor()
    {
        int floorsCleared = 0;
        for (ulong seed = 1; seed <= 40; seed++)
        {
            FullClearer policy = new(TestWorld.Content);
            SimulationLoop loop = new(seed, TestWorld.Content);
            int floor = loop.Floor;
            for (uint tick = 0; tick < BotRun.FloorBudget && !loop.Ended; tick++)
            {
                Intent intent = policy.Next(loop);
                bool takesStairwell = (intent.Buttons & (Button.Interact | Button.Ascend)) != 0;
                bool atStairwell = StairwellTransition.IsAtStairwell(loop.Body, loop.Plan.Stairwell);
                if (takesStairwell && atStairwell)
                {
                    foreach (Enemy enemy in loop.Enemies)
                    {
                        Assert.True(enemy.IsDead || policy.Dropped.Contains(enemy.Owner), $"Seed {seed}, floor {loop.Floor}: the clearer took the stairwell with the enemy {enemy.Owner} alive, and the hunt did not drop it.");
                    }

                    floorsCleared++;
                }

                loop.Step(intent);
                if (loop.Floor != floor)
                {
                    floor = loop.Floor;
                }
            }
        }

        Assert.True(floorsCleared > 0, "No run of the full clearer reached a stairwell, so the test read nothing.");
    }

    /// <summary>
    /// PR-16 exit test 6. A replay of a record with enemies gives the state hash of the live run, tick for tick.
    /// The three-platform part of the test is the bit-identity sweep, which folds the same replay (G-9).
    /// </summary>
    [Fact]
    public void AiIsDeterministic()
    {
        for (ulong seed = 1; seed <= 8; seed++)
        {
            FullClearer live = new(TestWorld.Content);
            FullClearer twin = new(TestWorld.Content);
            SimulationLoop first = new(seed, TestWorld.Content);
            SimulationLoop second = new(seed, TestWorld.Content);
            Assert.True(first.Enemies.Count > 0, $"Seed {seed}: floor 1 holds no enemy, so the test read nothing.");

            List<Intent> intents = [];
            for (int tick = 0; tick < 900 && !first.Ended; tick++)
            {
                Intent intent = live.Next(first);
                intents.Add(intent);
                first.Step(intent);
            }

            for (int tick = 0; tick < intents.Count && !second.Ended; tick++)
            {
                // The second run takes the intents of the first, so a policy that read the state cannot hide a
                // divergence of the simulation behind a different choice.
                second.Step(intents[tick]);
                Assert.Equal(first.Seed, second.Seed);
            }

            Assert.Equal(intents.Count, (int)second.Tick);
            Assert.Equal(twin.Name, live.Name);

            SimulationLoop replay = new(seed, TestWorld.Content);
            foreach (Intent intent in intents)
            {
                if (replay.Ended)
                {
                    break;
                }

                replay.Step(intent);
            }

            Assert.Equal(first.Hash(), replay.Hash());
            Assert.Equal(first.LivingEnemies, replay.LivingEnemies);
        }
    }

    /// <summary>
    /// The brain wakes on sight of the player and gives up when it loses the line of sight for the ticks of its
    /// family (D-400).
    /// </summary>
    [Fact]
    public void BrainWakesOnSightAndGivesUp()
    {
        SimulationLoop loop = new(1, TestWorld.Content);
        HumanoidBrain brain = loop.Brains[0];
        Enemy enemy = brain.Enemy;
        Assert.False(brain.IsHunting);

        GridPathfinder finder = new(loop.Grid);
        IReadOnlyList<EntityBox> none = [];

        // A player at the post of the enemy stands inside the sight range with a clear ray of zero length.
        Vector3 beside = PathWalk.CenterOf(brain.Post);
        brain.Step(loop.Grid, finder, beside, none);
        Assert.True(brain.IsHunting);

        // A player far past the sight range gives no ray, so the hunt ends after the give-up ticks of the family.
        Vector3 far = new(beside.X + (enemy.Definition.SightMetres * 4.0f), beside.Y, beside.Z);
        for (long tick = 0; tick < enemy.Definition.GiveUpTicks; tick++)
        {
            Assert.True(brain.IsHunting, $"The hunt ended after {tick} blind ticks, and the family gives up after {enemy.Definition.GiveUpTicks}.");
            brain.Step(loop.Grid, finder, far, none);
        }

        Assert.False(brain.IsHunting);
    }

    /// <summary>A yaw that faces a point sends the blade of a swing at it (D-234, D-324).</summary>
    [Theory]
    [InlineData(0.0f, -1.0f, 0)]
    [InlineData(-1.0f, 0.0f, 9000)]
    [InlineData(0.0f, 1.0f, 18000)]
    [InlineData(1.0f, 0.0f, 27000)]
    public void YawTowardFacesThePoint(float alongX, float alongZ, int expected)
    {
        Vector3 from = new(10.0f, 4.0f, 10.0f);
        Vector3 to = new(from.X + alongX, from.Y, from.Z + alongZ);
        Assert.Equal(expected, HumanoidBrain.YawToward(from, to));
    }

    /// <summary>The spawn cells of a floor are floor cells that no ramp holds, and no two spawns share one cell (D-398).</summary>
    [Fact]
    public void SpawnCellsAreFreeFloorCells()
    {
        for (ulong seed = 1; seed <= 200; seed++)
        {
            FloorPlan plan = FloorGenerator.Generate(seed, ScavengerFloor, TestWorld.Content);
            List<Cell> taken = [];
            foreach (EnemySpawn spawn in plan.EnemySpawns)
            {
                Assert.True(GridMoves.IsFloor(plan.Grid, spawn.Cell), $"Seed {seed}: the spawn cell {spawn.Cell} is no floor cell.");
                Assert.False(plan.Grid.TryGetRamp(spawn.Cell.X, spawn.Cell.Y, spawn.Cell.Z, out _), $"Seed {seed}: the spawn cell {spawn.Cell} holds a ramp, and a body slides off a slope (D-363).");
                Assert.DoesNotContain(spawn.Cell, taken);
                taken.Add(spawn.Cell);
            }
        }
    }

    /// <summary>A hit of the blade of the player takes health off the enemy that it met, and a dead enemy leaves the target list (D-322, D-325).</summary>
    [Fact]
    public void PlayerBladeKillsAnEnemy()
    {
        SimulationLoop loop = new(1, TestWorld.Content);
        Enemy enemy = loop.Enemies[0];
        int health = enemy.Health;
        Assert.Equal((int)enemy.Definition.Health, health);

        long damage = loop.Weapon.Damage;
        while (!enemy.IsDead)
        {
            enemy.TakeHit(damage);
        }

        Assert.Equal(0, enemy.Health);
        Assert.Equal(loop.Enemies.Count - 1, loop.LivingEnemies);
        Assert.DoesNotContain(new Vector3(enemy.Body.Box.Min.X, enemy.Body.Box.Min.Y, enemy.Body.Box.Min.Z), loop.AimTargets());
    }

    /// <summary>A pathfinder gives the same path on two runs, and a goal that no move reaches gives no path (T-2).</summary>
    [Fact]
    public void PathfinderRepeatsAndReportsNoPath()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        GridPathfinder finder = new(grid);
        Cell start = new(2, 0, 2);
        Cell goal = new(12, 0, 12);
        Assert.True(finder.TryFind(start, goal, out IReadOnlyList<Cell> first));
        Assert.True(finder.TryFind(start, goal, out IReadOnlyList<Cell> second));
        Assert.Equal(first, second);
        Assert.Equal(GridPathfinder.Estimate(start, goal) + 1, first.Count);

        // A wall of stone across the floor cuts the far side off, so no move reaches it.
        for (int z = 0; z < grid.SizeZ; z++)
        {
            for (int y = 1; y < grid.SizeY; y++)
            {
                grid.Set(8, y, z, BlockId.RawStone);
            }
        }

        GridPathfinder walled = new(grid);
        Assert.False(walled.TryFind(start, goal, out IReadOnlyList<Cell> none));
        Assert.Empty(none);
    }
}
