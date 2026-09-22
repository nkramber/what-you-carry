using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Determinism;

namespace WhatYouCarry.Tools.AudioSynth;

/// <summary>
/// The spectral layer (D-464, D-465): the analysis of a reference recording into band levels, and the render of the
/// levels as noise with random phases, with the trim, the fade, the time stretch, the pitch shift, and the tilt.
/// </summary>
/// <remarks>
/// <para>
/// A frame is 256, 512, or 1024 samples under a Hann window, a choice of each layer, and one frame starts every eighth
/// of a frame. A long frame keeps the low frequencies of a thump, and a short frame keeps the crackle of cloth sharp. The
/// 64 bands divide 40 Hz to 20 kHz on a log scale, and each band is one transform bin wide or more.
/// A level is the root mean square magnitude of the bins of its band, in whole decibels, from -100 to 80. The floor,
/// -100, is silence.
/// </para>
/// <para>
/// The render makes each frame from the levels: every bin takes the magnitude of its band and a random phase, and the
/// inverse transform, the window, and the overlap-add make the samples. The pitch shift reads each bin from the band of
/// its frequency over the pitch ratio. The tilt adds its decibels for each octave above or below 1 kHz. The time stretch
/// reads the frames at a fractional place, and a level between two frames follows a straight line in decibels.
/// </para>
/// <para>
/// The render gives equal bytes on every platform (D-453): it uses the exponential and the logarithm of
/// <see cref="DetDouble"/>, the sine and the cosine of <see cref="DetMath"/>, and a xorshift sequence from the seed.
/// </para>
/// </remarks>
public static class SpectralLayer
{
    /// <summary>The frame sizes that a layer can choose, in samples.</summary>
    public static readonly int[] FrameSizes = [256, 512, 1024];

    /// <summary>The frame size of a new layer: the long frame, for the low frequencies.</summary>
    public const int DefaultFrameSize = 1024;

    /// <summary>The count of hops in one frame: one frame starts every eighth of a frame.</summary>
    public const int HopsPerFrame = 8;

    /// <summary>The count of bands.</summary>
    public const int BandCount = 64;

    /// <summary>The level of silence, in decibels.</summary>
    public const int FloorDecibels = -100;

    /// <summary>The highest level, in decibels.</summary>
    public const int CeilingDecibels = 80;

    private const double LowestHz = 40.0;
    private const double HighestHz = 20000.0;
    private const double DecibelsPerNeper = 8.6858896380650365530;
    private const double TiltCenterHz = 1000.0;
    private const double TwoPi = 2.0 * Math.PI;

    /// <summary>The band levels of each frame of a recording, without the frames at the end that are silent in every band.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame size is not one of <see cref="FrameSizes"/>.</exception>
    public static int[][] Analyze(float[] samples, int frameSize)
    {
        Frame shape = Frame.Of(frameSize);
        int windowSize = shape.Size;
        int hopSize = windowSize / HopsPerFrame;
        int frames = ((samples.Length + hopSize - 1) / hopSize) + 1;
        List<int[]> levels = [];
        double[] real = new double[windowSize];
        double[] imaginary = new double[windowSize];
        for (int frame = 0; frame < frames; frame++)
        {
            int start = (frame * hopSize) - (windowSize / 2);
            for (int index = 0; index < windowSize; index++)
            {
                int position = start + index;
                real[index] = position >= 0 && position < samples.Length ? samples[position] * shape.Window[index] : 0.0;
                imaginary[index] = 0.0;
            }

            shape.Transform.Transform(real, imaginary);
            levels.Add(BandLevels(shape, real, imaginary));
        }

        while (levels.Count > 1 && Array.TrueForAll(levels[^1], level => level == FloorDecibels))
        {
            levels.RemoveAt(levels.Count - 1);
        }

        return [.. levels];
    }

    /// <summary>The samples of one spectral layer before its gain, after the trim and the fade.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame size of the layer is not one of <see cref="FrameSizes"/>.</exception>
    public static double[] Render(Layer layer, uint seed)
    {
        Frame shape = Frame.Of(layer.FrameSamples);
        int windowSize = shape.Size;
        int hopSize = windowSize / HopsPerFrame;
        IReadOnlyList<int[]> levels = layer.Levels;
        int outputFrames = (int)Math.Ceiling(levels.Count * layer.TimeStretch);
        int natural = outputFrames * hopSize;
        int length = Math.Max(Math.Min(LayeredSynthesizer.Samples(layer.TrimMs), natural), 1);
        double[] sum = new double[natural + windowSize];
        double[] weight = new double[natural + windowSize];
        int[] bandOfBin = BandOfEachBin(shape, DetDouble.Exp(layer.PitchSemitones / 12.0 * DetDouble.Ln2));
        double[] tilt = TiltOfEachBin(shape, layer.TiltDbPerOctave);
        double[] real = new double[windowSize];
        double[] imaginary = new double[windowSize];
        uint state = seed;
        for (int frame = 0; frame < outputFrames; frame++)
        {
            double source = frame / layer.TimeStretch;
            int first = Math.Min((int)Math.Floor(source), levels.Count - 1);
            int second = Math.Min(first + 1, levels.Count - 1);
            double between = source - first;
            Array.Clear(real);
            Array.Clear(imaginary);
            for (int bin = 1; bin < windowSize / 2; bin++)
            {
                int band = bandOfBin[bin];
                if (band < 0)
                {
                    continue;
                }

                int a = levels[first][band];
                int b = levels[second][band];
                if (a == FloorDecibels && b == FloorDecibels)
                {
                    continue;
                }

                double decibels = a + ((b - a) * between) + tilt[bin];
                double magnitude = DetDouble.Exp(decibels / DecibelsPerNeper);
                double phase = NextUnit(ref state) * TwoPi;
                real[bin] = magnitude * DetMath.Cos((float)phase);
                imaginary[bin] = magnitude * DetMath.Sin((float)phase);
                real[windowSize - bin] = real[bin];
                imaginary[windowSize - bin] = -imaginary[bin];
            }

            AddFrame(shape, sum, weight, real, imaginary, frame * hopSize);
        }

        return TrimAndFade(sum, weight, length, LayeredSynthesizer.Samples(layer.FadeMs));
    }

    /// <summary>Adds the inverse transform of one frame under the window, centered on a sample, to the sums.</summary>
    private static void AddFrame(Frame shape, double[] sum, double[] weight, double[] real, double[] imaginary, int center)
    {
        int windowSize = shape.Size;

        // The inverse transform is the forward transform of the conjugate, conjugated, over the size. The real part alone stays.
        for (int index = 0; index < windowSize; index++)
        {
            imaginary[index] = -imaginary[index];
        }

        shape.Transform.Transform(real, imaginary);
        int start = center - (windowSize / 2);
        for (int index = 0; index < windowSize; index++)
        {
            int position = start + index;
            if (position < 0 || position >= sum.Length)
            {
                continue;
            }

            sum[position] += real[index] / windowSize * shape.Window[index];
            weight[position] += shape.Window[index] * shape.Window[index];
        }
    }

    /// <summary>The overlap-add over its weights, to a length, with a fade in a straight line over the last samples.</summary>
    private static double[] TrimAndFade(double[] sum, double[] weight, int length, int fade)
    {
        double[] samples = new double[length];
        for (int index = 0; index < length; index++)
        {
            double value = weight[index] > 1e-9 ? sum[index] / weight[index] : 0.0;
            int fromEnd = length - index;
            samples[index] = fade > 0 && fromEnd <= fade ? value * (fromEnd - 1) / fade : value;
        }

        return samples;
    }

    /// <summary>The band of the frequency of each bin over the pitch ratio, or -1 outside the bands.</summary>
    private static int[] BandOfEachBin(Frame shape, double pitchRatio)
    {
        int[] bands = new int[shape.Size / 2];
        for (int bin = 0; bin < bands.Length; bin++)
        {
            double sourceBin = bin / pitchRatio;
            bands[bin] = -1;
            for (int band = 0; band < BandCount; band++)
            {
                if (sourceBin >= shape.Edges[band] && sourceBin < shape.Edges[band + 1])
                {
                    bands[bin] = band;
                    break;
                }
            }
        }

        return bands;
    }

    /// <summary>The decibels of the tilt at each bin: the tilt times the octaves from 1 kHz.</summary>
    private static double[] TiltOfEachBin(Frame shape, double tiltPerOctave)
    {
        double[] tilt = new double[shape.Size / 2];
        for (int bin = 1; bin < tilt.Length; bin++)
        {
            double hz = bin * (double)LayeredSynthesizer.SampleRate / shape.Size;
            tilt[bin] = tiltPerOctave * DetDouble.Log(hz / TiltCenterHz) / DetDouble.Ln2;
        }

        return tilt;
    }

    /// <summary>The level of each band of one transform, in whole decibels from the floor to the ceiling.</summary>
    private static int[] BandLevels(Frame shape, double[] real, double[] imaginary)
    {
        int[] levels = new int[BandCount];
        for (int band = 0; band < BandCount; band++)
        {
            double energy = 0.0;
            for (int bin = shape.Edges[band]; bin < shape.Edges[band + 1]; bin++)
            {
                energy += (real[bin] * real[bin]) + (imaginary[bin] * imaginary[bin]);
            }

            double meanSquare = energy / (shape.Edges[band + 1] - shape.Edges[band]);
            double decibels = meanSquare > 0.0 ? DecibelsPerNeper / 2.0 * DetDouble.Log(meanSquare) : FloorDecibels;
            levels[band] = (int)Math.Clamp(Math.Round(decibels, MidpointRounding.ToEven), FloorDecibels, CeilingDecibels);
        }

        return levels;
    }

    /// <summary>A value from 0 up to 1 from one step of the xorshift sequence of 32 bits, with the shifts 13, 17, and 5.</summary>
    private static double NextUnit(ref uint state)
    {
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;
        return state / 4294967296.0;
    }

    /// <summary>One frame size: its transform, its Hann window, and the first bin of each band. The three sizes are made once.</summary>
    private sealed class Frame
    {
        private static readonly Frame[] Sizes = [new(256), new(512), new(1024)];

        private Frame(int size)
        {
            this.Size = size;
            this.Transform = new Fft(size);
            this.Window = new double[size];
            for (int index = 0; index < size; index++)
            {
                this.Window[index] = 0.5 - (0.5 * DetMath.Cos((float)(TwoPi * index / size)));
            }

            this.Edges = BandEdges(size);
        }

        public int Size { get; }

        public Fft Transform { get; }

        public double[] Window { get; }

        public int[] Edges { get; }

        /// <summary>The frame of one size.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The size is not one of <see cref="FrameSizes"/>.</exception>
        public static Frame Of(int size)
        {
            foreach (Frame frame in Sizes)
            {
                if (frame.Size == size)
                {
                    return frame;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(size), size, "A frame is 256, 512, or 1024 samples.");
        }

        /// <summary>The first bin of each band, and one past the last bin of the last band, each band at least one bin wide.</summary>
        private static int[] BandEdges(int size)
        {
            int[] edges = new int[BandCount + 1];
            double binHz = LayeredSynthesizer.SampleRate / (double)size;
            double ratio = DetDouble.Log(HighestHz / LowestHz);
            for (int band = 0; band <= BandCount; band++)
            {
                int bin = (int)(LowestHz * DetDouble.Exp(ratio * band / BandCount) / binHz);
                edges[band] = band == 0 ? Math.Max(bin, 1) : Math.Max(bin, edges[band - 1] + 1);
            }

            edges[BandCount] = Math.Min(edges[BandCount], size / 2);
            return edges;
        }
    }
}
