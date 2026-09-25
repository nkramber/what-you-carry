using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Ui;

/// <summary>The four directions of the D-pad and of the left stick on a screen.</summary>
public enum FocusDirection
{
    /// <summary>Toward the top of the screen.</summary>
    Up,

    /// <summary>Toward the bottom of the screen.</summary>
    Down,

    /// <summary>Toward the left of the screen.</summary>
    Left,

    /// <summary>Toward the right of the screen.</summary>
    Right,
}

/// <summary>One control of a screen for the focus: its box in layout pixels, and whether left and right change its value.</summary>
/// <param name="Box">The box of the control on the base of <see cref="UiScale"/>.</param>
/// <param name="TakesSideways">True for a control such as a slider, whose value left and right change, so they never move the focus.</param>
public readonly record struct FocusControl(Rect2 Box, bool TakesSideways);

/// <summary>
/// The focus movement and the activation of every screen with a controller alone (D-15, D-90). Each direction of
/// a control names its neighbor, and <see cref="Apply"/> gives the neighbors to the engine, so the focus moves on
/// the same path on every platform.
/// </summary>
/// <remarks>
/// <para>
/// The neighbor in one direction is the nearest control whose center lies past the center of the control in that
/// direction. The distance counts the offset across the direction two times, so the focus keeps to its row or its
/// column before it jumps across. A direction with no such control has no neighbor, and the focus stays.
/// </para>
/// <para>
/// Tab and Shift+Tab follow the tab order, which is the order of the focus map, and they go around from the last
/// control to the first. They never follow a direction, because a direction with no neighbor keeps the focus, and
/// Tab then stopped at the last control of each row (F-135).
/// </para>
/// <para>
/// The engine moves the focus on its input actions for the directions and activates the control with the focus on
/// its accept action. The accept action of Godot 4.7 has no controller input, so <see cref="BindAccept"/> adds the A
/// button to it (D-449). <see cref="RequireControllerActions"/> stops a screen whose input map gives one of those
/// actions no controller input, so a screen never works with a mouse alone (D-15).
/// </para>
/// </remarks>
public static class Navigation
{
    /// <summary>The weight of the offset across the direction, against the distance along it.</summary>
    public const float AcrossWeight = 2.0f;

    /// <summary>The message of the error for a screen of no controls.</summary>
    public const string NoControlsMessage = "A screen for the focus holds one control or more.";

    /// <summary>The message of the error for an input action that has no controller input.</summary>
    public const string NoControllerActionMessage = "An input action of the focus has no controller input, and every screen works with a controller alone (D-15).";

    /// <summary>The message of the error for a list of boxes and a list of nodes that do not match.</summary>
    public const string CountMismatchMessage = "The focus map and the controls of a screen hold different counts.";

    private const string ActionField = "action";
    private const string BoxesField = "boxes";
    private const string NodesField = "nodes";

    private const string UpAction = "ui_up";
    private const string DownAction = "ui_down";
    private const string LeftAction = "ui_left";
    private const string RightAction = "ui_right";
    private const string AcceptAction = "ui_accept";

    /// <summary>
    /// The engine input actions that move the focus and activate a control. They stay names, and never an engine
    /// name type, so a test can read this class with no engine.
    /// </summary>
    private static readonly string[] FocusActions = [UpAction, DownAction, LeftAction, RightAction, AcceptAction];

    /// <summary>The controller button that activates the control with the focus (D-449).</summary>
    public const JoyButton AcceptButton = JoyButton.A;

    /// <summary>The device index of the engine that stands for every controller.</summary>
    public const int AllDevices = -1;

    /// <summary>The index of the neighbor of one control in one direction, or null when the focus stays.</summary>
    public static int? Neighbor(IReadOnlyList<FocusControl> controls, int from, FocusDirection direction)
    {
        bool sideways = direction is FocusDirection.Left or FocusDirection.Right;
        if (sideways && controls[from].TakesSideways)
        {
            return null;
        }

        Vector2 center = controls[from].Box.GetCenter();
        int? best = null;
        float bestScore = float.MaxValue;
        for (int index = 0; index < controls.Count; index++)
        {
            if (index == from)
            {
                continue;
            }

            Vector2 offset = controls[index].Box.GetCenter() - center;
            float along = direction switch
            {
                FocusDirection.Up => -offset.Y,
                FocusDirection.Down => offset.Y,
                FocusDirection.Left => -offset.X,
                _ => offset.X,
            };
            if (along <= 0.0f)
            {
                continue;
            }

            float across = sideways ? Math.Abs(offset.Y) : Math.Abs(offset.X);
            float score = along + (AcrossWeight * across);
            if (score < bestScore)
            {
                best = index;
                bestScore = score;
            }
        }

        return best;
    }

    /// <summary>
    /// The index of the control after one control in the tab order: the order of the focus map. The last control
    /// goes to the first, so Tab reaches every control of a screen from every control (F-135).
    /// </summary>
    /// <exception cref="ContextException">The screen holds no control.</exception>
    public static int TabNext(IReadOnlyList<FocusControl> controls, int from)
    {
        if (controls.Count == 0)
        {
            throw new ContextException(NoControlsMessage);
        }

        return (from + 1) % controls.Count;
    }

    /// <summary>
    /// The index of the control before one control in the tab order: the order of the focus map. The first control
    /// goes to the last, so Shift+Tab reaches every control of a screen from every control (F-135).
    /// </summary>
    /// <exception cref="ContextException">The screen holds no control.</exception>
    public static int TabPrevious(IReadOnlyList<FocusControl> controls, int from)
    {
        if (controls.Count == 0)
        {
            throw new ContextException(NoControlsMessage);
        }

        return (from + controls.Count - 1) % controls.Count;
    }

    /// <summary>The indexes of every control that the directions reach from one control, sorted.</summary>
    /// <exception cref="ContextException">The screen holds no control.</exception>
    public static IReadOnlyList<int> Reachable(IReadOnlyList<FocusControl> controls, int start)
    {
        if (controls.Count == 0)
        {
            throw new ContextException(NoControlsMessage);
        }

        bool[] seen = new bool[controls.Count];
        Queue<int> next = new();
        seen[start] = true;
        next.Enqueue(start);
        while (next.Count > 0)
        {
            int at = next.Dequeue();
            foreach (FocusDirection direction in Enum.GetValues<FocusDirection>())
            {
                if (Neighbor(controls, at, direction) is int neighbor && !seen[neighbor])
                {
                    seen[neighbor] = true;
                    next.Enqueue(neighbor);
                }
            }
        }

        List<int> reached = [];
        for (int index = 0; index < seen.Length; index++)
        {
            if (seen[index])
            {
                reached.Add(index);
            }
        }

        return reached;
    }

    /// <summary>
    /// Gives each control its neighbors from the focus map, and lets each one take the focus. The two lists match by
    /// index. A screen that shows gives the first focus with <see cref="FocusFirst"/>.
    /// </summary>
    /// <exception cref="ContextException">The lists hold different counts, or the screen holds no control.</exception>
    public static void Apply(IReadOnlyList<FocusControl> controls, IReadOnlyList<Control> nodes)
    {
        if (controls.Count != nodes.Count)
        {
            ContextException mismatch = new(CountMismatchMessage);
            mismatch.AddContext(BoxesField, controls.Count.ToString(CultureInfo.InvariantCulture));
            mismatch.AddContext(NodesField, nodes.Count.ToString(CultureInfo.InvariantCulture));
            throw mismatch;
        }

        if (nodes.Count == 0)
        {
            throw new ContextException(NoControlsMessage);
        }

        for (int index = 0; index < nodes.Count; index++)
        {
            Control node = nodes[index];
            node.FocusMode = Control.FocusModeEnum.All;
            node.FocusNeighborTop = PathTo(controls, nodes, index, FocusDirection.Up);
            node.FocusNeighborBottom = PathTo(controls, nodes, index, FocusDirection.Down);
            node.FocusNeighborLeft = PathTo(controls, nodes, index, FocusDirection.Left);
            node.FocusNeighborRight = PathTo(controls, nodes, index, FocusDirection.Right);

            // Tab and Shift+Tab walk the tab order of the focus map, and never stop at the end of a row (F-135).
            node.FocusNext = node.GetPathTo(nodes[TabNext(controls, index)]);
            node.FocusPrevious = node.GetPathTo(nodes[TabPrevious(controls, index)]);
        }
    }

    /// <summary>Gives the focus to the first control of a screen that shows, so a controller can start at once.</summary>
    /// <exception cref="ContextException">The screen holds no control.</exception>
    public static void FocusFirst(IReadOnlyList<Control> nodes)
    {
        if (nodes.Count == 0)
        {
            throw new ContextException(NoControlsMessage);
        }

        nodes[0].GrabFocus();
    }

    /// <summary>Adds the A button to the accept action of the engine, unless the action holds it already (D-449).</summary>
    public static void BindAccept()
    {
        foreach (InputEvent input in InputMap.ActionGetEvents(AcceptAction))
        {
            if (input is InputEventJoypadButton button && button.ButtonIndex == AcceptButton)
            {
                return;
            }
        }

        InputMap.ActionAddEvent(AcceptAction, new InputEventJoypadButton { ButtonIndex = AcceptButton, Device = AllDevices });
    }

    /// <summary>Stops a screen whose input map gives a focus action no controller button and no controller axis (D-15).</summary>
    /// <exception cref="ContextException">A focus action has no controller input.</exception>
    public static void RequireControllerActions()
    {
        foreach (string action in FocusActions)
        {
            bool controller = false;
            foreach (InputEvent input in InputMap.ActionGetEvents(action))
            {
                controller = controller || input is InputEventJoypadButton or InputEventJoypadMotion;
            }

            if (!controller)
            {
                ContextException error = new(NoControllerActionMessage);
                error.AddContext(ActionField, action);
                throw error;
            }
        }
    }

    /// <summary>The node path from one control to its neighbor, or the path to itself when the focus stays.</summary>
    private static NodePath PathTo(IReadOnlyList<FocusControl> controls, IReadOnlyList<Control> nodes, int from, FocusDirection direction)
    {
        int to = Neighbor(controls, from, direction) ?? from;
        return nodes[from].GetPathTo(nodes[to]);
    }
}
