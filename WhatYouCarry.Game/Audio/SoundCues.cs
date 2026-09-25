using System.Globalization;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Game.Audio;

/// <summary>
/// The cue of each Core event (D-454, D-456). Every action event has a cue. Of the timer events, the hunter spawn has a
/// cue. The expiry, a wave, and a wave skip have none: the Overseer spawns on the tick of expiry, and its sound carries
/// that moment (D-469).
/// </summary>
public static class SoundCues
{
    /// <summary>The marks, in seconds left, that sound the alarm. Every other mark sounds the tick (D-456).</summary>
    public static readonly int[] AlarmSeconds = [60, 30];

    private const string UnknownKind = "The event has a kind that no sound cue names.";
    private const string UnknownMark = "The timer mark is not a mark of the floor timer (D-456).";
    private const string KindField = "kind";
    private const string SecondsField = "seconds";

    /// <summary>The cue of one action event.</summary>
    /// <exception cref="ContextException">The kind is unknown, or a timer mark names seconds that are not a mark (T-2).</exception>
    public static SoundCue Of(ActionEvent action)
    {
        switch (action.Kind)
        {
            case ActionEventKind.SwingStart: return SoundCue.SwordSwing;
            case ActionEventKind.SwingHit: return SoundCue.SwordHit;
            case ActionEventKind.Dodge: return SoundCue.Dodge;
            case ActionEventKind.PlayerHit: return SoundCue.PlayerHit;
            case ActionEventKind.TimerMark: return MarkCue(action.Value);
        }

        ContextException error = new(UnknownKind);
        error.AddContext(KindField, ((int)action.Kind).ToString(CultureInfo.InvariantCulture));
        throw error;
    }

    /// <summary>The cue of one timer event, when the event has one.</summary>
    /// <returns>True for the hunter spawn, and false for the expiry, a wave, and a wave skip.</returns>
    /// <exception cref="ContextException">The kind is unknown (T-2).</exception>
    public static bool TryOf(TimerEvent timerEvent, out SoundCue cue)
    {
        switch (timerEvent.Kind)
        {
            case TimerEventKind.HunterSpawn:
                cue = SoundCue.HunterSpawn;
                return true;
            case TimerEventKind.Expiry:
            case TimerEventKind.Wave:
            case TimerEventKind.WaveSkip:
                cue = SoundCue.HunterSpawn;
                return false;
        }

        ContextException error = new(UnknownKind);
        error.AddContext(KindField, ((int)timerEvent.Kind).ToString(CultureInfo.InvariantCulture));
        throw error;
    }

    /// <summary>The alarm at 60 and 30 seconds left, and the tick at each other mark (D-456).</summary>
    private static SoundCue MarkCue(long seconds)
    {
        if (!IsTimerMark(seconds))
        {
            ContextException error = new(UnknownMark);
            error.AddContext(SecondsField, seconds.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        return System.Array.IndexOf(AlarmSeconds, (int)seconds) >= 0 ? SoundCue.TimerAlarm : SoundCue.TimerTick;
    }

    /// <summary>Answers whether a count of seconds left is one of the marks of <see cref="FloorTimer.MarkSeconds"/> (D-456).</summary>
    private static bool IsTimerMark(long seconds)
    {
        foreach (int mark in FloorTimer.MarkSeconds)
        {
            if (mark == seconds)
            {
                return true;
            }
        }

        return false;
    }
}
