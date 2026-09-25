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
    /// <summary>The current value. PR-6 set it to 1, PR-7 raised it to 2 when the state gained a position, PR-8 raised it to 3 when the pitch clamp changed, PR-9 raised it to 4 when the state gained the floor number and the run end, PR-59 raised it to 5 when the detail pass changed every dug floor (D-260), PR-10 raised it to 6 when the state gained the projectiles, PR-15 raised it to 7 when the state gained the player and the run end kind, and the sprint speed changed (D-319, D-322), PR-63 raised it to 8 when the dig sizes of D-341 changed every dug floor (D-260, G-20), PR-67 raised it to 9 when the dig restart of D-359 changed each floor that needs more than 1000 jobs (D-260, G-20), PR-64 raised it to 10 when the ramp cells of D-367 and the motion on a ramp of D-362 to D-366 entered the collision (D-345, G-20), and PR-68 raised it to 11 when the pillar rule of F-101 moved every floor that held a pillar in the column of a shaft (D-260, G-20), PR-66 raised it to 12 when the ramps of D-347 and the chamber tiers of D-348 changed every dug floor (D-260, G-20), PR-16 raised it to 13 when the enemies of D-395 to D-402 entered the state (G-20), PR-17 raised it to 14 when the floor timer, the Overseer, and the waves of D-407 to D-425 entered the state (G-20), and PR-72 raised it to 15 when the path search took the diagonal move of D-486 and the jump rule read the slope under the feet (F-107, F-108, G-20), and PR-81 raised it to 16 when a diagonal drop needed an open fall in the corner column and a waypoint arrival started the wedge count again, and the sweep read the box of the caller (D-545, D-546, D-549, F-111, F-112, G-20), and PR-88 raised it to 17 when a death on the tick of a stairwell press stayed a death and a descend on the deepest floor did nothing (D-322, D-579, G-20), and the state hash gained the stored path and the wedge count of each path follower (D-160, F-123).</summary>
    public const int Value = 17;
}
