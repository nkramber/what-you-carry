using System;
using System.Collections.Generic;
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
using WhatYouCarry.Game.Content;
using WhatYouCarry.Game.Input;
using WhatYouCarry.Game.Logging;
using WhatYouCarry.Game.Measure;
using WhatYouCarry.Game.Models;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Game.Review;
using WhatYouCarry.Game.Smoke;
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
/// A boot failure, a step failure, and a pose failure each write an error line and quit with exit code 1 (T-2). The
/// smoke session quits with exit code 0 only when the log holds no error line (D-114). The bot session of M-3 drives
/// the loop with the greedy descender over one floor, and the frame log flag writes every frame time to a file
/// at the end of any session (D-295, D-296). The content, the models, the clips, and the atlas come from the directory
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
/// The contact sheet flag starts no loop. It renders every block material and the body with the sword at game zoom
/// to one PNG file for the review of the owner, and quits (D-306, D-336).
/// </para>
/// </remarks>
public partial class Main : Node3D
{
    /// <summary>The seed of the first run. The hub of PR-30 picks a seed per run.</summary>
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

    /// <summary>The message of the error line of a boot failure.</summary>
    public const string BootFailedMessage = "The boot failed, and the game quits.";

    /// <summary>The message of the error line of a tick failure.</summary>
    public const string StepFailedMessage = "A tick failed, and the game quits.";

    /// <summary>The message of the error line of a frame whose pose of the player failed.</summary>
    public const string PoseFailedMessage = "The pose of the player failed, and the game quits.";

    /// <summary>The message of the error line of a bot session whose tick budget passed on the first floor.</summary>
    public const string BotStuckMessage = "The bot session passed its tick budget on the first floor, and the game quits.";

    /// <summary>The message of the error line of a frame log that the game could not write.</summary>
    public const string FrameLogFailedMessage = "The frame log could not be written, and the game quits.";

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
    private const string FloorField = "floor";
    private const string TickField = "tick";
    private const string SubsystemField = "subsystem";
    private const string EntitiesField = "entities";
    private const string ErrorField = "error";
    private const string FileField = "file";
    private const string ShotField = "shot";
    private const string HeadlessDisplay = "headless";
    private const string NoShotImage = "The viewport of the contact sheet gave no image for a shot.";

    private static readonly long[] NoEntities = [];

    private readonly PrintLogSink sink = new();
    private readonly JsonlLogger logger;
    private readonly IInputPoll poll = new EnginePoll();
    private readonly IntentBuilder builder = new();
    private readonly InputReader reader;

    private SimulationLoop? loop;
    private ModelNodeTree? playerNodes;
    private BlockbenchModel? playerModel;
    private PlayerClips? clips;
    private Camera3D? camera;
    private ShaderMaterial? worldMaterial;
    private GreedyDescender? bot;
    private EnemyNodes? enemyNodes;
    private int drawnFloor;
    private ScriptedPress? press;
    private FrameLog? frames;
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
            this.Boot();
        }
        catch (Exception error)
        {
            // The boot has no loop yet, so the line carries the first seed, the first floor, and tick zero.
            this.LogFailure(BootFailedMessage, RunFields(FirstSeed, SimulationLoop.FirstFloor, 0), error);
            this.Quit(ExitFailure);
        }
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        if (this.loop is null || this.ended)
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
            // code, so the session ends clean and its log names the end kind (D-403).
            this.logger.Write(LogContextKind.Run, LogLevel.Info, RunEndedMessage, this.EndFields());
            this.Quit(this.sink.ErrorCount == 0 ? ExitSuccess : ExitFailure);
            return;
        }

        Intent intent;
        if (this.smoke)
        {
            intent = SmokeSession.IntentAt(this.loop.Tick);
        }
        else if (this.bot is not null)
        {
            intent = this.bot.Next(this.loop);
        }
        else
        {
            intent = this.builder.Build(this.loop.Tick, this.reader.Read());
        }

        try
        {
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

        // A descent digs a new floor with its own enemies, so the trees of the old floor go and the new ones come.
        if (this.enemyNodes is not null)
        {
            if (this.loop.Floor != this.drawnFloor)
            {
                this.enemyNodes.Rebuild(this.loop.Enemies);
                this.drawnFloor = this.loop.Floor;
            }

            this.enemyNodes.AfterTick(this.loop.Enemies, this.loop.Hunter);
        }

        // The walk reads the horizontal distance of the tick, and its amount follows the speed (D-333).
        float stepX = this.currentFeet.X - this.previousFeet.X;
        float stepZ = this.currentFeet.Z - this.previousFeet.Z;
        float stride = MathF.Sqrt((stepX * stepX) + (stepZ * stepZ));
        this.walked += stride;
        this.walkAmount = WalkCycle.Amount(stride / PlayerBody.TickSeconds);

        if (this.smoke && this.loop.Tick >= SmokeSession.Ticks)
        {
            this.logger.Write(LogContextKind.Run, LogLevel.Info, EndMessage, this.EndFields());
            this.Quit(this.sink.ErrorCount == 0 ? ExitSuccess : ExitFailure);
        }
        else if (this.bot is not null && BotSession.IsComplete(this.loop))
        {
            this.logger.Write(LogContextKind.Run, LogLevel.Info, BotEndMessage, this.EndFields());
            this.Quit(this.sink.ErrorCount == 0 ? ExitSuccess : ExitFailure);
        }
        else if (this.bot is not null && BotSession.IsStuck(this.loop))
        {
            this.logger.Write(LogContextKind.Run, LogLevel.Error, BotStuckMessage, this.EndFields());
            this.Quit(ExitFailure);
        }
    }

    /// <inheritdoc/>
    public override void _Process(double delta)
    {
        if (this.loop is null || this.playerNodes is null || this.playerModel is null || this.clips is null || this.camera is null || this.worldMaterial is null || this.ended)
        {
            return;
        }

        this.frames?.Add(delta);

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
    }

    /// <inheritdoc/>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            this.reader.AddMouseMotion(motion.Relative.X, motion.Relative.Y);
        }
        else if (@event is InputEventJoypadMotion stick)
        {
            this.reader.AddLookStickMotion(stick.Device, stick.Axis, stick.AxisValue);
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

    /// <summary>The run fields of the loop, how the run ended, and the frame count and the 99th percentile when a frame log runs.</summary>
    private LogFields EndFields()
    {
        SimulationLoop loop = this.loop ?? throw new InvalidOperationException(StepFailedMessage);
        LogFields fields = RunFields(loop.Seed, loop.Floor, loop.Tick);
        fields.Add(EndStateField, RunEnds.TextOf(loop.End));
        if (this.frames is not null && this.frames.Frames.Count > 0)
        {
            fields.Add(FramesField, (long)this.frames.Frames.Count);
            fields.Add(FrameMicrosP99Field, this.frames.Percentile99());
        }

        return fields;
    }

    /// <summary>
    /// Reads the user arguments with one parser, loads the content, the models of the body and of the main weapon, the
    /// atlas, and the clips, starts the loop, and builds the scene. A bad argument stops the boot before the content loads
    /// (D-313, D-317). The contact sheet flag renders the sheet in place of the loop (D-306). In a play session the mouse is
    /// captured. In the smoke session and the bot session it is not.
    /// </summary>
    private void Boot()
    {
        UserArguments arguments = UserArguments.Parse(OS.GetCmdlineUserArgs());
        this.smoke = SmokeSession.IsRequested(arguments);
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
        WeaponDefinition weapon = SimulationLoop.MainWeapon(content);
        BlockbenchModel bodyModel = BlockbenchLoader.Parse(AssetPaths.BodyModel, AssetFile.Read(contentDirectory, AssetPaths.BodyModel));
        BlockbenchModel swordModel = BlockbenchLoader.Parse(weapon.Model, AssetFile.Read(contentDirectory, weapon.Model));
        ImageTexture atlas = AtlasFile.Load(contentDirectory);
        if (ContactSheet.IsRequested(arguments))
        {
            this.RenderContactSheet(ContactSheet.PathOf(arguments), atlas, bodyModel, swordModel);
            return;
        }

        PlayerClips playerClips = PlayerClips.Load(contentDirectory, weapon);
        SimulationLoop loop = new(FirstSeed, content);
        this.loop = loop;
        if (BotSession.IsRequested(arguments))
        {
            this.bot = new GreedyDescender(content);
        }

        this.currentFeet = loop.Body.Position;
        this.previousFeet = this.currentFeet;
        this.currentPose = loop.Camera();
        this.previousPose = this.currentPose;

        this.worldMaterial = WorldMaterial.Create(atlas);
        foreach (MeshInstance3D chunk in ChunkNodes.Build(loop.Grid, this.worldMaterial))
        {
            this.AddChild(chunk);
        }

        StandardMaterial3D modelMaterial = ModelMaterial(atlas);
        ModelNodeTree nodes = ModelNodes.Build(bodyModel, modelMaterial);
        ModelNodes.Hold(nodes, EquipmentSlots.Weapon, ModelNodes.Build(swordModel, modelMaterial).Root);
        this.playerNodes = nodes;
        this.playerModel = bodyModel;
        this.clips = playerClips;
        this.camera = PlaceholderScene.Camera();
        this.AddChild(nodes.Root);

        // Every enemy draws with the body model until PR-62 gives its family one (D-401). The rest pose stands on
        // the feet, so the root offset reads the lowest corner of that pose.
        float restLowest = ModelPose.LowestPoint(AssetPaths.BodyModel, bodyModel, BodyPose.RestRotations());
        EnemyNodes enemies = new(this, bodyModel, swordModel, modelMaterial, restLowest);
        enemies.Rebuild(loop.Enemies);
        this.enemyNodes = enemies;
        this.drawnFloor = loop.Floor;
        this.AddChild(this.camera);
        this.AddChild(PlaceholderScene.Light());

        if (!this.smoke && this.bot is null)
        {
            Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
        }

        this.logger.Write(LogContextKind.Run, LogLevel.Info, StartMessage, RunFields(loop.Seed, loop.Floor, loop.Tick));
    }

    /// <summary>The one material of every model: the atlas with nearest filtering, so a texel stays a square (D-85).</summary>
    private static StandardMaterial3D ModelMaterial(Texture2D atlas)
    {
        return new StandardMaterial3D
        {
            AlbedoTexture = atlas,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
        };
    }

    /// <summary>
    /// Renders the contact sheet one shot at a time, writes the PNG file, and quits (D-306). The scene draws some
    /// frames before the first shot, so every shader is ready, and each shot waits for its own frames after the
    /// camera moves. The headless display, a shot with no image, and a write failure are each an error line and
    /// exit code 1 (T-2).
    /// </summary>
    private async void RenderContactSheet(string path, Texture2D atlas, BlockbenchModel bodyModel, BlockbenchModel swordModel)
    {
        LogFields fields = RunFields(FirstSeed, SimulationLoop.FirstFloor, 0);
        fields.Add(FileField, path);
        try
        {
            if (DisplayServer.GetName() == HeadlessDisplay)
            {
                throw new ContextException(ContactSheetNeedsWindow);
            }

            ContactSheetNodes nodes = ContactSheetScene.Build(atlas, bodyModel, swordModel, ModelMaterial(atlas));
            this.AddChild(nodes.Viewport);
            Image sheet = Image.CreateEmpty(ContactSheet.SheetPixelsWide(), ContactSheet.SheetPixelsHigh(), false, Image.Format.Rgb8);
            await this.WaitFrames(ContactSheet.WarmUpFrames);
            foreach (SheetShot shot in ContactSheet.Shots())
            {
                nodes.Camera.LookAtFromPosition(ContactSheet.CameraPosition(shot), shot.Target, Vector3.Up);
                await this.WaitFrames(ContactSheet.FramesPerShot);
                Image frame = nodes.Viewport.GetTexture().GetImage();
                if (frame is null || frame.IsEmpty())
                {
                    ContextException error = new(NoShotImage);
                    error.AddContext(ShotField, shot.Index.ToString(CultureInfo.InvariantCulture));
                    throw error;
                }

                frame.Convert(Image.Format.Rgb8);
                sheet.BlitRect(frame, ContactSheet.CropRect(shot), ContactSheet.CellOrigin(shot));
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

    /// <summary>Waits until the engine draws the given count of frames.</summary>
    private async Task WaitFrames(int count)
    {
        for (int frame = 0; frame < count; frame++)
        {
            await this.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        }
    }

    /// <summary>Writes the error line of a failure, with the text of the exception after the run fields.</summary>
    private void LogFailure(string message, LogFields fields, Exception error)
    {
        fields.Add(ErrorField, error.Message);
        this.logger.Write(LogContextKind.Run, LogLevel.Error, message, fields);
    }

    /// <summary>
    /// Ends the session. The frame log, when one runs, goes to its file first, and a write failure of either
    /// kind, a disk error or a path the user cannot write, turns the exit code to failure. The engine quits at
    /// the end of the frame, and no later tick runs.
    /// </summary>
    private void Quit(int exitCode)
    {
        this.ended = true;
        if (this.frames is not null)
        {
            try
            {
                File.WriteAllText(this.frameLogPath, this.frames.Text());
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                // A path that the user cannot write raises the second kind, and it is not an IOException.
                LogFields fields = RunFields(FirstSeed, SimulationLoop.FirstFloor, 0);
                fields.Add(FileField, this.frameLogPath);
                this.LogFailure(FrameLogFailedMessage, fields, error);
                exitCode = ExitFailure;
            }
        }

        this.GetTree().Quit(exitCode);
    }
}
