using System;
using WhatYouCarry.Core.Determinism;

namespace WhatYouCarry.Tools.AudioSynth;

/// <summary>
/// The fast Fourier transform of a power-of-two count of real samples, in place, by the iterative radix-2 method. The
/// twiddle factors come from <see cref="DetMath"/>, so the result has equal bits on every platform.
/// </summary>
public sealed class Fft
{
    private readonly int size;
    private readonly double[] cosines;
    private readonly double[] sines;

    /// <summary>A transform of one size.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The size is not a power of two from 2 upward.</exception>
    public Fft(int size)
    {
        if (size < 2 || (size & (size - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, "The size of a transform is a power of two from 2 upward.");
        }

        this.size = size;
        this.cosines = new double[size / 2];
        this.sines = new double[size / 2];
        for (int index = 0; index < size / 2; index++)
        {
            float angle = (float)(-2.0 * Math.PI * index / size);
            this.cosines[index] = DetMath.Cos(angle);
            this.sines[index] = DetMath.Sin(angle);
        }
    }

    /// <summary>Transforms the real and imaginary parts in place.</summary>
    /// <exception cref="ArgumentException">A part does not hold the size of the transform.</exception>
    public void Transform(double[] real, double[] imaginary)
    {
        if (real.Length != this.size || imaginary.Length != this.size)
        {
            throw new ArgumentException("Each part of a transform holds the size of the transform.");
        }

        for (int index = 1, reversed = 0; index < this.size; index++)
        {
            int bit = this.size >> 1;
            for (; (reversed & bit) != 0; bit >>= 1)
            {
                reversed ^= bit;
            }

            reversed ^= bit;
            if (index < reversed)
            {
                (real[index], real[reversed]) = (real[reversed], real[index]);
                (imaginary[index], imaginary[reversed]) = (imaginary[reversed], imaginary[index]);
            }
        }

        for (int length = 2; length <= this.size; length <<= 1)
        {
            int step = this.size / length;
            for (int start = 0; start < this.size; start += length)
            {
                for (int offset = 0; offset < length / 2; offset++)
                {
                    double cosine = this.cosines[offset * step];
                    double sine = this.sines[offset * step];
                    int even = start + offset;
                    int odd = even + (length / 2);
                    double oddReal = (real[odd] * cosine) - (imaginary[odd] * sine);
                    double oddImaginary = (real[odd] * sine) + (imaginary[odd] * cosine);
                    real[odd] = real[even] - oddReal;
                    imaginary[odd] = imaginary[even] - oddImaginary;
                    real[even] += oddReal;
                    imaginary[even] += oddImaginary;
                }
            }
        }
    }
}
