using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Items;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Replay;

/// <summary>
/// The header of a run record (D-151). The record is this header as one UTF-8 JSON line, then the fixed frames
/// of D-162 (D-163).
/// </summary>
/// <remarks>
/// The initial state of D-151 holds the loadout items with their rolls, the tree state, and the amulet
/// assignment. The loadout holds one entry for each worn item: the item id and its affix ids (D-766). The tree and
/// the amulet have no type yet, so the header writes an empty tree and no amulet, and this type carries no field for
/// them. The schema is complete: a later record adds items to the same names, and an older reader then fails on a
/// list with an item, and never on an absent name (D-229). The line ends with a CRC-32 of the bytes before it, so a
/// changed bit of the header is an error and never another seed (D-637).
/// </remarks>
/// <param name="FormatVersion">The version of the record layout.</param>
/// <param name="SimulationVersion">The simulation version of the build that wrote the record (G-20).</param>
/// <param name="ContentHash">The hash of the content set (D-163).</param>
/// <param name="Seed">The run seed (D-159).</param>
/// <param name="Loadout">The worn items at the start of the run, in equip order (D-762, D-766).</param>
public sealed record RunRecordHeader(int FormatVersion, int SimulationVersion, string ContentHash, ulong Seed, IReadOnlyList<LoadoutEntry> Loadout);

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

    /// <summary>A header for a new run of this build, with no worn item.</summary>
    /// <exception cref="ContextException">The content hash is not 64 lowercase hexadecimal digits.</exception>
    public static RunRecordHeader NewHeader(string contentHash, ulong seed)
    {
        return NewHeader(contentHash, seed, []);
    }

    /// <summary>A header for a new run of this build, with the worn items of a loadout (D-762, D-766).</summary>
    /// <exception cref="ContextException">The content hash is not 64 lowercase hexadecimal digits.</exception>
    public static RunRecordHeader NewHeader(string contentHash, ulong seed, IReadOnlyList<LoadoutEntry> loadout)
    {
        if (!IsContentHash(contentHash))
        {
            ContextException error = new($"A content hash is {ContentHashLength} lowercase hexadecimal digits (D-221), and this one is '{contentHash}'.");
            error.AddContext("contentHash", contentHash);
            throw error;
        }

        return new RunRecordHeader(FormatVersion, Simulation.SimulationVersion.Value, contentHash, seed, loadout);
    }

    /// <summary>The header as one UTF-8 JSON line, with its line break.</summary>
    /// <exception cref="ContextException">The content hash is not 64 lowercase hexadecimal digits, or a loadout id holds a letter outside its form.</exception>
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
        text.Append("\":");
        AppendLoadout(text, header.Loadout);
        text.Append(",\"");
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

        // This build has no tree node and no amulet. A value of another kind here comes from a later format, and the
        // reader must say so and never step over it (D-229).
        IReadOnlyList<LoadoutEntry> loadout = ReadLoadout(members);
        ContentValidator.Value(HeaderName, members, TreeName, JsonMemberKind.EmptyList);
        ContentValidator.Value(HeaderName, members, AmuletName, JsonMemberKind.Null);

        return (new RunRecordHeader((int)formatVersion, (int)simulationVersion, contentHash, seed, loadout), lineEnd + 1);
    }

    /// <summary>
    /// Compares the CRC field with the CRC-32 of the bytes of the line before it (D-637, F-172). A flipped bit in the seed
    /// read as another seed, and the record then replayed another run with no error.
    /// </summary>
    /// <exception cref="ContextException">The field is absent, is not the last field of the line, or does not match. The error names the header.</exception>
    private static void CheckHeaderCrc(byte[] line, IReadOnlyList<JsonMember> members)
    {
        byte[] start = Encoding.UTF8.GetBytes(HeaderCrcStart);
        int at = LastIndexOf(line, start);
        if (at < 0)
        {
            throw ContentError.Make(HeaderName, HeaderCrcName, "is absent, and each header of format version 2 ends with the CRC-32 of its other fields (D-637)");
        }

        // The field ends the line: its digits and the closing brace follow it, and nothing else. A field after it lies
        // outside the CRC, so a changed seed there read as a valid header (PR #109 review P2-1).
        int digits = at + start.Length;
        int close = line.Length - 1;
        bool endsTheLine = close > digits && line[close] == (byte)'}';
        for (int index = digits; index < close && endsTheLine; index++)
        {
            endsTheLine = line[index] >= (byte)'0' && line[index] <= (byte)'9';
        }

        if (!endsTheLine)
        {
            throw ContentError.Make(HeaderName, HeaderCrcName, "is not the last field of the line, and the CRC-32 covers every other field only when it ends the line (D-637)");
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

    /// <summary>The names of the fields of one loadout entry (D-766).</summary>
    public const string ItemName = "item";
    public const string AffixesName = "affixes";

    /// <summary>The names that a loadout entry must carry (D-766).</summary>
    private static readonly IReadOnlyList<string> EntryRequired =
    [
        ItemName,
        AffixesName,
    ];

    /// <summary>The names that a loadout entry can carry beyond the required ones. There are none.</summary>
    private static readonly IReadOnlyList<string> EntryOptional = [];

    /// <summary>
    /// The loadout as a JSON list: one object for each entry, with the item id and the list of affix ids (D-766). The ids
    /// go inside quotation marks with no escape pass, so each one holds lowercase letters, digits, and hyphens alone.
    /// </summary>
    /// <exception cref="ContextException">An id is empty or holds another letter.</exception>
    private static void AppendLoadout(StringBuilder text, IReadOnlyList<LoadoutEntry> loadout)
    {
        text.Append('[');
        for (int index = 0; index < loadout.Count; index++)
        {
            LoadoutEntry entry = loadout[index];
            if (index > 0)
            {
                text.Append(',');
            }

            text.Append("{\"");
            text.Append(ItemName);
            text.Append("\":\"");
            text.Append(LoadoutId(entry.Item));
            text.Append("\",\"");
            text.Append(AffixesName);
            text.Append("\":[");
            for (int affix = 0; affix < entry.Affixes.Count; affix++)
            {
                if (affix > 0)
                {
                    text.Append(',');
                }

                text.Append('"');
                text.Append(LoadoutId(entry.Affixes[affix]));
                text.Append('"');
            }

            text.Append("]}");
        }

        text.Append(']');
    }

    /// <summary>An id of the loadout, which holds lowercase letters, digits, and hyphens alone, and at least one letter.</summary>
    /// <exception cref="ContextException">The id is empty or holds another letter.</exception>
    private static string LoadoutId(string id)
    {
        bool valid = id.Length > 0;
        for (int index = 0; index < id.Length && valid; index++)
        {
            char letter = id[index];
            valid = (letter >= 'a' && letter <= 'z') || (letter >= '0' && letter <= '9') || letter == '-';
        }

        if (!valid)
        {
            ContextException error = new($"A loadout id holds lowercase letters, digits, and hyphens, and this one is '{id}'. The header writes it with no escape pass (D-766).");
            error.AddContext("id", id);
            throw error;
        }

        return id;
    }

    /// <summary>The loadout of the header: an empty list, or a list of entries of D-766.</summary>
    /// <exception cref="ContextException">The loadout is of another kind, or an entry is absent a field, holds an unknown one, or holds one of another kind.</exception>
    private static IReadOnlyList<LoadoutEntry> ReadLoadout(IReadOnlyList<JsonMember> members)
    {
        foreach (JsonMember member in members)
        {
            if (member.Name != LoadoutName)
            {
                continue;
            }

            if (member.Kind == JsonMemberKind.EmptyList)
            {
                return [];
            }

            if (member.Kind != JsonMemberKind.ObjectList || member.Objects is null)
            {
                throw ContentError.Make(HeaderName, LoadoutName, "holds a value that is not a list of loadout entries (D-766)");
            }

            List<LoadoutEntry> loadout = [];
            for (int index = 0; index < member.Objects.Count; index++)
            {
                string field = LoadoutName + "[" + ((long)index).ToString(CultureInfo.InvariantCulture) + "]";
                IReadOnlyList<JsonMember> entry = member.Objects[index];
                ContentValidator.Check(HeaderName, entry, EntryRequired, EntryOptional);
                string item = ContentValidator.Value(HeaderName, entry, ItemName, JsonMemberKind.Text);
                loadout.Add(new LoadoutEntry(item, ReadAffixes(field, entry)));
            }

            return loadout;
        }

        throw ContentError.Make(HeaderName, LoadoutName, "is absent, and this type requires it");
    }

    /// <summary>The affix ids of one loadout entry: an empty list, or a list of text.</summary>
    /// <exception cref="ContextException">The affixes are of another kind.</exception>
    private static IReadOnlyList<string> ReadAffixes(string field, IReadOnlyList<JsonMember> entry)
    {
        foreach (JsonMember member in entry)
        {
            if (member.Name != AffixesName)
            {
                continue;
            }

            if (member.Kind == JsonMemberKind.EmptyList)
            {
                return [];
            }

            if (member.Kind != JsonMemberKind.TextList)
            {
                throw ContentError.Make(HeaderName, field, $"holds '{AffixesName}' that is not a list of affix ids (D-766)");
            }

            return ContentValidator.Texts(member.Value);
        }

        throw ContentError.Make(HeaderName, field, $"holds no '{AffixesName}', and each loadout entry holds one (D-766)");
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
