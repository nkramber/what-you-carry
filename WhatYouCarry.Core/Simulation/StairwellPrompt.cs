namespace WhatYouCarry.Core.Simulation;

/// <summary>
/// The stairwell prompt (D-50, D-140, D-431): the untimed choice to descend or to ascend. The prompt is open while
/// the body stands on the stairwell cell of a run that goes on, and it closes when the body leaves the cell.
/// </summary>
/// <remarks>
/// The prompt reads the same test as the pause of the countdown, so the countdown holds on each tick that the
/// prompt is open (D-140). The prompt does not hold the player, and the Overseer and the waves still attack
/// (D-417). The interact bit or the ascend bit of an intent answers it (D-257), from a player or a bot policy.
/// The prompt follows from the body and the plan, so it is not state, and the hash does not read it.
/// </remarks>
public static class StairwellPrompt
{
    /// <summary>Answers whether the prompt is open for the state of the loop.</summary>
    public static bool IsOpen(SimulationLoop loop)
    {
        return !loop.Ended && StairwellTransition.IsAtStairwell(loop.Body, loop.Plan.Stairwell);
    }
}
