using System;
using System.Collections.Generic;

namespace WhatYouCarry.Core.Determinism;

/// <summary>
/// The CRC-32 checksum of a byte range (D-162, D-224). Each intent frame ends with one, so a flipped bit in a
/// run record is an error that names its frame, and never a wrong intent that replays in silence (T-2).
/// </summary>
/// <remarks>
/// <para>
/// This is the CRC-32 of IEEE 802.3 in its reflected form: the polynomial 0xEDB88320, an initial value of all
/// ones, and a final complement. The text "123456789" gives 0xCBF43926, and `Crc32KnownAnswer` pins it.
/// </para>
/// <para>
/// The table and every step are integer operations, so two platforms give one checksum. Core holds the function
/// itself, because the platform class that computes one lives in a package that G-16 needs a decision for, and
/// the function is thirty lines (D-224).
/// </para>
/// </remarks>
public static class Crc32
{
    private const uint Polynomial = 0xEDB88320U;
    private const uint AllOnes = 0xFFFFFFFFU;

    private static readonly uint[] Table = MakeTable();

    /// <summary>The checksum of <paramref name="count"/> bytes from <paramref name="offset"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The range does not fit inside the bytes.</exception>
    public static uint Of(IReadOnlyList<byte> bytes, int offset, int count)
    {
        // The two checks read the count against the room that is left, so the sum never overflows.
        if (offset < 0 || offset > bytes.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), $"The offset must be inside the bytes. The offset is {offset}, and the bytes hold {bytes.Count}.");
        }

        if (count < 0 || count > bytes.Count - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(count), $"The count must fit inside the bytes. The count is {count}, the offset is {offset}, and the bytes hold {bytes.Count}.");
        }

        uint value = AllOnes;
        for (int index = offset; index < offset + count; index++)
        {
            value = Table[(value ^ bytes[index]) & 0xFFU] ^ (value >> 8);
        }

        return value ^ AllOnes;
    }

    /// <summary>The 256 partial checksums, one for each byte value. Each entry is eight shift-and-fold steps.</summary>
    private static uint[] MakeTable()
    {
        uint[] table = new uint[256];
        for (uint index = 0; index < 256; index++)
        {
            uint entry = index;
            for (int bit = 0; bit < 8; bit++)
            {
                entry = (entry & 1U) != 0 ? (entry >> 1) ^ Polynomial : entry >> 1;
            }

            table[index] = entry;
        }

        return table;
    }
}
