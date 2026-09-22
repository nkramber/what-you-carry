using System;
using System.Globalization;
using System.Text;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.AudioSynth;

/// <summary>
/// Reads a reference recording (D-461): a WAV file of 16-bit PCM at the sample rate of the synthesizer, mono or stereo.
/// A stereo file becomes the mean of its two channels. Any other form is an error that names the file and the field.
/// </summary>
/// <remarks>
/// The format chunk can name PCM directly, or name the extensible format with a PCM subformat, which the converter of
/// macOS writes. The first two bytes of the subformat identifier name the format.
/// </remarks>
public static class WavReader
{
    private const int FormatChunkLength = 16;
    private const int ExtensibleChunkLength = 40;
    private const short PcmFormat = 1;
    private const short ExtensibleFormat = unchecked((short)0xFFFE);

    /// <summary>The samples of one file, from minus 1 to 1.</summary>
    /// <exception cref="ContextException">The file is not a RIFF WAVE file, it has no format or data chunk, or its format is not 16-bit PCM at 44.1 kHz with one or two channels.</exception>
    public static float[] Read(string path, byte[] bytes)
    {
        if (bytes.Length < 12 || Tag(bytes, 0) != "RIFF" || Tag(bytes, 8) != "WAVE")
        {
            throw Error(path, "header", "is not a RIFF WAVE header");
        }

        int channels = 0;
        int offset = 12;
        while (offset + 8 <= bytes.Length)
        {
            string tag = Tag(bytes, offset);
            int length = BitConverter.ToInt32(bytes, offset + 4);
            int body = offset + 8;

            // The remaining count comes off the length, because body plus length passes the end of an int and turns
            // negative on a length near the top of the range (PR #87 review P2-2).
            if (length < 0 || length > bytes.Length - body)
            {
                throw Error(path, tag, "has a length past the end of the file");
            }

            if (tag == "fmt ")
            {
                channels = ReadFormat(path, bytes, body, length);
            }
            else if (tag == "data")
            {
                if (channels == 0)
                {
                    throw Error(path, "data", "comes before the format chunk");
                }

                return Samples(bytes, body, length, channels);
            }

            offset = body + length + (length % 2);
        }

        throw Error(path, "data", "is absent");
    }

    /// <summary>The count of channels of a format chunk that holds 16-bit PCM at 44.1 kHz.</summary>
    private static int ReadFormat(string path, byte[] bytes, int body, int length)
    {
        if (length < FormatChunkLength)
        {
            throw Error(path, "fmt ", "is shorter than 16 bytes");
        }

        short format = BitConverter.ToInt16(bytes, body);
        short channels = BitConverter.ToInt16(bytes, body + 2);
        int rate = BitConverter.ToInt32(bytes, body + 4);
        short bits = BitConverter.ToInt16(bytes, body + 14);
        if (format == ExtensibleFormat && length >= ExtensibleChunkLength)
        {
            format = BitConverter.ToInt16(bytes, body + 24);
        }

        if (format != PcmFormat || bits != 16 || rate != LayeredSynthesizer.SampleRate || channels < 1 || channels > 2)
        {
            string found = $"format {Text(format)}, {Text(bits)} bits, {Text(rate)} Hz, {Text(channels)} channels";
            throw Error(path, "fmt ", $"holds {found}, and a reference is 16-bit PCM at 44100 Hz with 1 or 2 channels");
        }

        return channels;
    }

    /// <summary>The samples of the data chunk, with the channels of each frame mixed to their mean.</summary>
    private static float[] Samples(byte[] bytes, int body, int length, int channels)
    {
        int frames = length / (2 * channels);
        float[] samples = new float[frames];
        for (int frame = 0; frame < frames; frame++)
        {
            double sum = 0.0;
            for (int channel = 0; channel < channels; channel++)
            {
                sum += BitConverter.ToInt16(bytes, body + (((frame * channels) + channel) * 2)) / 32768.0;
            }

            samples[frame] = (float)(sum / channels);
        }

        return samples;
    }

    private static string Tag(byte[] bytes, int offset)
    {
        return Encoding.ASCII.GetString(bytes, offset, 4);
    }

    private static ContextException Error(string path, string chunk, string reason)
    {
        ContextException error = new($"The reference file '{path}' is not valid: the chunk '{chunk}' {reason}.");
        error.AddContext("file", path);
        error.AddContext("chunk", chunk);
        return error;
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
