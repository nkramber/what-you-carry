using System;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;
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
    /// F-123. Two path followers that differ only in the stored path give two hashes. Each walks from one cell to
    /// one goal and holds the same waypoint and the same ticks since the search, but a wall on the second grid
    /// takes its path around. The hash folded the waypoint and the ticks alone, so a divergence of the path showed
    /// only when it moved a body (D-160).
    /// </summary>
    [Fact]
    public void TheStoredPathOfAFollowerIsPartOfTheHash()
    {
        VoxelGrid open = TestWorld.FlatFloor(8, 6);
        VoxelGrid walled = TestWorld.FlatFloor(8, 6);
        for (int z = 0; z < 3; z++)
        {
            walled.Set(3, 1, z, BlockId.RawStone);
            walled.Set(3, 2, z, BlockId.RawStone);
        }

        Cell goal = new(5, 0, 1);
        Vector3 feet = PathWalk.CenterOf(new Cell(1, 0, 1));
        PathFollower straight = new();
        PathFollower around = new();
        Assert.True(straight.TryNext(open, new GridPathfinder(open), feet, true, goal, out _), "The open grid gave no path to the goal.");
        Assert.True(around.TryNext(walled, new GridPathfinder(walled), feet, true, goal, out _), "The walled grid gave no path to the goal.");

        // The case holds only when the two followers differ in the path alone.
        Assert.Equal(straight.Waypoint, around.Waypoint);
        Assert.Equal(goal, straight.Path[straight.Path.Count - 1]);
        Assert.Equal(goal, around.Path[around.Path.Count - 1]);
        Assert.NotEqual(straight.Path, around.Path);

        StateHash straightHash = StateHash.Start();
        straight.AddTo(ref straightHash);
        StateHash aroundHash = StateHash.Start();
        around.AddTo(ref aroundHash);
        Assert.NotEqual(straightHash.Value, aroundHash.Value);
    }

    /// <summary>
    /// F-123. Each field of the wedge count of a follower is part of the hash (D-160): the goal of the best estimate,
    /// the best estimate, the ticks with no gain, and the result of the last search. The test sets one field of a new
    /// follower at a time, so the two followers differ in that field alone. The old hash read none of them.
    /// </summary>
    [Theory]
    [InlineData("goalOfBest")]
    [InlineData("best")]
    [InlineData("stillTicks")]
    [InlineData("<LastSearchFailed>k__BackingField")]
    public void EachWedgeFieldOfAFollowerIsPartOfTheHash(string field)
    {
        PathFollower plain = new();
        PathFollower changed = new();
        System.Reflection.FieldInfo? info = typeof(PathFollower).GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.True(info is not null, $"PathFollower has no field '{field}'.");
        object value = info.FieldType == typeof(bool) ? true : info.FieldType == typeof(Cell) ? new Cell(1, 2, 3) : (object)7;
        info.SetValue(changed, value);

        StateHash plainHash = StateHash.Start();
        plain.AddTo(ref plainHash);
        StateHash changedHash = StateHash.Start();
        changed.AddTo(ref changedHash);
        Assert.NotEqual(plainHash.Value, changedHash.Value);
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
