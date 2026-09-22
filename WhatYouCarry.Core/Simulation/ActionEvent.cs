namespace WhatYouCarry.Core.Simulation;

/// <summary>A gameplay fact of one tick that the Game layer plays a sound for (D-454). The timer expiry and the hunter spawn stay timer events.</summary>
public enum ActionEventKind
{
    /// <summary>A press of the attack bit started a swing of the player. The value is zero.</summary>
    SwingStart = 0,

    /// <summary>The blade of the player hit a target. The value is the owner id of the target.</summary>
    SwingHit = 1,

    /// <summary>A press of the dodge bit started a roll of the player. The value is zero.</summary>
    Dodge = 2,

    /// <summary>A hit landed on the player: it met no roll (D-328). The value is the damage.</summary>
    PlayerHit = 3,

    /// <summary>The running countdown reached a timer mark (D-456). The value is the seconds left.</summary>
    TimerMark = 4,
}

/// <summary>One action event: its kind, the tick of the intent that it came on, and a value that the kind names.</summary>
/// <param name="Kind">What happened.</param>
/// <param name="Tick">The tick of the intent that the event came on.</param>
/// <param name="Value">The owner id of a hit target, the damage of a hit on the player, the seconds left at a timer mark, or zero.</param>
public sealed record ActionEvent(ActionEventKind Kind, uint Tick, long Value);
