using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Simulation;

/// <summary>
/// The fixed-step loop (D-73). One call to <see cref="Step"/> is one tick, and the loop reads no clock: the Game
/// layer counts real time and calls <see cref="Step"/> once for each tick that the count covers.
/// </summary>
/// <remarks>
/// <para>
/// The state is the seed, the tick, the yaw and pitch sums in hundredths of a degree, the buttons of the last
/// intent (D-227), and after them the position and the vertical velocity of the player body (PR-7). The yaw
/// wraps at a full turn, and the pitch stops at 80 degrees up and 80 degrees down (D-241). A positive pitch
/// looks up (D-248). The camera and the aim ray come from the state on demand, and they are not state (D-245).
/// </para>
/// <para>
/// The look deltas apply first, and the body then moves by the yaw sum after them. The grid and the spawn point
/// come from the caller, because no generator exists before PR-9 (D-236). The grid is an input like the content
/// and not state, so the hash reads the body and not the blocks.
/// </para>
/// <para>
/// The loop rejects an intent whose tick is not the next one, because a replay that stepped over a frame would
/// diverge in silence, and it rejects a set reserved button bit (T-2, G-5, D-232).
/// </para>
/// </remarks>
public sealed class SimulationLoop
{
    /// <summary>The count of ticks in one second (D-73).</summary>
    public const int TicksPerSecond = 60;

    /// <summary>One full turn of yaw, in hundredths of a degree.</summary>
    public const int FullTurn = 36000;

    /// <summary>The largest pitch magnitude, in hundredths of a degree: 80 degrees up or down (D-241).</summary>
    public const int PitchLimit = 8000;

    /// <summary>A loop at tick zero for one run, with the body at rest at the spawn point.</summary>
    /// <exception cref="ContextException">The player box at the spawn point overlaps a solid cell or reaches past the grid.</exception>
    public SimulationLoop(ulong seed, VoxelGrid grid, Vector3 spawn)
    {
        this.Seed = seed;
        this.Grid = grid;
        this.Body = new PlayerBody(grid, spawn);
    }

    /// <summary>The seed of the run. Every random stream of the run derives from it (D-159).</summary>
    public ulong Seed { get; }

    /// <summary>The grid of the floor (D-78, D-236).</summary>
    public VoxelGrid Grid { get; }

    /// <summary>The player body (D-149, D-165).</summary>
    public PlayerBody Body { get; }

    /// <summary>The count of ticks that ran, which is also the tick of the next intent.</summary>
    public uint Tick { get; private set; }

    /// <summary>The yaw sum, in hundredths of a degree, from 0 up to but not including <see cref="FullTurn"/>.</summary>
    public int Yaw { get; private set; }

    /// <summary>The pitch sum, in hundredths of a degree, from minus <see cref="PitchLimit"/> to <see cref="PitchLimit"/>. Positive looks up (D-248).</summary>
    public int Pitch { get; private set; }

    /// <summary>The buttons of the last intent, as the bit mask of D-162.</summary>
    public ushort Buttons { get; private set; }

    /// <summary>Runs one tick with one intent.</summary>
    /// <exception cref="ContextException">The intent is not for the next tick, the tick counter is full, or the intent sets a reserved button bit.</exception>
    public void Step(Intent intent)
    {
        if (intent.Tick != this.Tick)
        {
            ContextException outOfOrder = new($"The loop is at tick {this.Tick}, and the intent is for tick {intent.Tick}. Every tick takes one intent, in order (G-5).");
            outOfOrder.AddContext("expectedTick", ((long)this.Tick).ToString(CultureInfo.InvariantCulture));
            outOfOrder.AddContext("intentTick", ((long)intent.Tick).ToString(CultureInfo.InvariantCulture));
            throw outOfOrder;
        }

        // The counter would wrap to zero in silence, and a tick that repeats an old number breaks the record order.
        if (this.Tick == uint.MaxValue)
        {
            throw new ContextException($"The tick counter is full at {this.Tick}, and the loop cannot run another tick.");
        }

        // A reserved bit comes from a build that assigned it, and this build cannot read it (D-232).
        if ((intent.Buttons & Button.ReservedMask) != 0)
        {
            string buttons = "0x" + ((ulong)intent.Buttons).ToString("x4", CultureInfo.InvariantCulture);
            ContextException reserved = new($"The intent at tick {intent.Tick} sets a reserved button bit. The buttons are {buttons}, and bits 8 to 15 are reserved (D-232).");
            reserved.AddContext("tick", ((long)intent.Tick).ToString(CultureInfo.InvariantCulture));
            reserved.AddContext("buttons", buttons);
            throw reserved;
        }

        int yaw = (this.Yaw + intent.YawDelta) % FullTurn;
        if (yaw < 0)
        {
            yaw += FullTurn;
        }

        int pitch = this.Pitch + intent.PitchDelta;
        if (pitch > PitchLimit)
        {
            pitch = PitchLimit;
        }
        else if (pitch < -PitchLimit)
        {
            pitch = -PitchLimit;
        }

        this.Yaw = yaw;
        this.Pitch = pitch;
        this.Buttons = intent.Buttons;
        this.Body.Step(intent, yaw);
        this.Tick++;
    }

    /// <summary>The camera pose for the state of this tick (D-245). Every call with one state gives one pose.</summary>
    public CameraPose Camera()
    {
        return OrbitCamera.Place(this.Grid, this.Body.Position, this.Yaw, this.Pitch);
    }

    /// <summary>
    /// The aim ray for the state of this tick: the crosshair from the camera, pulled toward a target when the
    /// last intent set the controller aim bit (D-243, D-244, D-247).
    /// </summary>
    /// <param name="targets">The target points, which PR-16 takes from the enemies.</param>
    public AimRay Aim(IReadOnlyList<Vector3> targets)
    {
        CameraPose pose = this.Camera();
        bool controllerAim = (this.Buttons & Button.ControllerAim) != 0;
        return AimAssist.Apply(new AimRay(pose.Position, pose.Forward), targets, controllerAim);
    }

    /// <summary>
    /// The hash of the whole state, in the declared field order (D-160): the five fields of D-227, then the
    /// position and the vertical velocity of the body. A new field goes after these, so the order of every
    /// earlier one stands.
    /// </summary>
    public StateHash Hash()
    {
        StateHash hash = StateHash.Start();
        hash.Add(this.Seed);
        hash.Add(this.Tick);
        hash.Add(this.Yaw);
        hash.Add(this.Pitch);
        hash.Add((uint)this.Buttons);
        hash.Add(this.Body.Position.X);
        hash.Add(this.Body.Position.Y);
        hash.Add(this.Body.Position.Z);
        hash.Add(this.Body.VerticalVelocity);
        return hash;
    }
}
