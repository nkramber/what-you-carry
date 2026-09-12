using System.Collections.Generic;
using Godot;

namespace WhatYouCarry.Game.Models;

/// <summary>
/// One model from a Blockbench file (D-9, D-18, D-86): the bones, the boxes, and the attachment points, in
/// meters in the frame of D-234. The loader builds one, a test reads it, and <see cref="ModelNodes"/> hands it
/// to the engine.
/// </summary>
/// <param name="Name">The model name from the file.</param>
/// <param name="Bones">Every bone, in file order. A parent comes before its children.</param>
/// <param name="Boxes">Every box, in file order.</param>
/// <param name="Attachments">Every attachment point, one per equipment slot that the model declares.</param>
public sealed record BlockbenchModel(string Name, IReadOnlyList<ModelBone> Bones, IReadOnlyList<ModelBox> Boxes, IReadOnlyList<AttachmentPoint> Attachments);

/// <summary>
/// One bone: a group of the Blockbench outliner (D-87). PR-15 rotates a bone about its pivot, and every box and
/// every child bone under it turns with it.
/// </summary>
/// <param name="Name">The bone name, unique in the model.</param>
/// <param name="Pivot">The pivot, in meters in model space.</param>
/// <param name="Parent">The index of the parent bone, or <see cref="NoParent"/> for a root bone.</param>
public sealed record ModelBone(string Name, Vector3 Pivot, int Parent)
{
    /// <summary>The parent index of a root bone.</summary>
    public const int NoParent = -1;
}

/// <summary>
/// One box: a cube element of the file. Its six faces take the sides of <see cref="BoxSide"/>, in that order.
/// </summary>
/// <param name="Name">The box name, unique in the model.</param>
/// <param name="Bone">The index of the bone that holds the box.</param>
/// <param name="From">The low corner, in meters in model space.</param>
/// <param name="To">The high corner, in meters in model space.</param>
/// <param name="Pivot">The pivot of the box, in meters in model space. The box mesh sits at it.</param>
/// <param name="Faces">The texture rectangle of each side, in the order of <see cref="BoxSide"/>.</param>
public sealed record ModelBox(string Name, int Bone, Vector3 From, Vector3 To, Vector3 Pivot, IReadOnlyList<FaceUv> Faces);

/// <summary>
/// The texture rectangle of one face, as fractions of the texture: the low corner and the high corner. A
/// Blockbench file gives pixels of the model resolution, and the loader divides them out.
/// </summary>
public readonly record struct FaceUv(Vector2 Low, Vector2 High);

/// <summary>The six sides of a box, in the names of Blockbench. The frame is the frame of D-234.</summary>
public enum BoxSide
{
    /// <summary>The face toward minus Z.</summary>
    North = 0,

    /// <summary>The face toward plus X.</summary>
    East = 1,

    /// <summary>The face toward plus Z.</summary>
    South = 2,

    /// <summary>The face toward minus X.</summary>
    West = 3,

    /// <summary>The face toward plus Y.</summary>
    Up = 4,

    /// <summary>The face toward minus Y.</summary>
    Down = 5,
}

/// <summary>
/// One attachment point: a locator of the file, named after an equipment slot of D-18. PR-22 hangs an armor
/// overlay from it.
/// </summary>
/// <param name="Slot">The slot name, one of <see cref="EquipmentSlots.Names"/>.</param>
/// <param name="Bone">The index of the bone that holds the point.</param>
/// <param name="Position">The point, in meters in model space.</param>
public sealed record AttachmentPoint(string Slot, int Bone, Vector3 Position);

/// <summary>The equipment slots that a model can attach to (D-18, D-26). The two ring slots are not modeled.</summary>
public static class EquipmentSlots
{
    /// <summary>The head slot.</summary>
    public const string Head = "head";

    /// <summary>The chest slot.</summary>
    public const string Chest = "chest";

    /// <summary>The legs slot.</summary>
    public const string Legs = "legs";

    /// <summary>The feet slot.</summary>
    public const string Feet = "feet";

    /// <summary>The amulet slot.</summary>
    public const string Amulet = "amulet";

    /// <summary>The shield slot (D-26).</summary>
    public const string Shield = "shield";

    /// <summary>The slot names, which are the locator names of a model file.</summary>
    public static readonly IReadOnlyList<string> Names = [Head, Chest, Legs, Feet, Amulet, Shield];

    /// <summary>Answers whether a name is a slot.</summary>
    public static bool Contains(string name)
    {
        foreach (string slot in Names)
        {
            if (slot == name)
            {
                return true;
            }
        }

        return false;
    }
}
