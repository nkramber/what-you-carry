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
/// The look device is the one of the latest look event (D-243). A mouse motion event names the mouse, and a
/// stick motion event past the dead zone on a look axis names the controller. A held stick sends no new event,
/// so a later mouse motion takes the look, and the stick takes it back when it moves again. The poll of each
/// tick reads the stick deflection and never the device (PR #49 review P2-1).
/// </para>
/// <para>
/// The prompt device is the one of the latest input of any kind (D-447). It is apart from the look device, because a
/// press of a key names the keyboard while the stick still holds the look. The prompt names its buttons from it.
/// </para>
/// <para>
/// The engine reports the two shift keys as one key and the two control keys as one key. D-289 names the left
/// key of each pair, and the right one works too.
/// </para>
/// <para>
/// The poll reads each button at the moment of the tick, so a press and a release inside one long frame would
/// never reach a tick. The reader therefore latches the press edge of each binding from the input events, and the
/// next read sets the bit of each latched press with the bits of the poll (F-135). The tick after it reads the poll
/// alone, so Core sees the press and then the release.
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

    private readonly IInputPoll poll;
    private float mouseX;
    private float mouseY;
    private bool controllerLook;
    private bool controllerLast;
    private ushort latched;

    /// <summary>A reader over one poll.</summary>
    public InputReader(IInputPoll poll)
    {
        this.poll = poll;
    }

    /// <summary>Adds the motion of one mouse event to the sum of this tick. The mouse then holds the look (D-243).</summary>
    public void AddMouseMotion(float relativeX, float relativeY)
    {
        this.mouseX += relativeX;
        this.mouseY += relativeY;
        this.controllerLook = false;
    }

    /// <summary>
    /// Takes one stick motion event. The controller holds the look when the event moves a look axis of the
    /// first controller past the dead zone (D-243). Any other event changes nothing.
    /// </summary>
    public void AddLookStickMotion(int device, JoyAxis axis, float value)
    {
        if (device != FirstController)
        {
            return;
        }

        if (axis != LookAxisX && axis != LookAxisY)
        {
            return;
        }

        if (IntentBuilder.StickCurve(value) != 0.0f)
        {
            this.controllerLook = true;
        }
    }

    /// <summary>True when the latest input came from the controller, and false when it came from the keyboard or the mouse (D-447).</summary>
    public bool ControllerLast => this.controllerLast;

    /// <summary>Takes one input of the keyboard or the mouse: a key, a mouse button, or a mouse motion (D-447).</summary>
    public void NoteKeyboardOrMouse()
    {
        this.controllerLast = false;
    }

    /// <summary>Takes one press of a controller button (D-447).</summary>
    public void NoteControllerButton()
    {
        this.controllerLast = true;
    }

    /// <summary>Takes one axis motion of a controller. A motion inside the dead zone names no device, so a stick at rest never takes the prompt (D-447).</summary>
    public void NoteControllerMotion(float value)
    {
        if (IntentBuilder.StickCurve(value) != 0.0f)
        {
            this.controllerLast = true;
        }
    }

    /// <summary>Takes one press edge of a key. A key of a binding with a bit latches that bit until the next read (F-135).</summary>
    public void LatchKeyPress(Key key)
    {
        if (key == JumpKey)
        {
            this.latched |= CoreButton.Jump;
        }
        else if (key == SprintKey)
        {
            this.latched |= CoreButton.Sprint;
        }
        else if (key == DodgeKey)
        {
            this.latched |= CoreButton.Dodge;
        }
        else if (key == InteractKey)
        {
            this.latched |= CoreButton.Interact;
        }
    }

    /// <summary>Takes one press edge of a mouse button. The attack button latches the attack bit until the next read (F-135).</summary>
    public void LatchMouseButtonPress(MouseButton button)
    {
        if (button == AttackButton)
        {
            this.latched |= CoreButton.Attack;
        }
    }

    /// <summary>Takes one press edge of a controller button. A button of the first controller with a bit latches that bit until the next read (F-135).</summary>
    public void LatchJoyButtonPress(int device, JoyButton button)
    {
        if (device != FirstController)
        {
            return;
        }

        if (button == JumpButton)
        {
            this.latched |= CoreButton.Jump;
        }
        else if (button == SprintButton)
        {
            this.latched |= CoreButton.Sprint;
        }
        else if (button == DodgeButton)
        {
            this.latched |= CoreButton.Dodge;
        }
        else if (button == InteractButton)
        {
            this.latched |= CoreButton.Interact;
        }
    }

    /// <summary>Takes one axis motion of a controller. The attack trigger of the first controller at the press point or past it latches the attack bit until the next read (F-135).</summary>
    public void LatchTriggerMotion(int device, JoyAxis axis, float value)
    {
        if (device == FirstController && axis == AttackAxis && value >= TriggerPressed)
        {
            this.latched |= CoreButton.Attack;
        }
    }

    /// <summary>The raw input of this tick: the bits of the poll and of the presses latched since the last read. The mouse sum and the latch start again after it.</summary>
    public RawInput Read()
    {
        float stickLookX = this.poll.GetJoyAxis(FirstController, LookAxisX);
        float stickLookY = this.poll.GetJoyAxis(FirstController, LookAxisY);
        float strafe = this.KeyAxis(RightKey, LeftKey) + this.poll.GetJoyAxis(FirstController, MoveAxisX);
        float forward = this.KeyAxis(ForwardKey, BackKey) - this.poll.GetJoyAxis(FirstController, MoveAxisY);

        ushort buttons = 0;
        if (this.poll.IsKeyPressed(JumpKey) || this.poll.IsJoyButtonPressed(FirstController, JumpButton))
        {
            buttons |= CoreButton.Jump;
        }

        if (this.poll.IsKeyPressed(SprintKey) || this.poll.IsJoyButtonPressed(FirstController, SprintButton))
        {
            buttons |= CoreButton.Sprint;
        }

        if (this.poll.IsKeyPressed(DodgeKey) || this.poll.IsJoyButtonPressed(FirstController, DodgeButton))
        {
            buttons |= CoreButton.Dodge;
        }

        if (this.poll.IsMouseButtonPressed(AttackButton) || this.poll.GetJoyAxis(FirstController, AttackAxis) >= TriggerPressed)
        {
            buttons |= CoreButton.Attack;
        }

        if (this.poll.IsKeyPressed(InteractKey) || this.poll.IsJoyButtonPressed(FirstController, InteractButton))
        {
            buttons |= CoreButton.Interact;
        }

        // A press that ended before this tick still reaches it through the latch (F-135).
        buttons |= this.latched;

        RawInput raw = new(this.mouseX, this.mouseY, stickLookX, stickLookY, strafe, forward, buttons, this.controllerLook);
        this.mouseX = 0.0f;
        this.mouseY = 0.0f;
        this.latched = 0;
        return raw;
    }

    /// <summary>One for the positive key, minus one for the negative key, and zero for both or neither.</summary>
    private float KeyAxis(Key positive, Key negative)
    {
        float axis = 0.0f;
        if (this.poll.IsKeyPressed(positive))
        {
            axis += 1.0f;
        }

        if (this.poll.IsKeyPressed(negative))
        {
            axis -= 1.0f;
        }

        return axis;
    }
}
