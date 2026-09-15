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
    /// <summary>The current value. PR-6 set it to 1, PR-7 raised it to 2 when the state gained a position, PR-8 raised it to 3 when the pitch clamp changed, PR-9 raised it to 4 when the state gained the floor number and the run end, PR-59 raised it to 5 when the detail pass changed every dug floor (D-260), PR-10 raised it to 6 when the state gained the projectiles, PR-15 raised it to 7 when the state gained the player and the run end kind, and the sprint speed changed (D-319, D-322), PR-63 raised it to 8 when the dig sizes of D-341 changed every dug floor (D-260, G-20), PR-67 raised it to 9 when the dig restart of D-359 changed each floor that needs more than 1000 jobs (D-260, G-20), and PR-64 raised it to 10 when the ramp cells of D-367 and the motion on a ramp of D-362 to D-366 entered the collision (D-345, G-20).</summary>
    public const int Value = 10;
}
