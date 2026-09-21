using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Combat;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The player of PR-15: the roll, the swing and its arc, the stagger and its guard, health, and death (D-315, D-316,
/// D-319 to D-329, D-335, D-337; PR-15 exit tests 1 to 4 and 6). The rule tests step a player on a flat floor with an
/// explicit yaw, and the replay test runs the loop on a dug floor.
/// </summary>
public sealed class PlayerTests
{
    private static readonly EntityBox[] NoTargets = [];

    /// <summary>The feet of a player in the middle of the flat floor of 16 blocks.</summary>
    private static readonly Vector3 Feet = new(8.5f, 1.0f, 8.5f);

    /// <summary>The main weapon of the repository content: the tier-0 sword (D-315, D-320).</summary>
    private static WeaponDefinition Sword => SimulationLoop.MainWeapon(TestWorld.Content);

    /// <summary>A player at rest in the middle of a flat floor of 16 blocks, with full health.</summary>
    private static Player NewPlayer(WeaponDefinition? weapon = null)
    {
        return new Player(TestWorld.FlatFloor(16, 6), Feet, weapon ?? Sword, Player.MaxHealth);
    }

    /// <summary>A box of the size of the player body, standing at a point.</summary>
    private static Aabb BodyBoxAt(Vector3 feet, float halfWidth = PlayerBody.HalfWidth)
    {
        return new Aabb(feet - new Vector3(halfWidth, 0.0f, halfWidth), feet + new Vector3(halfWidth, PlayerBody.Height, halfWidth));
    }

    /// <summary>
    /// A floor of stone with land one block high, and a pool of still water in the land from x 6 to 13 (D-258). Feet on
    /// the land stand at 2, and feet in the pool stand at 1.
    /// </summary>
    private static VoxelGrid PoolFloor()
    {
        VoxelGrid grid = new(16, 6, 16);
        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
                grid.Set(x, 1, z, x >= 6 && x <= 13 ? BlockId.StillWater : BlockId.RawStone);
            }
        }

        return grid;
    }

    /// <summary>A driver that steps a player and keeps the buttons of the tick before, so a set bit after a clear one is a press (D-323).</summary>
    private sealed class Driver
    {
        private ushort previous;

        public Driver(Player player)
        {
            this.Player = player;
        }

        public Player Player { get; }

        public void Tick(ushort buttons, sbyte strafe = 0, sbyte forward = 0, int yaw = 0, IReadOnlyList<EntityBox>? targets = null)
        {
            this.Player.Step(new Intent(0U, 0, 0, strafe, forward, buttons), this.previous, yaw, targets ?? NoTargets);
            this.previous = buttons;
        }

        public void Ticks(int count, ushort buttons = 0, sbyte strafe = 0, sbyte forward = 0)
        {
            for (int tick = 0; tick < count; tick++)
            {
                this.Tick(buttons, strafe, forward);
            }
        }
    }

    /// <summary>A sink that keeps the record in memory.</summary>
    private sealed class MemorySink : IRunRecordSink
    {
        public List<byte> Bytes { get; } = [];

        public void Append(byte[] bytes)
        {
            this.Bytes.AddRange(bytes);
        }
    }

    /// <summary>A log sink that keeps the lines.</summary>
    private sealed class CollectingSink : ILogSink
    {
        public List<string> Lines { get; } = [];

        public void Write(string line)
        {
            this.Lines.Add(line);
        }
    }

    /// <summary>The numbers of D-315, D-319, D-325, D-326, and D-327 hold, in Core and in the weapon file.</summary>
    [Fact]
    public void TheConstantsHold()
    {
        Assert.Equal(100, Player.MaxHealth);
        Assert.Equal(18, Player.RollTicks);
        Assert.Equal(3.0f, Player.RollDistance);
        Assert.InRange(Player.RollSpeed, 9.9999f, 10.0001f);
        Assert.Equal(45, Player.DodgeCooldownTicks);
        Assert.Equal(20, Player.StaggerTicks);
        Assert.Equal(30, Player.GuardTicks);
        Assert.Equal(7.0f, PlayerBody.SprintSpeed);

        WeaponDefinition sword = Sword;
        Assert.Equal("sword-basic", sword.Id);
        Assert.Equal(0L, sword.Tier);
        Assert.Equal(WeaponDefinition.OneHanded, sword.Handedness);
        Assert.Equal((12L, 6L, 18L), (sword.WindupTicks, sword.ActiveTicks, sword.RecoveryTicks));
        Assert.Equal(34L, sword.Damage);
        Assert.Equal((160L, 9000, 50L, 170L), (sword.ReachCentimetres, sword.ArcHundredths, sword.LowCentimetres, sword.HighCentimetres));
        Assert.Equal("models/sword-basic.bbmodel", sword.Model);
        Assert.Equal("models/player.sword-swing.json", sword.Animation);
    }

    /// <summary>
    /// PR-15 exit test 1. A press of the dodge bit inside the cooldown does nothing, and the press on the tick that the
    /// cooldown of 45 ticks ends starts the next roll. The cooldown counts from the press (D-316, D-327).
    /// </summary>
    [Fact]
    public void DodgeCooldownHolds()
    {
        Driver driver = new(NewPlayer());
        driver.Tick(Button.Dodge);
        Assert.Equal(Player.RollTicks - 1, driver.Player.RollRemaining);
        Assert.Equal(Player.DodgeCooldownTicks, driver.Player.DodgeCooldown);

        driver.Ticks(19);
        Assert.Equal(0, driver.Player.RollRemaining);
        driver.Tick(Button.Dodge);
        Assert.Equal(0, driver.Player.RollRemaining);
        Assert.Equal(Player.DodgeCooldownTicks - 20, driver.Player.DodgeCooldown);

        driver.Ticks(22);
        driver.Tick(Button.Dodge);
        Assert.Equal(0, driver.Player.RollRemaining);

        driver.Tick(0);
        driver.Tick(Button.Dodge);
        Assert.Equal(Player.RollTicks - 1, driver.Player.RollRemaining);
        Assert.Equal(Player.DodgeCooldownTicks, driver.Player.DodgeCooldown);
    }

    /// <summary>
    /// A roll moves 3 meters over its 18 ticks in the direction of the movement input, from the camera yaw, and backward
    /// with no input. A small deflection rolls as far as a full one (D-315, D-327).
    /// </summary>
    [Theory]
    [InlineData(127, 0, 0, 3.0f, 0.0f)]
    [InlineData(1, 0, 0, 3.0f, 0.0f)]
    [InlineData(0, 127, 0, 0.0f, -3.0f)]
    [InlineData(0, 0, 0, 0.0f, 3.0f)]
    [InlineData(0, 127, 9000, -3.0f, 0.0f)]
    public void ARollMovesThreeMetersInTheInputDirection(int strafe, int forward, int yaw, float expectedX, float expectedZ)
    {
        Driver driver = new(NewPlayer());
        driver.Tick(Button.Dodge, (sbyte)strafe, (sbyte)forward, yaw);
        driver.Ticks(Player.RollTicks + 5);

        Vector3 moved = driver.Player.Body.Position - Feet;
        Assert.InRange(moved.X, expectedX - 1e-3f, expectedX + 1e-3f);
        Assert.InRange(moved.Z, expectedZ - 1e-3f, expectedZ + 1e-3f);
    }

    /// <summary>The movement input during a roll does not steer it (D-327).</summary>
    [Fact]
    public void ARollDoesNotSteer()
    {
        Driver driver = new(NewPlayer());
        driver.Tick(Button.Dodge, 127, 0);
        driver.Ticks(Player.RollTicks - 1, 0, 0, 127);

        Assert.Equal(0, driver.Player.RollRemaining);
        Vector3 moved = driver.Player.Body.Position - Feet;
        Assert.InRange(moved.X, 3.0f - 1e-3f, 3.0f + 1e-3f);
        Assert.Equal(0.0f, moved.Z);
    }

    /// <summary>
    /// A roll needs the ground, and it cannot start in still water: the press does nothing, and the cooldown does not
    /// start. A roll that starts on land keeps its speed over water (D-329, D-337).
    /// </summary>
    [Fact]
    public void ARollNeedsTheGroundAndDryFeet()
    {
        Driver air = new(NewPlayer());
        air.Tick(Button.Jump);
        air.Tick(0);
        air.Tick(Button.Dodge);
        Assert.False(air.Player.Body.IsOnGround());
        Assert.Equal(0, air.Player.RollRemaining);
        Assert.Equal(0, air.Player.DodgeCooldown);

        VoxelGrid pool = PoolFloor();
        Driver wet = new(new Player(pool, new Vector3(9.5f, 1.0f, 8.5f), Sword, Player.MaxHealth));
        wet.Tick(0);
        wet.Tick(Button.Dodge);
        Assert.True(wet.Player.Body.IsInWater());
        Assert.True(wet.Player.Body.IsOnGround());
        Assert.Equal(0, wet.Player.RollRemaining);
        Assert.Equal(0, wet.Player.DodgeCooldown);

        Driver dry = new(new Player(pool, new Vector3(3.5f, 2.0f, 8.5f), Sword, Player.MaxHealth));
        dry.Tick(Button.Dodge, 127, 0);
        dry.Ticks(Player.RollTicks + 30);
        Assert.True(dry.Player.Body.IsInWater());
        Assert.InRange(dry.Player.Body.Position.X, 6.5f - 1e-3f, 6.5f + 1e-3f);
    }

    /// <summary>A sprint in water takes the factor one half: 3.5 meters per second (D-261, D-319).</summary>
    [Fact]
    public void ASprintInWaterIsHalfTheSprint()
    {
        Driver wet = new(new Player(PoolFloor(), new Vector3(9.5f, 1.0f, 8.5f), Sword, Player.MaxHealth));
        wet.Tick(Button.Sprint, 127, 0);

        float expected = 3.5f * PlayerBody.TickSeconds;
        Assert.InRange(wet.Player.Body.Position.X - 9.5f, expected - 1e-6f, expected + 1e-6f);
    }

    /// <summary>A roll cancels a swing, and no swing and no jump start during a roll (D-329).</summary>
    [Fact]
    public void ARollCancelsASwingAndTakesNoSwingOrJump()
    {
        Driver driver = new(NewPlayer());
        driver.Tick(Button.Attack);
        driver.Tick(0);
        Assert.Equal(2L, driver.Player.SwingTick);

        driver.Tick(Button.Dodge);
        Assert.Equal(Player.NoSwing, driver.Player.SwingTick);
        Assert.Equal(Player.RollTicks - 1, driver.Player.RollRemaining);

        driver.Tick(Button.Attack | Button.Jump);
        Assert.Equal(Player.NoSwing, driver.Player.SwingTick);
        Assert.Equal(0.0f, driver.Player.Body.VerticalVelocity);
        Assert.True(driver.Player.Body.IsOnGround());
    }

    /// <summary>
    /// PR-15 exit test 2. A box in the arc takes no hit in the windup or the recovery, and one hit in the active ticks.
    /// A box that covers the whole arc takes its hit on the first active tick, and never a second one in the swing (D-325).
    /// </summary>
    [Fact]
    public void SwordHitsOnlyInActiveFrames()
    {
        WeaponDefinition sword = Sword;
        EntityBox[][] targets =
        [
            [new EntityBox(7, BodyBoxAt(Feet + new Vector3(0.0f, 0.0f, -1.0f)))],
            [new EntityBox(8, BodyBoxAt(Feet + new Vector3(0.0f, 0.0f, -1.0f), 1.5f))],
        ];

        foreach (EntityBox[] target in targets)
        {
            Driver driver = new(NewPlayer());
            List<long> hitTicks = [];
            for (int tick = 0; tick < sword.SwingTicks + 10; tick++)
            {
                driver.Tick(tick == 0 ? Button.Attack : (ushort)0, targets: target);
                foreach (SwordHit hit in driver.Player.LastHits)
                {
                    Assert.Equal(target[0].Owner, hit.Owner);
                    Assert.Equal(sword.Damage, hit.Damage);
                    hitTicks.Add(tick);
                }
            }

            long hitTick = Assert.Single(hitTicks);
            Assert.InRange(hitTick, sword.WindupTicks, sword.WindupTicks + sword.ActiveTicks - 1);
            if (target[0].Owner == 8)
            {
                Assert.Equal(sword.WindupTicks, hitTick);
            }
        }
    }

    /// <summary>The blade turns from the right of the camera yaw to the left: a box at the right front takes the first active step, and one at the left front takes the last (D-325).</summary>
    [Fact]
    public void TheArcSweepsFromRightToLeft()
    {
        WeaponDefinition sword = Sword;
        float angle = 40.0f * MathF.PI / 180.0f;
        Vector3 right = Feet + new Vector3(MathF.Sin(angle), 0.0f, -MathF.Cos(angle));
        Vector3 left = Feet + new Vector3(-MathF.Sin(angle), 0.0f, -MathF.Cos(angle));
        EntityBox[] targets = [new EntityBox(1, BodyBoxAt(right, 0.05f)), new EntityBox(2, BodyBoxAt(left, 0.05f))];

        Driver driver = new(NewPlayer());
        Dictionary<int, long> hitTicks = [];
        for (int tick = 0; tick < sword.SwingTicks; tick++)
        {
            driver.Tick(tick == 0 ? Button.Attack : (ushort)0, targets: targets);
            foreach (SwordHit hit in driver.Player.LastHits)
            {
                hitTicks.Add(hit.Owner, tick);
            }
        }

        Assert.Equal(sword.WindupTicks, hitTicks[1]);
        Assert.Equal(sword.WindupTicks + sword.ActiveTicks - 1, hitTicks[2]);
    }

    /// <summary>The arc follows the camera yaw of each tick: a turn to the left before the active ticks puts a box at the left into the arc (D-324).</summary>
    [Fact]
    public void TheArcFollowsTheYawOfEachTick()
    {
        WeaponDefinition sword = Sword;
        EntityBox[] target = [new EntityBox(3, BodyBoxAt(Feet + new Vector3(-1.0f, 0.0f, 0.0f), 0.05f))];
        int[] turns = [0, 9000];
        int[] hits = new int[turns.Length];
        for (int run = 0; run < turns.Length; run++)
        {
            Driver driver = new(NewPlayer());
            for (int tick = 0; tick < sword.SwingTicks; tick++)
            {
                int yaw = tick >= sword.WindupTicks ? turns[run] : 0;
                driver.Tick(tick == 0 ? Button.Attack : (ushort)0, yaw: yaw, targets: target);
                hits[run] += driver.Player.LastHits.Count;
            }
        }

        Assert.Equal(0, hits[0]);
        Assert.Equal(1, hits[1]);
    }

    /// <summary>A box past the reach, under the band, over the band, or behind the body takes no hit (D-325).</summary>
    [Fact]
    public void TheArcKeepsItsReachAndItsBand()
    {
        WeaponDefinition sword = Sword;
        EntityBox[] targets =
        [
            new EntityBox(1, BodyBoxAt(Feet + new Vector3(0.0f, 0.0f, -1.95f))),
            new EntityBox(2, new Aabb(Feet + new Vector3(-0.3f, 0.0f, -1.3f), Feet + new Vector3(0.3f, 0.5f, -0.7f))),
            new EntityBox(3, new Aabb(Feet + new Vector3(-0.3f, 1.7f, -1.3f), Feet + new Vector3(0.3f, 2.5f, -0.7f))),
            new EntityBox(4, BodyBoxAt(Feet + new Vector3(0.0f, 0.0f, 1.0f))),
        ];

        Driver driver = new(NewPlayer());
        for (int tick = 0; tick < sword.SwingTicks; tick++)
        {
            driver.Tick(tick == 0 ? Button.Attack : (ushort)0, targets: targets);
            Assert.Empty(driver.Player.LastHits);
        }
    }

    /// <summary>The blade offsets span the arc in equal steps, and a blade line points along the frame of D-234.</summary>
    [Fact]
    public void TheBladeFollowsTheFrame()
    {
        WeaponDefinition sword = Sword;
        int[] offsets = new int[sword.ActiveTicks + 1];
        for (int step = 0; step <= sword.ActiveTicks; step++)
        {
            offsets[step] = MeleeWeapon.BladeOffset(sword, step);
        }

        Assert.Equal(new[] { -4500, -3000, -1500, 0, 1500, 3000, 4500 }, offsets);

        Vector3 forward = MeleeWeapon.BladeDirection(0);
        Vector3 rightFront = MeleeWeapon.BladeDirection(-4500);
        Vector3 left = MeleeWeapon.BladeDirection(9000);
        Assert.InRange(forward.Z, -1.0f - 1e-6f, -1.0f + 1e-6f);
        Assert.InRange(rightFront.X, 0.7071f - 1e-4f, 0.7071f + 1e-4f);
        Assert.InRange(rightFront.Z, -0.7071f - 1e-4f, -0.7071f + 1e-4f);
        Assert.InRange(left.X, -1.0f - 1e-6f, -1.0f + 1e-6f);
    }

    /// <summary>
    /// The wedge test is exact on its four cases: a box over the feet, a corner in the wedge, a box that a blade line
    /// crosses with no corner in the wedge, and a box that the rim crosses with no corner in the wedge (D-325).
    /// </summary>
    [Fact]
    public void TheWedgeTestReadsEveryCase()
    {
        WeaponDefinition sword = Sword;
        Vector3 feet = new(0.0f, 0.0f, 0.0f);

        Assert.True(MeleeWeapon.WedgeHits(sword, feet, -750, 750, new Aabb(new Vector3(-0.3f, 0.0f, -0.3f), new Vector3(0.3f, 1.8f, 0.3f))));
        Assert.True(MeleeWeapon.WedgeHits(sword, feet, -750, 750, new Aabb(new Vector3(-0.05f, 0.0f, -1.2f), new Vector3(0.05f, 1.8f, -1.1f))));

        // A thin slab across the right front: its corners stand at 15 and 59 degrees, outside the wedge from 30 to 45.
        Aabb slab = new(new Vector3(0.2f, 0.0f, -0.72f), new Vector3(1.2f, 1.8f, -0.70f));
        Assert.True(MeleeWeapon.WedgeHits(sword, feet, -4500, -3000, slab));

        // A wide box that the rim of 1.6 meters enters from outside: no corner and no blade line reaches it.
        Assert.True(MeleeWeapon.WedgeHits(sword, feet, -750, 750, new Aabb(new Vector3(-1.0f, 0.0f, -1.7f), new Vector3(1.0f, 1.8f, -1.59f))));
        Assert.False(MeleeWeapon.WedgeHits(sword, feet, -750, 750, new Aabb(new Vector3(-1.0f, 0.0f, -1.7f), new Vector3(1.0f, 1.8f, -1.61f))));

        // The same slab stands outside a wedge on the left.
        Assert.False(MeleeWeapon.WedgeHits(sword, feet, 3000, 4500, slab));
    }

    /// <summary>
    /// A wedge of no turn, a wedge that turns clockwise, and a wedge past a half turn are each an error that names both
    /// yaws, and a half turn is the widest wedge (D-325, T-2; PR #62 automated pass).
    /// </summary>
    [Fact]
    public void AWedgeOutsideItsTurnIsAnError()
    {
        WeaponDefinition sword = Sword;
        Aabb ahead = BodyBoxAt(Feet + new Vector3(0.0f, 0.0f, -1.0f));
        Aabb behind = BodyBoxAt(Feet + new Vector3(0.0f, 0.0f, 1.0f));

        ContextException none = Assert.Throws<ContextException>(() => MeleeWeapon.WedgeHits(sword, Feet, 1500, 1500, ahead));
        Assert.Contains("fromYaw=1500", none.Message, StringComparison.Ordinal);
        Assert.Contains("toYaw=1500", none.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => MeleeWeapon.WedgeHits(sword, Feet, 1500, 0, ahead));
        Assert.Throws<ContextException>(() => MeleeWeapon.WedgeHits(sword, Feet, 0, 18001, ahead));

        Assert.True(MeleeWeapon.WedgeHits(sword, Feet, -9000, 9000, ahead));
        Assert.False(MeleeWeapon.WedgeHits(sword, Feet, -9000, 9000, behind));
    }

    /// <summary>
    /// PR-15 exit test 3. A hit during the windup cancels the swing of a one-handed sword and staggers the player for 20
    /// ticks. The stagger stops the walk, the swing, the roll, and the jump, and a guard of 30 ticks follows it (D-321, D-326).
    /// </summary>
    [Fact]
    public void StaggerInterruptsSwing()
    {
        Driver driver = new(NewPlayer());
        driver.Tick(Button.Attack);
        driver.Ticks(4);
        Assert.Equal(5L, driver.Player.SwingTick);

        driver.Player.TakeHit(10);
        Assert.Equal(Player.NoSwing, driver.Player.SwingTick);
        Assert.Equal(Player.StaggerTicks, driver.Player.StaggerRemaining);
        Assert.Equal(90, driver.Player.Health);

        Vector3 before = driver.Player.Body.Position;
        driver.Tick(Button.Attack | Button.Dodge | Button.Jump, 127, 127);
        Assert.Equal(before, driver.Player.Body.Position);
        Assert.Equal(Player.NoSwing, driver.Player.SwingTick);
        Assert.Equal(0, driver.Player.RollRemaining);
        Assert.Equal(0, driver.Player.DodgeCooldown);

        driver.Ticks(Player.StaggerTicks - 1);
        Assert.Equal(0, driver.Player.StaggerRemaining);
        Assert.Equal(Player.GuardTicks, driver.Player.GuardRemaining);

        // A hit in the recovery cancels the swing too.
        Driver late = new(NewPlayer());
        late.Tick(Button.Attack);
        late.Ticks(25);
        late.Player.TakeHit(10);
        Assert.Equal(Player.NoSwing, late.Player.SwingTick);
    }

    /// <summary>Gravity still acts during a stagger: a player staggered in the air rises slower on each tick, takes no walk, and falls back to the ground (D-326).</summary>
    [Fact]
    public void AStaggerInTheAirStillFalls()
    {
        Driver driver = new(NewPlayer());
        driver.Tick(Button.Jump);
        float rising = driver.Player.Body.VerticalVelocity;
        driver.Player.TakeHit(10);
        driver.Tick(0, 127, 127);

        Assert.True(driver.Player.Body.VerticalVelocity < rising, $"The vertical velocity went from {rising} to {driver.Player.Body.VerticalVelocity} in the stagger.");
        Assert.Equal(Feet.X, driver.Player.Body.Position.X);
        Assert.Equal(Feet.Z, driver.Player.Body.Position.Z);

        driver.Ticks(60);
        Assert.True(driver.Player.Body.IsOnGround());
        Assert.InRange(driver.Player.Body.Position.Y, Feet.Y, Feet.Y + (2.0f * SweptAabb.ContactSkin));
    }

    /// <summary>A hit during a two-handed swing deals damage and staggers nothing, and out of a swing the same wielder staggers (D-29, D-321).</summary>
    [Fact]
    public void ATwoHandedSwingKeepsHyperArmor()
    {
        WeaponDefinition twoHanded = Sword with { Id = "test-two-handed", Handedness = WeaponDefinition.TwoHanded };
        Driver driver = new(NewPlayer(twoHanded));
        driver.Tick(Button.Attack);
        driver.Ticks(4);

        driver.Player.TakeHit(10);
        Assert.Equal(5L, driver.Player.SwingTick);
        Assert.Equal(0, driver.Player.StaggerRemaining);
        Assert.Equal(90, driver.Player.Health);

        driver.Ticks((int)twoHanded.SwingTicks);
        Assert.Equal(Player.NoSwing, driver.Player.SwingTick);
        driver.Player.TakeHit(10);
        Assert.Equal(Player.StaggerTicks, driver.Player.StaggerRemaining);
    }

    /// <summary>
    /// No stunlock: a hit during a stagger deals damage and does not start it again, and a hit during the guard of 30
    /// ticks after it staggers nothing. After the guard, a hit staggers again (D-326).
    /// </summary>
    [Fact]
    public void TheGuardStopsAStunlock()
    {
        Driver driver = new(NewPlayer());
        driver.Player.TakeHit(5);
        Assert.Equal(Player.StaggerTicks, driver.Player.StaggerRemaining);

        driver.Ticks(10);
        driver.Player.TakeHit(5);
        Assert.Equal(Player.StaggerTicks - 10, driver.Player.StaggerRemaining);
        Assert.Equal(90, driver.Player.Health);

        driver.Ticks(10);
        Assert.Equal(0, driver.Player.StaggerRemaining);
        Assert.Equal(Player.GuardTicks, driver.Player.GuardRemaining);

        driver.Ticks(Player.GuardTicks - 1);
        driver.Player.TakeHit(5);
        Assert.Equal(0, driver.Player.StaggerRemaining);
        Assert.Equal(1, driver.Player.GuardRemaining);
        Assert.Equal(85, driver.Player.Health);

        driver.Tick(0);
        Assert.Equal(0, driver.Player.GuardRemaining);
        driver.Player.TakeHit(5);
        Assert.Equal(Player.StaggerTicks, driver.Player.StaggerRemaining);
    }

    /// <summary>No hit lands during a roll: no damage and no stagger. After the roll, a hit lands (D-328).</summary>
    [Fact]
    public void ARollTakesNoHit()
    {
        Driver driver = new(NewPlayer());
        driver.Tick(Button.Dodge);
        driver.Player.TakeHit(50);
        Assert.Equal(Player.MaxHealth, driver.Player.Health);
        Assert.Equal(0, driver.Player.StaggerRemaining);
        Assert.Equal(Player.RollTicks - 1, driver.Player.RollRemaining);

        driver.Ticks(Player.RollTicks - 1);
        Assert.Equal(0, driver.Player.RollRemaining);
        driver.Player.TakeHit(50);
        Assert.Equal(50, driver.Player.Health);
        Assert.Equal(Player.StaggerTicks, driver.Player.StaggerRemaining);
    }

    /// <summary>
    /// PR-15 exit test 4. Health stops at zero, and zero is a death: the loop ends the run with the end kind death, takes
    /// no later intent, and hashes the end kind. A dead player takes no hit and no tick, and a negative damage is an error
    /// (D-322, T-2).
    /// </summary>
    [Fact]
    public void HealthNeverBelowZero()
    {
        Player player = NewPlayer();
        player.TakeHit(34);
        player.TakeHit(34);
        Assert.Equal(32, player.Health);
        Assert.False(player.IsDead);
        player.TakeHit(34);
        Assert.Equal(0, player.Health);
        Assert.True(player.IsDead);
        Assert.Throws<ContextException>(() => player.TakeHit(1));
        Assert.Throws<ContextException>(() => player.Step(new Intent(0U, 0, 0, 0, 0, 0), 0, 0, NoTargets));

        SimulationLoop loop = TestWorld.NewLoop(3UL);
        loop.Step(new Intent(0U, 0, 0, 0, 0, 0));
        StateHash alive = loop.Hash();
        Assert.Equal(RunEnd.None, loop.End);
        loop.Player.TakeHit(Player.MaxHealth + 50);
        Assert.Equal(0, loop.Player.Health);
        Assert.Equal(RunEnd.Death, loop.End);
        Assert.True(loop.Ended);
        Assert.NotEqual(alive, loop.Hash());
        ContextException ended = Assert.Throws<ContextException>(() => loop.Step(new Intent(1U, 0, 0, 0, 0, 0)));
        Assert.Contains("end=death", ended.Message, StringComparison.Ordinal);

        ContextException negative = Assert.Throws<ContextException>(() => NewPlayer().TakeHit(-1));
        Assert.Contains("damage=-1", negative.Message, StringComparison.Ordinal);
        ContextException spawnDead = Assert.Throws<ContextException>(() => new Player(TestWorld.FlatFloor(), TestWorld.Spawn, Sword, 0));
        Assert.Contains("health=0", spawnDead.Message, StringComparison.Ordinal);
    }

    /// <summary>A swing starts on a press: a held bit swings once, a press during a swing does nothing, and no press waits for the end of the swing (D-323).</summary>
    [Fact]
    public void ASwingStartsOnAPress()
    {
        WeaponDefinition sword = Sword;
        Driver driver = new(NewPlayer());
        driver.Ticks(80, Button.Attack);
        Assert.Equal(Player.NoSwing, driver.Player.SwingTick);

        driver.Tick(0);
        driver.Tick(Button.Attack);
        Assert.Equal(1L, driver.Player.SwingTick);

        driver.Ticks(25);
        driver.Tick(0);
        driver.Tick(Button.Attack);
        Assert.Equal(28L, driver.Player.SwingTick);

        driver.Ticks((int)sword.SwingTicks);
        Assert.Equal(Player.NoSwing, driver.Player.SwingTick);
    }

    /// <summary>The walk, the sprint, and the jump stay free during a swing (D-324).</summary>
    [Fact]
    public void TheWalkStaysFreeDuringASwing()
    {
        Driver swinging = new(NewPlayer());
        Driver walking = new(NewPlayer());
        swinging.Tick(Button.Attack | Button.Sprint, 0, 127);
        walking.Tick(Button.Sprint, 0, 127);
        swinging.Ticks(10, Button.Attack | Button.Sprint, 0, 127);
        walking.Ticks(10, Button.Sprint, 0, 127);

        Assert.Equal(walking.Player.Body.Position, swinging.Player.Body.Position);
        Assert.Equal(11L, swinging.Player.SwingTick);

        swinging.Tick(Button.Attack | Button.Jump);
        Assert.True(swinging.Player.Body.VerticalVelocity > 0.0f);
    }

    /// <summary>Health alone carries across a descent. The stagger of the player at the stairwell clears at the spawn of the next floor (D-24, D-335).</summary>
    [Fact]
    public void ADescentKeepsHealthAlone()
    {
        SimulationLoop loop = TestWorld.NewLoop(2UL);
        GreedyDescender policy = new(TestWorld.PeacefulContent);
        loop.Player.TakeHit(40);
        for (int tick = 0; tick < (int)loop.Timer.Length && loop.Floor == SimulationLoop.FirstFloor; tick++)
        {
            Intent intent = policy.Next(loop);
            if ((intent.Buttons & Button.Interact) != 0)
            {
                loop.Player.TakeHit(10);
                Assert.Equal(Player.StaggerTicks, loop.Player.StaggerRemaining);
            }

            loop.Step(intent);
        }

        Assert.Equal(2, loop.Floor);
        Assert.Equal(50, loop.Player.Health);
        Assert.Equal(0, loop.Player.StaggerRemaining);
        Assert.Equal(0, loop.Player.GuardRemaining);
        Assert.Equal(0, loop.Player.DodgeCooldown);
        Assert.Equal(Player.NoSwing, loop.Player.SwingTick);
    }

    /// <summary>
    /// PR-15 exit test 6. Over twenty seeds, a record with swings and rolls replays to the live end hash. The bit-identity
    /// sweep replays a record with swings and rolls too, so the three platforms assert the same (D-69, D-160).
    /// </summary>
    [Fact]
    public void PlayerIsDeterministic()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            MemorySink sink = new();
            RunRecorder recorder = new(sink, RunRecord.NewHeader(TestWorld.PeacefulContent.Hash, (ulong)seed));
            SimulationLoop live = TestWorld.NewLoop((ulong)seed);
            Random random = new(seed);
            int swings = 0;
            int rolls = 0;
            for (uint tick = 0; tick < 600; tick++)
            {
                ushort buttons = 0;
                buttons |= random.Next(20) == 0 ? Button.Attack : (ushort)0;
                buttons |= random.Next(40) == 0 ? Button.Dodge : (ushort)0;
                buttons |= random.Next(30) == 0 ? Button.Jump : (ushort)0;
                buttons |= random.Next(3) == 0 ? Button.Sprint : (ushort)0;
                Intent intent = new(tick, (short)random.Next(-300, 301), (short)random.Next(-50, 51), (sbyte)random.Next(-127, 128), (sbyte)random.Next(-127, 128), buttons);
                recorder.Record(intent);
                live.Step(intent);
                swings += live.Player.SwingTick == 1L ? 1 : 0;
                rolls += live.Player.RollRemaining == Player.RollTicks - 1 ? 1 : 0;
            }

            Assert.True(swings > 0 && rolls > 0, $"Seed {seed}: the record holds {swings} swings and {rolls} rolls, and the test needs both.");
            ReplayResult replay = RunReplayer.Replay(sink.Bytes, TestWorld.PeacefulContent, new JsonlLogger(new CollectingSink()));
            Assert.True(live.Hash().Value == replay.Loop.Hash().Value, $"Seed {seed}: the live hash is {live.Hash()}, and the replay gives {replay.Loop.Hash()}.");
        }
    }

    /// <summary>The hash reads the player: a swing alone, with no move, changes it (D-160).</summary>
    [Fact]
    public void TheHashCoversThePlayer()
    {
        SimulationLoop still = TestWorld.NewLoop(1UL);
        SimulationLoop swinging = TestWorld.NewLoop(1UL);
        SimulationLoop rolling = TestWorld.NewLoop(1UL);
        still.Step(new Intent(0U, 0, 0, 0, 0, 0));
        swinging.Step(new Intent(0U, 0, 0, 0, 0, Button.Attack));
        rolling.Step(new Intent(0U, 0, 0, 0, 0, Button.Dodge));

        Assert.Equal(still.Body.Position, swinging.Body.Position);
        Assert.NotEqual(still.Hash(), swinging.Hash());
        Assert.NotEqual(still.Hash(), rolling.Hash());
        Assert.NotEqual(swinging.Hash(), rolling.Hash());
    }

    /// <summary>
    /// The attack bit swings the weapon of the main weapon id, and a content set without it is an error at the loop
    /// (D-422, T-2). The pick of the Overseer sorts first in path order, and the player never swings it.
    /// </summary>
    [Fact]
    public void ALoopWithoutTheMainWeaponIsAnError()
    {
        Assert.Equal("overseer-pick", TestWorld.Content.Weapons[0].Id);
        Assert.Equal(SimulationLoop.MainWeaponId, SimulationLoop.MainWeapon(TestWorld.Content).Id);
        Assert.Equal(SimulationLoop.MainWeaponId, new SimulationLoop(1UL, TestWorld.Content).Weapon.Id);

        List<WeaponDefinition> others = [];
        foreach (WeaponDefinition weapon in TestWorld.Content.Weapons)
        {
            if (weapon.Id != SimulationLoop.MainWeaponId)
            {
                others.Add(weapon);
            }
        }

        ContentSet none = TestWorld.Content with { Weapons = others };
        ContextException error = Assert.Throws<ContextException>(() => new SimulationLoop(1UL, none));
        Assert.Contains("D-422", error.Message, StringComparison.Ordinal);
        Assert.Contains("weapon=sword-basic", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A death in a bot run ends the run as a crash that names the end kind, and never as the bottom (D-270, D-322).</summary>
    [Fact]
    public void ADeathInABotRunIsACrash()
    {
        BotRunResult result = BotRun.Play(new Killer(), 4UL, TestWorld.Content);
        Assert.Equal(BotRunEnd.Crash, result.End);
        Assert.Contains("end=death", result.Error, StringComparison.Ordinal);
    }

    /// <summary>A fixture policy that deals the player a death on its fifth tick.</summary>
    private sealed class Killer : IBotPolicy
    {
        public string Name => "killer";

        public bool PromisesProgress => true;

        public Intent Next(SimulationLoop loop)
        {
            if (loop.Tick == 5)
            {
                loop.Player.TakeHit(Player.MaxHealth);
            }

            return new Intent(loop.Tick, 0, 0, 0, 0, 0);
        }
    }
}
