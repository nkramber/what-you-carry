using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.AudioSynth;

/// <summary>
/// The layered sound synthesizer (D-93, D-462): it renders the layers of a sound to samples at 44.1 kHz, mixes them,
/// scales the mix so its peak is the gain of the sound, and ends the sound at its last sample within 60 decibels of the
/// peak.
/// </summary>
/// <remarks>
/// <para>
/// Each layer starts after its delay. A modal, noise, or tone layer rises in a straight line over its attack, holds, and
/// then falls by 60 decibels over its decay on an exponential curve. A spectral layer renders its levels with its
/// controls (<see cref="SpectralLayer"/>). A recording layer plays its recording to the trim, with the fade. The sound
/// lasts until the last layer ends. A modal layer strikes each of its
/// resonators with one short burst of white noise that falls in a straight line. A noise layer passes colored noise
/// through a state-variable filter. A tone layer runs a sine, a band-limited saw or square, or an FM pair. The cutoff of
/// a noise layer and the pitch of a tone layer move on an exponential curve from the start value to the end value over
/// the envelope of the layer.
/// </para>
/// <para>
/// The render gives equal bytes on every platform (D-453). It uses double arithmetic, the exponential and the
/// logarithm of <see cref="DetDouble"/>, and the sine and the cosine of <see cref="DetMath"/>, because the versions of the
/// runtime differ by platform. The noise of each layer comes from a xorshift sequence from the seed and the index of the
/// layer.
/// </para>
/// </remarks>
public static class LayeredSynthesizer
{
    /// <summary>The sample rate of every rendered sound, in samples per second (D-462).</summary>
    public const int SampleRate = 44100;

    private const double TwoPi = 2.0 * Math.PI;
    private const double HighestFrequencyHz = 0.45 * SampleRate;
    private const uint LayerSeedStep = 0x9E3779B9u;
    private const double Thousandth = 0.001;

    /// <summary>The samples of one sound, from minus 1 to 1.</summary>
    /// <param name="definition">The sound.</param>
    /// <param name="recordings">The samples of each recording that a recording layer names, by content path.</param>
    /// <exception cref="WhatYouCarry.Core.Logging.ContextException">A recording layer names a recording that the map does not hold (T-2).</exception>
    public static float[] Render(SoundDefinition definition, IReadOnlyDictionary<string, float[]> recordings)
    {
        double[][] parts = new double[definition.Layers.Count][];
        int length = 0;
        for (int index = 0; index < definition.Layers.Count; index++)
        {
            Layer layer = definition.Layers[index];
            uint seed = definition.Seed + ((uint)index * LayerSeedStep);
            parts[index] = RenderLayer(layer, seed == 0 ? 1u : seed, recordings);
            length = Math.Max(length, Samples(layer.DelayMs) + parts[index].Length);
        }

        double[] mix = new double[length];
        for (int index = 0; index < parts.Length; index++)
        {
            Layer layer = definition.Layers[index];
            int start = Samples(layer.DelayMs);
            for (int sample = 0; sample < parts[index].Length; sample++)
            {
                mix[start + sample] += parts[index][sample] * layer.Gain;
            }
        }

        // The gain of the sound is its peak, so the level in the game is a choice of the file, and the layers set the shape.
        double peak = 0.0;
        foreach (double value in mix)
        {
            peak = Math.Max(peak, Math.Abs(value));
        }

        // A tail more than 60 decibels below the peak is silent in play, and it costs memory and a voice.
        double scale = peak > 0.0 ? definition.Gain / peak : 0.0;
        int end = length;
        while (end > 1 && Math.Abs(mix[end - 1]) < peak * Thousandth)
        {
            end--;
        }

        float[] samples = new float[end];
        for (int sample = 0; sample < end; sample++)
        {
            samples[sample] = (float)Math.Clamp(mix[sample] * scale, -1.0, 1.0);
        }

        return samples;
    }

    /// <summary>The count of samples of a time in milliseconds, with the fraction cut.</summary>
    public static int Samples(double milliseconds)
    {
        return (int)(milliseconds * SampleRate / 1000.0);
    }

    /// <summary>The samples of the envelope of a layer: the attack, the hold, and the decay.</summary>
    private static int EnvelopeSamples(Layer layer)
    {
        return Samples(layer.AttackMs) + Samples(layer.HoldMs) + Math.Max(Samples(layer.DecayMs), 1);
    }

    /// <summary>The samples of one layer before its delay and its gain: a spectral render, a recording, or a source under the envelope.</summary>
    private static double[] RenderLayer(Layer layer, uint seed, IReadOnlyDictionary<string, float[]> recordings)
    {
        if (layer.Kind == LayerKind.Spectral)
        {
            return SpectralLayer.Render(layer, seed);
        }

        if (layer.Kind == LayerKind.Recording)
        {
            if (!recordings.TryGetValue(layer.File, out float[]? recording))
            {
                ContextException error = new("A recording layer names a recording that the render did not load (D-467).");
                error.AddContext("file", layer.File);
                throw error;
            }

            return TrimRecording(recording, Samples(layer.TrimMs), Samples(layer.FadeMs));
        }

        int count = EnvelopeSamples(layer);
        Envelope envelope = new(Samples(layer.AttackMs), Samples(layer.HoldMs), Math.Max(Samples(layer.DecayMs), 1));
        Func<double> source = layer.Kind switch
        {
            LayerKind.Modal => new ModalSource(layer, seed).Next,
            LayerKind.Noise => new FilteredNoise(layer, seed, count).Next,
            _ => new ToneSource(layer, count).Next,
        };

        double[] samples = new double[count];
        for (int sample = 0; sample < count; sample++)
        {
            samples[sample] = source() * envelope.Next();
        }

        return samples;
    }

    /// <summary>A recording cut to a length, with a fade in a straight line over its last samples.</summary>
    private static double[] TrimRecording(float[] recording, int trim, int fade)
    {
        int length = Math.Max(Math.Min(trim, recording.Length), 1);
        double[] samples = new double[length];
        for (int index = 0; index < length && index < recording.Length; index++)
        {
            int fromEnd = length - index;
            samples[index] = fade > 0 && fromEnd <= fade ? recording[index] * (fromEnd - 1) / (double)fade : recording[index];
        }

        return samples;
    }

    /// <summary>The sine of an angle in radians, reduced to one turn, from the polynomial of Core (G-2).</summary>
    private static double Sine(double radians)
    {
        double turns = radians / TwoPi;
        return DetMath.Sin((float)((turns - Math.Floor(turns)) * TwoPi));
    }

    /// <summary>The cosine of an angle in radians, reduced to one turn, from the polynomial of Core (G-2).</summary>
    private static double Cosine(double radians)
    {
        double turns = radians / TwoPi;
        return DetMath.Cos((float)((turns - Math.Floor(turns)) * TwoPi));
    }

    /// <summary>The factor that takes a value from the start to the end in a count of equal exponential steps.</summary>
    private static double SweepFactor(double start, double end, int count)
    {
        return DetDouble.Exp(DetDouble.Log(end / start) / count);
    }

    /// <summary>The attack in a straight line, the hold at 1, and the decay by 60 decibels on an exponential curve.</summary>
    private sealed class Envelope(int attack, int hold, int decay)
    {
        private readonly double decayFactor = DetDouble.Exp(DetDouble.LnThousandth / decay);
        private double level = 1.0;
        private int sample;

        public double Next()
        {
            int position = this.sample;
            this.sample++;
            if (position < attack)
            {
                return (position + 1) / (double)attack;
            }

            if (position <= attack + hold)
            {
                return 1.0;
            }

            this.level *= this.decayFactor;
            return this.level;
        }
    }

    /// <summary>A xorshift sequence of 32 bits, with the shifts 13, 17, and 5, as white noise from minus 1 to 1.</summary>
    private sealed class WhiteNoise(uint seed)
    {
        private uint state = seed;

        public double Next()
        {
            uint value = this.state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            this.state = value;
            return (value / (double)uint.MaxValue * 2.0) - 1.0;
        }
    }

    /// <summary>A bank of two-pole resonators that one short burst of white noise strikes.</summary>
    private sealed class ModalSource
    {
        private readonly WhiteNoise noise;
        private readonly int strike;
        private readonly double strikeScale;
        private readonly double[] b0;
        private readonly double[] a1;
        private readonly double[] a2;
        private readonly double[] amplitude;
        private readonly double[] y1;
        private readonly double[] y2;
        private int sample;

        public ModalSource(Layer layer, uint seed)
        {
            this.noise = new WhiteNoise(seed);
            this.strike = Math.Max(Samples(layer.StrikeMs), 1);
            this.strikeScale = 1.0 / Math.Sqrt(this.strike);
            int count = layer.Modes.Count;
            this.b0 = new double[count];
            this.a1 = new double[count];
            this.a2 = new double[count];
            this.amplitude = new double[count];
            this.y1 = new double[count];
            this.y2 = new double[count];
            for (int index = 0; index < count; index++)
            {
                Mode mode = layer.Modes[index];
                double angle = TwoPi * Math.Min(mode.FrequencyHz, HighestFrequencyHz) / SampleRate;
                double radius = DetDouble.Exp(DetDouble.LnThousandth / Math.Max(Samples(mode.DecayMs), 1));

                // With b0 at the sine of the angle, the impulse response of a resonator peaks near 1.
                this.b0[index] = Sine(angle);
                this.a1[index] = 2.0 * radius * Cosine(angle);
                this.a2[index] = -radius * radius;
                this.amplitude[index] = mode.Amplitude;
            }
        }

        public double Next()
        {
            double input = 0.0;
            if (this.sample < this.strike)
            {
                input = this.noise.Next() * (1.0 - (this.sample / (double)this.strike)) * this.strikeScale;
            }

            this.sample++;
            double sum = 0.0;
            for (int index = 0; index < this.b0.Length; index++)
            {
                double output = (this.b0[index] * input) + (this.a1[index] * this.y1[index]) + (this.a2[index] * this.y2[index]);
                this.y2[index] = this.y1[index];
                this.y1[index] = output;
                sum += output * this.amplitude[index];
            }

            return sum;
        }
    }

    /// <summary>White, pink, or brown noise through a state-variable filter whose cutoff sweeps.</summary>
    private sealed class FilteredNoise
    {
        private readonly WhiteNoise noise;
        private readonly NoiseColor color;
        private readonly FilterKind filter;
        private readonly double damping;
        private readonly double cutoffFactor;
        private double cutoff;
        private double pink0;
        private double pink1;
        private double pink2;
        private double brown;
        private double state1;
        private double state2;

        public FilteredNoise(Layer layer, uint seed, int count)
        {
            this.noise = new WhiteNoise(seed);
            this.color = layer.Color;
            this.filter = layer.Filter;
            this.damping = 2.0 - (2.0 * layer.Resonance);
            this.cutoff = layer.CutoffStartHz;
            this.cutoffFactor = SweepFactor(layer.CutoffStartHz, layer.CutoffEndHz, count);
        }

        public double Next()
        {
            double input = this.Colored();
            double frequency = Math.Min(this.cutoff, HighestFrequencyHz);
            this.cutoff *= this.cutoffFactor;

            // The state-variable filter of the topology-preserving transform, stable at every cutoff.
            double angle = Math.PI * frequency / SampleRate;
            double g = Sine(angle) / Cosine(angle);
            double b1 = 1.0 / (1.0 + (g * (g + this.damping)));
            double b2 = g * b1;
            double b3 = g * b2;
            double v3 = input - this.state2;
            double band = (b1 * this.state1) + (b2 * v3);
            double low = this.state2 + (b2 * this.state1) + (b3 * v3);
            this.state1 = (2.0 * band) - this.state1;
            this.state2 = (2.0 * low) - this.state2;
            switch (this.filter)
            {
                case FilterKind.LowPass:
                    return low;
                case FilterKind.HighPass:
                    return input - (this.damping * band) - low;
                default:
                    // The band-pass output times the damping has a gain of 1 at the cutoff.
                    return band * this.damping;
            }
        }

        /// <summary>One sample of the noise color: white, pink from three one-pole filters, or brown from a leaky sum.</summary>
        private double Colored()
        {
            double white = this.noise.Next();
            switch (this.color)
            {
                case NoiseColor.White:
                    return white;
                case NoiseColor.Pink:
                    this.pink0 = (0.99765 * this.pink0) + (white * 0.0990460);
                    this.pink1 = (0.96300 * this.pink1) + (white * 0.2965164);
                    this.pink2 = (0.57000 * this.pink2) + (white * 1.0526913);
                    return (this.pink0 + this.pink1 + this.pink2 + (white * 0.1848)) * 0.25;
                default:
                    this.brown = (this.brown + (0.02 * white)) / 1.02;
                    return this.brown * 3.5;
            }
        }
    }

    /// <summary>A sine, a band-limited saw or square, or an FM pair, whose pitch sweeps.</summary>
    private sealed class ToneSource
    {
        private readonly ToneWave wave;
        private readonly double frequencyFactor;
        private readonly double ratio;
        private readonly double index;
        private double frequency;
        private double phase;
        private double modulatorPhase;

        public ToneSource(Layer layer, int count)
        {
            this.wave = layer.Wave;
            this.frequency = layer.FrequencyStartHz;
            this.frequencyFactor = SweepFactor(layer.FrequencyStartHz, layer.FrequencyEndHz, count);
            this.ratio = layer.FmRatio;
            this.index = layer.FmIndex;
        }

        public double Next()
        {
            double step = Math.Min(this.frequency, HighestFrequencyHz) / SampleRate;
            double value;
            switch (this.wave)
            {
                case ToneWave.Sine:
                    value = Sine(TwoPi * this.phase);
                    break;
                case ToneWave.Saw:
                    value = (2.0 * this.phase) - 1.0 - PolyBlep(this.phase, step);
                    break;
                case ToneWave.Square:
                    double half = this.phase + 0.5 - Math.Floor(this.phase + 0.5);
                    value = (this.phase < 0.5 ? 1.0 : -1.0) + PolyBlep(this.phase, step) - PolyBlep(half, step);
                    break;
                default:
                    value = Sine((TwoPi * this.phase) + (this.index * Sine(TwoPi * this.modulatorPhase)));
                    this.modulatorPhase += step * this.ratio;
                    this.modulatorPhase -= Math.Floor(this.modulatorPhase);
                    break;
            }

            this.phase += step;
            this.phase -= Math.Floor(this.phase);
            this.frequency *= this.frequencyFactor;
            return value;
        }

        /// <summary>The correction of a jump of a saw or a square near its edge, so the edge makes less aliasing.</summary>
        private static double PolyBlep(double position, double step)
        {
            if (position < step)
            {
                double x = position / step;
                return x + x - (x * x) - 1.0;
            }

            if (position > 1.0 - step)
            {
                double x = (position - 1.0) / step;
                return (x * x) + x + x + 1.0;
            }

            return 0.0;
        }
    }
}
