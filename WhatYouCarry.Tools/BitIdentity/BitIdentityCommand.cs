using System;
using WhatYouCarry.Core.Determinism;

namespace WhatYouCarry.Tools.BitIdentity;

/// <summary>
/// <c>bit-identity</c>. Prints the state hash of <see cref="BitIdentitySweep"/> as 16 hexadecimal digits on one
/// line of standard output, and the context on standard error. Exit 0 means the sweep ran. Exit 2 means a usage
/// error. The CI job compares the standard output of the three platforms (D-69, D-71).
/// </summary>
public static class BitIdentityCommand
{
    public static int Run(string[] args)
    {
        if (args.Length != 0)
        {
            Console.Error.WriteLine($"Unexpected argument '{args[0]}'. The bit-identity command takes no option.");
            return 2;
        }

        StateHash hash = BitIdentitySweep.Run();

        // The context goes to standard error, so standard output holds the hash alone and the job can compare it.
        Console.Error.WriteLine($"bit-identity: run seed {BitIdentitySweep.RunSeed:x16}, {BitIdentitySweep.DrawsPerStream} draws per stream, {BitIdentitySweep.AngleSamples} angle samples.");
        Console.Out.WriteLine(hash.ToString());
        return 0;
    }
}
