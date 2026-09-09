namespace WhatYouCarry.Core.Replay;

/// <summary>
/// Where the bytes of a run record go (D-225). Core builds the header and each frame and opens no file, so a
/// disk failure never reaches the simulation thread (D-72, G-1).
/// </summary>
/// <remarks>
/// The Game layer appends the bytes to the record file in the user directory (PR-31), and a test collects them
/// in memory. Those are the two concrete callers that D-111 asks for before an interface. The same rule holds
/// for the logger (D-211) and the content source (D-219).
/// </remarks>
public interface IRunRecordSink
{
    /// <summary>Takes the next bytes of the record. The first call carries the header line, and each later call carries one frame.</summary>
    void Append(byte[] bytes);
}
