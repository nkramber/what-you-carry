using System;
using System.IO;
using Godot;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Game.Content;
using WhatYouCarry.Game.Input;
using WhatYouCarry.Game.Logging;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Game.Smoke;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game;

/// <summary>
/// The root node (D-63). It owns one Core simulation, steps it once per physics frame at 60 Hz (D-73), builds
/// one intent per tick from the input (D-77), and draws the player box and the camera between the last two
/// ticks (D-245). No engine physics runs here (G-3).
/// </summary>
/// <remarks>
/// <para>
/// The fixed step of the engine is the clock. A physics frame is one tick, and the interpolation fraction of
/// the engine places each render frame between the last two ticks. Core reads no clock and no engine value.
/// </para>
/// <para>
/// A boot failure and a step failure both write an error line and quit with exit code 1 (T-2). The smoke
/// session quits with exit code 0 only when the log holds no error line (D-114). The content comes from the
/// directory next to the project directory, which is the content directory of the checkout (D-219).
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

    /// <summary>The message of the error line of a boot failure.</summary>
    public const string BootFailedMessage = "The boot failed, and the game quits.";

    /// <summary>The message of the error line of a tick failure.</summary>
    public const string StepFailedMessage = "A tick failed, and the game quits.";

    private const string ProjectRoot = "res://";
    private const string ParentDirectory = "..";
    private const string ContentDirectoryName = "content";
    private const string SeedField = "seed";
    private const string FloorField = "floor";
    private const string TickField = "tick";
    private const string SubsystemField = "subsystem";
    private const string EntitiesField = "entities";
    private const string ErrorField = "error";

    private static readonly long[] NoEntities = [];

    private readonly PrintLogSink sink = new();
    private readonly JsonlLogger logger;
    private readonly IntentBuilder builder = new();
    private readonly InputReader reader = new(new EnginePoll());

    private SimulationLoop? loop;
    private MeshInstance3D? playerBox;
    private Camera3D? camera;
    private bool smoke;
    private bool ended;
    private CoreVector3 previousFeet;
    private CoreVector3 currentFeet;
    private CameraPose previousPose;
    private CameraPose currentPose;

    /// <summary>A root node with its logger. The boot runs when the node enters the tree.</summary>
    public Main()
    {
        this.logger = new JsonlLogger(this.sink);
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

        Intent intent = this.smoke
            ? SmokeSession.IntentAt(this.loop.Tick)
            : this.builder.Build(this.loop.Tick, this.reader.Read());

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

        if (this.smoke && this.loop.Tick >= SmokeSession.Ticks)
        {
            this.logger.Write(LogContextKind.Run, LogLevel.Info, EndMessage, RunFields(this.loop.Seed, this.loop.Floor, this.loop.Tick));
            this.Quit(this.sink.ErrorCount == 0 ? ExitSuccess : ExitFailure);
        }
    }

    /// <inheritdoc/>
    public override void _Process(double delta)
    {
        if (this.loop is null || this.playerBox is null || this.camera is null)
        {
            return;
        }

        float fraction = (float)Engine.GetPhysicsInterpolationFraction();
        CoreVector3 feet = RenderInterpolation.Between(this.previousFeet, this.currentFeet, fraction);
        this.playerBox.Position = RenderInterpolation.ToGodot(feet) + new Vector3(0.0f, PlayerBody.Height / 2.0f, 0.0f);

        CameraPose pose = RenderInterpolation.Between(this.previousPose, this.currentPose, fraction);
        this.camera.LookAtFromPosition(
            RenderInterpolation.ToGodot(pose.Position),
            RenderInterpolation.ToGodot(pose.Position + pose.Forward),
            RenderInterpolation.ToGodot(pose.Up));
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

    /// <summary>
    /// Reads the user arguments, loads the content, starts the loop, and builds the scene. In a play session the
    /// mouse is captured. In the smoke session it is not, because no window exists.
    /// </summary>
    private void Boot()
    {
        string[] arguments = OS.GetCmdlineUserArgs();
        this.smoke = SmokeSession.IsRequested(arguments);

        string projectDirectory = ProjectSettings.GlobalizePath(ProjectRoot);
        string contentDirectory = Path.GetFullPath(Path.Combine(projectDirectory, ParentDirectory, ContentDirectoryName));
        ContentSet content = new ContentLoader(new DirectoryContentSource(contentDirectory)).Load();

        SimulationLoop loop = new(FirstSeed, content);
        this.loop = loop;
        this.currentFeet = loop.Body.Position;
        this.previousFeet = this.currentFeet;
        this.currentPose = loop.Camera();
        this.previousPose = this.currentPose;

        this.playerBox = PlaceholderScene.PlayerBox();
        this.camera = PlaceholderScene.Camera();
        this.AddChild(PlaceholderScene.Floor(loop.Grid.SizeX, loop.Grid.SizeZ, loop.Body.Position.Y));
        this.AddChild(this.playerBox);
        this.AddChild(this.camera);
        this.AddChild(PlaceholderScene.Light());

        if (!this.smoke)
        {
            Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
        }

        this.logger.Write(LogContextKind.Run, LogLevel.Info, StartMessage, RunFields(loop.Seed, loop.Floor, loop.Tick));
    }

    /// <summary>Writes the error line of a failure, with the text of the exception after the run fields.</summary>
    private void LogFailure(string message, LogFields fields, Exception error)
    {
        fields.Add(ErrorField, error.Message);
        this.logger.Write(LogContextKind.Run, LogLevel.Error, message, fields);
    }

    /// <summary>Ends the session. The engine quits at the end of the frame, and no later tick runs.</summary>
    private void Quit(int exitCode)
    {
        this.ended = true;
        this.GetTree().Quit(exitCode);
    }
}
