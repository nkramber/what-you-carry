using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WhatYouCarry.Core.Determinism;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>
/// Writes a truecolor PNG (D-305, D-598): three sRGB bytes per pixel, red, green, and blue, and the image data in
/// stored deflate blocks.
/// </summary>
/// <remarks>
/// A stored block copies the bytes and compresses nothing. The file then has one byte form on every platform, which
/// a compressor of the platform does not promise, so the committed atlas can equal the generator output byte for
/// byte on Linux, macOS, and Windows. The atlas of 512 by 512 pixels gives a file of about 790 kilobytes. The chunk
/// checksum is the CRC-32 of Core, which is the checksum that the PNG format names.
/// </remarks>
public static class PngWriter
{
    /// <summary>The most bytes in one stored deflate block.</summary>
    public const int MaxStoredBlock = 65535;

    /// <summary>The eight bytes that start every PNG file.</summary>
    public static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];

    private const byte BitDepth = 8;
    private const byte TrueColor = 2;
    private const int BytesPerPixel = 3;
    private const byte NoFilter = 0;
    private const byte ZlibMethod = 0x78;
    private const byte ZlibFlags = 0x01;
    private const uint AdlerModulus = 65521;

    /// <summary>The file of one image: the size, and one color per pixel, row by row.</summary>
    /// <exception cref="ArgumentException">The size is not positive, or the pixel count differs from the size.</exception>
    public static byte[] Write(int width, int height, AtlasColor[] pixels)
    {
        CheckImage(width, height, pixels);
        List<byte> file = [.. Signature];
        AddChunk(file, "IHDR", Header(width, height));
        AddChunk(file, "IDAT", StoredZlib(Scanlines(width, height, pixels)));
        AddChunk(file, "IEND", []);
        return [.. file];
    }

    /// <summary>The size and the pixel count must agree.</summary>
    private static void CheckImage(int width, int height, AtlasColor[] pixels)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException($"The image size must be positive. The size is {Text(width)} by {Text(height)}.");
        }

        if ((long)pixels.Length != (long)width * height)
        {
            throw new ArgumentException($"The image holds {Text(pixels.Length)} pixels, and a size of {Text(width)} by {Text(height)} needs {((long)width * height).ToString(CultureInfo.InvariantCulture)}.", nameof(pixels));
        }
    }

    /// <summary>The header chunk data: the size, 8 bits per channel, the truecolor type, and no interlace.</summary>
    private static byte[] Header(int width, int height)
    {
        List<byte> header = [];
        AddBigEndian(header, (uint)width);
        AddBigEndian(header, (uint)height);
        header.Add(BitDepth);
        header.Add(TrueColor);
        header.Add(0);
        header.Add(0);
        header.Add(0);
        return [.. header];
    }

    /// <summary>Each row of red, green, and blue bytes after a filter byte of zero, which is no filter.</summary>
    private static byte[] Scanlines(int width, int height, AtlasColor[] pixels)
    {
        int rowBytes = 1 + (width * BytesPerPixel);
        byte[] data = new byte[rowBytes * height];
        for (int row = 0; row < height; row++)
        {
            int start = row * rowBytes;
            data[start] = NoFilter;
            for (int column = 0; column < width; column++)
            {
                AtlasColor color = pixels[(row * width) + column];
                int at = start + 1 + (column * BytesPerPixel);
                data[at] = color.Red;
                data[at + 1] = color.Green;
                data[at + 2] = color.Blue;
            }
        }

        return data;
    }

    /// <summary>A zlib stream of stored deflate blocks: the zlib header, the blocks, and the Adler-32 checksum of the data.</summary>
    private static byte[] StoredZlib(byte[] data)
    {
        List<byte> stream = [ZlibMethod, ZlibFlags];
        int offset = 0;
        bool final = false;
        while (!final)
        {
            int length = Math.Min(MaxStoredBlock, data.Length - offset);
            final = offset + length == data.Length;
            int inverse = ~length & 0xFFFF;
            stream.Add(final ? (byte)1 : (byte)0);
            stream.Add((byte)(length & 0xFF));
            stream.Add((byte)(length >> 8));
            stream.Add((byte)(inverse & 0xFF));
            stream.Add((byte)(inverse >> 8));
            for (int index = 0; index < length; index++)
            {
                stream.Add(data[offset + index]);
            }

            offset += length;
        }

        AddBigEndian(stream, Adler32(data));
        return [.. stream];
    }

    /// <summary>The Adler-32 checksum that ends a zlib stream: two sums modulo 65521.</summary>
    private static uint Adler32(byte[] data)
    {
        uint low = 1;
        uint high = 0;
        foreach (byte value in data)
        {
            low = (low + value) % AdlerModulus;
            high = (high + low) % AdlerModulus;
        }

        return (high << 16) | low;
    }

    /// <summary>One chunk: the data length, the type, the data, and the CRC-32 of the type and the data.</summary>
    private static void AddChunk(List<byte> file, string type, byte[] data)
    {
        List<byte> typeAndData = [.. Encoding.ASCII.GetBytes(type), .. data];
        AddBigEndian(file, (uint)data.Length);
        file.AddRange(typeAndData);
        AddBigEndian(file, Crc32.Of(typeAndData, 0, typeAndData.Count));
    }

    private static void AddBigEndian(List<byte> bytes, uint value)
    {
        bytes.Add((byte)(value >> 24));
        bytes.Add((byte)(value >> 16));
        bytes.Add((byte)(value >> 8));
        bytes.Add((byte)value);
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
