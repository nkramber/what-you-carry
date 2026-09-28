using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Game.Animation;
using WhatYouCarry.Game.Audio;
using WhatYouCarry.Game.Content;
using WhatYouCarry.Game.Input;
using WhatYouCarry.Game.Logging;
using WhatYouCarry.Game.Measure;
using WhatYouCarry.Game.Models;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Game.Review;
using WhatYouCarry.Game.Smoke;
using WhatYouCarry.Game.Ui;
using WhatYouCarry.Game.World;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game;

/// <summary>
/// The root node (D-63). It owns one Core simulation, steps it once per physics frame at 60 Hz (D-73), builds
/// one intent per tick from the input (D-77), and draws the world chunks, the player model, and the camera
/// between the last two ticks (D-245, D-291). No engine physics runs here (G-3).
/// </summary>
/// <remarks>
/// <para>
/// The fixed step of the engine is the clock. A physics frame is one tick, and the interpolation fraction of
/// the engine places each render frame between the last two ticks. Core reads no clock and no engine value.
/// The world material takes the fade segment from the camera to the player on every frame (D-292).
/// </para>
/// <para>
/// A boot failure, a step failure, a pose failure, and a failure anywhere else in an engine callback each write an
/// error line and quit with exit code 1 (T-2). The smoke session quits with exit code 0 only when the log holds no
/// error line (D-114). The bot session of M-3 drives
/// the loop with the greedy descender over one floor, or with the bot policy of the policy flag (D-646), on the seed
/// of the seed flag or the first seed (D-704), and the frame log flag writes every frame time to a file at the end of any session (D-295, D-296). The frame log marks
/// each floor transition and each expiry of the floor timer. The content, the models, the clips, and the atlas come from the directory
/// next to the project directory, which is the content directory of the checkout (D-219, D-305).
/// </para>
/// <para>
/// The window opens in the borderless fullscreen of the engine at the resolution of the display, from the
/// project settings (D-310). The Escape key and the Start button of a controller end any session that runs
/// the loop, with an end line and exit code 0, until the escape menu of PR-53 replaces the test exit (D-311).
/// The poll of the tick reads both inputs before the intent, so the exit needs no input event. The press flag
/// gives the engine one scripted press of either input at one tick, so a headless test proves the exit.
/// </para>
/// <para>
/// The player model holds the sword of the main weapon in the right hand (D-330). On each frame the body takes the
/// pose of the player state: the stagger clip, the roll clip, or the walk with the swing clip over it (D-331, D-333).
/// The walk reads the horizontal distance of each tick. The body faces the camera yaw (D-332), and the lowest box
/// corner of the pose stands on the feet.
/// </para>
/// <para>
/// The chunk swap digs the next floor on one task during each floor, offers the plan to the loop before each tick,
/// and uploads its chunks a few at a time (D-72, D-429). A descent swaps the chunks in one frame, and the
/// interpolation starts again at the spawn, so no frame draws the body between two floors.
/// </para>
/// <para>
/// The sound bank plays the sounds of each tick: a sound for each action event, the expiry, and the hunter spawn, and
/// the steps of the player and of the Overseer on their stride rhythm (D-453, D-454, D-455).
/// </para>
/// <para>
/// The HUD shows the health, the timer, the damage numbers, and the stairwell prompt on every frame (D-36, PR-19). The
/// prompt shows while the body stands on the stairwell cell (D-431), and it names the buttons of the device of the
/// last input (D-447). At the open prompt, a tap of interact descends and a hold of one second ascends (D-448). The
/// smoke session also builds the hidden fixture screen of the navigation, so the focus map runs on every platform.
/// </para>
/// <para>
/// The contact sheet flag starts no loop. It renders every block material and the body with the sword at game zoom
/// to one PNG file for the review of the owner, and quits (D-306, D-336). The HUD shot flag builds the scene of play at
/// the spawn, takes no tick, renders one frame of the Deck size with the HUD fixture, and quits (D-133).
/// </para>
/// </remarks>
public partial class Main : Node3D
{
    /// <summary>The seed of a run with no seed flag. The seed flag of the bot session sets another (D-704). The hub of PR-30 picks a seed per run.</summary>
    public const ulong FirstSeed = 1;

    /// <summary>The exit code of a session with no error line.</summary>
    public const int ExitSuccess = 0;

    /// <summary>The exit code of a session with an error line.</summary>
    public const int ExitFailure = 1;

    /// <summary>The subsystem name of every log line of this node.</summary>
    public const string Subsystem = "main";

    /// <summary>The message of the line at the start of the run.</summary>
    public const string StartMessage = "The run starts.";

    /// <summary>The message of the line at the end of the smoke session.</summary>
    public const string EndMessage = "The smoke session ends.";

    /// <summary>The line of a session whose run ended before its own end, by an ascend or by a death (D-322, D-403).</summary>
    public const string RunEndedMessage = "The run ended, and the session ends with it.";

    /// <summary>The field of a session log line that names how the run ended (D-403).</summary>
    public const string EndStateField = "endState";

    /// <summary>The message of the line at the end of the bot session.</summary>
    public const string BotEndMessage = "The bot session ends.";

    /// <summary>The message of the line at the end of a session that the test exit ends (D-311).</summary>
    public const string TestExitMessage = "The test exit ends the session.";

    /// <summary>The message of the line at the end of a session that a close of the window ends (F-161).</summary>
    public const string WindowClosedMessage = "The close of the window ends the session.";

    /// <summary>The message of the error line of a boot failure.</summary>
    public const string BootFailedMessage = "The boot failed, and the game quits.";

    /// <summary>The message of the error line of a tick failure.</summary>
    public const string StepFailedMessage = "A tick failed, and the game quits.";

    /// <summary>The message of the error line of an engine callback that failed outside a narrower guard (T-2).</summary>
    public const string CallbackFailedMessage = "An engine callback failed, and the game quits.";

    /// <summary>The name of the field of the callback line that names the engine callback.</summary>
    public const string CallbackField = "callback";

    /// <summary>The message of the error line of a frame whose pose of the player failed.</summary>
    public const string PoseFailedMessage = "The pose of the player failed, and the game quits.";

    /// <summary>The message of the error line of a bot session whose tick budget passed on the first floor.</summary>
    public const string BotStuckMessage = "The bot session passed its tick budget on the first floor, and the game quits.";

    /// <summary>The message of the error line of a frame log that the game could not write.</summary>
    public const string FrameLogFailedMessage = "The frame log could not be written, and the game quits.";

    /// <summary>The message of the line of the boot that built the HUD (PR-19 exit test 5).</summary>
    public const string HudBuiltMessage = "The HUD is built.";

    /// <summary>The message of the line of the boot that loaded the sound bank (D-453).</summary>
    public const string SoundsLoadedMessage = "The sound bank is loaded.";

    /// <summary>The message of the error line of a tick whose sounds failed.</summary>
    public const string SoundFailedMessage = "The sounds of a tick failed, and the game quits.";

    /// <summary>The name of the field of the sound line that holds the count of rendered files.</summary>
    public const string SoundFilesField = "soundFiles";

    /// <summary>The name of the field of the sound line that names the audio driver. The dummy driver plays nothing.</summary>
    public const string AudioDriverField = "audioDriver";

    /// <summary>The message of the line of the boot that built the fixture screen of the navigation (D-446).</summary>
    public const string FixtureBuiltMessage = "The fixture screen of the navigation is built.";

    /// <summary>The message of the line at the end of the HUD shot.</summary>
    public const string HudShotEndMessage = "The HUD shot is written.";

    /// <summary>The message of the error line of a HUD shot that failed.</summary>
    public const string HudShotFailedMessage = "The HUD shot failed, and the game quits.";

    /// <summary>The message of the error when the HUD shot starts on the headless display.</summary>
    public const string HudShotNeedsWindow = "The HUD shot needs a window, and the headless display renders no image.";

    /// <summary>The name of the field of the fixture line that holds the count of its controls.</summary>
    public const string ControlsField = "controls";

    /// <summary>The message of the line of the tick whose state opens the stairwell prompt (D-431).</summary>
    public const string PromptOpenMessage = "The stairwell prompt opens.";

    /// <summary>The message of the line of the frame whose tick descended and swapped the chunks (D-429).</summary>
    public const string SwapMessage = "The floor transition swaps the world.";

    /// <summary>The message of the error line of a smoke session whose walk passed its budget on floor 1.</summary>
    public const string SmokeStuckMessage = "The smoke session passed its budget to descend on floor 1, and the game quits.";

    /// <summary>The message of the error line of a transition test with a frame over the hitch budget of D-427.</summary>
    public const string HitchOverBudgetMessage = "A frame near a floor transition is over the hitch budget, and the game quits.";

    /// <summary>The message of the error line of a transition test whose run ended before its count of transitions.</summary>
    public const string TransitionsShortMessage = "The run ended before the count of transitions of the session, and the game quits.";

    /// <summary>The name of the field of the swap line that tells whether the loop took the plan of the worker (D-429).</summary>
    public const string FromWorkerField = "fromWorker";

    /// <summary>The name of the field of the swap line that tells whether a dig task runs in the tick of the descent (F-193).</summary>
    public const string DiggingField = "digging";

    /// <summary>The name of the field of the swap line that holds the count of enemy trees that the descent took hidden from the plan of the worker (F-193).</summary>
    public const string StagedTreesField = "stagedTrees";

    /// <summary>The name of the field of the swap line that holds the count of enemy trees that the tick of the descent built (F-193).</summary>
    public const string BuiltTreesField = "builtTrees";

    /// <summary>The message of the line of the tick whose loop took the plan of the worker, with the time of the dig (D-429).</summary>
    public const string DigMessage = "The worker dug the next floor.";

    /// <summary>The message of the error of a swap that offered the plan of the next floor to the loop and holds no plan.</summary>
    private const string NoStagedPlanMessage = "The chunk swap offered the plan of the next floor and holds no plan.";

    /// <summary>The message of the line that closes the trace window of one transition (D-435).</summary>
    public const string TraceMessage = "The trace of a floor transition closes.";

    /// <summary>The name of the field of the dig line that holds the time of the dig, in microseconds.</summary>
    public const string DigMicrosField = "digMicros";

    /// <summary>The name of the field of the dig line that holds the time of the chunk meshes on the task, in microseconds.</summary>
    public const string MeshMicrosField = "meshMicros";

    /// <summary>The name of the field of the end line that holds the count of transitions of the frame log.</summary>
    public const string TransitionsField = "transitions";

    /// <summary>The name of the field of the end line that holds the slowest frame near a transition, in microseconds.</summary>
    public const string TransitionMicrosMaxField = "transitionMicrosMax";

    /// <summary>The message of the line of the tick whose floor timer expires, the tick of the spawn of the Overseer (D-45, D-646).</summary>
    public const string ExpiryMessage = "The floor timer expires.";

    /// <summary>The name of the field of the end line that holds the count of expiries of the frame log (D-646).</summary>
    public const string ExpiriesField = "expiries";

    /// <summary>The name of the field of the end line that holds the slowest frame near an expiry of the floor timer, in microseconds (D-646).</summary>
    public const string ExpiryMicrosMaxField = "expiryMicrosMax";

    /// <summary>The name of the field of the end line of a bot session that names its bot policy (D-646).</summary>
    public const string PolicyField = "policy";

    /// <summary>The message of the line at the end of the contact sheet.</summary>
    public const string ContactSheetEndMessage = "The contact sheet is written.";

    /// <summary>The message of the error line of a contact sheet that failed.</summary>
    public const string ContactSheetFailedMessage = "The contact sheet failed, and the game quits.";

    /// <summary>The message of the error when the contact sheet starts on the headless display.</summary>
    public const string ContactSheetNeedsWindow = "The contact sheet needs a window, and the headless display renders no image.";

    /// <summary>The name of the field of the end line that holds the count of frames of the frame log.</summary>
    public const string FramesField = "frames";

    /// <summary>The name of the field of the end line that holds the 99th percentile frame time, in microseconds.</summary>
    public const string FrameMicrosP99Field = "frameMicrosP99";

    private const string ProjectRoot = "res://";
    private const string ParentDirectory = "..";
    private const string ContentDirectoryName = "content";
    private const string SeedField = "seed";
    private const string TraceTransitionField = "transition";
    private const string TraceSlowestField = "slowestFrameMicros";
    private const string TraceTickField = "maxTickMicros";
    private const string TraceUploadField = "maxUploadMicros";
    private const string TracePauseField = "gcPauseMicros";
    private const string TraceGen0Field = "gen0";
    private const string TraceGen1Field = "gen1";
    private const string TraceGen2Field = "gen2";
    private const string TraceDiggingField = "diggingFrames";
    private const string FloorField = "floor";
    private const string TickField = "tick";
    private const string SubsystemField = "subsystem";
    private const string EntitiesField = "entities";
    private const string FileField = "file";
    private const string ShotField = "shot";
    private const string HeadlessDisplay = "headless";
    private const string NoShotImage = "The viewport of the contact sheet gave no image for a shot.";
    private const string NoHudImage = "The viewport of the HUD shot gave no image.";

    private static readonly long[] NoEntities = [];

    private readonly PrintLogSink sink = new();
    private readonly JsonlLogger logger;
    private readonly IInputPoll poll = new EnginePoll();
    private readonly IntentBuilder builder = new();
    private readonly InputReader reader;

    private SimulationLoop? loop;
    private ulong seed = FirstSeed;
    private ModelNodeTree? playerNodes;
    private BlockbenchModel? playerModel;
    private PlayerClips? clips;
    private Camera3D? camera;
    private ShaderMaterial? worldMaterial;
    private IBotPolicy? bot;
    private GreedyDescender? smokeWalker;
    private ChunkSwap? chunks;
    private readonly StairwellHold hold = new();
    private Hud? hud;
    private SoundBank? sounds;
    private SoundDirector? director;
    private Camera3D? hudCamera;
    private HudState? shotState;
    private bool promptOpen;
    private int transitions = 1;
    private bool transitionTest;
    private uint floorStartTick;
    private TransitionTrace? trace;
    private long tickMicros;
    private long lastPauseMicros;
    private int lastGen0;
    private int lastGen1;
    private int lastGen2;
    private EnemyNodes? enemyNodes;
    private int drawnFloor;
    private ScriptedPress? press;
    private FrameLog? frames;
    private readonly RealFrameClock frameClock = new();
    private string frameLogPath = string.Empty;
    private bool smoke;
    private bool ended;
    private CoreVector3 previousFeet;
    private CoreVector3 currentFeet;
    private CameraPose previousPose;
    private CameraPose currentPose;
    private float walked;
    private float walkAmount;

    /// <summary>A root node with its logger. The boot runs when the node enters the tree.</summary>
    public Main()
    {
        this.logger = new JsonlLogger(this.sink);
        this.reader = new InputReader(this.poll);
    }

    /// <inheritdoc/>
    public override void _Ready()
    {
        try
        {
            // The engine then sends the close request of the window to the handler and waits for its quit, so a close
            // writes the end line and the frame log, and releases the sounds (F-161). The handler is a signal of the
            // root window, and not an override of _Notification, which takes every notification of the engine into
            // managed code, the notifications of the engine shutdown too.
            this.GetTree().AutoAcceptQuit = false;
            this.GetTree().Root.CloseRequested += this.OnCloseRequested;
            this.Boot();
        }
        catch (Exception error)
        {
            // The boot has no loop yet, so the line carries the seed of the session, the first floor, and tick zero. The
            // seed is the first seed until the boot reads the seed flag. The boot measured no frame, so the session
            // writes no frame log, and never an empty one (F-122).
            this.LogFailure(BootFailedMessage, RunFields(this.seed, SimulationLoop.FirstFloor, 0), error);
            this.frames = null;
            this.Quit(ExitFailure);
        }
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        try
        {
            this.PhysicsFrame();
        }
        catch (Exception error)
        {
            this.FailCallback(nameof(_PhysicsProcess), error);
        }
    }

    /// <summary>The body of one physics frame: one tick of the session, and its time.</summary>
    private void PhysicsFrame()
    {
        if (this.loop is null || this.ended || this.shotState is not null)
        {
            return;
        }

        long started = Stopwatch.GetTimestamp();
        this.Tick();
        this.tickMicros += (long)Stopwatch.GetElapsedTime(started).TotalMicroseconds;
    }

    /// <summary>Runs one tick of the session: the test exit, the intent, the step, the swap, the prompt, and the end checks.</summary>
    private void Tick()
    {
        if (this.loop is null)
        {
            return;
        }

        if (this.press is ScriptedPress press && this.loop.Tick == press.Tick)
        {
            // The engine holds the input down from the next frame, and the poll of a later tick reads it.
            Godot.Input.ParseInputEvent(TestExit.EventOf(press));
        }

        if (TestExit.IsPressed(this.poll))
        {
            this.logger.Write(LogContextKind.Run, LogLevel.Info, TestExitMessage, this.EndFields());
            this.Quit(this.sink.ErrorCount == 0 ? ExitSuccess : ExitFailure);
            return;
        }

        if (this.loop.Ended)
        {
            // A run that ended takes no intent (D-322). A death is an outcome of a fight and never a fault of the
            // code, so the session ends clean and its log names the end kind (D-403). A transition test that ends
            // early measured fewer transitions than it asked for, so it fails (D-435).
            if (this.transitionTest)
            {
                this.logger.Write(LogContextKind.Run, LogLevel.Error, TransitionsShortMessage, this.EndFields());
                this.Quit(ExitFailure);
                return;
            }

            this.logger.Write(LogContextKind.Run, LogLevel.Info, RunEndedMessage, this.EndFields());
            this.Quit(this.sink.ErrorCount == 0 ? ExitSuccess : ExitFailure);
            return;
        }

        Intent intent;
        if (this.smokeWalker is not null && this.loop.Floor == SimulationLoop.FirstFloor)
        {
            intent = this.smokeWalker.Next(this.loop);
        }
        else if (this.smoke)
        {
            intent = SmokeSession.ScriptIntent(this.loop.Tick, this.floorStartTick);
        }
        else if (this.bot is not null)
        {
            intent = this.bot.Next(this.loop);
        }
        else
        {
            // The tap and the hold at the open prompt set the interact bit or the ascend bit on one tick (D-448).
            RawInput raw = this.reader.Read();
            raw = raw with { Buttons = this.hold.Apply(raw.Buttons, StairwellPrompt.IsOpen(this.loop)) };
            intent = this.builder.Build(this.loop.Tick, raw);
        }

        try
        {
            if (this.chunks is not null && this.chunks.BeforeTick(this.loop))
            {
                // The plan names the enemies of the next floor, so their trees build hidden during this floor (F-193).
                this.enemyNodes?.Stage(this.chunks.StagedPlan ?? throw new InvalidOperationException(NoStagedPlanMessage));
                if (this.trace is not null)
                {
                    LogFields dug = RunFields(this.loop.Seed, this.loop.Floor, this.loop.Tick);
                    dug.Add(DigMicrosField, this.chunks.LastDigMicros);
                    dug.Add(MeshMicrosField, this.chunks.LastMeshMicros);
                    this.logger.Write(LogContextKind.Run, LogLevel.Info, DigMessage, dug);
                }
            }

            this.loop.Step(intent);
        }
        catch (Exception error)
        {
            this.LogFailure(StepFailedMessage, RunFields(this.loop.Seed, this.loop.Floor, this.loop.Tick), error);
            this.Quit(ExitFailure);
            return;
        }

        this.previousFeet = this.currentFeet;
        this.currentFeet = this.loop.Body.Position;
        this.previousPose = this.currentPose;
        this.currentPose = this.loop.Camera();

        bool swapped = this.chunks is not null && this.chunks.AfterTick(this.loop);
        if (swapped)
        {
            // The body stands at the spawn of the new floor, so no frame draws it between two floors.
            this.previousFeet = this.currentFeet;
            this.previousPose = this.currentPose;
            this.floorStartTick = this.loop.Tick;
            this.frames?.MarkTransition();
            this.trace?.MarkTransition();
        }

        // The Overseer spawns on the tick of the expiry and scans the floor for a cell, so the frame log marks that
        // tick for the Deck measurement (D-646, RR-P3-16). The Game reads the events of the step, and feeds none back.
        if (FrameLog.HoldsExpiry(this.loop.LastEvents))
        {
            this.frames?.MarkExpiry();
            this.logger.Write(LogContextKind.Run, LogLevel.Info, ExpiryMessage, RunFields(this.loop.Seed, this.loop.Floor, this.loop.Tick));
        }

        bool open = StairwellPrompt.IsOpen(this.loop);
        if (open && !this.promptOpen)
        {
            this.logger.Write(LogContextKind.Run, LogLevel.Info, PromptOpenMessage, RunFields(this.loop.Seed, this.loop.Floor, this.loop.Tick));
        }

        this.promptOpen = open;
        this.hud?.AfterTick(this.loop);
        if (this.sounds is not null && this.director is not null)
        {
            try
            {
                this.sounds.Play(this.director.AfterTick(this.loop), this.loop.Hunter);
            }
            catch (Exception error)
            {
                this.LogFailure(SoundFailedMessage, RunFields(this.loop.Seed, this.loop.Floor, this.loop.Tick), error);
                this.Quit(ExitFailure);
                return;
            }
        }

        // A descent digs a new floor with its own enemies, so the trees of the old floor go and the new ones come.
        if (this.enemyNodes is not null)
        {
            if (this.loop.Floor != this.drawnFloor)
            {
                this.enemyNodes.Rebuild(this.loop.Plan, this.loop.Enemies);
                this.drawnFloor = this.loop.Floor;
            }

            this.enemyNodes.AfterTick(this.loop.Enemies, this.loop.Hunter);
        }

        // The swap line comes after the trees of the new floor, so it tells what the tick of the descent did: a dig
        // task that runs, and the trees that it took hidden or built (F-193).
        if (swapped && this.chunks is not null)
        {
            LogFields swap = RunFields(this.loop.Seed, this.loop.Floor, this.loop.Tick);
            swap.Add(FromWorkerField, this.chunks.LastSwapFromWorker);
            swap.Add(DiggingField, this.chunks.IsDigging);
            if (this.enemyNodes is not null)
            {
                swap.Add(StagedTreesField, (long)this.enemyNodes.LastStagedTrees);
                swap.Add(BuiltTreesField, (long)this.enemyNodes.LastBuiltTrees);
            }

            this.logger.Write(LogContextKind.Run, LogLevel.Info, SwapMessage, swap);
        }

        // The walk reads the horizontal distance of the tick, and its amount follows the speed (D-333).
        float stepX = this.currentFeet.X - this.previousFeet.X;
        float stepZ = this.currentFeet.Z - this.previousFeet.Z;
        float stride = MathF.Sqrt((stepX * stepX) + (stepZ * stepZ));
        this.walked += stride;
        this.walkAmount = WalkCycle.Amount(stride / PlayerBody.TickSeconds);

        uint ticksOnFloor = this.loop.Tick - this.floorStartTick;
        if (this.smoke && SmokeSession.IsComplete(this.loop, ticksOnFloor))
        {
            this.logger.Write(LogContextKind.Run, LogLevel.Info, EndMessage, this.EndFields());
            this.Quit(this.sink.ErrorCount == 0 ? ExitSuccess : ExitFailure);
        }
        else if (this.smoke && SmokeSession.IsStuck(this.loop))
        {
            this.logger.Write(LogContextKind.Run, LogLevel.Error, SmokeStuckMessage, this.EndFields());
            this.Quit(ExitFailure);
        }
        else if (this.bot is not null && BotSession.IsComplete(this.loop, this.transitions, ticksOnFloor))
        {
            this.EndBotSession();
        }
        else if (this.bot is not null && BotSession.IsStuck(this.loop, this.transitions, ticksOnFloor))
        {
            this.logger.Write(LogContextKind.Run, LogLevel.Error, BotStuckMessage, this.EndFields());
            this.Quit(ExitFailure);
        }
    }

    /// <inheritdoc/>
    public override void _Process(double delta)
    {
        try
        {
            this.DrawFrame(delta);
        }
        catch (Exception error)
        {
            this.FailCallback(nameof(_Process), error);
        }
    }

    /// <summary>The body of one render frame: the frame time, the uploads, the pose, the camera, and the HUD.</summary>
    private void DrawFrame(double delta)
    {
        if (this.loop is null || this.playerNodes is null || this.playerModel is null || this.clips is null || this.camera is null || this.worldMaterial is null || this.ended)
        {
            return;
        }

        // The engine delta is smoothed and rounded to the vsync interval, so the log reads the real time of the frame
        // from the engine clock (D-635, F-170). The first frame has no frame before it, and it enters no log.
        long? frameMicros = this.frameClock.Next((long)Time.GetTicksUsec());
        if (frameMicros is long micros)
        {
            this.frames?.AddMicros(micros);
        }

        long uploadStarted = Stopwatch.GetTimestamp();
        this.chunks?.UploadSome();
        this.enemyNodes?.BuildSome();
        long uploadMicros = (long)Stopwatch.GetElapsedTime(uploadStarted).TotalMicroseconds;
        if (frameMicros is long traced)
        {
            this.TraceFrame(traced, uploadMicros);
        }

        float fraction = (float)Engine.GetPhysicsInterpolationFraction();
        CoreVector3 feet = RenderInterpolation.Between(this.previousFeet, this.currentFeet, fraction);

        float lowest;
        try
        {
            IReadOnlyDictionary<string, CoreVector3> rotations = BodyPose.Rotations(this.loop.Player, this.clips, this.walked, this.walkAmount);
            ModelNodes.Pose(this.playerNodes, rotations);
            lowest = ModelPose.LowestPoint(AssetPaths.BodyModel, this.playerModel, rotations);
        }
        catch (Exception error)
        {
            this.LogFailure(PoseFailedMessage, RunFields(this.loop.Seed, this.loop.Floor, this.loop.Tick), error);
            this.Quit(ExitFailure);
            return;
        }

        // The lowest box corner of the pose stands on the feet, so a roll or a stride never sinks a box into the floor.
        Vector3 feetPoint = RenderInterpolation.ToGodot(feet);
        this.playerNodes.Root.Position = feetPoint + new Vector3(0.0f, -lowest, 0.0f);

        // The body faces the yaw of the last tick, so a walk sideways is a strafe (D-332). The yaw turns counterclockwise
        // seen from above, as a positive rotation about Y does (D-234).
        this.playerNodes.Root.RotationDegrees = new Vector3(0.0f, this.loop.Yaw / 100.0f, 0.0f);

        this.enemyNodes?.Draw(this.loop.Enemies, this.loop.Hunter, fraction);

        CameraPose pose = RenderInterpolation.Between(this.previousPose, this.currentPose, fraction);
        Vector3 cameraPosition = RenderInterpolation.ToGodot(pose.Position);
        this.camera.LookAtFromPosition(
            cameraPosition,
            RenderInterpolation.ToGodot(pose.Position + pose.Forward),
            RenderInterpolation.ToGodot(pose.Up));

        Vector3 playerCenter = feetPoint + new Vector3(0.0f, PlayerBody.Height / 2.0f, 0.0f);
        WorldMaterial.SetFade(this.worldMaterial, cameraPosition, playerCenter);

        if (this.hud is not null && this.hudCamera is not null)
        {
            // The shot camera stands where the play camera stands, in a viewport of the Deck size (D-133).
            this.hudCamera.GlobalTransform = this.camera.GlobalTransform;
            this.hud.Draw(this.shotState ?? HudState.Of(this.loop, this.reader.ControllerLast), this.hudCamera, (float)delta);
        }
    }

    /// <summary>The handler of the close request of the root window (F-161). Every exception ends the session, as in each engine callback (F-115).</summary>
    private void OnCloseRequested()
    {
        try
        {
            this.CloseWindow();
        }
        catch (Exception error)
        {
            this.FailCallback(nameof(OnCloseRequested), error);
        }
    }

    /// <summary>
    /// The close of the window ends the session as the test exit does: one end line with the run fields, then the quit,
    /// which writes the frame log and releases the sounds (D-311, F-161). The engine quit with no end line, no frame
    /// log, and no release before the boot turned off its automatic quit. A session that already ended ignores it.
    /// </summary>
    private void CloseWindow()
    {
        if (this.ended)
        {
            return;
        }

        LogFields fields = this.loop is null ? this.SessionFields() : this.EndFields();
        this.logger.Write(LogContextKind.Run, LogLevel.Info, WindowClosedMessage, fields);
        this.Quit(this.sink.ErrorCount == 0 ? ExitSuccess : ExitFailure);
    }

    /// <inheritdoc/>
    public override void _UnhandledInput(InputEvent @event)
    {
        try
        {
            this.ReadInputEvent(@event);
        }
        catch (Exception error)
        {
            this.FailCallback(nameof(_UnhandledInput), error);
        }
    }

    /// <summary>
    /// The body of one input event: the look motion, the press edge of each button, and the device of the last input
    /// (D-447). The reader latches each press, so a press and a release inside one frame still reach a tick (F-135).
    /// A key repeat of a held key is no new press.
    /// </summary>
    private void ReadInputEvent(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            this.reader.AddMouseMotion(motion.Relative.X, motion.Relative.Y);
            this.reader.NoteKeyboardOrMouse();
        }
        else if (@event is InputEventJoypadMotion stick)
        {
            this.reader.AddLookStickMotion(stick.Device, stick.Axis, stick.AxisValue);
            this.reader.LatchTriggerMotion(stick.Device, stick.Axis, stick.AxisValue);
            this.reader.NoteControllerMotion(stick.AxisValue);
        }
        else if (@event is InputEventKey key && key.IsPressed())
        {
            if (!key.IsEcho())
            {
                this.reader.LatchKeyPress(key.Keycode);
            }

            this.reader.NoteKeyboardOrMouse();
        }
        else if (@event is InputEventMouseButton mouseButton && mouseButton.IsPressed())
        {
            this.reader.LatchMouseButtonPress(mouseButton.ButtonIndex);
            this.reader.NoteKeyboardOrMouse();
        }
        else if (@event is InputEventJoypadButton joyButton && joyButton.IsPressed())
        {
            this.reader.LatchJoyButtonPress(joyButton.Device, joyButton.ButtonIndex);
            this.reader.NoteControllerButton();
        }
    }

    /// <summary>The required fields of a run line (D-113), and no entity yet.</summary>
    private static LogFields RunFields(ulong seed, int floor, uint tick)
    {
        LogFields fields = new();
        fields.Add(SeedField, seed);
        fields.Add(FloorField, (long)floor);
        fields.Add(TickField, (long)tick);
        fields.Add(SubsystemField, Subsystem);
        fields.Add(EntitiesField, NoEntities);
        return fields;
    }

    /// <summary>
    /// The run fields of the loop, how the run ended, the bot policy of a bot session, and the frame count and the 99th
    /// percentile when a frame log runs. A frame log with a transition or an expiry adds the count and the slowest frame
    /// of the windows of that kind, and a log with none of a kind adds no field of that kind.
    /// </summary>
    private LogFields EndFields()
    {
        SimulationLoop loop = this.loop ?? throw new InvalidOperationException(StepFailedMessage);
        LogFields fields = RunFields(loop.Seed, loop.Floor, loop.Tick);
        fields.Add(EndStateField, RunEnds.TextOf(loop.End));
        if (this.bot is not null)
        {
            fields.Add(PolicyField, this.bot.Name);
        }

        if (this.frames is not null && this.frames.Frames.Count > 0)
        {
            fields.Add(FramesField, (long)this.frames.Frames.Count);
            fields.Add(FrameMicrosP99Field, this.frames.Percentile99());
        }

        if (this.frames is not null && this.frames.Transitions > 0)
        {
            fields.Add(TransitionsField, (long)this.frames.Transitions);
            fields.Add(TransitionMicrosMaxField, Slowest(this.frames.TransitionMaxima()));
        }

        if (this.frames is not null && this.frames.Expiries > 0)
        {
            fields.Add(ExpiriesField, (long)this.frames.Expiries);
            fields.Add(ExpiryMicrosMaxField, Slowest(this.frames.ExpiryMaxima()));
        }

        return fields;
    }

    /// <summary>
    /// Adds the frame to the trace of the transition test, with the tick time since the last frame and the collector
    /// pause and collections since the last frame, and logs the summary of a window that closes (D-435).
    /// </summary>
    private void TraceFrame(long frameMicros, long uploadMicros)
    {
        if (this.trace is null || this.loop is null)
        {
            this.tickMicros = 0;
            return;
        }

        long pause = (long)GC.GetTotalPauseDuration().TotalMicroseconds;
        int gen0 = GC.CollectionCount(0);
        int gen1 = GC.CollectionCount(1);
        int gen2 = GC.CollectionCount(2);
        TraceFrame frame = new(
            frameMicros,
            this.tickMicros,
            uploadMicros,
            pause - this.lastPauseMicros,
            gen0 - this.lastGen0,
            gen1 - this.lastGen1,
            gen2 - this.lastGen2,
            this.chunks?.IsDigging ?? false);
        this.tickMicros = 0;
        this.lastPauseMicros = pause;
        this.lastGen0 = gen0;
        this.lastGen1 = gen1;
        this.lastGen2 = gen2;

        TransitionSummary? summary = this.trace.AddFrame(frame);
        if (summary is null)
        {
            return;
        }

        LogFields fields = RunFields(this.loop.Seed, this.loop.Floor, this.loop.Tick);
        fields.Add(TraceTransitionField, (long)summary.Transition);
        fields.Add(TraceSlowestField, summary.SlowestFrameMicros);
        fields.Add(TraceTickField, summary.MaxPhysicsMicros);
        fields.Add(TraceUploadField, summary.MaxUploadMicros);
        fields.Add(TracePauseField, summary.GcPauseMicros);
        fields.Add(TraceGen0Field, (long)summary.Gen0);
        fields.Add(TraceGen1Field, (long)summary.Gen1);
        fields.Add(TraceGen2Field, (long)summary.Gen2);
        fields.Add(TraceDiggingField, (long)summary.DiggingFrames);
        this.logger.Write(LogContextKind.Run, LogLevel.Info, TraceMessage, fields);
    }

    /// <summary>The largest of the values. The list holds one value for each mark, and the caller reads it only when a mark exists, so it is never empty.</summary>
    private static long Slowest(IReadOnlyList<long> values)
    {
        long slowest = values[0];
        foreach (long value in values)
        {
            slowest = Math.Max(slowest, value);
        }

        return slowest;
    }

    /// <summary>
    /// Ends the bot session clean. A transition test fails when the frame log holds fewer transitions than the
    /// count of the session, or a transition whose slowest frame is over the hitch budget (D-427, D-435).
    /// </summary>
    private void EndBotSession()
    {
        if (this.transitionTest && this.frames is not null)
        {
            bool tooFew = this.frames.Transitions < this.transitions;
            bool over = false;
            foreach (long slowest in this.frames.TransitionMaxima())
            {
                over = over || slowest > BotSession.HitchBudgetMicros;
            }

            if (tooFew || over)
            {
                this.logger.Write(LogContextKind.Run, LogLevel.Error, tooFew ? TransitionsShortMessage : HitchOverBudgetMessage, this.EndFields());
                this.Quit(ExitFailure);
                return;
            }
        }

        this.logger.Write(LogContextKind.Run, LogLevel.Info, BotEndMessage, this.EndFields());
        this.Quit(this.sink.ErrorCount == 0 ? ExitSuccess : ExitFailure);
    }

    /// <summary>
    /// Reads the user arguments with one parser, loads the content, the models of the body and of the main weapon, the
    /// atlas, and the clips, starts the loop, and builds the scene. A bad argument, or a flag of the Game layer before the
    /// separator, stops the boot before the content loads (D-313, D-317, D-624). The contact sheet flag renders the sheet in place of the loop (D-306). In a play session the mouse is
    /// captured. In the smoke session and the bot session it is not.
    /// </summary>
    private void Boot()
    {
        UserArguments.RejectFlagsBeforeSeparator(OS.GetCmdlineArgs());
        UserArguments arguments = UserArguments.Parse(OS.GetCmdlineUserArgs());
        this.smoke = SmokeSession.IsRequested(arguments);

        // A bad policy name stops the boot before the content loads. With no policy flag, the name is the greedy descender (D-646).
        string policyName = BotSession.PolicyNameOf(arguments);

        // A bad seed stops the boot before the content loads. With no seed flag, the seed is the first seed (D-704, D-705).
        this.seed = BotSession.SeedOf(arguments);
        if (TestExit.IsPressRequested(arguments))
        {
            this.press = TestExit.PressOf(arguments);
        }

        if (FrameLog.IsRequested(arguments))
        {
            this.frameLogPath = FrameLog.PathOf(arguments);
            this.frames = new FrameLog();
        }

        string projectDirectory = ProjectSettings.GlobalizePath(ProjectRoot);
        string contentDirectory = Path.GetFullPath(Path.Combine(projectDirectory, ParentDirectory, ContentDirectoryName));
        ContentSet content = new ContentLoader(new DirectoryContentSource(contentDirectory)).Load();
        this.transitionTest = arguments.Has(BotSession.TransitionsFlag);
        if (this.transitionTest)
        {
            // A run with the enemies dies before ten floors, so the transition test walks floors with none (D-437).
            content = content with { Enemies = [] };
        }

        WeaponDefinition weapon = SimulationLoop.MainWeapon(content);
        BlockbenchModel bodyModel = BlockbenchLoader.Parse(AssetPaths.BodyModel, AssetFile.Read(contentDirectory, AssetPaths.BodyModel));
        BlockbenchModel swordModel = BlockbenchLoader.Parse(weapon.Model, AssetFile.Read(contentDirectory, weapon.Model));
        Image atlasImage = AtlasFile.Load(contentDirectory);
        ImageTexture atlas = AtlasFile.ModelTexture(atlasImage, contentDirectory);
        TextureLayout layout = TextureLayoutFile.Load(contentDirectory);
        BlockTiles tiles = new(layout);
        ImageTexture blockAtlas = BlockAtlas.Build(atlasImage, tiles, contentDirectory);
        if (ContactSheet.IsRequested(arguments))
        {
            this.RenderContactSheet(ContactSheet.PathOf(arguments), contentDirectory, content.Enemies, content.Hunter, atlas, blockAtlas, layout, swordModel);
            return;
        }

        PlayerClips playerClips = PlayerClips.Load(contentDirectory, weapon);
        SimulationLoop loop = new(this.seed, content);
        this.loop = loop;
        if (BotSession.IsRequested(arguments))
        {
            this.bot = BotSession.PolicyFor(policyName, content, loop.Seed);
            this.transitions = BotSession.TransitionsOf(arguments);
            this.trace = this.transitionTest ? new TransitionTrace() : null;
        }

        if (this.smoke)
        {
            this.smokeWalker = new GreedyDescender(content);
        }

        this.currentFeet = loop.Body.Position;
        this.previousFeet = this.currentFeet;
        this.currentPose = loop.Camera();
        this.previousPose = this.currentPose;

        this.worldMaterial = WorldMaterial.Create(blockAtlas);
        this.chunks = new ChunkSwap(this, this.worldMaterial, tiles, new NextFloorWorker(content), loop.Seed);
        this.chunks.Start(loop);

        StandardMaterial3D modelMaterial = ModelMaterial(atlas);
        ModelNodeTree nodes = ModelNodes.Build(bodyModel, modelMaterial, layout);
        ModelNodes.Hold(nodes, EquipmentSlots.Weapon, ModelNodes.Build(swordModel, modelMaterial, layout).Root);
        this.playerNodes = nodes;
        this.playerModel = bodyModel;

        // The player carries the lantern, so it follows the feet and turns with the body (D-678, D-679).
        OmniLight3D lantern = SceneLight.Lantern();
        lantern.Position = SceneLight.LanternOffset;
        nodes.Root.AddChild(lantern);
        this.clips = playerClips;
        this.camera = PlaceholderScene.Camera();
        this.AddChild(nodes.Root);

        // Each enemy draws with the model of its family, and the Overseer with the model of the hunter file (D-673,
        // D-698). The rest pose stands on the feet, so the root offset reads the lowest corner of each model.
        IReadOnlyDictionary<string, EnemyModel> familyModels = EnemyModels.Load(contentDirectory, content.Enemies);
        EnemyModel hunterModel = EnemyModels.Read(contentDirectory, content.Hunter.Model);
        EnemyNodes enemies = new(this, familyModels, hunterModel, swordModel, modelMaterial, layout);
        enemies.Rebuild(loop.Plan, loop.Enemies);
        this.enemyNodes = enemies;
        this.drawnFloor = loop.Floor;
        this.AddChild(this.camera);
        this.AddChild(SceneLight.Environment());

        if (HudShot.IsRequested(arguments))
        {
            this.StartHudShot(HudShot.PathOf(arguments), content.Strings, loop);
            return;
        }

        // The accept action of the engine has no controller input, so every session binds the A button (D-449).
        Navigation.BindAccept();
        this.hud = Hud.Build(content.Strings, this);
        this.hudCamera = this.camera;
        this.logger.Write(LogContextKind.Run, LogLevel.Info, HudBuiltMessage, RunFields(loop.Seed, loop.Floor, loop.Tick));

        // The sounds of each tick follow its Core events and the stride of each body (D-453, D-454).
        this.sounds = SoundBank.Build(contentDirectory, this);
        this.director = new SoundDirector(content.Hunter.SpeedMetresPerSecond(0));
        LogFields loaded = RunFields(loop.Seed, loop.Floor, loop.Tick);
        loaded.Add(SoundFilesField, (long)this.sounds.Files);
        loaded.Add(AudioDriverField, AudioServer.GetDriverName());
        this.logger.Write(LogContextKind.Run, LogLevel.Info, SoundsLoadedMessage, loaded);
        if (this.smoke)
        {
            // No menu screen exists before PR-53, so the smoke session proves the focus map on the fixture (D-446).
            Navigation.RequireControllerActions();
            NavigationFixtureNodes fixture = NavigationFixture.Build(content.Strings, this);
            LogFields built = RunFields(loop.Seed, loop.Floor, loop.Tick);
            built.Add(ControlsField, (long)fixture.Controls.Count);
            this.logger.Write(LogContextKind.Run, LogLevel.Info, FixtureBuiltMessage, built);
        }

        if (!this.smoke && this.bot is null)
        {
            Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
        }

        this.logger.Write(LogContextKind.Run, LogLevel.Info, StartMessage, RunFields(loop.Seed, loop.Floor, loop.Tick));
    }

    /// <summary>
    /// The one material of every model: the atlas with nearest filtering, so a texel stays a square (D-85), and with
    /// mipmaps, so a distant face does not shimmer (D-677).
    /// </summary>
    private static StandardMaterial3D ModelMaterial(Texture2D atlas)
    {
        return new StandardMaterial3D
        {
            AlbedoTexture = atlas,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.NearestWithMipmaps,
        };
    }

    /// <summary>
    /// Renders the contact sheet one shot at a time, writes the PNG file, and quits (D-306). The scene draws some
    /// frames before the first shot, so every shader is ready, and each shot waits for its own frames after the
    /// camera moves. The headless display, a shot with no image, and a write failure are each an error line and
    /// exit code 1 (T-2).
    /// </summary>
    private async void RenderContactSheet(string path, string contentDirectory, IReadOnlyList<EnemyDefinition> families, HunterDefinition hunter, Texture2D atlas, Texture2D blockAtlas, TextureLayout layout, BlockbenchModel swordModel)
    {
        LogFields fields = RunFields(FirstSeed, SimulationLoop.FirstFloor, 0);
        fields.Add(FileField, path);
        try
        {
            if (DisplayServer.GetName() == HeadlessDisplay)
            {
                throw new ContextException(ContactSheetNeedsWindow);
            }

            IReadOnlyList<string> modelPaths = ContactSheet.Models(families, hunter);
            Dictionary<string, BlockbenchModel> models = [];
            foreach (string modelPath in modelPaths)
            {
                models.Add(modelPath, BlockbenchLoader.Parse(modelPath, AssetFile.Read(contentDirectory, modelPath)));
            }

            IReadOnlyList<SheetShot> shots = ContactSheet.Shots(modelPaths);
            ContactSheetNodes nodes = ContactSheetScene.Build(blockAtlas, layout, shots, models, swordModel, ModelMaterial(atlas));
            this.AddChild(nodes.Viewport);
            Image sheet = Image.CreateEmpty(ContactSheet.SheetPixelsWide(), ContactSheet.SheetPixelsHigh(modelPaths.Count), false, Image.Format.Rgb8);
            await this.WaitFrames(ContactSheet.WarmUpFrames);
            foreach (SheetShot shot in shots)
            {
                nodes.Camera.LookAtFromPosition(ContactSheet.CameraPosition(shot), shot.Target, Vector3.Up);
                nodes.Lantern.Position = ContactSheet.LanternPosition(shot);
                await this.WaitFrames(ContactSheet.FramesPerShot);
                Image frame = nodes.Viewport.GetTexture().GetImage();
                if (frame is null || frame.IsEmpty())
                {
                    ContextException error = new(NoShotImage);
                    error.AddContext(ShotField, shot.Index.ToString(CultureInfo.InvariantCulture));
                    throw error;
                }

                frame.Convert(Image.Format.Rgb8);
                sheet.BlitRect(frame, ContactSheet.CropRect(shot), ContactSheet.CellOrigin(shot, modelPaths.Count));
            }

            File.WriteAllBytes(path, sheet.SavePngToBuffer());
            this.logger.Write(LogContextKind.Run, LogLevel.Info, ContactSheetEndMessage, fields);
            this.Quit(this.sink.ErrorCount == 0 ? ExitSuccess : ExitFailure);
        }
        catch (Exception error)
        {
            this.LogFailure(ContactSheetFailedMessage, fields, error);
            this.Quit(ExitFailure);
        }
    }

    /// <summary>
    /// Renders the HUD shot and quits (D-133). The HUD and a camera go into a viewport of the Deck size that shares the
    /// world of the scene. The HUD shows the fixture state, the boss bar placeholder, and one damage number of each
    /// kind above the body, which come after the warm-up so they stand at the start of their lifetime. The headless
    /// display, a frame with no image, and a write failure are each an error line and exit code 1 (T-2).
    /// </summary>
    private async void StartHudShot(string path, Strings strings, SimulationLoop loop)
    {
        LogFields fields = RunFields(loop.Seed, loop.Floor, loop.Tick);
        fields.Add(FileField, path);
        try
        {
            if (DisplayServer.GetName() == HeadlessDisplay)
            {
                throw new ContextException(HudShotNeedsWindow);
            }

            SubViewport viewport = new()
            {
                Size = new Vector2I(HudShot.PixelsWide, HudShot.PixelsHigh),
                RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
                Msaa3D = PlaceholderScene.EdgeSmoothing,
            };
            this.AddChild(viewport);
            Camera3D shotCamera = PlaceholderScene.Camera();
            viewport.AddChild(shotCamera);
            Hud shotHud = Hud.Build(strings, viewport);
            shotHud.ShowBoss(shotHud.BossPlaceholderName(), HudShot.BossHealth, HudShot.BossMost);
            this.shotState = new HudState(HudShot.Health, Player.MaxHealth, loop.Timer.Remaining, loop.Timer.Expired, true, true, false);
            this.hud = shotHud;
            this.hudCamera = shotCamera;

            await this.WaitFrames(HudShot.WarmUpFrames);
            shotHud.AfterTick(loop);
            shotHud.AddNumber(HudShot.NumberOwner, HudShot.DealtAmount, false);
            shotHud.AddNumber(HudShot.NumberOwner, HudShot.TakenAmount, true);
            await this.WaitFrames(ContactSheet.FramesPerShot);

            Image frame = viewport.GetTexture().GetImage();
            if (frame is null || frame.IsEmpty())
            {
                throw new ContextException(NoHudImage);
            }

            frame.Convert(Image.Format.Rgb8);
            File.WriteAllBytes(path, frame.SavePngToBuffer());
            this.logger.Write(LogContextKind.Run, LogLevel.Info, HudShotEndMessage, fields);
            this.Quit(this.sink.ErrorCount == 0 ? ExitSuccess : ExitFailure);
        }
        catch (Exception error)
        {
            this.LogFailure(HudShotFailedMessage, fields, error);
            this.Quit(ExitFailure);
        }
    }

    /// <summary>Waits until the engine draws the given count of frames.</summary>
    private async Task WaitFrames(int count)
    {
        for (int frame = 0; frame < count; frame++)
        {
            await this.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        }
    }

    /// <summary>
    /// Writes the error line of an engine callback that failed, and quits with exit code 1. The engine glue would print
    /// the exception and call the callbacks again, so a session went on half updated and ended with exit code 0 (T-2,
    /// F-115). The line carries the run fields of the loop, or of the boot when no loop exists yet.
    /// </summary>
    private void FailCallback(string callback, Exception error)
    {
        LogFields fields = this.SessionFields();
        fields.Add(CallbackField, callback);
        this.LogFailure(CallbackFailedMessage, fields, error);
        this.Quit(ExitFailure);
    }

    /// <summary>The run fields of the loop, or of the boot when no loop exists yet: the seed of the session, the first floor, and tick zero.</summary>
    private LogFields SessionFields()
    {
        return this.loop is null
            ? RunFields(this.seed, SimulationLoop.FirstFloor, 0)
            : RunFields(this.loop.Seed, this.loop.Floor, this.loop.Tick);
    }

    /// <summary>Writes the error line of a failure, with the text and the type of the exception and of each inner exception after the run fields (F-121).</summary>
    private void LogFailure(string message, LogFields fields, Exception error)
    {
        FailureFields.Add(fields, error);
        this.logger.Write(LogContextKind.Run, LogLevel.Error, message, fields);
    }

    /// <summary>
    /// Ends the session. The frame log, when one runs, goes to its file first, and any write failure, such as a disk
    /// error, a path the user cannot write, or a path the system rejects, is an error line with the run fields of the
    /// loop and turns the exit code to failure. The sound bank releases its streams. The engine quits at the end of the
    /// frame in every case, and no later tick runs.
    /// </summary>
    private void Quit(int exitCode)
    {
        this.ended = true;
        try
        {
            this.sounds?.Release();
            if (this.frames is not null)
            {
                try
                {
                    File.WriteAllText(this.frameLogPath, this.frames.Text());
                }
                catch (Exception error)
                {
                    LogFields fields = this.SessionFields();
                    fields.Add(FileField, this.frameLogPath);
                    this.LogFailure(FrameLogFailedMessage, fields, error);
                    exitCode = ExitFailure;
                }
            }
        }
        finally
        {
            // A failure that left this method before the quit left the session running with no tick, and the
            // process never ended (F-122). An error that leaves here reaches the guard of the caller, which writes
            // its error line and quits with exit code 1.
            this.GetTree().Quit(exitCode);
        }
    }
}
