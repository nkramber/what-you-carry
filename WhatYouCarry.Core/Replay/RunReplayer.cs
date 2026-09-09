using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;

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
/// The loop digs every floor from the seed, the floor number, and the content set, so the record and the
/// content set decide the whole run, and two callers with one record and one content set replay one run
/// (D-236). The content hash of the header must match the hash of the set that the caller hands over.
/// </para>
/// <para>
/// A torn tail is not an error. A crash can stop a write inside a frame, and the bytes before it are a complete
/// run up to that tick (D-152). The replay stops at the last complete frame and writes one warning line that
/// names the floor of the state (D-228).
/// </para>
/// </remarks>
public static class RunReplayer
{
    /// <summary>The subsystem name of the torn-tail line.</summary>
    public const string Subsystem = "replay";

    /// <summary>The name of the extra field of the torn-tail line: the count of bytes after the last complete frame.</summary>
    public const string TornBytesName = "tornBytes";

    /// <summary>The message of the torn-tail line.</summary>
    public const string TornTailMessage = "The run record ends inside a frame, and the replay stops at the last complete frame.";

    /// <summary>The state after every complete frame of the record.</summary>
    /// <param name="record">The whole record: the header line and the frames.</param>
    /// <param name="content">The content set that this build loaded. Its hash must match the header (D-163).</param>
    /// <param name="logger">The logger that takes the torn-tail line.</param>
    /// <exception cref="ContextException">The header is not valid, a version or the content hash does not match this build, the content cannot dig a floor, or a frame fails its checksum, its tick order, its reserved bits, or the run end.</exception>
    public static ReplayResult Replay(IReadOnlyList<byte> record, ContentSet content, JsonlLogger logger)
    {
        return Replay(record, content, logger, SilentObserver.Instance);
    }

    /// <summary>
    /// The state after every complete frame of the record, with an observer that reads the loop after each
    /// replayed frame (PR-8 exit test 5).
    /// </summary>
    /// <param name="record">The whole record: the header line and the frames.</param>
    /// <param name="content">The content set that this build loaded. Its hash must match the header (D-163).</param>
    /// <param name="logger">The logger that takes the torn-tail line.</param>
    /// <param name="observer">The reader of the loop after each replayed frame.</param>
    /// <exception cref="ContextException">The header is not valid, a version or the content hash does not match this build, the content cannot dig a floor, or a frame fails its checksum, its tick order, its reserved bits, or the run end.</exception>
    public static ReplayResult Replay(IReadOnlyList<byte> record, ContentSet content, JsonlLogger logger, IReplayObserver observer)
    {
        (RunRecordHeader header, int bodyStart) = RunRecord.ReadHeader(record);

        if (header.ContentHash != content.Hash)
        {
            ContextException mismatch = new($"The run record comes from content hash {header.ContentHash}, and this build loaded content hash {content.Hash}. A replay is exact only on a match (D-151).");
            mismatch.AddContext("recordContentHash", header.ContentHash);
            mismatch.AddContext("buildContentHash", content.Hash);
            throw mismatch;
        }

        int bodyLength = record.Count - bodyStart;
        int frameCount = bodyLength / Intent.FrameSize;
        int tornBytes = bodyLength - (frameCount * Intent.FrameSize);

        SimulationLoop loop = new(header.Seed, content);
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
            fields.Add("floor", (long)loop.Floor);
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
