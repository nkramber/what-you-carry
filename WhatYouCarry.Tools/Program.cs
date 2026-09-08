using System;
using WhatYouCarry.Tools.ReviewGate;
using WhatYouCarry.Tools.SteCheck;

namespace WhatYouCarry.Tools;

public static class Program
{
    private const string Usage = "Usage: WhatYouCarry.Tools <command> [options]. Commands: review-gate, ste-check.";

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
            default:
                Console.Error.WriteLine($"Unknown command '{command}'. {Usage}");
                return 2;
        }
    }
}
