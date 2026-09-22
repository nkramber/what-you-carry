using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Tools.TextureGen;

namespace WhatYouCarry.Tools.AudioSynth;

/// <summary>The kind of one layer of a sound (D-462).</summary>
public enum LayerKind
{
    /// <summary>A bank of damped resonances that a short noise burst strikes.</summary>
    Modal = 0,

    /// <summary>Colored noise through a filter with a cutoff envelope.</summary>
    Noise = 1,

    /// <summary>A tone with a pitch envelope.</summary>
    Tone = 2,

    /// <summary>Band levels, analysed from a reference, rendered as noise with random phases (D-464).</summary>
    Spectral = 3,

    /// <summary>A committed CC0 recording, for a pitched sound (D-467).</summary>
    Recording = 4,
}

/// <summary>The color of the noise of a noise layer.</summary>
public enum NoiseColor
{
    /// <summary>Equal power at every frequency.</summary>
    White = 0,

    /// <summary>Power that falls 3 decibels each octave.</summary>
    Pink = 1,

    /// <summary>Power that falls 6 decibels each octave.</summary>
    Brown = 2,
}

/// <summary>The filter of a noise layer.</summary>
public enum FilterKind
{
    /// <summary>Passes the frequencies below the cutoff.</summary>
    LowPass = 0,

    /// <summary>Passes the frequencies above the cutoff.</summary>
    HighPass = 1,

    /// <summary>Passes the frequencies near the cutoff.</summary>
    BandPass = 2,
}

/// <summary>The wave of a tone layer. An FM tone is a sine carrier with a sine modulator.</summary>
public enum ToneWave
{
    /// <summary>A sine wave.</summary>
    Sine = 0,

    /// <summary>A sawtooth wave.</summary>
    Saw = 1,

    /// <summary>A square wave.</summary>
    Square = 2,

    /// <summary>A sine carrier whose phase a sine modulator moves.</summary>
    Fm = 3,
}

/// <summary>One resonance of a modal layer: its frequency, the time of its fall of 60 decibels, and its amplitude.</summary>
public sealed record Mode(double FrequencyHz, double DecayMs, double Amplitude);

/// <summary>
/// One layer of a sound. Every kind has the delay and the gain. A modal, noise, or tone layer has the envelope. A modal
/// layer reads the strike and the modes, a noise layer reads the color, the filter, the cutoffs, and the resonance, and
/// a tone layer reads the wave, the frequencies, and, for an FM tone, the ratio and the index. A spectral layer reads the
/// trim, the fade, the time stretch, the pitch shift, the tilt, the frame size, and the levels (D-465). A recording layer
/// reads the trim, the fade, and the file (D-467). A field of another kind holds zero, its first value, or an empty value.
/// </summary>
public sealed record Layer(
    LayerKind Kind,
    double DelayMs,
    double AttackMs,
    double HoldMs,
    double DecayMs,
    double Gain,
    double StrikeMs,
    IReadOnlyList<Mode> Modes,
    NoiseColor Color,
    FilterKind Filter,
    double CutoffStartHz,
    double CutoffEndHz,
    double Resonance,
    ToneWave Wave,
    double FrequencyStartHz,
    double FrequencyEndHz,
    double FmRatio,
    double FmIndex,
    double TrimMs,
    double FadeMs,
    double TimeStretch,
    double PitchSemitones,
    double TiltDbPerOctave,
    int FrameSamples,
    IReadOnlyList<int[]> Levels,
    string File);

/// <summary>The range of one number field: its lowest and highest value.</summary>
public sealed record FieldRange(double Minimum, double Maximum);

/// <summary>
/// One sound parameter file under <c>audio/sfx/</c> (D-462, D-466): a seed, a gain, and 1 to 4 layers.
/// <see cref="LayeredSynthesizer"/> renders it, and the <c>audio-analyze</c> command writes the levels of a spectral layer.
/// </summary>
/// <remarks>
/// <para>
/// The file is a file of this project, so an absent field and an unknown field are each an error that names the
/// field and its place (D-92, D-168). Each kind of layer has its own field set, and a field of another kind is an
/// unknown field. Each number stays inside the range of <see cref="Ranges"/>. A level of a spectral layer is a whole
/// number from <see cref="SpectralLayer.FloorDecibels"/> to <see cref="SpectralLayer.CeilingDecibels"/>, and each frame
/// holds one level for each of the <see cref="SpectralLayer.BandCount"/> bands.
/// </para>
/// <para>
/// The seed starts a xorshift sequence, and a zero seed stays zero forever, so a zero seed is an error.
/// </para>
/// </remarks>
public sealed record SoundDefinition(string ContentPath, uint Seed, double Gain, IReadOnlyList<Layer> Layers)
{
    /// <summary>The most layers of one sound (D-462).</summary>
    public const int MaxLayers = 4;

    /// <summary>The most modes of one modal layer.</summary>
    public const int MaxModes = 8;

    /// <summary>The field of the seed.</summary>
    public const string SeedKey = "seed";

    /// <summary>The field of the gain of the sound, or of one layer.</summary>
    public const string GainKey = "gain";

    /// <summary>The field of the list of layers.</summary>
    public const string LayersKey = "layers";

    /// <summary>The field of the kind of a layer.</summary>
    public const string KindKey = "kind";

    /// <summary>The field of the list of modes of a modal layer.</summary>
    public const string ModesKey = "modes";

    /// <summary>The field of the wave of a tone layer.</summary>
    public const string WaveKey = "wave";

    /// <summary>The field of the content path of the file of a recording layer.</summary>
    public const string FileKey = "file";

    /// <summary>The field of the frame size of a spectral layer, in samples.</summary>
    public const string FrameKey = "frameSamples";

    /// <summary>The field of the level frames of a spectral layer.</summary>
    public const string LevelsKey = "levels";

    /// <summary>The most frames of one spectral layer: about 12 seconds.</summary>
    public const int MaxFrames = 4096;

    /// <summary>The range of every number field, by name. The seed is a whole number and stands apart.</summary>
    public static readonly IReadOnlyDictionary<string, FieldRange> Ranges = new Dictionary<string, FieldRange>
    {
        [GainKey] = new(0.0, 1.0),
        ["delayMs"] = new(0.0, 2000.0),
        ["attackMs"] = new(0.0, 2000.0),
        ["holdMs"] = new(0.0, 4000.0),
        ["decayMs"] = new(1.0, 8000.0),
        ["strikeMs"] = new(0.1, 50.0),
        ["frequencyHz"] = new(20.0, 16000.0),
        ["amplitude"] = new(0.0, 1.0),
        ["cutoffStartHz"] = new(20.0, 20000.0),
        ["cutoffEndHz"] = new(20.0, 20000.0),
        ["resonance"] = new(0.0, 0.95),
        ["frequencyStartHz"] = new(20.0, 16000.0),
        ["frequencyEndHz"] = new(20.0, 16000.0),
        ["fmRatio"] = new(0.25, 16.0),
        ["fmIndex"] = new(0.0, 20.0),
        ["trimMs"] = new(1.0, 12000.0),
        ["fadeMs"] = new(0.0, 1000.0),
        ["timeStretch"] = new(0.25, 4.0),
        ["pitchSemitones"] = new(-24.0, 24.0),
        ["tiltDbPerOctave"] = new(-12.0, 12.0),
    };

    private static readonly string[] RootFields = [SeedKey, GainKey, LayersKey];
    private static readonly string[] EnvelopeFields = [KindKey, "delayMs", "attackMs", "holdMs", "decayMs", GainKey];
    private static readonly string[] ModalFields = [.. EnvelopeFields, "strikeMs", ModesKey];
    private static readonly string[] NoiseFields = [.. EnvelopeFields, "color", "filter", "cutoffStartHz", "cutoffEndHz", "resonance"];
    private static readonly string[] ToneFields = [.. EnvelopeFields, WaveKey, "frequencyStartHz", "frequencyEndHz"];
    private static readonly string[] FmToneFields = [.. ToneFields, "fmRatio", "fmIndex"];
    private static readonly string[] SpectralFields = [KindKey, "delayMs", GainKey, "trimMs", "fadeMs", "timeStretch", "pitchSemitones", "tiltDbPerOctave", FrameKey, LevelsKey];
    private static readonly string[] RecordingFields = [KindKey, "delayMs", GainKey, "trimMs", "fadeMs", FileKey];
    private static readonly string[] ModeFields = ["frequencyHz", "decayMs", "amplitude"];
    private static readonly string[] KindNames = ["modal", "noise", "tone", "spectral", "recording"];
    private static readonly string[] ColorNames = ["white", "pink", "brown"];
    private static readonly string[] FilterNames = ["lowpass", "highpass", "bandpass"];
    private static readonly string[] WaveNames = ["sine", "saw", "square", "fm"];

    /// <summary>The definition of one file.</summary>
    /// <exception cref="ContextException">The file is not a JSON object, a field is absent, unknown, or of another kind, a number is outside its range, a name is not a name of its field, a list has the wrong count, or the seed is zero.</exception>
    public static SoundDefinition Parse(string path, byte[] bytes)
    {
        using JsonDocument document = TextureJson.Parse(path, bytes);
        JsonElement root = document.RootElement;
        JsonShape.CheckNoUnknownMember(path, root, TextureJson.RootName, RootFields);
        uint seed = ReadSeed(path, root);
        double gain = Number(path, root, TextureJson.RootName, GainKey);
        JsonElement list = JsonShape.Member(path, root, TextureJson.RootName, LayersKey);
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() < 1 || list.GetArrayLength() > MaxLayers)
        {
            throw ContentError.Make(path, LayersKey, $"is not a list of 1 to {Text(MaxLayers)} layers");
        }

        List<Layer> layers = [];
        foreach (JsonElement item in list.EnumerateArray())
        {
            layers.Add(ReadLayer(path, item, $"{LayersKey}[{Text(layers.Count)}]"));
        }

        return new SoundDefinition(path, seed, gain, layers);
    }

    /// <summary>One layer, with the field set of its kind.</summary>
    private static Layer ReadLayer(string path, JsonElement item, string owner)
    {
        LayerKind kind = (LayerKind)Name(path, item, owner, KindKey, KindNames);
        if (kind == LayerKind.Spectral)
        {
            return ReadSpectral(path, item, owner);
        }

        if (kind == LayerKind.Recording)
        {
            return ReadRecording(path, item, owner);
        }

        double delay = Number(path, item, owner, "delayMs");
        double attack = Number(path, item, owner, "attackMs");
        double hold = Number(path, item, owner, "holdMs");
        double decay = Number(path, item, owner, "decayMs");
        double gain = Number(path, item, owner, GainKey);
        Layer empty = Empty(kind) with { DelayMs = delay, AttackMs = attack, HoldMs = hold, DecayMs = decay, Gain = gain };
        switch (kind)
        {
            case LayerKind.Modal:
                JsonShape.CheckNoUnknownMember(path, item, owner, ModalFields);
                return empty with { StrikeMs = Number(path, item, owner, "strikeMs"), Modes = ReadModes(path, item, owner) };
            case LayerKind.Noise:
                JsonShape.CheckNoUnknownMember(path, item, owner, NoiseFields);
                return empty with
                {
                    Color = (NoiseColor)Name(path, item, owner, "color", ColorNames),
                    Filter = (FilterKind)Name(path, item, owner, "filter", FilterNames),
                    CutoffStartHz = Number(path, item, owner, "cutoffStartHz"),
                    CutoffEndHz = Number(path, item, owner, "cutoffEndHz"),
                    Resonance = Number(path, item, owner, "resonance"),
                };
            default:
                ToneWave wave = (ToneWave)Name(path, item, owner, WaveKey, WaveNames);
                JsonShape.CheckNoUnknownMember(path, item, owner, wave == ToneWave.Fm ? FmToneFields : ToneFields);
                Layer tone = empty with
                {
                    Wave = wave,
                    FrequencyStartHz = Number(path, item, owner, "frequencyStartHz"),
                    FrequencyEndHz = Number(path, item, owner, "frequencyEndHz"),
                };
                return wave == ToneWave.Fm
                    ? tone with { FmRatio = Number(path, item, owner, "fmRatio"), FmIndex = Number(path, item, owner, "fmIndex") }
                    : tone;
        }
    }

    /// <summary>A layer of one kind with every field at zero, its first value, or an empty list.</summary>
    private static Layer Empty(LayerKind kind)
    {
        return new Layer(kind, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, [], NoiseColor.White, FilterKind.LowPass, 0.0, 0.0, 0.0, ToneWave.Sine, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0, [], string.Empty);
    }

    /// <summary>A spectral layer: the delay, the gain, the four controls of D-465, and the level frames.</summary>
    private static Layer ReadSpectral(string path, JsonElement item, string owner)
    {
        JsonShape.CheckNoUnknownMember(path, item, owner, SpectralFields);
        return Empty(LayerKind.Spectral) with
        {
            DelayMs = Number(path, item, owner, "delayMs"),
            Gain = Number(path, item, owner, GainKey),
            TrimMs = Number(path, item, owner, "trimMs"),
            FadeMs = Number(path, item, owner, "fadeMs"),
            TimeStretch = Number(path, item, owner, "timeStretch"),
            PitchSemitones = Number(path, item, owner, "pitchSemitones"),
            TiltDbPerOctave = Number(path, item, owner, "tiltDbPerOctave"),
            FrameSamples = ReadFrameSize(path, item, owner),
            Levels = ReadLevels(path, item, owner),
        };
    }

    /// <summary>A recording layer: the delay, the gain, the trim, the fade, and a WAV file under the recording directory.</summary>
    private static Layer ReadRecording(string path, JsonElement item, string owner)
    {
        JsonShape.CheckNoUnknownMember(path, item, owner, RecordingFields);
        string file = JsonShape.Text(path, item, owner, FileKey);
        bool placed = file.StartsWith(AssetPaths.RecordingDirectory, StringComparison.Ordinal) && file.EndsWith(AssetPaths.SoundExtension, StringComparison.Ordinal);
        if (!placed || file.Contains("..", StringComparison.Ordinal) || file.Contains('\\', StringComparison.Ordinal))
        {
            throw ContentError.Make(path, FileKey, $"on '{owner}' is '{file}', and a recording is a WAV file under '{AssetPaths.RecordingDirectory}'");
        }

        return Empty(LayerKind.Recording) with
        {
            DelayMs = Number(path, item, owner, "delayMs"),
            Gain = Number(path, item, owner, GainKey),
            TrimMs = Number(path, item, owner, "trimMs"),
            FadeMs = Number(path, item, owner, "fadeMs"),
            File = file,
        };
    }

    /// <summary>The frame size of a spectral layer: one of the sizes of <see cref="SpectralLayer.FrameSizes"/>.</summary>
    private static int ReadFrameSize(string path, JsonElement item, string owner)
    {
        JsonElement value = JsonShape.Member(path, item, owner, FrameKey);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int size) || Array.IndexOf(SpectralLayer.FrameSizes, size) < 0)
        {
            throw ContentError.Make(path, FrameKey, $"on '{owner}' is not one of the frame sizes 256, 512, and 1024");
        }

        return size;
    }

    /// <summary>The level frames of a spectral layer: 1 to 4096 lists, each of one whole number for each band.</summary>
    private static IReadOnlyList<int[]> ReadLevels(string path, JsonElement item, string owner)
    {
        JsonElement list = JsonShape.Member(path, item, owner, LevelsKey);
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() < 1 || list.GetArrayLength() > MaxFrames)
        {
            throw ContentError.Make(path, LevelsKey, $"on '{owner}' is not a list of 1 to {Text(MaxFrames)} frames");
        }

        List<int[]> frames = [];
        foreach (JsonElement frame in list.EnumerateArray())
        {
            string place = $"frame {Text(frames.Count)} of '{owner}'";
            if (frame.ValueKind != JsonValueKind.Array || frame.GetArrayLength() != SpectralLayer.BandCount)
            {
                throw ContentError.Make(path, LevelsKey, $"holds a {place} that is not a list of {Text(SpectralLayer.BandCount)} levels");
            }

            int[] levels = new int[SpectralLayer.BandCount];
            int band = 0;
            foreach (JsonElement level in frame.EnumerateArray())
            {
                if (level.ValueKind != JsonValueKind.Number || !level.TryGetInt32(out int value) || value < SpectralLayer.FloorDecibels || value > SpectralLayer.CeilingDecibels)
                {
                    throw ContentError.Make(path, LevelsKey, $"holds a level at band {Text(band)} of {place} that is not a whole number from {Text(SpectralLayer.FloorDecibels)} to {Text(SpectralLayer.CeilingDecibels)}");
                }

                levels[band] = value;
                band++;
            }

            frames.Add(levels);
        }

        return frames;
    }

    /// <summary>The modes of a modal layer: a list of 1 to 8 objects.</summary>
    private static IReadOnlyList<Mode> ReadModes(string path, JsonElement item, string owner)
    {
        JsonElement list = JsonShape.Member(path, item, owner, ModesKey);
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() < 1 || list.GetArrayLength() > MaxModes)
        {
            throw ContentError.Make(path, ModesKey, $"on '{owner}' is not a list of 1 to {Text(MaxModes)} modes");
        }

        List<Mode> modes = [];
        foreach (JsonElement mode in list.EnumerateArray())
        {
            string place = $"{owner}.{ModesKey}[{Text(modes.Count)}]";
            JsonShape.CheckNoUnknownMember(path, mode, place, ModeFields);
            modes.Add(new Mode(Number(path, mode, place, "frequencyHz"), Number(path, mode, place, "decayMs"), Number(path, mode, place, "amplitude")));
        }

        return modes;
    }

    /// <summary>A number member inside the range of its name.</summary>
    private static double Number(string path, JsonElement owner, string ownerName, string name)
    {
        FieldRange range = Ranges[name];
        JsonElement value = JsonShape.Member(path, owner, ownerName, name);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out double number) || !double.IsFinite(number) || number < range.Minimum || number > range.Maximum)
        {
            throw ContentError.Make(path, name, $"on '{ownerName}' is not a number from {Text(range.Minimum)} to {Text(range.Maximum)}");
        }

        return number;
    }

    /// <summary>A text member that is one of the names of its field, as the index of the name.</summary>
    private static int Name(string path, JsonElement owner, string ownerName, string name, string[] names)
    {
        string text = JsonShape.Text(path, owner, ownerName, name);
        int index = Array.IndexOf(names, text);
        if (index < 0)
        {
            throw ContentError.Make(path, name, $"on '{ownerName}' is '{text}', and the names are {string.Join(", ", names)}");
        }

        return index;
    }

    /// <summary>The seed member: a whole number that 32 bits hold, and not zero.</summary>
    private static uint ReadSeed(string path, JsonElement root)
    {
        JsonElement value = JsonShape.Member(path, root, TextureJson.RootName, SeedKey);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetUInt32(out uint seed) || seed == 0)
        {
            throw ContentError.Make(path, SeedKey, "is not a whole number from 1 to 4294967295, and a zero seed never leaves zero");
        }

        return seed;
    }

    private static string Text(double number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
