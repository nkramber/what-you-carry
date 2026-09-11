namespace WhatYouCarry.Game.Input;

/// <summary>
/// The raw input of one tick, before the sensitivity and the quantization (D-77): the mouse motion in pixels
/// since the last tick, the look stick deflection, the movement fractions, the pressed buttons as the Core
/// bits, and whether a controller gave the last look input (D-243).
/// </summary>
/// <remarks>
/// The strafe is positive to the right, and the forward is positive ahead (D-233). A stick deflection runs
/// from minus one to one with the sign of the engine: right and down are positive. The buttons hold no
/// controller aim bit, because <see cref="IntentBuilder"/> sets that one from <see cref="ControllerLook"/>.
/// </remarks>
public readonly record struct RawInput(
    float MouseX,
    float MouseY,
    float StickLookX,
    float StickLookY,
    float Strafe,
    float Forward,
    ushort Buttons,
    bool ControllerLook);
