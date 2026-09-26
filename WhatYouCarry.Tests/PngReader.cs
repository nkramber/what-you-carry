using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Tools.TextureGen;

namespace WhatYouCarry.Tests;

/// <summary>A PNG as a test reads it: the size, and one color per pixel.</summary>
internal sealed record PngImage(int Width, int Height, AtlasColor[] Pixels);

/// <summary>
/// A strict reader of the truecolor PNG that the texture generator writes (D-598), so a test reads the file and not
/// the buffer of the generator. It checks the signature and every chunk checksum, reads the header and the image data,
/// and inflates the data with the platform decompressor. Every row must carry filter type
/// zero. Anything else is an error that names what the reader saw.
/// </summary>
internal static class PngReader
{

    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];

    /// <summary>The image of one file.</summary>
    /// <exception cref="InvalidDataException">The file is not a PNG of a form that the generator writes or wrote.</exception>
    public static PngImage Read(byte[] file)
    {
        for (int index = 0; index < Signature.Length; index++)
        {
            if (index >= file.Length || file[index] != Signature[index])
            {
                throw new InvalidDataException($"The PNG signature differs at byte {index}.");
            }
        }

        int offset = Signature.Length;
        int width = 0;
        int height = 0;
        bool headerRead = false;
        using MemoryStream compressed = new();
        bool ended = false;
        while (!ended)
        {
            if (offset + 12 > file.Length)
            {
                throw new InvalidDataException($"The file ends inside a chunk at byte {offset}.");
            }

            int length = ReadInt(file, offset);
            string type = Encoding.ASCII.GetString(file, offset + 4, 4);
            if (length < 0 || offset + 12 + length > file.Length)
            {
                throw new InvalidDataException($"The chunk '{type}' at byte {offset} has the length {length}, which passes the end of the file.");
            }

            uint stored = (uint)ReadInt(file, offset + 8 + length);
            uint actual = Crc32.Of(file, offset + 4, 4 + length);
            if (stored != actual)
            {
                throw new InvalidDataException($"The chunk '{type}' at byte {offset} stores the checksum {stored:x8}, and its bytes give {actual:x8}.");
            }

            int data = offset + 8;
            switch (type)
            {
                case "IHDR":
                    width = ReadInt(file, data);
                    height = ReadInt(file, data + 4);
                    string form = $"{file[data + 8]} {file[data + 9]} {file[data + 10]} {file[data + 11]} {file[data + 12]}";
                    if (form != "8 2 0 0 0")
                    {
                        throw new InvalidDataException($"The header gives the bit depth, color type, compression, filter, and interlace '{form}', and the generator writes '8 2 0 0 0'.");
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
                    throw new InvalidDataException($"The file holds the chunk '{type}', which the generator never writes.");
            }

            offset += 12 + length;
        }

        if (offset != file.Length)
        {
            throw new InvalidDataException($"The file holds {file.Length - offset} bytes after the end chunk.");
        }

        if (!headerRead)
        {
            throw new InvalidDataException("The file has no header chunk.");
        }

        compressed.Position = 0;
        using ZLibStream inflater = new(compressed, CompressionMode.Decompress);
        using MemoryStream raw = new();
        inflater.CopyTo(raw);
        byte[] rows = raw.ToArray();
        const int bytesPerPixel = 3;
        int rowBytes = 1 + (width * bytesPerPixel);
        if (rows.Length != rowBytes * height)
        {
            throw new InvalidDataException($"The image data holds {rows.Length} bytes, and {width} by {height} pixels of {bytesPerPixel} bytes need {rowBytes * height}.");
        }

        AtlasColor[] pixels = new AtlasColor[width * height];
        for (int row = 0; row < height; row++)
        {
            int start = row * rowBytes;
            if (rows[start] != 0)
            {
                throw new InvalidDataException($"The row {row} has the filter type {rows[start]}, and the generator writes type 0.");
            }

            for (int column = 0; column < width; column++)
            {
                int at = start + 1 + (column * bytesPerPixel);
                pixels[(row * width) + column] = new AtlasColor(rows[at], rows[at + 1], rows[at + 2]);
            }
        }

        return new PngImage(width, height, pixels);
    }

    private static int ReadInt(byte[] bytes, int offset)
    {
        return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
    }
}
