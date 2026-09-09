using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The projectile simulation, the arc solver, and the shot of the attack bit (G-6, D-159, D-231, D-265 to D-268; PR-10 exit tests 1 to 6).</summary>
public sealed class ProjectileTests
{
    /// <summary>The owner id of a test source, which owns no entity box.</summary>
    private const int Source = -1;

    /// <summary>A sink that keeps the record in memory.</summary>
    private sealed class MemorySink : IRunRecordSink
    {
        public List<byte> Bytes { get; } = [];

        public void Append(byte[] bytes)
        {
            this.Bytes.AddRange(bytes);
        }
    }

    private sealed class CollectingSink : ILogSink
    {
        public List<string> Lines { get; } = [];

        public void Write(string line)
        {
            this.Lines.Add(line);
        }
    }

    /// <summary>The index of a definition of the repository content by id. An absent id is a defect of the test.</summary>
    private static int Index(string id)
    {
        for (int index = 0; index < TestWorld.Content.Projectiles.Count; index++)
        {
            if (TestWorld.Content.Projectiles[index].Id == id)
            {
                return index;
            }
        }

        throw new InvalidOperationException($"The content set has no projectile definition '{id}'.");
    }

    private static ProjectileDefinition Definition(string id)
    {
        return TestWorld.Content.Projectiles[Index(id)];
    }

    /// <summary>A simulation over a grid with the definitions of the repository content.</summary>
    private static ProjectileSimulation Simulation(VoxelGrid grid)
    {
        return new ProjectileSimulation(grid, TestWorld.Content.Projectiles);
    }

    private static float Distance(Vector3 first, Vector3 second)
    {
        return (first - second).Length();
    }

    /// <summary>The angle between two directions, in degrees, from a double reference.</summary>
    private static double DegreesBetween(Vector3 first, Vector3 second)
    {
        double dot = ((double)first.X * second.X) + ((double)first.Y * second.Y) + ((double)first.Z * second.Z);
        double lengths = Math.Sqrt(((double)first.X * first.X) + ((double)first.Y * first.Y) + ((double)first.Z * first.Z))
            * Math.Sqrt(((double)second.X * second.X) + ((double)second.Y * second.Y) + ((double)second.Z * second.Z));
        return Math.Acos(Math.Clamp(dot / lengths, -1.0, 1.0)) * 180.0 / Math.PI;
    }

    /// <summary>The test-only set of D-149 holds the four extremes, and the two plain definitions come first in path order (D-265).</summary>
    [Fact]
    public void TheTestOnlySetHoldsTheExtremes()
    {
        Assert.Equal("arrow", TestWorld.Content.Projectiles[0].Id);
        Assert.Equal(100, TestWorld.Content.Projectiles[0].SpreadHundredths);

        ProjectileDefinition slow = Definition("test-slow-arc");
        ProjectileDefinition fast = Definition("test-fast-flat");
        ProjectileDefinition longLife = Definition("test-long-life");
        ProjectileDefinition wide = Definition("test-wide-spread");
        foreach (ProjectileDefinition other in TestWorld.Content.Projectiles)
        {
            Assert.True(other.SpeedCentimetres >= slow.SpeedCentimetres, $"'{other.Id}' is slower than the slow arc.");
            Assert.True(other.SpeedCentimetres <= fast.SpeedCentimetres, $"'{other.Id}' is faster than the fast flat shot.");
            Assert.True(other.LifetimeTicks <= longLife.LifetimeTicks, $"'{other.Id}' lives longer than the long life.");
            Assert.True(other.SpreadHundredths <= wide.SpreadHundredths, $"'{other.Id}' spreads wider than the wide spread.");
        }

        Assert.Equal(0, fast.GravityScalePercent);
        Assert.Equal(300, slow.GravityScalePercent);
    }

    /// <summary>
    /// PR-10 exit test 1. The fastest definition, fired at a one-block wall from ten thousand random positions
    /// and directions, hits the wall face or the floor and never holds a position beyond the wall. A failure
    /// names its seed (D-66).
    /// </summary>
    [Fact]
    public void NoTunnelThroughMinimumWall()
    {
        const int wallX = 16;
        VoxelGrid grid = TestWorld.FlatFloor(32, 16);
        for (int y = 1; y < 16; y++)
        {
            for (int z = 0; z < 32; z++)
            {
                grid.Set(wallX, y, z, BlockId.RawStone);
            }
        }

        int fast = Index("test-fast-flat");
        Rng spread = Rng.ForStream(1UL, RngStream.Projectile);
        for (int seed = 1; seed <= 10000; seed++)
        {
            Random random = new(seed);
            Vector3 origin = new((float)(1.0 + (random.NextDouble() * 14.0)), (float)(1.5 + (random.NextDouble() * 13.0)), (float)(1.0 + (random.NextDouble() * 30.0)));
            Vector3 direction = new((float)(0.05 + random.NextDouble()), (float)((random.NextDouble() * 2.0) - 1.0), (float)((random.NextDouble() * 2.0) - 1.0));

            ProjectileSimulation simulation = Simulation(grid);
            simulation.Fire(fast, Source, origin, direction, spread);
            int ticks = 0;
            while (simulation.Live.Count > 0)
            {
                IReadOnlyList<ProjectileEnd> ends = simulation.Step([]);
                ticks++;
                foreach (Projectile live in simulation.Live)
                {
                    Assert.True(live.Position.X < wallX, $"Seed {seed}: the projectile is at {live.Position}, past the wall at x = {wallX}, after {ticks} ticks.");
                }

                foreach (ProjectileEnd end in ends)
                {
                    Assert.Equal(ProjectileEndKind.Grid, end.Kind);
                    Assert.True(end.Point.X <= wallX + 1e-3f, $"Seed {seed}: the projectile ended at {end.Point}, past the wall at x = {wallX}.");
                }

                Assert.True(ticks <= 200, $"Seed {seed}: the projectile flew {ticks} ticks in a closed room.");
            }
        }
    }

    /// <summary>PR-10 exit test 2. Every projectile of every definition ends by a hit or by its lifetime, inside its lifetime budget, and none flies on after it.</summary>
    [Fact]
    public void EveryProjectileTerminates()
    {
        VoxelGrid grid = TestWorld.FlatFloor(64, 30);
        ProjectileSimulation simulation = Simulation(grid);
        Rng spread = Rng.ForStream(2UL, RngStream.Projectile);
        Random random = new(2);
        long longest = 0;
        for (int definition = 0; definition < TestWorld.Content.Projectiles.Count; definition++)
        {
            longest = Math.Max(longest, TestWorld.Content.Projectiles[definition].LifetimeTicks);
            for (int shot = 0; shot < 200; shot++)
            {
                Vector3 direction = new((float)((random.NextDouble() * 2.0) - 1.0), (float)((random.NextDouble() * 2.0) - 1.0), (float)((random.NextDouble() * 2.0) - 1.0));
                if (direction.Length() == 0.0f)
                {
                    direction = new Vector3(0.0f, 1.0f, 0.0f);
                }

                simulation.Fire(definition, Source, new Vector3(32.0f, 15.0f, 32.0f), direction, spread);
            }
        }

        int fired = simulation.Live.Count;
        int ended = 0;
        for (int tick = 1; tick <= longest + 1; tick++)
        {
            foreach (ProjectileEnd end in simulation.Step([]))
            {
                ended++;
                long lifetime = TestWorld.Content.Projectiles[end.Projectile.Definition].LifetimeTicks;
                Assert.True(end.Projectile.Age <= lifetime + 1, $"A projectile of definition {end.Projectile.Definition} ended at age {end.Projectile.Age}, past its lifetime {lifetime}.");
                Assert.True(end.Kind == ProjectileEndKind.Grid || end.Kind == ProjectileEndKind.Lifetime, $"A projectile ended by {end.Kind} with no entity box in the room.");
                Assert.Equal(-1, end.EntityIndex);
            }
        }

        Assert.Equal(fired, ended);
        Assert.Empty(simulation.Live);
    }

    /// <summary>
    /// PR-10 exit test 3. Over one thousand reachable targets, a shot along the solved direction passes within one
    /// block of the target. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void ArcSolverReachesTarget()
    {
        int definition = Index("test-long-life");
        ProjectileDefinition shape = Definition("test-long-life");
        float speed = shape.SpeedCentimetres / 100.0f;
        float gravity = PlayerBody.Gravity * (shape.GravityScalePercent / 100.0f);
        VoxelGrid grid = TestWorld.FlatFloor(128, 32);
        Vector3 origin = new(64.0f, 12.0f, 64.0f);
        Rng spread = Rng.ForStream(3UL, RngStream.Projectile);

        int reached = 0;
        for (int seed = 1; reached < 1000; seed++)
        {
            Random random = new(seed);
            double angle = random.NextDouble() * 2.0 * Math.PI;
            double reach = 4.0 + (random.NextDouble() * 30.0);
            Vector3 target = origin + new Vector3((float)(Math.Cos(angle) * reach), (float)((random.NextDouble() * 12.0) - 6.0), (float)(Math.Sin(angle) * reach));
            ArcSolution solution = ArcSolver.Solve(speed, gravity, origin, target);
            if (!solution.Reachable)
            {
                continue;
            }

            reached++;
            Assert.InRange(solution.Direction.Length(), 0.999f, 1.001f);
            ProjectileSimulation simulation = Simulation(grid);
            simulation.Fire(definition, Source, origin, solution.Direction, spread);
            float nearest = Distance(origin, target);
            while (simulation.Live.Count > 0)
            {
                simulation.Step([]);
                foreach (Projectile live in simulation.Live)
                {
                    nearest = Math.Min(nearest, Distance(live.Position, target));
                }
            }

            Assert.True(nearest <= 1.0f, $"Seed {seed}: the shot at {target} from {origin} came no nearer than {nearest} blocks.");
        }
    }

    /// <summary>PR-10 exit test 4. A target past the range, or too high, gets a clear report and no direction that claims to reach it (T-2).</summary>
    [Fact]
    public void ArcSolverReportsUnreachable()
    {
        Vector3 origin = new(0.0f, 0.0f, 0.0f);
        const float speed = 20.0f;
        const float gravity = 10.0f;

        // The level range is v^2 / g, which is 40 meters here.
        Assert.False(ArcSolver.Solve(speed, gravity, origin, new Vector3(100.0f, 0.0f, 0.0f)).Reachable);
        Assert.True(ArcSolver.Solve(speed, gravity, origin, new Vector3(39.0f, 0.0f, 0.0f)).Reachable);

        // The apex is v^2 / (2 g), which is 20 meters here.
        Assert.False(ArcSolver.Solve(speed, gravity, origin, new Vector3(0.0f, 30.0f, 0.0f)).Reachable);
        ArcSolution up = ArcSolver.Solve(speed, gravity, origin, new Vector3(0.0f, 10.0f, 0.0f));
        Assert.True(up.Reachable);
        Assert.Equal(new Vector3(0.0f, 1.0f, 0.0f), up.Direction);
        Assert.True(ArcSolver.Solve(speed, gravity, origin, new Vector3(0.0f, -100.0f, 0.0f)).Reachable);

        // Without gravity every target but the origin is a straight line.
        ArcSolution flat = ArcSolver.Solve(speed, 0.0f, origin, new Vector3(0.0f, 0.0f, -300.0f));
        Assert.True(flat.Reachable);
        Assert.Equal(new Vector3(0.0f, 0.0f, -1.0f), flat.Direction);
        Assert.False(ArcSolver.Solve(speed, 0.0f, origin, origin).Reachable);
    }

    /// <summary>
    /// PR-10 exit test 5. A run with shots on a dug floor replays to the live end hash, and a run without shots
    /// gives another hash. The bit-identity sweep fires shots on its own, so the three platforms assert the same.
    /// </summary>
    [Fact]
    public void ProjectilesAreDeterministic()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            MemorySink sink = new();
            RunRecorder recorder = new(sink, RunRecord.NewHeader(TestWorld.Content.Hash, (ulong)seed));
            SimulationLoop live = TestWorld.NewLoop((ulong)seed);
            SimulationLoop silent = TestWorld.NewLoop((ulong)seed);
            Random random = new(seed);
            int shots = 0;
            int flying = 0;
            for (uint tick = 0; tick < 240; tick++)
            {
                ushort buttons = tick % 20 == 0 ? Button.Attack : (ushort)0;
                Intent intent = new(tick, (short)random.Next(-300, 301), (short)random.Next(-100, 101), 0, 0, buttons);
                recorder.Record(intent);
                live.Step(intent);
                silent.Step(intent with { Buttons = 0 });
                shots += live.LastEnds.Count;
                if (live.Projectiles.Live.Count > 0)
                {
                    // A projectile in flight is state, so the two hashes differ while one is up.
                    flying++;
                    Assert.NotEqual(live.Hash(), silent.Hash());
                }
            }

            shots += live.Projectiles.Live.Count;
            Assert.Equal(12, shots);
            Assert.True(flying > 0, $"Seed {seed}: no projectile was ever in flight.");
            Assert.Empty(silent.Projectiles.Live);

            ReplayResult replay = RunReplayer.Replay(sink.Bytes, TestWorld.Content, new JsonlLogger(new CollectingSink()));
            Assert.True(live.Hash().Value == replay.Loop.Hash().Value, $"Seed {seed}: the live hash is {live.Hash()}, and the replay gives {replay.Loop.Hash()}.");
            Assert.Equal(live.Projectiles.Live.Count, replay.Loop.Projectiles.Live.Count);
        }
    }

    /// <summary>PR-10 exit test 6. A shot at a player box registers a hit on the box at its near face, and a shot of the owner of the box passes through it.</summary>
    [Fact]
    public void EntityBoxHit()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        PlayerBody body = new(grid, new Vector3(8.5f, 1.0f, 8.5f));
        EntityBox[] boxes = [new EntityBox(SimulationLoop.PlayerOwner, body.Box)];
        int fast = Index("test-fast-flat");
        Rng spread = Rng.ForStream(4UL, RngStream.Projectile);

        ProjectileSimulation simulation = Simulation(grid);
        simulation.Fire(fast, Source, new Vector3(2.5f, 1.9f, 8.5f), new Vector3(1.0f, 0.0f, 0.0f), spread);
        List<ProjectileEnd> ends = [];
        while (simulation.Live.Count > 0)
        {
            ends.AddRange(simulation.Step(boxes));
        }

        ProjectileEnd hit = Assert.Single(ends);
        Assert.Equal(ProjectileEndKind.Entity, hit.Kind);
        Assert.Equal(0, hit.EntityIndex);
        Assert.InRange(hit.Point.X, body.Box.Min.X - 1e-4f, body.Box.Min.X + 1e-4f);
        Assert.Equal(1.9f, hit.Point.Y);

        ProjectileSimulation own = Simulation(grid);
        own.Fire(fast, SimulationLoop.PlayerOwner, new Vector3(2.5f, 1.9f, 8.5f), new Vector3(1.0f, 0.0f, 0.0f), spread);
        List<ProjectileEnd> ownEnds = [];
        while (own.Live.Count > 0)
        {
            ownEnds.AddRange(own.Step(boxes));
        }

        Assert.Equal(ProjectileEndKind.Grid, Assert.Single(ownEnds).Kind);
    }

    /// <summary>The spread stays inside the cone of the definition, spreads at all, and a zero spread keeps the direction exactly (D-266).</summary>
    [Fact]
    public void SpreadStaysInsideTheCone()
    {
        VoxelGrid grid = TestWorld.FlatFloor(64, 30);
        ProjectileSimulation simulation = Simulation(grid);
        Rng spread = Rng.ForStream(5UL, RngStream.Projectile);
        Vector3 axis = new(0.0f, 0.0f, -1.0f);
        int wide = Index("test-wide-spread");
        double widest = 0.0;
        for (int shot = 0; shot < 1000; shot++)
        {
            simulation.Fire(wide, Source, new Vector3(32.0f, 15.0f, 32.0f), axis, spread);
        }

        foreach (Projectile projectile in simulation.Live)
        {
            double degrees = DegreesBetween(projectile.Velocity, axis);
            widest = Math.Max(widest, degrees);

            // A yaw offset and a pitch offset of at most 15 degrees each stay inside 22 degrees together.
            Assert.True(degrees <= 22.0, $"A shot spread by {degrees} degrees, past the cone.");
            Assert.InRange(projectile.Velocity.Length(), 39.99f, 40.01f);
        }

        Assert.True(widest > 10.0, $"The widest of one thousand shots spread {widest} degrees.");

        ProjectileSimulation exact = Simulation(grid);
        exact.Fire(Index("test-fast-flat"), Source, new Vector3(32.0f, 15.0f, 32.0f), new Vector3(3.0f, 0.0f, -4.0f), spread);
        Assert.Equal(new Vector3(180.0f, 0.0f, -240.0f), exact.Live[0].Velocity);
    }

    /// <summary>The attack bit fires once per press: a held bit fires once, and a release and a press fire again (D-267).</summary>
    [Fact]
    public void TheAttackBitFiresOncePerPress()
    {
        SimulationLoop loop = TestWorld.NewLoop(6UL);
        int shots = 0;
        ushort[] presses = [Button.Attack, Button.Attack, Button.Attack, 0, Button.Attack, 0, 0, Button.Attack | Button.Jump];
        for (uint tick = 0; tick < presses.Length; tick++)
        {
            loop.Step(new Intent(tick, 0, 0, 0, 0, presses[tick]));
            shots += loop.LastEnds.Count;
        }

        shots += loop.Projectiles.Live.Count;
        Assert.Equal(3, shots);
    }

    /// <summary>A shot starts at the shoulder point of the camera and flies toward the crosshair hit, and it is the first definition of the content set (D-265, D-268).</summary>
    [Fact]
    public void TheShotStartsAtTheShoulder()
    {
        SimulationLoop loop = TestWorld.NewLoop(7UL);
        loop.Step(new Intent(0U, 0, 0, 0, 0, 0));
        Core.Camera.CameraPose pose = loop.Camera();
        loop.Step(new Intent(1U, 0, 0, 0, 0, Button.Attack));

        Projectile shot = loop.Projectiles.Live.Count > 0 ? loop.Projectiles.Live[0] : loop.LastEnds[0].Projectile;
        Assert.Equal(0, shot.Definition);
        Assert.Equal(SimulationLoop.PlayerOwner, shot.Owner);
        Assert.True(Distance(shot.Position, pose.Shoulder) <= 1.0f, $"The shot is at {shot.Position}, far from the shoulder {pose.Shoulder}.");

        float speed = TestWorld.Content.Projectiles[0].SpeedCentimetres / 100.0f;
        Assert.InRange(shot.Velocity.Length(), speed - 0.5f, speed + 0.5f);
        Assert.True(DegreesBetween(shot.Velocity, pose.Forward) < 15.0, $"The shot flies along {shot.Velocity}, away from the look {pose.Forward}.");
    }

    /// <summary>A content set without a projectile definition makes the attack bit an error that names D-265, and a shot of an index outside the list is an error too (T-2).</summary>
    [Fact]
    public void AShotWithoutADefinitionIsAnError()
    {
        ContentSet none = TestWorld.Content with { Projectiles = [] };
        SimulationLoop loop = new(8UL, none);
        loop.Step(new Intent(0U, 0, 0, 0, 0, Button.Jump));
        ContextException error = Assert.Throws<ContextException>(() => loop.Step(new Intent(1U, 0, 0, 0, 0, Button.Attack)));
        Assert.Contains("D-265", error.Message, StringComparison.Ordinal);

        ProjectileSimulation simulation = Simulation(TestWorld.FlatFloor());
        Rng spread = Rng.ForStream(9UL, RngStream.Projectile);
        Assert.Throws<ContextException>(() => simulation.Fire(99, Source, new Vector3(4.5f, 2.0f, 4.5f), new Vector3(1.0f, 0.0f, 0.0f), spread));
        Assert.Throws<ContextException>(() => simulation.Fire(0, Source, new Vector3(4.5f, 2.0f, 4.5f), new Vector3(0.0f, 0.0f, 0.0f), spread));
    }

    /// <summary>The segment test enters a box at the near face, reads a start inside as zero, and misses a box beside the segment.</summary>
    [Fact]
    public void TheSegmentTestFindsTheNearFace()
    {
        Aabb box = new(new Vector3(4.0f, 0.0f, 4.0f), new Vector3(5.0f, 2.0f, 5.0f));
        Assert.Equal(0.5f, ProjectileSimulation.SegmentEntry(new Vector3(2.0f, 1.0f, 4.5f), new Vector3(4.0f, 0.0f, 0.0f), box));
        Assert.Equal(0.0f, ProjectileSimulation.SegmentEntry(new Vector3(4.5f, 1.0f, 4.5f), new Vector3(4.0f, 0.0f, 0.0f), box));
        Assert.Equal(-1.0f, ProjectileSimulation.SegmentEntry(new Vector3(2.0f, 3.0f, 4.5f), new Vector3(4.0f, 0.0f, 0.0f), box));
        Assert.Equal(-1.0f, ProjectileSimulation.SegmentEntry(new Vector3(2.0f, 1.0f, 4.5f), new Vector3(1.0f, 0.0f, 0.0f), box));
        Assert.Equal(-1.0f, ProjectileSimulation.SegmentEntry(new Vector3(6.0f, 1.0f, 4.5f), new Vector3(4.0f, 0.0f, 0.0f), box));
    }

    /// <summary>The projectiles end with the floor: a descent starts an empty simulation on the next floor.</summary>
    [Fact]
    public void TheProjectilesEndWithTheFloor()
    {
        SimulationLoop loop = TestWorld.NewLoop(10UL);
        loop.Step(new Intent(0U, 0, 0, 0, 0, Button.Attack));
        Assert.Same(loop.Grid, loop.Plan.Grid);
        Assert.Equal(TestWorld.Content.Projectiles.Count, loop.Projectiles.Definitions.Count);
    }
}
