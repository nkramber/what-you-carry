using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Game.Ui;
using WhatYouCarry.Tools.DetLint;
using Xunit;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Tests;

/// <summary>The HUD, its layout, the damage numbers, and the navigation (PR-19, D-441 to D-447, PR-98, D-725 to D-728).</summary>
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

    /// <summary>A number lives 0.8 seconds, holds full opacity for 0.5 seconds then fades (D-444, D-731), and a number of no health is an error (T-2).</summary>
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
        Assert.Equal(1.0f, DamageNumbers.Opacity(numbers.Live[0].Age), 4);
        numbers.Advance(0.3f);
        Assert.Empty(numbers.Live);

        ContextException empty = Assert.Throws<ContextException>(() => numbers.Add(3, 0, false));
        Assert.Contains(DamageNumbers.EmptyHitMessage, empty.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => DamageNumbers.BoxOf([]));
        Assert.Equal(new Rect2(1.0f, 2.0f, 4.0f, 6.0f), DamageNumbers.BoxOf([new Vector2(5.0f, 2.0f), new Vector2(1.0f, 8.0f)]));
    }

    /// <summary>
    /// PR-98 exit test 1. A damage number has a font of 28 pixels at 800p (D-725), and a black outline of 3 pixels that
    /// the engine size 10 draws (D-726). The box of a number holds one line of the font with the outline on each side.
    /// </summary>
    [Fact]
    public void DamageNumberSizeAndOutline()
    {
        Assert.Equal(28, DamageNumbers.FontPixels);
        Assert.Equal(3, DamageNumbers.OutlineWide);
        Assert.Equal(10, DamageNumbers.EngineOutlineSize);
        Assert.True(DamageNumbers.High >= DamageNumbers.FontPixels + (2 * DamageNumbers.OutlineWide), $"The box height {DamageNumbers.High} does not hold the font and the outline.");
        Assert.True(DamageNumbers.Padding >= DamageNumbers.OutlineWide, $"The padding {DamageNumbers.Padding} does not hold the outline.");
    }

    /// <summary>
    /// PR-98 exit test 1. A number holds full opacity for 0.5 seconds, then fades linearly to zero at 0.8 seconds
    /// (D-731).
    /// </summary>
    [Fact]
    public void DamageNumberHoldsThenFades()
    {
        Assert.Equal(1.0f, DamageNumbers.Opacity(0.0f), 4);
        Assert.Equal(1.0f, DamageNumbers.Opacity(0.3f), 4);
        Assert.Equal(1.0f, DamageNumbers.Opacity(0.5f), 4);
        Assert.Equal(0.5f, DamageNumbers.Opacity(0.65f), 4);
        Assert.Equal(0.0f, DamageNumbers.Opacity(0.8f), 4);
    }

    /// <summary>
    /// PR-98 exit test 1. A dead enemy has no entity box, so its numbers hide with its model, the kill blow included
    /// (D-730). The player and each living enemy keep a box.
    /// </summary>
    [Fact]
    public void ADeadOwnerHasNoBox()
    {
        SimulationLoop loop = new(1, TestWorld.Content);
        Assert.True(loop.Enemies.Count >= 2, $"Floor 1 of seed 1 holds {loop.Enemies.Count} enemies, and the test needs two.");
        Enemy dead = loop.Enemies[0];
        dead.TakeHit(dead.Health);
        Assert.True(dead.IsDead);

        Dictionary<int, WhatYouCarry.Core.Physics.Aabb> boxes = Hud.EntityBoxes(loop);
        Assert.False(boxes.ContainsKey(dead.Owner), $"The dead enemy {dead.Owner} keeps a box.");
        Assert.True(boxes.ContainsKey(SimulationLoop.PlayerOwner));
        Assert.True(boxes.ContainsKey(loop.Enemies[1].Owner));
    }

    /// <summary>
    /// PR-98 exit test 1. The stairwell prompt stands at the bottom right of the Deck (D-728). The body box never
    /// overlaps it over the whole pitch range: with the full boom, at the nearest drawn camera of D-720, and with a
    /// wall on the right that pulls the shoulder point in (F-201). With the shoulder point in its place, the body box
    /// grown by the margin of the model also stays clear.
    /// </summary>
    [Fact]
    public void PromptClearsTheBody()
    {
        Rect2 prompt = HudLayout.For(Deck).Prompt;
        Assert.Equal(Deck.X - HudLayout.Margin, prompt.End.X, 3);
        Assert.Equal(Deck.Y - HudLayout.Margin, prompt.End.Y, 3);

        CoreVector3 feet = new(12.5f, TestWorld.FloorTop, 12.5f);
        CameraPose near = OrbitCamera.Place(PromptRoom(true, false), feet, 0, 0);
        Assert.Equal(OrbitCamera.ClosestView, (near.View - near.Shoulder).Length(), 3);

        (bool BackWall, bool RightWall, float Margin)[] cases =
        [
            (false, false, 0.0f), (true, false, 0.0f), (true, true, 0.0f),
            (false, false, Hud.SilhouetteMargin), (true, false, Hud.SilhouetteMargin),
        ];
        foreach ((bool backWall, bool rightWall, float margin) in cases)
        {
            VoxelGrid grid = PromptRoom(backWall, rightWall);
            for (int pitch = -SimulationLoop.PitchLimit; pitch <= SimulationLoop.PitchLimit; pitch += 250)
            {
                Rect2 body = BodyOnDeck(OrbitCamera.Place(grid, feet, 0, pitch), feet, margin);
                Assert.False(body.Intersects(prompt), $"Back wall {backWall}, right wall {rightWall}, margin {margin}, pitch {pitch}: the body box {body} overlaps the prompt {prompt}.");
            }
        }
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
        // A direction with no neighbor keeps the focus. Tab and Shift+Tab never read a direction, so they go on (F-135).
        Assert.Null(Navigation.Neighbor(controls, slider, FocusDirection.Left));
        Assert.Null(Navigation.Neighbor(controls, slider, FocusDirection.Right));
        Assert.Null(Navigation.Neighbor(controls, 0, FocusDirection.Up));
        Assert.Null(Navigation.Neighbor(controls, 2, FocusDirection.Right));
        Assert.Equal(3, Navigation.TabNext(controls, 2));
        Assert.Equal(2, Navigation.TabPrevious(controls, 3));
        Assert.Equal(0, Navigation.TabNext(controls, slider));
        Assert.Equal(slider, Navigation.TabPrevious(controls, 0));

        Rect2 screen = new(Vector2.Zero, UiScale.Base);
        Assert.All(controls, control => Assert.True(screen.Encloses(control.Box), $"The fixture control {control.Box} is not inside the Deck screen."));
    }

    /// <summary>
    /// F-135. Tab walks the focus map in its order and goes around from the last control to the first, and Shift+Tab
    /// walks it back, so each one visits every control of the fixture from every control. The end of a row, where the
    /// right neighbor is absent, is no stop.
    /// </summary>
    [Fact]
    public void TabWalksTheFocusMapAround()
    {
        IReadOnlyList<FocusControl> controls = NavigationFixture.Controls();
        for (int start = 0; start < controls.Count; start++)
        {
            int next = start;
            int previous = start;
            HashSet<int> forward = [];
            HashSet<int> backward = [];
            for (int step = 0; step < controls.Count; step++)
            {
                next = Navigation.TabNext(controls, next);
                previous = Navigation.TabPrevious(controls, previous);
                forward.Add(next);
                backward.Add(previous);
                Assert.Equal((start + step + 1) % controls.Count, next);
            }

            Assert.Equal(controls.Count, forward.Count);
            Assert.Equal(controls.Count, backward.Count);
            Assert.Equal(start, next);
            Assert.Equal(start, previous);
        }
    }

    /// <summary>F-135, the boundary. One control is its own next and previous, so the focus stays, and a screen of no controls is an error (T-2).</summary>
    [Fact]
    public void TabOnOneControlStaysAndOnNoControlFails()
    {
        FocusControl only = new(new Rect2(0.0f, 0.0f, 100.0f, 50.0f), false);
        Assert.Equal(0, Navigation.TabNext([only], 0));
        Assert.Equal(0, Navigation.TabPrevious([only], 0));
        ContextException none = Assert.Throws<ContextException>(() => Navigation.TabNext([], 0));
        Assert.Contains(Navigation.NoControlsMessage, none.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => Navigation.TabPrevious([], 0));
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
        Assert.True(new HudState(100, 100, 600, false, true, true, false).Paused);
        Assert.False(new HudState(100, 100, 600, false, false, false, false).Paused);
        Assert.False(new HudState(100, 100, 0, true, true, true, false).Paused);
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
    /// <summary>
    /// A room of 24 by 12 by 24 over a stone floor. At yaw zero the camera looks toward minus Z, so a back wall fills
    /// z = 14 and past it, 1.5 meters behind the feet at z = 12.5. A right wall fills x = 13 and past it.
    /// </summary>
    private static VoxelGrid PromptRoom(bool backWall, bool rightWall)
    {
        VoxelGrid grid = TestWorld.FlatFloor(24, 12);
        for (int x = 0; x < 24; x++)
        {
            for (int y = 1; y < 12; y++)
            {
                for (int z = 0; z < 24; z++)
                {
                    if ((backWall && z >= 14) || (rightWall && x >= 13))
                    {
                        grid.Set(x, y, z, BlockId.RawStone);
                    }
                }
            }
        }

        return grid;
    }

    /// <summary>
    /// The screen box on the Deck of the body box on its feet, grown by a margin on each side, from the drawn camera of
    /// a pose. The engine camera keeps the height of the view, so one focal length in pixels serves
    /// both axes. The box stops at the screen edge.
    /// </summary>
    private static Rect2 BodyOnDeck(CameraPose pose, CoreVector3 feet, float margin)
    {
        float focal = (Deck.Y / 2.0f) / MathF.Tan(PlaceholderScene.ViewDegrees * MathF.PI / 360.0f);
        float side = PlayerBody.HalfWidth + margin;
        List<Vector2> points = [];
        for (int corner = 0; corner < 8; corner++)
        {
            CoreVector3 point = new(
                feet.X + ((corner & 1) == 0 ? -side : side),
                feet.Y + ((corner & 2) == 0 ? -margin : PlayerBody.Height + margin),
                feet.Z + ((corner & 4) == 0 ? -side : side));
            CoreVector3 fromView = point - pose.View;
            float depth = CoreVector3.Dot(fromView, pose.Forward);
            Assert.True(depth > 0.0f, $"The body corner {point} stands behind the drawn camera {pose.View}.");
            points.Add(new Vector2(
                (Deck.X / 2.0f) + (CoreVector3.Dot(fromView, pose.Right) / depth * focal),
                (Deck.Y / 2.0f) - (CoreVector3.Dot(fromView, pose.Up) / depth * focal)));
        }

        return DamageNumbers.BoxOf(points).Intersection(new Rect2(Vector2.Zero, Deck));
    }
}
