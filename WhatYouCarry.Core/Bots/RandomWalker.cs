using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Bots;

/// <summary>
/// The random walker (D-127, D-149): it holds one movement, one yaw rate, and one jump choice for a random
/// number of ticks, then draws again. It promises no progress, so its run ends by budget (D-270).
/// </summary>
public sealed class RandomWalker : IBotPolicy
{
    /// <summary>The name of the policy in the run log.</summary>
    public const string PolicyName = "random-walker";

    /// <summary>The longest hold of one choice, in ticks.</summary>
    public const int LongestHold = 60;

    /// <summary>The largest yaw rate magnitude, in hundredths of a degree per tick: three degrees.</summary>
    public const int LargestYawRate = 300;

    private readonly Rng rng;
    private int remaining;
    private sbyte moveX;
    private sbyte moveY;
    private short yawRate;
    private bool jump;

    /// <summary>A walker that draws from the Bot stream of the run seed (D-272).</summary>
    public RandomWalker(ulong seed)
    {
        this.rng = Rng.ForStream(seed, RngStream.Bot);
    }

    /// <inheritdoc/>
    public string Name => PolicyName;

    /// <inheritdoc/>
    public bool PromisesProgress => false;

    /// <inheritdoc/>
    public Intent Next(SimulationLoop loop)
    {
        if (this.remaining == 0)
        {
            this.moveX = (sbyte)(this.rng.NextInt(255) - 127);
            this.moveY = (sbyte)(this.rng.NextInt(255) - 127);
            this.yawRate = (short)(this.rng.NextInt((2 * LargestYawRate) + 1) - LargestYawRate);
            this.jump = this.rng.NextInt(4) == 0;
            this.remaining = 1 + this.rng.NextInt(LongestHold);
        }

        this.remaining--;
        return new Intent(loop.Tick, this.yawRate, 0, this.moveX, this.moveY, this.jump ? Button.Jump : (ushort)0);
    }
}
