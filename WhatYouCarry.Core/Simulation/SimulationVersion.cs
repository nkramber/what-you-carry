namespace WhatYouCarry.Core.Simulation;

/// <summary>
/// The version of the simulation (D-151, G-20). A run record carries the value that wrote it, and a replay is
/// exact only when the value matches this constant.
/// </summary>
/// <remarks>
/// Every Core change that moves a simulation number raises this value by one, and the review confirms the raise
/// (G-20). The bit-identity known answer moves with it, so a change that forgets the raise fails the test first.
/// </remarks>
public static class SimulationVersion
{
    /// <summary>The current value. PR-6 set it to 1.</summary>
    public const int Value = 1;
}
