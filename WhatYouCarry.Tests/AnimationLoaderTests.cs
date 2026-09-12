using System;
using System.Collections.Generic;
using System.Text;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The animation loader and the clip of the Assets project (D-87, D-298).</summary>
public sealed class AnimationLoaderTests
{
    private const string Path = "models/rig.attack.json";
    private const string Model = "models/rig.bbmodel";

    /// <summary>A valid file gives its model, its name from the file name, its length, its tracks, and its phases.</summary>
    [Fact]
    public void LoaderReadsEveryField()
    {
        string json = "{\"model\": \"models/rig.bbmodel\", \"length\": 30, "
            + "\"bones\": {\"arm_bone\": [{\"tick\": 0, \"rotation\": [0, 0, 0]}, {\"tick\": 12, \"rotation\": [-90, 0, 0]}], \"body\": [{\"tick\": 5, \"rotation\": [0, 10, 0]}]}, "
            + "\"phases\": [{\"start\": 0, \"end\": 12, \"tag\": \"windup\"}, {\"start\": 12, \"end\": 18, \"tag\": \"active\"}, {\"start\": 18, \"end\": 30, \"tag\": \"recovery\"}]}";

        AnimationClip clip = Parse(json);

        Assert.Equal(Model, clip.Model);
        Assert.Equal("attack", clip.Name);
        Assert.Equal(30, clip.Length);
        Assert.Equal(2, clip.Tracks.Count);
        Assert.Equal("arm_bone", clip.Tracks[0].Bone);
        Assert.Equal(new Keyframe(12, new Vector3(-90.0f, 0.0f, 0.0f)), clip.Tracks[0].Keyframes[1]);
        Assert.Equal(3, clip.Phases.Count);
        Assert.Equal(new PhaseRange(12, 18, PhaseTags.Active), clip.Phases[1]);
        Assert.Equal(new int[] { 0, 5, 12 }, clip.KeyframeTicks());
    }

    /// <summary>A track is linear between two keyframes, and it holds the first before it and the last after it.</summary>
    [Fact]
    public void TrackInterpolatesBetweenKeyframes()
    {
        BoneTrack track = new("arm", [new Keyframe(10, new Vector3(0.0f, 0.0f, 0.0f)), new Keyframe(20, new Vector3(-90.0f, 40.0f, 0.0f))]);

        Assert.Equal(new Vector3(0.0f, 0.0f, 0.0f), track.RotationAt(0));
        Assert.Equal(new Vector3(0.0f, 0.0f, 0.0f), track.RotationAt(10));
        Assert.Equal(new Vector3(-45.0f, 20.0f, 0.0f), track.RotationAt(15));
        Assert.Equal(new Vector3(-90.0f, 40.0f, 0.0f), track.RotationAt(20));
        Assert.Equal(new Vector3(-90.0f, 40.0f, 0.0f), track.RotationAt(25));
    }

    /// <summary>The rotations at one tick hold every bone with a track, and no other bone.</summary>
    [Fact]
    public void RotationsAtHoldEveryTrack()
    {
        AnimationClip clip = Parse(ModelJson.Animation(Model, "arm_bone", 10, "[0, 0, -90]"));

        IReadOnlyDictionary<string, Vector3> rotations = clip.RotationsAt(5);

        Assert.Single(rotations);
        Assert.Equal(new Vector3(0.0f, 0.0f, -45.0f), rotations["arm_bone"]);
    }

    /// <summary>A field that the format does not name is an error that names it (D-168).</summary>
    [Theory]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"length\": 20, \"bones\": {}, \"phases\": [{\"start\": 0, \"end\": 20, \"tag\": \"idle\"}], \"loop\": true}", "loop")]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"length\": 20, \"bones\": {\"arm_bone\": [{\"tick\": 0, \"rotation\": [0, 0, 0], \"ease\": 1}]}, \"phases\": [{\"start\": 0, \"end\": 20, \"tag\": \"idle\"}]}", "ease")]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"length\": 20, \"bones\": {}, \"phases\": [{\"start\": 0, \"end\": 20, \"tag\": \"idle\", \"name\": \"x\"}]}", "name")]
    public void LoaderRejectsAnUnknownField(string json, string field)
    {
        ContextException error = Assert.Throws<ContextException>(() => Parse(json));

        Assert.Contains(field, error.Message, StringComparison.Ordinal);
        Assert.Contains("D-168", error.Message, StringComparison.Ordinal);
    }

    /// <summary>An absent field is an error that names it.</summary>
    [Theory]
    [InlineData("{\"length\": 20, \"bones\": {}, \"phases\": [{\"start\": 0, \"end\": 20, \"tag\": \"idle\"}]}", "model")]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"bones\": {}, \"phases\": [{\"start\": 0, \"end\": 20, \"tag\": \"idle\"}]}", "length")]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"length\": 20, \"phases\": [{\"start\": 0, \"end\": 20, \"tag\": \"idle\"}]}", "bones")]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"length\": 20, \"bones\": {}}", "phases")]
    [InlineData("{\"model\": \"models/rig.bbmodel\", \"length\": 20, \"bones\": {\"arm_bone\": [{\"tick\": 0}]}, \"phases\": [{\"start\": 0, \"end\": 20, \"tag\": \"idle\"}]}", "rotation")]
    public void LoaderRejectsAnAbsentField(string json, string field)
    {
        ContextException error = Assert.Throws<ContextException>(() => Parse(json));

        Assert.Contains(field, error.Message, StringComparison.Ordinal);
        Assert.Contains(Path, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A keyframe outside the animation, two keyframes out of order, and an empty track are errors that name the bone.</summary>
    [Theory]
    [InlineData("[{\"tick\": 0, \"rotation\": [0, 0, 0]}, {\"tick\": 21, \"rotation\": [0, 0, 0]}]", "outside the animation")]
    [InlineData("[{\"tick\": -1, \"rotation\": [0, 0, 0]}]", "outside the animation")]
    [InlineData("[{\"tick\": 10, \"rotation\": [0, 0, 0]}, {\"tick\": 10, \"rotation\": [0, 0, 0]}]", "ascend")]
    [InlineData("[{\"tick\": 10, \"rotation\": [0, 0, 0]}, {\"tick\": 4, \"rotation\": [0, 0, 0]}]", "ascend")]
    [InlineData("[]", "at least one")]
    public void LoaderRejectsABadTrack(string keyframes, string reason)
    {
        string json = "{\"model\": \"models/rig.bbmodel\", \"length\": 20, \"bones\": {\"arm_bone\": " + keyframes + "}, \"phases\": [{\"start\": 0, \"end\": 20, \"tag\": \"idle\"}]}";

        ContextException error = Assert.Throws<ContextException>(() => Parse(json));

        Assert.Contains("arm_bone", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>The phases start at zero, follow each other with no gap, end at the length, and carry a tag of D-298.</summary>
    [Theory]
    [InlineData("[]", "no phase")]
    [InlineData("[{\"start\": 2, \"end\": 20, \"tag\": \"idle\"}]", "tick 0")]
    [InlineData("[{\"start\": 0, \"end\": 10, \"tag\": \"windup\"}, {\"start\": 12, \"end\": 20, \"tag\": \"active\"}]", "no gap")]
    [InlineData("[{\"start\": 0, \"end\": 10, \"tag\": \"windup\"}, {\"start\": 8, \"end\": 20, \"tag\": \"active\"}]", "no overlap")]
    [InlineData("[{\"start\": 0, \"end\": 18, \"tag\": \"idle\"}]", "length of the animation")]
    [InlineData("[{\"start\": 0, \"end\": 0, \"tag\": \"idle\"}]", "not after its start")]
    [InlineData("[{\"start\": 0, \"end\": 20, \"tag\": \"swing\"}]", "not a phase tag")]
    public void LoaderRejectsBadPhases(string phases, string reason)
    {
        string json = "{\"model\": \"models/rig.bbmodel\", \"length\": 20, \"bones\": {}, \"phases\": " + phases + "}";

        ContextException error = Assert.Throws<ContextException>(() => Parse(json));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A length under one tick is an error.</summary>
    [Fact]
    public void LoaderRejectsAZeroLength()
    {
        string json = "{\"model\": \"models/rig.bbmodel\", \"length\": 0, \"bones\": {}, \"phases\": []}";

        ContextException error = Assert.Throws<ContextException>(() => Parse(json));

        Assert.Contains("length", error.Message, StringComparison.Ordinal);
        Assert.Contains("at least one tick", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The file name is the model stem, a dot, the name, and the extension, in the directory of the model (D-298).</summary>
    [Theory]
    [InlineData("models/rig.attack.json", "models/rig.bbmodel", "attack")]
    [InlineData("models/rig.walk_slow.json", "models/rig.bbmodel", "walk_slow")]
    public void LoaderTakesTheNameFromTheFileName(string path, string model, string name)
    {
        string json = ModelJson.Animation(model, "arm_bone", 10, "[0, 0, 0]");

        AnimationClip clip = AnimationLoader.Parse(path, Encoding.UTF8.GetBytes(json));

        Assert.Equal(name, clip.Name);
    }

    /// <summary>A file that is not next to its model, or whose name does not follow from the model, is an error that names the model.</summary>
    [Theory]
    [InlineData("animations/rig.attack.json", "models/rig.bbmodel", "next to its model")]
    [InlineData("models/hero.attack.json", "models/rig.bbmodel", "model stem")]
    [InlineData("models/rig.json", "models/rig.bbmodel", "model stem")]
    [InlineData("models/rig..json", "models/rig.bbmodel", "model stem")]
    [InlineData("models/rig.attack.slow.json", "models/rig.bbmodel", "model stem")]
    public void LoaderRejectsAFileNameThatDoesNotFollowFromTheModel(string path, string model, string reason)
    {
        string json = ModelJson.Animation(model, "arm_bone", 10, "[0, 0, 0]");

        ContextException error = Assert.Throws<ContextException>(() => AnimationLoader.Parse(path, Encoding.UTF8.GetBytes(json)));

        Assert.Contains(model, error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A file that is not JSON is an error that names the file.</summary>
    [Fact]
    public void LoaderRejectsBrokenJson()
    {
        ContextException error = Assert.Throws<ContextException>(() => Parse("{\"model\": "));

        Assert.Contains(Path, error.Message, StringComparison.Ordinal);
        Assert.Contains("not valid JSON", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The animation path of a model and a name follows the file name rule.</summary>
    [Fact]
    public void AssetPathsBuildTheAnimationPath()
    {
        Assert.Equal("models/player.attack.json", AssetPaths.AnimationPath("models/player.bbmodel", "attack"));
        Assert.Equal("models/player", AssetPaths.ModelStem("models/player.bbmodel"));
    }

    private static AnimationClip Parse(string json)
    {
        return AnimationLoader.Parse(Path, Encoding.UTF8.GetBytes(json));
    }
}
