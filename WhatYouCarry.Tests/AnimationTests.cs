using System;
using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Game.Animation;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The clips of the player body, the sword model, the walk, and the pose of a frame (D-87, D-330, D-331, D-333; PR-15 exit test 5).</summary>
public sealed class AnimationTests
{
    private static readonly EntityBox[] NoTargets = [];

    private static string ContentRoot()
    {
        return Path.Combine(RepositoryRoot.Find(), "content");
    }

    private static WeaponDefinition Sword => SimulationLoop.MainWeapon(TestWorld.Content);

    private static PlayerClips Clips()
    {
        return PlayerClips.Load(ContentRoot(), Sword);
    }

    private static BlockbenchModel LoadModel(string contentPath)
    {
        return BlockbenchLoader.Parse(contentPath, File.ReadAllBytes(Path.Combine(ContentRoot(), contentPath)));
    }

    private static Player NewPlayer()
    {
        return new Player(TestWorld.FlatFloor(16, 6), new Vector3(8.5f, 1.0f, 8.5f), Sword, Player.MaxHealth);
    }

    /// <summary>
    /// PR-15 exit test 5. The phases of the swing clip equal the windup, active, and recovery ticks of the sword, and the
    /// roll clip and the stagger clip last the ticks of the roll and of the stagger (D-87, D-315, D-326, D-327, D-331).
    /// </summary>
    [Fact]
    public void AnimationMatchesCore()
    {
        PlayerClips clips = Clips();
        WeaponDefinition sword = Sword;
        int windup = (int)sword.WindupTicks;
        int activeEnd = (int)(sword.WindupTicks + sword.ActiveTicks);
        int swingEnd = (int)sword.SwingTicks;

        Assert.Equal(sword.SwingTicks, (long)clips.Swing.Length);
        PhaseRange[] swingPhases = [new(0, windup, PhaseTags.Windup), new(windup, activeEnd, PhaseTags.Active), new(activeEnd, swingEnd, PhaseTags.Recovery)];
        Assert.Equal(swingPhases, clips.Swing.Phases);

        Assert.Equal(Player.RollTicks, clips.Dodge.Length);
        Assert.Equal(new[] { new PhaseRange(0, Player.RollTicks, PhaseTags.Idle) }, clips.Dodge.Phases);
        Assert.Equal(Player.StaggerTicks, clips.Stagger.Length);
        Assert.Equal(new[] { new PhaseRange(0, Player.StaggerTicks, PhaseTags.Idle) }, clips.Stagger.Phases);
    }

    /// <summary>Every track of the three clips turns a bone of the body, the body holds the weapon point on the right lower arm, and the sword model loads (D-330, D-331).</summary>
    [Fact]
    public void TheClipsAndTheSwordFitTheBody()
    {
        BlockbenchModel body = LoadModel(AssetPaths.BodyModel);
        PlayerClips clips = Clips();
        foreach (AnimationClip clip in new[] { clips.Swing, clips.Dodge, clips.Stagger })
        {
            Assert.Equal(AssetPaths.BodyModel, clip.Model);
            foreach (BoneTrack track in clip.Tracks)
            {
                Assert.True(body.BoneIndex(track.Bone) != ModelBone.NoParent, $"The clip '{clip.Name}' turns '{track.Bone}', which is not a bone of the body.");
            }
        }

        AttachmentPoint weapon = Assert.Single(body.Attachments, point => point.Slot == EquipmentSlots.Weapon);
        Assert.Equal("arm_right_lower", body.Bones[weapon.Bone].Name);

        BlockbenchModel sword = LoadModel(Sword.Model);
        Assert.Equal(4, sword.Boxes.Count);
        Assert.Single(sword.Bones);
    }

    /// <summary>The walk swings the upper legs 30 degrees and the arms 20 degrees against them over one cycle per 1.2 meters, and its amount follows the speed (D-333).</summary>
    [Fact]
    public void WalkCycleSwingsTheLimbsAgainstEachOther()
    {
        Dictionary<string, Vector3> quarter = WalkCycle.Rotations(WalkCycle.StrideMeters / 4.0f, 1.0f);
        Assert.InRange(quarter[WalkCycle.LegLeft].X, 30.0f - 1e-3f, 30.0f + 1e-3f);
        Assert.InRange(quarter[WalkCycle.LegRight].X, -30.0f - 1e-3f, -30.0f + 1e-3f);
        Assert.InRange(quarter[WalkCycle.ArmLeft].X, -20.0f - 1e-3f, -20.0f + 1e-3f);
        Assert.InRange(quarter[WalkCycle.ArmRight].X, 20.0f - 1e-3f, 20.0f + 1e-3f);

        Dictionary<string, Vector3> threeQuarters = WalkCycle.Rotations(WalkCycle.StrideMeters * 0.75f, 1.0f);
        Assert.InRange(threeQuarters[WalkCycle.LegLeft].X, -30.0f - 1e-3f, -30.0f + 1e-3f);

        foreach (Vector3 rotation in WalkCycle.Rotations(WalkCycle.StrideMeters / 4.0f, 0.0f).Values)
        {
            Assert.Equal(0.0f, MathF.Abs(rotation.X));
        }

        Assert.Equal(0.0f, WalkCycle.Amount(0.0f));
        Assert.Equal(0.5f, WalkCycle.Amount(PlayerBody.WalkSpeed / 2.0f));
        Assert.Equal(1.0f, WalkCycle.Amount(PlayerBody.SprintSpeed));
    }

    /// <summary>
    /// The pose of a frame plays the clip of the player state at the tick that the state gives: the walk at rest, the
    /// swing clip over the walk during a swing, the stagger clip during a stagger, and the roll clip during a roll (D-87, D-331).
    /// </summary>
    [Fact]
    public void BodyPosePlaysTheClipOfTheState()
    {
        PlayerClips clips = Clips();
        Player player = NewPlayer();
        Assert.Equal(WalkCycle.Rotations(0.5f, 0.25f), BodyPose.Rotations(player, clips, 0.5f, 0.25f));

        player.Step(new Intent(0U, 0, 0, 0, 0, Button.Attack), 0, 0, NoTargets);
        IReadOnlyDictionary<string, Vector3> swing = BodyPose.Rotations(player, clips, 0.5f, 0.25f);
        foreach (KeyValuePair<string, Vector3> track in clips.Swing.RotationsAt(1))
        {
            Assert.Equal(track.Value, swing[track.Key]);
        }

        Assert.Equal(WalkCycle.Rotations(0.5f, 0.25f)[WalkCycle.LegLeft], swing[WalkCycle.LegLeft]);

        player.TakeHit(1);
        Assert.Equal(clips.Stagger.RotationsAt(0), BodyPose.Rotations(player, clips, 0.5f, 0.25f));
        player.Step(new Intent(1U, 0, 0, 0, 0, 0), Button.Attack, 0, NoTargets);
        Assert.Equal(clips.Stagger.RotationsAt(1), BodyPose.Rotations(player, clips, 0.5f, 0.25f));

        Player roller = NewPlayer();
        roller.Step(new Intent(0U, 0, 0, 0, 0, Button.Dodge), 0, 0, NoTargets);
        Assert.Equal(clips.Dodge.RotationsAt(1), BodyPose.Rotations(roller, clips, 0.0f, 0.0f));
    }

    /// <summary>The lowest box corner of the body at rest stands on the origin, the middle of the roll puts it far under the origin, and a bone that the model lacks is an error (D-331, T-2).</summary>
    [Fact]
    public void LowestPointFollowsThePose()
    {
        BlockbenchModel body = LoadModel(AssetPaths.BodyModel);
        Assert.InRange(ModelPose.LowestPoint(AssetPaths.BodyModel, body, new Dictionary<string, Vector3>()), -1e-5f, 1e-5f);

        float upsideDown = ModelPose.LowestPoint(AssetPaths.BodyModel, body, Clips().Dodge.RotationsAt(9));
        Assert.True(upsideDown < -0.3f, $"The lowest corner in the middle of the roll is at {upsideDown} meters.");

        ContextException unknown = Assert.Throws<ContextException>(() => ModelPose.LowestPoint(AssetPaths.BodyModel, body, new Dictionary<string, Vector3> { ["tail"] = new(0.0f, 0.0f, 0.0f) }));
        Assert.Contains("tail", unknown.Message, StringComparison.Ordinal);
    }

    /// <summary>A swing clip of another model, and an absent clip file, are each an error that names the file (T-2).</summary>
    [Fact]
    public void ABadClipIsAnError()
    {
        using TemporaryContentDirectory content = new();
        const string otherClip = "models/other.swing.json";
        content.Write(otherClip, ModelJson.Animation("models/other.bbmodel", "arm", 10, "[0, 0, 10]"));

        ContextException other = Assert.Throws<ContextException>(() => PlayerClips.Load(content.Content, Sword with { Animation = otherClip }));
        Assert.Contains(otherClip, other.Message, StringComparison.Ordinal);
        Assert.Contains("models/other.bbmodel", other.Message, StringComparison.Ordinal);

        ContextException absent = Assert.Throws<ContextException>(() => PlayerClips.Load(content.Content, Sword));
        Assert.Contains("player.sword-swing.json", absent.Message, StringComparison.Ordinal);
    }
}
