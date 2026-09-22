using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Audio;

/// <summary>One sound of the game: what the Game layer plays for an event or a stride (D-454).</summary>
public enum SoundCue
{
    /// <summary>The player swing starts.</summary>
    SwordSwing = 0,

    /// <summary>The blade of the player hits a target.</summary>
    SwordHit = 1,

    /// <summary>A foot of the player comes down.</summary>
    Footstep = 2,

    /// <summary>The player dodges.</summary>
    Dodge = 3,

    /// <summary>A hit lands on the player.</summary>
    PlayerHit = 4,

    /// <summary>The Overseer spawns.</summary>
    HunterSpawn = 5,

    /// <summary>A foot of the Overseer comes down.</summary>
    HunterStep = 6,

    /// <summary>The countdown reaches 60 or 30 seconds left (D-456).</summary>
    TimerAlarm = 7,

    /// <summary>The countdown reaches one of the last marks (D-456).</summary>
    TimerTick = 8,
}

/// <summary>
/// The binding of one cue: the bus, the rendered file, and whether the sound plays at the place of the Overseer, so its
/// step comes nearer (D-409). One sound is one file, and the synthesizer mixes its parts (D-462).
/// </summary>
public sealed record SoundBinding(SoundCue Cue, string Bus, string File, bool AtHunter);

/// <summary>One bus of the mix and its default volume, from 0 to 1 (D-452).</summary>
public sealed record BusDefault(string Name, float Volume);

/// <summary>
/// The buses of the mix and the binding of each cue (D-451, D-452, D-453). Every sound of the first set goes to the
/// Effects bus. The UI bus waits for the menu sounds, and the Music bus waits for PR-50.
/// </summary>
public static class SoundBindings
{
    /// <summary>The bus that every other bus sends to. The engine names it.</summary>
    public const string MasterBus = "Master";

    /// <summary>The bus of the sound effects.</summary>
    public const string EffectsBus = "Effects";

    /// <summary>The bus of the music (PR-50).</summary>
    public const string MusicBus = "Music";

    /// <summary>The bus of the menu sounds.</summary>
    public const string UiBus = "UI";

    private const string NoBinding = "No sound binding names the cue.";
    private const string CueField = "cue";
    private const string SwordSwing = AssetPaths.SoundDirectory + "sword-swing.wav";
    private const string SwordHit = AssetPaths.SoundDirectory + "sword-hit.wav";
    private const string Footstep = AssetPaths.SoundDirectory + "footstep.wav";
    private const string Dodge = AssetPaths.SoundDirectory + "dodge.wav";
    private const string PlayerHit = AssetPaths.SoundDirectory + "player-hit.wav";
    private const string HunterSpawn = AssetPaths.SoundDirectory + "hunter-spawn.wav";
    private const string HunterStep = AssetPaths.SoundDirectory + "hunter-step.wav";
    private const string TimerAlarm = AssetPaths.SoundDirectory + "timer-alarm.wav";
    private const string TimerTick = AssetPaths.SoundDirectory + "timer-tick.wav";

    /// <summary>The buses in the order the boot adds them, Master first, with the default volumes of D-452.</summary>
    public static readonly IReadOnlyList<BusDefault> Buses =
    [
        new BusDefault(MasterBus, 1.0f),
        new BusDefault(EffectsBus, 1.0f),
        new BusDefault(MusicBus, 0.7f),
        new BusDefault(UiBus, 0.8f),
    ];

    /// <summary>The binding of every cue, in the order of the cue values.</summary>
    public static readonly IReadOnlyList<SoundBinding> All =
    [
        new SoundBinding(SoundCue.SwordSwing, EffectsBus, SwordSwing, false),
        new SoundBinding(SoundCue.SwordHit, EffectsBus, SwordHit, false),
        new SoundBinding(SoundCue.Footstep, EffectsBus, Footstep, false),
        new SoundBinding(SoundCue.Dodge, EffectsBus, Dodge, false),
        new SoundBinding(SoundCue.PlayerHit, EffectsBus, PlayerHit, false),
        new SoundBinding(SoundCue.HunterSpawn, EffectsBus, HunterSpawn, false),
        new SoundBinding(SoundCue.HunterStep, EffectsBus, HunterStep, true),
        new SoundBinding(SoundCue.TimerAlarm, EffectsBus, TimerAlarm, false),
        new SoundBinding(SoundCue.TimerTick, EffectsBus, TimerTick, false),
    ];

    /// <summary>The binding of one cue.</summary>
    /// <exception cref="ContextException">No binding names the cue (T-2).</exception>
    public static SoundBinding Of(SoundCue cue)
    {
        foreach (SoundBinding binding in All)
        {
            if (binding.Cue == cue)
            {
                return binding;
            }
        }

        ContextException error = new(NoBinding);
        error.AddContext(CueField, ((int)cue).ToString(CultureInfo.InvariantCulture));
        throw error;
    }
}
