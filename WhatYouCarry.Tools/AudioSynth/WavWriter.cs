using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace WhatYouCarry.Tools.AudioSynth;

/// <summary>
/// Writes a WAV file of 16-bit mono samples at the sample rate of the synthesizer (D-453, D-462): the RIFF header,
/// the format chunk, and the data chunk, with each value in little-endian order.
/// </summary>
/// <remarks>
/// A sample from minus 1 to 1 becomes a whole number from minus 32767 to 32767, with the fraction cut toward zero.
/// The file then has one byte form on every platform, so the committed sound can equal the synthesizer output.
/// </remarks>
public static class WavWriter
{
    /// <summary>The bytes of the header before the samples.</summary>
    public const int HeaderLength = 44;

    /// <summary>The largest magnitude of a 16-bit sample.</summary>
    public const int FullScale = 32767;

    private const short PcmFormat = 1;
    private const short Channels = 1;
    private const short BitsPerSample = 16;
    private const short BytesPerSample = BitsPerSample / 8;

    /// <summary>The file of one list of samples.</summary>
    /// <exception cref="ArgumentException">The list is empty, or a sample is not a number from minus 1 to 1.</exception>
    public static byte[] Write(float[] samples)
    {
        if (samples.Length == 0)
        {
            throw new ArgumentException("A WAV file holds one sample or more, and the list is empty.", nameof(samples));
        }

        List<byte> file = new(HeaderLength + (samples.Length * BytesPerSample));
        int dataLength = samples.Length * BytesPerSample;
        AddText(file, "RIFF");
        AddInt(file, HeaderLength - 8 + dataLength);
        AddText(file, "WAVE");
        AddText(file, "fmt ");
        AddInt(file, 16);
        AddShort(file, PcmFormat);
        AddShort(file, Channels);
        AddInt(file, LayeredSynthesizer.SampleRate);
        AddInt(file, LayeredSynthesizer.SampleRate * BytesPerSample * Channels);
        AddShort(file, BytesPerSample * Channels);
        AddShort(file, BitsPerSample);
        AddText(file, "data");
        AddInt(file, dataLength);
        for (int index = 0; index < samples.Length; index++)
        {
            float sample = samples[index];
            if (!float.IsFinite(sample) || sample < -1.0f || sample > 1.0f)
            {
                throw new ArgumentException($"The sample {index.ToString(CultureInfo.InvariantCulture)} is {sample.ToString(CultureInfo.InvariantCulture)}, and a sample is a number from -1 to 1.", nameof(samples));
            }

            AddShort(file, (short)(sample * FullScale));
        }

        return [.. file];
    }

    private static void AddText(List<byte> file, string text)
    {
        file.AddRange(Encoding.ASCII.GetBytes(text));
    }

    private static void AddInt(List<byte> file, int value)
    {
        file.Add((byte)value);
        file.Add((byte)(value >> 8));
        file.Add((byte)(value >> 16));
        file.Add((byte)(value >> 24));
    }

    private static void AddShort(List<byte> file, short value)
    {
        file.Add((byte)value);
        file.Add((byte)(value >> 8));
    }
}
