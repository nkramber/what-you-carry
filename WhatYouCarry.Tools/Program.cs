using System;
using WhatYouCarry.Tools.BitIdentity;
using WhatYouCarry.Tools.BotRunner;
using WhatYouCarry.Tools.DetLint;
using WhatYouCarry.Tools.ReviewGate;
using WhatYouCarry.Tools.SteCheck;

namespace WhatYouCarry.Tools;

public static class Program
{
    private const string Usage = "Usage: WhatYouCarry.Tools <command> [options]. Commands: review-gate, ste-check, det-lint, bit-identity, bot-run, night-record.";

    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine(Usage);
            return 2;
        }

        string command = args[0];
        string[] commandArgs = args[1..];
        switch (command)
        {
            case "review-gate":
                return ReviewGateCommand.Run(commandArgs);
            case "ste-check":
                return SteCheckCommand.Run(commandArgs);
            case "det-lint":
                return DetLintCommand.Run(commandArgs);
            case "bit-identity":
                return BitIdentityCommand.Run(commandArgs);
            case "bot-run":
                return BotRunCommand.Run(commandArgs);
            case "night-record":
                return NightRecordCommand.Run(commandArgs);
            default:
                Console.Error.WriteLine($"Unknown command '{command}'. {Usage}");
                return 2;
        }
    }
}
