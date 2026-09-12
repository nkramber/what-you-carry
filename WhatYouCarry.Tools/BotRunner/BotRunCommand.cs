using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.BotRunner;

/// <summary>
/// <c>bot-run --policy &lt;name&gt; --seeds &lt;from&gt;-&lt;to&gt; --output &lt;directory&gt; --root &lt;checkout&gt;</c>.
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

    /// <summary>The largest count of seeds in one command. The night runs five thousand, and a range past this is a typo.</summary>
    public const ulong LargestSpan = 1000000;

    private const string Usage = "Usage: bot-run --policy <random-walker|greedy-descender> --seeds <from>-<to> --output <directory> --root <checkout>";

    public static int Run(string[] args)
    {
        string? policy = null;
        string? seeds = null;
        string? output = null;
        string? root = null;
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

        int[] counts = new int[4];
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

            string path = Path.Combine(output, $"{policy}-{seed.ToString(CultureInfo.InvariantCulture)}.jsonl");
            using FileLogSink sink = new(path);
            WriteLog(result, new JsonlLogger(sink));
        }

        Console.Out.WriteLine($"bot-run: policy {policy}, seeds {from}-{to}, bottom {counts[(int)BotRunEnd.Bottom]}, budget {counts[(int)BotRunEnd.Budget]}, softlock {counts[(int)BotRunEnd.Softlock]}, crash {counts[(int)BotRunEnd.Crash]}.");
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
            default:
                ContextException error = new($"No bot policy has the name '{name}'. The policies are {RandomWalker.PolicyName} and {GreedyDescender.PolicyName}.");
                error.AddContext("policy", name);
                throw error;
        }
    }

    /// <summary>
    /// Writes the two lines of one run log: the start at tick zero on floor 1, and the end with the end state,
    /// the deepest floor, the ticks, and the text of a crash (D-113).
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

        LogFields end = new();
        end.Add("seed", result.Seed);
        end.Add("floor", (long)result.FloorsReached);
        end.Add("tick", (long)result.Ticks);
        end.Add("subsystem", Subsystem);
        end.Add("entities", entities);
        end.Add(PolicyName, result.Policy);
        end.Add(EndStateName, EndStateText(result.End));
        end.Add(FloorsReachedName, (long)result.FloorsReached);
        if (result.End == BotRunEnd.Crash)
        {
            end.Add(ErrorName, result.Error);
        }

        logger.Write(LogContextKind.Run, result.End == BotRunEnd.Crash ? LogLevel.Error : LogLevel.Info, "The bot run ends.", end);
    }

    /// <summary>The text of an end state in the log (D-270).</summary>
    public static string EndStateText(BotRunEnd end)
    {
        switch (end)
        {
            case BotRunEnd.Bottom: return "bottom";
            case BotRunEnd.Budget: return "budget";
            case BotRunEnd.Softlock: return "softlock";
            case BotRunEnd.Crash: return "crash";
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
/// (D-219). It skips the model directory, which holds the animation files that Core never reads (D-298).
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
            if (contentPath.StartsWith(ContentLoader.ModelDirectory, StringComparison.Ordinal))
            {
                continue;
            }

            files.Add(new ContentFile(contentPath, File.ReadAllBytes(file)));
        }

        return files;
    }
}
