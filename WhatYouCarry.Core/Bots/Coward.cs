using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Bots;

/// <summary>
/// The coward (D-149): it walks to the first stairwell and ascends there, so its run ends as an ascend on floor 1
/// (D-430). It promises progress, so a coward that does not reach the stairwell before expiry ends as a softlock
/// (D-420, D-433).
/// </summary>
/// <remarks>
/// The walk, the swing, and the roll are the ones of the greedy descender with floor 1 as its last floor. A second
/// copy of the walk would drift from the first, and the two policies test one path. The policy draws nothing, so
/// it holds no stream (D-272).
/// </remarks>
public sealed class Coward : IBotPolicy
{
    /// <summary>The name of the policy in the run log.</summary>
    public const string PolicyName = "coward";

    private readonly GreedyDescender walk = new(SimulationLoop.FirstFloor);

    /// <inheritdoc/>
    public string Name => PolicyName;

    /// <inheritdoc/>
    public bool PromisesProgress => true;

    /// <inheritdoc/>
    public Intent Next(SimulationLoop loop)
    {
        return this.walk.Next(loop);
    }
}
