using Godot;
using WhatYouCarry.Core.Content;

namespace WhatYouCarry.Game.Ui;

/// <summary>
/// The screen text of the stairwell prompt (D-50, D-431): the two choices, from the string table (G-8). The text
/// shows while the prompt is open and hides when it closes. The HUD of PR-19 places it and names the buttons.
/// </summary>
public sealed class StairwellPromptNodes
{
    private const string DescendId = "run.descend";
    private const string AscendId = "run.ascend";

    private StairwellPromptNodes(CanvasLayer layer, VBoxContainer box)
    {
        this.Layer = layer;
        this.Box = box;
    }

    /// <summary>The layer over the world that holds the text.</summary>
    public CanvasLayer Layer { get; }

    /// <summary>The box of the two labels. It shows while the prompt is open.</summary>
    public VBoxContainer Box { get; }

    /// <summary>The layer and the two labels, hidden, with the text of the string table.</summary>
    /// <exception cref="WhatYouCarry.Core.Logging.ContextException">The string table holds no text for a choice.</exception>
    public static StairwellPromptNodes Build(Strings strings)
    {
        VBoxContainer box = new() { Visible = false };
        box.SetAnchorsPreset(Control.LayoutPreset.Center);
        box.AddChild(new Label { Text = strings.Get(DescendId), HorizontalAlignment = HorizontalAlignment.Center });
        box.AddChild(new Label { Text = strings.Get(AscendId), HorizontalAlignment = HorizontalAlignment.Center });
        CanvasLayer layer = new();
        layer.AddChild(box);
        return new StairwellPromptNodes(layer, box);
    }

    /// <summary>Shows the text while the prompt is open, and hides it when the prompt is closed.</summary>
    public void Show(bool open)
    {
        this.Box.Visible = open;
    }
}
