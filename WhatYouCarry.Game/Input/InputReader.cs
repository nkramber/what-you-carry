using Godot;
using CoreButton = WhatYouCarry.Core.Simulation.Button;

namespace WhatYouCarry.Game.Input;

/// <summary>
/// Reads the keyboard, the mouse, and the first controller once per tick into a <see cref="RawInput"/>
/// (D-15, D-289). The mouse motion sums between two ticks, because the engine reports it per event and a
/// tick takes the whole motion since the last one.
/// </summary>
/// <remarks>
/// <para>
/// The bindings are the ones of D-289 for the actions that have a bit in <see cref="CoreButton"/>: jump, sprint,
/// dodge, attack, and interact. Block, throwable, reload, satchel, and amulet have no bit yet, and the PR that
/// assigns each bit adds its binding here. The keyboard and the controller both work at once, and Core clamps
/// the sum of the two (D-233).
/// </para>
/// <para>
/// The engine reports the two shift keys as one key and the two control keys as one key. D-289 names the left
/// key of each pair, and the right one works too.
/// </para>
/// </remarks>
public sealed class InputReader
{
    /// <summary>Forward on the keyboard (D-289).</summary>
    public const Key ForwardKey = Key.W;

    /// <summary>Back on the keyboard (D-289).</summary>
    public const Key BackKey = Key.S;

    /// <summary>Strafe left on the keyboard (D-289).</summary>
    public const Key LeftKey = Key.A;

    /// <summary>Strafe right on the keyboard (D-289).</summary>
    public const Key RightKey = Key.D;

    /// <summary>Jump on the keyboard (D-289).</summary>
    public const Key JumpKey = Key.Space;

    /// <summary>Sprint on the keyboard: Left Shift (D-289).</summary>
    public const Key SprintKey = Key.Shift;

    /// <summary>Dodge on the keyboard: Left Control (D-289).</summary>
    public const Key DodgeKey = Key.Ctrl;

    /// <summary>Interact on the keyboard (D-289).</summary>
    public const Key InteractKey = Key.E;

    /// <summary>Attack on the mouse (D-289).</summary>
    public const MouseButton AttackButton = MouseButton.Left;

    /// <summary>The device index of the first controller.</summary>
    public const int FirstController = 0;

    /// <summary>Strafe on the controller: the left stick (D-289).</summary>
    public const JoyAxis MoveAxisX = JoyAxis.LeftX;

    /// <summary>Forward on the controller: the left stick, with down positive in the engine (D-289).</summary>
    public const JoyAxis MoveAxisY = JoyAxis.LeftY;

    /// <summary>Look on the controller: the right stick (D-289).</summary>
    public const JoyAxis LookAxisX = JoyAxis.RightX;

    /// <summary>Look on the controller: the right stick, with down positive in the engine (D-289).</summary>
    public const JoyAxis LookAxisY = JoyAxis.RightY;

    /// <summary>Attack on the controller: the right trigger (D-289).</summary>
    public const JoyAxis AttackAxis = JoyAxis.TriggerRight;

    /// <summary>Jump on the controller (D-289).</summary>
    public const JoyButton JumpButton = JoyButton.A;

    /// <summary>Dodge on the controller (D-289).</summary>
    public const JoyButton DodgeButton = JoyButton.B;

    /// <summary>Sprint on the controller: the left bumper (D-289).</summary>
    public const JoyButton SprintButton = JoyButton.LeftShoulder;

    /// <summary>Interact on the controller (D-289).</summary>
    public const JoyButton InteractButton = JoyButton.X;

    /// <summary>The trigger deflection that counts as a press.</summary>
    public const float TriggerPressed = 0.5f;

    private float mouseX;
    private float mouseY;
    private bool controllerLook;

    /// <summary>Adds the motion of one mouse event to the sum of this tick. The mouse then holds the look (D-243).</summary>
    public void AddMouseMotion(InputEventMouseMotion motion)
    {
        this.mouseX += motion.Relative.X;
        this.mouseY += motion.Relative.Y;
        this.controllerLook = false;
    }

    /// <summary>The raw input of this tick. The mouse sum starts again after it.</summary>
    public RawInput Read()
    {
        float stickLookX = Godot.Input.GetJoyAxis(FirstController, LookAxisX);
        float stickLookY = Godot.Input.GetJoyAxis(FirstController, LookAxisY);
        if (IntentBuilder.StickCurve(stickLookX) != 0.0f || IntentBuilder.StickCurve(stickLookY) != 0.0f)
        {
            this.controllerLook = true;
        }

        float strafe = KeyAxis(RightKey, LeftKey) + Godot.Input.GetJoyAxis(FirstController, MoveAxisX);
        float forward = KeyAxis(ForwardKey, BackKey) - Godot.Input.GetJoyAxis(FirstController, MoveAxisY);

        ushort buttons = 0;
        if (Godot.Input.IsKeyPressed(JumpKey) || Godot.Input.IsJoyButtonPressed(FirstController, JumpButton))
        {
            buttons |= CoreButton.Jump;
        }

        if (Godot.Input.IsKeyPressed(SprintKey) || Godot.Input.IsJoyButtonPressed(FirstController, SprintButton))
        {
            buttons |= CoreButton.Sprint;
        }

        if (Godot.Input.IsKeyPressed(DodgeKey) || Godot.Input.IsJoyButtonPressed(FirstController, DodgeButton))
        {
            buttons |= CoreButton.Dodge;
        }

        if (Godot.Input.IsMouseButtonPressed(AttackButton) || Godot.Input.GetJoyAxis(FirstController, AttackAxis) >= TriggerPressed)
        {
            buttons |= CoreButton.Attack;
        }

        if (Godot.Input.IsKeyPressed(InteractKey) || Godot.Input.IsJoyButtonPressed(FirstController, InteractButton))
        {
            buttons |= CoreButton.Interact;
        }

        RawInput raw = new(this.mouseX, this.mouseY, stickLookX, stickLookY, strafe, forward, buttons, this.controllerLook);
        this.mouseX = 0.0f;
        this.mouseY = 0.0f;
        return raw;
    }

    /// <summary>One for the positive key, minus one for the negative key, and zero for both or neither.</summary>
    private static float KeyAxis(Key positive, Key negative)
    {
        float axis = 0.0f;
        if (Godot.Input.IsKeyPressed(positive))
        {
            axis += 1.0f;
        }

        if (Godot.Input.IsKeyPressed(negative))
        {
            axis -= 1.0f;
        }

        return axis;
    }
}
