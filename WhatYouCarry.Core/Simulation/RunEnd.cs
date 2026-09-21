namespace WhatYouCarry.Core.Simulation;

/// <summary>How a run ended, when it did (D-50, D-257, D-322). The state hash reads the kind as one byte.</summary>
public enum RunEnd : byte
{
    /// <summary>The run goes on.</summary>
    None = 0,

    /// <summary>The run ascended at a stairwell.</summary>
    Ascend = 1,

    /// <summary>The health of the player reached zero.</summary>
    Death = 2,
}

/// <summary>The name of a run end in a log line or a message (D-322, D-403).</summary>
public static class RunEnds
{
    /// <summary>The name of one end. The switch is explicit, so no reflection reads the enum (G-2).</summary>
    public static string TextOf(RunEnd end)
    {
        switch (end)
        {
            case RunEnd.Ascend: return "ascend";
            case RunEnd.Death: return "death";
            default: return "none";
        }
    }
}
