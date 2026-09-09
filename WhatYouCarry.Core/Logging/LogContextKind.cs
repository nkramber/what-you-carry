namespace WhatYouCarry.Core.Logging;

/// <summary>
/// Which field set one log line must carry (D-113). The two sets never mix, because a line inside a run names
/// a tick and a line outside one names a screen.
/// </summary>
public enum LogContextKind
{
    /// <summary>Inside a run. The line carries the seed, the floor, the tick, the subsystem, and the entity ids.</summary>
    Run = 0,

    /// <summary>Outside a run. The line carries the save versions, the screen, the action, and the file paths.</summary>
    Hub = 1,
}
