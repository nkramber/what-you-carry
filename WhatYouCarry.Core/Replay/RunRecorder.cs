using System.Globalization;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Replay;

/// <summary>
/// Writes a run record (D-97, G-5). The constructor writes the header, and <see cref="Record"/> appends one
/// frame per tick from the first tick, so no tick runs before its intent is on the record.
/// </summary>
/// <remarks>
/// The recorder hands each byte array to an <see cref="IRunRecordSink"/> and opens no file (D-225). The Game
/// layer calls <see cref="Record"/> and then <see cref="SimulationLoop.Step"/> with the same intent, so the
/// record is ahead of the state at every moment, and a crash loses no tick that ran.
/// </remarks>
public sealed class RunRecorder
{
    private readonly IRunRecordSink sink;

    /// <summary>A recorder that writes the header now and the frames as they come.</summary>
    /// <exception cref="ContextException">The content hash of the header is not 64 lowercase hexadecimal digits.</exception>
    public RunRecorder(IRunRecordSink sink, RunRecordHeader header)
    {
        this.sink = sink;
        this.sink.Append(RunRecord.WriteHeader(header));
    }

    /// <summary>The tick that the next frame must carry. It starts at zero.</summary>
    public uint NextTick { get; private set; }

    /// <summary>Appends one frame. The intent must carry <see cref="NextTick"/>.</summary>
    /// <exception cref="ContextException">The intent is not for the next tick, or the tick counter is full.</exception>
    public void Record(Intent intent)
    {
        // A frame out of order would replay as another run, and a gap would replay as a shorter one (G-5).
        if (intent.Tick != this.NextTick)
        {
            ContextException outOfOrder = new($"The record is at tick {this.NextTick}, and the intent is for tick {intent.Tick}. Every tick takes one frame, in order (G-5).");
            outOfOrder.AddContext("expectedTick", ((long)this.NextTick).ToString(CultureInfo.InvariantCulture));
            outOfOrder.AddContext("intentTick", ((long)intent.Tick).ToString(CultureInfo.InvariantCulture));
            throw outOfOrder;
        }

        if (this.NextTick == uint.MaxValue)
        {
            throw new ContextException($"The tick counter is full at {this.NextTick}, and the record cannot take another frame.");
        }

        this.sink.Append(intent.Encode());
        this.NextTick++;
    }
}
