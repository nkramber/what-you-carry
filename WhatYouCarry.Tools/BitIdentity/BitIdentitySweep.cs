using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Combat;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Tools.BitIdentity;

/// <summary>
/// A fixed run of the RNG, of DetMath, of the floor generator, of one recorded run through the replay, of the
/// camera over that run, of a projectile run, of the sword arc, and of a body, the rays, and the search on ramp
/// courses, folded into one state hash (D-69, D-71). Two platforms that give the same hash agree on every bit of
/// all eight.
/// </summary>
/// <remarks>
/// <para>
/// Every number here is a constant of this file, so the sweep takes no input and reads no content file. The
/// content set of the sweep is a floor template and two chamber kinds that this file declares. The hash changes
/// only when this file changes or when Core changes its numbers. The `bit-identity` CI job runs it on Linux
/// x64, Windows x64, and macOS arm64 and compares the three results.
/// </para>
/// <para>
/// A change to the hash is a change to the simulation. A deliberate one updates
/// <c>BitIdentityKnownAnswer</c> in the test project, and G-20 asks the review to confirm it.
/// </para>
/// </remarks>
public static class BitIdentitySweep
{
    /// <summary>The run seed of the sweep. It has no meaning beyond staying the same.</summary>
    public const ulong RunSeed = 0x5EED1234ABCD9876UL;

    /// <summary>The count of numbers taken from each stream.</summary>
    public const int DrawsPerStream = 1024;

    /// <summary>The count of angle samples across [-4 pi, 4 pi].</summary>
    public const int AngleSamples = 2048;

    /// <summary>The width of one side of the Atan2 grid.</summary>
    public const int AtanGridSide = 64;

    /// <summary>The count of frames in the recorded run of the sweep (PR-6 exit test 7).</summary>
    public const int ReplayFrames = 600;

    /// <summary>The content hash that the sweep record carries. It has no meaning beyond its shape (D-221).</summary>
    public const string ReplayContentHash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    /// <summary>The count of shots of the projectile run of the sweep (D-320).</summary>
    public const int ProjectileShots = 16;

    /// <summary>The count of ticks of the body on each ramp course of the sweep (PR-64 exit test 6).</summary>
    public const int RampTicks = 180;

    /// <summary>The rises of the ramp courses, in the order of D-367. The list is explicit, so the sweep reads no enum metadata.</summary>
    private static readonly RampRise[] SweptRises = [RampRise.PlusX, RampRise.MinusX, RampRise.PlusZ, RampRise.MinusZ];

    /// <summary>The count of yaws at which the sweep folds every step of the sword arc (D-325).</summary>
    public const int ArcYaws = 8;

    /// <summary>The count of boxes on the ring around the feet that the sword arc is tested against (D-325).</summary>
    public const int ArcBoxes = 24;

    /// <summary>The floors that the sweep digs and folds, one per band of the sweep content (PR-9 exit test 7, PR-59 exit test 4).</summary>
    private static readonly int[] SweptFloors = [1, 2, 3];

    /// <summary>Three fixed aim targets around the spawn of the sweep floor, so the assist pulls on some ticks (PR-8 exit test 5).</summary>
    private static readonly Vector3[] TargetOffsets =
    [
        new(-3.0f, 1.0f, -3.0f),
        new(2.5f, 1.5f, -4.0f),
        new(0.5f, 0.8f, 3.5f),
    ];

    /// <summary>
    /// The streams that the sweep reads. The list is explicit, so a new value of <see cref="RngStream"/> leaves
    /// this hash alone until a later change adds it here on purpose.
    /// </summary>
    private static readonly RngStream[] SweptStreams =
    [
        RngStream.Procgen,
        RngStream.Loot,
        RngStream.Enemy,
        RngStream.Projectile,
        RngStream.Bot,
    ];

    /// <summary>The hash of the whole sweep.</summary>
    public static StateHash Run()
    {
        StateHash hash = StateHash.Start();
        AddStreams(ref hash);
        AddAngles(ref hash);
        AddRootsAndPowers(ref hash);
        AddFloors(ref hash, SweepContent());
        AddReplay(ref hash, SweepIntents(), SweepContent());
        AddProjectileRun(ref hash, SweepContent());
        AddSwordArcs(ref hash, SweepContent());
        foreach (RampRise rise in SweptRises)
        {
            for (int run = Ramp.SteepestRun; run <= Ramp.ShallowestRun; run++)
            {
                AddRampRun(ref hash, rise, run);
            }
        }

        return hash;
    }

    /// <summary>
    /// Builds a course along one rise: a stone floor, a ramp of the run across the whole width, and a high floor one
    /// block up. A body walks up the slope on a diagonal with a jump on the slope, rolls down, and sprints down, and
    /// the sweep folds its position, its vertical velocity, and the camera on every tick. Rays down onto the slope and
    /// along the rise fold too, with the reachability search in both directions (PR-64 exit test 6). No dug floor
    /// holds a ramp before PR-66, so this run keeps the ramp collision, the ray march, and the search in the
    /// three-platform comparison.
    /// </summary>
    private static void AddRampRun(ref StateHash hash, RampRise rise, int run)
    {
        const int Length = 16;
        const int Width = 9;
        const int RampStart = 6;
        bool alongX = rise == RampRise.PlusX || rise == RampRise.MinusX;
        bool towardPlus = rise == RampRise.PlusX || rise == RampRise.PlusZ;
        VoxelGrid grid = alongX ? new VoxelGrid(Length, 8, Width) : new VoxelGrid(Width, 8, Length);
        for (int along = 0; along < Length; along++)
        {
            int cellAlong = towardPlus ? along : Length - 1 - along;
            for (int across = 0; across < Width; across++)
            {
                int x = alongX ? cellAlong : across;
                int z = alongX ? across : cellAlong;
                grid.Set(x, 0, z, BlockId.RawStone);
                if (along >= RampStart && along < RampStart + run)
                {
                    grid.Set(x, 1, z, new Ramp(rise, run, along - RampStart).Id);
                }
                else if (along >= RampStart + run)
                {
                    grid.Set(x, 1, z, BlockId.RawStone);
                }
            }
        }

        float sign = towardPlus ? 1.0f : -1.0f;
        Vector3 uphill = alongX ? new Vector3(sign, 0.0f, 0.0f) : new Vector3(0.0f, 0.0f, sign);
        Vector3 sideways = alongX ? new Vector3(0.0f, 0.0f, 1.0f) : new Vector3(1.0f, 0.0f, 0.0f);
        float lowAlong = towardPlus ? 3.5f : Length - 3.5f;
        Vector3 start = alongX ? new Vector3(lowAlong, 1.0f, 4.4f) : new Vector3(4.4f, 1.0f, lowAlong);
        PlayerBody body = new(grid, start);
        for (int tick = 0; tick < RampTicks; tick++)
        {
            // A walk up with a drift across the slope, then a roll down, which leaves the slope, then a sprint down.
            Vector3 velocity = (uphill * PlayerBody.WalkSpeed) + (sideways * 0.5f);
            bool follow = true;
            if (tick >= 100 && tick < 100 + Player.RollTicks)
            {
                velocity = uphill * -Player.RollSpeed;
                follow = false;
            }
            else if (tick >= 100 + Player.RollTicks)
            {
                velocity = uphill * -PlayerBody.SprintSpeed;
            }

            body.Move(velocity, tick == 60, false, follow);
            hash.Add(body.Position.X);
            hash.Add(body.Position.Y);
            hash.Add(body.Position.Z);
            hash.Add(body.VerticalVelocity);

            int pitch = ((tick * 131) % ((2 * SimulationLoop.PitchLimit) + 1)) - SimulationLoop.PitchLimit;
            CameraPose pose = OrbitCamera.Place(grid, body.Position, (tick * 997) % SimulationLoop.FullTurn, pitch);
            hash.Add(pose.Position.X);
            hash.Add(pose.Position.Y);
            hash.Add(pose.Position.Z);
        }

        Vector3 up = new(0.0f, 1.0f, 0.0f);
        for (int sample = 0; sample < 8; sample++)
        {
            Vector3 over = start + (uphill * (2.5f + (sample * 0.37f))) + (up * 4.0f);
            RayHit downHit = GridRay.FirstSolid(grid, over, over + (uphill * 0.6f) - (up * 4.5f) + (sideways * 0.2f));
            hash.Add(downHit.Hit);
            hash.Add(downHit.Distance);

            Vector3 level = start + (up * (0.05f + (sample * 0.12f)));
            RayHit levelHit = GridRay.FirstSolid(grid, level, level + (uphill * 10.0f));
            hash.Add(levelHit.Hit);
            hash.Add(levelHit.Distance);
        }

        int lowCell = towardPlus ? 3 : Length - 1 - 3;
        int highCell = towardPlus ? 12 : Length - 1 - 12;
        Cell low = alongX ? new Cell(lowCell, 0, 4) : new Cell(4, 0, lowCell);
        Cell high = alongX ? new Cell(highCell, 1, 4) : new Cell(4, 1, highCell);
        hash.Add(Reachability.From(grid, low).Distance(high));
        hash.Add(Reachability.From(grid, high).Distance(low));
    }

    /// <summary>
    /// The content set of the sweep: three templates on a small grid with the dig sizes of D-341, one per band of
    /// D-210 on floors 1, 2, and 3, two chamber kinds whose weights fill the budget, one projectile definition with a
    /// spread for the projectile run, and one weapon definition, which the attack bit of the sweep intents swings
    /// (D-320). The three swept floors then take every block of the detail pass, and a gallery and drifts of two
    /// heights. Each template lists the three ramp slopes of D-346, and the large chamber kind takes a tier chance
    /// of 50, so the sweep covers the ramps and the tiers of D-388 to D-391. The hash of the set has no meaning beyond its shape (D-221).
    /// </summary>
    public static ContentSet SweepContent()
    {
        FloorTemplate[] floors =
        [
            new("sweep-working", 1, 1, 4, 8, 100, DetailPass.WorkingMine, 32, 12, 32, 7, 5, 5, 4, 5, 8, [2, 3, 4]),
            new("sweep-older", 2, 2, 4, 8, 100, DetailPass.OlderWorkings, 32, 12, 32, 7, 5, 5, 4, 5, 8, [2, 3, 4]),
            new("sweep-deep", 3, 3, 4, 8, 100, DetailPass.Deep, 32, 12, 32, 7, 5, 5, 4, 5, 8, [2, 3, 4]),
        ];
        ChamberKind[] kinds =
        [
            new("sweep-small", 10, 1, 2, 3, 5, 0),
            new("sweep-large", 25, 2, 3, 5, 8, 50),
        ];
        ProjectileDefinition[] projectiles =
        [
            new("sweep-shot", 4000, 100, 300, 1, 0, 200),
        ];
        WeaponDefinition[] weapons =
        [
            new("sweep-sword", 0, WeaponDefinition.OneHanded, 12, 6, 18, 34, 160, 9000, 50, 170, "models/sweep-sword.bbmodel", "models/sweep.swing.json"),
        ];
        return new ContentSet(ReplayContentHash, floors, kinds, projectiles, weapons, Strings.FromMembers(Strings.FilePath, []));
    }

    /// <summary>
    /// Fires the projectile definition of the sweep from over the spawn of floor 1 in fixed directions, with the
    /// spread of the Projectile stream, steps every shot to its end against the grid and one entity box, and folds
    /// each end (D-320, PR-10 exit test 5). No intent fires a shot from PR-15 onward, so this run keeps the
    /// integrator, the spread, and the swept collision in the three-platform comparison.
    /// </summary>
    private static void AddProjectileRun(ref StateHash hash, ContentSet content)
    {
        FloorPlan plan = FloorGenerator.Generate(RunSeed, 1, content);
        ProjectileSimulation simulation = new(plan.Grid, content.Projectiles);
        Rng spread = Rng.ForStream(RunSeed, RngStream.Projectile);
        Vector3 origin = plan.Spawn + new Vector3(0.0f, 1.5f, 0.0f);
        for (int shot = 0; shot < ProjectileShots; shot++)
        {
            float angle = shot * (DetMath.Pi / 8.0f);
            simulation.Fire(0, -1, origin, new Vector3(DetMath.Sin(angle), 0.25f, DetMath.Cos(angle)), spread);
        }

        EntityBox[] boxes = [new EntityBox(0, new Aabb(plan.Spawn + new Vector3(1.0f, 0.0f, 1.0f), plan.Spawn + new Vector3(1.6f, 1.8f, 1.6f)))];
        while (simulation.Live.Count > 0)
        {
            foreach (ProjectileEnd end in simulation.Step(boxes))
            {
                hash.Add((int)end.Kind);
                hash.Add(end.Point.X);
                hash.Add(end.Point.Y);
                hash.Add(end.Point.Z);
                hash.Add(end.Projectile.Age);
                hash.Add(end.EntityIndex);
            }
        }
    }

    /// <summary>
    /// Folds the blade lines and the wedge test of the sweep weapon for every step of the arc at eight yaws, against a
    /// ring of boxes around the feet (D-325). No enemy stands in the loop before PR-16, so the replay swings at no box,
    /// and this fold keeps the arc in the three-platform comparison.
    /// </summary>
    private static void AddSwordArcs(ref StateHash hash, ContentSet content)
    {
        WeaponDefinition weapon = content.Weapons[0];
        Vector3 feet = new(10.0f, 1.0f, 10.0f);
        for (int yawStep = 0; yawStep < ArcYaws; yawStep++)
        {
            int yaw = yawStep * (SimulationLoop.FullTurn / ArcYaws);
            for (int step = 0; step < weapon.ActiveTicks; step++)
            {
                int fromYaw = yaw + MeleeWeapon.BladeOffset(weapon, step);
                int toYaw = yaw + MeleeWeapon.BladeOffset(weapon, step + 1);
                Vector3 blade = MeleeWeapon.BladeDirection(fromYaw);
                hash.Add(blade.X);
                hash.Add(blade.Z);
                for (int ring = 0; ring < ArcBoxes; ring++)
                {
                    float angle = ring * (DetMath.Pi / 12.0f);
                    float distance = 0.5f + ((ring % 4) * 0.45f);
                    Vector3 center = feet + new Vector3(DetMath.Sin(angle) * distance, 0.0f, -DetMath.Cos(angle) * distance);
                    Aabb box = new(center + new Vector3(-0.3f, 0.2f, -0.3f), center + new Vector3(0.3f, 1.4f, 0.3f));
                    hash.Add(MeleeWeapon.WedgeHits(weapon, feet, fromYaw, toYaw, box));
                }
            }
        }
    }

    /// <summary>
    /// Digs each swept floor and folds every block, the spawn, the stairwell, and the chamber count (PR-9 exit
    /// test 7). The three platforms must agree on every draw of the budget, every stamp of the walk, and every
    /// cell of the reachability search that places the stairwell.
    /// </summary>
    private static void AddFloors(ref StateHash hash, ContentSet content)
    {
        foreach (int floor in SweptFloors)
        {
            FloorPlan plan = FloorGenerator.Generate(RunSeed, floor, content);
            hash.Add(plan.Floor);
            for (int y = 0; y < plan.Grid.SizeY; y++)
            {
                for (int z = 0; z < plan.Grid.SizeZ; z++)
                {
                    for (int x = 0; x < plan.Grid.SizeX; x++)
                    {
                        hash.Add((byte)plan.Grid.Get(x, y, z));
                    }
                }
            }

            hash.Add(plan.Spawn.X);
            hash.Add(plan.Spawn.Y);
            hash.Add(plan.Spawn.Z);
            hash.Add(plan.Stairwell.X);
            hash.Add(plan.Stairwell.Y);
            hash.Add(plan.Stairwell.Z);
            hash.Add(plan.Chambers.Count);
            hash.Add(plan.Shafts.Count);
            hash.Add(plan.Ramps.Count);
        }
    }

    /// <summary>Draws from each stream in turn: the raw word, the float, and a bounded integer.</summary>
    private static void AddStreams(ref StateHash hash)
    {
        foreach (RngStream stream in SweptStreams)
        {
            Rng rng = Rng.ForStream(RunSeed, stream);
            hash.Add((int)stream);
            for (int draw = 0; draw < DrawsPerStream; draw++)
            {
                hash.Add(rng.NextUInt());
                hash.Add(rng.NextFloat());

                // The bound changes with the draw, so the rejection path runs for many different bounds.
                hash.Add(rng.NextInt(draw + 1));
            }
        }
    }

    /// <summary>Sweeps Sin and Cos across [-4 pi, 4 pi] at an even step.</summary>
    private static void AddAngles(ref StateHash hash)
    {
        float start = -4.0f * DetMath.Pi;
        float step = (8.0f * DetMath.Pi) / AngleSamples;
        for (int sample = 0; sample <= AngleSamples; sample++)
        {
            float angle = start + (sample * step);
            hash.Add(DetMath.Sin(angle));
            hash.Add(DetMath.Cos(angle));
        }
    }

    /// <summary>Sweeps Atan2 over a square grid, then Sqrt and Pow over a fixed set of values.</summary>
    private static void AddRootsAndPowers(ref StateHash hash)
    {
        float span = 2.0f * DetMath.Pi;
        float gridStep = (2.0f * span) / AtanGridSide;
        for (int row = 0; row <= AtanGridSide; row++)
        {
            for (int column = 0; column <= AtanGridSide; column++)
            {
                float y = -span + (row * gridStep);
                float x = -span + (column * gridStep);

                // The zero vector has no angle, so the sweep steps over the one grid point that holds it.
                if (y == 0.0f && x == 0.0f)
                {
                    continue;
                }

                hash.Add(DetMath.Atan2(y, x));
            }
        }

        // The grid above never makes a negative zero, so it cannot see a platform that reads the sign bit of a
        // zero differently. These pairs cover both signs of zero on both axes (F-62).
        float[] signCases = [-0.0f, 0.0f, -1.0f, 1.0f];
        foreach (float y in signCases)
        {
            foreach (float x in signCases)
            {
                if (y == 0.0f && x == 0.0f)
                {
                    continue;
                }

                hash.Add(DetMath.Atan2(y, x));
            }
        }

        for (int sample = 0; sample <= 512; sample++)
        {
            float value = sample * 0.5f;
            hash.Add(DetMath.Sqrt(value));
            hash.Add(DetMath.Floor((value * 0.375f) - 3.5f));
            hash.Add(DetMath.Clamp(value - 64.0f, -10.0f, 10.0f));
            hash.Add(DetMath.Lerp(-2.5f, 7.25f, value * 0.001953125f));
        }

        for (int exponent = -4; exponent <= 8; exponent++)
        {
            hash.Add(DetMath.Pow(1.5f, exponent));
            hash.Add(DetMath.Pow(-0.75f, exponent));
            hash.Add(DetMath.Pow(2.0f, exponent));
        }
    }

    /// <summary>The intents of the sweep run, which the seed makes. The record and the camera fold read one list.</summary>
    private static IReadOnlyList<Intent> SweepIntents()
    {
        Rng rng = Rng.ForStream(RunSeed, RngStream.Enemy);
        List<Intent> intents = [];
        for (uint tick = 0; tick < ReplayFrames; tick++)
        {
            uint look = rng.NextUInt();
            uint rest = rng.NextUInt();

            // The buttons keep the assigned bits alone, because a set reserved bit is an error (D-232, D-243).
            // The stairwell bits stay clear, so the sweep run stays on floor 1 and never ends (D-257).
            ushort buttons = (ushort)((rest >> 16) & Button.AssignedMask & ~(Button.Interact | Button.Ascend));
            intents.Add(new Intent(tick, (short)look, (short)(look >> 16), (sbyte)rest, (sbyte)(rest >> 8), buttons));
        }

        return intents;
    }

    /// <summary>
    /// Records the run, replays the record on floor 1 of the sweep seed, and folds in the camera pose and the
    /// aim ray of every replayed tick, the end state, and the checksum of the whole record (PR-6 exit test 7,
    /// PR-8 exit test 5). The three platforms must agree on the frame bytes, the header text, the CRC-32, the
    /// loop, the collision of the body with the dug floor, the ray march of the boom, and the assist pull.
    /// </summary>
    /// <remarks>
    /// The camera values come from the loop that replays the record, through <see cref="IReplayObserver"/>, and
    /// never from a second live run. A replay defect that changes the camera sequence then changes this hash.
    /// </remarks>
    private static void AddReplay(ref StateHash hash, IReadOnlyList<Intent> intents, ContentSet content)
    {
        MemorySink sink = new();
        RunRecorder recorder = new(sink, RunRecord.NewHeader(ReplayContentHash, RunSeed));
        foreach (Intent intent in intents)
        {
            recorder.Record(intent);
        }

        CameraFold cameras = new();
        ReplayResult result = RunReplayer.Replay(sink.Bytes, content, new JsonlLogger(new RejectingLogSink()), cameras);
        hash.Add(cameras.Hash.Value);
        hash.Add(result.Loop.Hash().Value);
        hash.Add(Crc32.Of(sink.Bytes, 0, sink.Bytes.Count));
    }

    /// <summary>
    /// Folds the camera pose and the aim ray of every replayed tick into one hash (PR-8 exit test 5). The replay
    /// calls it after each frame, so the values come from the replay traversal itself. The targets sit at fixed
    /// offsets from the spawn of the floor, so the assist has something to pull toward.
    /// </summary>
    private sealed class CameraFold : IReplayObserver
    {
        private StateHash hash = StateHash.Start();

        /// <summary>The hash of every pose and ray so far.</summary>
        public StateHash Hash => this.hash;

        public void AfterTick(SimulationLoop loop)
        {
            CameraPose pose = loop.Camera();
            this.hash.Add(pose.Position.X);
            this.hash.Add(pose.Position.Y);
            this.hash.Add(pose.Position.Z);
            this.hash.Add(pose.Forward.X);
            this.hash.Add(pose.Forward.Y);
            this.hash.Add(pose.Forward.Z);

            Vector3[] targets = new Vector3[TargetOffsets.Length];
            for (int index = 0; index < TargetOffsets.Length; index++)
            {
                targets[index] = loop.Plan.Spawn + TargetOffsets[index];
            }

            AimRay aim = loop.Aim(targets);
            this.hash.Add(aim.Direction.X);
            this.hash.Add(aim.Direction.Y);
            this.hash.Add(aim.Direction.Z);
        }
    }

    /// <summary>A sink that keeps the record in memory. The sweep never touches the disk.</summary>
    private sealed class MemorySink : IRunRecordSink
    {
        public List<byte> Bytes { get; } = [];

        public void Append(byte[] bytes)
        {
            this.Bytes.AddRange(bytes);
        }
    }

    /// <summary>The sweep record has no torn tail, so a log line here is a defect of the sweep and never a line to drop (T-2).</summary>
    private sealed class RejectingLogSink : ILogSink
    {
        public void Write(string line)
        {
            throw new InvalidOperationException($"The sweep replay wrote a log line, and its record has no torn tail. The line is {line}");
        }
    }
}
