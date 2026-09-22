using System.Collections.Generic;
using System.Globalization;
using Godot;
using WhatYouCarry.Core.Content;

namespace WhatYouCarry.Game.Ui;

/// <summary>The layer of the navigation fixture, and its controls in the order of the focus map.</summary>
public sealed record NavigationFixtureNodes(CanvasLayer Layer, IReadOnlyList<Control> Controls);

/// <summary>
/// The fixture screen of the navigation test (D-446): six buttons in a grid of three columns and two rows, and one
/// slider under them. No menu screen exists before PR-53, so this screen proves the focus map of
/// <see cref="Navigation"/> on the kinds of control that the later screens hold.
/// </summary>
/// <remarks>
/// The smoke session builds the screen hidden, so the engine part of the focus map runs on every platform
/// (D-114). The grid tests the four directions, and the slider tests a control whose value left and right change.
/// </remarks>
public static class NavigationFixture
{
    /// <summary>The count of columns of the button grid.</summary>
    public const int Columns = 3;

    /// <summary>The count of rows of the button grid.</summary>
    public const int Rows = 2;

    /// <summary>The width of a button and the width of the slider.</summary>
    public const float ButtonWide = 240.0f;

    /// <summary>The height of a button and of the slider.</summary>
    public const float ButtonHigh = 56.0f;

    /// <summary>The space between two buttons.</summary>
    public const float Spacing = 24.0f;

    /// <summary>The id of the format of a button label: the number of the button, from one.</summary>
    public const string ButtonId = "ui.fixture.button";

    /// <summary>The id of the label of the slider.</summary>
    public const string SliderId = "ui.fixture.slider";

    /// <summary>The font size of a label.</summary>
    public const int FontPixels = 22;

    /// <summary>The controls of the fixture on the base of <see cref="UiScale"/>: the buttons row by row, then the slider.</summary>
    public static IReadOnlyList<FocusControl> Controls()
    {
        float gridWide = (Columns * ButtonWide) + ((Columns - 1) * Spacing);
        float gridHigh = ((Rows + 1) * ButtonHigh) + (Rows * Spacing);
        float left = (UiScale.BaseWide - gridWide) / 2.0f;
        float top = (UiScale.BaseHigh - gridHigh) / 2.0f;

        List<FocusControl> controls = [];
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                Rect2 box = new(left + (column * (ButtonWide + Spacing)), top + (row * (ButtonHigh + Spacing)), ButtonWide, ButtonHigh);
                controls.Add(new FocusControl(box, false));
            }
        }

        Rect2 slider = new((UiScale.BaseWide - ButtonWide) / 2.0f, top + (Rows * (ButtonHigh + Spacing)), ButtonWide, ButtonHigh);
        controls.Add(new FocusControl(slider, true));
        return controls;
    }

    /// <summary>The hidden screen: a layer with one control for each entry of <see cref="Controls"/>, and the focus map applied.</summary>
    /// <remarks>The caller adds the layer to the tree first, because a node path needs both nodes in one tree.</remarks>
    public static NavigationFixtureNodes Build(Strings strings, Node parent)
    {
        IReadOnlyList<FocusControl> controls = Controls();
        CanvasLayer layer = new() { Visible = false };
        parent.AddChild(layer);
        LabelSettings font = new() { FontSize = FontPixels };

        List<Control> nodes = [];
        for (int index = 0; index < controls.Count; index++)
        {
            Control node;
            if (controls[index].TakesSideways)
            {
                VBoxContainer holder = new();
                holder.AddChild(new Label { Text = strings.Get(SliderId), LabelSettings = font });
                HSlider slider = new() { MinValue = 0.0, MaxValue = 1.0, Step = 0.1, Value = 0.5 };
                holder.AddChild(slider);
                layer.AddChild(holder);
                holder.Position = controls[index].Box.Position;
                holder.Size = controls[index].Box.Size;
                node = slider;
            }
            else
            {
                string text = string.Format(CultureInfo.InvariantCulture, strings.Get(ButtonId), index + 1);
                Button button = new() { Text = text };
                layer.AddChild(button);
                button.Position = controls[index].Box.Position;
                button.Size = controls[index].Box.Size;
                node = button;
            }

            nodes.Add(node);
        }

        Navigation.Apply(controls, nodes);
        return new NavigationFixtureNodes(layer, nodes);
    }
}
