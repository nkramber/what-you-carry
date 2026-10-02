using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Tools.AssetQa;

/// <summary>
/// The clip check (D-135, D-301): no two boxes of a model, or of a model plus one overlay, penetrate each other
/// at the rest pose or at any keyframe of any animation of the model. The swing clip of each enemy counts as an
/// animation of the enemy model, also when the clip names another model with the same bones (D-742).
/// </summary>
/// <remarks>
/// <para>
/// A shared face is not a clip, and an overlap under the float noise of <see cref="BoxOverlap.TouchEpsilon"/>
/// is not one either. Two pairs are exempt. A box under a bone and a box under the parent of that bone may
/// overlap, because that overlap is the joint. An overlay box and the body box that it covers overlap by
/// design: the body box of its name (D-300), and each other body box of its bone that it fully encloses at the rest
/// pose, such as the brow, the nose, and the beard under a head piece (D-767).
/// </para>
/// <para>
/// The check poses the body with each overlay alone, and never two overlays together, because two pieces
/// for one slot enclose the same limb and never dress the body at the same time. A pair of two body boxes
/// counts once, on the pass with no overlay, so a body clip gives one finding and not one per overlay.
/// </para>
/// </remarks>
public static class ClipCheck
{
    private const string RestPose = "the rest pose";
    private const string NotABone = "is not a bone of the model, and every track of an animation names one";

    /// <summary>Every clip finding of the set: first each animation of no body model, then the order of the models, the poses, and the pairs, then the swing clip of each enemy.</summary>
    public static IReadOnlyList<AssetFinding> Run(AssetSet set)
    {
        List<AssetFinding> findings = [];
        foreach (LoadedAnimation animation in set.Animations)
        {
            // An animation of no body model is never posed, so the check would pass it with no word (D-135, F-119).
            if (!set.ReadsBody(animation.Clip.Model))
            {
                findings.Add(new AssetFinding(animation.Path, $"names the model '{animation.Clip.Model}', and the set has no body model at that path, so no pose reads the animation (D-135)"));
            }
        }

        foreach (LoadedModel body in set.Bodies)
        {
            List<LoadedAnimation> animations = [];
            foreach (LoadedAnimation animation in set.AnimationsOf(body))
            {
                string? unknown = UnknownBone(body.Model, animation.Clip);
                if (unknown is not null)
                {
                    // A track that names no bone of the model is one finding on the animation, and no pose reads the animation.
                    findings.Add(new AssetFinding(animation.Path, $"'{unknown}' {NotABone} '{body.Path}'"));
                    continue;
                }

                animations.Add(animation);
            }

            List<LoadedModel?> dressings = [null];
            if (body.Path == AssetPaths.BodyModel)
            {
                foreach (LoadedModel overlay in set.Overlays)
                {
                    dressings.Add(overlay);
                }
            }

            foreach (LoadedModel? overlay in dressings)
            {
                CheckPose(body, overlay, RestPose, new Dictionary<string, Vector3>(), findings);
                foreach (LoadedAnimation animation in animations)
                {
                    CheckAnimation(body, overlay, animation, findings);
                }
            }
        }

        foreach (WieldedClip wielded in set.Wielded)
        {
            CheckWielded(set, wielded, findings);
        }

        return findings;
    }

    /// <summary>
    /// The swing clip of one enemy at each keyframe, on the model of that enemy (D-742). A clip that names the same
    /// model had its check in the loop of the models, and a file that did not load has its own load finding.
    /// </summary>
    private static void CheckWielded(AssetSet set, WieldedClip wielded, List<AssetFinding> findings)
    {
        LoadedModel? body = set.Body(wielded.ModelPath);
        if (body is null)
        {
            if (!set.FailedToLoad(wielded.ModelPath))
            {
                findings.Add(new AssetFinding(wielded.Source, $"names the model '{wielded.ModelPath}', and the set has no body model at that path, so no pose reads its swing clip (D-742)"));
            }

            return;
        }

        LoadedAnimation? animation = set.Animation(wielded.AnimationPath);
        if (animation is null)
        {
            if (!set.FailedToLoad(wielded.AnimationPath))
            {
                findings.Add(new AssetFinding(wielded.Source, $"swings the clip '{wielded.AnimationPath}' of its weapon, and the set has no animation at that path (D-742)"));
            }

            return;
        }

        if (animation.Clip.Model == body.Path)
        {
            return;
        }

        string? unknown = UnknownBone(body.Model, animation.Clip);
        if (unknown is not null)
        {
            findings.Add(new AssetFinding(animation.Path, $"'{unknown}' {NotABone} '{body.Path}', which '{wielded.Source}' swings it on (D-742)"));
            return;
        }

        foreach (int tick in animation.Clip.KeyframeTicks())
        {
            string pose = $"tick {tick.ToString(CultureInfo.InvariantCulture)} of '{animation.Path}', the swing of '{wielded.Source}'";
            CheckPose(body, null, pose, animation.Clip.RotationsAt(tick), findings);
        }
    }

    /// <summary>The first track bone that the model does not have, or null when every track names a bone.</summary>
    private static string? UnknownBone(BlockbenchModel model, AnimationClip clip)
    {
        foreach (BoneTrack track in clip.Tracks)
        {
            if (model.BoneIndex(track.Bone) == ModelBone.NoParent)
            {
                return track.Bone;
            }
        }

        return null;
    }

    /// <summary>Every keyframe tick of one animation, on the body and one overlay or none. Every track names a bone of the body.</summary>
    private static void CheckAnimation(LoadedModel body, LoadedModel? overlay, LoadedAnimation animation, List<AssetFinding> findings)
    {
        foreach (int tick in animation.Clip.KeyframeTicks())
        {
            string pose = $"tick {tick.ToString(CultureInfo.InvariantCulture)} of '{animation.Path}'";
            CheckPose(body, overlay, pose, animation.Clip.RotationsAt(tick), findings);
        }
    }

    /// <summary>One pose of the body, with the boxes of one overlay or none, against every pair.</summary>
    private static void CheckPose(LoadedModel body, LoadedModel? overlay, string pose, IReadOnlyDictionary<string, Vector3> rotations, List<AssetFinding> findings)
    {
        IReadOnlyList<BoneTransform> transforms = ModelPose.BoneTransforms(body.Path, body.Model, rotations);
        List<CheckedBox> boxes = [];
        foreach (ModelBox box in body.Model.Boxes)
        {
            boxes.Add(new CheckedBox(body.Path, ModelPose.Place(box, transforms[box.Bone]), IsOverlay: false, []));
        }

        if (overlay is not null)
        {
            foreach (ModelBox box in overlay.Model.Boxes)
            {
                ModelBox? covered = body.Model.Box(box.Name);
                if (covered is null)
                {
                    // The overlay check reports the name. This check has no bone to pose the box with.
                    continue;
                }

                ModelBox onBodyBone = box with { Bone = covered.Bone };
                boxes.Add(new CheckedBox(overlay.Path, ModelPose.Place(onBodyBone, transforms[covered.Bone]), IsOverlay: true, CoveredNames(body.Model, onBodyBone)));
            }
        }

        for (int first = 0; first < boxes.Count; first++)
        {
            for (int second = first + 1; second < boxes.Count; second++)
            {
                if (overlay is not null && !boxes[first].IsOverlay && !boxes[second].IsOverlay)
                {
                    // Two body boxes count once, on the pass with no overlay.
                    continue;
                }

                CheckPair(body.Model, boxes[first], boxes[second], pose, findings);
            }
        }
    }

    /// <summary>One pair of posed boxes. An exempt pair gives no finding.</summary>
    private static void CheckPair(BlockbenchModel body, CheckedBox first, CheckedBox second, string pose, List<AssetFinding> findings)
    {
        if (IsJoint(body, first.Box.Bone, second.Box.Bone) || IsCover(first, second))
        {
            return;
        }

        float depth = BoxOverlap.Depth(first.Box, second.Box);
        if (depth <= BoxOverlap.TouchEpsilon)
        {
            return;
        }

        string firstName = Describe(body, first);
        string secondName = Describe(body, second);
        string depthText = depth.ToString("0.0000", CultureInfo.InvariantCulture);
        findings.Add(new AssetFinding(first.Path, $"at {pose}, {firstName} clips {secondName} by {depthText} meters (D-135, D-301)"));
    }

    /// <summary>A bone and its parent are one joint (D-301).</summary>
    private static bool IsJoint(BlockbenchModel body, int firstBone, int secondBone)
    {
        return body.Bones[firstBone].Parent == secondBone || body.Bones[secondBone].Parent == firstBone;
    }

    /// <summary>An overlay box and a body box that it covers overlap by design (D-300, D-767).</summary>
    private static bool IsCover(CheckedBox first, CheckedBox second)
    {
        if (first.IsOverlay == second.IsOverlay)
        {
            return false;
        }

        CheckedBox overlay = first.IsOverlay ? first : second;
        CheckedBox covered = first.IsOverlay ? second : first;
        foreach (string name in overlay.Covers)
        {
            if (name == covered.Box.Name)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The names of the body boxes that one overlay box covers, at the rest pose: the box of its name, and each other box
    /// of the same bone that it fully encloses on every axis (D-300, D-767). A box of one bone moves with the overlay, so
    /// the rest pose decides the cover for every pose.
    /// </summary>
    private static IReadOnlyList<string> CoveredNames(BlockbenchModel body, ModelBox overlay)
    {
        List<string> names = [overlay.Name];
        foreach (ModelBox box in body.Boxes)
        {
            bool sameBone = box.Bone == overlay.Bone && box.Name != overlay.Name;
            bool encloses = overlay.From.X <= box.From.X && overlay.From.Y <= box.From.Y && overlay.From.Z <= box.From.Z
                && overlay.To.X >= box.To.X && overlay.To.Y >= box.To.Y && overlay.To.Z >= box.To.Z;
            if (sameBone && encloses)
            {
                names.Add(box.Name);
            }
        }

        return names;
    }

    private static string Describe(BlockbenchModel body, CheckedBox box)
    {
        string kind = box.IsOverlay ? "the overlay box" : "the box";
        return $"{kind} '{box.Box.Name}' of bone '{body.Bones[box.Box.Bone].Name}'";
    }

    /// <summary>One posed box in a check: the file it came from, whether it is an overlay box, and the names of the body boxes that an overlay box covers.</summary>
    private readonly record struct CheckedBox(string Path, PosedBox Box, bool IsOverlay, IReadOnlyList<string> Covers);
}
