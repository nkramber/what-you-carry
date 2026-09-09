using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Procgen;

namespace WhatYouCarry.Core.Simulation;

/// <summary>
/// The choice at the stairwell (D-50, D-257): the interact bit descends, and the ascend bit ends the run. Both
/// act at the stairwell alone, and both reach the record through the intent stream, so a replay reproduces the
/// choice with no frame of another kind (G-5).
/// </summary>
/// <remarks>
/// The body is at the stairwell when it stands on the ground and the block under its feet center is the
/// stairwell cell. The ground probe is the one of <see cref="PlayerBody"/>, so a body that rests on the cell
/// reads it on every tick. An intent that sets both bits ascends, because the run end is the choice that costs
/// nothing to take back on the next run (D-50). A later decision can change that order without a change to
/// the record layout.
/// </remarks>
public static class StairwellTransition
{
    /// <summary>Answers whether the body stands on the stairwell cell.</summary>
    public static bool IsAtStairwell(PlayerBody body, Cell stairwell)
    {
        if (!body.IsOnGround())
        {
            return false;
        }

        int x = (int)DetMath.Floor(body.Position.X);
        int y = (int)DetMath.Floor(body.Position.Y - PlayerBody.GroundProbe);
        int z = (int)DetMath.Floor(body.Position.Z);
        return x == stairwell.X && y == stairwell.Y && z == stairwell.Z;
    }

    /// <summary>The action that the buttons of an intent take at the stairwell. Away from it, none.</summary>
    public static StairwellAction Choose(ushort buttons, PlayerBody body, Cell stairwell)
    {
        if (!IsAtStairwell(body, stairwell))
        {
            return StairwellAction.None;
        }

        if ((buttons & Button.Ascend) != 0)
        {
            return StairwellAction.Ascend;
        }

        if ((buttons & Button.Interact) != 0)
        {
            return StairwellAction.Descend;
        }

        return StairwellAction.None;
    }
}

/// <summary>What the player does at the stairwell on one tick (D-257).</summary>
public enum StairwellAction
{
    /// <summary>Nothing. The body is away from the stairwell, or neither bit is set.</summary>
    None = 0,

    /// <summary>The run goes on at the next floor (D-3).</summary>
    Descend = 1,

    /// <summary>The run ends (D-50).</summary>
    Ascend = 2,
}
