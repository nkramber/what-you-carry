using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Tools.BotRunner;

/// <summary>
/// <c>bot-run --policy &lt;name&gt; --seeds &lt;from&gt;-&lt;to&gt; --output &lt;directory&gt; --root &lt;checkout&gt; [--summary &lt;file&gt;]</c>.
/// Plays one headless run per seed with the policy over the content of the checkout, and writes one JSONL run
/// log per run to the output directory (D-115, D-127). Exit 0 means no crash, and no softlock on a policy that
/// promises progress.
/// </summary>
/// <remarks>
/// The PR job runs one hundred seeds per policy, and the night job five thousand (D-115). The command prints one
/// summary line per end state on standard output, and the log of each run holds the policy, the seed, the end
/// state, the deepest floor, the ticks, and the text of a crash.
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

    /// <summary>The largest count of seeds in one command. The night runs five thousand, and a range past this is a typo.</summary>
    public const ulong LargestSpan = 1000000;

    private const string Usage = "Usage: bot-run --policy <random-walker|greedy-descender|full-clearer|timer-tester> --seeds <from>-<to> --output <directory> --root <checkout> [--summary <file>]";

    public static int Run(string[] args)
    {
        string? policy = null;
        string? seeds = null;
        string? output = null;
        string? root = null;
        string? summary = null;
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

        if (!TryParseSeeds(seeds, out ulong from, out ulong to))
        {
            Console.Error.WriteLine($"The seed range '{seeds}' is not <from>-<to> with from at or below to. {Usage}");
            return 2;
        }

        if (to - from >= LargestSpan)
        {
            Console.Error.WriteLine($"The seed range '{seeds}' holds more than {LargestSpan} seeds. {Usage}");
            return 2;
        }

        ContentSet content = new ContentLoader(new DirectoryContentSource(Path.Combine(root, "content"))).Load();
        Directory.CreateDirectory(output);

        int[] counts = new int[5];
        SortedDictionary<string, int> causes = new(StringComparer.Ordinal);
        bool promises = false;
        // The count and not the seed drives the loop, so a range that ends at the largest seed cannot wrap.
        ulong span = to - from + 1;
        for (ulong offset = 0; offset < span; offset++)
        {
            ulong seed = from + offset;
            IBotPolicy bot = CreatePolicy(policy, seed, content);
            promises = bot.PromisesProgress;
            BotRunResult result = BotRun.Play(bot, seed, content);
            counts[(int)result.End]++;
            if (result.End == BotRunEnd.Death)
            {
                causes[result.Cause] = causes.TryGetValue(result.Cause, out int earlier) ? earlier + 1 : 1;
            }

            string path = Path.Combine(output, $"{policy}-{seed.ToString(CultureInfo.InvariantCulture)}.jsonl");
            using FileLogSink sink = new(path);
            WriteLog(result, new JsonlLogger(sink));
        }

        Console.Out.WriteLine($"bot-run: policy {policy}, seeds {from}-{to}, bottom {counts[(int)BotRunEnd.Bottom]}, budget {counts[(int)BotRunEnd.Budget]}, softlock {counts[(int)BotRunEnd.Softlock]}, death {counts[(int)BotRunEnd.Death]}, crash {counts[(int)BotRunEnd.Crash]}.");
        if (summary is not null)
        {
            // One line for each policy, appended, so the night gathers every policy into its record (D-403).
            File.AppendAllText(summary, DeathLine(policy, counts[(int)BotRunEnd.Death], causes), new UTF8Encoding(false));
        }


        // A death is a real outcome of a fight, and never a fault of the code, so it fails no gate (D-403).
        bool failed = counts[(int)BotRunEnd.Crash] > 0 || (promises && counts[(int)BotRunEnd.Softlock] > 0);
        return failed ? 1 : 0;
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
            default:
                ContextException error = new($"No bot policy has the name '{name}'. The policies are {RandomWalker.PolicyName}, {GreedyDescender.PolicyName}, {FullClearer.PolicyName}, and {TimerTester.PolicyName}.");
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
    /// One summary line of a policy: its name, an equals sign, and the count of deaths (D-403), then one
    /// <c>cause:count</c> word for each cause, in the ordinal order of the causes (D-411).
    /// </summary>
    public static string DeathLine(string policy, int deaths, SortedDictionary<string, int> causes)
    {
        StringBuilder line = new();
        line.Append(policy).Append('=').Append(deaths.ToString(CultureInfo.InvariantCulture));
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

    /// <summary>The text of an end state in the log (D-270, D-403).</summary>
    public static string EndStateText(BotRunEnd end)
    {
        switch (end)
        {
            case BotRunEnd.Bottom: return "bottom";
            case BotRunEnd.Budget: return "budget";
            case BotRunEnd.Softlock: return "softlock";
            case BotRunEnd.Crash: return "crash";
            case BotRunEnd.Death: return "death";
            default: throw new ArgumentOutOfRangeException(nameof(end), $"The end state {(int)end} has no name.");
        }
    }

    private static bool TryParseSeeds(string text, out ulong from, out ulong to)
    {
        from = 0;
        to = 0;
        int dash = text.IndexOf('-', StringComparison.Ordinal);
        if (dash <= 0)
        {
            return false;
        }

        return ulong.TryParse(text[..dash], NumberStyles.Integer, CultureInfo.InvariantCulture, out from)
            && ulong.TryParse(text[(dash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out to)
            && from <= to;
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
