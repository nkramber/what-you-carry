using System;
using System.Collections.Generic;
using WhatYouCarry.Tools.DetLint;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The Game string rule (G-8, D-98, D-221; PR-5 exit test 6).</summary>
public sealed class GameStringScanTests
{
    /// <summary>PR-5 exit test 6. A string that a player sees, written inline, is one finding.</summary>
    [Fact]
    public void LintFlagsInlineString()
    {
        LintFinding finding = Assert.Single(Scan("""
            public sealed class Screen
            {
                public string Title() => "Descend";
            }
            """));

        Assert.Equal("L-STRING", finding.Rule);
        Assert.Equal("Descend", finding.Symbol);
        Assert.Contains("string table", finding.Detail, StringComparison.Ordinal);
    }

    /// <summary>A string that comes from the table is not a finding, and the id inside the call is not one.</summary>
    [Fact]
    public void AStringFromTheTableIsNotAFinding()
    {
        Assert.Empty(Scan("""
            public sealed class Screen
            {
                public string Title(Strings strings) => strings.Get("hub.descend");
            }
            """));
    }

    /// <summary>A name that the engine needs is not a string that a player reads.</summary>
    [Theory]
    [InlineData("public object A(Node n) => n.GetNode(\"Panel/Label\");")]
    [InlineData("public object A(Node n) => n.FindChild(\"Label\");")]
    [InlineData("public object A(Node n) => n.Connect(\"pressed\", null);")]
    [InlineData("private const string ScenePath = \"res://ui/hub.tscn\";")]
    public void AnEngineNameIsNotAFinding(string member)
    {
        Assert.Empty(Scan($"public sealed class Screen {{ {member} }}"));
    }

    /// <summary>An empty string names nothing that a player reads.</summary>
    [Fact]
    public void AnEmptyStringIsNotAFinding()
    {
        Assert.Empty(Scan("""public sealed class Screen { public string A() => ""; }"""));
    }

    /// <summary>Each inline string is its own finding, and the report names the text and the line.</summary>
    [Fact]
    public void EachInlineStringIsItsOwnFinding()
    {
        IReadOnlyList<LintFinding> findings = Scan("""
            public sealed class Screen
            {
                public string A() => "Ascend";

                public string B() => "Bank";
            }
            """);

        Assert.Equal(2, findings.Count);
        Assert.Equal(3, findings[0].Line);
        Assert.Equal(5, findings[1].Line);
    }

    /// <summary>The Game project of this checkout has no inline string (G-19).</summary>
    [Fact]
    public void LintPassesGame()
    {
        string root = RepositoryRoot.Find();
        IReadOnlyList<LintFinding> findings = GameStringScan.Run(root);
        Assert.True(findings.Count == 0, string.Join(Environment.NewLine, findings));

        // The scan must read the real directory, and the build output stays out of it.
        Assert.DoesNotContain(GameStringScan.SourceFiles(root), path => path.Contains(".godot", StringComparison.Ordinal));
    }

    /// <summary>
    /// The engine list holds no `Get`, because any type can hold a `Get` method. The receiver list covers the
    /// one call that takes an id (F-79).
    /// </summary>
    [Fact]
    public void TheEngineListHoldsNoGet()
    {
        Assert.DoesNotContain(GameStringScan.StringsGet, GameStringScan.AllowedStringPositions);
        Assert.Contains("GetNode", GameStringScan.AllowedStringPositions);
        Assert.Contains("Strings", GameStringScan.StringTableReceivers);
    }

    /// <summary>
    /// A `Get` call takes an id only when its receiver names the string table. Any other `Get` can carry a text
    /// that a player reads (F-79).
    /// </summary>
    [Theory]
    [InlineData("public string A(Bag inventory) => inventory.Get(\"You died\");", 1)]
    [InlineData("public string A(Bag bag) => bag.Get(\"Ascend\");", 1)]
    [InlineData("public string A(Strings strings) => strings.Get(\"hub.descend\");", 0)]
    [InlineData("public string A() => Strings.Get(\"hub.descend\");", 0)]
    [InlineData("public string A(Content c) => c.Strings.Get(\"hub.descend\");", 0)]
    public void OnlyTheStringTableReceiverExemptsAGet(string member, int expected)
    {
        Assert.Equal(expected, Scan($"public sealed class Screen {{ {member} }}").Count);
    }

    private static IReadOnlyList<LintFinding> Scan(string source)
    {
        return GameStringScan.ScanText(source, "WhatYouCarry.Game/Screen.cs");
    }
}
