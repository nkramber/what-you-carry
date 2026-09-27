using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Replay;

/// <summary>
/// The header of a run record (D-151). The record is this header as one UTF-8 JSON line, then the fixed frames
/// of D-162 (D-163).
/// </summary>
/// <remarks>
/// The initial state of D-151 holds the loadout items with their rolls, the tree state, and the amulet
/// assignment. None of those types exists in Phase 1, so the header writes an empty loadout, an empty tree, and
/// no amulet, and this type carries no field for them yet. The schema is complete: a Phase 3 record adds items to
/// the same names, and an older reader then fails on a list with an item, and never on an absent name (D-229).
/// The line ends with a CRC-32 of the bytes before it, so a changed bit of the header is an error and never another
/// seed (D-637).
/// </remarks>
public sealed record RunRecordHeader(int FormatVersion, int SimulationVersion, string ContentHash, ulong Seed);

/// <summary>
/// Writes and reads the header line of a run record (D-151, D-163). The frames after it are the work of
/// <see cref="RunRecorder"/> and <see cref="RunReplayer"/>.
/// </summary>
public static class RunRecord
{
    /// <summary>
    /// The version of the record layout. It moves when the header or the frame changes shape. Version 2 adds the CRC-32
    /// of the header (D-637, F-172).
    /// </summary>
    public const int FormatVersion = 2;

    /// <summary>The count of hexadecimal digits in a content hash (D-221).</summary>
    public const int ContentHashLength = 64;

    /// <summary>The name that an error gives the header, in the place where a content error names its file.</summary>
    public const string HeaderName = "run record header";

    /// <summary>The names of the header fields, in the order that the writer puts them.</summary>
    public const string FormatVersionName = "formatVersion";
    public const string SimulationVersionName = "simulationVersion";
    public const string ContentHashName = "contentHash";
    public const string SeedName = "seed";
    public const string LoadoutName = "loadout";
    public const string TreeName = "tree";
    public const string AmuletName = "amulet";
    public const string HeaderCrcName = "headerCrc";

    /// <summary>The bytes that start the CRC field, the last field of the line. The CRC covers each byte before them.</summary>
    private const string HeaderCrcStart = ",\"" + HeaderCrcName + "\":";

    /// <summary>The names that a header must carry (D-151).</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        FormatVersionName,
        SimulationVersionName,
        ContentHashName,
        SeedName,
        LoadoutName,
        TreeName,
        AmuletName,
        HeaderCrcName,
    ];

    /// <summary>The names that a header can carry beyond the required ones. There are none.</summary>
    public static readonly IReadOnlyList<string> Optional = [];

    /// <summary>A header for a new run of this build.</summary>
    /// <exception cref="ContextException">The content hash is not 64 lowercase hexadecimal digits.</exception>
    public static RunRecordHeader NewHeader(string contentHash, ulong seed)
    {
        if (!IsContentHash(contentHash))
        {
            ContextException error = new($"A content hash is {ContentHashLength} lowercase hexadecimal digits (D-221), and this one is '{contentHash}'.");
            error.AddContext("contentHash", contentHash);
            throw error;
        }

        return new RunRecordHeader(FormatVersion, Simulation.SimulationVersion.Value, contentHash, seed);
    }

    /// <summary>The header as one UTF-8 JSON line, with its line break.</summary>
    /// <exception cref="ContextException">The content hash is not 64 lowercase hexadecimal digits.</exception>
    public static byte[] WriteHeader(RunRecordHeader header)
    {
        // The hash goes inside quotation marks with no escape pass, so it must hold nothing that JSON reserves.
        if (!IsContentHash(header.ContentHash))
        {
            ContextException error = new($"A content hash is {ContentHashLength} lowercase hexadecimal digits (D-221), and this header holds '{header.ContentHash}'.");
            error.AddContext("contentHash", header.ContentHash);
            throw error;
        }

        StringBuilder text = new();
        text.Append("{\"");
        text.Append(FormatVersionName);
        text.Append("\":");
        text.Append(((long)header.FormatVersion).ToString(CultureInfo.InvariantCulture));
        text.Append(",\"");
        text.Append(SimulationVersionName);
        text.Append("\":");
        text.Append(((long)header.SimulationVersion).ToString(CultureInfo.InvariantCulture));
        text.Append(",\"");
        text.Append(ContentHashName);
        text.Append("\":\"");
        text.Append(header.ContentHash);
        text.Append("\",\"");
        text.Append(SeedName);
        text.Append("\":");
        text.Append(header.Seed.ToString(CultureInfo.InvariantCulture));
        text.Append(",\"");
        text.Append(LoadoutName);
        text.Append("\":[],\"");
        text.Append(TreeName);
        text.Append("\":[],\"");
        text.Append(AmuletName);
        text.Append("\":null");

        // The CRC covers every byte of the line before its own field (D-637).
        byte[] body = Encoding.UTF8.GetBytes(text.ToString());
        uint crc = Crc32.Of(body, 0, body.Length);
        text.Append(HeaderCrcStart);
        text.Append(((long)crc).ToString(CultureInfo.InvariantCulture));
        text.Append("}\n");
        return Encoding.UTF8.GetBytes(text.ToString());
    }

    /// <summary>
    /// The header of a record, and the offset of the first frame after it. The format version is checked first,
    /// so a record of another layout fails on the version and not on a field that the layout does not hold.
    /// </summary>
    /// <exception cref="ContextException">The record holds no complete header line, a version does not match this build, or a field is absent, unknown, or of another kind.</exception>
    public static (RunRecordHeader Header, int BodyStart) ReadHeader(IReadOnlyList<byte> record)
    {
        int lineEnd = -1;
        for (int index = 0; index < record.Count; index++)
        {
            if (record[index] == (byte)'\n')
            {
                lineEnd = index;
                break;
            }
        }

        // A record without a header line names no version and no seed, so nothing can replay it.
        if (lineEnd < 0)
        {
            ContextException noHeader = new("The run record holds no complete header line, so nothing can replay it.");
            noHeader.AddContext("bytes", ((long)record.Count).ToString(CultureInfo.InvariantCulture));
            throw noHeader;
        }

        byte[] line = new byte[lineEnd];
        for (int index = 0; index < lineEnd; index++)
        {
            line[index] = record[index];
        }

        IReadOnlyList<JsonMember> members = JsonObjectReader.Read(HeaderName, line);

        long formatVersion = Number(members, FormatVersionName);
        if (formatVersion != FormatVersion)
        {
            ContextException mismatch = new($"The run record has format version {formatVersion}, and this build reads format version {FormatVersion}.");
            mismatch.AddContext("recordFormatVersion", formatVersion.ToString(CultureInfo.InvariantCulture));
            mismatch.AddContext("buildFormatVersion", ((long)FormatVersion).ToString(CultureInfo.InvariantCulture));
            throw mismatch;
        }

        CheckHeaderCrc(line, members);
        ContentValidator.Check(HeaderName, members, Required, Optional);

        long simulationVersion = Number(members, SimulationVersionName);
        if (simulationVersion != Simulation.SimulationVersion.Value)
        {
            ContextException mismatch = new($"The run record comes from simulation version {simulationVersion}, and this build is simulation version {Simulation.SimulationVersion.Value}. A replay is exact only on a match (D-151).");
            mismatch.AddContext("recordSimulationVersion", simulationVersion.ToString(CultureInfo.InvariantCulture));
            mismatch.AddContext("buildSimulationVersion", ((long)Simulation.SimulationVersion.Value).ToString(CultureInfo.InvariantCulture));
            throw mismatch;
        }

        string contentHash = ContentValidator.Value(HeaderName, members, ContentHashName, JsonMemberKind.Text);
        if (!IsContentHash(contentHash))
        {
            throw ContentError.Make(HeaderName, ContentHashName, $"must hold {ContentHashLength} lowercase hexadecimal digits (D-221), and it holds '{contentHash}'");
        }

        string seedText = ContentValidator.Value(HeaderName, members, SeedName, JsonMemberKind.Number);
        if (!ulong.TryParse(seedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong seed))
        {
            throw ContentError.Make(HeaderName, SeedName, "holds a number that does not fit an unsigned 64-bit seed");
        }

        // Phase 1 has no item, no tree node, and no amulet. A value of another kind here comes from a later
        // format, and the reader must say so and never step over it (D-229).
        ContentValidator.Value(HeaderName, members, LoadoutName, JsonMemberKind.EmptyList);
        ContentValidator.Value(HeaderName, members, TreeName, JsonMemberKind.EmptyList);
        ContentValidator.Value(HeaderName, members, AmuletName, JsonMemberKind.Null);

        return (new RunRecordHeader((int)formatVersion, (int)simulationVersion, contentHash, seed), lineEnd + 1);
    }

    /// <summary>
    /// Compares the CRC field with the CRC-32 of the bytes of the line before it (D-637, F-172). A flipped bit in the seed
    /// read as another seed, and the record then replayed another run with no error.
    /// </summary>
    /// <exception cref="ContextException">The field is absent, is not the last field, or does not match. The error names the header.</exception>
    private static void CheckHeaderCrc(byte[] line, IReadOnlyList<JsonMember> members)
    {
        int at = LastIndexOf(line, Encoding.UTF8.GetBytes(HeaderCrcStart));
        if (at < 0)
        {
            throw ContentError.Make(HeaderName, HeaderCrcName, "is absent, and each header of format version 2 ends with the CRC-32 of its other fields (D-637)");
        }

        long stored = Number(members, HeaderCrcName);
        uint computed = Crc32.Of(line, 0, at);
        if (stored != computed)
        {
            ContextException mismatch = ContentError.Make(HeaderName, HeaderCrcName, $"is {stored}, and the CRC-32 of the header before it is {computed}, so a byte of the header changed after the write (D-637)");
            mismatch.AddContext("storedCrc", stored.ToString(CultureInfo.InvariantCulture));
            mismatch.AddContext("computedCrc", ((long)computed).ToString(CultureInfo.InvariantCulture));
            throw mismatch;
        }
    }

    /// <summary>The offset of the last place where the part starts in the line, or -1 when the line does not hold it.</summary>
    private static int LastIndexOf(byte[] line, byte[] part)
    {
        for (int index = line.Length - part.Length; index >= 0; index--)
        {
            int matched = 0;
            while (matched < part.Length && line[index + matched] == part[matched])
            {
                matched++;
            }

            if (matched == part.Length)
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>Answers whether the text is 64 lowercase hexadecimal digits, which is the form of D-221.</summary>
    private static bool IsContentHash(string text)
    {
        if (text.Length != ContentHashLength)
        {
            return false;
        }

        for (int index = 0; index < text.Length; index++)
        {
            char letter = text[index];
            bool isDigit = letter >= '0' && letter <= '9';
            bool isLowerHex = letter >= 'a' && letter <= 'f';
            if (!isDigit && !isLowerHex)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>One whole-number field of the header.</summary>
    private static long Number(IReadOnlyList<JsonMember> members, string name)
    {
        string text = ContentValidator.Value(HeaderName, members, name, JsonMemberKind.Number);
        if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
        {
            throw ContentError.Make(HeaderName, name, "holds a number that does not fit a whole number");
        }

        return value;
    }
}
