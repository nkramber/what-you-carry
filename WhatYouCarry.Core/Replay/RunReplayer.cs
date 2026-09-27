using System;
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
/// D-151 and start the floor fresh. A frame that fails its checksum, with a byte that is not zero from its start
/// to the end of the record, is an error that names the frame and its byte offset (D-656).
/// </para>
/// <para>
/// The loop digs every floor from the seed, the floor number, and the content set, so the record and the
/// content set decide the whole run, and two callers with one record and one content set replay one run
/// (D-236). The content hash of the header must match the hash of the set that the caller hands over.
/// </para>
/// <para>
/// A torn tail is not an error. A crash can stop a write inside a frame, or leave whole frames of zeros at the end
/// of the file, and the bytes before them are a complete run up to that tick (D-152). The replay cuts the tail at
/// the first frame that fails its checksum when that frame is short or when each byte from its start to the end of
/// the record is zero (D-656). The CRC-32 of twelve zero bytes is not zero, so a frame of zeros never passes its
/// checksum. The replay stops before the cut and writes one warning line that names the floor of the state and the
/// count of bytes that it cut (D-228). <see cref="ReplayResult.FrameCount"/> and
/// <see cref="ReplayResult.TornBytes"/> give the cut to the caller.
/// </para>
/// </remarks>
public static class RunReplayer
{
    /// <summary>The subsystem name of the torn-tail line.</summary>
    public const string Subsystem = "replay";

    /// <summary>The name of the extra field of the torn-tail line: the count of bytes that the cut removed (D-656).</summary>
    public const string TornBytesName = "tornBytes";

    /// <summary>The message of the torn-tail line.</summary>
    public const string TornTailMessage = "The run record ends in a torn tail, a short frame or frames of zeros, and the replay stops at the last frame before it.";

    /// <summary>The state after every frame of the record before a torn tail (D-656).</summary>
    /// <param name="record">The whole record: the header line and the frames.</param>
    /// <param name="content">The content set that this build loaded. Its hash must match the header (D-163).</param>
    /// <param name="logger">The logger that takes the torn-tail line.</param>
    /// <exception cref="ContextException">The header is not valid, a version or the content hash does not match this build, the content cannot dig a floor, or a frame fails its tick order, its reserved bits, or the run end. A frame that fails its checksum is an error when a byte from its start to the end of the record is not zero (D-656).</exception>
    public static ReplayResult Replay(IReadOnlyList<byte> record, ContentSet content, JsonlLogger logger)
    {
        return Replay(record, content, logger, SilentObserver.Instance);
    }

    /// <summary>
    /// The state after every frame of the record before a torn tail (D-656), with an observer that reads the loop
    /// after each replayed frame (PR-8 exit test 5).
    /// </summary>
    /// <param name="record">The whole record: the header line and the frames.</param>
    /// <param name="content">The content set that this build loaded. Its hash must match the header (D-163).</param>
    /// <param name="logger">The logger that takes the torn-tail line.</param>
    /// <param name="observer">The reader of the loop after each replayed frame.</param>
    /// <exception cref="ContextException">The header is not valid, a version or the content hash does not match this build, the content cannot dig a floor, or a frame fails its tick order, its reserved bits, or the run end. A frame that fails its checksum is an error when a byte from its start to the end of the record is not zero (D-656).</exception>
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
        int frameCount = FramesBeforeTornTail(record, bodyStart, header.Seed);
        int tornBytes = bodyLength - (frameCount * Intent.FrameSize);

        SimulationLoop loop = new(header.Seed, content);
        for (int frame = 0; frame < frameCount; frame++)
        {
            uint tick = loop.Tick;
            try
            {
                loop.Step(Intent.Decode(record, bodyStart + (frame * Intent.FrameSize)));
            }
            catch (ContextException error)
            {
                // The loop names the seed, the floor, and the tick of an error inside the tick, and a failed decode
                // or a failed check before the tick names none of them. The frame index is what a reader needs to
                // find the bytes, and only this loop knows it (D-113, F-121).
                AddFrameContext(error, header.Seed, loop.Floor, tick, frame);
                throw;
            }
            catch (Exception error)
            {
                ContextException wrapped = new($"Frame {frame} of the run record failed: {error.Message}", error);
                AddFrameContext(wrapped, header.Seed, loop.Floor, tick, frame);
                throw wrapped;
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

    /// <summary>
    /// The count of frames before the torn tail of the record (D-656). The tail starts at the first frame that fails
    /// its checksum, when that frame is short or when each byte from its start to the end of the record is zero. A
    /// record with no torn tail gives the count of all its frames.
    /// </summary>
    /// <exception cref="ContextException">A frame fails its checksum, and a byte from its start to the end of the record is not zero. The error names the frame, its byte offset in the record, and the two checksums (T-2).</exception>
    private static int FramesBeforeTornTail(IReadOnlyList<byte> record, int bodyStart, ulong seed)
    {
        int wholeFrames = (record.Count - bodyStart) / Intent.FrameSize;
        for (int frame = 0; frame < wholeFrames; frame++)
        {
            int offset = bodyStart + (frame * Intent.FrameSize);
            uint stored = Intent.StoredChecksum(record, offset);
            uint computed = Intent.ComputedChecksum(record, offset);
            if (stored == computed)
            {
                continue;
            }

            int firstNonZero = -1;
            for (int index = offset; index < record.Count; index++)
            {
                if (record[index] != 0)
                {
                    firstNonZero = index;
                    break;
                }
            }

            // A crash can leave whole frames of zeros at the end of the file. They are a torn tail, and the
            // frames before them are the run (D-152, D-656).
            if (firstNonZero < 0)
            {
                return frame;
            }

            // A byte that is not zero after a failed checksum is data that changed, and never a torn tail. A replay
            // that stopped there would lose the frames after it in silence (T-2, D-656).
            ContextException corrupt = new($"Frame {frame} at byte {offset} of the run record fails its checksum, and the byte at {firstNonZero} is not zero, so the frame is not a torn tail (D-656). The frame holds {Intent.Hex(stored)}, and its bytes give {Intent.Hex(computed)}.");
            corrupt.AddContext("seed", seed.ToString(CultureInfo.InvariantCulture));
            corrupt.AddContext("frame", ((long)frame).ToString(CultureInfo.InvariantCulture));
            corrupt.AddContext("byteOffset", ((long)offset).ToString(CultureInfo.InvariantCulture));
            corrupt.AddContext("nonZeroByte", ((long)firstNonZero).ToString(CultureInfo.InvariantCulture));
            corrupt.AddContext("storedChecksum", Intent.Hex(stored));
            corrupt.AddContext("computedChecksum", Intent.Hex(computed));
            throw corrupt;
        }

        // The bytes after the last whole frame, if any, are a short frame: a crash stopped the write inside it
        // (D-152, D-656).
        return wholeFrames;
    }

    /// <summary>Adds the seed, the floor, the tick, and the frame index to an error of one frame, and keeps a field that the loop already added (design 6.1, F-121).</summary>
    private static void AddFrameContext(ContextException error, ulong seed, int floor, uint tick, int frame)
    {
        error.AddContextIfAbsent("seed", seed.ToString(CultureInfo.InvariantCulture));
        error.AddContextIfAbsent("floor", ((long)floor).ToString(CultureInfo.InvariantCulture));
        error.AddContextIfAbsent("tick", ((long)tick).ToString(CultureInfo.InvariantCulture));
        error.AddContext("frame", ((long)frame).ToString(CultureInfo.InvariantCulture));
    }
}

/// <summary>
/// What a replay gives back: the header, the loop after the last replayed frame, the count of frames that ran,
/// and the count of bytes in the torn tail that the replay cut, which is zero for a whole record (D-656). The torn
/// tail is a short frame, whole frames of zeros, or both.
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
