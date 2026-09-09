namespace WhatYouCarry.Core.Logging;

/// <summary>
/// Where a finished log line goes (D-211). Core builds the line and never opens a file, so a disk failure never
/// reaches the simulation thread (D-72, G-1).
/// </summary>
/// <remarks>
/// The Game layer writes the line to the user directory, and a test collects the lines in a list. Those are the
/// two concrete callers that D-111 asks for before an interface.
/// </remarks>
public interface ILogSink
{
    /// <summary>Takes one finished line. The line holds one JSON object and no line break.</summary>
    void Write(string line);
}
