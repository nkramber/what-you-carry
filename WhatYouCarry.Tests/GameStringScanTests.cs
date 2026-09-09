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

    /// <summary>Every allowed position is a name that the engine reads, and `Get` is the table call.</summary>
    [Fact]
    public void TheAllowedPositionsAreEngineNames()
    {
        Assert.Contains(GameStringScan.StringsGet, GameStringScan.AllowedStringPositions);
        Assert.Contains("GetNode", GameStringScan.AllowedStringPositions);
    }

    private static IReadOnlyList<LintFinding> Scan(string source)
    {
        return GameStringScan.ScanText(source, "WhatYouCarry.Game/Screen.cs");
    }
}
