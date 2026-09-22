using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.AudioSynth;

/// <summary>One rendered sound: the content path of its parameter file, the content path of its WAV file, and the bytes of the WAV file.</summary>
public sealed record RenderedSound(string ParameterPath, string SoundPath, byte[] Bytes);

/// <summary>
/// <c>audio-synth --root &lt;checkout&gt;</c>: the sound synthesizer (D-65, D-93, D-462). It reads every parameter file
/// under <c>audio/sfx/</c>, renders each one, and writes the WAV file next to it. A test holds each committed WAV file
/// equal to the output, so run the command after each change to a parameter file (D-453).
/// </summary>
public static class AudioSynthCommand
{
    /// <summary>The content directory under the checkout root.</summary>
    public const string ContentDirectoryName = "content";

    /// <summary>The file pattern of a parameter file.</summary>
    public const string ParameterPattern = "*" + AssetPaths.SoundParameterExtension;

    /// <summary>The whole run as an exit code: 0 when every sound is written, 1 on a parameter file that is bad or unreadable, or on a write failure, 2 on a bad command line.</summary>
    public static int Run(string[] args)
    {
        string? root = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--root" && i + 1 < args.Length)
            {
                root = args[i + 1];
                i++;
                continue;
            }

            Console.Error.WriteLine($"Unexpected argument '{args[i]}'. The only option is --root <checkout>, and every argument must belong to it.");
            return 2;
        }

        if (root is null)
        {
            Console.Error.WriteLine("The option is required: --root <checkout>.");
            return 2;
        }

        string contentRoot = Path.Combine(root, ContentDirectoryName);
        IReadOnlyList<RenderedSound> sounds;
        try
        {
            sounds = RenderAll(contentRoot);
        }
        catch (ContextException error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }

        foreach (RenderedSound sound in sounds)
        {
            string output = Path.Combine(contentRoot, sound.SoundPath);
            try
            {
                File.WriteAllBytes(output, sound.Bytes);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                // A path that the user cannot write raises the second kind, and it is not an IOException.
                Console.Error.WriteLine($"The sound could not be written to '{output}'. {error.Message}");
                return 1;
            }

            Console.WriteLine($"audio-synth: wrote {sound.SoundPath}, {sound.Bytes.Length.ToString(CultureInfo.InvariantCulture)} bytes.");
        }

        return 0;
    }

    /// <summary>The WAV file of one parameter file, with the recordings that its recording layers name, read from the content directory.</summary>
    /// <exception cref="ContextException">The parameter file is not valid, or a recording is absent, unreadable, or not valid.</exception>
    public static byte[] Render(string contentRoot, string parameterPath, byte[] parameterBytes)
    {
        SoundDefinition definition = SoundDefinition.Parse(parameterPath, parameterBytes);
        Dictionary<string, float[]> recordings = [];
        foreach (Layer layer in definition.Layers)
        {
            if (layer.Kind == LayerKind.Recording && !recordings.ContainsKey(layer.File))
            {
                string file = Path.Combine(contentRoot, layer.File);
                if (!File.Exists(file))
                {
                    throw new ContextException($"The sound file '{parameterPath}' names the recording '{layer.File}', and the file '{file}' does not exist (D-467).");
                }

                recordings.Add(layer.File, WavReader.Read(layer.File, File.ReadAllBytes(file)));
            }
        }

        return WavWriter.Write(LayeredSynthesizer.Render(definition, recordings));
    }

    /// <summary>Every parameter file of the sound directory, rendered, in ordinal order of the file name.</summary>
    /// <exception cref="ContextException">The directory is absent, the user cannot read it or a file in it, it holds no parameter file, or a file is not valid.</exception>
    public static IReadOnlyList<RenderedSound> RenderAll(string contentRoot)
    {
        string directory = Path.Combine(contentRoot, AssetPaths.SoundDirectory);
        if (!Directory.Exists(directory))
        {
            throw new ContextException($"The sound directory '{directory}' does not exist.");
        }

        try
        {
            string[] files = Directory.GetFiles(directory, ParameterPattern);
            Array.Sort(files, StringComparer.Ordinal);
            if (files.Length == 0)
            {
                throw new ContextException($"The sound directory '{directory}' holds no parameter file, and the game needs the sounds of D-454.");
            }

            List<RenderedSound> sounds = [];
            foreach (string file in files)
            {
                string parameterPath = AssetPaths.SoundDirectory + Path.GetFileName(file);
                sounds.Add(new RenderedSound(parameterPath, AssetPaths.SoundPath(parameterPath), Render(contentRoot, parameterPath, File.ReadAllBytes(file))));
            }

            return sounds;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            // A directory or a file that the user cannot read raises the second kind, and it is not an IOException.
            throw new ContextException($"The sound directory '{directory}' or a file in it could not be read. {error.Message}", error);
        }
    }
}
