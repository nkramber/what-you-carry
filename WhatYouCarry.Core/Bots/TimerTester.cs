using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Bots;

/// <summary>
/// The timer tester (D-149, D-421): it stands at the floor entry through the expiry and after it, and it never runs
/// and never fights. Its run proves that a wait is never safe, because the Overseer finds a player who waits and
/// kills it (D-45).
/// </summary>
/// <remarks>
/// The policy promises no progress, so a floor that it never leaves reads no softlock at expiry (D-420), and its
/// run ends by death or by the wander budget (D-270). It draws nothing, so it holds no stream (D-272).
/// </remarks>
public sealed class TimerTester : IBotPolicy
{
    /// <summary>The name of the policy in the run log.</summary>
    public const string PolicyName = "timer-tester";

    /// <inheritdoc/>
    public string Name => PolicyName;

    /// <inheritdoc/>
    public bool PromisesProgress => false;

    /// <inheritdoc/>
    public Intent Next(SimulationLoop loop)
    {
        return new Intent(loop.Tick, 0, 0, 0, 0, 0);
    }
}
