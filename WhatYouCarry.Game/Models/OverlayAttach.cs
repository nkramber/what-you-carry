using System.Collections.Generic;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Render;

namespace WhatYouCarry.Game.Models;

/// <summary>
/// Hangs the boxes of an armor overlay on the body (D-82, D-300). Each overlay box takes the bone of the body box of
/// its name, so it moves with that limb in every pose, as the clip check poses it (D-135).
/// </summary>
/// <remarks>
/// The mesh of an overlay box grows by <see cref="Outset"/> on each side. A face of an overlay can lie in the plane
/// of a body face, such as the side of a chest piece on the side of the torso, and the outset puts the overlay face
/// in front, so the two never fight for the pixel. The outset is a fraction of a texel, and the screen alone reads it.
/// </remarks>
public static class OverlayAttach
{
    /// <summary>The growth of an overlay mesh on each side, in meters: an eighth of a texel at 64 texels per meter.</summary>
    public const float Outset = 0.002f;

    /// <summary>The mark between the overlay name and the box name in the name of a mesh node.</summary>
    private const string NodeNameSeparator = "_";

    private const string NoBodyBox = "The overlay names a box that the body does not have (D-300).";
    private const string BoxField = "box";
    private const string OverlayField = "overlay";
    private const string BodyField = "body";

    /// <summary>The mesh nodes of the overlay, each under the bone of the body box of its name, in the box order of the overlay.</summary>
    /// <exception cref="ContextException">An overlay box names no body box, or the layout has no canvas for a drawn face.</exception>
    public static IReadOnlyList<MeshInstance3D> Attach(ModelNodeTree body, BlockbenchModel bodyModel, BlockbenchModel overlay, Material material, TextureLayout layout)
    {
        List<MeshInstance3D> attached = [];
        foreach (ModelBox box in overlay.Boxes)
        {
            ModelBox? covered = bodyModel.Box(box.Name);
            if (covered is null)
            {
                ContextException error = new(NoBodyBox);
                error.AddContext(BoxField, box.Name);
                error.AddContext(OverlayField, overlay.Path);
                error.AddContext(BodyField, bodyModel.Path);
                throw error;
            }

            ModelBone bone = bodyModel.Bones[covered.Bone];
            MeshInstance3D instance = new()
            {
                Name = overlay.Name + NodeNameSeparator + box.Name,
                MaterialOverride = material,
                Position = RenderInterpolation.ToGodot(box.Pivot - bone.Pivot),
            };
            ArrayMeshBuilder.BuildInto(instance, BoxGeometry.Build(overlay.Path, box, layout, Outset));
            body.Bones[bone.Name].AddChild(instance);
            attached.Add(instance);
        }

        return attached;
    }
}
