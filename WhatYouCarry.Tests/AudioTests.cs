using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Game.Audio;
using WhatYouCarry.Tools.AudioSynth;
using Xunit;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Tests;

/// <summary>
/// The sounds of PR-20: the deterministic math, the spectral analysis and render, the recordings, the sound files, the
/// action events of Core, the cues, the rhythm of the steps, and the hunter pitch (D-450 to D-468; PR-20 exit tests 1 to 4).
/// </summary>
[Collection(ConsoleCollection.Name)]
public sealed class AudioTests
{
    private const string FootstepSound = AssetPaths.SoundDirectory + "footstep.json";
    private const string BadFile = AssetPaths.SoundDirectory + "bad.json";

    /// <summary>The Core events of the fixture list of exit test 3: every action kind, a timer mark at each mark, the expiry, and the hunter spawn (D-454, D-456).</summary>
    private static readonly ActionEvent[] FixtureActions =
    [
        new ActionEvent(ActionEventKind.SwingStart, 0, 0),
        new ActionEvent(ActionEventKind.SwingHit, 0, 1),
        new ActionEvent(ActionEventKind.Dodge, 0, 0),
        new ActionEvent(ActionEventKind.PlayerHit, 0, 10),
        .. FloorTimer.MarkSeconds.Select(seconds => new ActionEvent(ActionEventKind.TimerMark, 0, seconds)),
    ];

    private static readonly TimerEvent[] FixtureTimerEvents =
    [
        new TimerEvent(TimerEventKind.HunterSpawn, 1, 0, 0, 0),
    ];

    /// <summary>PR-20 exit test 1. One sound file rendered twice gives equal bytes.</summary>
    [Fact]
    public void SynthIsDeterministic()
    {
        byte[] definition = File.ReadAllBytes(Path.Combine(ContentRoot(), FootstepSound));

        byte[] first = AudioSynthCommand.Render(ContentRoot(), FootstepSound, definition);
        byte[] second = AudioSynthCommand.Render(ContentRoot(), FootstepSound, definition);

        Assert.True(first.Length > WavWriter.HeaderLength, "The render holds no sample.");
        Assert.Equal(first, second);
    }

    /// <summary>PR-20 exit test 2. A sound file with an extra field fails, and the error names the field and the file (D-92, D-168).</summary>
    [Fact]
    public void SynthRejectsUnknownField()
    {
        JsonObject file = MinimalSpectral();
        file["reverb"] = 0.5;

        ContextException error = Assert.Throws<ContextException>(() => SoundDefinition.Parse(BadFile, Bytes(file)));

        Assert.Contains(BadFile, error.Message, StringComparison.Ordinal);
        Assert.Contains("reverb", error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// PR-20 exit test 3. Each Core event of the fixture list maps to a cue, and the file of each cue is a rendered sound
    /// of the repository (D-453, D-454).
    /// </summary>
    [Fact]
    public void EveryEventHasSound()
    {
        HashSet<string> rendered = AudioSynthCommand.RenderAll(ContentRoot()).Select(sound => sound.SoundPath).ToHashSet(StringComparer.Ordinal);
        List<SoundCue> cues = [.. FixtureActions.Select(SoundCues.Of)];
        foreach (TimerEvent timerEvent in FixtureTimerEvents)
        {
            Assert.True(SoundCues.TryOf(timerEvent, out SoundCue cue), $"The timer event {timerEvent.Kind} has no cue.");
            cues.Add(cue);
        }

        // The Overseer spawns on the tick of expiry, and its sound carries that moment, so the expiry has no cue (D-469).
        Assert.False(SoundCues.TryOf(new TimerEvent(TimerEventKind.Expiry, 1, 0, 0, 0), out _));

        foreach (SoundCue cue in cues)
        {
            string file = SoundBindings.Of(cue).File;
            Assert.True(rendered.Contains(file), $"The cue {cue} names '{file}', and no sound file renders it.");
            Assert.True(File.Exists(Path.Combine(ContentRoot(), file)), $"The cue {cue} names '{file}', and the repository holds no such file.");
        }

        // A new action kind must join the fixture list, so no kind stays silent.
        Assert.Equal(Enum.GetValues<ActionEventKind>().Order(), FixtureActions.Select(action => action.Kind).Distinct().Order());
    }

    /// <summary>
    /// PR-20 exit test 4. The pitch of the hunter step rises with the speed of the Overseer, from 1 at expiry to 2 at the
    /// sprint speed, and it stays at 2 after that (D-408, D-455).
    /// </summary>
    [Fact]
    public void HunterPitchFollowsSpeed()
    {
        HunterDefinition overseer = TestWorld.Content.Hunter;
        float start = overseer.SpeedMetresPerSecond(0);
        long[] seconds = [0, 10, 20, 40, 60];
        float[] ratios = seconds.Select(second => HunterPitch.Ratio(overseer.SpeedMetresPerSecond(second * SimulationLoop.TicksPerSecond), start)).ToArray();

        Assert.Equal(1.0f, ratios[0]);
        for (int index = 1; index < ratios.Length; index++)
        {
            Assert.True(ratios[index] > ratios[index - 1], $"The pitch at {seconds[index]} seconds after expiry is {ratios[index]}, and at {seconds[index - 1]} seconds it is {ratios[index - 1]}.");
        }

        Assert.Equal(HunterPitch.MaxRatio, HunterPitch.Ratio(overseer.SpeedMetresPerSecond(70L * SimulationLoop.TicksPerSecond), start), 4);
        Assert.Equal(HunterPitch.MaxRatio, HunterPitch.Ratio(overseer.SpeedMetresPerSecond(200L * SimulationLoop.TicksPerSecond), start));
        Assert.Throws<ContextException>(() => HunterPitch.Ratio(1.0f, 0.0f));
    }

    /// <summary>Each committed WAV file equals the synthesizer output, so a change of a sound file or a recording without a new render fails here (D-453).</summary>
    [Fact]
    public void CommittedSoundsMatchTheSynthesizer()
    {
        IReadOnlyList<RenderedSound> sounds = AudioSynthCommand.RenderAll(ContentRoot());

        Assert.NotEmpty(sounds);
        foreach (RenderedSound sound in sounds)
        {
            byte[] committed = File.ReadAllBytes(Path.Combine(ContentRoot(), sound.SoundPath));
            Assert.True(committed.SequenceEqual(sound.Bytes), $"The committed '{sound.SoundPath}' differs from the synthesizer output. Run 'make sounds', and commit the WAV files (D-453).");
        }
    }

    /// <summary>
    /// PR-20 exit test 5. Each sword and hunter sound is the file that the owner approved (D-470). A change to one of
    /// these files needs a new approval and a new decision.
    /// </summary>
    [Theory]
    [InlineData("sword-swing.wav", "8fe02d941a5eb99f258349763878878c4ce1947a78fb33c146432da0db8a4c10")]
    [InlineData("sword-hit.wav", "61254364b5b6844d554e129ec6fb78a80bbf22b3653fa27d2888ffba72d4ee96")]
    [InlineData("hunter-spawn.wav", "3f1510257264bb3251615460c9cefb8774b61b877a6d4cac3be19a5842a885fb")]
    [InlineData("hunter-step.wav", "30cd989bbfcc98953a39b731d512b20c47dff6d24a50ae919883e0893e1f97b0")]
    public void ApprovedSoundsAreTheOwnerChoice(string file, string sha256)
    {
        byte[] bytes = File.ReadAllBytes(Path.Combine(ContentRoot(), AssetPaths.SoundDirectory, file));

        Assert.Equal(sha256, Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(bytes)));
    }

    /// <summary>Every sound file has a binding, every binding names a rendered sound, and the sound directory holds no other WAV file.</summary>
    [Fact]
    public void EverySoundFileIsBound()
    {
        string directory = Path.Combine(ContentRoot(), AssetPaths.SoundDirectory);
        string[] waves = Directory.GetFiles(directory, "*" + AssetPaths.SoundExtension).Select(file => AssetPaths.SoundDirectory + Path.GetFileName(file)).Order(StringComparer.Ordinal).ToArray();
        string[] rendered = AudioSynthCommand.RenderAll(ContentRoot()).Select(sound => sound.SoundPath).Order(StringComparer.Ordinal).ToArray();
        string[] bound = SoundBindings.All.Select(binding => binding.File).Order(StringComparer.Ordinal).ToArray();

        Assert.Equal(rendered, waves);
        Assert.Equal(rendered, bound);
        Assert.Equal(Enum.GetValues<SoundCue>(), SoundBindings.All.Select(binding => binding.Cue).ToArray());
    }

    /// <summary>The step of the Overseer plays at its place, and every other sound plays without a place (D-409).</summary>
    [Fact]
    public void TheHunterStepPlaysAtTheHunter()
    {
        Assert.True(SoundBindings.Of(SoundCue.HunterStep).AtHunter);
        Assert.Single(SoundBindings.All, binding => binding.AtHunter);
    }

    /// <summary>The buses and their default volumes are the owner answer of D-452, and every binding names one of them.</summary>
    [Fact]
    public void BusesHoldTheOwnerVolumes()
    {
        Assert.Equal(
            new[] { ("Master", 1.0f), ("Effects", 1.0f), ("Music", 0.7f), ("UI", 0.8f) },
            SoundBindings.Buses.Select(bus => (bus.Name, bus.Volume)).ToArray());
        string[] names = SoundBindings.Buses.Select(bus => bus.Name).ToArray();
        Assert.All(SoundBindings.All, binding => Assert.Contains(binding.Bus, names));
    }

    /// <summary>Each recording lies under the recording directory, and the source file names it with its CC0 license (D-461, D-467).</summary>
    [Fact]
    public void EveryRecordingHasItsSource()
    {
        JsonNode sources = JsonNode.Parse(File.ReadAllText(Path.Combine(ContentRoot(), AssetPaths.RecordingDirectory, "sources.json")))
            ?? throw new InvalidOperationException("The source file of the recordings holds no JSON value.");
        Dictionary<string, JsonNode> byFile = [];
        foreach (JsonNode? record in sources["recordings"]?.AsArray() ?? [])
        {
            Assert.NotNull(record);
            byFile.Add(record["file"]!.GetValue<string>(), record);
            Assert.Contains("publicdomain/zero", record["license"]!.GetValue<string>(), StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(record["author"]!.GetValue<string>()), "A record of a recording names no author.");
        }

        string[] files = Directory.GetFiles(Path.Combine(ContentRoot(), AssetPaths.RecordingDirectory), "*" + AssetPaths.SoundExtension).Select(file => Path.GetFileName(file)).ToArray();
        Assert.Equal(files.Order(StringComparer.Ordinal), byFile.Keys.Order(StringComparer.Ordinal));
        foreach (RenderedSound sound in AudioSynthCommand.RenderAll(ContentRoot()))
        {
            SoundDefinition definition = SoundDefinition.Parse(sound.ParameterPath, File.ReadAllBytes(Path.Combine(ContentRoot(), sound.ParameterPath)));
            foreach (Layer layer in definition.Layers.Where(layer => layer.Kind == LayerKind.Recording))
            {
                Assert.True(byFile.ContainsKey(Path.GetFileName(layer.File)), $"The sound '{sound.ParameterPath}' plays '{layer.File}', and the source file does not name it.");
            }
        }
    }

    /// <summary>An absent field, a value outside its range, a bad name, a bad frame size, and a bad level are each an error that names the field (D-92, D-462).</summary>
    [Theory]
    [InlineData("kind", "\"organ\"")]
    [InlineData("trimMs", "0")]
    [InlineData("timeStretch", "8")]
    [InlineData("pitchSemitones", "-40")]
    [InlineData("frameSamples", "777")]
    [InlineData("levels", "[[1,2,3]]")]
    [InlineData("gain", null)]
    public void BadLayersFail(string field, string? value)
    {
        JsonObject file = MinimalSpectral();
        JsonObject layer = file["layers"]!.AsArray()[0]!.AsObject();
        if (value is null)
        {
            layer.Remove(field);
        }
        else
        {
            layer[field] = JsonNode.Parse(value);
        }

        ContextException error = Assert.Throws<ContextException>(() => SoundDefinition.Parse(BadFile, Bytes(file)));

        Assert.Contains(BadFile, error.Message, StringComparison.Ordinal);
        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A bad seed and a gain outside its range are each an error that names the field (D-92).</summary>
    [Theory]
    [InlineData("seed", "0")]
    [InlineData("gain", "1.5")]
    [InlineData("gain", null)]
    public void BadRootFieldsFail(string field, string? value)
    {
        JsonObject file = MinimalSpectral();
        if (value is null)
        {
            file.Remove(field);
        }
        else
        {
            file[field] = JsonNode.Parse(value);
        }

        ContextException error = Assert.Throws<ContextException>(() => SoundDefinition.Parse(BadFile, Bytes(file)));

        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A recording layer names a WAV file under the recording directory, and no other path (T-2, D-467).</summary>
    [Theory]
    [InlineData("audio/recordings/../../secret.wav")]
    [InlineData("audio/sfx/footstep.wav")]
    [InlineData("audio/recordings/footstep.ogg")]
    public void ARecordingOutsideItsDirectoryFails(string file)
    {
        JsonObject definition = new()
        {
            ["seed"] = 1,
            ["gain"] = 0.5,
            ["layers"] = new JsonArray(new JsonObject
            {
                ["kind"] = "recording",
                ["delayMs"] = 0.0,
                ["gain"] = 1.0,
                ["trimMs"] = 100.0,
                ["fadeMs"] = 5.0,
                ["file"] = file,
            }),
        };

        ContextException error = Assert.Throws<ContextException>(() => SoundDefinition.Parse(BadFile, Bytes(definition)));

        Assert.Contains("'file'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The analysis of a recording gives a file of one spectral layer whose render holds the shape of the recording (D-464).</summary>
    [Fact]
    public void TheRenderFollowsTheAnalysis()
    {
        float[] reference = Burst(4410, 0.8f);
        int[][] levels = SpectralLayer.Analyze(reference, SpectralLayer.DefaultFrameSize);

        AnalysisResult result = AudioAnalyzeCommand.Analyze(BadFile, null, reference, 1, null);

        Assert.Equal(levels.Length, result.Frames);
        Assert.True(result.ErrorDecibels < 8.0, $"The resynthesis error is {result.ErrorDecibels} decibels.");
        SoundDefinition definition = SoundDefinition.Parse(BadFile, result.Definition);
        Assert.Single(definition.Layers);
        Assert.Equal(LayerKind.Spectral, definition.Layers[0].Kind);
        Assert.Equal(SpectralLayer.DefaultFrameSize, definition.Layers[0].FrameSamples);
    }

    /// <summary>A loud recording does not reach the ceiling of a level, so the analysis keeps the shape of its loudest bands (D-464).</summary>
    [Fact]
    public void ALoudRecordingDoesNotReachTheCeiling()
    {
        int[][] levels = SpectralLayer.Analyze(Burst(4410, 1.0f), SpectralLayer.DefaultFrameSize);

        int loudest = levels.Max(frame => frame.Max());
        Assert.InRange(loudest, SpectralLayer.FloorDecibels + 1, SpectralLayer.CeilingDecibels - 1);
    }

    /// <summary>Each frame size of a spectral layer renders, and a size outside the list is an error (D-464).</summary>
    [Theory]
    [InlineData(256)]
    [InlineData(512)]
    [InlineData(1024)]
    public void EveryFrameSizeRenders(int frameSize)
    {
        float[] reference = Burst(4410, 0.5f);
        AnalysisResult result = AudioAnalyzeCommand.Analyze(BadFile, null, reference, 1, frameSize);
        SoundDefinition definition = SoundDefinition.Parse(BadFile, result.Definition);

        float[] samples = LayeredSynthesizer.Render(definition, new Dictionary<string, float[]>());

        Assert.Equal(frameSize, definition.Layers[0].FrameSamples);
        Assert.NotEmpty(samples);
        Assert.All(samples, sample => Assert.InRange(sample, -1.0f, 1.0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpectralLayer.Analyze(reference, 333));
    }

    /// <summary>The exponential and the logarithm of the tool agree with the runtime, and each rejects a value outside its range (D-453).</summary>
    [Fact]
    public void DeterministicMathMatchesTheRuntime()
    {
        foreach (double value in new[] { 0.001, 0.5, 1.0, 2.0, 10.0, 1234.5, 1e-9, 1e9 })
        {
            Assert.Equal(Math.Log(value), DetDouble.Log(value), 12);
        }

        foreach (double value in new[] { -20.0, -6.9, -1.0, 0.0, 0.5, 3.0, 20.0, 300.0 })
        {
            Assert.Equal(Math.Exp(value), DetDouble.Exp(value), Math.Exp(value) * 1e-12);
        }

        Assert.Equal(0.0, DetDouble.Exp(-800.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => DetDouble.Exp(800.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => DetDouble.Log(0.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => DetDouble.Log(double.NaN));
    }

    /// <summary>The WAV file is 16-bit mono PCM at 44.1 kHz, and the reader reads what the writer wrote (D-462).</summary>
    [Fact]
    public void WavFilesRoundTrip()
    {
        byte[] file = WavWriter.Write([1.0f, -1.0f, 0.5f]);

        Assert.Equal(WavWriter.HeaderLength + 6, file.Length);
        Assert.Equal("RIFF", Encoding.ASCII.GetString(file, 0, 4));
        Assert.Equal("WAVE", Encoding.ASCII.GetString(file, 8, 4));
        Assert.Equal(1, BitConverter.ToInt16(file, 20));
        Assert.Equal(1, BitConverter.ToInt16(file, 22));
        Assert.Equal(LayeredSynthesizer.SampleRate, BitConverter.ToInt32(file, 24));
        Assert.Equal(16, BitConverter.ToInt16(file, 34));
        Assert.Equal(32767, BitConverter.ToInt16(file, 44));

        float[] read = WavReader.Read("round-trip.wav", file);
        Assert.Equal(3, read.Length);
        Assert.Equal(1.0f, read[0], 3);
        Assert.Equal(-1.0f, read[1], 3);
        Assert.Equal(0.5f, read[2], 3);
        Assert.Throws<ArgumentException>(() => WavWriter.Write([]));
        Assert.Throws<ArgumentException>(() => WavWriter.Write([float.NaN]));
        Assert.Throws<ContextException>(() => WavReader.Read("bad.wav", [1, 2, 3, 4]));
    }

    /// <summary>A press of attack gives one swing start and a press of dodge gives one dodge, on the tick of the press alone (D-323, D-454).</summary>
    [Fact]
    public void PressesGiveActionEvents()
    {
        SimulationLoop loop = new(1, TestWorld.PeacefulContent);

        loop.Step(new Intent(loop.Tick, 0, 0, 0, 0, Button.Attack));
        Assert.Equal(new[] { new ActionEvent(ActionEventKind.SwingStart, 0, 0) }, loop.LastActions);
        loop.Step(new Intent(loop.Tick, 0, 0, 0, 0, Button.Attack));
        Assert.Empty(loop.LastActions);

        loop.Step(new Intent(loop.Tick, 0, 0, 0, 0, Button.Dodge));
        Assert.Equal(new[] { new ActionEvent(ActionEventKind.Dodge, 2, 0) }, loop.LastActions);
        loop.Step(new Intent(loop.Tick, 0, 0, 0, 0, Button.Dodge));
        Assert.Empty(loop.LastActions);
    }

    /// <summary>
    /// Over the runs of the full clearer, the swing hit events of each tick name the hits of the player blade, and the
    /// player hit events of each tick sum to the health that the player lost (D-454). Each failure names its seed.
    /// </summary>
    [Fact]
    public void HitEventsFollowTheHits()
    {
        int swingHits = 0;
        int playerHits = 0;
        for (ulong seed = 1; seed <= 20; seed++)
        {
            FullClearer policy = new(TestWorld.Content);
            SimulationLoop loop = new(seed, TestWorld.Content);
            for (uint tick = 0; tick < 3000 && !loop.Ended; tick++)
            {
                int health = loop.Player.Health;
                loop.Step(policy.Next(loop));
                long[] blade = loop.Player.LastHits.Select(hit => (long)hit.Owner).ToArray();
                long[] swingEvents = loop.LastActions.Where(action => action.Kind == ActionEventKind.SwingHit).Select(action => action.Value).ToArray();
                long taken = loop.LastActions.Where(action => action.Kind == ActionEventKind.PlayerHit).Sum(action => action.Value);
                Assert.True(blade.SequenceEqual(swingEvents), $"Seed {seed}, tick {loop.Tick}: the swing hit events differ from the hits of the blade.");
                Assert.True(Math.Min(taken, health) == health - loop.Player.Health, $"Seed {seed}, tick {loop.Tick}: the player hit events sum to {taken}, and the player lost {health - loop.Player.Health} health.");
                swingHits += swingEvents.Length;
                playerHits += loop.LastActions.Count(action => action.Kind == ActionEventKind.PlayerHit);
            }
        }

        Assert.True(swingHits > 0, "No run of the full clearer hit an enemy, so the test read no swing hit.");
        Assert.True(playerHits > 0, "No run of the full clearer took a hit, so the test read no player hit.");
    }

    /// <summary>A hit during a roll does not land, so it gives no player hit event (D-328, D-454).</summary>
    [Fact]
    public void ARollTakesNoHit()
    {
        Player player = new(TestWorld.FlatFloor(16, 6), new CoreVector3(8.5f, TestWorld.FloorTop, 8.5f), SimulationLoop.MainWeapon(TestWorld.Content), Player.MaxHealth);
        player.Step(new Intent(0, 0, 0, 0, 0, Button.Dodge), 0, 0, []);
        Assert.True(player.StartedRoll);

        Assert.False(player.TakeHit(10));
        Assert.Equal(Player.MaxHealth, player.Health);

        for (uint tick = 1; tick <= Player.RollTicks; tick++)
        {
            player.Step(new Intent(tick, 0, 0, 0, 0, 0), 0, 0, []);
        }

        Assert.False(player.StartedRoll);
        Assert.True(player.TakeHit(10));
        Assert.Equal(Player.MaxHealth - 10, player.Health);
    }

    /// <summary>An idle run gives one timer mark at each mark, in order, on the tick whose step takes the countdown onto it (D-456).</summary>
    [Fact]
    public void TimerMarksComeInOrder()
    {
        SimulationLoop loop = new(1, WithTimer(TestWorld.PeacefulContent, 61));
        List<long> marks = [];
        while (!loop.Timer.Expired)
        {
            loop.Step(new Intent(loop.Tick, 0, 0, 0, 0, 0));
            foreach (ActionEvent action in loop.LastActions)
            {
                Assert.Equal(ActionEventKind.TimerMark, action.Kind);
                Assert.Equal(action.Value * SimulationLoop.TicksPerSecond, loop.Timer.Remaining);
                marks.Add(action.Value);
            }
        }

        Assert.Equal(FloorTimer.MarkSeconds.Select(seconds => (long)seconds), marks);
        Assert.Equal(60, FloorTimer.MarkAt(3600));
        Assert.Equal(0, FloorTimer.MarkAt(3599));
        Assert.Equal(0, FloorTimer.MarkAt(0));
    }

    /// <summary>
    /// A countdown that stands on the 60-second mark while the player waits at the stairwell gives no mark, because a
    /// mark sounds only when the running countdown reaches it (D-140, D-456).
    /// </summary>
    [Fact]
    public void APausedTimerGivesNoMark()
    {
        const long LongTimer = 600;
        SimulationLoop probe = new(3, WithTimer(TestWorld.PeacefulContent, LongTimer));
        long walk = (LongTimer * SimulationLoop.TicksPerSecond) - WalkToStairwell(probe, 0);
        long idle = (SimulationLoop.TicksPerSecond - (walk % SimulationLoop.TicksPerSecond)) % SimulationLoop.TicksPerSecond;
        long seconds = ((walk + idle) / SimulationLoop.TicksPerSecond) + 60;

        SimulationLoop loop = new(3, WithTimer(TestWorld.PeacefulContent, seconds));
        long remaining = WalkToStairwell(loop, idle);
        Assert.Equal(60L * SimulationLoop.TicksPerSecond, remaining);
        for (int tick = 0; tick < 300; tick++)
        {
            loop.Step(new Intent(loop.Tick, 0, 0, 0, 0, 0));
            Assert.True(StairwellTransition.IsAtStairwell(loop.Body, loop.Plan.Stairwell), $"The body left the stairwell on tick {loop.Tick}.");
            Assert.Empty(loop.LastActions);
        }
    }

    /// <summary>The cues of the marks: the alarm at 60 and 30 seconds, the tick at the rest, and an error for seconds that are not a mark (D-456). A wave has no cue.</summary>
    [Fact]
    public void MarksChooseTheAlarmOrTheTick()
    {
        SoundCue[] cues = FloorTimer.MarkSeconds.Select(seconds => SoundCues.Of(new ActionEvent(ActionEventKind.TimerMark, 0, seconds))).ToArray();

        Assert.Equal(new[] { SoundCue.TimerAlarm, SoundCue.TimerAlarm }.Concat(Enumerable.Repeat(SoundCue.TimerTick, 8)), cues);
        Assert.Throws<ContextException>(() => SoundCues.Of(new ActionEvent(ActionEventKind.TimerMark, 0, 45)));
        Assert.False(SoundCues.TryOf(new TimerEvent(TimerEventKind.Expiry, 1, 0, 0, 0), out _));
        Assert.False(SoundCues.TryOf(new TimerEvent(TimerEventKind.Wave, 1, 0, 1, 2), out _));
        Assert.False(SoundCues.TryOf(new TimerEvent(TimerEventKind.WaveSkip, 1, 0, 1, 1), out _));
    }

    /// <summary>The stride of the player follows its speed: 2.5 steps each second at the walk, 3 at the sprint, and the ends hold outside (D-463).</summary>
    [Fact]
    public void FootstepCadenceFollowsSpeed()
    {
        Assert.Equal(PlayerBody.WalkSpeed / FootstepCadence.WalkStepsPerSecond, FootstepCadence.StrideMeters(PlayerBody.WalkSpeed), 4);
        Assert.Equal(PlayerBody.SprintSpeed / FootstepCadence.SprintStepsPerSecond, FootstepCadence.StrideMeters(PlayerBody.SprintSpeed), 4);
        Assert.Equal(FootstepCadence.WalkStrideMeters, FootstepCadence.StrideMeters(1.0f));
        Assert.Equal(FootstepCadence.SprintStrideMeters, FootstepCadence.StrideMeters(12.0f));

        float middle = (PlayerBody.WalkSpeed + PlayerBody.SprintSpeed) / 2.0f;
        float rate = middle / FootstepCadence.StrideMeters(middle);
        Assert.InRange(rate, FootstepCadence.WalkStepsPerSecond, FootstepCadence.SprintStepsPerSecond);
    }

    /// <summary>A walk makes one footstep for each stride of its speed, and a body at rest makes none (D-454, D-463).</summary>
    [Fact]
    public void FootstepsFollowTheStride()
    {
        SimulationLoop loop = new(1, TestWorld.PeacefulContent);
        SoundDirector director = new(TestWorld.Content.Hunter.SpeedMetresPerSecond(0));
        int footsteps = 0;
        for (int tick = 0; tick < 60; tick++)
        {
            loop.Step(new Intent(loop.Tick, 0, 0, 0, 0, 0));
            footsteps += director.AfterTick(loop).Count(play => play.Cue == SoundCue.Footstep);
        }

        Assert.Equal(0, footsteps);

        float walked = 0.0f;
        CoreVector3 last = loop.Body.Position;
        for (int tick = 0; tick < 120; tick++)
        {
            loop.Step(new Intent(loop.Tick, 0, 0, 0, 127, 0));
            footsteps += director.AfterTick(loop).Count(play => play.Cue == SoundCue.Footstep);
            CoreVector3 feet = loop.Body.Position;
            if (loop.Body.IsOnGround())
            {
                walked += MathF.Sqrt(((feet.X - last.X) * (feet.X - last.X)) + ((feet.Z - last.Z) * (feet.Z - last.Z)));
            }

            last = feet;
        }

        Assert.True(walked > 2.0f, $"The body walked {walked} meters, and the test needs a walk of several strides.");

        // The clock and this sum add the same moves, and the float remainder can move the count at a stride end by one.
        int expected = (int)(walked / FootstepCadence.WalkStrideMeters);
        Assert.InRange(footsteps, expected - 1, expected + 1);
    }

    /// <summary>The stride clock makes no step on its first move, in the air, or after a reset, and a step on each stride on the ground.</summary>
    [Fact]
    public void StrideClockCountsGroundDistance()
    {
        StrideClock clock = new();

        Assert.False(clock.Advance(new CoreVector3(0.0f, 0.0f, 0.0f), true, 1.0f));
        Assert.False(clock.Advance(new CoreVector3(0.6f, 0.0f, 0.0f), true, 1.0f));
        Assert.True(clock.Advance(new CoreVector3(1.2f, 5.0f, 0.0f), true, 1.0f));
        Assert.False(clock.Advance(new CoreVector3(3.0f, 0.0f, 0.0f), false, 1.0f));
        clock.Reset();
        Assert.False(clock.Advance(new CoreVector3(50.0f, 0.0f, 0.0f), true, 1.0f));
        Assert.Throws<ContextException>(() => clock.Advance(new CoreVector3(51.0f, 0.0f, 0.0f), true, 0.0f));
    }

    /// <summary>
    /// A short burst of noise under a fall, as a stand-in for a recording. The spectral layer holds the shape of noise, and
    /// a pure tone is the one thing that its random phases do not hold (D-464, D-467).
    /// </summary>
    private static float[] Burst(int samples, float peak)
    {
        float[] burst = new float[samples];
        uint state = 12345;
        for (int index = 0; index < samples; index++)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            float fall = 1.0f - (index / (float)samples);
            burst[index] = peak * fall * (float)((state / (double)uint.MaxValue * 2.0) - 1.0);
        }

        return burst;
    }

    /// <summary>Walks a loop with the greedy descender until the body stands at the stairwell, after some idle ticks, and gives the countdown that remains.</summary>
    private static long WalkToStairwell(SimulationLoop loop, long idle)
    {
        for (long tick = 0; tick < idle; tick++)
        {
            loop.Step(new Intent(loop.Tick, 0, 0, 0, 0, 0));
        }

        GreedyDescender policy = new(TestWorld.PeacefulContent);
        while (!StairwellTransition.IsAtStairwell(loop.Body, loop.Plan.Stairwell))
        {
            Assert.True(loop.Floor == SimulationLoop.FirstFloor && !loop.Timer.Expired, $"The descender left floor 1 or met expiry at tick {loop.Tick}.");
            loop.Step(policy.Next(loop));
        }

        return loop.Timer.Remaining;
    }

    /// <summary>A content set whose every floor runs a timer of some seconds, with no extra time on a boss floor.</summary>
    private static ContentSet WithTimer(ContentSet content, long seconds)
    {
        List<FloorTemplate> floors = [];
        foreach (FloorTemplate floor in content.Floors)
        {
            floors.Add(floor with { TimerSeconds = seconds, BossTimerSeconds = 0 });
        }

        return content with { Floors = floors };
    }

    /// <summary>The smallest valid sound file: one spectral layer of two frames.</summary>
    private static JsonObject MinimalSpectral()
    {
        JsonArray levels = new(Frame(-20), Frame(-30));
        return new JsonObject
        {
            ["seed"] = 1,
            ["gain"] = 0.5,
            ["layers"] = new JsonArray(new JsonObject
            {
                ["kind"] = "spectral",
                ["delayMs"] = 0.0,
                ["gain"] = 1.0,
                ["trimMs"] = 100.0,
                ["fadeMs"] = 5.0,
                ["timeStretch"] = 1.0,
                ["pitchSemitones"] = 0.0,
                ["tiltDbPerOctave"] = 0.0,
                ["frameSamples"] = SpectralLayer.DefaultFrameSize,
                ["levels"] = levels,
            }),
        };
    }

    /// <summary>One frame of levels, every band at one level.</summary>
    private static JsonArray Frame(int level)
    {
        return new JsonArray([.. Enumerable.Range(0, SpectralLayer.BandCount).Select(band => (JsonNode)level)]);
    }

    private static byte[] Bytes(JsonObject file)
    {
        return Encoding.UTF8.GetBytes(file.ToJsonString());
    }

    private static string ContentRoot()
    {
        return Path.Combine(RepositoryRoot.Find(), AudioSynthCommand.ContentDirectoryName);
    }
}
