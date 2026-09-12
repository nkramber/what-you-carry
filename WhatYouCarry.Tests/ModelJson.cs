namespace WhatYouCarry.Tests;

/// <summary>The text of a small Blockbench project file, for the loader and the asset QA tests.</summary>
internal static class ModelJson
{
    /// <summary>A whole model file around the three lists.</summary>
    public static string Model(string elements, string groups, string outliner, string formatVersion = "5.0", string name = "test")
    {
        return "{\"meta\": {\"format_version\": \"" + formatVersion + "\", \"model_format\": \"free\", \"box_uv\": false}, "
            + "\"name\": \"" + name + "\", \"resolution\": {\"width\": 64, \"height\": 64}, "
            + "\"elements\": [" + elements + "], \"groups\": [" + groups + "], \"outliner\": " + outliner + ", \"textures\": []}";
    }

    /// <summary>One cube element. A null size leaves the field out.</summary>
    public static string Cube(string name, string uuid, string from = "[0, 0, 0]", string? to = "[8, 8, 8]", string origin = "[0, 0, 0]", string extra = "")
    {
        string faces = "{\"north\": {\"uv\": [0, 0, 8, 8]}, \"east\": {\"uv\": [0, 0, 8, 8]}, \"south\": {\"uv\": [0, 0, 8, 8]}, "
            + "\"west\": {\"uv\": [0, 0, 8, 8]}, \"up\": {\"uv\": [0, 0, 8, 8]}, \"down\": {\"uv\": [0, 0, 8, 8]}}";
        string size = to is null ? string.Empty : "\"to\": " + to + ", ";
        return "{\"name\": \"" + name + "\", \"uuid\": \"" + uuid + "\", \"type\": \"cube\", " + extra
            + "\"from\": " + from + ", " + size + "\"origin\": " + origin + ", \"faces\": " + faces + "}";
    }

    /// <summary>One locator element, at eight units up.</summary>
    public static string Locator(string name, string uuid)
    {
        return "{\"name\": \"" + name + "\", \"uuid\": \"" + uuid + "\", \"type\": \"locator\", \"position\": [0, 8, 0]}";
    }

    /// <summary>One group, which is a bone.</summary>
    public static string Group(string name, string uuid, string extra = "", string origin = "[0, 0, 0]")
    {
        return "{\"name\": \"" + name + "\", \"uuid\": \"" + uuid + "\", " + extra + "\"origin\": " + origin + ", \"children\": []}";
    }

    /// <summary>
    /// A rig of a torso and an arm under two sibling bones of one root bone. The torso fills x from -4 to 4,
    /// and the arm stands next to it from x 4 to 8, so the two touch on one face at rest. The arm bone pivots
    /// at the shoulder, at (4, 16, 0), and a turn of minus 90 degrees about Z swings the arm into the torso.
    /// </summary>
    public static string SiblingRig()
    {
        return Model(
            elements: Cube("torso", "e1", from: "[-4, 0, -4]", to: "[4, 16, 4]", origin: "[0, 0, 0]")
                + ", " + Cube("arm", "e2", from: "[4, 8, -2]", to: "[8, 16, 2]", origin: "[4, 16, 0]"),
            groups: Group("body", "g1") + ", " + Group("torso_bone", "g2") + ", " + Group("arm_bone", "g3", origin: "[4, 16, 0]"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [{\"uuid\": \"g2\", \"children\": [\"e1\"]}, {\"uuid\": \"g3\", \"children\": [\"e2\"]}]}]",
            name: "rig");
    }

    /// <summary>The same rig with the arm bone under the torso bone, so the two boxes meet at a joint.</summary>
    public static string JointRig()
    {
        return Model(
            elements: Cube("torso", "e1", from: "[-4, 0, -4]", to: "[4, 16, 4]", origin: "[0, 0, 0]")
                + ", " + Cube("arm", "e2", from: "[4, 8, -2]", to: "[8, 16, 2]", origin: "[4, 16, 0]"),
            groups: Group("body", "g1") + ", " + Group("torso_bone", "g2") + ", " + Group("arm_bone", "g3", origin: "[4, 16, 0]"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [{\"uuid\": \"g2\", \"children\": [\"e1\", {\"uuid\": \"g3\", \"children\": [\"e2\"]}]}]}]",
            name: "rig");
    }

    /// <summary>A body of one torso box under the root bone, for the overlay tests.</summary>
    public static string TorsoBody()
    {
        return Model(
            elements: Cube("torso", "e1", from: "[-4, 0, -4]", to: "[4, 16, 4]", origin: "[0, 0, 0]"),
            groups: Group("body", "g1"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [\"e1\"]}]",
            name: "player");
    }

    /// <summary>An overlay of one box with a name, from one corner to the other.</summary>
    public static string Overlay(string boxName, string from, string to)
    {
        return Model(
            elements: Cube(boxName, "e1", from: from, to: to, origin: "[0, 0, 0]"),
            groups: Group("body", "g1"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [\"e1\"]}]",
            name: "chest");
    }

    /// <summary>An animation of one bone with two keyframes, at tick 0 and at the tick given, over one idle phase.</summary>
    public static string Animation(string model, string bone, int tick, string rotation, int length = 20)
    {
        return "{\"model\": \"" + model + "\", \"length\": " + length + ", "
            + "\"bones\": {\"" + bone + "\": [{\"tick\": 0, \"rotation\": [0, 0, 0]}, {\"tick\": " + tick + ", \"rotation\": " + rotation + "}]}, "
            + "\"phases\": [{\"start\": 0, \"end\": " + length + ", \"tag\": \"idle\"}]}";
    }
}
