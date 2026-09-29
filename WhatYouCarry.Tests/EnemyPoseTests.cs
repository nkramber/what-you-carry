using System;
using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Animation;
using WhatYouCarry.Game.Render;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The swing clip of each enemy and of the Overseer, and the pose that it gives on each tick (D-739, D-741; PR-100 exit test 1).</summary>
public sealed class EnemyPoseTests
{
    private static readonly EntityBox[] NoTargets = [];

    private static string ContentRoot()
    {
        return Path.Combine(RepositoryRoot.Find(), "content");
    }

    private static IReadOnlyDictionary<string, EnemyModel> FamilyModels()
    {
        return EnemyModels.Load(ContentRoot(), TestWorld.Content.Enemies);
    }

    private static EnemyModel HunterModel()
    {
        return EnemyModels.Read(ContentRoot(), TestWorld.Content.Hunter.Model);
    }

    private static EnemyClips Clips()
    {
        return EnemyClips.Load(ContentRoot(), TestWorld.Content, FamilyModels(), HunterModel());
    }

    /// <summary>The weapon of one id in the repository content. An absent id fails the test.</summary>
    private static WeaponDefinition WeaponOf(string id)
    {
        foreach (WeaponDefinition weapon in TestWorld.Content.Weapons)
        {
            if (weapon.Id == id)
            {
                return weapon;
            }
        }

        throw new InvalidOperationException($"The content set holds no weapon '{id}'.");
    }

    /// <summary>
    /// PR-100 exit test 1. The scavenger plays the swing clip of the player on each tick of its swing, and stands in the
    /// rest pose before and after it (D-401, D-741). On <c>main</c> each enemy stood in the rest pose on every tick.
    /// </summary>
    [Fact]
    public void ScavengerPlaysItsClipOnEachTickOfTheSwing()
    {
        SimulationLoop loop = new(1, TestWorld.Content);
        Enemy scavenger = loop.Enemies[0];
        AnimationClip clip = Clips().Of(scavenger.Definition);
        Assert.Equal("models/player.sword-swing.json", scavenger.Weapon.Animation);
        Assert.Empty(EnemyPose.Rotations(scavenger.SwingTick, clip));

        scavenger.StartSwing();
        for (long tick = 0; tick < scavenger.Weapon.SwingTicks; tick++)
        {
            Assert.Equal(tick, scavenger.SwingTick);
            Assert.Equal(clip.RotationsAt((int)tick), EnemyPose.Rotations(scavenger.SwingTick, clip));
            scavenger.Step(new Vector3(0.0f, 0.0f, 0.0f), false, 0, NoTargets);
        }

        Assert.Empty(EnemyPose.Rotations(scavenger.SwingTick, clip));

        // The end of the windup turns the sword arm away from rest, so a clip of the rest pose fails this test.
        Assert.NotEqual(new Vector3(0.0f, 0.0f, 0.0f), clip.RotationsAt((int)scavenger.Weapon.WindupTicks)["arm_right"]);
    }

    /// <summary>
    /// PR-100 exit test 1. The Overseer plays the clip of its pick on each tick of its swing, from the first tick to the
    /// last of the 66, and stands in the rest pose before it (D-741).
    /// </summary>
    [Fact]
    public void OverseerPlaysItsClipOnEachTickOfTheSwing()
    {
        VoxelGrid grid = TestWorld.FlatFloor();
        WeaponDefinition pick = WeaponOf(TestWorld.Content.Hunter.Weapon);
        Hunter overseer = new(grid, new Cell(2, 0, 2), TestWorld.Content.Hunter, pick, 1);
        Vector3 player = new(3.5f, TestWorld.FloorTop, 2.5f);
        GridPathfinder pathfinder = new(grid);
        AnimationClip clip = Clips().Hunter;
        Assert.Equal("models/overseer.pick-swing.json", pick.Animation);
        Assert.Equal(66, pick.SwingTicks);
        Assert.Empty(EnemyPose.Rotations(overseer.SwingTick, clip));

        List<long> ticks = [];
        for (int step = 0; step < 200 && (ticks.Count == 0 || overseer.IsSwinging); step++)
        {
            overseer.Step(grid, pathfinder, player, 0, NoTargets);
            if (overseer.IsSwinging)
            {
                ticks.Add(overseer.SwingTick);
                Assert.Equal(clip.RotationsAt((int)overseer.SwingTick), EnemyPose.Rotations(overseer.SwingTick, clip));
            }
        }

        Assert.NotEmpty(ticks);
        Assert.InRange(ticks[0], 0L, 1L);
        Assert.Equal(pick.SwingTicks - 1, ticks[^1]);
        for (int index = 1; index < ticks.Count; index++)
        {
            Assert.Equal(ticks[index - 1] + 1, ticks[index]);
        }

        Assert.NotEqual(new Vector3(0.0f, 0.0f, 0.0f), clip.RotationsAt((int)pick.WindupTicks)["arm_right"]);
    }

    /// <summary>
    /// The tree of each enemy stands its root at the lowest corner of the rest pose (D-741). So each tick of each swing
    /// keeps that lowest corner, and the sword that the right hand holds stays over it, as D-591 asks of the player.
    /// </summary>
    [Fact]
    public void EachSwingKeepsTheFeetAndTheSwordOverTheFloor()
    {
        EnemyClips clips = Clips();
        BlockbenchModel sword = BlockbenchLoader.Parse(SimulationLoop.MainWeapon(TestWorld.Content).Model, File.ReadAllBytes(Path.Combine(ContentRoot(), SimulationLoop.MainWeapon(TestWorld.Content).Model)));
        List<(EnemyModel Model, AnimationClip Clip)> swings = [(HunterModel(), clips.Hunter)];
        foreach (EnemyDefinition family in TestWorld.Content.Enemies)
        {
            swings.Add((EnemyModels.Of(FamilyModels(), family), clips.Of(family)));
        }

        foreach ((EnemyModel model, AnimationClip clip) in swings)
        {
            AttachmentPoint hand = Assert.Single(model.Model.Attachments, point => point.Slot == EquipmentSlots.Weapon);
            RotationMatrix hold = RotationMatrix.FromEulerDegrees(hand.RotationDegrees);
            for (int tick = 0; tick <= clip.Length; tick++)
            {
                IReadOnlyDictionary<string, Vector3> rotations = clip.RotationsAt(tick);
                float lowest = ModelPose.LowestPoint(model.Path, model.Model, rotations);
                Assert.True(MathF.Abs(lowest - model.Lowest) <= 1e-4f, $"Tick {tick} of '{clip.Name}' on '{model.Path}' moves the lowest corner from {model.Lowest} to {lowest}.");

                BoneTransform arm = ModelPose.BoneTransforms(model.Path, model.Model, rotations)[hand.Bone];
                foreach (ModelBox box in sword.Boxes)
                {
                    foreach (float x in new[] { box.From.X, box.To.X })
                    {
                        foreach (float y in new[] { box.From.Y, box.To.Y })
                        {
                            foreach (float z in new[] { box.From.Z, box.To.Z })
                            {
                                Vector3 corner = arm.Apply(hand.Position + hold.Apply(new Vector3(x, y, z)));
                                Assert.True(corner.Y >= model.Lowest, $"A corner of '{box.Name}' is {model.Lowest - corner.Y} under the floor at tick {tick} of '{clip.Name}' on '{model.Path}'.");
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>A clip plays tick for tick, so a clip that does not last the swing of its weapon is an error that names both counts (D-87, T-2).</summary>
    [Fact]
    public void AClipThatDoesNotLastItsSwingIsAnError()
    {
        WeaponDefinition pick = WeaponOf(TestWorld.Content.Hunter.Weapon);
        WeaponDefinition longer = pick with { RecoveryTicks = pick.RecoveryTicks + 1 };

        ContextException error = Assert.Throws<ContextException>(() => EnemyClips.Swing(ContentRoot(), longer, HunterModel()));

        Assert.Contains("D-87", error.Message, StringComparison.Ordinal);
        Assert.Contains("models/overseer.pick-swing.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("67", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A clip that turns a bone that the enemy model does not have is an error that names the bone (D-741, T-2).</summary>
    [Fact]
    public void AClipBoneThatTheModelLacksIsAnError()
    {
        WeaponDefinition sword = SimulationLoop.MainWeapon(TestWorld.Content);
        BlockbenchModel swordModel = BlockbenchLoader.Parse(sword.Model, File.ReadAllBytes(Path.Combine(ContentRoot(), sword.Model)));
        EnemyModel boneless = new(sword.Model, swordModel, 0.0f);

        ContextException error = Assert.Throws<ContextException>(() => EnemyClips.Swing(ContentRoot(), sword, boneless));

        Assert.Contains("turns a bone", error.Message, StringComparison.Ordinal);
        Assert.Contains(sword.Model, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A family that names a weapon that the set does not hold is an error that names the weapon and the family (T-2).</summary>
    [Fact]
    public void AnAbsentWeaponIsAnError()
    {
        EnemyDefinition family = TestWorld.Content.Enemies[0];
        ContentSet content = TestWorld.Content with { Enemies = [family with { Weapon = "no-such-weapon" }] };

        ContextException error = Assert.Throws<ContextException>(() => EnemyClips.Load(ContentRoot(), content, FamilyModels(), HunterModel()));

        Assert.Contains("no-such-weapon", error.Message, StringComparison.Ordinal);
        Assert.Contains(family.Id, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A family with no loaded clip is an error that names the family, and never the rest pose (T-2).</summary>
    [Fact]
    public void AFamilyWithNoClipIsAnError()
    {
        EnemyDefinition ghost = TestWorld.Content.Enemies[0] with { Id = "ghost" };

        ContextException error = Assert.Throws<ContextException>(() => Clips().Of(ghost));

        Assert.Contains("ghost", error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The trees of the enemies take the pose of the swing tick of their enemy after each tick, and the Overseer too
    /// (D-739). The node side needs the engine, so this test reads the source of the two calls.
    /// </summary>
    [Fact]
    public void EveryTreeTakesThePoseOfItsSwingAfterTheTick()
    {
        string nodes = RepositoryRoot.ReadFile("WhatYouCarry.Game/Render/EnemyNodes.cs");
        int start = nodes.IndexOf("public void AfterTick(", StringComparison.Ordinal);
        Assert.True(start >= 0, "EnemyNodes has no AfterTick.");
        int end = nodes.IndexOf("\n    }\n", start, StringComparison.Ordinal);
        string body = nodes[start..end];

        Assert.Contains("ModelNodes.Pose(this.trees[index], EnemyPose.Rotations(enemy.SwingTick, this.clips.Of(enemy.Definition)));", body, StringComparison.Ordinal);
        Assert.Contains("ModelNodes.Pose(this.hunterTree, EnemyPose.Rotations(hunter.SwingTick, this.clips.Hunter));", body, StringComparison.Ordinal);

        string main = RepositoryRoot.ReadFile("WhatYouCarry.Game/Main.cs");
        Assert.Contains("EnemyClips enemyClips = EnemyClips.Load(contentDirectory, content, familyModels, hunterModel);", main, StringComparison.Ordinal);
        Assert.Contains("new(this, familyModels, hunterModel, enemyClips, swordModel,", main, StringComparison.Ordinal);
    }
}
