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

    /// <summary>
    /// A text that reaches the screen with no literal node is a finding (F-132). The compiler lowers an
    /// interpolated string and a const to text, and the old scan read literal nodes alone. `Load` took a literal
    /// on any receiver, so a node that shows the text passed as an engine call. Each row gave no finding on the
    /// old scan.
    /// </summary>
    [Theory]
    [InlineData("public void A(Label l, int f) { l.Text = $\"You died on floor {f}\"; }", "$\"You died on floor {f}\"")]
    [InlineData("private const string Died = \"You died\"; public void A(Label l) { l.Text = Died; }", "Died")]
    [InlineData("public void A(Hud h) { h.Load(\"You died\"); }", "You died")]
    [InlineData("private const string Died = \"You died\"; public Label A() => new Label { Text = Died };", "Died")]
    [InlineData("private const string Died = \"You died\"; public void A(Label l) { l.TooltipText = Screen.Died; }", "Died")]
    [InlineData("public string A(int f) => $\"{f} seconds\";", "$\"{f} seconds\"")]
    public void ALoweredTextIsAFinding(string member, string symbol)
    {
        LintFinding finding = Assert.Single(Scan($"public sealed class Screen {{ {member} }}"));
        Assert.Equal("L-STRING", finding.Rule);
        Assert.Equal(symbol, finding.Symbol);
    }

    /// <summary>
    /// Each const that reaches a text member is its own finding, through each branch of a <c>?:</c> (F-132).
    /// </summary>
    [Fact]
    public void EachConstThatReachesATextIsAFinding()
    {
        IReadOnlyList<LintFinding> findings = Scan("""
            public sealed class Screen
            {
                private const string Died = "You died";
                private const string Won = "You won";

                public void A(Label l, bool dead) { l.Text = dead ? Died : (Won); }
            }
            """);

        Assert.Equal(["Died", "Won"], [findings[0].Symbol, findings[1].Symbol]);
        Assert.Equal(2, findings.Count);
    }

    /// <summary>
    /// The boundary of the new rules (F-132). An exception constructor may build its text by interpolation,
    /// written with its type or target-typed. An engine loader takes a path. A const id reaches the text through
    /// the table. An interpolated node path is an engine name.
    /// </summary>
    [Theory]
    [InlineData("public void A(int f) { throw new ContextException($\"The floor {f} is outside the run.\"); }")]
    [InlineData("public void A(int f) { ContextException error = new($\"The floor {f} is outside the run.\"); throw error; }")]
    [InlineData("public void A(int f) { Core.Logging.ContextException error = new($\"The floor {f} is outside the run.\"); throw error; }")]
    [InlineData("public object A() => GD.Load<Shader>(\"res://world/world.gdshader\");")]
    [InlineData("public object A() => ResourceLoader.Load(\"res://ui/hub.tscn\");")]
    [InlineData("private const string DiedId = \"hud.died\"; public void A(Label l, Strings strings) { l.Text = strings.Get(DiedId); }")]
    [InlineData("private const string ScenePath = \"res://ui/hub.tscn\"; public object A() => GD.Load<PackedScene>(ScenePath);")]
    [InlineData("public object A(Node n, int i) => n.GetNode($\"Panel/Slot{i}\");")]
    public void TheBoundaryOfTheLoweredTextRules(string member)
    {
        Assert.Empty(Scan($"public sealed class Screen {{ {member} }}"));
    }

    /// <summary>
    /// An interpolated string outside an exception constructor is a finding, also when the statement throws the
    /// value later. The exemption reads the argument alone (F-132).
    /// </summary>
    [Fact]
    public void AnInterpolationOutsideTheConstructorIsAFinding()
    {
        LintFinding finding = Assert.Single(Scan("public sealed class Screen { public void A(int f) { string text = $\"The floor {f}\"; throw new ContextException(text); } }"));
        Assert.Equal("L-STRING", finding.Rule);
    }

    private static IReadOnlyList<LintFinding> Scan(string source)
    {
        return GameStringScan.ScanText(source, "WhatYouCarry.Game/Screen.cs");
    }

    /// <summary>
    /// PR #104 P2-1. A const string of one Game file that reaches a text member of another file is a finding, because
    /// the scan joins the const names of every file first. The old scan read each file alone. A const of another file
    /// that only names a string id stays allowed.
    /// </summary>
    [Fact]
    public void AConstOfAnotherFileThatReachesATextIsAFinding()
    {
        GameSource texts = new("static class Texts { public const string Died = \"You died\"; public const string DiedId = \"run.died\"; }", "WhatYouCarry.Game/Texts.cs");
        GameSource screen = new("class Screen { void Show(Label label, Strings strings) { label.Text = Texts.Died; label.TooltipText = strings.Get(Texts.DiedId); } }", "WhatYouCarry.Game/Screen.cs");

        IReadOnlyList<LintFinding> findings = GameStringScan.ScanSources([texts, screen]);

        LintFinding crossFile = Assert.Single(findings, finding => finding.Path == "WhatYouCarry.Game/Screen.cs");
        Assert.Contains("Died", crossFile.ToString(), StringComparison.Ordinal);
    }
}
