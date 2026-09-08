using System;
using WhatYouCarry.Core.Determinism;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The state hash (D-160, PR-3 exit test 5).</summary>
public sealed class StateHashTests
{
    /// <summary>
    /// PR-3 exit test 5. The declared order of the fields is part of the hash, so a state that adds the same
    /// values in another order gives another hash.
    /// </summary>
    /// <remarks>
    /// The Phase 1 roadmap first wrote this exit test as "two states with equal fields in a different insertion
    /// order hash equal". That is the opposite of D-160, which names "a fixed declared order", and no FNV-1a hash
    /// can hold it. D-160 is the decision, so the roadmap line is corrected and this test asserts the decision.
    /// </remarks>
    [Fact]
    public void StateHashOrderIsPartOfTheContract()
    {
        StateHash declared = StateHash.Start();
        declared.Add(7);
        declared.Add(1.5f);
        declared.Add(true);

        StateHash reordered = StateHash.Start();
        reordered.Add(1.5f);
        reordered.Add(true);
        reordered.Add(7);

        Assert.NotEqual(declared.Value, reordered.Value);

        StateHash repeated = StateHash.Start();
        repeated.Add(7);
        repeated.Add(1.5f);
        repeated.Add(true);

        Assert.Equal(declared.Value, repeated.Value);
        Assert.Equal(declared, repeated);
    }

    /// <summary>
    /// The published FNV-1a 64 test vectors. The hash folds one byte at a time, so a string of bytes reproduces
    /// the reference numbers and proves the constants and the step.
    /// </summary>
    [Theory]
    [InlineData("", 0xcbf29ce484222325UL)]
    [InlineData("a", 0xaf63dc4c8601ec8cUL)]
    [InlineData("foobar", 0x85944171f73967e8UL)]
    public void StateHashMatchesThePublishedFnvVectors(string text, ulong expected)
    {
        StateHash hash = StateHash.Start();
        foreach (char letter in text)
        {
            hash.Add((byte)letter);
        }

        Assert.Equal(expected, hash.Value);
    }

    /// <summary>
    /// A hash that `default` made holds zero and not the FNV offset basis. It reports that, and it never gives
    /// a stable number that is the FNV-1a hash of nothing (T-2, F-63).
    /// </summary>
    [Fact]
    public void ADefaultStateHashReportsItself()
    {
        StateHash uninitialized = default;

        // The review trigger. The old code accepted the field and returned a number that no FNV-1a hash holds.
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => uninitialized.Add(1));
        Assert.Contains("StateHash.Start()", error.Message, StringComparison.Ordinal);

        Assert.Throws<InvalidOperationException>(() => uninitialized.Add(1.0f));
        Assert.Throws<InvalidOperationException>(() => uninitialized.Add(true));
        Assert.Throws<InvalidOperationException>(() => uninitialized.Add((byte)1));
        Assert.Throws<InvalidOperationException>(() => uninitialized.Value);
        Assert.Throws<InvalidOperationException>(() => uninitialized.ToString());

        // A default hash equals no started hash, and neither comparison throws.
        Assert.NotEqual(StateHash.Start(), uninitialized);
        Assert.Equal(default, uninitialized);
        _ = uninitialized.GetHashCode();
    }

    /// <summary>An empty hash is the FNV offset basis, and the text form is 16 hexadecimal digits.</summary>
    [Fact]
    public void AnEmptyHashIsTheOffsetBasis()
    {
        StateHash hash = StateHash.Start();
        Assert.Equal(0xcbf29ce484222325UL, hash.Value);
        Assert.Equal("cbf29ce484222325", hash.ToString());
        Assert.Equal(16, hash.ToString().Length);
    }

    /// <summary>
    /// The hash reads the bits and not the number, so positive zero and negative zero differ, and two NaN values
    /// with different payloads differ.
    /// </summary>
    [Fact]
    public void TheHashReadsTheBitsAndNotTheNumber()
    {
        StateHash positive = StateHash.Start();
        positive.Add(0.0f);

        StateHash negative = StateHash.Start();
        negative.Add(-0.0f);

        Assert.Equal(0.0f, -0.0f);
        Assert.NotEqual(positive.Value, negative.Value);
    }

    /// <summary>A signed field hashes as its two's complement bits, so it agrees with the unsigned form.</summary>
    [Fact]
    public void ASignedFieldHashesAsItsBits()
    {
        StateHash signed = StateHash.Start();
        signed.Add(-1);

        StateHash unsigned = StateHash.Start();
        unsigned.Add(0xFFFFFFFFU);

        Assert.Equal(signed.Value, unsigned.Value);

        StateHash signedLong = StateHash.Start();
        signedLong.Add(-1L);

        StateHash unsignedLong = StateHash.Start();
        unsignedLong.Add(0xFFFFFFFFFFFFFFFFUL);

        Assert.Equal(signedLong.Value, unsignedLong.Value);
    }

    /// <summary>A field of one width does not hash as the same value at another width.</summary>
    [Fact]
    public void TheWidthOfAFieldIsPartOfTheHash()
    {
        StateHash asInt = StateHash.Start();
        asInt.Add(1);

        StateHash asLong = StateHash.Start();
        asLong.Add(1L);

        StateHash asByte = StateHash.Start();
        asByte.Add((byte)1);

        Assert.NotEqual(asInt.Value, asLong.Value);
        Assert.NotEqual(asInt.Value, asByte.Value);
    }

    /// <summary>A boolean adds one byte, 1 for true and 0 for false.</summary>
    [Fact]
    public void ABooleanAddsOneByte()
    {
        StateHash yes = StateHash.Start();
        yes.Add(true);

        StateHash one = StateHash.Start();
        one.Add((byte)1);

        StateHash no = StateHash.Start();
        no.Add(false);

        Assert.Equal(one.Value, yes.Value);
        Assert.NotEqual(yes.Value, no.Value);
    }
}
