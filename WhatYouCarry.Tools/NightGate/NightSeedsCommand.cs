using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>
/// <c>night-seeds --sweep &lt;name&gt; --date &lt;yyyy-MM-dd&gt; --root &lt;checkout&gt; --carry &lt;night.json&gt;</c>.
/// Prints the seed list of one sweep of a night on standard output, in the form of the <c>--seeds</c> option of
/// <c>bot-run</c> (D-564 to D-567). The list holds the fixed range, the slice of the date, the extra fixed seeds of the
/// checkout, and the failed seeds that the record of main carries. Standard error names each part, so the run log
/// names the window, and each failure replays.
/// </summary>
/// <remarks>Exit 0 prints the list. Exit 2 means a wrong option, an unreadable file, or a date before day 0.</remarks>
public static class NightSeedsCommand
{
    private const string Usage = "Usage: night-seeds --sweep <random-walker|greedy-descender|full-clearer|timer-tester|coward|reachability> --date <yyyy-MM-dd> --root <checkout> --carry <night.json>";

    public static int Run(string[] args)
    {
        string? sweep = null;
        string? dateText = null;
        string? root = null;
        string? carry = null;
        int i = 0;
        while (i < args.Length)
        {
            if (i + 1 >= args.Length)
            {
                Console.Error.WriteLine($"The option '{args[i]}' needs a value. {Usage}");
                return 2;
            }

            switch (args[i])
            {
                case "--sweep": sweep = args[i + 1]; break;
                case "--date": dateText = args[i + 1]; break;
                case "--root": root = args[i + 1]; break;
                case "--carry": carry = args[i + 1]; break;
                default:
                    Console.Error.WriteLine($"Unexpected argument '{args[i]}'. {Usage}");
                    return 2;
            }

            i += 2;
        }

        if (sweep is null || dateText is null || root is null || carry is null)
        {
            Console.Error.WriteLine($"Every option is required. {Usage}");
            return 2;
        }

        if (Array.IndexOf(NightSeeds.Sweeps, sweep) < 0)
        {
            Console.Error.WriteLine($"The sweep '{sweep}' is not one of: {string.Join(", ", NightSeeds.Sweeps)}. {Usage}");
            return 2;
        }

        if (!DateOnly.TryParseExact(dateText, NightSeeds.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
        {
            Console.Error.WriteLine($"The date '{dateText}' is not in the form {NightSeeds.DateFormat}. {Usage}");
            return 2;
        }

        if (date < NightSeeds.DayZero)
        {
            Console.Error.WriteLine($"The date {dateText} comes before day 0 of the slices, {NightSeeds.Text(NightSeeds.DayZero)} (D-566).");
            return 2;
        }

        string extraPath = Path.Combine(root, NightSeeds.ExtraSeedsPath);
        IReadOnlyList<ulong> extra;
        IReadOnlyList<ulong> carried;
        try
        {
            extra = NightSeeds.SeedsOf(NightSeeds.ReadExtraSeeds(File.ReadAllText(extraPath), extraPath), sweep);
            carried = NightSeeds.SeedsOf(NightSeeds.ReadRecordSeeds(File.ReadAllText(carry), NightSeeds.FailedSeedsName, carry), sweep);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or FormatException)
        {
            Console.Error.WriteLine($"night-seeds: the sweep {sweep} has no seed list: {exception.Message}");
            return 2;
        }

        List<SeedRange> plan = NightSeeds.Plan(sweep, date, extra, carried);
        int day = date.DayNumber - NightSeeds.DayZero.DayNumber;
        Console.Error.WriteLine($"night-seeds: the sweep {sweep} on {dateText}, day {day}: the fixed seeds {plan[0]}, the slice {plan[1]}, the extra seeds [{string.Join(' ', extra)}] of {NightSeeds.ExtraSeedsPath}, and the carried seeds [{string.Join(' ', carried)}] of {carry}.");
        Console.Out.WriteLine(NightSeeds.FormatList(plan));
        return 0;
    }
}
