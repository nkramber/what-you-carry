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
