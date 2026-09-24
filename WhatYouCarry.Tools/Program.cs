using System;
using WhatYouCarry.Tools.AssetQa;
using WhatYouCarry.Tools.AudioSynth;
using WhatYouCarry.Tools.BitIdentity;
using WhatYouCarry.Tools.BotRunner;
using WhatYouCarry.Tools.CiSkip;
using WhatYouCarry.Tools.CodexReview;
using WhatYouCarry.Tools.DetLint;
using WhatYouCarry.Tools.DocGate;
using WhatYouCarry.Tools.HandoffRotate;
using WhatYouCarry.Tools.NightGate;
using WhatYouCarry.Tools.ReviewGate;
using WhatYouCarry.Tools.SteCheck;
using WhatYouCarry.Tools.TextureGen;

namespace WhatYouCarry.Tools;

public static class Program
{
    private const string Usage = "Usage: WhatYouCarry.Tools <command> [options]. Commands: review-gate, doc-gate, handoff-rotate, ste-check, det-lint, asset-qa, texture-gen, audio-synth, audio-analyze, bit-identity, bot-run, night-record, night-gate, night-promote, night-publish-check, ci-skip, codex-review.";

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
            case "doc-gate":
                return DocGateCommand.Run(commandArgs);
            case "handoff-rotate":
                return HandoffRotateCommand.Run(commandArgs);
            case "ste-check":
                return SteCheckCommand.Run(commandArgs);
            case "det-lint":
                return DetLintCommand.Run(commandArgs);
            case "asset-qa":
                return AssetQaCommand.Run(commandArgs);
            case "texture-gen":
                return TextureGenCommand.Run(commandArgs);
            case "audio-synth":
                return AudioSynthCommand.Run(commandArgs);
            case "audio-analyze":
                return AudioAnalyzeCommand.Run(commandArgs);
            case "bit-identity":
                return BitIdentityCommand.Run(commandArgs);
            case "bot-run":
                return BotRunCommand.Run(commandArgs);
            case "night-record":
                return NightRecordCommand.Run(commandArgs);
            case "night-gate":
                return NightGateCommand.Run(commandArgs);
            case "night-promote":
                return NightPromoteCommand.Run(commandArgs);
            case "night-publish-check":
                return NightPublishCheckCommand.Run(commandArgs);
            case "ci-skip":
                return CiSkipCommand.Run(commandArgs);
            case "codex-review":
                return CodexReviewCommand.Run(commandArgs);
            default:
                Console.Error.WriteLine($"Unknown command '{command}'. {Usage}");
                return 2;
        }
    }
}
