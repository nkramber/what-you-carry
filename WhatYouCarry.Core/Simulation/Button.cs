namespace WhatYouCarry.Core.Simulation;

/// <summary>
/// The bits of the button mask of an intent (D-162, D-232, D-243). Bits 9 to 15 are reserved, and a set reserved
/// bit is an error.
/// </summary>
/// <remarks>
/// The whole list is one decision, so a later PR cites its bit and needs no decision of its own. PR-7 reads
/// jump and sprint, and PR-8 reads the controller aim flag. The other six wait for the PR that owns each action.
/// </remarks>
public static class Button
{
    /// <summary>Bit 0. The body jumps when it stands on the ground (PR-7).</summary>
    public const ushort Jump = 0x0001;

    /// <summary>Bit 1. The body moves at the sprint speed (PR-7).</summary>
    public const ushort Sprint = 0x0002;

    /// <summary>Bit 2. The dodge roll (PR-15).</summary>
    public const ushort Dodge = 0x0004;

    /// <summary>Bit 3. The attack with the main weapon (PR-15).</summary>
    public const ushort Attack = 0x0008;

    /// <summary>Bit 4. The use of the quick slot item (PR-23).</summary>
    public const ushort Use = 0x0010;

    /// <summary>Bit 5. The interaction with the world, such as the stairwell (PR-9).</summary>
    public const ushort Interact = 0x0020;

    /// <summary>Bit 6. The quick slot moves to the next item (PR-23).</summary>
    public const ushort QuickSlotNext = 0x0040;

    /// <summary>Bit 7. The quick slot moves to the previous item (PR-23).</summary>
    public const ushort QuickSlotPrevious = 0x0080;

    /// <summary>Bit 8. A controller aims on this tick, so aim assist runs (D-243, PR-8). The Game layer sets it from the device that gave the look input.</summary>
    public const ushort ControllerAim = 0x0100;

    /// <summary>The nine assigned bits.</summary>
    public const ushort AssignedMask = 0x01FF;

    /// <summary>The seven reserved bits. A set one is an error (D-232, D-243).</summary>
    public const ushort ReservedMask = 0xFE00;
}
