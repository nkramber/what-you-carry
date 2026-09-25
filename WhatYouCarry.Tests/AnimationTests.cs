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
    /// The weapons of the checkout whose swing clip does not match their phases, which the match theory leaves out until
    /// the owner answers (F-124). The Overseer pick swings 30, 6, and 30 ticks, 66 in all, and it names the 36-tick clip
    /// of the sword, whose phases are 12, 6, and 18 ticks.
    /// </summary>
    private static readonly string[] UnmatchedWeapons = ["overseer-pick"];

    /// <summary>The id of every weapon of the checkout but the ones of <see cref="UnmatchedWeapons"/> (F-124).</summary>
    public static TheoryData<string> MatchedWeapons
    {
        get
        {
            TheoryData<string> ids = [];
            foreach (WeaponDefinition weapon in TestWorld.Content.Weapons)
            {
                if (Array.IndexOf(UnmatchedWeapons, weapon.Id) < 0)
                {
                    ids.Add(weapon.Id);
                }
            }

            return ids;
        }
    }

    /// <summary>
    /// PR-15 exit test 5. The phases of the swing clip of each weapon equal its windup, active, and recovery ticks, and the
    /// roll clip and the stagger clip last the ticks of the roll and of the stagger (D-87, D-315, D-326, D-327, D-331).
    /// The test read the main weapon alone, so a weapon that names the clip of another passed with no check (F-124).
    /// </summary>
    [Theory]
    [MemberData(nameof(MatchedWeapons))]
    public void AnimationMatchesCore(string weaponId)
    {
        WeaponDefinition weapon = WeaponOf(weaponId);
        PlayerClips clips = PlayerClips.Load(ContentRoot(), weapon);
        int windup = (int)weapon.WindupTicks;
        int activeEnd = (int)(weapon.WindupTicks + weapon.ActiveTicks);
        int swingEnd = (int)weapon.SwingTicks;

        Assert.Equal(weapon.SwingTicks, (long)clips.Swing.Length);
        PhaseRange[] swingPhases = [new(0, windup, PhaseTags.Windup), new(windup, activeEnd, PhaseTags.Active), new(activeEnd, swingEnd, PhaseTags.Recovery)];
        Assert.Equal(swingPhases, clips.Swing.Phases);

        Assert.Equal(Player.RollTicks, clips.Dodge.Length);
        Assert.Equal(new[] { new PhaseRange(0, Player.RollTicks, PhaseTags.Idle) }, clips.Dodge.Phases);
        Assert.Equal(Player.StaggerTicks, clips.Stagger.Length);
        Assert.Equal(new[] { new PhaseRange(0, Player.StaggerTicks, PhaseTags.Idle) }, clips.Stagger.Phases);
    }

    /// <summary>
    /// Each weapon that the match theory leaves out is a weapon of the checkout whose clip still does not last its
    /// swing, so the list names no weapon by mistake, and a fixed clip moves its weapon back into the theory (F-124).
    /// </summary>
    [Fact]
    public void EachUnmatchedWeaponStillMissesItsClip()
    {
        foreach (string id in UnmatchedWeapons)
        {
            WeaponDefinition weapon = WeaponOf(id);
            PlayerClips clips = PlayerClips.Load(ContentRoot(), weapon);
            Assert.True(weapon.SwingTicks != clips.Swing.Length, $"The swing clip of '{id}' lasts its {weapon.SwingTicks} ticks now, so the match theory takes it again.");
        }
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
        Assert.Equal(8, sword.Boxes.Count);
        Assert.Single(sword.Bones);
    }

    /// <summary>
    /// D-591. The sword that the right hand tilts forward stays over the floor at rest, over one stride of the walk at
    /// full amount, at each tick of the swing over the walk, and at each tick of the stagger. The Game stands the lowest
    /// corner of the body on the floor, so each sword corner must lie at or over that corner. The roll is out of this
    /// test: it turns the body upside down, and it sank the sword of PR-15 too (OQ-206). Straight down, the sword of
    /// D-587 reaches 2.5 units under the floor at rest.
    /// </summary>
    [Fact]
    public void TheHeldSwordStaysOverTheFloor()
    {
        BlockbenchModel body = LoadModel(AssetPaths.BodyModel);
        BlockbenchModel sword = LoadModel(Sword.Model);
        AttachmentPoint hand = Assert.Single(body.Attachments, point => point.Slot == EquipmentSlots.Weapon);
        RotationMatrix hold = RotationMatrix.FromEulerDegrees(hand.RotationDegrees);
        PlayerClips clips = Clips();
        const int Phases = 32;

        List<(string Name, IReadOnlyDictionary<string, Vector3> Rotations)> poses = [("the rest pose", BodyPose.RestRotations())];
        for (int phase = 0; phase < Phases; phase++)
        {
            float walked = WalkCycle.StrideMeters * phase / Phases;
            poses.Add(($"the walk at {walked} meters", WalkCycle.Rotations(walked, 1.0f)));
            for (int tick = 0; tick <= clips.Swing.Length; tick++)
            {
                Dictionary<string, Vector3> swing = WalkCycle.Rotations(walked, 1.0f);
                foreach (KeyValuePair<string, Vector3> track in clips.Swing.RotationsAt(tick))
                {
                    swing[track.Key] = track.Value;
                }

                poses.Add(($"tick {tick} of the swing at {walked} meters of the walk", swing));
            }
        }

        for (int tick = 0; tick <= clips.Stagger.Length; tick++)
        {
            poses.Add(($"tick {tick} of the stagger", clips.Stagger.RotationsAt(tick)));
        }

        foreach ((string name, IReadOnlyDictionary<string, Vector3> rotations) in poses)
        {
            BoneTransform arm = ModelPose.BoneTransforms(body.Path, body, rotations)[hand.Bone];
            float floor = ModelPose.LowestPoint(body.Path, body, rotations);
            foreach (ModelBox box in sword.Boxes)
            {
                foreach (float x in new[] { box.From.X, box.To.X })
                {
                    foreach (float y in new[] { box.From.Y, box.To.Y })
                    {
                        foreach (float z in new[] { box.From.Z, box.To.Z })
                        {
                            Vector3 corner = arm.Apply(hand.Position + hold.Apply(new Vector3(x, y, z)));
                            Assert.True(corner.Y >= floor, $"A corner of '{box.Name}' is {floor - corner.Y} meters under the floor at {name}.");
                        }
                    }
                }
            }
        }
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
