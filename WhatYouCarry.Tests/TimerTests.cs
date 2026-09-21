using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WhatYouCarry.Core.Ai;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using WhatYouCarry.Tools.BitIdentity;
using Xunit;
using Xunit.Abstractions;

namespace WhatYouCarry.Tests;

/// <summary>The floor timer, the Overseer, and the waves (D-44 to D-46, D-407 to D-425; PR-17 exit tests 1 to 7).</summary>
/// <remarks>
/// Exit tests 5 and 6 run their seeds in parallel. Each seed plays its own loop, and Core holds no mutable static
/// state, so a run on one thread reads nothing of a run on another. The sums do not depend on the order.
/// </remarks>
public sealed class TimerTests(ITestOutputHelper output)
{
    /// <summary>An intent that does nothing on one tick.</summary>
    private static Intent Idle(SimulationLoop loop) => new(loop.Tick, 0, 0, 0, 0, 0);

    /// <summary>A content set whose every floor runs a timer of a few seconds, with no extra time on a boss floor.</summary>
    private static ContentSet WithTimer(ContentSet content, long seconds)
    {
        List<FloorTemplate> floors = [];
        foreach (FloorTemplate floor in content.Floors)
        {
            floors.Add(floor with { TimerSeconds = seconds, BossTimerSeconds = 0 });
        }

        return content with { Floors = floors };
    }

    /// <summary>A content set whose every weapon deals no damage, so a run past expiry lasts for the test.</summary>
    private static ContentSet WithHarmlessWeapons(ContentSet content)
    {
        List<WeaponDefinition> weapons = [];
        foreach (WeaponDefinition weapon in content.Weapons)
        {
            weapons.Add(weapon with { Damage = 0 });
        }

        return content with { Weapons = weapons };
    }

    /// <summary>Steps a loop with idle intents until its timer expires. The run must not end first.</summary>
    private static void IdleToExpiry(SimulationLoop loop)
    {
        while (!loop.Timer.Expired)
        {
            Assert.False(loop.Ended, $"The run ended at tick {loop.Tick} before expiry.");
            loop.Step(Idle(loop));
        }
    }

    /// <summary>The lengths of D-407: three, four, and five minutes by band, and two more on the boss floors of D-6.</summary>
    [Fact]
    public void TimerLengthsFollowTheBands()
    {
        long[] minutes = [0, 3, 3, 3, 3, 5, 4, 4, 4, 4, 6, 5, 5, 5, 5, 7];
        for (int floor = 1; floor <= 15; floor++)
        {
            FloorTimer timer = FloorTimer.For(FloorGenerator.TemplateFor(floor, TestWorld.Content), floor);
            Assert.True(minutes[floor] * 60 * SimulationLoop.TicksPerSecond == timer.Length, $"Floor {floor}: the timer is {timer.Length} ticks.");
        }

        Assert.Throws<ContextException>(() => new FloorTimer(0));
    }

    /// <summary>The countdown pauses at the stairwell, and the ticks after expiry count on there (D-140, D-417).</summary>
    [Fact]
    public void TimerCountsAfterExpiryAtTheStairwell()
    {
        FloorTimer timer = new(3);
        Assert.False(timer.Step(true));
        Assert.Equal(3, timer.Remaining);
        Assert.False(timer.Step(false));
        Assert.False(timer.Step(false));
        Assert.True(timer.Step(false));
        Assert.True(timer.Expired);
        Assert.Equal(0, timer.TicksAfterExpiry);
        Assert.False(timer.Step(true));
        Assert.False(timer.Step(false));
        Assert.Equal(2, timer.TicksAfterExpiry);
    }

    /// <summary>PR-17 exit test 1. No countdown runs while the player stands at the stairwell (D-140).</summary>
    [Fact]
    public void TimerPausesAtStairwell()
    {
        SimulationLoop loop = TestWorld.NewLoop(3UL);
        GreedyDescender policy = new(TestWorld.PeacefulContent);
        while (!StairwellTransition.IsAtStairwell(loop.Body, loop.Plan.Stairwell))
        {
            Assert.True(loop.Floor == SimulationLoop.FirstFloor && !loop.Timer.Expired, $"The descender left floor 1 or met expiry at tick {loop.Tick}.");
            loop.Step(policy.Next(loop));
        }

        long remaining = loop.Timer.Remaining;
        Assert.True(remaining > 0);
        for (int tick = 0; tick < 600; tick++)
        {
            loop.Step(Idle(loop));
            Assert.True(StairwellTransition.IsAtStairwell(loop.Body, loop.Plan.Stairwell), $"The body left the stairwell on tick {loop.Tick}.");
            Assert.Equal(remaining, loop.Timer.Remaining);
        }
    }

    /// <summary>
    /// PR-17 exit test 2. One Overseer spawns on the tick whose step takes the countdown to zero, and none before
    /// it. It spawns out of the sight of the player, with a path to the player (D-415). Over twenty seeds.
    /// </summary>
    [Fact]
    public void HunterSpawnsAtExpiry()
    {
        ContentSet content = WithTimer(TestWorld.PeacefulContent, 2);
        for (ulong seed = 1; seed <= 20; seed++)
        {
            SimulationLoop loop = new(seed, content);
            while (loop.Timer.Remaining > 1)
            {
                loop.Step(Idle(loop));
                Assert.True(loop.Hunter is null, $"Seed {seed}: a hunter before expiry at tick {loop.Tick}.");
            }

            uint expiryTick = loop.Tick;
            loop.Step(Idle(loop));
            Assert.True(loop.Timer.Expired, $"Seed {seed}.");
            Hunter hunter = Assert.IsType<Hunter>(loop.Hunter);
            Assert.Equal([new TimerEvent(TimerEventKind.Expiry, 1, expiryTick, 0, 0), new TimerEvent(TimerEventKind.HunterSpawn, 1, expiryTick, 0, 0)], loop.LastEvents);

            Vector3 spawnFeet = new(hunter.Spawn.X + 0.5f, hunter.Spawn.Y + 1.0f, hunter.Spawn.Z + 0.5f);
            Assert.False(PathWalk.Sees(loop.Grid, loop.Body.Position, spawnFeet), $"Seed {seed}: the player sees the spawn {hunter.Spawn}.");
            Assert.True(new GridPathfinder(loop.Grid).TryFind(hunter.Spawn, PathWalk.FloorCellOf(loop.Body.Position), out IReadOnlyList<Cell> _), $"Seed {seed}: no path from the spawn {hunter.Spawn}.");

            // The spawn is the farthest hidden cell by path from the player that has a path back (D-415).
            Reachability reach = Reachability.From(loop.Grid, PathWalk.FloorCellOf(loop.Body.Position));
            int spawnDistance = reach.Distance(hunter.Spawn);
            for (int y = 0; y < loop.Grid.SizeY; y++)
            {
                for (int z = 0; z < loop.Grid.SizeZ; z++)
                {
                    for (int x = 0; x < loop.Grid.SizeX; x++)
                    {
                        Cell cell = new(x, y, z);
                        if (!reach.IsReachable(cell) || reach.Distance(cell) <= spawnDistance)
                        {
                            continue;
                        }

                        bool hidden = !PathWalk.Sees(loop.Grid, loop.Body.Position, new Vector3(x + 0.5f, y + 1.0f, z + 0.5f));
                        bool pathBack = new GridPathfinder(loop.Grid).TryFind(cell, PathWalk.FloorCellOf(loop.Body.Position), out IReadOnlyList<Cell> _);
                        Assert.False(hidden && pathBack, $"Seed {seed}: the cell {cell} lies farther than the spawn {hunter.Spawn}, hidden, with a path back.");
                    }
                }
            }

            loop.Step(Idle(loop));
            Assert.Same(hunter, loop.Hunter);
        }
    }

    /// <summary>A floor with no cell out of the sight of the player is a fault of D-415, and the error names the cell.</summary>
    [Fact]
    public void AnOpenFloorHasNoHunterSpawn()
    {
        VoxelGrid grid = TestWorld.FlatFloor();
        ContextException error = Assert.Throws<ContextException>(() => Hunter.FindSpawn(grid, new GridPathfinder(grid), TestWorld.Spawn));
        Assert.Contains("D-415", error.Message, StringComparison.Ordinal);
        Assert.Contains("playerCell=", error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// PR-17 exit test 3. No hit takes health from the Overseer, and a hit staggers it by the rules of D-326, with
    /// hyper-armor through its two-handed swing (D-29, D-414).
    /// </summary>
    [Fact]
    public void HunterCannotDie()
    {
        ContentSet content = WithTimer(TestWorld.PeacefulContent, 1);
        SimulationLoop loop = new(1UL, content);
        IdleToExpiry(loop);
        Hunter hunter = Assert.IsType<Hunter>(loop.Hunter);

        for (int hit = 0; hit < 100; hit++)
        {
            hunter.TakeHit(1000000);
        }

        Assert.Equal(Core.Combat.Stagger.StaggerTicks, hunter.StaggerRemaining);
        for (int tick = 0; tick < 600 && !loop.Ended; tick++)
        {
            loop.Step(Idle(loop));
            Assert.Same(hunter, loop.Hunter);
        }

        Assert.Throws<ContextException>(() => hunter.TakeHit(-1));
        Assert.True(PickOfContent().IsTwoHanded, "The pick is two-handed, so its swing gives hyper-armor (D-29, D-413).");
    }

    /// <summary>A two-handed swing of the Overseer takes no stagger, and a hit outside it does (D-29, D-414).</summary>
    [Fact]
    public void HunterSwingHasHyperArmor()
    {
        VoxelGrid grid = TestWorld.FlatFloor();
        WeaponDefinition pick = PickOfContent();
        Hunter hunter = new(grid, new Cell(2, 0, 2), TestWorld.Content.Hunter, pick, 1);
        Vector3 player = new(3.5f, TestWorld.FloorTop, 2.5f);
        IReadOnlyList<EntityBox> none = [];
        hunter.Step(grid, new GridPathfinder(grid), player, 0, none);
        Assert.True(hunter.IsSwinging);
        hunter.TakeHit(10);
        Assert.Equal(0, hunter.StaggerRemaining);
        Assert.True(hunter.IsSwinging);
    }

    /// <summary>The pick of the content set.</summary>
    private static WeaponDefinition PickOfContent()
    {
        foreach (WeaponDefinition weapon in TestWorld.Content.Weapons)
        {
            if (weapon.Id == TestWorld.Content.Hunter.Weapon)
            {
                return weapon;
            }
        }

        throw new InvalidOperationException("The content set holds no pick.");
    }

    /// <summary>
    /// PR-17 exit test 4. The speed one minute after expiry exceeds the speed at expiry, the speed passes the sprint
    /// at 70 seconds, and the Overseer of a loop moves at the curve (D-408, D-423).
    /// </summary>
    [Fact]
    public void HunterSpeedGrows()
    {
        HunterDefinition overseer = TestWorld.Content.Hunter;
        Assert.Equal(3.5f, overseer.SpeedMetresPerSecond(0));
        Assert.True(overseer.SpeedMetresPerSecond(3600) > overseer.SpeedMetresPerSecond(0));
        Assert.True(overseer.SpeedMetresPerSecond(4200) >= PlayerBody.SprintSpeed);
        Assert.True(overseer.SpeedMetresPerSecond(4199) < PlayerBody.SprintSpeed);
        Assert.True(overseer.SpeedMetresPerSecond(1) > overseer.SpeedMetresPerSecond(0), "The speed rises on every tick (D-423).");

        ContentSet content = WithHarmlessWeapons(WithTimer(TestWorld.PeacefulContent, 1));
        SimulationLoop loop = new(2UL, content);
        IdleToExpiry(loop);
        loop.Step(Idle(loop));
        Hunter hunter = Assert.IsType<Hunter>(loop.Hunter);
        float atExpiry = hunter.Speed;
        while (loop.Timer.TicksAfterExpiry < 3600 + 1)
        {
            Assert.False(loop.Ended);
            loop.Step(Idle(loop));
        }

        Assert.True(hunter.Speed > atExpiry, $"The speed is {hunter.Speed} one minute after expiry and {atExpiry} at expiry.");
    }

    /// <summary>
    /// PR-17 exit test 5. The timer tester waits at the entry, and over one thousand seeds, with the enemy spawns and
    /// the waves off, every run ends in death with the cause <c>overseer</c> (D-411, D-421). The content set with no
    /// enemy family is the switch: a floor with no family holds no enemy and no post for a wave (D-418).
    /// </summary>
    [Fact]
    public void TimerTesterAlwaysDies()
    {
        uint expiryTick = (uint)(FloorGenerator.TemplateFor(1, TestWorld.Content).TimerSeconds * SimulationLoop.TicksPerSecond) - 1;
        TimerEvent expiry = new(TimerEventKind.Expiry, 1, expiryTick, 0, 0);
        ConcurrentBag<string> failures = [];
        Parallel.For(1, 1001, index =>
        {
            ulong seed = (ulong)index;
            BotRunResult result = BotRun.Play(new TimerTester(), seed, TestWorld.PeacefulContent);
            bool dies = result.End == BotRunEnd.Death && result.Cause == TestWorld.Content.Hunter.Id;
            if (!dies || result.FloorsReached != 1 || !HasEvent(result.Events, expiry))
            {
                failures.Add($"Seed {seed}: the run ended as {result.End} with the cause '{result.Cause}' on floor {result.FloorsReached} at tick {result.Ticks}. {result.Error}");
            }
        });

        Assert.True(failures.IsEmpty, string.Join("\n", failures));
    }

    /// <summary>Answers whether a list of timer events holds one event.</summary>
    private static bool HasEvent(IReadOnlyList<TimerEvent> events, TimerEvent wanted)
    {
        foreach (TimerEvent timerEvent in events)
        {
            if (timerEvent == wanted)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// PR-17 exit test 6. Over one thousand seeds, a floor counts when the greedy descender completes it or its timer
    /// expires. The timer expires on under 5 percent of at least 500 such floors (D-412). A floor on which the
    /// policy dies before expiry does not count.
    /// </summary>
    [Fact]
    public void GreedyDescenderRarelyMeetsHunter()
    {
        int counted = 0;
        int expired = 0;
        Parallel.For(1, 1001, index =>
        {
            ulong seed = (ulong)index;
            GreedyDescender policy = new(TestWorld.Content);
            SimulationLoop loop = new(seed, TestWorld.Content);
            while (!loop.Ended)
            {
                if (loop.Timer.Expired)
                {
                    Interlocked.Increment(ref expired);
                    Interlocked.Increment(ref counted);
                    break;
                }

                int floor = loop.Floor;
                loop.Step(policy.Next(loop));
                if (loop.Floor != floor || loop.End == RunEnd.Ascend)
                {
                    Interlocked.Increment(ref counted);
                }
            }
        });

        output.WriteLine($"The timer expired on {expired} of {counted} floors.");
        Assert.True(counted >= 500, $"{counted} floors counted.");
        Assert.True(expired * 20 < counted, $"The timer expired on {expired} of {counted} floors.");
    }

    /// <summary>
    /// PR-17 exit test 7. A record that runs past expiry, with the Overseer and three waves, replays to one state
    /// hash, and the live run gives the same hash. The bit-identity sweep folds the same run on three platforms, so
    /// the known answer holds it (G-9).
    /// </summary>
    [Fact]
    public void TimerIsDeterministic()
    {
        ContentSet content = BitIdentitySweep.SweepContent();
        const ulong Seed = 0x5EED1234ABCD9876UL;
        List<byte> bytes = [];
        RunRecorder recorder = new(new ListSink(bytes), RunRecord.NewHeader(content.Hash, Seed));
        SimulationLoop live = new(Seed, content);
        Random random = new(17);
        for (uint tick = 0; tick < 600; tick++)
        {
            Intent intent = new(tick, (short)random.Next(-200, 201), 0, (sbyte)random.Next(-127, 128), (sbyte)random.Next(-127, 128), 0);
            recorder.Record(intent);
            live.Step(intent);
        }

        Assert.NotNull(live.Hunter);
        Assert.Equal(3, live.Escalation.Waves);
        Assert.False(live.Ended);
        ReplayResult first = RunReplayer.Replay(bytes, content, new JsonlLogger(new LineSink()));
        ReplayResult second = RunReplayer.Replay(bytes, content, new JsonlLogger(new LineSink()));
        Assert.Equal(live.Hash(), first.Loop.Hash());
        Assert.Equal(first.Loop.Hash(), second.Loop.Hash());
    }

    /// <summary>A log sink that keeps the lines of a replay.</summary>
    private sealed class LineSink : ILogSink
    {
        public List<string> Lines { get; } = [];

        public void Write(string line)
        {
            this.Lines.Add(line);
        }
    }

    /// <summary>A record sink that keeps the bytes in one list.</summary>
    private sealed class ListSink(List<byte> bytes) : IRunRecordSink
    {
        public void Append(byte[] part)
        {
            bytes.AddRange(part);
        }
    }

    /// <summary>
    /// A wave spawns n enemies at the posts out of sight, from a cursor that goes on, up to the cap of living wave
    /// enemies, and the wave number rises when the cap cuts a wave (D-410, D-418, D-424).
    /// </summary>
    [Fact]
    public void WavesGrowAndFillToTheCap()
    {
        VoxelGrid grid = WalledFloor();
        FloorTemplate template = FloorGenerator.TemplateFor(1, TestWorld.Content) with { WaveIntervalSeconds = 1, WaveCap = 4 };
        EnemyDefinition family = TestWorld.Content.Enemies[0];
        EnemySpawn[] posts = [new(new Cell(12, 0, 2), family, 1), new(new Cell(12, 0, 6), family, 1), new(new Cell(3, 0, 6), family, 1)];
        Vector3 player = new(3.5f, TestWorld.FloorTop, 2.5f);
        Escalation escalation = new(template);

        Assert.Equal(0, escalation.Step(grid, 0, 0, posts, player).Wave);
        Assert.Equal(0, escalation.Step(grid, 59, 0, posts, player).Wave);
        WaveSpawns first = escalation.Step(grid, 60, 0, posts, player);
        Assert.Equal(1, first.Wave);
        Assert.Equal([posts[0]], first.Posts);

        // The third post stands in the sight of the player, so the cursor goes past it (D-418).
        WaveSpawns second = escalation.Step(grid, 120, 1, posts, player);
        Assert.Equal(2, second.Wave);
        Assert.Equal([posts[1], posts[0]], second.Posts);
        Assert.Equal(0, second.Skipped);

        // Three are alive and the cap is four, so wave 3 spawns one (D-424).
        WaveSpawns third = escalation.Step(grid, 180, 3, posts, player);
        Assert.Equal(3, third.Wave);
        Assert.Single(third.Posts);
        WaveSpawns full = escalation.Step(grid, 240, 4, posts, player);
        Assert.Equal(4, full.Wave);
        Assert.Empty(full.Posts);
        Assert.Equal(0, full.Skipped);

        // A floor with no post out of sight skips the spawns of a wave, and the result counts them (D-418).
        WaveSpawns skipped = new Escalation(template).Step(grid, 60, 0, [posts[2]], player);
        Assert.Empty(skipped.Posts);
        Assert.Equal(1, skipped.Skipped);
    }

    /// <summary>A flat floor of 16 by 16 blocks with a stone wall at x 8 that hides one half from the other.</summary>
    private static VoxelGrid WalledFloor()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 6);
        for (int z = 0; z < 16; z++)
        {
            for (int y = 1; y < 6; y++)
            {
                grid.Set(8, y, z, BlockId.RawStone);
            }
        }

        return grid;
    }

    /// <summary>A wave enemy hunts from its spawn and never gives up, and an enemy of the plan starts asleep (D-400, D-419).</summary>
    [Fact]
    public void WaveBrainsNeverGiveUp()
    {
        VoxelGrid grid = WalledFloor();
        EnemyDefinition family = TestWorld.Content.Enemies[0];
        WeaponDefinition weapon = SimulationLoop.MainWeapon(TestWorld.Content);
        Vector3 player = new(3.5f, TestWorld.FloorTop, 2.5f);
        GridPathfinder pathfinder = new(grid);
        IReadOnlyList<EntityBox> none = [];

        HumanoidBrain wave = new(new Enemy(grid, new Vector3(12.5f, 1.0f, 2.5f), family, weapon, 1), new Cell(12, 0, 2), true);
        HumanoidBrain plan = new(new Enemy(grid, new Vector3(12.5f, 1.0f, 6.5f), family, weapon, 2), new Cell(12, 0, 6), false);
        Assert.True(wave.IsHunting);
        Assert.False(plan.IsHunting);
        for (long tick = 0; tick < family.GiveUpTicks * 2; tick++)
        {
            wave.Step(grid, pathfinder, player, none);
            plan.Step(grid, pathfinder, player, none);
            Assert.True(wave.IsHunting, $"The wave enemy gave up on tick {tick}.");
            Assert.False(plan.IsHunting, $"The plan enemy woke with no sight on tick {tick}.");
        }
    }

    /// <summary>
    /// A loop past expiry adds the wave enemies as relentless enemies at the posts, with new owner ids, and logs each
    /// wave (D-410, D-419). No weapon deals damage, so the run lasts.
    /// </summary>
    [Fact]
    public void TheLoopSpawnsTheWaves()
    {
        ContentSet content = WithHarmlessWeapons(WithTimer(TestWorld.Content, 1));
        for (ulong seed = 1; seed <= 10; seed++)
        {
            SimulationLoop loop = new(seed, content);
            int planEnemies = loop.Enemies.Count;
            if (planEnemies == 0)
            {
                continue;
            }

            IdleToExpiry(loop);
            List<TimerEvent> waves = [];
            while (loop.Timer.TicksAfterExpiry < 3 * 30 * SimulationLoop.TicksPerSecond && !loop.Ended)
            {
                loop.Step(Idle(loop));
                foreach (TimerEvent timerEvent in loop.LastEvents)
                {
                    if (timerEvent.Kind == TimerEventKind.Wave)
                    {
                        waves.Add(timerEvent);
                    }
                }
            }

            if (loop.Ended)
            {
                Assert.Equal(RunEnd.Death, loop.End);
                continue;
            }

            Assert.Equal(3, waves.Count);
            int spawned = 0;
            foreach (TimerEvent wave in waves)
            {
                spawned += wave.Count;
            }

            Assert.Equal(planEnemies + spawned, loop.Enemies.Count);
            for (int index = planEnemies; index < loop.Enemies.Count; index++)
            {
                Assert.True(loop.Brains[index].IsRelentless);
                Assert.True(loop.Enemies[index].Owner > Assert.IsType<Hunter>(loop.Hunter).Owner);
            }

            return;
        }

        Assert.Fail("No seed of 1 to 10 gave a floor with enemies and a run past three waves.");
    }

    /// <summary>A death carries the id of the family whose hit took the last health (D-411).</summary>
    [Fact]
    public void ADeathCarriesItsCause()
    {
        int deaths = 0;
        for (ulong seed = 1; seed <= 20; seed++)
        {
            BotRunResult result = BotRun.Play(new FullClearer(TestWorld.Content), seed, TestWorld.Content);
            if (result.End != BotRunEnd.Death)
            {
                Assert.Equal(string.Empty, result.Cause);
                continue;
            }

            deaths++;
            Assert.True(result.Cause == TestWorld.Content.Enemies[0].Id || result.Cause == TestWorld.Content.Hunter.Id, $"Seed {seed}: the cause is '{result.Cause}'.");
        }

        Assert.True(deaths > 0, "No full clearer run of 20 seeds died, so the test read no cause.");
    }

    /// <summary>The timer tester stands still and promises no progress, so its run never reads softlock (D-420, D-421).</summary>
    [Fact]
    public void TimerTesterStandsStill()
    {
        TimerTester tester = new();
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        Assert.False(tester.PromisesProgress);
        Assert.Equal(TimerTester.PolicyName, tester.Name);
        Assert.Equal(Idle(loop), tester.Next(loop));
    }
}
