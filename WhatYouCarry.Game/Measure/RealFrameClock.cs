using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Measure;

/// <summary>
/// The real wall time of each render frame, from readings of the engine clock in microseconds (D-635, F-170). The
/// engine delta of a frame is smoothed and rounded to the vsync interval, so one missed vsync could read as two frames
/// or as one. The clock gives the time between two frames as it passed.
/// </summary>
/// <remarks>It calls no engine API, so a test drives it with plain readings.</remarks>
public sealed class RealFrameClock
{
    private const string PreviousField = "previous";
    private const string NowField = "now";
    private const string BackwardMessage = "The engine clock gave a reading below the reading of the frame before, and the clock of a frame never runs backward.";

    private long previous = -1;

    /// <summary>
    /// The microseconds since the reading of the frame before, or null for the first reading. The first frame has no
    /// frame before it, so its time is absent, and never a zero (T-2).
    /// </summary>
    /// <exception cref="ContextException">The reading is below the reading before it.</exception>
    public long? Next(long nowMicros)
    {
        if (this.previous < 0)
        {
            this.previous = nowMicros;
            return null;
        }

        if (nowMicros < this.previous)
        {
            ContextException error = new(BackwardMessage);
            error.AddContext(PreviousField, this.previous.ToString(System.Globalization.CultureInfo.InvariantCulture));
            error.AddContext(NowField, nowMicros.ToString(System.Globalization.CultureInfo.InvariantCulture));
            throw error;
        }

        long elapsed = nowMicros - this.previous;
        this.previous = nowMicros;
        return elapsed;
    }
}
