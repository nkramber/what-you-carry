using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Determinism;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The seeded RNG (D-159, PR-3 exit tests 1 and 2).</summary>
public sealed class RngTests
{
    /// <summary>
    /// PR-3 exit test 1. The first sixteen outputs of run seed 1 on the Procgen stream.
    /// </summary>
    /// <remarks>
    /// A second implementation of xoshiro128** and SplitMix64, written from the published algorithms, produced
    /// these numbers. The test therefore checks the algorithm and not only the last run of this code.
    /// </remarks>
    [Fact]
    public void RngKnownAnswer()
    {
        uint[] expected =
        [
            0xC58837C3U, 0x47454E01U, 0xDFA23619U, 0x248ADAA6U,
            0xA559985AU, 0xB4F13D73U, 0x3312E990U, 0x9F67DA9EU,
            0x32EB682DU, 0x3EE0F719U, 0xFF3ACA6DU, 0xF068EFF0U,
            0xFE5D5F57U, 0xBCEF7DD7U, 0x8E950938U, 0xF1E8C98AU,
        ];

        Rng rng = Rng.ForStream(1UL, RngStream.Procgen);
        for (int index = 0; index < expected.Length; index++)
        {
            Assert.Equal(expected[index], rng.NextUInt());
        }
    }

    /// <summary>
    /// PR-3 exit test 2. Two streams of one run stay apart across ten thousand outputs: no position holds the
    /// same number, and no run of four outputs of one stream appears anywhere in the other. A shared position in
    /// the cycle would show as a long matching run.
    /// </summary>
    [Fact]
    public void RngStreamsDiffer()
    {
        const int draws = 10000;
        RngStream[] streams = [RngStream.Procgen, RngStream.Loot, RngStream.Enemy, RngStream.Projectile];

        Dictionary<RngStream, uint[]> outputs = [];
        foreach (RngStream stream in streams)
        {
            Rng rng = Rng.ForStream(20260908UL, stream);
            uint[] values = new uint[draws];
            for (int index = 0; index < draws; index++)
            {
                values[index] = rng.NextUInt();
            }

            outputs[stream] = values;
        }

        for (int first = 0; first < streams.Length; first++)
        {
            for (int second = first + 1; second < streams.Length; second++)
            {
                uint[] left = outputs[streams[first]];
                uint[] right = outputs[streams[second]];

                int samePosition = 0;
                for (int index = 0; index < draws; index++)
                {
                    if (left[index] == right[index])
                    {
                        samePosition++;
                    }
                }

                Assert.True(
                    samePosition == 0,
                    $"{streams[first]} and {streams[second]} give the same number at {samePosition} of {draws} positions.");

                HashSet<(uint, uint, uint, uint)> windows = [];
                for (int index = 0; index + 3 < draws; index++)
                {
                    windows.Add((left[index], left[index + 1], left[index + 2], left[index + 3]));
                }

                for (int index = 0; index + 3 < draws; index++)
                {
                    (uint, uint, uint, uint) window = (right[index], right[index + 1], right[index + 2], right[index + 3]);
                    Assert.False(
                        windows.Contains(window),
                        $"{streams[second]} repeats a run of four outputs of {streams[first]} at index {index}.");
                }
            }
        }
    }

    /// <summary>One seed and one stream always give one sequence, and another seed gives another one.</summary>
    [Fact]
    public void OneSeedAndStreamGiveOneSequence()
    {
        Rng first = Rng.ForStream(77UL, RngStream.Loot);
        Rng second = Rng.ForStream(77UL, RngStream.Loot);
        Rng other = Rng.ForStream(78UL, RngStream.Loot);

        int sameAsOther = 0;
        for (int index = 0; index < 256; index++)
        {
            uint value = first.NextUInt();
            Assert.Equal(value, second.NextUInt());
            if (value == other.NextUInt())
            {
                sameAsOther++;
            }
        }

        Assert.Equal(0, sameAsOther);
    }

    /// <summary>
    /// Floor zero is the run stream, and floors one to fifteen of every subsystem give sixty streams that stay
    /// apart from each other and from the four run streams: no first sixteen outputs match at any position (D-159, PR-9).
    /// </summary>
    [Fact]
    public void FloorStreamsAreDistinct()
    {
        const int floors = 15;
        const int draws = 16;
        RngStream[] streams = [RngStream.Procgen, RngStream.Loot, RngStream.Enemy, RngStream.Projectile];

        Rng run = Rng.ForStream(31UL, RngStream.Loot);
        Rng zero = Rng.ForStream(31UL, RngStream.Loot, 0);
        for (int draw = 0; draw < draws; draw++)
        {
            Assert.Equal(run.NextUInt(), zero.NextUInt());
        }

        List<uint[]> outputs = [];
        foreach (RngStream stream in streams)
        {
            for (int floor = 0; floor <= floors; floor++)
            {
                Rng rng = Rng.ForStream(31UL, stream, floor);
                uint[] values = new uint[draws];
                for (int draw = 0; draw < draws; draw++)
                {
                    values[draw] = rng.NextUInt();
                }

                outputs.Add(values);
            }
        }

        for (int first = 0; first < outputs.Count; first++)
        {
            for (int second = first + 1; second < outputs.Count; second++)
            {
                int matches = 0;
                for (int draw = 0; draw < draws; draw++)
                {
                    if (outputs[first][draw] == outputs[second][draw])
                    {
                        matches++;
                    }
                }

                Assert.True(matches == 0, $"Streams {first} and {second} share {matches} of the first {draws} outputs.");
            }
        }
    }

    /// <summary>A negative floor is an error with its value in the message (T-2).</summary>
    [Fact]
    public void ANegativeFloorIsAnError()
    {
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(
            () => Rng.ForStream(1UL, RngStream.Procgen, -1));
        Assert.Contains("-1", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The bound check accepts every declared stream, so a new value cannot pass the bound in silence.</summary>
    [Fact]
    public void EveryDeclaredStreamIsAccepted()
    {
        foreach (RngStream stream in Enum.GetValues<RngStream>())
        {
            Rng rng = Rng.ForStream(5UL, stream);
            Assert.NotEqual(0U, rng.NextUInt() | 1U);
        }
    }

    /// <summary>A stream outside the declared set is an error with its value in the message (T-2).</summary>
    [Fact]
    public void AnUndeclaredStreamIsAnError()
    {
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(
            () => Rng.ForStream(1UL, (RngStream)99));
        Assert.Contains("99", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Every float lands in [0, 1), and the four quarters of the range all fill.</summary>
    [Fact]
    public void NextFloatStaysInsideTheUnitRange()
    {
        Rng rng = Rng.ForStream(31UL, RngStream.Projectile);
        int[] quarters = new int[4];
        for (int index = 0; index < 100000; index++)
        {
            float value = rng.NextFloat();
            Assert.InRange(value, 0.0f, 0.99999994f);
            quarters[(int)(value * 4.0f)]++;
        }

        foreach (int count in quarters)
        {
            Assert.InRange(count, 24000, 26000);
        }
    }

    /// <summary>The first floats match the 24-bit conversion of the first words, with no rounding of its own.</summary>
    [Fact]
    public void NextFloatUsesTwentyFourBitsOfTheWord()
    {
        Rng words = Rng.ForStream(1UL, RngStream.Procgen);
        Rng floats = Rng.ForStream(1UL, RngStream.Procgen);
        for (int index = 0; index < 64; index++)
        {
            float expected = (words.NextUInt() >> 8) * (1.0f / 16777216.0f);
            Assert.Equal(expected, floats.NextFloat());
        }
    }

    /// <summary>Every bounded integer stays inside its bound, and a small bound fills evenly.</summary>
    [Fact]
    public void NextIntStaysInsideItsBoundAndFillsEvenly()
    {
        Rng rng = Rng.ForStream(9001UL, RngStream.Enemy);
        int[] counts = new int[6];
        for (int index = 0; index < 60000; index++)
        {
            int value = rng.NextInt(6);
            Assert.InRange(value, 0, 5);
            counts[value]++;
        }

        foreach (int count in counts)
        {
            Assert.InRange(count, 9500, 10500);
        }

        // A bound of one has one answer, and it must never draw outside it.
        for (int index = 0; index < 100; index++)
        {
            Assert.Equal(0, rng.NextInt(1));
        }
    }

    /// <summary>A bound of zero or below is an error with the bound in the message (T-2).</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ANonPositiveBoundIsAnError(int bound)
    {
        Rng rng = Rng.ForStream(1UL, RngStream.Procgen);
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(bound));
        Assert.Contains(bound.ToString(System.Globalization.CultureInfo.InvariantCulture), error.Message, StringComparison.Ordinal);
    }
}
