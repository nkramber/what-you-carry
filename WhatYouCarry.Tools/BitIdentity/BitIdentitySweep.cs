using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Tools.BitIdentity;

/// <summary>
/// A fixed run of the RNG, of DetMath, of the floor generator, of one recorded run through the replay, and of
/// the camera over that run, folded into one state hash (D-69, D-71). Two platforms that give the same hash
/// agree on every bit of all five.
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
        return hash;
    }

    /// <summary>
    /// The content set of the sweep: three templates on a small grid, one per band of D-210 on floors 1, 2, and
    /// 3, and two chamber kinds whose weights fill the budget. The three swept floors then take every block of
    /// the detail pass. The hash of the set has no meaning beyond its shape (D-221).
    /// </summary>
    public static ContentSet SweepContent()
    {
        FloorTemplate[] floors =
        [
            new("sweep-working", 1, 1, 4, 8, 100, DetailPass.WorkingMine, 32, 12, 32),
            new("sweep-older", 2, 2, 4, 8, 100, DetailPass.OlderWorkings, 32, 12, 32),
            new("sweep-deep", 3, 3, 4, 8, 100, DetailPass.Deep, 32, 12, 32),
        ];
        ChamberKind[] kinds =
        [
            new("sweep-small", 10, 1, 2, 3, 5),
            new("sweep-large", 25, 2, 3, 5, 8),
        ];
        ProjectileDefinition[] projectiles = [];
        return new ContentSet(ReplayContentHash, floors, kinds, projectiles, Strings.FromMembers(Strings.FilePath, []));
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
