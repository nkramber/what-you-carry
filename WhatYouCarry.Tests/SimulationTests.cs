using System;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The CRC-32, the intent frame, and the fixed-step loop (D-73, D-162, D-224, D-227; PR-6). The loop tests run on the flat floor of <see cref="TestWorld"/> (PR-7).</summary>
public sealed class SimulationTests
{
    /// <summary>The check value of the CRC-32 standard: the text "123456789" gives 0xCBF43926.</summary>
    [Fact]
    public void Crc32KnownAnswer()
    {
        byte[] text = "123456789"u8.ToArray();
        Assert.Equal(0xCBF43926U, Crc32.Of(text, 0, text.Length));
        Assert.Equal(0U, Crc32.Of(text, 0, 0));
    }

    /// <summary>The checksum reads the range alone, so the bytes around it do not matter.</summary>
    [Fact]
    public void Crc32ReadsTheRangeAlone()
    {
        byte[] text = "xx123456789yy"u8.ToArray();
        Assert.Equal(0xCBF43926U, Crc32.Of(text, 2, 9));
    }

    /// <summary>A range outside the bytes is an error, and never a checksum of the wrong bytes (T-2).</summary>
    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, 4)]
    [InlineData(3, 1)]
    [InlineData(4, 0)]
    [InlineData(1, -1)]
    public void Crc32RejectsARangeOutsideTheBytes(int offset, int count)
    {
        byte[] bytes = [1, 2, 3];
        Assert.Throws<ArgumentOutOfRangeException>(() => Crc32.Of(bytes, offset, count));
    }

    /// <summary>The frame layout of D-162, byte for byte, with every field little-endian.</summary>
    [Fact]
    public void IntentFrameLayout()
    {
        Intent intent = new(0x01020304U, 0x0506, -2, 3, -4, 0xABCD);
        byte[] frame = intent.Encode();

        Assert.Equal(Intent.FrameSize, frame.Length);
        Assert.Equal(new byte[] { 0x04, 0x03, 0x02, 0x01, 0x06, 0x05, 0xFE, 0xFF, 0x03, 0xFC, 0xCD, 0xAB }, frame[..Intent.ChecksumOffset]);

        uint checksum = Crc32.Of(frame, 0, Intent.ChecksumOffset);
        Assert.Equal((byte)checksum, frame[12]);
        Assert.Equal((byte)(checksum >> 8), frame[13]);
        Assert.Equal((byte)(checksum >> 16), frame[14]);
        Assert.Equal((byte)(checksum >> 24), frame[15]);
    }

    /// <summary>Every intent survives an encode and a decode, over one thousand seeds. A failure names its seed (D-66).</summary>
    [Fact]
    public void IntentRoundTrip()
    {
        for (int seed = 1; seed <= 1000; seed++)
        {
            Random random = new(seed);
            Intent intent = RandomIntent(random, (uint)random.Next());
            Intent decoded = Intent.Decode(intent.Encode(), 0);
            Assert.True(intent == decoded, $"Seed {seed}: {intent} decoded as {decoded}.");
        }
    }

    /// <summary>The extreme values of each field round trip, because a sign bit is the easiest bit to lose.</summary>
    [Fact]
    public void IntentExtremesRoundTrip()
    {
        Intent low = new(0U, short.MinValue, short.MinValue, sbyte.MinValue, sbyte.MinValue, 0);
        Intent high = new(uint.MaxValue, short.MaxValue, short.MaxValue, sbyte.MaxValue, sbyte.MaxValue, ushort.MaxValue);
        Assert.Equal(low, Intent.Decode(low.Encode(), 0));
        Assert.Equal(high, Intent.Decode(high.Encode(), 0));
    }

    /// <summary>The decode reads from the offset, so a frame inside a record decodes in place.</summary>
    [Fact]
    public void IntentDecodesAtAnOffset()
    {
        Intent intent = new(7U, 1, 2, 3, 4, 5);
        byte[] record = new byte[40];
        intent.Encode().CopyTo(record, 20);
        Assert.Equal(intent, Intent.Decode(record, 20));
    }

    /// <summary>A flipped bit in any byte of a frame is an error that names the tick and both checksums (T-2).</summary>
    [Fact]
    public void IntentFlipFailsTheChecksum()
    {
        Intent intent = new(42U, 100, -100, 1, -1, 0x0101);
        for (int index = 0; index < Intent.FrameSize; index++)
        {
            for (int bit = 0; bit < 8; bit++)
            {
                byte[] frame = intent.Encode();
                frame[index] ^= (byte)(1 << bit);
                ContextException error = Assert.Throws<ContextException>(() => Intent.Decode(frame, 0));
                Assert.Contains("storedChecksum", error.Message, StringComparison.Ordinal);
                Assert.Contains("computedChecksum", error.Message, StringComparison.Ordinal);
            }
        }
    }

    /// <summary>A frame needs sixteen bytes, and fewer is an error that names the count.</summary>
    [Fact]
    public void IntentDecodeNeedsAWholeFrame()
    {
        byte[] frame = new Intent(1U, 0, 0, 0, 0, 0).Encode();
        ContextException error = Assert.Throws<ContextException>(() => Intent.Decode(frame, 1));
        Assert.Contains("15", error.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => Intent.Decode(frame, -1));
    }

    /// <summary>The loop runs at 60 Hz, and the simulation version is 2 since PR-7 gave the state a position (D-73, D-151, G-20).</summary>
    [Fact]
    public void TheConstantsHold()
    {
        Assert.Equal(60, SimulationLoop.TicksPerSecond);
        Assert.Equal(2, SimulationVersion.Value);
    }

    /// <summary>One intent is one tick, and the loop starts at tick zero.</summary>
    [Fact]
    public void OneIntentIsOneTick()
    {
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        Assert.Equal(0U, loop.Tick);
        loop.Step(new Intent(0U, 0, 0, 0, 0, 0));
        loop.Step(new Intent(1U, 0, 0, 0, 0, 0));
        Assert.Equal(2U, loop.Tick);
    }

    /// <summary>An intent for another tick is an error that names both ticks (G-5, T-2).</summary>
    [Fact]
    public void AnIntentOutOfOrderIsAnError()
    {
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        loop.Step(new Intent(0U, 0, 0, 0, 0, 0));

        ContextException error = Assert.Throws<ContextException>(() => loop.Step(new Intent(5U, 0, 0, 0, 0, 0)));
        Assert.Contains("expectedTick=1", error.Message, StringComparison.Ordinal);
        Assert.Contains("intentTick=5", error.Message, StringComparison.Ordinal);
        Assert.Equal(1U, loop.Tick);
    }

    /// <summary>The yaw sum wraps at a full turn in both directions, and it never goes negative (D-227).</summary>
    [Fact]
    public void YawWraps()
    {
        // A delta is an int16, so a turn near 360 degrees takes two intents.
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        loop.Step(new Intent(0U, 30000, 0, 0, 0, 0));
        loop.Step(new Intent(1U, 5990, 0, 0, 0, 0));
        Assert.Equal(35990, loop.Yaw);
        loop.Step(new Intent(2U, 20, 0, 0, 0, 0));
        Assert.Equal(10, loop.Yaw);
        loop.Step(new Intent(3U, -20, 0, 0, 0, 0));
        Assert.Equal(35990, loop.Yaw);
    }

    /// <summary>The pitch sum stops at straight up and straight down (D-227).</summary>
    [Fact]
    public void PitchClamps()
    {
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        loop.Step(new Intent(0U, 0, 8000, 0, 0, 0));
        loop.Step(new Intent(1U, 0, 8000, 0, 0, 0));
        Assert.Equal(SimulationLoop.PitchLimit, loop.Pitch);
        loop.Step(new Intent(2U, 0, short.MinValue, 0, 0, 0));
        Assert.Equal(-SimulationLoop.PitchLimit, loop.Pitch);
    }

    /// <summary>The buttons are the mask of the last intent, and the hash reads them.</summary>
    [Fact]
    public void ButtonsFollowTheLastIntent()
    {
        SimulationLoop pressed = TestWorld.NewLoop(1UL);
        SimulationLoop released = TestWorld.NewLoop(1UL);
        pressed.Step(new Intent(0U, 0, 0, 0, 0, 0x0004));
        released.Step(new Intent(0U, 0, 0, 0, 0, 0));

        Assert.Equal((ushort)0x0004, pressed.Buttons);
        Assert.NotEqual(pressed.Hash(), released.Hash());
    }

    /// <summary>Two runs with one intent stream and two seeds give two hashes, because the seed is state (D-159).</summary>
    [Fact]
    public void TheHashCoversTheSeed()
    {
        SimulationLoop first = TestWorld.NewLoop(1UL);
        SimulationLoop second = TestWorld.NewLoop(2UL);
        Assert.NotEqual(first.Hash(), second.Hash());
        Assert.Equal(first.Hash(), TestWorld.NewLoop(1UL).Hash());
    }

    /// <summary>
    /// A random intent for one tick. The tests, the replay tests, and the body tests share it. The buttons keep
    /// the eight assigned bits, because a set reserved bit is an error (D-232).
    /// </summary>
    public static Intent RandomIntent(Random random, uint tick)
    {
        return new Intent(
            tick,
            (short)random.Next(short.MinValue, short.MaxValue + 1),
            (short)random.Next(short.MinValue, short.MaxValue + 1),
            (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue + 1),
            (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue + 1),
            (ushort)random.Next(0, Button.AssignedMask + 1));
    }
}
