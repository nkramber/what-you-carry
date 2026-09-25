using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Tools.AssetQa;
using WhatYouCarry.Tools.TextureGen;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// F-118. A member name that repeats in one object of an asset file is an error that names the file, at each of the
/// five parse sites. The old parse kept the last value in silence, and a repeated bone track crashed the clip check
/// and the pose with no file named (D-92, T-2).
/// </summary>
[Collection(ConsoleCollection.Name)]
public sealed class RepeatedKeyTests
{
    private const string Rig = "models/rig.bbmodel";
    private const string Attack = "models/rig.attack.json";

    /// <summary>An animation with two tracks of one bone.</summary>
    private static string TwoTracksOfOneBone()
    {
        const string Track = "[{\"tick\": 0, \"rotation\": [0, 0, 0]}, {\"tick\": 10, \"rotation\": [0, 0, -90]}]";
        return "{\"model\": \"" + Rig + "\", \"length\": 20, \"bones\": {\"arm_bone\": " + Track + ", \"arm_bone\": " + Track + "}, "
            + "\"phases\": [{\"start\": 0, \"end\": 20, \"tag\": \"idle\"}]}";
    }

    [Fact]
    public void TheAnimationLoaderRejectsARepeatedBoneTrack()
    {
        ContextException error = Assert.Throws<ContextException>(() => AnimationLoader.Parse(Attack, Encoding.UTF8.GetBytes(TwoTracksOfOneBone())));
        Assert.Contains(Attack, error.Message, StringComparison.Ordinal);
        Assert.Contains("arm_bone", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheModelLoaderRejectsARepeatedKey()
    {
        string model = ModelJson.SiblingRig();
        string repeated = "{\"name\": \"first\", " + model.TrimStart()[1..];
        repeated = repeated.Replace("{\"name\": \"first\", ", "{\"name\": \"first\", \"name\": \"second\", ", StringComparison.Ordinal);
        ContextException error = Assert.Throws<ContextException>(() => BlockbenchLoader.Parse(Rig, Encoding.UTF8.GetBytes(repeated)));
        Assert.Contains(Rig, error.Message, StringComparison.Ordinal);
        Assert.Contains("name", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLayoutRejectsARepeatedKey()
    {
        string layout = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "content", "textures", "layout.json"));
        int open = layout.IndexOf('{', StringComparison.Ordinal);
        string repeated = layout[..(open + 1)] + "\"atlas\": \"textures/other.png\", " + layout[(open + 1)..];
        Assert.Contains("\"atlas\"", layout[(open + 1)..], StringComparison.Ordinal);
        ContextException error = Assert.Throws<ContextException>(() => TextureLayout.Parse(AssetPaths.LayoutFile, Encoding.UTF8.GetBytes(repeated)));
        Assert.Contains(AssetPaths.LayoutFile, error.Message, StringComparison.Ordinal);
        Assert.Contains("atlas", error.Message, StringComparison.Ordinal);

        // The boundary beside the repeated key: the committed layout, with each name once, loads.
        _ = TextureLayout.Parse(AssetPaths.LayoutFile, Encoding.UTF8.GetBytes(layout));
    }

    [Fact]
    public void TheTextureParseRejectsARepeatedKey()
    {
        ContextException error = Assert.Throws<ContextException>(() => TextureJson.Parse(AssetPaths.PaletteFile, Encoding.UTF8.GetBytes("{\"ramps\": [], \"ramps\": []}")));
        Assert.Contains(AssetPaths.PaletteFile, error.Message, StringComparison.Ordinal);
        Assert.Contains("ramps", error.Message, StringComparison.Ordinal);

        // The boundary beside the repeated key: two names that differ parse.
        using JsonDocument document = TextureJson.Parse(AssetPaths.PaletteFile, Encoding.UTF8.GetBytes("{\"ramps\": [], \"colors\": []}"));
        Assert.Equal(2, document.RootElement.EnumerateObject().Count());
    }

    /// <summary>The asset gate reports the repeated bone track as a finding on the animation, and exits 1, where the old gate crashed.</summary>
    [Fact]
    public void AssetQaReportsARepeatedBoneTrack()
    {
        using TemporaryContentDirectory content = new();
        content.Write(Rig, ModelJson.SiblingRig());
        content.Write(Attack, TwoTracksOfOneBone());

        IReadOnlyList<AssetFinding> findings = AssetQaCommand.Findings(AssetSet.Read(content.Content), content.Content);

        Assert.Contains(findings, finding => finding.Path == Attack && finding.Message.Contains("arm_bone", StringComparison.Ordinal));
        Assert.Equal(1, AssetQaCommand.Run(["--root", content.Root]));
    }

    /// <summary>A loader fault that is not a content error, here a string of invalid UTF-8, is a finding that names the file, and the gate exits 1.</summary>
    [Fact]
    public void AssetQaReportsALoaderFaultAsAFinding()
    {
        using TemporaryContentDirectory content = new();
        content.Write(Rig, ModelJson.SiblingRig());
        byte[] animation = Encoding.UTF8.GetBytes(ModelJson.Animation(Rig, "arm_bone", 10, "[0, 0, 0]"));
        int model = Array.IndexOf(animation, (byte)'r', Encoding.UTF8.GetByteCount("{\"model\": \"models/"));
        animation[model] = 0xFF;
        File.WriteAllBytes(Path.Combine(content.Content, "models", "rig.attack.json"), animation);

        IReadOnlyList<AssetFinding> findings = AssetQaCommand.Findings(AssetSet.Read(content.Content), content.Content);

        Assert.Contains(findings, finding => finding.Path == Attack && finding.Message.Contains("the loader failed with", StringComparison.Ordinal));
        Assert.Contains(findings, finding => finding.Path == Attack && finding.Message.Contains("not valid UTF-8", StringComparison.Ordinal));
        Assert.Equal(1, AssetQaCommand.Run(["--root", content.Root]));
    }
}
