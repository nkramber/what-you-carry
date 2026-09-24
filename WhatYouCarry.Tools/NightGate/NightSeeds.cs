using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using WhatYouCarry.Core.Bots;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>One inclusive range of seeds, from <see cref="From"/> to <see cref="To"/>.</summary>
public sealed record SeedRange(ulong From, ulong To)
{
    /// <summary>True when the range holds the seed.</summary>
    public bool Holds(ulong seed)
    {
        return seed >= From && seed <= To;
    }

    /// <summary>The range in the form <c>from-to</c>.</summary>
    public override string ToString()
    {
        return $"{From.ToString(CultureInfo.InvariantCulture)}-{To.ToString(CultureInfo.InvariantCulture)}";
    }
}

/// <summary>
/// The seeds of each night (D-564 to D-567). The fixed set stays the gate: seeds 1 to 5000 of each bot policy, and
/// seeds 1 to 100000 of the seed sweep. The night also runs a slice past the fixed range, which the UTC date of the
/// night start selects, the extra fixed seeds of <see cref="ExtraSeedsPath"/>, and the failed seeds that the record of
/// main carries. The rules do no I/O.
/// </summary>
public static class NightSeeds
{
    /// <summary>The name of the seed sweep in a seed list, beside the names of the bot policies.</summary>
    public const string ReachabilitySweep = "reachability";

    /// <summary>The sweeps of each night, in the order of the night steps.</summary>
    public static readonly string[] Sweeps = [RandomWalker.PolicyName, GreedyDescender.PolicyName, FullClearer.PolicyName, TimerTester.PolicyName, Coward.PolicyName, ReachabilitySweep];

    /// <summary>The last fixed seed of each bot policy, and the size of its slice: one tenth (D-566).</summary>
    public const ulong BotFixedTop = 5000;
    public const ulong BotSliceSize = 500;

    /// <summary>The last fixed seed of the seed sweep, and the size of its slice: one tenth (D-566).</summary>
    public const ulong ReachabilityFixedTop = 100000;
    public const ulong ReachabilitySliceSize = 10000;

    /// <summary>Day 0 of the slices (D-566). The slice of a date k days later is the k-th window past the fixed range.</summary>
    public static readonly DateOnly DayZero = new(2026, 9, 24);

    /// <summary>The file of the extra fixed seeds, relative to the checkout (D-567). A fix PR of a slice failure adds its seed here.</summary>
    public const string ExtraSeedsPath = "WhatYouCarry.Tools/NightGate/extra-seeds.json";

    /// <summary>The field of a night record that names the failed seeds of each sweep (D-567).</summary>
    public const string FailedSeedsName = "failedSeeds";

    /// <summary>The field of a night record that names the carried seeds that its night ran, by sweep (D-569).</summary>
    public const string CarriedSeedsName = "carriedSeeds";

    /// <summary>The field of a night record that names the date and the window of the slice of each sweep (D-564).</summary>
    public const string SliceName = "slice";

    /// <summary>The field of the slice that holds its date.</summary>
    public const string SliceDateName = "date";

    /// <summary>The form of a date in the command options and the record.</summary>
    public const string DateFormat = "yyyy-MM-dd";

    /// <summary>The fixed range of a sweep: seeds 1 to its fixed top.</summary>
    /// <exception cref="ArgumentException">The sweep is not one of <see cref="Sweeps"/>.</exception>
    public static SeedRange FixedRange(string sweep)
    {
        return new SeedRange(1, FixedTop(sweep));
    }

    /// <summary>
    /// The slice of a sweep on a date (D-566). For a fixed range that ends at F and a slice size S, the slice of day k
    /// holds the seeds from F + 1 + k × S to F + (k + 1) × S. Each window follows the last, so no seed runs twice.
    /// </summary>
    /// <exception cref="ArgumentException">The sweep is not one of <see cref="Sweeps"/>, or the date comes before <see cref="DayZero"/>.</exception>
    public static SeedRange Slice(string sweep, DateOnly date)
    {
        int day = date.DayNumber - DayZero.DayNumber;
        if (day < 0)
        {
            throw new ArgumentException($"The date {Text(date)} comes before day 0 of the slices, {Text(DayZero)} (D-566).", nameof(date));
        }

        ulong top = FixedTop(sweep);
        ulong size = SliceSize(sweep);
        ulong from = top + 1 + ((ulong)day * size);
        return new SeedRange(from, from + size - 1);
    }

    /// <summary>
    /// The seed list of a sweep on a date: the fixed range, the slice, then each extra seed and each carried seed in
    /// ascending order. A seed that an earlier part holds does not repeat, so no seed runs twice in one night.
    /// </summary>
    /// <exception cref="ArgumentException">The sweep is not one of <see cref="Sweeps"/>, or the date comes before <see cref="DayZero"/>.</exception>
    public static List<SeedRange> Plan(string sweep, DateOnly date, IReadOnlyList<ulong> extra, IReadOnlyList<ulong> carried)
    {
        SeedRange fixedRange = FixedRange(sweep);
        SeedRange slice = Slice(sweep, date);
        List<SeedRange> plan = [fixedRange, slice];
        SortedSet<ulong> singles = [];
        foreach (ulong seed in extra)
        {
            singles.Add(seed);
        }

        foreach (ulong seed in carried)
        {
            singles.Add(seed);
        }

        foreach (ulong seed in singles)
        {
            if (!fixedRange.Holds(seed) && !slice.Holds(seed))
            {
                plan.Add(new SeedRange(seed, seed));
            }
        }

        return plan;
    }

    /// <summary>A seed list in the form of the <c>--seeds</c> option: ranges and single seeds, with a comma between them.</summary>
    public static string FormatList(IReadOnlyList<SeedRange> seeds)
    {
        StringBuilder text = new();
        foreach (SeedRange range in seeds)
        {
            if (text.Length > 0)
            {
                text.Append(',');
            }

            text.Append(range.From == range.To ? range.From.ToString(CultureInfo.InvariantCulture) : range.ToString());
        }

        return text.ToString();
    }

    /// <summary>
    /// Reads a seed list: items with a comma between them, and each item a single seed or a range <c>from-to</c> with
    /// from at or below to. Returns null with the reason in <paramref name="error"/> when the text is not a list.
    /// </summary>
    public static List<SeedRange>? TryParseList(string text, out string error)
    {
        List<SeedRange> seeds = [];
        foreach (string item in text.Split(','))
        {
            int dash = item.IndexOf('-', StringComparison.Ordinal);
            string first = dash < 0 ? item : item[..dash];
            string last = dash < 0 ? item : item[(dash + 1)..];
            if (!ulong.TryParse(first, NumberStyles.None, CultureInfo.InvariantCulture, out ulong from)
                || !ulong.TryParse(last, NumberStyles.None, CultureInfo.InvariantCulture, out ulong to)
                || from > to)
            {
                error = $"the item '{item}' of the seed list '{text}' is not one seed, or a range <from>-<to> with from at or below to";
                return null;
            }

            seeds.Add(new SeedRange(from, to));
        }

        error = string.Empty;
        return seeds;
    }

    /// <summary>The count of seeds of a list. A seed that two items hold counts two times.</summary>
    public static ulong Count(IReadOnlyList<SeedRange> seeds)
    {
        ulong count = 0;
        foreach (SeedRange range in seeds)
        {
            count += range.To - range.From + 1;
        }

        return count;
    }

    /// <summary>
    /// Reads the extra fixed seeds (D-567): one JSON object with an array of seeds for each sweep of
    /// <see cref="Sweeps"/>, and no other field. Each seed comes after the fixed range of its sweep, and no seed of a
    /// sweep repeats.
    /// </summary>
    /// <exception cref="FormatException">The text breaks one of these rules. The message names the file, the sweep, and the seed (T-2).</exception>
    public static Dictionary<string, List<ulong>> ReadExtraSeeds(string text, string path)
    {
        using JsonDocument document = ParseObject(text, path);
        Dictionary<string, List<ulong>> extra = new(StringComparer.Ordinal);
        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            if (Array.IndexOf(Sweeps, property.Name) < 0)
            {
                throw new FormatException($"The file {path} names the sweep '{property.Name}', and the sweeps are {string.Join(", ", Sweeps)}.");
            }

            List<ulong> seeds = ReadSeedArray(property, path);
            ulong top = FixedTop(property.Name);
            HashSet<ulong> seen = [];
            foreach (ulong seed in seeds)
            {
                if (seed <= top)
                {
                    throw new FormatException($"The file {path} holds the extra seed {seed} of the sweep '{property.Name}', and the fixed range already holds seeds 1 to {top}.");
                }

                if (!seen.Add(seed))
                {
                    throw new FormatException($"The file {path} holds the extra seed {seed} of the sweep '{property.Name}' two times.");
                }
            }

            extra[property.Name] = seeds;
        }

        foreach (string sweep in Sweeps)
        {
            if (!extra.ContainsKey(sweep))
            {
                throw new FormatException($"The file {path} has no field for the sweep '{sweep}'. Each sweep needs an array, an empty one included.");
            }
        }

        return extra;
    }

    /// <summary>
    /// Reads one seed field of a night record, <see cref="FailedSeedsName"/> or <see cref="CarriedSeedsName"/>, as the
    /// seeds of each sweep. A record of a night before PR-84 holds no such field, and it gives an empty map, because
    /// that night ran no slice (D-567).
    /// </summary>
    /// <exception cref="FormatException">The record is not a JSON object, or the field is not an object of seed arrays by sweep (T-2).</exception>
    public static Dictionary<string, List<ulong>> ReadRecordSeeds(string recordText, string field, string source)
    {
        using JsonDocument document = ParseObject(recordText.TrimStart('﻿'), source);
        Dictionary<string, List<ulong>> seeds = new(StringComparer.Ordinal);
        if (!document.RootElement.TryGetProperty(field, out JsonElement element))
        {
            return seeds;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException($"The field '{field}' of the night record {source} is a {element.ValueKind}, and it holds an object of seed arrays by sweep.");
        }

        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (Array.IndexOf(Sweeps, property.Name) < 0)
            {
                throw new FormatException($"The field '{field}' of the night record {source} names the sweep '{property.Name}', and the sweeps are {string.Join(", ", Sweeps)}.");
            }

            seeds[property.Name] = ReadSeedArray(property, $"{source}, field '{field}'");
        }

        return seeds;
    }

    /// <summary>
    /// Reads the slice windows of a night record, by sweep (D-564). A record of a night before PR-84 holds no slice,
    /// and it gives an empty map, because that night ran the fixed set alone.
    /// </summary>
    /// <exception cref="FormatException">The record is not a JSON object, or the slice is not an object of windows by sweep with a date (T-2).</exception>
    public static Dictionary<string, SeedRange> ReadSlices(string recordText, string source)
    {
        using JsonDocument document = ParseObject(recordText.TrimStart('\uFEFF'), source);
        Dictionary<string, SeedRange> slices = new(StringComparer.Ordinal);
        if (!document.RootElement.TryGetProperty(SliceName, out JsonElement element))
        {
            return slices;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException($"The field '{SliceName}' of the night record {source} is a {element.ValueKind}, and it holds an object of windows by sweep.");
        }

        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (property.Name == SliceDateName)
            {
                continue;
            }

            string text = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString()! : property.Value.GetRawText();
            List<SeedRange>? window = Array.IndexOf(Sweeps, property.Name) < 0 ? null : TryParseList(text, out _);
            if (window is null || window.Count != 1)
            {
                throw new FormatException($"The slice of the night record {source} holds '{property.Name}': '{text}', and each member is a sweep of {string.Join(", ", Sweeps)} with one window <from>-<to>.");
            }

            slices[property.Name] = window[0];
        }

        return slices;
    }

    /// <summary>
    /// True when a night of a record ran the seed of the sweep: the seed lies in the fixed range, in the slice of the
    /// record, or in the carried seeds of the record (D-569).
    /// </summary>
    public static bool Ran(string sweep, ulong seed, IReadOnlyDictionary<string, SeedRange> slices, IReadOnlyDictionary<string, List<ulong>> carried)
    {
        if (FixedRange(sweep).Holds(seed))
        {
            return true;
        }

        if (slices.TryGetValue(sweep, out SeedRange? slice) && slice.Holds(seed))
        {
            return true;
        }

        foreach (ulong carriedSeed in SeedsOf(carried, sweep))
        {
            if (carriedSeed == seed)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The seeds of one sweep in a map, or an empty list when the map holds none.</summary>
    public static IReadOnlyList<ulong> SeedsOf(IReadOnlyDictionary<string, List<ulong>> seeds, string sweep)
    {
        return seeds.TryGetValue(sweep, out List<ulong>? list) ? list : [];
    }

    /// <summary>
    /// The failure line of one sweep that ran to its end: the sweep name, a colon, and each failed seed with a space
    /// before it. A sweep with no failure writes its line too, because the line proves that the sweep ended (D-567).
    /// </summary>
    public static string FailureLine(string sweep, IReadOnlyList<ulong> failed)
    {
        StringBuilder line = new(sweep);
        line.Append(':');
        foreach (ulong seed in failed)
        {
            line.Append(' ').Append(seed.ToString(CultureInfo.InvariantCulture));
        }

        return line.Append('\n').ToString();
    }

    /// <summary>
    /// Reads the failure lines of a night: the failed seeds of each sweep that ran to its end. A sweep with no line did
    /// not end. A blank line is skipped.
    /// </summary>
    /// <exception cref="FormatException">A line names no sweep of <see cref="Sweeps"/>, repeats a sweep, or holds a word that is not a seed (T-2).</exception>
    public static Dictionary<string, List<ulong>> ReadFailures(string text, string source)
    {
        Dictionary<string, List<ulong>> failures = new(StringComparer.Ordinal);
        foreach (string raw in text.Split('\n'))
        {
            string line = raw.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            int colon = line.IndexOf(':', StringComparison.Ordinal);
            string sweep = colon < 0 ? line : line[..colon];
            if (colon < 0 || Array.IndexOf(Sweeps, sweep) < 0)
            {
                throw new FormatException($"The failure line '{line}' of {source} is not one sweep of {string.Join(", ", Sweeps)}, a colon, and the failed seeds.");
            }

            if (failures.ContainsKey(sweep))
            {
                throw new FormatException($"The failures of {source} hold two lines of the sweep '{sweep}'.");
            }

            List<ulong> seeds = [];
            foreach (string word in line[(colon + 1)..].Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!ulong.TryParse(word, NumberStyles.None, CultureInfo.InvariantCulture, out ulong seed) || seed == 0)
                {
                    throw new FormatException($"The failure line '{line}' of {source} holds '{word}', and a seed is a whole number of one or more.");
                }

                seeds.Add(seed);
            }

            failures[sweep] = seeds;
        }

        return failures;
    }

    /// <summary>
    /// The seed fields of a night record, as JSON members with a comma between them (D-564, D-567, D-569):
    /// <list type="bullet">
    /// <item><see cref="SliceName"/>: the date and the window of the slice of each sweep.</item>
    /// <item><see cref="CarriedSeedsName"/>: for each sweep that ended, the failed seeds of the record of main that it ran.</item>
    /// <item><see cref="FailedSeedsName"/>: for each sweep that ended, its failed seeds. For each sweep that did not end, the failed seeds of the record of main, because no night passed them yet.</item>
    /// </list>
    /// </summary>
    /// <param name="date">The UTC date of the night start.</param>
    /// <param name="status">The status of the night.</param>
    /// <param name="failures">The failed seeds of each sweep that ended, from <see cref="ReadFailures"/>.</param>
    /// <param name="carry">The failed seeds of the record of main at the night start, from <see cref="ReadRecordSeeds"/>.</param>
    /// <exception cref="FormatException">A night with the status success names a failed seed, or a sweep that did not end (T-2).</exception>
    public static string RecordFields(DateOnly date, string status, IReadOnlyDictionary<string, List<ulong>> failures, IReadOnlyDictionary<string, List<ulong>> carry)
    {
        StringBuilder slice = new($"{{\"{SliceDateName}\":\"{Text(date)}\"");
        StringBuilder carried = new("{");
        StringBuilder failed = new("{");
        foreach (string sweep in Sweeps)
        {
            slice.Append(",\"").Append(sweep).Append("\":\"").Append(Slice(sweep, date)).Append('"');
            bool ended = failures.TryGetValue(sweep, out List<ulong>? sweepFailures);
            if (status == "success" && (!ended || sweepFailures!.Count > 0))
            {
                string state = ended ? $"the failed seeds {string.Join(' ', sweepFailures!)}" : "no failure line, so it did not end";
                throw new FormatException($"The night reads success, and the sweep '{sweep}' has {state}. A night that succeeds ends each sweep with no failure (D-567).");
            }

            IReadOnlyList<ulong> fromMain = SeedsOf(carry, sweep);
            AppendSeeds(carried, sweep, ended ? fromMain : []);
            AppendSeeds(failed, sweep, ended ? sweepFailures! : fromMain);
        }

        slice.Append('}');
        carried.Append('}');
        failed.Append('}');
        return $"\"{SliceName}\":{slice},\"{CarriedSeedsName}\":{carried},\"{FailedSeedsName}\":{failed}";
    }

    /// <summary>A date in the form of <see cref="DateFormat"/>.</summary>
    public static string Text(DateOnly date)
    {
        return date.ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    /// <summary>Appends one member <c>"sweep":[seeds]</c> to an object in progress, with a comma before each member after the first.</summary>
    private static void AppendSeeds(StringBuilder json, string sweep, IReadOnlyList<ulong> seeds)
    {
        if (json.Length > 1)
        {
            json.Append(',');
        }

        json.Append('"').Append(sweep).Append("\":[");
        for (int index = 0; index < seeds.Count; index++)
        {
            if (index > 0)
            {
                json.Append(',');
            }

            json.Append(seeds[index].ToString(CultureInfo.InvariantCulture));
        }

        json.Append(']');
    }

    /// <exception cref="ArgumentException">The sweep is not one of <see cref="Sweeps"/>.</exception>
    private static ulong FixedTop(string sweep)
    {
        CheckSweep(sweep);
        return sweep == ReachabilitySweep ? ReachabilityFixedTop : BotFixedTop;
    }

    /// <exception cref="ArgumentException">The sweep is not one of <see cref="Sweeps"/>.</exception>
    private static ulong SliceSize(string sweep)
    {
        CheckSweep(sweep);
        return sweep == ReachabilitySweep ? ReachabilitySliceSize : BotSliceSize;
    }

    /// <exception cref="ArgumentException">The sweep is not one of <see cref="Sweeps"/>.</exception>
    private static void CheckSweep(string sweep)
    {
        if (Array.IndexOf(Sweeps, sweep) < 0)
        {
            throw new ArgumentException($"No night sweep has the name '{sweep}'. The sweeps are {string.Join(", ", Sweeps)}.", nameof(sweep));
        }
    }

    /// <exception cref="FormatException">The text is not JSON, or its root is not an object.</exception>
    private static JsonDocument ParseObject(string text, string source)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(text);
        }
        catch (JsonException exception)
        {
            throw new FormatException($"The text of {source} is not JSON: {exception.Message}", exception);
        }

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            JsonValueKind kind = document.RootElement.ValueKind;
            document.Dispose();
            throw new FormatException($"The JSON of {source} is a {kind}, and it must be an object.");
        }

        return document;
    }

    /// <exception cref="FormatException">The value is not an array of whole numbers of one or more.</exception>
    private static List<ulong> ReadSeedArray(JsonProperty property, string source)
    {
        if (property.Value.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException($"The sweep '{property.Name}' of {source} is a {property.Value.ValueKind}, and it holds an array of seeds.");
        }

        List<ulong> seeds = [];
        foreach (JsonElement item in property.Value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Number || !item.TryGetUInt64(out ulong seed) || seed == 0)
            {
                throw new FormatException($"The sweep '{property.Name}' of {source} holds '{item.GetRawText()}', and a seed is a whole number of one or more.");
            }

            seeds.Add(seed);
        }

        return seeds;
    }
}
