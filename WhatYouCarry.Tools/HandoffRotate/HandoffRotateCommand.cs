using System;
using System.IO;

namespace WhatYouCarry.Tools.HandoffRotate;

/// <summary>
/// <c>handoff-rotate --root &lt;checkout&gt;</c>. Keeps the 10 newest handoff entries and moves each older entry to the
/// top of the archive (D-146, D-379). It prints the moved session numbers and the next session number (D-187).
/// Exit 0 means the files hold the rule. Exit 1 means a file breaks the order, or a file is absent, and nothing
/// changed. Exit 2 means the command itself is wrong.
/// </summary>
public static class HandoffRotateCommand
{
    private const string Name = "handoff-rotate";

    private const string Usage = "Usage: handoff-rotate --root <checkout>";

    public static int Run(string[] args)
    {
        if (args.Length != 2 || args[0] != "--root")
        {
            Console.Error.WriteLine($"Expected exactly the option '--root <checkout>'. {Usage}");
            return 2;
        }

        string handoffPath = Path.Combine(args[1], HandoffRotateRules.HandoffPath);
        string archivePath = Path.Combine(args[1], HandoffRotateRules.ArchivePath);
        foreach (string path in new[] { handoffPath, archivePath })
        {
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"{Name}: the file '{path}' does not exist. Nothing changed.");
                return 1;
            }
        }

        HandoffRotation rotation;
        try
        {
            rotation = HandoffRotateRules.Rotate(File.ReadAllText(handoffPath), File.ReadAllText(archivePath));
        }
        catch (InvalidOperationException error)
        {
            Console.Error.WriteLine($"{Name}: {error.Message} Nothing changed.");
            return 1;
        }

        if (rotation.Moved.Count == 0)
        {
            Console.Out.WriteLine($"{Name}: the handoff holds {HandoffRotateRules.KeepCount} entries or fewer. Nothing moved. The next session number is {rotation.NextSession}.");
            return 0;
        }

        // The archive write comes first. A stop between the two writes leaves copies at the archive top, and the
        // next run names that state and changes nothing.
        File.WriteAllText(archivePath, rotation.Archive);
        File.WriteAllText(handoffPath, rotation.Handoff);
        Console.Out.WriteLine($"{Name}: moved Session {string.Join(", Session ", rotation.Moved)} to '{HandoffRotateRules.ArchivePath}'. The next session number is {rotation.NextSession}.");
        return 0;
    }
}
