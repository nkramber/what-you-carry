using System.Collections.Generic;
using System.Globalization;
using Godot;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Content;
using WhatYouCarry.Game.Render;

namespace WhatYouCarry.Game.Audio;

/// <summary>
/// The buses of the mix and one player for each rendered file (D-451, D-452, D-453). The boot reads every WAV file of
/// the bindings from the content directory, and the Game layer plays the sounds of each tick through it.
/// </summary>
/// <remarks>
/// <para>
/// An absent file, a file that the engine cannot decode, and a bus that already exists are each an error that names
/// the file or the bus, and never a silent game (T-2). The smoke session loads the bank at boot on every platform.
/// </para>
/// <para>
/// Each player holds a few voices, so a sound can start again before its last one ends. The step of the Overseer plays
/// from a player at its place, so it comes nearer (D-409). The pitch of a play sets the pitch of the player of its cue.
/// </para>
/// <para>
/// The end of a session stops every player and releases every stream, because a playback or a stream that a player
/// still holds at the quit is an object that the engine reports as leaked.
/// </para>
/// <para>
/// The dummy driver of a headless session mixes nothing, so a playback there never ends, and the engine reports each
/// one as leaked at the quit. On that driver the bank loads and checks every file, and it plays nothing. The boot line
/// names the driver, so the smoke session shows the choice.
/// </para>
/// </remarks>
public sealed class SoundBank
{
    /// <summary>The voices of each player.</summary>
    public const int Voices = 4;

    private const string NotDecoded = "The engine could not decode the sound file as a WAV file.";
    private const string BusExists = "The audio bus already exists.";
    private const string FileField = "file";
    private const string BusField = "bus";
    private const string CountField = "count";

    /// <summary>The name of the audio driver that mixes nothing (D-114).</summary>
    public const string DummyDriver = "Dummy";

    private readonly Dictionary<SoundCue, AudioStreamPlayer> flat;
    private readonly Dictionary<SoundCue, AudioStreamPlayer3D> placed;
    private readonly List<AudioStreamWav> streams;

    private SoundBank(Dictionary<SoundCue, AudioStreamPlayer> flat, Dictionary<SoundCue, AudioStreamPlayer3D> placed, List<AudioStreamWav> streams)
    {
        this.flat = flat;
        this.placed = placed;
        this.streams = streams;
    }

    /// <summary>The count of rendered files that the bank holds.</summary>
    public int Files => this.streams.Count;

    /// <summary>Answers whether the audio driver mixes, so the bank plays its sounds. The dummy driver does not.</summary>
    public static bool Mixes => AudioServer.GetDriverName() != DummyDriver;

    /// <summary>Adds the buses, reads every file of the bindings, and puts one player for each file under the parent.</summary>
    /// <exception cref="ContextException">A bus exists already, or a file is absent or does not decode.</exception>
    public static SoundBank Build(string contentDirectory, Node parent)
    {
        AddBuses();
        Dictionary<SoundCue, AudioStreamPlayer> flat = [];
        Dictionary<SoundCue, AudioStreamPlayer3D> placed = [];
        List<AudioStreamWav> streams = [];
        foreach (SoundBinding binding in SoundBindings.All)
        {
            AudioStreamWav stream = Decode(binding.File, AssetFile.Read(contentDirectory, binding.File));
            streams.Add(stream);
            if (binding.AtHunter)
            {
                AudioStreamPlayer3D player = new() { Stream = stream, Bus = binding.Bus, MaxPolyphony = Voices };
                parent.AddChild(player);
                placed[binding.Cue] = player;
            }
            else
            {
                AudioStreamPlayer player = new() { Stream = stream, Bus = binding.Bus, MaxPolyphony = Voices };
                parent.AddChild(player);
                flat[binding.Cue] = player;
            }
        }

        return new SoundBank(flat, placed, streams);
    }

    /// <summary>Stops every player, takes its stream away, and releases every stream, at the end of a session.</summary>
    public void Release()
    {
        foreach (AudioStreamPlayer player in this.flat.Values)
        {
            player.Stop();
            player.Stream = null;
        }

        foreach (AudioStreamPlayer3D player in this.placed.Values)
        {
            player.Stop();
            player.Stream = null;
        }

        foreach (AudioStreamWav stream in this.streams)
        {
            stream.Dispose();
        }

        this.streams.Clear();
    }

    /// <summary>Plays the sounds of one tick. A sound at the Overseer plays at its feet, and waits for a hunter that exists.</summary>
    public void Play(IReadOnlyList<SoundPlay> plays, Hunter? hunter)
    {
        if (!Mixes)
        {
            return;
        }

        foreach (SoundPlay play in plays)
        {
            if (this.flat.TryGetValue(play.Cue, out AudioStreamPlayer? player))
            {
                player.PitchScale = play.Pitch;
                player.Play();
            }
            else if (hunter is not null && this.placed.TryGetValue(play.Cue, out AudioStreamPlayer3D? placedPlayer))
            {
                placedPlayer.Position = RenderInterpolation.ToGodot(hunter.Body.Position);
                placedPlayer.PitchScale = play.Pitch;
                placedPlayer.Play();
            }
        }
    }

    /// <summary>Adds the buses after Master, in the order of <see cref="SoundBindings.Buses"/>, each with its default volume and a send to Master (D-452).</summary>
    private static void AddBuses()
    {
        foreach (BusDefault bus in SoundBindings.Buses)
        {
            int index = AudioServer.GetBusIndex(bus.Name);
            if (bus.Name != SoundBindings.MasterBus)
            {
                if (index >= 0)
                {
                    ContextException error = new(BusExists);
                    error.AddContext(BusField, bus.Name);
                    error.AddContext(CountField, AudioServer.BusCount.ToString(CultureInfo.InvariantCulture));
                    throw error;
                }

                index = AudioServer.BusCount;
                AudioServer.AddBus();
                AudioServer.SetBusName(index, bus.Name);
                AudioServer.SetBusSend(index, SoundBindings.MasterBus);
            }

            AudioServer.SetBusVolumeDb(index, Mathf.LinearToDb(bus.Volume));
        }
    }

    /// <summary>The stream of one WAV file.</summary>
    /// <exception cref="ContextException">The engine cannot decode the bytes.</exception>
    private static AudioStreamWav Decode(string file, byte[] bytes)
    {
        AudioStreamWav? stream = AudioStreamWav.LoadFromBuffer(bytes, new Godot.Collections.Dictionary());
        if (stream is null)
        {
            ContextException error = new(NotDecoded);
            error.AddContext(FileField, file);
            throw error;
        }

        return stream;
    }
}
