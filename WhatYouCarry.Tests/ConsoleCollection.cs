using System.IO;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The one collection of every test class that reads, writes, or redirects the process console: a command
/// under test writes its summary there, and a command test captures it. xUnit runs the classes of one
/// collection one after another, so no capture reads the line of another test (PR #34 review P1-1).
/// </summary>
/// <remarks>
/// <see cref="EveryConsoleTestIsInTheCollection"/> reads the test sources and fails on a class that touches
/// the console outside the collection, so a new command test cannot bring the race back.
/// </remarks>
[CollectionDefinition(Name)]
public sealed class ConsoleCollection
{
    /// <summary>The name of the collection.</summary>
    public const string Name = "Console";
}

/// <summary>The shape rule of the console collection, over every test source of the repository.</summary>
public sealed class ConsoleCollectionTests
{
    /// <summary>A test source that calls a command or redirects the console carries the collection attribute on its class.</summary>
    [Fact]
    public void EveryConsoleTestIsInTheCollection()
    {
        string root = Path.Combine(RepositoryRoot.Find(), "WhatYouCarry.Tests");
        string[] marks = ["Console.SetOut(", "Console.SetError(", "Program.Main(", "Command.Run("];
        int inCollection = 0;
        foreach (string file in Directory.EnumerateFiles(root, "*Tests.cs"))
        {
            string text = File.ReadAllText(file);
            bool touches = false;
            foreach (string mark in marks)
            {
                touches |= text.Contains(mark, System.StringComparison.Ordinal);
            }

            if (!touches || Path.GetFileName(file) == "ConsoleCollection.cs")
            {
                continue;
            }

            Assert.True(text.Contains("[Collection(ConsoleCollection.Name)]", System.StringComparison.Ordinal), $"{Path.GetFileName(file)} touches the console and is not in the console collection.");
            inCollection++;
        }

        Assert.True(inCollection >= 4, $"The collection holds {inCollection} classes.");
    }
}
