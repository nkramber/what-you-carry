using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Bots;

/// <summary>
/// A bot policy (D-127, D-149): a simple deterministic player that gives the loop one intent per tick. A policy
/// draws from the Bot stream of its run seed and never from a simulation stream, so a bot run replays like a
/// player run (D-272).
/// </summary>
/// <remarks>
/// The random walker and the greedy descender are the two concrete policies of PR-11 (D-111). Later PRs add a
/// policy with the system it exercises. A policy holds its own state, such as a path or a count of ticks, and
/// that state is not simulation state: the record holds the intents, and a replay needs no policy.
/// </remarks>
public interface IBotPolicy
{
    /// <summary>The name that the run log carries (D-127).</summary>
    string Name { get; }

    /// <summary>
    /// Answers whether the policy promises floor progress. A run of such a policy that makes none inside the floor
    /// budget ends as a softlock, and a run of any other policy ends by its budget (D-270).
    /// </summary>
    bool PromisesProgress { get; }

    /// <summary>The intent of the next tick, for the state of the loop after the last one.</summary>
    Intent Next(SimulationLoop loop);
}
