using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Godot;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game;
using WhatYouCarry.Game.Models;
using WhatYouCarry.Game.Render;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The Blockbench loader and the box geometry (D-9, D-18, D-86, D-87, OQ-159; PR-13 exit tests 1 and 2).</summary>
public sealed class BlockbenchLoaderTests
{
    /// <summary>The pivots of the ten boxes of the player model, in meters: sixteen file units per meter.</summary>
    public static readonly IReadOnlyDictionary<string, Vector3> PlayerPivots = new Dictionary<string, Vector3>
    {
        ["head_box"] = new(0.0f, 1.3f, 0.0f),
        ["torso_box"] = new(0.0f, 0.625f, 0.0f),
        ["arm_left_upper_box"] = new(-0.40625f, 1.3f, 0.0f),
        ["arm_left_lower_box"] = new(-0.40625f, 0.9625f, 0.0f),
        ["arm_right_upper_box"] = new(0.40625f, 1.3f, 0.0f),
        ["arm_right_lower_box"] = new(0.40625f, 0.9625f, 0.0f),
        ["leg_left_upper_box"] = new(-0.15625f, 0.625f, 0.0f),
        ["leg_left_lower_box"] = new(-0.15625f, 0.3125f, 0.0f),
        ["leg_right_upper_box"] = new(0.15625f, 0.625f, 0.0f),
        ["leg_right_lower_box"] = new(0.15625f, 0.3125f, 0.0f),
    };

    /// <summary>PR-13 exit test 1. The player model has ten boxes, each with its declared pivot, and each box mesh has six quads.</summary>
    [Fact]
    public void LoaderBuildsEveryBox()
    {
        BlockbenchModel model = PlayerModel();

        Assert.Equal(PlayerPivots.Count, model.Boxes.Count);
        foreach (ModelBox box in model.Boxes)
        {
            Assert.True(PlayerPivots.TryGetValue(box.Name, out Vector3 pivot), $"The model holds a box '{box.Name}' that the table does not name.");
            Assert.Equal(pivot, box.Pivot);
            Assert.True(box.Bone >= 0 && box.Bone < model.Bones.Count, $"The box '{box.Name}' names bone {box.Bone}, and the model has {model.Bones.Count} bones.");

            MeshData mesh = BoxGeometry.Build(box);
            Assert.Equal(BoxGeometry.Faces, mesh.QuadCount);
            Assert.Equal(BoxGeometry.Faces * MeshData.QuadVertices, mesh.Positions.Count);
        }
    }

    /// <summary>PR-13 exit test 2. A box with no size is an error that names the box and the field.</summary>
    [Fact]
    public void LoaderRejectsMalformed()
    {
        string json = Model(
            elements: Cube("torso", "e1", from: "[0, 0, 0]", to: null, origin: "[0, 0, 0]"),
            groups: Group("body", "g1"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [\"e1\"]}]");

        ContextException error = Assert.Throws<ContextException>(() => BlockbenchLoader.Parse("models/bad.bbmodel", Encoding.UTF8.GetBytes(json)));

        Assert.Contains("torso", error.Message, StringComparison.Ordinal);
        Assert.Contains("'to'", error.Message, StringComparison.Ordinal);
        Assert.Contains("models/bad.bbmodel", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The player model has one attachment point per slot of D-18, on the bone of its limb.</summary>
    [Fact]
    public void PlayerModelHasEveryAttachmentPoint()
    {
        BlockbenchModel model = PlayerModel();

        Assert.Equal(EquipmentSlots.Names.Count, model.Attachments.Count);
        foreach (string slot in EquipmentSlots.Names)
        {
            AttachmentPoint point = Assert.Single(model.Attachments, point => point.Slot == slot);
            Assert.True(point.Bone >= 0 && point.Bone < model.Bones.Count);
        }

        AttachmentPoint head = Assert.Single(model.Attachments, point => point.Slot == EquipmentSlots.Head);
        Assert.Equal("head", model.Bones[head.Bone].Name);
        Assert.Equal(new Vector3(0.0f, 1.8f, 0.0f), head.Position);

        AttachmentPoint shield = Assert.Single(model.Attachments, point => point.Slot == EquipmentSlots.Shield);
        Assert.Equal("arm_left_lower", model.Bones[shield.Bone].Name);
    }

    /// <summary>Every bone comes after its parent, and the root bone is the body.</summary>
    [Fact]
    public void PlayerModelBonesFollowTheirParents()
    {
        BlockbenchModel model = PlayerModel();

        Assert.Equal(10, model.Bones.Count);
        Assert.Equal("body", model.Bones[0].Name);
        Assert.Equal(ModelBone.NoParent, model.Bones[0].Parent);
        for (int index = 1; index < model.Bones.Count; index++)
        {
            Assert.True(model.Bones[index].Parent >= 0 && model.Bones[index].Parent < index, $"The bone '{model.Bones[index].Name}' comes before its parent.");
        }
    }

    /// <summary>The face rectangles divide by the model resolution, so a face reads as a fraction of the texture.</summary>
    [Fact]
    public void LoaderDividesTheFacesByTheResolution()
    {
        BlockbenchModel model = PlayerModel();
        ModelBox head = Assert.Single(model.Boxes, box => box.Name == "head_box");

        // The north face of the head is at pixels (8, 8) to (16, 16) of 64.
        FaceUv north = head.Faces[(int)BoxSide.North];
        Assert.Equal(new Vector2(0.125f, 0.125f), north.Low);
        Assert.Equal(new Vector2(0.25f, 0.25f), north.High);
    }

    /// <summary>A rotation on a box or a bone is an error that names it, because PR-13 reads the rest pose alone.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LoaderRejectsARotation(bool onTheBox)
    {
        string rotation = "\"rotation\": [0, 45, 0], ";
        string json = Model(
            elements: Cube("torso", "e1", extra: onTheBox ? rotation : string.Empty),
            groups: Group("body", "g1", extra: onTheBox ? string.Empty : rotation),
            outliner: "[{\"uuid\": \"g1\", \"children\": [\"e1\"]}]");

        ContextException error = Assert.Throws<ContextException>(() => Parse(json));

        Assert.Contains(onTheBox ? "torso" : "body", error.Message, StringComparison.Ordinal);
        Assert.Contains("rest pose", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A zero rotation passes, because Blockbench writes one on a box that was never turned.</summary>
    [Fact]
    public void LoaderAcceptsAZeroRotation()
    {
        string json = Model(
            elements: Cube("torso", "e1", extra: "\"rotation\": [0, 0, 0], "),
            groups: Group("body", "g1"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [\"e1\"]}]");

        Assert.Single(Parse(json).Boxes);
    }

    /// <summary>A file of a format before version 5 is an error that names the version, because the groups list is absent there.</summary>
    [Fact]
    public void LoaderRejectsAnOldFormat()
    {
        string json = Model(
            elements: Cube("torso", "e1"),
            groups: Group("body", "g1"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [\"e1\"]}]",
            formatVersion: "4.10");

        ContextException error = Assert.Throws<ContextException>(() => Parse(json));

        Assert.Contains("4.10", error.Message, StringComparison.Ordinal);
        Assert.Contains("format_version", error.Message, StringComparison.Ordinal);
    }

    /// <summary>An element that is neither a cube nor a locator is an error that names it.</summary>
    [Fact]
    public void LoaderRejectsAMeshElement()
    {
        string json = Model(
            elements: "{\"name\": \"blob\", \"uuid\": \"e1\", \"type\": \"mesh\"}",
            groups: Group("body", "g1"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [\"e1\"]}]");

        ContextException error = Assert.Throws<ContextException>(() => Parse(json));

        Assert.Contains("blob", error.Message, StringComparison.Ordinal);
        Assert.Contains("mesh", error.Message, StringComparison.Ordinal);
    }

    /// <summary>An element that the outliner does not place, and an element at the root, are errors that name it.</summary>
    [Theory]
    [InlineData("[{\"uuid\": \"g1\", \"children\": []}]", "not in the outliner")]
    [InlineData("[\"e1\", {\"uuid\": \"g1\", \"children\": []}]", "root of the outliner")]
    [InlineData("[{\"uuid\": \"g1\", \"children\": [\"e1\", \"e1\"]}]", "twice")]
    public void LoaderRejectsAMisplacedElement(string outliner, string reason)
    {
        string json = Model(elements: Cube("torso", "e1"), groups: Group("body", "g1"), outliner: outliner);

        ContextException error = Assert.Throws<ContextException>(() => Parse(json));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A locator whose name is not a slot, and two locators of one slot, are errors.</summary>
    [Fact]
    public void LoaderRejectsABadSlot()
    {
        string unknown = Model(
            elements: Cube("torso", "e1") + ", " + Locator("hat", "e2"),
            groups: Group("body", "g1"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [\"e1\", \"e2\"]}]");
        ContextException unknownError = Assert.Throws<ContextException>(() => Parse(unknown));
        Assert.Contains("hat", unknownError.Message, StringComparison.Ordinal);
        Assert.Contains("D-18", unknownError.Message, StringComparison.Ordinal);

        string twice = Model(
            elements: Cube("torso", "e1") + ", " + Locator("head", "e2") + ", " + Locator("head", "e3"),
            groups: Group("body", "g1"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [\"e1\", \"e2\", \"e3\"]}]");
        ContextException twiceError = Assert.Throws<ContextException>(() => Parse(twice));
        Assert.Contains("one attachment point", twiceError.Message, StringComparison.Ordinal);
    }

    /// <summary>Two boxes of one name are an error, because a keyframe and the asset QA name a box by it.</summary>
    [Fact]
    public void LoaderRejectsARepeatedName()
    {
        string json = Model(
            elements: Cube("torso", "e1") + ", " + Cube("torso", "e2"),
            groups: Group("body", "g1"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [\"e1\", \"e2\"]}]");

        ContextException error = Assert.Throws<ContextException>(() => Parse(json));

        Assert.Contains("torso", error.Message, StringComparison.Ordinal);
        Assert.Contains("unique", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A file that is not JSON is an error that names the file.</summary>
    [Fact]
    public void LoaderRejectsBrokenJson()
    {
        ContextException error = Assert.Throws<ContextException>(() => BlockbenchLoader.Parse("models/broken.bbmodel", Encoding.UTF8.GetBytes("{\"meta\": ")));

        Assert.Contains("models/broken.bbmodel", error.Message, StringComparison.Ordinal);
        Assert.Contains("not valid JSON", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The corners of every face run clockwise as seen from outside, which is the front-face order of the engine.</summary>
    [Fact]
    public void BoxFacesAreClockwiseFromOutside()
    {
        ModelBox box = Assert.Single(PlayerModel().Boxes, box => box.Name == "head_box");
        MeshData mesh = BoxGeometry.Build(box);

        for (int triangle = 0; triangle < mesh.Indices.Count; triangle += 3)
        {
            Vector3 a = mesh.Positions[mesh.Indices[triangle]];
            Vector3 b = mesh.Positions[mesh.Indices[triangle + 1]];
            Vector3 c = mesh.Positions[mesh.Indices[triangle + 2]];
            Vector3 normal = mesh.Normals[mesh.Indices[triangle]];
            Assert.True((b - a).Cross(c - a).Dot(normal) < 0.0f, $"The triangle at index {triangle} runs counterclockwise as seen from its normal {normal}.");
        }
    }

    /// <summary>The vertices of a box are relative to its pivot, so the mesh instance at the pivot puts the box in place.</summary>
    [Fact]
    public void BoxVerticesAreRelativeToThePivot()
    {
        ModelBox box = Assert.Single(PlayerModel().Boxes, box => box.Name == "head_box");
        MeshData mesh = BoxGeometry.Build(box);

        Vector3 low = box.From - box.Pivot;
        Vector3 high = box.To - box.Pivot;
        foreach (Vector3 position in mesh.Positions)
        {
            Assert.True(position.X == low.X || position.X == high.X);
            Assert.True(position.Y == low.Y || position.Y == high.Y);
            Assert.True(position.Z == low.Z || position.Z == high.Z);
        }

        Assert.Equal(new Vector3(-0.25f, 0.0f, -0.25f), low);
        Assert.Equal(new Vector3(0.25f, 0.5f, 0.25f), high);
    }

    /// <summary>The player model of the checkout, which the root node loads at boot.</summary>
    private static BlockbenchModel PlayerModel()
    {
        string file = Path.Combine(RepositoryRoot.Find(), "content", Main.PlayerModelPath);
        return BlockbenchLoader.Parse(Main.PlayerModelPath, File.ReadAllBytes(file));
    }

    private static BlockbenchModel Parse(string json)
    {
        return BlockbenchLoader.Parse("models/test.bbmodel", Encoding.UTF8.GetBytes(json));
    }

    /// <summary>A whole model file around the three lists.</summary>
    private static string Model(string elements, string groups, string outliner, string formatVersion = "5.0")
    {
        return "{\"meta\": {\"format_version\": \"" + formatVersion + "\", \"model_format\": \"free\", \"box_uv\": false}, "
            + "\"name\": \"test\", \"resolution\": {\"width\": 64, \"height\": 64}, "
            + "\"elements\": [" + elements + "], \"groups\": [" + groups + "], \"outliner\": " + outliner + ", \"textures\": []}";
    }

    /// <summary>One cube element. A null size leaves the field out.</summary>
    private static string Cube(string name, string uuid, string from = "[0, 0, 0]", string? to = "[8, 8, 8]", string origin = "[0, 0, 0]", string extra = "")
    {
        string faces = "{\"north\": {\"uv\": [0, 0, 8, 8]}, \"east\": {\"uv\": [0, 0, 8, 8]}, \"south\": {\"uv\": [0, 0, 8, 8]}, "
            + "\"west\": {\"uv\": [0, 0, 8, 8]}, \"up\": {\"uv\": [0, 0, 8, 8]}, \"down\": {\"uv\": [0, 0, 8, 8]}}";
        string size = to is null ? string.Empty : "\"to\": " + to + ", ";
        return "{\"name\": \"" + name + "\", \"uuid\": \"" + uuid + "\", \"type\": \"cube\", " + extra
            + "\"from\": " + from + ", " + size + "\"origin\": " + origin + ", \"faces\": " + faces + "}";
    }

    private static string Locator(string name, string uuid)
    {
        return "{\"name\": \"" + name + "\", \"uuid\": \"" + uuid + "\", \"type\": \"locator\", \"position\": [0, 8, 0]}";
    }

    private static string Group(string name, string uuid, string extra = "")
    {
        return "{\"name\": \"" + name + "\", \"uuid\": \"" + uuid + "\", " + extra + "\"origin\": [0, 0, 0], \"children\": []}";
    }
}
