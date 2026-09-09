using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Tools.BitIdentity;

/// <summary>
/// A fixed run of the RNG, of DetMath, of one recorded run through the replay, and of the camera over that run,
/// folded into one state hash (D-69, D-71). Two platforms that give the same hash agree on every bit of all four.
/// </summary>
/// <remarks>
/// <para>
/// Every number here is a constant of this file, so the sweep takes no input and needs no content. The hash
/// changes only when this file changes or when Core changes its numbers. The `bit-identity` CI job runs it on
/// Linux x64, Windows x64, and macOS arm64 and compares the three results.
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

    /// <summary>The side of the sweep grid, in blocks.</summary>
    public const int ReplayGridSide = 16;

    /// <summary>The height of the sweep grid, in blocks.</summary>
    public const int ReplayGridHeight = 8;

    /// <summary>The feet center of the body at tick zero, over the middle of the floor.</summary>
    public static readonly Vector3 ReplaySpawn = new(8.5f, 1.0f, 8.5f);

    /// <summary>Three fixed aim targets in the sweep grid, so the assist pulls on some ticks (PR-8 exit test 5).</summary>
    private static readonly Vector3[] SweepTargets =
    [
        new(4.5f, 2.0f, 4.5f),
        new(11.5f, 2.5f, 3.5f),
        new(8.5f, 1.8f, 13.5f),
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
        IReadOnlyList<Intent> intents = SweepIntents();
        AddReplay(ref hash, intents);
        AddCamera(ref hash, intents);
        return hash;
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
            ushort buttons = (ushort)((rest >> 16) & Button.AssignedMask);
            intents.Add(new Intent(tick, (short)look, (short)(look >> 16), (sbyte)rest, (sbyte)(rest >> 8), buttons));
        }

        return intents;
    }

    /// <summary>
    /// Records the run, replays the record, and folds in the end state and the checksum of the whole record
    /// (PR-6 exit test 7). The three platforms must agree on the frame bytes, the header text, the CRC-32, the
    /// loop, and the collision of the body with the fixed grid (PR-7).
    /// </summary>
    private static void AddReplay(ref StateHash hash, IReadOnlyList<Intent> intents)
    {
        MemorySink sink = new();
        RunRecorder recorder = new(sink, RunRecord.NewHeader(ReplayContentHash, RunSeed));
        foreach (Intent intent in intents)
        {
            recorder.Record(intent);
        }

        ReplayResult result = RunReplayer.Replay(sink.Bytes, ReplayContentHash, ReplayGrid(), ReplaySpawn, new JsonlLogger(new RejectingLogSink()));
        hash.Add(result.Loop.Hash().Value);
        hash.Add(Crc32.Of(sink.Bytes, 0, sink.Bytes.Count));
    }

    /// <summary>
    /// Runs the same intents live and folds in the camera pose and the aim ray of every tick (PR-8 exit test 5).
    /// The three platforms must agree on the ray march of the boom and on the assist pull.
    /// </summary>
    private static void AddCamera(ref StateHash hash, IReadOnlyList<Intent> intents)
    {
        SimulationLoop loop = new(RunSeed, ReplayGrid(), ReplaySpawn);
        foreach (Intent intent in intents)
        {
            loop.Step(intent);
            CameraPose pose = loop.Camera();
            hash.Add(pose.Position.X);
            hash.Add(pose.Position.Y);
            hash.Add(pose.Position.Z);
            hash.Add(pose.Forward.X);
            hash.Add(pose.Forward.Y);
            hash.Add(pose.Forward.Z);

            AimRay aim = loop.Aim(SweepTargets);
            hash.Add(aim.Direction.X);
            hash.Add(aim.Direction.Y);
            hash.Add(aim.Direction.Z);
        }
    }

    /// <summary>
    /// The fixed grid of the sweep replay: a stone floor, a scatter of one-block steps, and a scatter of
    /// two-block pillars, from two fixed patterns. The body walks, jumps onto the steps, and stops at the
    /// pillars. The two patterns leave the spawn column clear.
    /// </summary>
    public static VoxelGrid ReplayGrid()
    {
        VoxelGrid grid = new(ReplayGridSide, ReplayGridHeight, ReplayGridSide);
        for (int x = 0; x < ReplayGridSide; x++)
        {
            for (int z = 0; z < ReplayGridSide; z++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);

                if (((x * 7) + (z * 3)) % 5 == 1)
                {
                    grid.Set(x, 1, z, BlockId.RawStone);
                }

                if (((x * 3) + (z * 5)) % 11 == 2)
                {
                    grid.Set(x, 1, z, BlockId.RawStone);
                    grid.Set(x, 2, z, BlockId.RawStone);
                }
            }
        }

        return grid;
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
