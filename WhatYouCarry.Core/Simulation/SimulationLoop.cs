using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Simulation;

/// <summary>
/// The fixed-step loop (D-73). One call to <see cref="Step"/> is one tick, and the loop reads no clock: the Game
/// layer counts real time and calls <see cref="Step"/> once for each tick that the count covers.
/// </summary>
/// <remarks>
/// <para>
/// The state is the seed, the tick, the yaw and pitch sums in hundredths of a degree, the buttons of the last
/// intent (D-227), then the position and the vertical velocity of the player body (PR-7), then the floor number
/// and the run end (PR-9, D-322), then the projectiles in flight (PR-10), then the player: the health, the dodge
/// cooldown, the roll, the swing, the stagger, and the guard (PR-15). The yaw wraps at a full turn, and the pitch
/// stops at 80 degrees up and 80 degrees down (D-241). A positive pitch looks up (D-248). The camera and the aim
/// ray come from the state on demand, and they are not state (D-245).
/// </para>
/// <para>
/// The attack bit swings the main weapon: the first weapon definition of the content set, until the loadout of
/// PR-30 (D-320). No intent fires a projectile before PR-24, and the projectiles of a floor end with the floor.
/// No enemy exists before PR-16, so the blade has no target box yet.
/// </para>
/// <para>
/// The floor comes from the seed, the floor number, and the content set, so the grid and the spawn are inputs
/// like the content and not state, and the hash reads the body and not the blocks (D-236). At the stairwell,
/// the interact bit digs the next floor and the ascend bit ends the run (D-257). Health alone carries to the next
/// floor (D-335). A run ends as a death when the health of the player reaches zero (D-322). A loop whose run
/// ended takes no intent, because an intent after the end has no tick to run on (T-2).
/// </para>
/// <para>
/// The look deltas apply first, and the player then runs its tick with the yaw sum after them. The projectiles
/// run after the player, and the stairwell reads the body last. The loop rejects an intent whose tick is not the
/// next one, because a replay that stepped over a frame would diverge in silence, and it rejects a set reserved
/// button bit (T-2, G-5, D-232).
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

    /// <summary>The floor that every run starts at (D-3).</summary>
    public const int FirstFloor = 1;

    /// <summary>The owner id of the player in the projectile simulation.</summary>
    public const int PlayerOwner = 0;

    /// <summary>The target boxes of the blade. PR-16 takes them from the enemies, and no enemy exists before it.</summary>
    private static readonly EntityBox[] NoTargets = [];

    private readonly ContentSet content;
    private bool ascended;

    /// <summary>A loop at tick zero for one run, on floor 1 of the seed, with the player at rest at the spawn point.</summary>
    /// <exception cref="ContextException">The content set holds no weapon definition, or it cannot dig floor 1.</exception>
    public SimulationLoop(ulong seed, ContentSet content)
    {
        this.Seed = seed;
        this.content = content;
        this.Weapon = MainWeapon(content);
        this.Plan = FloorGenerator.Generate(seed, FirstFloor, content);
        this.Player = new Player(this.Plan.Grid, this.Plan.Spawn, this.Weapon, Player.MaxHealth);
        this.Projectiles = new ProjectileSimulation(this.Plan.Grid, content.Projectiles);
    }

    /// <summary>
    /// The main weapon of a content set: the first weapon definition, in the ordinal path order of the typed lists,
    /// until the loadout of PR-30 (D-320). The Game layer reads the same rule for the model and the clips.
    /// </summary>
    /// <exception cref="ContextException">The content set holds no weapon definition (T-2).</exception>
    public static WeaponDefinition MainWeapon(ContentSet content)
    {
        if (content.Weapons.Count == 0)
        {
            throw new ContextException("The content set holds no weapon definition, and the attack bit swings the first one (D-320).");
        }

        return content.Weapons[0];
    }

    /// <summary>The seed of the run. Every random stream of the run derives from it (D-159).</summary>
    public ulong Seed { get; }

    /// <summary>The main weapon of the run: the first weapon definition of the content set (D-320).</summary>
    public WeaponDefinition Weapon { get; }

    /// <summary>The dug floor that the body stands in (D-253).</summary>
    public FloorPlan Plan { get; private set; }

    /// <summary>The grid of the floor (D-78, D-236).</summary>
    public VoxelGrid Grid => this.Plan.Grid;

    /// <summary>The player (PR-15). A descent puts a new player at the spawn of the next floor with the same health (D-335).</summary>
    public Player Player { get; private set; }

    /// <summary>The body of the player (D-149, D-165).</summary>
    public PlayerBody Body => this.Player.Body;

    /// <summary>The projectiles of the floor (G-6). A descent starts an empty simulation on the next floor.</summary>
    public ProjectileSimulation Projectiles { get; private set; }

    /// <summary>The projectiles that ended on the last tick, in flight order. The Game layer reads the hit points from it. It is not state.</summary>
    public IReadOnlyList<ProjectileEnd> LastEnds { get; private set; } = [];

    /// <summary>The floor number, from one (D-3). The state holds it, and the hash reads it after the body.</summary>
    public int Floor { get; private set; } = FirstFloor;

    /// <summary>How the run ended: an ascend at the stairwell (D-50), a death at zero health (D-322), or no end yet.</summary>
    public RunEnd End => this.ascended ? RunEnd.Ascend : (this.Player.IsDead ? RunEnd.Death : RunEnd.None);

    /// <summary>Answers whether the run ended. An ended loop takes no intent.</summary>
    public bool Ended => this.End != RunEnd.None;

    /// <summary>The count of ticks that ran, which is also the tick of the next intent.</summary>
    public uint Tick { get; private set; }

    /// <summary>The yaw sum, in hundredths of a degree, from 0 up to but not including <see cref="FullTurn"/>.</summary>
    public int Yaw { get; private set; }

    /// <summary>The pitch sum, in hundredths of a degree, from minus <see cref="PitchLimit"/> to <see cref="PitchLimit"/>. Positive looks up (D-248).</summary>
    public int Pitch { get; private set; }

    /// <summary>The buttons of the last intent, as the bit mask of D-162.</summary>
    public ushort Buttons { get; private set; }

    /// <summary>Runs one tick with one intent.</summary>
    /// <exception cref="ContextException">The run ended, the intent is not for the next tick, the tick counter is full, the intent sets a reserved button bit, or the content set cannot dig the next floor.</exception>
    public void Step(Intent intent)
    {
        if (this.Ended)
        {
            ContextException ended = new($"The run ended at tick {this.Tick} on floor {this.Floor}, and the loop takes no intent after the end (D-50, D-322).");
            ended.AddContext("tick", ((long)this.Tick).ToString(CultureInfo.InvariantCulture));
            ended.AddContext("floor", ((long)this.Floor).ToString(CultureInfo.InvariantCulture));
            ended.AddContext("end", EndText(this.End));
            throw ended;
        }

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
            ContextException reserved = new($"The intent at tick {intent.Tick} sets a reserved button bit. The buttons are {buttons}, and bits 10 to 15 are reserved (D-232, D-257).");
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

        ushort previousButtons = this.Buttons;
        this.Yaw = yaw;
        this.Pitch = pitch;
        this.Buttons = intent.Buttons;
        this.Player.Step(intent, previousButtons, yaw, NoTargets);

        EntityBox[] boxes = [new EntityBox(PlayerOwner, this.Body.Box)];
        this.LastEnds = this.Projectiles.Step(boxes);
        this.Tick++;

        StairwellAction action = StairwellTransition.Choose(intent.Buttons, this.Body, this.Plan.Stairwell);
        if (action == StairwellAction.Ascend)
        {
            this.ascended = true;
        }
        else if (action == StairwellAction.Descend)
        {
            this.Descend();
        }
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
    /// position and the vertical velocity of the body, then the floor number and the run end as one byte, then the
    /// projectiles in flight order, then the player. A new field goes after these, so the order of every earlier one
    /// stands.
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
        hash.Add(this.Floor);
        hash.Add((byte)this.End);
        this.Projectiles.AddTo(ref hash);
        this.Player.AddTo(ref hash);
        return hash;
    }

    /// <summary>The name of a run end in a message. The switch is explicit, so no reflection reads the enum (G-2).</summary>
    private static string EndText(RunEnd end)
    {
        switch (end)
        {
            case RunEnd.Ascend: return "ascend";
            case RunEnd.Death: return "death";
            default: return "none";
        }
    }

    /// <summary>Digs the next floor from the run seed and the next floor number, and puts a player at rest at its spawn with the same health (D-257, D-335).</summary>
    private void Descend()
    {
        int next = this.Floor + 1;
        this.Plan = FloorGenerator.Generate(this.Seed, next, this.content);
        this.Player = new Player(this.Plan.Grid, this.Plan.Spawn, this.Weapon, this.Player.Health);
        this.Projectiles = new ProjectileSimulation(this.Plan.Grid, this.content.Projectiles);
        this.Floor = next;
    }
}
