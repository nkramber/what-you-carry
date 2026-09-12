using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Assets;

/// <summary>
/// Reads an animation file into an <see cref="AnimationClip"/> (D-87, D-298). The file is
/// <c>models/&lt;model&gt;.&lt;animation&gt;.json</c>, next to the model that its <c>model</c> field names.
/// </summary>
/// <remarks>
/// <para>
/// The format is this project's own, so every field is required and an unknown field is an error (D-168).
/// The root holds <c>model</c>, <c>length</c>, <c>bones</c>, and <c>phases</c>. Each bone entry is a list of
/// keyframes with a <c>tick</c> and a <c>rotation</c> of three euler degrees. Each phase has a <c>start</c>,
/// an <c>end</c>, and a <c>tag</c>. The phases run from tick zero to the length with no gap and no overlap.
/// </para>
/// <para>
/// Every failure names the file and the bone, the keyframe, or the phase at fault (D-92, T-2). The loader
/// does not open the model. The asset QA checks that each bone name is a bone of the model when it poses it.
/// </para>
/// </remarks>
public static class AnimationLoader
{
    private const string RootName = "the file";
    private const string ModelKey = "model";
    private const string LengthKey = "length";
    private const string BonesKey = "bones";
    private const string PhasesKey = "phases";
    private const string TickKey = "tick";
    private const string RotationKey = "rotation";
    private const string StartKey = "start";
    private const string EndKey = "end";
    private const string TagKey = "tag";

    private static readonly string[] RootKeys = [ModelKey, LengthKey, BonesKey, PhasesKey];
    private static readonly string[] KeyframeKeys = [TickKey, RotationKey];
    private static readonly string[] PhaseKeys = [StartKey, EndKey, TagKey];

    private const string NotOneObject = "the file must hold one JSON object";
    private const string NotAList = "is not a list";
    private const string LengthNotPositive = "must be at least one tick";
    private const string NoKeyframe = "holds no keyframe, and a track needs at least one";
    private const string TickOutOfRange = "is outside the animation, which runs from tick 0 to the length";
    private const string TickNotAscending = "does not come after the keyframe before it, and the keyframes of a bone ascend";
    private const string NoPhase = "holds no phase, and the phases cover the whole animation";
    private const string PhaseNotAfterStart = "has an end that is not after its start";
    private const string PhaseGap = "does not start where the phase before it ends, and the phases have no gap and no overlap";
    private const string FirstPhaseNotAtZero = "does not start at tick 0";
    private const string LastPhaseNotAtLength = "does not end at the length of the animation";
    private const string NotATag = "is not a phase tag of D-298";
    private const string NotNextToTheModel = "is not next to its model, and an animation file lives in the directory of its model (D-298)";
    private const string NameNotFromTheModel = "does not start with the model stem and a dot, as in 'models/player.attack.json' (D-298)";

    /// <summary>The clip of one file. The path is relative to the content directory, with forward slashes.</summary>
    /// <exception cref="ContextException">The file is not valid JSON, a field is absent, of another kind, or outside its bounds, or the file name does not follow from the model. The error names the bone, the keyframe, or the phase.</exception>
    public static AnimationClip Parse(string path, byte[] bytes)
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

            JsonShape.CheckNoUnknownMember(path, root, RootName, RootKeys);
            string model = JsonShape.Text(path, root, RootName, ModelKey);
            string name = AnimationName(path, model);
            int length = JsonShape.WholeNumber(path, root, RootName, LengthKey);
            if (length < 1)
            {
                throw ContentError.Make(path, LengthKey, LengthNotPositive);
            }

            List<BoneTrack> tracks = ReadTracks(path, JsonShape.Member(path, root, RootName, BonesKey), length);
            List<PhaseRange> phases = ReadPhases(path, JsonShape.Member(path, root, RootName, PhasesKey), length);
            return new AnimationClip(model, name, length, tracks, phases);
        }
    }

    /// <summary>The animation name from the file name, which must be the model stem, a dot, the name, and the extension.</summary>
    private static string AnimationName(string path, string model)
    {
        string modelStem = AssetPaths.ModelStem(model);
        string prefix = modelStem + AssetPaths.AnimationSeparator;
        if (Directory(path) != Directory(model))
        {
            throw ContentError.Make(path, ModelKey, $"is '{model}', and the animation {NotNextToTheModel}");
        }

        if (!path.StartsWith(prefix, System.StringComparison.Ordinal) || !path.EndsWith(AssetPaths.AnimationExtension, System.StringComparison.Ordinal))
        {
            throw ContentError.Make(path, ModelKey, $"is '{model}', and the file name {NameNotFromTheModel}");
        }

        int nameLength = path.Length - prefix.Length - AssetPaths.AnimationExtension.Length;
        if (nameLength <= 0)
        {
            throw ContentError.Make(path, ModelKey, $"is '{model}', and the file name {NameNotFromTheModel}");
        }

        string name = path.Substring(prefix.Length, nameLength);
        if (name.IndexOf(AssetPaths.AnimationSeparator, System.StringComparison.Ordinal) >= 0)
        {
            throw ContentError.Make(path, ModelKey, $"is '{model}', and the file name {NameNotFromTheModel}");
        }

        return name;
    }

    /// <summary>The directory part of a content path, up to and including the last slash.</summary>
    private static string Directory(string path)
    {
        int slash = path.LastIndexOf('/');
        return slash < 0 ? string.Empty : path.Substring(0, slash + 1);
    }

    /// <summary>One track per member of the bones object. Each member is a list of keyframes.</summary>
    private static List<BoneTrack> ReadTracks(string path, JsonElement bones, int length)
    {
        if (bones.ValueKind != JsonValueKind.Object)
        {
            throw ContentError.Make(path, BonesKey, "is not an object of bone names");
        }

        List<BoneTrack> tracks = [];
        foreach (JsonProperty bone in bones.EnumerateObject())
        {
            tracks.Add(new BoneTrack(bone.Name, ReadKeyframes(path, bone.Name, bone.Value, length)));
        }

        return tracks;
    }

    /// <summary>The keyframes of one bone: at least one, in ascending tick order, inside the animation.</summary>
    private static List<Keyframe> ReadKeyframes(string path, string bone, JsonElement list, int length)
    {
        if (list.ValueKind != JsonValueKind.Array)
        {
            throw ContentError.Make(path, bone, NotAList);
        }

        List<Keyframe> keyframes = [];
        int index = 0;
        foreach (JsonElement item in list.EnumerateArray())
        {
            string keyframeName = $"{bone}[{index.ToString(CultureInfo.InvariantCulture)}]";
            JsonShape.CheckNoUnknownMember(path, item, keyframeName, KeyframeKeys);
            int tick = JsonShape.WholeNumber(path, item, keyframeName, TickKey);
            if (tick < 0 || tick > length)
            {
                throw ContentError.Make(path, TickKey, $"on '{keyframeName}' is {tick.ToString(CultureInfo.InvariantCulture)}, which {TickOutOfRange}");
            }

            if (keyframes.Count > 0 && tick <= keyframes[keyframes.Count - 1].Tick)
            {
                throw ContentError.Make(path, TickKey, $"on '{keyframeName}' is {tick.ToString(CultureInfo.InvariantCulture)}, which {TickNotAscending}");
            }

            Vector3 rotation = JsonShape.Vector(path, item, keyframeName, RotationKey);
            keyframes.Add(new Keyframe(tick, rotation));
            index++;
        }

        if (keyframes.Count == 0)
        {
            throw ContentError.Make(path, bone, NoKeyframe);
        }

        return keyframes;
    }

    /// <summary>The phases: at least one, from tick zero to the length, each after the one before it, with a tag.</summary>
    private static List<PhaseRange> ReadPhases(string path, JsonElement list, int length)
    {
        if (list.ValueKind != JsonValueKind.Array)
        {
            throw ContentError.Make(path, PhasesKey, NotAList);
        }

        List<PhaseRange> phases = [];
        int index = 0;
        foreach (JsonElement item in list.EnumerateArray())
        {
            string phaseName = $"{PhasesKey}[{index.ToString(CultureInfo.InvariantCulture)}]";
            JsonShape.CheckNoUnknownMember(path, item, phaseName, PhaseKeys);
            int start = JsonShape.WholeNumber(path, item, phaseName, StartKey);
            int end = JsonShape.WholeNumber(path, item, phaseName, EndKey);
            string tag = JsonShape.Text(path, item, phaseName, TagKey);
            if (end <= start)
            {
                throw ContentError.Make(path, EndKey, $"on '{phaseName}' {PhaseNotAfterStart}");
            }

            if (!PhaseTags.Contains(tag))
            {
                throw ContentError.Make(path, TagKey, $"on '{phaseName}' is '{tag}', which {NotATag}");
            }

            if (phases.Count == 0 && start != 0)
            {
                throw ContentError.Make(path, StartKey, $"on '{phaseName}' {FirstPhaseNotAtZero}");
            }

            if (phases.Count > 0 && start != phases[phases.Count - 1].End)
            {
                throw ContentError.Make(path, StartKey, $"on '{phaseName}' {PhaseGap}");
            }

            phases.Add(new PhaseRange(start, end, tag));
            index++;
        }

        if (phases.Count == 0)
        {
            throw ContentError.Make(path, PhasesKey, NoPhase);
        }

        if (phases[phases.Count - 1].End != length)
        {
            throw ContentError.Make(path, EndKey, $"on '{PhasesKey}[{(phases.Count - 1).ToString(CultureInfo.InvariantCulture)}]' {LastPhaseNotAtLength}");
        }

        return phases;
    }
}
