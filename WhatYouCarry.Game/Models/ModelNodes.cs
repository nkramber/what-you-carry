using Godot;
using WhatYouCarry.Game.Render;

namespace WhatYouCarry.Game.Models;

/// <summary>
/// The scene nodes of one model: a node per bone, a mesh instance per box under its bone, and an empty node per
/// attachment point. The root node sits at the model origin, which is the point between the feet of a body.
/// </summary>
/// <remarks>
/// Every position here is relative to the parent, so a bone node at its pivot moves its boxes and its child
/// bones with it. PR-15 rotates the bone nodes from the keyframes (D-87), and PR-22 adds a child under an
/// attachment node.
/// </remarks>
public static class ModelNodes
{
    /// <summary>The node tree of a model, with one material on every box.</summary>
    public static Node3D Build(BlockbenchModel model, Material material)
    {
        Node3D root = new() { Name = model.Name };
        Node3D[] boneNodes = new Node3D[model.Bones.Count];
        for (int index = 0; index < model.Bones.Count; index++)
        {
            ModelBone bone = model.Bones[index];
            Vector3 parentPivot = bone.Parent == ModelBone.NoParent ? Vector3.Zero : model.Bones[bone.Parent].Pivot;
            Node3D node = new() { Name = bone.Name, Position = bone.Pivot - parentPivot };
            boneNodes[index] = node;
            Node3D parent = bone.Parent == ModelBone.NoParent ? root : boneNodes[bone.Parent];
            parent.AddChild(node);
        }

        foreach (ModelBox box in model.Boxes)
        {
            MeshInstance3D instance = new()
            {
                Name = box.Name,
                Mesh = ArrayMeshBuilder.Build(BoxGeometry.Build(box)),
                MaterialOverride = material,
                Position = box.Pivot - model.Bones[box.Bone].Pivot,
            };
            boneNodes[box.Bone].AddChild(instance);
        }

        foreach (AttachmentPoint point in model.Attachments)
        {
            Node3D node = new() { Name = point.Slot, Position = point.Position - model.Bones[point.Bone].Pivot };
            boneNodes[point.Bone].AddChild(node);
        }

        return root;
    }
}
