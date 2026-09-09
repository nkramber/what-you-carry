using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Replay;

/// <summary>
/// Replays a run record (D-97, D-151). It reads the header, checks the two versions and the content hash, and
/// drives a fresh <see cref="SimulationLoop"/> with each frame. It ignores the live bank and tree, because the
/// header holds the initial state and the frames hold every input.
/// </summary>
/// <remarks>
/// <para>
/// A version or content mismatch is an error that names both values, so the caller can show the notice of
/// D-151 and start the floor fresh. A frame that fails its checksum is an error that names the frame.
/// </para>
/// <para>
/// The caller supplies the grid and the spawn point, because no generator exists before PR-9 (D-236). The
/// record does not carry them, so two callers with two grids replay two runs from one record until PR-9 derives
/// both from the seed. The bit-identity sweep and every test build the same grid on each side.
/// </para>
/// <para>
/// A torn tail is not an error. A crash can stop a write inside a frame, and the bytes before it are a complete
/// run up to that tick (D-152). The replay stops at the last complete frame and writes one warning line.
/// </para>
/// </remarks>
public static class RunReplayer
{
    /// <summary>
    /// The floor that the torn-tail line names (D-228). Every run starts at floor 1 (D-3), and a Phase 1 run
    /// never leaves it. PR-31 reads the floor from the state instead, when the state holds one.
    /// </summary>
    public const int Floor = 1;

    /// <summary>The subsystem name of the torn-tail line.</summary>
    public const string Subsystem = "replay";

    /// <summary>The name of the extra field of the torn-tail line: the count of bytes after the last complete frame.</summary>
    public const string TornBytesName = "tornBytes";

    /// <summary>The message of the torn-tail line.</summary>
    public const string TornTailMessage = "The run record ends inside a frame, and the replay stops at the last complete frame.";

    /// <summary>The state after every complete frame of the record.</summary>
    /// <param name="record">The whole record: the header line and the frames.</param>
    /// <param name="buildContentHash">The hash of the content set that this build loaded (D-163).</param>
    /// <param name="grid">The grid of the floor (D-236).</param>
    /// <param name="spawn">The feet center of the player at tick zero (D-236).</param>
    /// <param name="logger">The logger that takes the torn-tail line.</param>
    /// <exception cref="ContextException">The header is not valid, a version or the content hash does not match this build, the spawn point is inside rock, or a frame fails its checksum, its tick order, or its reserved bits.</exception>
    public static ReplayResult Replay(IReadOnlyList<byte> record, string buildContentHash, VoxelGrid grid, Vector3 spawn, JsonlLogger logger)
    {
        return Replay(record, buildContentHash, grid, spawn, logger, SilentObserver.Instance);
    }

    /// <summary>
    /// The state after every complete frame of the record, with an observer that reads the loop after each
    /// replayed frame (PR-8 exit test 5).
    /// </summary>
    /// <param name="record">The whole record: the header line and the frames.</param>
    /// <param name="buildContentHash">The hash of the content set that this build loaded (D-163).</param>
    /// <param name="grid">The grid of the floor (D-236).</param>
    /// <param name="spawn">The feet center of the player at tick zero (D-236).</param>
    /// <param name="logger">The logger that takes the torn-tail line.</param>
    /// <param name="observer">The reader of the loop after each replayed frame.</param>
    /// <exception cref="ContextException">The header is not valid, a version or the content hash does not match this build, the spawn point is inside rock, or a frame fails its checksum, its tick order, or its reserved bits.</exception>
    public static ReplayResult Replay(IReadOnlyList<byte> record, string buildContentHash, VoxelGrid grid, Vector3 spawn, JsonlLogger logger, IReplayObserver observer)
    {
        (RunRecordHeader header, int bodyStart) = RunRecord.ReadHeader(record);

        if (header.ContentHash != buildContentHash)
        {
            ContextException mismatch = new($"The run record comes from content hash {header.ContentHash}, and this build loaded content hash {buildContentHash}. A replay is exact only on a match (D-151).");
            mismatch.AddContext("recordContentHash", header.ContentHash);
            mismatch.AddContext("buildContentHash", buildContentHash);
            throw mismatch;
        }

        int bodyLength = record.Count - bodyStart;
        int frameCount = bodyLength / Intent.FrameSize;
        int tornBytes = bodyLength - (frameCount * Intent.FrameSize);

        SimulationLoop loop = new(header.Seed, grid, spawn);
        for (int frame = 0; frame < frameCount; frame++)
        {
            try
            {
                loop.Step(Intent.Decode(record, bodyStart + (frame * Intent.FrameSize)));
            }
            catch (ContextException error)
            {
                // The decode names the tick and the loop names both ticks. The frame index is what a reader
                // needs to find the bytes, and only this loop knows it (D-113).
                error.AddContext("frame", ((long)frame).ToString(CultureInfo.InvariantCulture));
                throw;
            }

            observer.AfterTick(loop);
        }

        if (tornBytes != 0)
        {
            LogFields fields = new();
            fields.Add("seed", header.Seed);
            fields.Add("floor", (long)Floor);
            fields.Add("tick", (long)loop.Tick);
            fields.Add("subsystem", Subsystem);
            long[] entities = [];
            fields.Add("entities", entities);
            fields.Add(TornBytesName, (long)tornBytes);
            logger.Write(LogContextKind.Run, LogLevel.Warning, TornTailMessage, fields);
        }

        return new ReplayResult(header, loop, frameCount, tornBytes);
    }
}

/// <summary>
/// What a replay gives back: the header, the loop after the last complete frame, the count of frames that ran,
/// and the count of bytes in a torn tail, which is zero for a whole record.
/// </summary>
public sealed record ReplayResult(RunRecordHeader Header, SimulationLoop Loop, int FrameCount, int TornBytes);

/// <summary>The observer of a replay that reads nothing. The overload without an observer uses it.</summary>
internal sealed class SilentObserver : IReplayObserver
{
    /// <summary>The one instance. It holds no state.</summary>
    public static readonly SilentObserver Instance = new();

    private SilentObserver()
    {
    }

    /// <inheritdoc/>
    public void AfterTick(SimulationLoop loop)
    {
    }
}
