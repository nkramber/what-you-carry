using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Simulation;

/// <summary>
/// The input of one tick (D-74, D-77). The frame is the fixed 16 bytes of D-162: the tick, the yaw and pitch
/// deltas in hundredths of a degree, the movement, the buttons, and a CRC-32 over the twelve bytes before it.
/// </summary>
/// <remarks>
/// <para>
/// Every multi-byte field is little-endian, declared here and the same on every platform. The Game layer applies
/// the sensitivity before it quantizes the deltas, so Core reads whole hundredths and never a float (D-77).
/// </para>
/// <para>
/// The checksum makes a flipped bit an error that names its frame. A frame is exactly 16 bytes, so a record needs
/// no length prefix, and a torn tail is the bytes after the last multiple of 16 (D-152, D-226).
/// </para>
/// </remarks>
public readonly record struct Intent(uint Tick, short YawDelta, short PitchDelta, sbyte MoveX, sbyte MoveY, ushort Buttons)
{
    /// <summary>The count of bytes in one frame (D-162).</summary>
    public const int FrameSize = 16;

    /// <summary>The offset of the checksum inside a frame. The checksum covers every byte before it.</summary>
    public const int ChecksumOffset = 12;

    /// <summary>The frame of this intent, with its checksum.</summary>
    public byte[] Encode()
    {
        byte[] frame = new byte[FrameSize];
        frame[0] = (byte)this.Tick;
        frame[1] = (byte)(this.Tick >> 8);
        frame[2] = (byte)(this.Tick >> 16);
        frame[3] = (byte)(this.Tick >> 24);
        frame[4] = (byte)this.YawDelta;
        frame[5] = (byte)(this.YawDelta >> 8);
        frame[6] = (byte)this.PitchDelta;
        frame[7] = (byte)(this.PitchDelta >> 8);
        frame[8] = (byte)this.MoveX;
        frame[9] = (byte)this.MoveY;
        frame[10] = (byte)this.Buttons;
        frame[11] = (byte)(this.Buttons >> 8);

        uint checksum = Crc32.Of(frame, 0, ChecksumOffset);
        frame[12] = (byte)checksum;
        frame[13] = (byte)(checksum >> 8);
        frame[14] = (byte)(checksum >> 16);
        frame[15] = (byte)(checksum >> 24);
        return frame;
    }

    /// <summary>The intent of the frame that starts at <paramref name="offset"/>.</summary>
    /// <exception cref="ContextException">The bytes end inside the frame, or the checksum does not match the frame.</exception>
    public static Intent Decode(IReadOnlyList<byte> bytes, int offset)
    {
        if (offset < 0 || bytes.Count - offset < FrameSize)
        {
            ContextException tooShort = new($"An intent frame needs {FrameSize} bytes, and the bytes hold {bytes.Count - offset} from the offset {offset}.");
            tooShort.AddContext("offset", ((long)offset).ToString(CultureInfo.InvariantCulture));
            tooShort.AddContext("bytes", ((long)bytes.Count).ToString(CultureInfo.InvariantCulture));
            throw tooShort;
        }

        uint tick = ReadUInt32(bytes, offset);
        uint stored = ReadUInt32(bytes, offset + ChecksumOffset);
        uint computed = Crc32.Of(bytes, offset, ChecksumOffset);

        // A checksum that differs means the bytes changed after the recorder wrote them. The frame is not an
        // intent then, and a replay that took it would diverge in silence (T-2, D-162).
        if (stored != computed)
        {
            ContextException corrupt = new($"The intent frame at tick {tick} fails its checksum. The frame holds {Hex(stored)}, and its bytes give {Hex(computed)}.");
            corrupt.AddContext("tick", ((long)tick).ToString(CultureInfo.InvariantCulture));
            corrupt.AddContext("storedChecksum", Hex(stored));
            corrupt.AddContext("computedChecksum", Hex(computed));
            throw corrupt;
        }

        return new Intent(
            tick,
            (short)(bytes[offset + 4] | (bytes[offset + 5] << 8)),
            (short)(bytes[offset + 6] | (bytes[offset + 7] << 8)),
            (sbyte)bytes[offset + 8],
            (sbyte)bytes[offset + 9],
            (ushort)(bytes[offset + 10] | (bytes[offset + 11] << 8)));
    }

    /// <summary>One little-endian 32-bit word.</summary>
    private static uint ReadUInt32(IReadOnlyList<byte> bytes, int offset)
    {
        return bytes[offset]
            | ((uint)bytes[offset + 1] << 8)
            | ((uint)bytes[offset + 2] << 16)
            | ((uint)bytes[offset + 3] << 24);
    }

    /// <summary>A checksum as eight lowercase hexadecimal digits with a 0x prefix.</summary>
    private static string Hex(uint value)
    {
        return "0x" + ((ulong)value).ToString("x8", CultureInfo.InvariantCulture);
    }
}
