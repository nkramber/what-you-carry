using CoreButton = WhatYouCarry.Core.Simulation.Button;

namespace WhatYouCarry.Game.Input;

/// <summary>
/// The tap and the hold of the interact input at the open stairwell prompt (D-448). A tap descends, and a hold of
/// one second ascends. Core reads the interact bit and the ascend bit of the intent (D-257), so this type sets each
/// bit on one tick and Core does not change.
/// </summary>
/// <remarks>
/// <para>
/// While the prompt is open, the interact bit of the raw input never reaches the intent as it is. A press that
/// starts at the open prompt arms the hold. The tick of a release before one second sets the interact bit, and the
/// tick that the hold reaches one second sets the ascend bit. After either one, the press counts no more until the
/// next press.
/// </para>
/// <para>
/// A press that starts before the prompt opens does not arm the hold, so a player who holds interact onto the cell
/// does not end the run (D-448). While the prompt is closed, the buttons pass as they are. A prompt that closes
/// during a hold disarms it, and the release then sets no bit.
/// </para>
/// </remarks>
public sealed class StairwellHold
{
    /// <summary>The ticks of a hold that ascends: one second at 60 ticks each second (D-448).</summary>
    public const int HoldTicks = 60;

    private bool wasDown;
    private bool armed;
    private int heldTicks;

    /// <summary>The ticks of the hold that is armed now, or zero.</summary>
    public int HeldTicks => this.armed ? this.heldTicks : 0;

    /// <summary>The buttons of one tick, after the tap and the hold at a prompt that is open or closed.</summary>
    public ushort Apply(ushort buttons, bool promptOpen)
    {
        bool down = (buttons & CoreButton.Interact) != 0;
        bool pressed = down && !this.wasDown;
        bool released = !down && this.wasDown;
        this.wasDown = down;

        if (!promptOpen)
        {
            this.armed = false;
            this.heldTicks = 0;
            return buttons;
        }

        ushort others = (ushort)(buttons & ~CoreButton.Interact);
        if (pressed)
        {
            this.armed = true;
            this.heldTicks = 0;
        }

        if (!this.armed)
        {
            return others;
        }

        if (released)
        {
            this.armed = false;
            return (ushort)(others | CoreButton.Interact);
        }

        this.heldTicks++;
        if (this.heldTicks >= HoldTicks)
        {
            this.armed = false;
            return (ushort)(others | CoreButton.Ascend);
        }

        return others;
    }
}
