using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Godot;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Models;

/// <summary>
/// Reads a Blockbench project file into a <see cref="BlockbenchModel"/> (D-9, D-18, D-86, OQ-159). The file is
/// the JSON that Blockbench 5 writes for its generic format: a flat list of elements, a flat list of groups,
/// and an outliner tree of ids that gives the hierarchy.
/// </summary>
/// <remarks>
/// <para>
/// A group is a bone, a cube element is a box under its bone, and a locator element is an attachment point
/// named after an equipment slot. Sixteen units of the file are one meter (OQ-159), and the frame of the file is
/// the frame of D-234, so no axis changes.
/// </para>
/// <para>
/// PR-13 reads the rest pose alone. A box or a bone with a rotation is an error that names it, because the
/// keyframe rotation arrives with PR-15 (D-87), and a silent drop of a rotation would show a wrong model (T-2).
/// Every failure names the file and the box, the bone, or the point at fault (D-92).
/// </para>
/// </remarks>
public static class BlockbenchLoader
{
    /// <summary>The file extension that Blockbench writes for a project (OQ-159).</summary>
    public const string Extension = ".bbmodel";

    /// <summary>The count of file units in one meter (OQ-159).</summary>
    public const int UnitsPerMeter = 16;

    /// <summary>The lowest major format version that the loader reads. Blockbench 5 writes the flat groups list.</summary>
    public const int MinimumFormatMajor = 5;

    private const string RootName = "the file";
    private const string MetaKey = "meta";
    private const string FormatVersionKey = "format_version";
    private const string NameKey = "name";
    private const string ResolutionKey = "resolution";
    private const string WidthKey = "width";
    private const string HeightKey = "height";
    private const string ElementsKey = "elements";
    private const string GroupsKey = "groups";
    private const string OutlinerKey = "outliner";
    private const string UuidKey = "uuid";
    private const string TypeKey = "type";
    private const string CubeType = "cube";
    private const string LocatorType = "locator";
    private const string FromKey = "from";
    private const string ToKey = "to";
    private const string OriginKey = "origin";
    private const string RotationKey = "rotation";
    private const string FacesKey = "faces";
    private const string UvKey = "uv";
    private const string PositionKey = "position";
    private const string ChildrenKey = "children";

    private const string NotOneObject = "the file must hold one JSON object";
    private const string NotAList = "is not a list";
    private const string ResolutionNotPositive = "must be above zero";
    private const string RotationNotZero = "is not zero, and PR-13 reads the rest pose alone (D-87)";
    private const string FromAboveTo = "has a component above the same component of 'to'";
    private const string NotASlot = "is a locator whose name is not an equipment slot of D-18";
    private const string SlotTwice = "is the name of two locators, and a slot has one attachment point";
    private const string NameTwice = "is the name of two boxes or two bones, and every name is unique";
    private const string NotAnOutlinerEntry = "holds an entry that is neither an id nor a group";
    private const string NotInTheOutliner = "is not in the outliner, and every box hangs from a bone";
    private const string PlacedTwice = "appears twice in the outliner";
    private const string UnknownId = "names an id that the file does not declare";
    private const string ElementAtRoot = "is an element at the root of the outliner, and every element hangs from a bone";

    private const string NorthFace = "north";
    private const string EastFace = "east";
    private const string SouthFace = "south";
    private const string WestFace = "west";
    private const string UpFace = "up";
    private const string DownFace = "down";

    /// <summary>The face names of the file, in the order of <see cref="BoxSide"/>.</summary>
    private static readonly string[] FaceNames = [NorthFace, EastFace, SouthFace, WestFace, UpFace, DownFace];

    /// <summary>The model of one file.</summary>
    /// <exception cref="ContextException">The file is not valid JSON, its format is too old, or a field is absent, of another kind, or outside its bounds. The error names the box, the bone, or the point.</exception>
    public static BlockbenchModel Parse(string path, byte[] bytes)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(bytes);
        }
        catch (JsonException error)
        {
            throw ContentError.MakeForFile(path, $"the file is not valid JSON. {error.Message}");
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw ContentError.MakeForFile(path, NotOneObject);
            }

            CheckFormatVersion(path, root);
            string name = JsonShape.Text(path, root, RootName, NameKey);
            Vector2 resolution = ReadResolution(path, root);
            Dictionary<string, JsonElement> groups = ReadById(path, root, GroupsKey);
            Dictionary<string, JsonElement> elements = ReadById(path, root, ElementsKey);

            List<OutlinerEntry> entries = [];
            WalkOutliner(path, JsonShape.Member(path, root, RootName, OutlinerKey), ModelBone.NoParent, groups, elements, entries);
            CheckEveryIdPlacedOnce(path, groups, elements, entries);

            List<ModelBone> bones = [];
            List<ModelBox> boxes = [];
            List<AttachmentPoint> attachments = [];
            foreach (OutlinerEntry entry in entries)
            {
                if (entry.IsGroup)
                {
                    bones.Add(ReadBone(path, groups[entry.Id], entry.Parent));
                    continue;
                }

                JsonElement element = elements[entry.Id];
                string elementName = JsonShape.Text(path, element, entry.Id, NameKey);
                string type = JsonShape.Text(path, element, elementName, TypeKey);
                if (type == CubeType)
                {
                    boxes.Add(ReadBox(path, element, elementName, entry.Parent, resolution));
                }
                else if (type == LocatorType)
                {
                    attachments.Add(ReadAttachment(path, element, elementName, entry.Parent, attachments));
                }
                else
                {
                    throw ContentError.Make(path, TypeKey, $"on '{elementName}' is '{type}', and the loader reads a cube or a locator (D-83)");
                }
            }

            CheckUniqueNames(path, bones, boxes);
            return new BlockbenchModel(name, bones, boxes, attachments);
        }
    }

    /// <summary>One meter for each <see cref="UnitsPerMeter"/> file units.</summary>
    public static Vector3 ToMeters(Vector3 units)
    {
        return units / UnitsPerMeter;
    }

    /// <summary>The major number of the format version must reach <see cref="MinimumFormatMajor"/>.</summary>
    private static void CheckFormatVersion(string path, JsonElement root)
    {
        JsonElement meta = JsonShape.Member(path, root, RootName, MetaKey);
        string version = JsonShape.Text(path, meta, MetaKey, FormatVersionKey);
        int dot = version.IndexOf('.', System.StringComparison.Ordinal);
        string majorText = dot < 0 ? version : version.Substring(0, dot);
        if (!int.TryParse(majorText, NumberStyles.None, CultureInfo.InvariantCulture, out int major) || major < MinimumFormatMajor)
        {
            throw ContentError.Make(path, FormatVersionKey, $"is '{version}', and the loader reads version {MinimumFormatMajor.ToString(CultureInfo.InvariantCulture)} or later, which Blockbench 5 writes");
        }
    }

    /// <summary>The texture resolution of the model, in pixels. The face rectangles divide by it.</summary>
    private static Vector2 ReadResolution(string path, JsonElement root)
    {
        JsonElement resolution = JsonShape.Member(path, root, RootName, ResolutionKey);
        float width = JsonShape.Number(path, resolution, ResolutionKey, WidthKey);
        float height = JsonShape.Number(path, resolution, ResolutionKey, HeightKey);
        if (width <= 0.0f)
        {
            throw ContentError.Make(path, WidthKey, ResolutionNotPositive);
        }

        if (height <= 0.0f)
        {
            throw ContentError.Make(path, HeightKey, ResolutionNotPositive);
        }

        return new Vector2(width, height);
    }

    /// <summary>Every object of one list, by its id. A repeated id is an error.</summary>
    private static Dictionary<string, JsonElement> ReadById(string path, JsonElement root, string listName)
    {
        JsonElement list = JsonShape.Member(path, root, RootName, listName);
        if (list.ValueKind != JsonValueKind.Array)
        {
            throw ContentError.Make(path, listName, NotAList);
        }

        Dictionary<string, JsonElement> byId = [];
        int index = 0;
        foreach (JsonElement item in list.EnumerateArray())
        {
            string id = JsonShape.Text(path, item, $"{listName}[{index.ToString(CultureInfo.InvariantCulture)}]", UuidKey);
            if (byId.ContainsKey(id))
            {
                throw ContentError.Make(path, UuidKey, $"'{id}' appears twice in '{listName}'");
            }

            byId.Add(id, item);
            index++;
        }

        return byId;
    }

    /// <summary>
    /// Walks one level of the outliner and its children. A text entry is an element under the current bone,
    /// and an object entry is a group, which becomes the next bone. The parent of a root group is
    /// <see cref="ModelBone.NoParent"/>.
    /// </summary>
    private static void WalkOutliner(string path, JsonElement list, int parent, Dictionary<string, JsonElement> groups, Dictionary<string, JsonElement> elements, List<OutlinerEntry> entries)
    {
        if (list.ValueKind != JsonValueKind.Array)
        {
            throw ContentError.Make(path, OutlinerKey, NotAList);
        }

        foreach (JsonElement item in list.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                string id = item.GetString() ?? string.Empty;
                if (!elements.ContainsKey(id))
                {
                    throw ContentError.Make(path, OutlinerKey, $"{UnknownId}: '{id}'");
                }

                if (parent == ModelBone.NoParent)
                {
                    throw ContentError.Make(path, JsonShape.Text(path, elements[id], id, NameKey), ElementAtRoot);
                }

                entries.Add(new OutlinerEntry(id, IsGroup: false, parent));
                continue;
            }

            if (item.ValueKind != JsonValueKind.Object)
            {
                throw ContentError.Make(path, OutlinerKey, NotAnOutlinerEntry);
            }

            string groupId = JsonShape.Text(path, item, OutlinerKey, UuidKey);
            if (!groups.ContainsKey(groupId))
            {
                throw ContentError.Make(path, OutlinerKey, $"{UnknownId}: '{groupId}'");
            }

            int boneIndex = 0;
            foreach (OutlinerEntry entry in entries)
            {
                if (entry.IsGroup)
                {
                    boneIndex++;
                }
            }

            entries.Add(new OutlinerEntry(groupId, IsGroup: true, parent));
            WalkOutliner(path, JsonShape.Member(path, item, OutlinerKey, ChildrenKey), boneIndex, groups, elements, entries);
        }
    }

    /// <summary>Every declared group and element stands in the outliner exactly once.</summary>
    private static void CheckEveryIdPlacedOnce(string path, Dictionary<string, JsonElement> groups, Dictionary<string, JsonElement> elements, List<OutlinerEntry> entries)
    {
        HashSet<string> placed = [];
        foreach (OutlinerEntry entry in entries)
        {
            if (!placed.Add(entry.Id))
            {
                throw ContentError.Make(path, UuidKey, $"'{entry.Id}' {PlacedTwice}");
            }
        }

        foreach (KeyValuePair<string, JsonElement> group in groups)
        {
            if (!placed.Contains(group.Key))
            {
                throw ContentError.Make(path, JsonShape.Text(path, group.Value, group.Key, NameKey), NotInTheOutliner);
            }
        }

        foreach (KeyValuePair<string, JsonElement> element in elements)
        {
            if (!placed.Contains(element.Key))
            {
                throw ContentError.Make(path, JsonShape.Text(path, element.Value, element.Key, NameKey), NotInTheOutliner);
            }
        }
    }

    /// <summary>One bone from one group object. A rotation must be absent or zero.</summary>
    private static ModelBone ReadBone(string path, JsonElement group, int parent)
    {
        string name = JsonShape.Text(path, group, GroupsKey, NameKey);
        Vector3 pivot = JsonShape.Vector(path, group, name, OriginKey);
        CheckNoRotation(path, group, name);
        return new ModelBone(name, ToMeters(pivot), parent);
    }

    /// <summary>One box from one cube element. The six face rectangles divide by the resolution.</summary>
    private static ModelBox ReadBox(string path, JsonElement element, string name, int bone, Vector2 resolution)
    {
        Vector3 from = JsonShape.Vector(path, element, name, FromKey);
        Vector3 to = JsonShape.Vector(path, element, name, ToKey);
        Vector3 pivot = JsonShape.Vector(path, element, name, OriginKey);
        CheckNoRotation(path, element, name);
        if (from.X > to.X || from.Y > to.Y || from.Z > to.Z)
        {
            throw ContentError.Make(path, FromKey, $"on '{name}' {FromAboveTo}");
        }

        JsonElement faces = JsonShape.Member(path, element, name, FacesKey);
        FaceUv[] uvs = new FaceUv[FaceNames.Length];
        for (int side = 0; side < FaceNames.Length; side++)
        {
            JsonElement face = JsonShape.Member(path, faces, name, FaceNames[side]);
            Vector4 rectangle = JsonShape.Rectangle(path, face, $"{name}.{FaceNames[side]}", UvKey);
            uvs[side] = new FaceUv(
                new Vector2(rectangle.X / resolution.X, rectangle.Y / resolution.Y),
                new Vector2(rectangle.Z / resolution.X, rectangle.W / resolution.Y));
        }

        return new ModelBox(name, bone, ToMeters(from), ToMeters(to), ToMeters(pivot), uvs);
    }

    /// <summary>One attachment point from one locator element. The name is a slot, and each slot has one point.</summary>
    private static AttachmentPoint ReadAttachment(string path, JsonElement element, string name, int bone, List<AttachmentPoint> earlier)
    {
        if (!EquipmentSlots.Contains(name))
        {
            throw ContentError.Make(path, name, NotASlot);
        }

        foreach (AttachmentPoint point in earlier)
        {
            if (point.Slot == name)
            {
                throw ContentError.Make(path, name, SlotTwice);
            }
        }

        Vector3 position = JsonShape.Vector(path, element, name, PositionKey);
        return new AttachmentPoint(name, bone, ToMeters(position));
    }

    /// <summary>A rotation field, when present, must be zero on every axis.</summary>
    private static void CheckNoRotation(string path, JsonElement owner, string name)
    {
        if (!owner.TryGetProperty(RotationKey, out JsonElement _))
        {
            return;
        }

        Vector3 rotation = JsonShape.Vector(path, owner, name, RotationKey);
        if (rotation != Vector3.Zero)
        {
            throw ContentError.Make(path, RotationKey, $"on '{name}' {RotationNotZero}");
        }
    }

    /// <summary>No two bones and no two boxes share a name, and a bone and a box never share one either.</summary>
    private static void CheckUniqueNames(string path, List<ModelBone> bones, List<ModelBox> boxes)
    {
        HashSet<string> names = [];
        foreach (ModelBone bone in bones)
        {
            if (!names.Add(bone.Name))
            {
                throw ContentError.Make(path, bone.Name, NameTwice);
            }
        }

        foreach (ModelBox box in boxes)
        {
            if (!names.Add(box.Name))
            {
                throw ContentError.Make(path, box.Name, NameTwice);
            }
        }
    }

    /// <summary>One node of the outliner in walk order: a group or an element, and the bone above it.</summary>
    private readonly record struct OutlinerEntry(string Id, bool IsGroup, int Parent);
}
