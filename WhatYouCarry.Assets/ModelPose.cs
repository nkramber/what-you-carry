using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Assets;

/// <summary>
/// The pose of a model at one set of bone rotations (D-87, D-298): a transform per bone, and a posed box per
/// box. The asset QA tests the posed boxes for a clip (D-135, D-301).
/// </summary>
/// <remarks>
/// A bone turns about its pivot, and every child bone turns with it, so the transform of a bone is the
/// transform of its parent after its own turn about its own pivot. The bones of a model come in parent-first
/// order, so one pass builds every transform. A box is axis-aligned in its rest pose, so a posed box is the
/// rest box under the transform of its bone: the center moves, the half extents stay, and the axes turn.
/// </remarks>
public static class ModelPose
{
    private const string NotABone = "is not a bone of the model, and every track of an animation names one";

    /// <summary>The transform of every bone, by bone index, at the rotations. A bone with no rotation stays at rest.</summary>
    /// <exception cref="ContextException">A rotation names a bone that the model does not have.</exception>
    public static IReadOnlyList<BoneTransform> BoneTransforms(string path, BlockbenchModel model, IReadOnlyDictionary<string, Vector3> rotationDegreesByBone)
    {
        foreach (string bone in rotationDegreesByBone.Keys)
        {
            if (model.BoneIndex(bone) == ModelBone.NoParent)
            {
                throw ContentError.Make(path, bone, NotABone);
            }
        }

        BoneTransform[] transforms = new BoneTransform[model.Bones.Count];
        for (int index = 0; index < model.Bones.Count; index++)
        {
            ModelBone bone = model.Bones[index];
            RotationMatrix rotation = rotationDegreesByBone.TryGetValue(bone.Name, out Vector3 degrees)
                ? RotationMatrix.FromEulerDegrees(degrees)
                : RotationMatrix.Identity;
            BoneTransform local = BoneTransform.AboutPivot(rotation, bone.Pivot);
            transforms[index] = bone.Parent == ModelBone.NoParent ? local : transforms[bone.Parent].Then(local);
        }

        return transforms;
    }

    /// <summary>One box under the transform of its bone.</summary>
    public static PosedBox Place(ModelBox box, BoneTransform transform)
    {
        Vector3 restCenter = (box.From + box.To) * 0.5f;
        Vector3 halfExtents = (box.To - box.From) * 0.5f;
        return new PosedBox(box.Name, box.Bone, transform.Apply(restCenter), halfExtents, transform.Rotation);
    }

    /// <summary>
    /// The height of the lowest box corner of a model at the rotations, in meters over the model origin. The Game layer
    /// stands that corner on the feet, so a roll or a stride never sinks a box into the floor (D-331, D-333).
    /// </summary>
    /// <exception cref="ContextException">A rotation names a bone that the model does not have, or the model holds no box.</exception>
    public static float LowestPoint(string path, BlockbenchModel model, IReadOnlyDictionary<string, Vector3> rotationDegreesByBone)
    {
        if (model.Boxes.Count == 0)
        {
            throw ContentError.MakeForFile(path, "the model holds no box, and a pose has no lowest point");
        }

        IReadOnlyList<BoneTransform> transforms = BoneTransforms(path, model, rotationDegreesByBone);
        float lowest = float.MaxValue;
        foreach (ModelBox box in model.Boxes)
        {
            PosedBox posed = Place(box, transforms[box.Bone]);
            RotationMatrix axes = posed.Axes;

            // The second row of the matrix holds the height of each box axis, so the box reaches this far down from its center.
            float down = (System.MathF.Abs(axes.M10) * posed.HalfExtents.X) + (System.MathF.Abs(axes.M11) * posed.HalfExtents.Y) + (System.MathF.Abs(axes.M12) * posed.HalfExtents.Z);
            float bottom = posed.Center.Y - down;
            if (bottom < lowest)
            {
                lowest = bottom;
            }
        }

        return lowest;
    }
}

/// <summary>A rotation and then a translation: the place of a bone in model space at a pose.</summary>
/// <param name="Rotation">The rotation of the bone frame.</param>
/// <param name="Translation">The offset after the rotation.</param>
public readonly record struct BoneTransform(RotationMatrix Rotation, Vector3 Translation)
{
    /// <summary>The transform that changes nothing.</summary>
    public static readonly BoneTransform Rest = new(RotationMatrix.Identity, new Vector3(0.0f, 0.0f, 0.0f));

    /// <summary>The turn of a bone about its pivot: a point moves to the pivot plus the rotated offset from the pivot.</summary>
    public static BoneTransform AboutPivot(RotationMatrix rotation, Vector3 pivot)
    {
        return new BoneTransform(rotation, pivot - rotation.Apply(pivot));
    }

    /// <summary>The point after the transform.</summary>
    public Vector3 Apply(Vector3 point)
    {
        return this.Rotation.Apply(point) + this.Translation;
    }

    /// <summary>This transform after a child transform: the child turns first, then this one.</summary>
    public BoneTransform Then(BoneTransform child)
    {
        return new BoneTransform(RotationMatrix.Multiply(this.Rotation, child.Rotation), this.Apply(child.Translation));
    }
}

/// <summary>One box at a pose: a rotated box in model space, in meters.</summary>
/// <param name="Name">The box name.</param>
/// <param name="Bone">The index of the bone that holds the box, in the model that the box came from.</param>
/// <param name="Center">The center of the box.</param>
/// <param name="HalfExtents">Half the size of the box along each of its own axes.</param>
/// <param name="Axes">The rotation of the box: its three axes are the columns.</param>
public sealed record PosedBox(string Name, int Bone, Vector3 Center, Vector3 HalfExtents, RotationMatrix Axes);
