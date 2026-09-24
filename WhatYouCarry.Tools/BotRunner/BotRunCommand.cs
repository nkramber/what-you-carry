using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Tools.NightGate;

namespace WhatYouCarry.Tools.BotRunner;

/// <summary>
/// <c>bot-run --policy &lt;name&gt; --seeds &lt;list&gt; --output &lt;directory&gt; --root &lt;checkout&gt; [--summary &lt;file&gt;]
/// [--failures &lt;file&gt;]</c>. Plays one headless run per seed with the policy over the content of the checkout, and
/// writes one JSONL run log per run to the output directory (D-115, D-127). The list holds ranges <c>from-to</c> and
/// single seeds, with a comma between them (D-564). Exit 0 means no crash, and no softlock on a policy that promises
/// progress.
/// </summary>
/// <remarks>
/// The PR job runs one hundred seeds per policy, and the night job five thousand and a slice (D-115, D-564). The command prints one
/// summary line per end state on standard output, and the log of each run holds the policy, the seed, the end
/// state, the deepest floor, the ticks, and the text of a crash. The failures file takes one line with each failed
/// seed, which the night record reads (D-567).
/// </remarks>
public static class BotRunCommand
{
    /// <summary>The subsystem name of every bot log line.</summary>
    public const string Subsystem = "bot";

    /// <summary>The names of the extra fields of the end line.</summary>
    public const string PolicyName = "policy";
    public const string EndStateName = "endState";
    public const string FloorsReachedName = "floorsReached";
    public const string ErrorName = "error";
    public const string CauseName = "cause";
    public const string EventName = "timerEvent";
    public const string WaveName = "wave";
    public const string CountName = "count";

    /// <summary>The largest count of seeds in one command. The night runs about five thousand five hundred, and a list past this is a typo.</summary>
    public const ulong LargestSpan = 1000000;

    private const string Usage = "Usage: bot-run --policy <random-walker|greedy-descender|full-clearer|timer-tester|coward> --seeds <from>-<to>[,<seed>|,<from>-<to>]... --output <directory> --root <checkout> [--summary <file>] [--failures <file>]";

    public static int Run(string[] args)
    {
        string? policy = null;
        string? seeds = null;
        string? output = null;
        string? root = null;
        string? summary = null;
        string? failures = null;
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
                case "--policy": policy = args[i + 1]; break;
                case "--seeds": seeds = args[i + 1]; break;
                case "--output": output = args[i + 1]; break;
                case "--root": root = args[i + 1]; break;
                case "--summary": summary = args[i + 1]; break;
                case "--failures": failures = args[i + 1]; break;
                default:
                    Console.Error.WriteLine($"Unexpected argument '{args[i]}'. {Usage}");
                    return 2;
            }

            i += 2;
        }

        if (policy is null || seeds is null || output is null || root is null)
        {
            Console.Error.WriteLine($"Every option is required. {Usage}");
            return 2;
        }

        List<SeedRange>? list = NightSeeds.TryParseList(seeds, out string listError);
        if (list is null)
        {
            Console.Error.WriteLine($"The seeds are wrong: {listError}. {Usage}");
            return 2;
        }

        // Each range holds at most the largest count, so the sum of a sane list cannot wrap.
        foreach (SeedRange range in list)
        {
            if (range.To - range.From >= LargestSpan)
            {
                Console.Error.WriteLine($"The seed range '{range}' holds more than {LargestSpan} seeds. {Usage}");
                return 2;
            }
        }

        if (NightSeeds.Count(list) > LargestSpan)
        {
            Console.Error.WriteLine($"The seed list '{seeds}' holds more than {LargestSpan} seeds. {Usage}");
            return 2;
        }

        ContentSet content = new ContentLoader(new DirectoryContentSource(Path.Combine(root, "content"))).Load();
        Directory.CreateDirectory(output);

        int[] counts = new int[6];
        SortedDictionary<string, int> causes = new(StringComparer.Ordinal);
        bool promises = false;
        List<ulong> failed = [];
        foreach (SeedRange range in list)
        {
            // The count and not the seed drives the loop, so a range that ends at the largest seed cannot wrap.
            ulong span = range.To - range.From + 1;
            for (ulong offset = 0; offset < span; offset++)
            {
                ulong seed = range.From + offset;
                IBotPolicy bot = CreatePolicy(policy, seed, content);
                promises = bot.PromisesProgress;
                BotRunResult result = BotRun.Play(bot, seed, content);
                counts[(int)result.End]++;
                if (result.End == BotRunEnd.Death)
                {
                    causes[result.Cause] = causes.TryGetValue(result.Cause, out int earlier) ? earlier + 1 : 1;
                }

                if (IsFailure(result.End, promises))
                {
                    failed.Add(seed);
                }

                string path = Path.Combine(output, $"{policy}-{seed.ToString(CultureInfo.InvariantCulture)}.jsonl");
                using FileLogSink sink = new(path);
                WriteLog(result, new JsonlLogger(sink));
            }
        }

        Console.Out.WriteLine($"bot-run: policy {policy}, seeds {seeds}, bottom {counts[(int)BotRunEnd.Bottom]}, ascend {counts[(int)BotRunEnd.Ascend]}, budget {counts[(int)BotRunEnd.Budget]}, softlock {counts[(int)BotRunEnd.Softlock]}, death {counts[(int)BotRunEnd.Death]}, crash {counts[(int)BotRunEnd.Crash]}.");
        if (summary is not null)
        {
            // One line for each policy, appended, so the night gathers every policy into its record (D-403).
            File.AppendAllText(summary, DeathLine(policy, counts[(int)BotRunEnd.Death], counts[(int)BotRunEnd.Ascend], causes), new UTF8Encoding(false));
        }

        if (failures is not null)
        {
            // One line for each policy, appended, so the night record names each failed seed (D-567).
            File.AppendAllText(failures, NightSeeds.FailureLine(policy, failed), new UTF8Encoding(false));
        }

        if (failed.Count > 0)
        {
            Console.Out.WriteLine($"bot-run: policy {policy}, the failed seeds: {string.Join(' ', failed)}.");
        }

        return failed.Count > 0 ? 1 : 0;
    }

    /// <summary>
    /// True when an end state fails the gate: a crash, or a softlock of a policy that promises progress. A death is a
    /// real outcome of a fight, and never a fault of the code, so it fails no gate (D-403).
    /// </summary>
    public static bool IsFailure(BotRunEnd end, bool promisesProgress)
    {
        return end == BotRunEnd.Crash || (promisesProgress && end == BotRunEnd.Softlock);
    }

    /// <summary>The policy of a name for one run seed.</summary>
    /// <exception cref="ContextException">No policy has the name.</exception>
    public static IBotPolicy CreatePolicy(string name, ulong seed, ContentSet content)
    {
        switch (name)
        {
            case RandomWalker.PolicyName: return new RandomWalker(seed);
            case GreedyDescender.PolicyName: return new GreedyDescender(content);
            case FullClearer.PolicyName: return new FullClearer(content);
            case TimerTester.PolicyName: return new TimerTester();
            case Coward.PolicyName: return new Coward();
            default:
                ContextException error = new($"No bot policy has the name '{name}'. The policies are {RandomWalker.PolicyName}, {GreedyDescender.PolicyName}, {FullClearer.PolicyName}, {TimerTester.PolicyName}, and {Coward.PolicyName}.");
                error.AddContext("policy", name);
                throw error;
        }
    }

    /// <summary>
    /// Writes the lines of one run log: the start at tick zero on floor 1, one line for each timer event in tick
    /// order (M-5), and the end with the end state, the deepest floor, the ticks, the cause of a death (D-411), and
    /// the text of a crash (D-113).
    /// </summary>
    public static void WriteLog(BotRunResult result, JsonlLogger logger)
    {
        long[] entities = [];

        LogFields start = new();
        start.Add("seed", result.Seed);
        start.Add("floor", 1L);
        start.Add("tick", 0L);
        start.Add("subsystem", Subsystem);
        start.Add("entities", entities);
        start.Add(PolicyName, result.Policy);
        logger.Write(LogContextKind.Run, LogLevel.Info, "The bot run starts.", start);

        foreach (TimerEvent timerEvent in result.Events)
        {
            LogFields line = new();
            line.Add("seed", result.Seed);
            line.Add("floor", (long)timerEvent.Floor);
            line.Add("tick", (long)timerEvent.Tick);
            line.Add("subsystem", Subsystem);
            line.Add("entities", entities);
            line.Add(PolicyName, result.Policy);
            line.Add(EventName, EventText(timerEvent.Kind));
            line.Add(WaveName, (long)timerEvent.Wave);
            line.Add(CountName, (long)timerEvent.Count);
            logger.Write(LogContextKind.Run, LogLevel.Info, EventMessage(timerEvent.Kind), line);
        }

        LogFields end = new();
        end.Add("seed", result.Seed);
        end.Add("floor", (long)result.FloorsReached);
        end.Add("tick", (long)result.Ticks);
        end.Add("subsystem", Subsystem);
        end.Add("entities", entities);
        end.Add(PolicyName, result.Policy);
        end.Add(EndStateName, EndStateText(result.End));
        end.Add(FloorsReachedName, (long)result.FloorsReached);
        if (result.End == BotRunEnd.Death)
        {
            end.Add(CauseName, result.Cause);
        }

        if (result.End == BotRunEnd.Crash)
        {
            end.Add(ErrorName, result.Error);
        }

        logger.Write(LogContextKind.Run, result.End == BotRunEnd.Crash ? LogLevel.Error : LogLevel.Info, "The bot run ends.", end);
    }

    /// <summary>
    /// One summary line of a policy: its name, an equals sign, and the count of deaths (D-403), then the word
    /// <c>ascends=count</c> (D-430), then one <c>cause:count</c> word for each cause, in the ordinal order of the
    /// causes (D-411).
    /// </summary>
    public static string DeathLine(string policy, int deaths, int ascends, SortedDictionary<string, int> causes)
    {
        StringBuilder line = new();
        line.Append(policy).Append('=').Append(deaths.ToString(CultureInfo.InvariantCulture));
        line.Append(' ').Append(NightRecordCommand.AscendsWord).Append('=').Append(ascends.ToString(CultureInfo.InvariantCulture));
        foreach (KeyValuePair<string, int> cause in causes)
        {
            line.Append(' ').Append(cause.Key).Append(':').Append(cause.Value.ToString(CultureInfo.InvariantCulture));
        }

        return line.Append('\n').ToString();
    }

    /// <summary>The text of a timer event kind in the log (M-5).</summary>
    public static string EventText(TimerEventKind kind)
    {
        switch (kind)
        {
            case TimerEventKind.Expiry: return "expiry";
            case TimerEventKind.HunterSpawn: return "hunter-spawn";
            case TimerEventKind.Wave: return "wave";
            case TimerEventKind.WaveSkip: return "wave-skip";
            default: throw new ArgumentOutOfRangeException(nameof(kind), $"The timer event kind {(int)kind} has no name.");
        }
    }

    /// <summary>The message of a timer event line.</summary>
    private static string EventMessage(TimerEventKind kind)
    {
        switch (kind)
        {
            case TimerEventKind.Expiry: return "The floor timer expires.";
            case TimerEventKind.HunterSpawn: return "The Overseer spawns.";
            case TimerEventKind.Wave: return "A wave spawns.";
            case TimerEventKind.WaveSkip: return "A wave skips spawns that found no post out of sight.";
            default: throw new ArgumentOutOfRangeException(nameof(kind), $"The timer event kind {(int)kind} has no message.");
        }
    }

    /// <summary>The text of an end state in the log (D-270, D-403, D-430).</summary>
    public static string EndStateText(BotRunEnd end)
    {
        switch (end)
        {
            case BotRunEnd.Bottom: return "bottom";
            case BotRunEnd.Budget: return "budget";
            case BotRunEnd.Softlock: return "softlock";
            case BotRunEnd.Crash: return "crash";
            case BotRunEnd.Death: return "death";
            case BotRunEnd.Ascend: return "ascend";
            default: throw new ArgumentOutOfRangeException(nameof(end), $"The end state {(int)end} has no name.");
        }
    }
}

/// <summary>A log sink that appends each line to one file. The runner opens the file, because Core opens none (D-211).</summary>
public sealed class FileLogSink : ILogSink, IDisposable
{
    private readonly StreamWriter writer;

    public FileLogSink(string path)
    {
        this.writer = new StreamWriter(path, append: false);
    }

    public void Write(string line)
    {
        this.writer.WriteLine(line);
    }

    public void Dispose()
    {
        this.writer.Dispose();
    }
}

/// <summary>
/// The content source of a checkout directory, which reads every JSON file under it with a forward-slash path
/// (D-219). It skips the model directory and the texture directory, which hold the animation, palette, and rule
/// files that Core never reads (D-298, D-305).
/// </summary>
public sealed class DirectoryContentSource : IContentSource
{
    private readonly string root;

    public DirectoryContentSource(string root)
    {
        this.root = root;
    }

    public IReadOnlyList<ContentFile> Read()
    {
        List<ContentFile> files = [];
        foreach (string file in Directory.EnumerateFiles(this.root, "*.json", SearchOption.AllDirectories))
        {
            string contentPath = Path.GetRelativePath(this.root, file).Replace('\\', '/');
            if (ContentLoader.IsAssetPath(contentPath))
            {
                continue;
            }

            files.Add(new ContentFile(contentPath, File.ReadAllBytes(file)));
        }

        return files;
    }
}
