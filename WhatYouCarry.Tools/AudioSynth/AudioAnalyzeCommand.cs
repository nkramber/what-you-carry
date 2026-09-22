using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.AudioSynth;

/// <summary>The result of one analysis: the new sound file, the count of frames, and the resynthesis error in decibels.</summary>
public sealed record AnalysisResult(byte[] Definition, int Frames, double ErrorDecibels);

/// <summary>
/// <c>audio-analyze --root &lt;checkout&gt; --sound &lt;name&gt; [--part &lt;n&gt;] [--frame &lt;samples&gt;]</c>: the
/// analysis of one reference into the levels of a spectral layer (D-461, D-464, D-466). Part 1, the default, reads
/// <c>&lt;name&gt;.wav</c> under the reference directory and writes the first spectral layer of
/// <c>audio/sfx/&lt;name&gt;.json</c>. Part n reads <c>&lt;name&gt;-&lt;n&gt;.wav</c> and writes the n-th spectral layer,
/// so one sound can mix two references. The controls of the layer stay as they stand, and the frame option sets its frame
/// size first. A file that does not exist yet, or a file with one spectral layer fewer than the part, gains a spectral
/// layer with neutral controls and the long frame. Run <c>audio-synth</c> after it (D-453).
/// </summary>
/// <remarks>
/// The command reports the resynthesis error: the render of the levels with neutral controls is analysed again, and the
/// error is the mean difference of the two sets of levels over each band that is above the floor in either set, after
/// the mean offset of the two sets comes off. The gain of the sound sets the level, so a constant offset is no error.
/// </remarks>
public static class AudioAnalyzeCommand
{
    /// <summary>The directory of the reference recordings, relative to the checkout root. The game never ships it (D-461).</summary>
    public const string ReferenceDirectory = "WhatYouCarry.Tools/AudioSynth/References/";

    /// <summary>The seed of a new sound file.</summary>
    public const uint NewSeed = 1;

    /// <summary>The peak of a new sound file.</summary>
    public const double NewGain = 0.5;

    /// <summary>The fade of a new spectral layer, in milliseconds.</summary>
    public const double NewFadeMs = 10.0;

    /// <summary>The message of a sound name that is not one file name.</summary>
    public const string BadSoundName = "The sound name must be one file name: no directory, no '..', and no separator.";

    /// <summary>The whole run as an exit code: 0 when the sound file is written, 1 on a bad file or a write failure, 2 on a bad command line.</summary>
    public static int Run(string[] args)
    {
        string? root = null;
        string? sound = null;
        int? frame = null;
        int part = 1;
        for (int i = 0; i < args.Length; i++)
        {
            bool hasValue = i + 1 < args.Length;
            if (args[i] == "--root" && hasValue)
            {
                root = args[++i];
            }
            else if (args[i] == "--sound" && hasValue)
            {
                sound = args[++i];
            }
            else if (args[i] == "--part" && hasValue && int.TryParse(args[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out int number) && number >= 1 && number <= SoundDefinition.MaxLayers)
            {
                part = number;
                i++;
            }
            else if (args[i] == "--frame" && hasValue && int.TryParse(args[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out int size) && Array.IndexOf(SpectralLayer.FrameSizes, size) >= 0)
            {
                frame = size;
                i++;
            }
            else
            {
                Console.Error.WriteLine($"Unexpected argument '{args[i]}'. The options are --root <checkout>, --sound <name>, --part <1 to 4>, and --frame <256, 512, or 1024>.");
                return 2;
            }
        }

        if (root is null || sound is null)
        {
            Console.Error.WriteLine("The options are required: --root <checkout> and --sound <name>.");
            return 2;
        }

        if (!IsOneName(sound))
        {
            Console.Error.WriteLine($"{BadSoundName} The name is '{sound}'.");
            return 2;
        }

        string parameterPath = AssetPaths.SoundDirectory + sound + AssetPaths.SoundParameterExtension;
        string parameterFile = Path.Combine(root, AudioSynthCommand.ContentDirectoryName, parameterPath);
        string referenceName = part == 1 ? sound : sound + "-" + part.ToString(CultureInfo.InvariantCulture);
        string referenceFile = Path.Combine(root, ReferenceDirectory, referenceName + AssetPaths.SoundExtension);
        AnalysisResult result;
        try
        {
            float[] reference = WavReader.Read(referenceFile, ReadFile(referenceFile));
            byte[]? existing = File.Exists(parameterFile) ? ReadFile(parameterFile) : null;
            result = Analyze(parameterPath, existing, reference, part, frame);
            File.WriteAllBytes(parameterFile, result.Definition);
        }
        catch (ContextException error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            // A path that the user cannot write raises the second kind, and it is not an IOException.
            Console.Error.WriteLine($"The sound file could not be written to '{parameterFile}'. {error.Message}");
            return 1;
        }

        string frames = result.Frames.ToString(CultureInfo.InvariantCulture);
        string decibels = result.ErrorDecibels.ToString("F2", CultureInfo.InvariantCulture);
        Console.WriteLine($"audio-analyze: {parameterPath}, {frames} frames, resynthesis error {decibels} dB. Run audio-synth next.");
        return 0;
    }

    /// <summary>The sound file with the levels of one reference in its first spectral layer, or a new file of one spectral layer.</summary>
    /// <param name="parameterPath">The content path of the sound file, for errors.</param>
    /// <param name="existing">The bytes of the existing sound file, or null for a new one.</param>
    /// <param name="reference">The samples of the reference.</param>
    /// <param name="part">The spectral layer to write, from 1.</param>
    /// <param name="frame">The frame size to set on the layer, or null to keep the size of the file.</param>
    /// <exception cref="ContextException">The existing file is not valid, it has fewer spectral layers than the part less one, or it holds four layers already.</exception>
    public static AnalysisResult Analyze(string parameterPath, byte[]? existing, float[] reference, int part, int? frame)
    {
        JsonNode root = existing is null ? NewFile() : ExistingFile(parameterPath, existing);
        JsonObject spectral = SpectralPart(parameterPath, root, part, reference.Length);
        if (frame is int size)
        {
            spectral[SoundDefinition.FrameKey] = size;
        }

        int frameSize = spectral[SoundDefinition.FrameKey]?.GetValue<int>() ?? SpectralLayer.DefaultFrameSize;
        int[][] levels = SpectralLayer.Analyze(reference, frameSize);
        spectral[SoundDefinition.LevelsKey] = new JsonArray([.. levels.Select(values => (JsonNode)new JsonArray([.. values.Select(level => (JsonNode)level)]))]);
        byte[] definition = SoundFileWriter.Write(root);
        SoundDefinition.Parse(parameterPath, definition);
        return new AnalysisResult(definition, levels.Length, ResynthesisError(levels, frameSize));
    }

    /// <summary>The mean difference in decibels of the levels and the levels of their neutral render, over each band above the floor in either.</summary>
    private static double ResynthesisError(int[][] levels, int frameSize)
    {
        Layer neutral = new(LayerKind.Spectral, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, [], NoiseColor.White, FilterKind.LowPass, 0.0, 0.0, 0.0, ToneWave.Sine, 0.0, 0.0, 0.0, 0.0, 12000.0, 0.0, 1.0, 0.0, 0.0, frameSize, levels, string.Empty);
        double[] render = SpectralLayer.Render(neutral, NewSeed);
        int[][] again = SpectralLayer.Analyze([.. render.Select(sample => (float)sample)], frameSize);
        System.Collections.Generic.List<int> differences = [];
        for (int frame = 0; frame < levels.Length; frame++)
        {
            for (int band = 0; band < SpectralLayer.BandCount; band++)
            {
                int a = levels[frame][band];
                int b = frame < again.Length ? again[frame][band] : SpectralLayer.FloorDecibels;
                if (a > SpectralLayer.FloorDecibels || b > SpectralLayer.FloorDecibels)
                {
                    differences.Add(b - a);
                }
            }
        }

        if (differences.Count == 0)
        {
            return 0.0;
        }

        double offset = differences.Average();
        return differences.Average(difference => Math.Abs(difference - offset));
    }

    /// <summary>A new sound file with no layer yet.</summary>
    private static JsonNode NewFile()
    {
        return new JsonObject
        {
            [SoundDefinition.SeedKey] = NewSeed,
            [SoundDefinition.GainKey] = NewGain,
            [SoundDefinition.LayersKey] = new JsonArray(),
        };
    }

    /// <summary>A spectral layer with neutral controls, the long frame, and a trim to the length of the reference.</summary>
    private static JsonObject NewSpectral(int referenceSamples)
    {
        return new JsonObject
        {
            [SoundDefinition.KindKey] = "spectral",
            ["delayMs"] = 0.0,
            [SoundDefinition.GainKey] = 1.0,
            ["trimMs"] = Math.Ceiling(referenceSamples * 1000.0 / LayeredSynthesizer.SampleRate),
            ["fadeMs"] = NewFadeMs,
            ["timeStretch"] = 1.0,
            ["pitchSemitones"] = 0.0,
            ["tiltDbPerOctave"] = 0.0,
            [SoundDefinition.FrameKey] = SpectralLayer.DefaultFrameSize,
            [SoundDefinition.LevelsKey] = new JsonArray(),
        };
    }

    /// <summary>The JSON of an existing sound file, after it passes the parse.</summary>
    private static JsonNode ExistingFile(string parameterPath, byte[] existing)
    {
        SoundDefinition.Parse(parameterPath, existing);
        return JsonNode.Parse(existing) ?? throw new ContextException($"The sound file '{parameterPath}' holds no JSON value.");
    }

    /// <summary>The spectral layer of one part, from 1. The file gains a new spectral layer for the part after its last one.</summary>
    /// <exception cref="ContextException">The file has fewer spectral layers than the part less one, or it holds four layers already (T-2).</exception>
    private static JsonObject SpectralPart(string parameterPath, JsonNode root, int part, int referenceSamples)
    {
        JsonArray layers = root[SoundDefinition.LayersKey]?.AsArray() ?? throw new ContextException($"The sound file '{parameterPath}' has no list of layers.");
        int found = 0;
        foreach (JsonNode? layer in layers)
        {
            if (layer is JsonObject owner && owner[SoundDefinition.KindKey]?.GetValue<string>() == "spectral")
            {
                found++;
                if (found == part)
                {
                    return owner;
                }
            }
        }

        if (found != part - 1)
        {
            string counts = $"{found.ToString(CultureInfo.InvariantCulture)} spectral layers, and part {part.ToString(CultureInfo.InvariantCulture)} needs {(part - 1).ToString(CultureInfo.InvariantCulture)} first";
            throw new ContextException($"The sound file '{parameterPath}' holds {counts}. Analyse the parts in order (D-466).");
        }

        if (layers.Count >= SoundDefinition.MaxLayers)
        {
            throw new ContextException($"The sound file '{parameterPath}' holds {SoundDefinition.MaxLayers.ToString(CultureInfo.InvariantCulture)} layers already, and part {part.ToString(CultureInfo.InvariantCulture)} needs a new one (D-462).");
        }

        JsonObject added = NewSpectral(referenceSamples);
        layers.Add(added);
        return added;
    }

    /// <summary>
    /// Answers whether a sound name is one file name. A name with a directory, a parent step, or a separator writes
    /// outside the sound directory, so the command takes none of them (PR #87 review P2-1).
    /// </summary>
    public static bool IsOneName(string sound)
    {
        return sound.Length > 0
            && sound != "."
            && sound != ".."
            && sound.IndexOfAny(['/', '\\', ':']) < 0
            && sound.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
            && Path.GetFileName(sound) == sound;
    }

    /// <summary>The bytes of one file. An absent file, and a file that the user cannot read, are each an error that names the path (T-2).</summary>
    private static byte[] ReadFile(string file)
    {
        if (!File.Exists(file))
        {
            throw new ContextException($"The file '{file}' does not exist.");
        }

        try
        {
            return File.ReadAllBytes(file);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            // A file that the user cannot read raises the second kind, and it is not an IOException.
            throw new ContextException($"The file '{file}' could not be read. {error.Message}", error);
        }
    }
}
