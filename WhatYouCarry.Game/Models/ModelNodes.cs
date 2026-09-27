using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Render;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Models;

/// <summary>The scene nodes of one model: the root, the node of each bone by bone name, the node of each attachment point by slot name, and the node of each box.</summary>
/// <param name="Root">The root node at the model origin.</param>
/// <param name="Bones">The node of each bone, by bone name.</param>
/// <param name="Attachments">The node of each attachment point, by slot name.</param>
/// <param name="Boxes">The mesh node of each box, in the box order of the model.</param>
public sealed record ModelNodeTree(Node3D Root, IReadOnlyDictionary<string, Node3D> Bones, IReadOnlyDictionary<string, Node3D> Attachments, IReadOnlyList<MeshInstance3D> Boxes);

/// <summary>
/// The scene nodes of one model: a node per bone, a mesh instance per box under its bone, and an empty node per
/// attachment point. The root node sits at the model origin, which is the point between the feet of a body.
/// The model comes from the Assets project in Core vectors, and this is the place that turns them into engine
/// vectors (D-299).
/// </summary>
/// <remarks>
/// <para>
/// Every position here is relative to the parent, so a bone node at its pivot moves its boxes and its child bones
/// with it. A pose turns each bone node about its pivot with the matrix of the Assets project, which the asset QA
/// poses the boxes with, so the screen and the clip check read one pose (D-87, D-298, D-301).
/// </para>
/// <para>
/// A held model hangs from an attachment node: the sword of the right hand from the weapon point (D-330). The node
/// takes the rotation of its locator, so the held model tilts with it (D-591). PR-22 hangs an armor overlay the same way.
/// </para>
/// </remarks>
public static class ModelNodes
{
    private const string NotABone = "The pose names a bone that the model does not have.";
    private const string OtherTemplate = "The template tree holds another count of boxes than the model, so it is the tree of another model.";
    private const string TemplateBoxesField = "templateBoxes";
    private const string ModelBoxesField = "modelBoxes";
    private const string NotASlot = "The model has no attachment point for the slot.";
    private const string BoneField = "bone";
    private const string SlotField = "slot";
    private const string ModelField = "model";

    /// <summary>The node tree of a model, with one material on every box. Each face reads its canvas from the texture layout (D-505).</summary>
    /// <exception cref="ContextException">The layout has no canvas for a face of the model.</exception>
    public static ModelNodeTree Build(BlockbenchModel model, Material material, TextureLayout layout)
    {
        return BuildTree(model, material, (instance, index) => ArrayMeshBuilder.BuildInto(instance, BoxGeometry.Build(model.Path, model.Boxes[index], layout)));
    }

    /// <summary>
    /// The node tree of a model that shares the mesh of each box of a template tree of the same model, and builds no
    /// mesh. A descent builds the tree of each enemy this way, so it costs nodes alone (F-192).
    /// </summary>
    /// <exception cref="ContextException">The template holds another count of boxes, so it is the tree of another model.</exception>
    public static ModelNodeTree Share(ModelNodeTree template, BlockbenchModel model, Material material)
    {
        if (template.Boxes.Count != model.Boxes.Count)
        {
            ContextException error = new(OtherTemplate);
            error.AddContext(ModelField, model.Path);
            error.AddContext(TemplateBoxesField, template.Boxes.Count.ToString(CultureInfo.InvariantCulture));
            error.AddContext(ModelBoxesField, model.Boxes.Count.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        return BuildTree(model, material, (instance, index) => ArrayMeshBuilder.ShareInto(instance, template.Boxes[index]));
    }

    /// <summary>The nodes of a model. The give-mesh call puts the mesh of each box, by its index in the model, on its node.</summary>
    private static ModelNodeTree BuildTree(BlockbenchModel model, Material material, Action<MeshInstance3D, int> giveMesh)
    {
        Node3D root = new() { Name = model.Name };
        Node3D[] boneNodes = new Node3D[model.Bones.Count];
        Dictionary<string, Node3D> bones = [];
        for (int index = 0; index < model.Bones.Count; index++)
        {
            ModelBone bone = model.Bones[index];
            CoreVector3 parentPivot = bone.Parent == ModelBone.NoParent ? new CoreVector3(0.0f, 0.0f, 0.0f) : model.Bones[bone.Parent].Pivot;
            Node3D node = new() { Name = bone.Name, Position = RenderInterpolation.ToGodot(bone.Pivot - parentPivot) };
            boneNodes[index] = node;
            bones.Add(bone.Name, node);
            Node3D parent = bone.Parent == ModelBone.NoParent ? root : boneNodes[bone.Parent];
            parent.AddChild(node);
        }

        List<MeshInstance3D> boxes = [];
        for (int index = 0; index < model.Boxes.Count; index++)
        {
            ModelBox box = model.Boxes[index];
            MeshInstance3D instance = new()
            {
                Name = box.Name,
                MaterialOverride = material,
                Position = RenderInterpolation.ToGodot(box.Pivot - model.Bones[box.Bone].Pivot),
            };
            giveMesh(instance, index);
            boneNodes[box.Bone].AddChild(instance);
            boxes.Add(instance);
        }

        Dictionary<string, Node3D> attachments = [];
        foreach (AttachmentPoint point in model.Attachments)
        {
            RotationMatrix hold = RotationMatrix.FromEulerDegrees(point.RotationDegrees);
            Node3D node = new()
            {
                Name = point.Slot,
                Position = RenderInterpolation.ToGodot(point.Position - model.Bones[point.Bone].Pivot),
                Basis = new Basis(RenderInterpolation.ToGodot(hold.AxisX()), RenderInterpolation.ToGodot(hold.AxisY()), RenderInterpolation.ToGodot(hold.AxisZ())),
            };
            boneNodes[point.Bone].AddChild(node);
            attachments.Add(point.Slot, node);
        }

        return new ModelNodeTree(root, bones, attachments, boxes);
    }

    /// <summary>
    /// Turns every bone node to the rotation of its bone in degrees, in the euler order of Blockbench, and every other bone
    /// node to rest (D-298).
    /// </summary>
    /// <exception cref="ContextException">A rotation names a bone that the model does not have (T-2).</exception>
    public static void Pose(ModelNodeTree tree, IReadOnlyDictionary<string, CoreVector3> rotationDegreesByBone)
    {
        foreach (string bone in rotationDegreesByBone.Keys)
        {
            if (!tree.Bones.ContainsKey(bone))
            {
                ContextException error = new(NotABone);
                error.AddContext(BoneField, bone);
                error.AddContext(ModelField, tree.Root.Name);
                throw error;
            }
        }

        foreach (KeyValuePair<string, Node3D> bone in tree.Bones)
        {
            RotationMatrix rotation = rotationDegreesByBone.TryGetValue(bone.Key, out CoreVector3 degrees)
                ? RotationMatrix.FromEulerDegrees(degrees)
                : RotationMatrix.Identity;
            bone.Value.Basis = new Basis(
                RenderInterpolation.ToGodot(rotation.AxisX()),
                RenderInterpolation.ToGodot(rotation.AxisY()),
                RenderInterpolation.ToGodot(rotation.AxisZ()));
        }
    }

    /// <summary>Hangs a node from the attachment point of a slot, at the point (D-330).</summary>
    /// <exception cref="ContextException">The model has no attachment point for the slot (T-2).</exception>
    public static void Hold(ModelNodeTree tree, string slot, Node3D held)
    {
        if (!tree.Attachments.TryGetValue(slot, out Node3D? point))
        {
            ContextException error = new(NotASlot);
            error.AddContext(SlotField, slot);
            error.AddContext(ModelField, tree.Root.Name);
            throw error;
        }

        point.AddChild(held);
    }
}
