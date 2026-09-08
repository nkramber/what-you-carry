using System;

namespace WhatYouCarry.Core.Determinism;

/// <summary>
/// A seeded random number stream (D-159). The generator is xoshiro128**, and SplitMix64 turns the 64-bit run
/// seed and the stream id into the four state words.
/// </summary>
/// <remarks>
/// <para>
/// Every step is an integer operation, so two platforms give the same numbers bit for bit. A run replays from
/// its seed alone (G-5).
/// </para>
/// <para>
/// This is a class and not a struct on purpose. A struct copy would give the copy its own stream, and the two
/// halves would then give the same numbers twice. A subsystem holds one stream and shares it by reference.
/// </para>
/// </remarks>
public sealed class Rng
{
    // The SplitMix64 constants. The first is the golden-ratio step, and the other two are the finalizer.
    private const ulong GoldenGamma = 0x9E3779B97F4A7C15UL;
    private const ulong FinalizerA = 0xBF58476D1CE4E5B9UL;
    private const ulong FinalizerB = 0x94D049BB133111EBUL;

    private uint state0;
    private uint state1;
    private uint state2;
    private uint state3;

    private Rng(uint state0, uint state1, uint state2, uint state3)
    {
        this.state0 = state0;
        this.state1 = state1;
        this.state2 = state2;
        this.state3 = state3;
    }

    /// <summary>
    /// The stream of one subsystem for one run. Two streams of one run never share a state word, because the
    /// stream id goes through the SplitMix64 finalizer before it seeds the words.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The stream is not a value of <see cref="RngStream"/>.</exception>
    public static Rng ForStream(ulong runSeed, RngStream stream)
    {
        // An explicit bound, because Enum.IsDefined reads the enum through reflection, and Core has none (G-2).
        // `EveryDeclaredStreamIsAccepted` walks the declared values, so a new value that passes this bound fails
        // the test until the bound names it.
        if (stream < RngStream.Procgen || stream > RngStream.Projectile)
        {
            throw new ArgumentOutOfRangeException(nameof(stream), $"The stream must be a value of RngStream. The value is {(int)stream}.");
        }

        // The finalizer spreads every bit of the run seed and the stream id over all 64 bits, so two adjacent
        // stream ids give seeder states that are far apart.
        ulong seederState = Finalize(runSeed + (((ulong)stream + 1UL) * GoldenGamma));

        ulong first = NextSeederWord(ref seederState);
        ulong second = NextSeederWord(ref seederState);

        uint word0 = (uint)first;
        uint word1 = (uint)(first >> 32);
        uint word2 = (uint)second;
        uint word3 = (uint)(second >> 32);

        // The all-zero state is the one fixed point of xoshiro128**, and it would give zero forever.
        // The seeder makes it about as likely as 2^-128, and a silent zero stream is still a defect (T-2).
        if ((word0 | word1 | word2 | word3) == 0U)
        {
            throw new InvalidOperationException($"The seeder gave the all-zero state, which xoshiro128** cannot use. The run seed is {runSeed}, and the stream is {stream}.");
        }

        return new Rng(word0, word1, word2, word3);
    }

    /// <summary>The next number of the stream, over the whole range of a 32-bit unsigned integer.</summary>
    public uint NextUInt()
    {
        // xoshiro128**, by Blackman and Vigna. The result comes from the state before the step.
        uint result = RotateLeft(this.state1 * 5U, 7) * 9U;
        uint shifted = this.state1 << 9;

        this.state2 ^= this.state0;
        this.state3 ^= this.state1;
        this.state1 ^= this.state2;
        this.state0 ^= this.state3;
        this.state2 ^= shifted;
        this.state3 = RotateLeft(this.state3, 11);

        return result;
    }

    /// <summary>
    /// The next number of the stream, in [0, 1). The 24 bits fill the float mantissa exactly, so every step
    /// of the conversion is exact and the spacing is even.
    /// </summary>
    public float NextFloat()
    {
        // 2^-24 is a power of two, so the multiply is exact.
        return (this.NextUInt() >> 8) * (1.0f / 16777216.0f);
    }

    /// <summary>
    /// The next number of the stream, in [0, <paramref name="exclusiveUpperBound"/>). Every value is equally
    /// likely, because the method draws again for a value in the partial block at the top of the range.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The bound is zero or negative.</exception>
    public int NextInt(int exclusiveUpperBound)
    {
        if (exclusiveUpperBound <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exclusiveUpperBound), $"The bound must be above zero. The bound is {exclusiveUpperBound}.");
        }

        ulong bound = (ulong)exclusiveUpperBound;

        // The draw covers 2^32 values. Keep the largest multiple of the bound that fits, and reject the rest.
        // A plain remainder without the rejection would favor the low values.
        ulong range = 1UL << 32;
        ulong limit = range - (range % bound);

        ulong value;
        do
        {
            // The loop always ends: the rejected part is under half the range, so each draw ends it with a
            // chance above one half. The count of draws depends on the state alone, so a replay repeats it.
            value = this.NextUInt();
        }
        while (value >= limit);

        return (int)(value % bound);
    }

    /// <summary>One SplitMix64 step: advance the seeder state, then mix it.</summary>
    private static ulong NextSeederWord(ref ulong seederState)
    {
        seederState += GoldenGamma;
        return Finalize(seederState);
    }

    /// <summary>The SplitMix64 finalizer. It spreads every input bit over all 64 output bits.</summary>
    private static ulong Finalize(ulong value)
    {
        ulong mixed = value;
        mixed = (mixed ^ (mixed >> 30)) * FinalizerA;
        mixed = (mixed ^ (mixed >> 27)) * FinalizerB;
        return mixed ^ (mixed >> 31);
    }

    /// <summary>A left rotation of a 32-bit word. The count must be from 1 to 31, and both call sites are literal.</summary>
    private static uint RotateLeft(uint value, int count)
    {
        return (value << count) | (value >> (32 - count));
    }
}
