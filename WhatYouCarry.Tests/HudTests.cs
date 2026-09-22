using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Ui;
using WhatYouCarry.Tools.DetLint;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The HUD, its layout, the damage numbers, and the navigation (PR-19, D-441 to D-447).</summary>
public sealed class HudTests
{
    private const string UiDirectory = "WhatYouCarry.Game/Ui";

    private static readonly Vector2 Deck = new(1280.0f, 800.0f);

    /// <summary>
    /// PR-19 exit test 1. No string literal in the HUD source stands outside <c>Strings.Get</c> or a const name
    /// (G-8). No HUD file builds a text by interpolation, which the string rule of det-lint does not read. Every id
    /// that a HUD file gives to <c>Strings.Get</c> is a const whose value the string table holds.
    /// </summary>
    [Fact]
    public void HudReadsStringTable()
    {
        Strings strings = new ContentLoader(new RepositoryContentSource()).Load().Strings;
        IReadOnlyList<(string Path, SyntaxNode Root)> files = UiFiles();
        Assert.Contains(files, file => file.Path.EndsWith("/Hud.cs", StringComparison.Ordinal));
        Dictionary<string, string> constants = ConstStrings(files);

        int ids = 0;
        foreach ((string path, SyntaxNode root) in files)
        {
            Assert.Empty(GameStringScan.ScanText(root.SyntaxTree.ToString(), path));
            Assert.DoesNotContain(root.DescendantNodes(), node => node is InterpolatedStringExpressionSyntax);
            foreach (ExpressionSyntax argument in TableArguments(root))
            {
                foreach (string id in IdsOf(argument, constants, path))
                {
                    Assert.True(strings.Has(id), $"{path} reads the id '{id}', and the string table holds no such id.");
                    ids++;
                }
            }
        }

        Assert.True(ids >= 10, $"The HUD sources read {ids} ids from the table, and the HUD needs ten or more.");
    }

    /// <summary>The check of exit test 1 finds a text that a HUD file builds by interpolation, and a text written inline.</summary>
    [Fact]
    public void HudStringCheckFindsInlineText()
    {
        SyntaxNode interpolated = CSharpSyntaxTree.ParseText("class A { string T(int s) => $\"{s} seconds\"; }").GetRoot();
        Assert.Contains(interpolated.DescendantNodes(), node => node is InterpolatedStringExpressionSyntax);
        Assert.NotEmpty(GameStringScan.ScanText("class A { void T(Label l) { l.Text = \"Paused\"; } }", "Hud.cs"));
    }

    /// <summary>
    /// PR-19 exit test 2. A damage number never overlaps the screen box of its entity, and it stays inside the screen,
    /// for boxes over the whole screen, partly off it, and near each edge (F-24, D-444). A property test: each failure
    /// names its seed (D-66).
    /// </summary>
    [Fact]
    public void DamageNumberAvoidsSilhouette()
    {
        Rect2 screen = new(Vector2.Zero, Deck);
        int placed = 0;
        for (int seed = 0; seed < 5000; seed++)
        {
            Random random = new(seed);
            float wide = 10.0f + (random.NextSingle() * 400.0f);
            float high = 10.0f + (random.NextSingle() * 600.0f);
            Rect2 entity = new(
                (random.NextSingle() * (Deck.X + 200.0f)) - 100.0f - (wide / 2.0f),
                (random.NextSingle() * (Deck.Y + 200.0f)) - 100.0f - (high / 2.0f),
                wide,
                high);
            int amount = 1 + random.Next(9999);
            float age = random.NextSingle() * DamageNumbers.LifetimeSeconds;
            int newer = random.Next(4);

            Rect2? place = DamageNumbers.Place(entity, Deck, amount, age, newer);
            if (place is not Rect2 number)
            {
                continue;
            }

            placed++;
            Assert.False(number.Intersects(entity), $"Seed {seed}: the number {number} overlaps the entity box {entity}.");
            Assert.True(screen.Encloses(number), $"Seed {seed}: the number {number} is not inside the screen.");
        }

        Assert.True(placed > 2500, $"Only {placed} of 5000 numbers had a place, so the test reads too few.");
    }

    /// <summary>A number stands above its entity, under it when the space above is off the screen, and nowhere when the entity is off the screen.</summary>
    [Fact]
    public void DamageNumberPlaces()
    {
        Rect2 middle = new(600.0f, 300.0f, 80.0f, 200.0f);
        Rect2 above = Assert.NotNull(DamageNumbers.Place(middle, Deck, 25, 0.0f, 0));
        Assert.Equal(middle.Position.Y - DamageNumbers.Gap, above.End.Y, 3);
        Assert.Equal(middle.GetCenter().X, above.GetCenter().X, 3);

        Rect2 risen = Assert.NotNull(DamageNumbers.Place(middle, Deck, 25, DamageNumbers.LifetimeSeconds / 2.0f, 0));
        Assert.True(risen.Position.Y < above.Position.Y, "A number of a greater age stands no higher.");

        Rect2 stacked = Assert.NotNull(DamageNumbers.Place(middle, Deck, 25, 0.0f, 1));
        Assert.Equal(above.Position.Y - DamageNumbers.High, stacked.Position.Y, 3);

        Rect2 top = new(600.0f, 5.0f, 80.0f, 200.0f);
        Rect2 under = Assert.NotNull(DamageNumbers.Place(top, Deck, 25, 0.0f, 0));
        Assert.Equal(top.End.Y + DamageNumbers.Gap, under.Position.Y, 3);

        Assert.Null(DamageNumbers.Place(new Rect2(-300.0f, 300.0f, 80.0f, 200.0f), Deck, 25, 0.0f, 0));
        Assert.Null(DamageNumbers.Place(new Rect2(600.0f, 0.0f, 80.0f, 800.0f), Deck, 25, 0.0f, 0));
    }

    /// <summary>A number lives 0.8 seconds, fades, and a number of no health is an error (D-444, T-2).</summary>
    [Fact]
    public void DamageNumbersLiveTheirLifetime()
    {
        DamageNumbers numbers = new();
        numbers.Add(3, 10, false);
        numbers.Add(0, 4, true);
        numbers.Add(3, 7, false);
        Assert.Equal(1, numbers.NewerOnOwner(0));
        Assert.Equal(0, numbers.NewerOnOwner(1));
        Assert.Equal(0, numbers.NewerOnOwner(2));

        numbers.Advance(0.5f);
        Assert.Equal(3, numbers.Live.Count);
        Assert.Equal(1.0f - (0.5f / DamageNumbers.LifetimeSeconds), DamageNumbers.Opacity(numbers.Live[0].Age), 4);
        numbers.Advance(0.3f);
        Assert.Empty(numbers.Live);

        ContextException empty = Assert.Throws<ContextException>(() => numbers.Add(3, 0, false));
        Assert.Contains(DamageNumbers.EmptyHitMessage, empty.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => DamageNumbers.BoxOf([]));
        Assert.Equal(new Rect2(1.0f, 2.0f, 4.0f, 6.0f), DamageNumbers.BoxOf([new Vector2(5.0f, 2.0f), new Vector2(1.0f, 8.0f)]));
    }

    /// <summary>
    /// PR-19 exit test 3. Every element of the HUD stands inside 1280 by 800 at the Deck scale of 1.0, keeps its
    /// margin, and overlaps no other element. The same holds on a wider screen and on a screen of 16 by 9 (D-446, G-15).
    /// </summary>
    [Fact]
    public void HudFitsDeck()
    {
        Assert.Equal(1.0f, UiScale.Factor(Deck));
        Assert.Equal(Deck, UiScale.LayoutSize(Deck));

        foreach (Vector2 screen in new[] { Deck, new Vector2(1920.0f, 1080.0f), new Vector2(2560.0f, 1600.0f), new Vector2(1280.0f, 720.0f) })
        {
            Vector2 size = UiScale.LayoutSize(screen);
            Assert.True(size.X >= UiScale.BaseWide - 0.01f || size.Y >= UiScale.BaseHigh - 0.01f, $"The layout size {size} of {screen} is smaller than the base on both sides.");
            Rect2 inner = new(HudLayout.Margin / 2.0f, HudLayout.Margin / 2.0f, size.X - HudLayout.Margin, size.Y - HudLayout.Margin);
            IReadOnlyList<Rect2> elements = HudLayout.For(size).Elements();
            for (int index = 0; index < elements.Count; index++)
            {
                Assert.True(inner.Encloses(elements[index]), $"On {screen}, element {index} at {elements[index]} is not inside {inner}.");
                for (int other = index + 1; other < elements.Count; other++)
                {
                    Assert.False(elements[index].Intersects(elements[other]), $"On {screen}, element {index} overlaps element {other}.");
                }
            }
        }
    }

    /// <summary>The scale of a screen is the smaller ratio to the base, and a screen with no size is an error (D-446, T-2).</summary>
    [Fact]
    public void UiScaleFitsTheBase()
    {
        Assert.Equal(1.35f, UiScale.Factor(new Vector2(1920.0f, 1080.0f)), 4);
        Assert.Equal(2.0f, UiScale.Factor(new Vector2(2560.0f, 1600.0f)), 4);
        Assert.Equal(0.9f, UiScale.Factor(new Vector2(1280.0f, 720.0f)), 4);
        Assert.Throws<ContextException>(() => UiScale.Factor(Vector2.Zero));
    }

    /// <summary>
    /// PR-19 exit test 4. The four directions reach every control of the fixture screen from every control, with no
    /// mouse, and the slider keeps left and right for its value (D-446, D-15).
    /// </summary>
    [Fact]
    public void NavigationReachesEveryControl()
    {
        IReadOnlyList<FocusControl> controls = NavigationFixture.Controls();
        Assert.Equal((NavigationFixture.Columns * NavigationFixture.Rows) + 1, controls.Count);
        int[] all = Enumerable.Range(0, controls.Count).ToArray();
        for (int start = 0; start < controls.Count; start++)
        {
            Assert.Equal(all, Navigation.Reachable(controls, start));
        }

        int slider = controls.Count - 1;
        Assert.Equal(1, Navigation.Neighbor(controls, 0, FocusDirection.Right));
        Assert.Equal(3, Navigation.Neighbor(controls, 0, FocusDirection.Down));
        Assert.Equal(slider, Navigation.Neighbor(controls, 4, FocusDirection.Down));
        Assert.Equal(4, Navigation.Neighbor(controls, slider, FocusDirection.Up));
        Assert.Null(Navigation.Neighbor(controls, slider, FocusDirection.Left));
        Assert.Null(Navigation.Neighbor(controls, slider, FocusDirection.Right));
        Assert.Null(Navigation.Neighbor(controls, 0, FocusDirection.Up));
        Assert.Null(Navigation.Neighbor(controls, 2, FocusDirection.Right));

        Rect2 screen = new(Vector2.Zero, UiScale.Base);
        Assert.All(controls, control => Assert.True(screen.Encloses(control.Box), $"The fixture control {control.Box} is not inside the Deck screen."));
    }

    /// <summary>A control that no direction reaches is not in the reached list, so the check of exit test 4 finds it.</summary>
    [Fact]
    public void NavigationFindsAnUnreachableControl()
    {
        FocusControl first = new(new Rect2(0.0f, 0.0f, 100.0f, 50.0f), true);
        FocusControl beside = new(new Rect2(200.0f, 0.0f, 100.0f, 50.0f), false);
        Assert.Equal([0], Navigation.Reachable([first, beside], 0));
        Assert.Throws<ContextException>(() => Navigation.Reachable([], 0));
    }

    /// <summary>The timer shows whole seconds rounded up in minutes and seconds, and a count below zero is an error (D-443, T-2).</summary>
    [Theory]
    [InlineData(0L, "0:00")]
    [InlineData(1L, "0:01")]
    [InlineData(60L, "0:01")]
    [InlineData(61L, "0:02")]
    [InlineData(247L * 60L, "4:07")]
    [InlineData(600L * 60L, "10:00")]
    public void TimerShowsMinutesAndSeconds(long ticks, string shown)
    {
        Strings strings = new ContentLoader(new RepositoryContentSource()).Load().Strings;
        Assert.Equal(shown, HudText.Timer(strings, ticks));
    }

    /// <summary>The health number, and the prompt lines of each device with the tap and the hold (D-442, D-447, D-448).</summary>
    [Fact]
    public void HudTextReadsTheTable()
    {
        Strings strings = new ContentLoader(new RepositoryContentSource()).Load().Strings;
        Assert.Equal("72 / 100", HudText.Health(strings, 72, 100));
        Assert.Equal("E: Descend", HudText.Descend(strings, false));
        Assert.Equal("Hold E: Ascend", HudText.Ascend(strings, false));
        Assert.Equal("X: Descend", HudText.Descend(strings, true));
        Assert.Equal("Hold X: Ascend", HudText.Ascend(strings, true));
        ContextException negative = Assert.Throws<ContextException>(() => HudText.Timer(strings, -1));
        Assert.Contains(HudText.NegativeTicksMessage, negative.Message, StringComparison.Ordinal);
    }

    /// <summary>The timer pauses at the open prompt before expiry alone (D-140, D-443).</summary>
    [Fact]
    public void HudStatePausesAtTheOpenPrompt()
    {
        Assert.True(new HudState(100, 100, 600, false, true, false).Paused);
        Assert.False(new HudState(100, 100, 600, false, false, false).Paused);
        Assert.False(new HudState(100, 100, 0, true, true, false).Paused);
    }

    /// <summary>Every source file under the Ui directory of the Game project, parsed, sorted by path.</summary>
    private static IReadOnlyList<(string Path, SyntaxNode Root)> UiFiles()
    {
        string root = RepositoryRoot.Find();
        List<(string, SyntaxNode)> files = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, UiDirectory), "*.cs").OrderBy(path => path, StringComparer.Ordinal))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            files.Add((relative, CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot()));
        }

        return files;
    }

    /// <summary>The value of each const string field of the files, by field name. A name declared twice is an error of the test.</summary>
    private static Dictionary<string, string> ConstStrings(IReadOnlyList<(string Path, SyntaxNode Root)> files)
    {
        Dictionary<string, string> constants = new(StringComparer.Ordinal);
        foreach ((string path, SyntaxNode root) in files)
        {
            foreach (FieldDeclarationSyntax field in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                if (!field.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ConstKeyword)))
                {
                    continue;
                }

                foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                {
                    if (variable.Initializer?.Value is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
                    {
                        Assert.True(constants.TryAdd(variable.Identifier.ValueText, literal.Token.ValueText), $"{path} declares the const '{variable.Identifier.ValueText}' a second time.");
                    }
                }
            }
        }

        return constants;
    }

    /// <summary>The argument of each call of <c>Get</c> on the string table in one file.</summary>
    private static IEnumerable<ExpressionSyntax> TableArguments(SyntaxNode root)
    {
        foreach (InvocationExpressionSyntax call in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (call.Expression is MemberAccessExpressionSyntax access
                && access.Name.Identifier.ValueText == GameStringScan.StringsGet
                && GameStringScan.StringTableReceivers.Contains(ReceiverName(access.Expression)))
            {
                yield return Assert.Single(call.ArgumentList.Arguments).Expression;
            }
        }
    }

    /// <summary>The last name of a receiver: <c>strings</c> for both <c>strings</c> and <c>this.strings</c>.</summary>
    private static string ReceiverName(ExpressionSyntax receiver)
    {
        return receiver switch
        {
            SimpleNameSyntax name => name.Identifier.ValueText,
            MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
            _ => string.Empty,
        };
    }

    /// <summary>The ids that one argument of <c>Get</c> can name: a const, a member of a type that is a const, or either branch of a condition.</summary>
    private static IEnumerable<string> IdsOf(ExpressionSyntax argument, Dictionary<string, string> constants, string path)
    {
        if (argument is ConditionalExpressionSyntax condition)
        {
            return IdsOf(condition.WhenTrue, constants, path).Concat(IdsOf(condition.WhenFalse, constants, path));
        }

        string name = argument switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
            _ => throw new InvalidOperationException($"{path} gives Strings.Get the argument '{argument}', and the HUD gives it a const id alone."),
        };

        Assert.True(constants.TryGetValue(name, out string? id), $"{path} gives Strings.Get '{argument}', which is no const string of the HUD.");
        return [id!];
    }
}
