using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureTrace;

/// <summary>One pixel of a screenshot: three sRGB bytes and the opacity.</summary>
public readonly record struct ScreenshotPixel(byte Red, byte Green, byte Blue, byte Alpha);

/// <summary>A screenshot as the trace reads it: the size, and one pixel for each place, row by row from the top left corner.</summary>
public sealed record ScreenshotImage(int Width, int Height, ScreenshotPixel[] Pixels);

/// <summary>
/// Reads the PNG screenshot of a look reference for the trace of D-612. The reader takes 8 bits for each channel, the
/// color type RGB or RGBA, no interlace, and each of the five row filters. It checks every chunk checksum. It skips an
/// ancillary chunk, such as a color profile or a pixel size, because a screenshot tool writes them and the trace reads
/// the bytes alone. Any other form is an error that names the file and what the reader saw.
/// </summary>
public static class ScreenshotPng
{
    private const byte TrueColor = 2;
    private const byte TrueColorAlpha = 6;

    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];

    /// <summary>The image of one file.</summary>
    /// <param name="path">The path of the file, for an error.</param>
    /// <param name="file">The bytes of the file.</param>
    /// <exception cref="ContextException">The file is not a PNG of a form that the reader takes.</exception>
    public static ScreenshotImage Read(string path, byte[] file)
    {
        for (int index = 0; index < Signature.Length; index++)
        {
            if (index >= file.Length || file[index] != Signature[index])
            {
                throw Error(path, $"the PNG signature differs at byte {Text(index)}");
            }
        }

        int offset = Signature.Length;
        int width = 0;
        int height = 0;
        byte colorType = 0;
        bool headerRead = false;
        using MemoryStream compressed = new();
        bool ended = false;
        while (!ended)
        {
            if (offset + 12 > file.Length)
            {
                throw Error(path, $"the file ends inside a chunk at byte {Text(offset)}");
            }

            int length = ReadInt(file, offset);
            string type = Encoding.ASCII.GetString(file, offset + 4, 4);
            if (length < 0 || offset + 12 + length > file.Length)
            {
                throw Error(path, $"the chunk '{type}' at byte {Text(offset)} has the length {Text(length)}, which passes the end of the file");
            }

            uint stored = (uint)ReadInt(file, offset + 8 + length);
            uint actual = Crc32.Of(file, offset + 4, 4 + length);
            if (stored != actual)
            {
                throw Error(path, $"the chunk '{type}' at byte {Text(offset)} stores the checksum {stored:x8}, and its bytes give {actual:x8}");
            }

            int data = offset + 8;
            switch (type)
            {
                case "IHDR":
                    width = ReadInt(file, data);
                    height = ReadInt(file, data + 4);
                    colorType = file[data + 9];
                    bool known = file[data + 8] == 8 && (colorType == TrueColor || colorType == TrueColorAlpha) && file[data + 10] == 0 && file[data + 11] == 0 && file[data + 12] == 0;
                    if (!known || width <= 0 || height <= 0)
                    {
                        throw Error(path, $"the header gives {Text(width)} by {Text(height)} pixels, the bit depth {Text(file[data + 8])}, the color type {Text(colorType)}, and the interlace {Text(file[data + 12])}, and the trace reads 8 bits of RGB or RGBA with no interlace. Save the screenshot again as a PNG of that form");
                    }

                    headerRead = true;
                    break;
                case "IDAT":
                    compressed.Write(file, data, length);
                    break;
                case "IEND":
                    ended = true;
                    break;
                default:
                    // The fifth bit of the first letter marks an ancillary chunk, which a reader can skip (PNG section 5.4).
                    if ((file[offset + 4] & 0x20) == 0)
                    {
                        throw Error(path, $"the file holds the critical chunk '{type}', which the trace does not read");
                    }

                    break;
            }

            offset += 12 + length;
        }

        if (!headerRead)
        {
            throw Error(path, "the file has no header chunk");
        }

        int bytesPerPixel = colorType == TrueColor ? 3 : 4;
        byte[] rows = Inflate(path, compressed);
        int rowBytes = 1 + (width * bytesPerPixel);
        if (rows.Length != (long)rowBytes * height)
        {
            throw Error(path, $"the image data holds {Text(rows.Length)} bytes, and {Text(width)} by {Text(height)} pixels of {Text(bytesPerPixel)} bytes need {Text(rowBytes * height)}");
        }

        Unfilter(path, rows, rowBytes, height, bytesPerPixel);
        ScreenshotPixel[] pixels = new ScreenshotPixel[width * height];
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                int at = (row * rowBytes) + 1 + (column * bytesPerPixel);
                byte alpha = bytesPerPixel == 4 ? rows[at + 3] : byte.MaxValue;
                pixels[(row * width) + column] = new ScreenshotPixel(rows[at], rows[at + 1], rows[at + 2], alpha);
            }
        }

        return new ScreenshotImage(width, height, pixels);
    }

    private static byte[] Inflate(string path, MemoryStream compressed)
    {
        compressed.Position = 0;
        try
        {
            using ZLibStream inflater = new(compressed, CompressionMode.Decompress);
            using MemoryStream raw = new();
            inflater.CopyTo(raw);
            return raw.ToArray();
        }
        catch (InvalidDataException error)
        {
            throw Error(path, $"the image data does not inflate: {error.Message}");
        }
    }

    /// <summary>
    /// Undoes the filter of each row in place (PNG section 9.2). Each row starts with its filter type: none, sub, up,
    /// average, or Paeth. A filter works on bytes, and it reads the byte one pixel to the left and the byte above.
    /// </summary>
    private static void Unfilter(string path, byte[] rows, int rowBytes, int height, int bytesPerPixel)
    {
        for (int row = 0; row < height; row++)
        {
            int start = row * rowBytes;
            byte filter = rows[start];
            if (filter > 4)
            {
                throw Error(path, $"the row {Text(row)} has the filter type {Text(filter)}, and a PNG row filter is 0 to 4");
            }

            for (int index = 1; index < rowBytes; index++)
            {
                int left = index > bytesPerPixel ? rows[start + index - bytesPerPixel] : 0;
                int up = row > 0 ? rows[start - rowBytes + index] : 0;
                int upLeft = row > 0 && index > bytesPerPixel ? rows[start - rowBytes + index - bytesPerPixel] : 0;
                int predictor = filter switch
                {
                    1 => left,
                    2 => up,
                    3 => (left + up) / 2,
                    4 => Paeth(left, up, upLeft),
                    _ => 0,
                };
                rows[start + index] = (byte)(rows[start + index] + predictor);
            }
        }
    }

    /// <summary>The Paeth predictor: of the left, the up, and the up-left byte, the one nearest to left + up - upLeft.</summary>
    private static int Paeth(int left, int up, int upLeft)
    {
        int estimate = left + up - upLeft;
        int toLeft = Math.Abs(estimate - left);
        int toUp = Math.Abs(estimate - up);
        int toUpLeft = Math.Abs(estimate - upLeft);
        if (toLeft <= toUp && toLeft <= toUpLeft)
        {
            return left;
        }

        return toUp <= toUpLeft ? up : upLeft;
    }

    private static ContextException Error(string path, string reason)
    {
        return new ContextException($"The screenshot '{path}' cannot be traced: {reason}.");
    }

    private static int ReadInt(byte[] bytes, int offset)
    {
        return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
