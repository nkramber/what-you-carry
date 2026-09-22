using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Simulation;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Audio;

/// <summary>One sound to play after a tick: its cue and its pitch ratio.</summary>
public sealed record SoundPlay(SoundCue Cue, float Pitch);

/// <summary>
/// Chooses the sounds of each tick from the loop (D-454): a cue for each action event, a cue for the expiry and the
/// hunter spawn, a footstep on the stride rhythm of the player, and a hunter step on the stride rhythm of the
/// Overseer at the pitch of its speed (D-455). It reads the loop, and it holds no engine type.
/// </summary>
/// <remarks>
/// The stride of the player follows its speed (D-463), and the Overseer keeps one stride. A roll and a move in the air
/// make no footstep. A descent resets both rhythms, so the move to the spawn of the next floor makes no step.
/// </remarks>
public sealed class SoundDirector
{
    /// <summary>The distance of one step of the Overseer, in meters. It is tall, so its step is long and slow (D-409).</summary>
    public const float HunterStepMeters = 1.4f;

    private readonly StrideClock footsteps = new();
    private readonly StrideClock hunterSteps = new();
    private readonly float hunterStartSpeed;
    private int floor;
    private CoreVector3 lastFeet;
    private bool hasLastFeet;
    private float playerSpeed;

    /// <summary>A director for one run.</summary>
    /// <param name="hunterStartSpeed">The speed of the Overseer at expiry, in meters per second, from its definition (D-408).</param>
    public SoundDirector(float hunterStartSpeed)
    {
        this.hunterStartSpeed = hunterStartSpeed;
    }

    /// <summary>The horizontal speed of the player on the last tick, in meters each second, and zero on the first tick of a floor.</summary>
    private float SpeedOf(CoreVector3 feet)
    {
        if (!this.hasLastFeet)
        {
            this.lastFeet = feet;
            this.hasLastFeet = true;
            return 0.0f;
        }

        float stepX = feet.X - this.lastFeet.X;
        float stepZ = feet.Z - this.lastFeet.Z;
        this.lastFeet = feet;
        return MathF.Sqrt((stepX * stepX) + (stepZ * stepZ)) / PlayerBody.TickSeconds;
    }

    /// <summary>The sounds of the tick that the loop just ran, in event order, then the footstep, then the hunter step.</summary>
    /// <exception cref="WhatYouCarry.Core.Logging.ContextException">An event has a kind or a mark that no cue names (T-2).</exception>
    public IReadOnlyList<SoundPlay> AfterTick(SimulationLoop loop)
    {
        List<SoundPlay> plays = [];
        if (loop.Floor != this.floor)
        {
            this.floor = loop.Floor;
            this.footsteps.Reset();
            this.hunterSteps.Reset();
            this.hasLastFeet = false;
        }

        this.playerSpeed = this.SpeedOf(loop.Body.Position);
        foreach (ActionEvent action in loop.LastActions)
        {
            plays.Add(new SoundPlay(SoundCues.Of(action), 1.0f));
        }

        foreach (TimerEvent timerEvent in loop.LastEvents)
        {
            if (SoundCues.TryOf(timerEvent, out SoundCue cue))
            {
                plays.Add(new SoundPlay(cue, 1.0f));
            }
        }

        bool playerGrounded = loop.Player.RollRemaining == 0 && loop.Body.IsOnGround();
        float speed = this.playerSpeed;
        if (this.footsteps.Advance(loop.Body.Position, playerGrounded, FootstepCadence.StrideMeters(speed)))
        {
            plays.Add(new SoundPlay(SoundCue.Footstep, 1.0f));
        }

        Hunter? hunter = loop.Hunter;
        if (hunter is null)
        {
            this.hunterSteps.Reset();
        }
        else if (this.hunterSteps.Advance(hunter.Body.Position, hunter.Body.IsOnGround(), HunterStepMeters))
        {
            plays.Add(new SoundPlay(SoundCue.HunterStep, HunterPitch.Ratio(hunter.Speed, this.hunterStartSpeed)));
        }

        return plays;
    }
}
