using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Tools.AssetQa;

/// <summary>
/// The clip check (D-135, D-301): no two boxes of a model, or of a model plus one overlay, penetrate each other
/// at the rest pose or at any keyframe of any animation of the model.
/// </summary>
/// <remarks>
/// <para>
/// A shared face is not a clip, and an overlap under the float noise of <see cref="BoxOverlap.TouchEpsilon"/>
/// is not one either. Two pairs are exempt. A box under a bone and a box under the parent of that bone may
/// overlap, because that overlap is the joint. An overlay box and the body box that it encloses overlap by
/// design (D-300).
/// </para>
/// <para>
/// The check poses the body with each overlay alone, and never two overlays together, because two pieces
/// for one slot enclose the same limb and never dress the body at the same time.
/// </para>
/// </remarks>
public static class ClipCheck
{
    private const string RestPose = "the rest pose";

    /// <summary>Every clip finding of the set, in the order of the models, then the poses, then the pairs.</summary>
    public static IReadOnlyList<AssetFinding> Run(AssetSet set)
    {
        List<AssetFinding> findings = [];
        foreach (LoadedModel body in set.Bodies)
        {
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
                foreach (LoadedAnimation animation in set.AnimationsOf(body))
                {
                    CheckAnimation(body, overlay, animation, findings);
                }
            }
        }

        return findings;
    }

    /// <summary>Every keyframe tick of one animation, on the body and one overlay or none.</summary>
    private static void CheckAnimation(LoadedModel body, LoadedModel? overlay, LoadedAnimation animation, List<AssetFinding> findings)
    {
        foreach (int tick in animation.Clip.KeyframeTicks())
        {
            string pose = $"tick {tick.ToString(CultureInfo.InvariantCulture)} of '{animation.Path}'";
            IReadOnlyDictionary<string, Vector3> rotations = animation.Clip.RotationsAt(tick);
            try
            {
                CheckPose(body, overlay, pose, rotations, findings);
            }
            catch (ContextException error)
            {
                // A track that names no bone of the model is a defect of the animation, and the check reports it once per pose.
                findings.Add(new AssetFinding(animation.Path, error.Message));
                return;
            }
        }
    }

    /// <summary>One pose of the body, with the boxes of one overlay or none, against every pair.</summary>
    private static void CheckPose(LoadedModel body, LoadedModel? overlay, string pose, IReadOnlyDictionary<string, Vector3> rotations, List<AssetFinding> findings)
    {
        IReadOnlyList<BoneTransform> transforms = ModelPose.BoneTransforms(body.Path, body.Model, rotations);
        List<CheckedBox> boxes = [];
        foreach (ModelBox box in body.Model.Boxes)
        {
            boxes.Add(new CheckedBox(body.Path, ModelPose.Place(box, transforms[box.Bone]), IsOverlay: false));
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
                boxes.Add(new CheckedBox(overlay.Path, ModelPose.Place(onBodyBone, transforms[covered.Bone]), IsOverlay: true));
            }
        }

        for (int first = 0; first < boxes.Count; first++)
        {
            for (int second = first + 1; second < boxes.Count; second++)
            {
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

    /// <summary>An overlay box and the body box of the same name overlap by design (D-300).</summary>
    private static bool IsCover(CheckedBox first, CheckedBox second)
    {
        return first.IsOverlay != second.IsOverlay && first.Box.Name == second.Box.Name;
    }

    private static string Describe(BlockbenchModel body, CheckedBox box)
    {
        string kind = box.IsOverlay ? "the overlay box" : "the box";
        return $"{kind} '{box.Box.Name}' of bone '{body.Bones[box.Box.Bone].Name}'";
    }

    /// <summary>One posed box in a check: the file it came from, and whether it is an overlay box.</summary>
    private readonly record struct CheckedBox(string Path, PosedBox Box, bool IsOverlay);
}
