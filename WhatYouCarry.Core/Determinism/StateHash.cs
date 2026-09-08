using System;
using System.Globalization;

namespace WhatYouCarry.Core.Determinism;

/// <summary>
/// The FNV-1a 64 hash of a simulation state (D-160). A caller adds each field in a fixed declared order, and
/// the hash of that order is the identity of the state.
/// </summary>
/// <remarks>
/// <para>
/// The hash reads the raw bit pattern of each field, never a printed form, so it needs no formatting and it
/// gives the same bits on every platform. Two floats that compare equal but hold different bits, such as
/// positive zero and negative zero, hash differently. That is the contract: the state is the bits.
/// </para>
/// <para>
/// The order of the calls is part of the hash. FNV-1a folds each byte into the running value, so a state that
/// adds the same fields in another order gives another hash. `StateHashOrderIsPartOfTheContract` asserts it.
/// </para>
/// </remarks>
public struct StateHash : IEquatable<StateHash>
{
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    private ulong value;
    private bool started;

    private StateHash(ulong value)
    {
        this.value = value;
        this.started = true;
    }

    /// <summary>The hash so far. It is the identity of every field added up to this point.</summary>
    /// <exception cref="InvalidOperationException">The hash comes from <c>default</c> and not from <see cref="Start"/>.</exception>
    public readonly ulong Value
    {
        get
        {
            this.EnsureStarted();
            return this.value;
        }
    }

    /// <summary>A new hash with no field in it.</summary>
    public static StateHash Start()
    {
        return new StateHash(OffsetBasis);
    }

    /// <summary>Adds an unsigned 64-bit field.</summary>
    public void Add(ulong field)
    {
        // Little-endian byte order, declared here and the same on every platform.
        for (int shift = 0; shift < 64; shift += 8)
        {
            this.AddByte((byte)(field >> shift));
        }
    }

    /// <summary>Adds an unsigned 32-bit field.</summary>
    public void Add(uint field)
    {
        for (int shift = 0; shift < 32; shift += 8)
        {
            this.AddByte((byte)(field >> shift));
        }
    }

    /// <summary>Adds a signed 64-bit field, through its two's complement bits.</summary>
    public void Add(long field)
    {
        this.Add((ulong)field);
    }

    /// <summary>Adds a signed 32-bit field, through its two's complement bits.</summary>
    public void Add(int field)
    {
        this.Add((uint)field);
    }

    /// <summary>Adds a float field, through its raw IEEE bits.</summary>
    public void Add(float field)
    {
        this.Add(BitConverter.SingleToUInt32Bits(field));
    }

    /// <summary>Adds a boolean field as one byte, 1 for true and 0 for false.</summary>
    public void Add(bool field)
    {
        this.AddByte(field ? (byte)1 : (byte)0);
    }

    /// <summary>Adds a byte field.</summary>
    public void Add(byte field)
    {
        this.AddByte(field);
    }

    /// <inheritdoc/>
    /// <remarks>A hash that `default` made equals no hash that <see cref="Start"/> made, and this never throws.</remarks>
    public readonly bool Equals(StateHash other)
    {
        return this.value == other.value && this.started == other.started;
    }

    /// <inheritdoc/>
    public readonly override bool Equals(object? other)
    {
        return other is StateHash hash && this.Equals(hash);
    }

    /// <inheritdoc/>
    /// <remarks>This reads the field and never throws, because a dictionary may hold a hash that `default` made.</remarks>
    public readonly override int GetHashCode()
    {
        return (int)this.value ^ (int)(this.value >> 32);
    }

    /// <summary>The hash as 16 hexadecimal digits. This is the form that the bit-identity job compares.</summary>
    /// <exception cref="InvalidOperationException">The hash comes from <c>default</c> and not from <see cref="Start"/>.</exception>
    public readonly override string ToString()
    {
        this.EnsureStarted();
        return this.value.ToString("x16", CultureInfo.InvariantCulture);
    }

    /// <summary>One FNV-1a step: exclusive-or the byte into the value, then multiply by the prime.</summary>
    private void AddByte(byte part)
    {
        this.EnsureStarted();
        this.value ^= part;
        this.value *= Prime;
    }

    /// <summary>
    /// Stops a hash that <c>default</c> made. Its value is zero, and FNV-1a starts from the offset basis, so
    /// it would give a stable number that is not the FNV-1a hash of anything. An absent value is an error and
    /// never a zero (T-2, D-160, F-63).
    /// </summary>
    private readonly void EnsureStarted()
    {
        if (!this.started)
        {
            throw new InvalidOperationException("This StateHash comes from `default`, which holds no FNV-1a offset basis. Make one with StateHash.Start().");
        }
    }
}
