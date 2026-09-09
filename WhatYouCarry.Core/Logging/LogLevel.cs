namespace WhatYouCarry.Core.Logging;

/// <summary>
/// The severity of one log line (D-212). Every line carries one, so a crash report and a night sweep can read
/// the file at more than one weight (PR-55, PR-11).
/// </summary>
public enum LogLevel
{
    /// <summary>A detail that only a session under investigation needs.</summary>
    Debug = 0,

    /// <summary>An ordinary event of the run or the hub.</summary>
    Info = 1,

    /// <summary>A condition that the code handled, and that a reader must know about.</summary>
    Warning = 2,

    /// <summary>A failure. The code did not do what the caller asked.</summary>
    Error = 3,
}
