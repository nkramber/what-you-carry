using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Content;
using WhatYouCarry.Game.Logging;
using WhatYouCarry.Tools.DetLint;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The shape of the Game project and its workflows (D-63, D-73, D-219, G-3; PR-12 exit tests 5 and 6).</summary>
public sealed class GameShapeTests
{
    /// <summary>The engine types that would feed the simulation from the engine, and that no Game file names (G-3, D-76, D-80).</summary>
    public static readonly IReadOnlyList<string> EnginePhysicsNames =
    [
        "PhysicsBody3D",
        "CharacterBody3D",
        "RigidBody3D",
        "StaticBody3D",
        "AnimatableBody3D",
        "Area3D",
        "CollisionShape3D",
        "CollisionPolygon3D",
        "PhysicsServer3D",
        "PhysicsDirectSpaceState3D",
        "RayCast3D",
        "ShapeCast3D",
        "NavigationAgent3D",
        "NavigationRegion3D",
        "NavigationServer3D",
        "NavigationMesh",
        "NavigationLink3D",
        "NavigationObstacle3D",
    ];

    /// <summary>PR-12 exit test 6. No Game file names a physics body or a navigation node (G-3).</summary>
    [Fact]
    public void GameReadsNoEnginePhysics()
    {
        string root = RepositoryRoot.Find();
        IReadOnlyList<string> files = GameStringScan.SourceFiles(root);
        Assert.NotEmpty(files);
        foreach (string file in files)
        {
            Assert.Null(EnginePhysicsDefect(File.ReadAllText(file), Path.GetFileName(file)));
        }
    }

    /// <summary>The check names the type and the file, so a regression cannot pass in silence.</summary>
    [Fact]
    public void EnginePhysicsCheckFindsABodyAndAnAgent()
    {
        Assert.Equal(
            "Player.cs names the engine type CharacterBody3D, and Core owns every collision and every path (G-3).",
            EnginePhysicsDefect("public partial class Player : CharacterBody3D { }", "Player.cs"));
        Assert.Equal(
            "Path.cs names the engine type NavigationAgent3D, and Core owns every collision and every path (G-3).",
            EnginePhysicsDefect("NavigationAgent3D agent = new();", "Path.cs"));
        Assert.Null(EnginePhysicsDefect("public partial class Main : Node3D { }", "Main.cs"));
    }

    /// <summary>
    /// F-115. The body of each engine callback in the Game layer is one try statement whose last catch takes every
    /// exception with no filter and ends the session. The engine glue prints an exception that leaves a callback and
    /// calls the callbacks again, so a session went on half updated and quit with exit code 0 (T-2, D-114).
    /// </summary>
    [Fact]
    public void EveryEngineCallbackCatchesEveryException()
    {
        string root = RepositoryRoot.Find();
        List<string> callbacks = [];
        foreach (string file in GameStringScan.SourceFiles(root))
        {
            foreach (string defect in EngineCallbackDefects(File.ReadAllText(file), Path.GetFileName(file), callbacks))
            {
                Assert.Fail(defect);
            }
        }

        Assert.Contains("Main.cs _PhysicsProcess", callbacks);
        Assert.Contains("Main.cs _Process", callbacks);
        Assert.Contains("Main.cs _UnhandledInput", callbacks);
        Assert.Contains("Main.cs _Ready", callbacks);
    }

    /// <summary>The check names the file and the callback of a body with no catch-all, and passes a guarded body.</summary>
    [Fact]
    public void EngineCallbackCheckFindsAnUnguardedBody()
    {
        const string Unguarded = "class Main { public override void _Process(double delta) { this.Draw(); } }";
        const string Filtered = "class Main { public override void _Process(double delta) { try { this.Draw(); } catch (Exception error) when (error is IOException) { this.Fail(error); } } }";
        const string Guarded = "class Main { public override void _Process(double delta) { try { this.Draw(); } catch (Exception error) { this.Fail(error); } } }";
        List<string> callbacks = [];

        Assert.Equal(["Main.cs: the engine callback _Process is not one try statement with a catch of every exception (T-2, F-115)."], EngineCallbackDefects(Unguarded, "Main.cs", callbacks));
        Assert.Single(EngineCallbackDefects(Filtered, "Main.cs", callbacks));
        Assert.Empty(EngineCallbackDefects(Guarded, "Main.cs", callbacks));
    }

    /// <summary>
    /// One defect for each override whose name starts with an underscore, the engine callbacks of Godot, and whose
    /// body is not one try statement with a catch of <c>Exception</c> and no filter. It adds the name of each
    /// callback that it reads to the list.
    /// </summary>
    private static List<string> EngineCallbackDefects(string source, string fileName, List<string> callbacks)
    {
        List<string> defects = [];
        Microsoft.CodeAnalysis.SyntaxNode tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source).GetRoot();
        foreach (Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax method in tree.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
        {
            string name = method.Identifier.Text;
            bool isOverride = method.Modifiers.Any(modifier => modifier.Text == "override");
            if (!isOverride || !name.StartsWith('_'))
            {
                continue;
            }

            callbacks.Add($"{fileName} {name}");
            bool guarded = method.Body is not null
                && method.Body.Statements.Count == 1
                && method.Body.Statements[0] is Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax attempt
                && attempt.Catches.Any(clause => clause.Filter is null && clause.Declaration?.Type.ToString() == "Exception");
            if (!guarded)
            {
                defects.Add($"{fileName}: the engine callback {name} is not one try statement with a catch of every exception (T-2, F-115).");
            }
        }

        return defects;
    }

    /// <summary>The project runs the root scene, and the fixed step of the engine is 60 Hz (D-63, D-73).</summary>
    [Fact]
    public void ProjectRunsTheMainSceneAtSixtyTicks()
    {
        string project = RepositoryRoot.ReadFile("WhatYouCarry.Game/project.godot");
        Assert.Contains("run/main_scene=\"res://Main.tscn\"", project, StringComparison.Ordinal);
        Assert.Contains("common/physics_ticks_per_second=60", project, StringComparison.Ordinal);
    }

    /// <summary>The game starts with no engine splash image, by the owner's request in PR #85.</summary>
    [Fact]
    public void BootShowsNoSplashImage()
    {
        string project = RepositoryRoot.ReadFile("WhatYouCarry.Game/project.godot");
        int application = project.IndexOf("[application]", StringComparison.Ordinal);
        Assert.True(application >= 0, "The project has no application section.");
        Assert.Contains("boot_splash/show_image=false", SectionAfter(project, application), StringComparison.Ordinal);
    }

    /// <summary>
    /// PR-60 exit test 1. The project opens the window in the borderless fullscreen of the engine, and it sets no
    /// window size, so the viewport takes the resolution of the display (D-310). The exclusive mode changes the
    /// video mode of the display, and the project never asks for it.
    /// </summary>
    [Fact]
    public void WindowOpensFullscreen()
    {
        string project = RepositoryRoot.ReadFile("WhatYouCarry.Game/project.godot");
        int display = project.IndexOf("[display]", StringComparison.Ordinal);
        Assert.True(display >= 0, "The project has no display section.");

        string section = SectionAfter(project, display);
        Assert.Contains($"window/size/mode={(int)DisplayServer.WindowMode.Fullscreen}", section, StringComparison.Ordinal);
        Assert.DoesNotContain("window/size/viewport_width", project, StringComparison.Ordinal);
        Assert.DoesNotContain("window/size/viewport_height", project, StringComparison.Ordinal);
        Assert.DoesNotContain("window/size/window_width_override", project, StringComparison.Ordinal);
        Assert.DoesNotContain("window/size/window_height_override", project, StringComparison.Ordinal);
        Assert.Equal(3, (int)DisplayServer.WindowMode.Fullscreen);
    }

    /// <summary>The root scene is one node with the root script, and the script has its uid file (D-63).</summary>
    [Fact]
    public void MainSceneAttachesTheRootScript()
    {
        string scene = RepositoryRoot.ReadFile("WhatYouCarry.Game/Main.tscn");
        Assert.Contains("type=\"Script\" path=\"res://Main.cs\"", scene, StringComparison.Ordinal);
        Assert.Contains("[node name=\"Main\" type=\"Node3D\"]", scene, StringComparison.Ordinal);
        Assert.StartsWith("uid://", RepositoryRoot.ReadFile("WhatYouCarry.Game/Main.cs.uid"), StringComparison.Ordinal);
    }

    /// <summary>The error prefix of the print sink is the start of every error line of the logger, and of no other line (D-212).</summary>
    [Fact]
    public void ErrorPrefixMatchesTheLogger()
    {
        Assert.StartsWith(PrintLogSink.ErrorPrefix, JsonlLogger.BuildLine(LogLevel.Error, "x", new LogFields()), StringComparison.Ordinal);
        Assert.False(JsonlLogger.BuildLine(LogLevel.Info, "x", new LogFields()).StartsWith(PrintLogSink.ErrorPrefix, StringComparison.Ordinal));
        Assert.False(JsonlLogger.BuildLine(LogLevel.Warning, "x", new LogFields()).StartsWith(PrintLogSink.ErrorPrefix, StringComparison.Ordinal));
    }

    /// <summary>The directory source reads the content of the checkout with forward-slash paths, and the loader takes it (D-219).</summary>
    [Fact]
    public void DirectoryContentSourceReadsTheCheckout()
    {
        string content = Path.Combine(RepositoryRoot.Find(), "content");
        IReadOnlyList<ContentFile> files = new DirectoryContentSource(content).Read();

        Assert.Equal(new RepositoryContentSource().Read().Count, files.Count);
        Assert.Contains(files, file => file.Path == Strings.FilePath);
        Assert.DoesNotContain(files, file => file.Path.Contains('\\', StringComparison.Ordinal));
        Assert.Equal(TestWorld.Content.Hash, new ContentLoader(new DirectoryContentSource(content)).Load().Hash);
    }

    /// <summary>
    /// Every content source skips the model, texture, and audio directories, because the animation, paint, palette,
    /// recipe, layout, and sound parameter files there are JSON that Core never reads (D-298, D-305, D-453, D-505).
    /// </summary>
    [Fact]
    public void ContentSourcesSkipTheAssetDirectories()
    {
        using TemporaryContentDirectory content = new();
        content.Write("floors/a.json", "{}");
        content.Write("models/rig.attack.json", "{}");
        content.Write("models/rig.paint.json", "{}");
        content.Write("models/armor/chest.json", "{}");
        content.Write("textures/palette.json", "{}");
        content.Write("textures/recipes/raw-stone.json", "{}");
        content.Write("textures/blocks.json", "{}");
        content.Write("textures/layout.json", "{}");
        content.Write("texturesets/b.json", "{}");
        content.Write("audio/sfx/footstep.json", "{}");

        string[] gamePaths = new DirectoryContentSource(content.Content).Read().Select(file => file.Path).OrderBy(path => path, StringComparer.Ordinal).ToArray();
        string[] toolPaths = new WhatYouCarry.Tools.BotRunner.DirectoryContentSource(content.Content).Read().Select(file => file.Path).OrderBy(path => path, StringComparer.Ordinal).ToArray();

        // A directory whose name only starts with the texture directory name is not the texture directory.
        Assert.Equal(new[] { "floors/a.json", "texturesets/b.json" }, gamePaths);
        Assert.Equal(new[] { "floors/a.json", "texturesets/b.json" }, toolPaths);
        Assert.True(ContentLoader.IsAssetPath("textures/palette.json"));
        Assert.True(ContentLoader.IsAssetPath("models/player.attack.json"));
        Assert.False(ContentLoader.IsAssetPath("floors/a.json"));
        Assert.False(ContentLoader.IsAssetPath("texturesets/b.json"));
        Assert.True(ContentLoader.IsAssetPath("audio/sfx/footstep.json"));
        Assert.False(ContentLoader.IsAssetPath("audiobooks/c.json"));
    }

    /// <summary>An absent directory is an error that names the path, and never an empty set (T-2).</summary>
    [Fact]
    public void DirectoryContentSourceRejectsAnAbsentDirectory()
    {
        string absent = Path.Combine(RepositoryRoot.Find(), "no-such-content");
        ContextException error = Assert.Throws<ContextException>(() => new DirectoryContentSource(absent).Read());
        Assert.Contains(absent, error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// PR-12 exit test 5, the shape. One job per platform, each with the pinned engine from the cache, the
    /// executable in the variable, and the one test of the smoke category (D-61, D-114).
    /// </summary>
    [Fact]
    public void SmokeWorkflowHasOneJobPerPlatform()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/smoke.yml");
        Dictionary<string, string> runsOnByJob = WorkflowText.RunsOnByJob(workflow);
        Assert.Equal(4, runsOnByJob.Count);
        Assert.Equal("ubuntu-latest", runsOnByJob["ci-skip"]);
        Assert.Equal("ubuntu-latest", runsOnByJob["linux-x64"]);
        Assert.Equal("windows-latest", runsOnByJob["windows-x64"]);
        Assert.Contains("macos-arm64-self-hosted", runsOnByJob["macos-arm64"], StringComparison.Ordinal);

        Assert.Contains("GODOT_VERSION: 4.7.2-stable", workflow, StringComparison.Ordinal);
        Assert.Equal(3, Count(workflow, "uses: actions/cache@v4"));
        Assert.Equal(3, Count(workflow, $"{SmokeSessionTests.GodotVariable}:"));
        Assert.Equal(3, Count(workflow, $"--filter \"Category={SmokeSessionTests.SmokeCategory}\""));
    }

    /// <summary>
    /// The CI jobs hold no engine, so they leave the smoke category to the smoke workflow. Each hosted leg runs the
    /// sweep category in one job and every other test in the other job, so the two jobs cover the suite (D-479).
    /// </summary>
    [Fact]
    public void CiWorkflowLeavesTheSmokeCategoryToTheSmokeWorkflow()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/ci.yml");
        string smoke = SmokeSessionTests.SmokeCategory;
        string sweep = SweepScope.SweepCategory;
        Assert.Equal(1, Count(workflow, $"dotnet test WhatYouCarry.slnx --no-build --filter \"Category!={smoke}\""));
        Assert.Contains($"--filter \"Category!={smoke}\"", WorkflowText.JobText(workflow, "macos-arm64"), StringComparison.Ordinal);
        foreach (string leg in new[] { "linux-x64", "windows-x64" })
        {
            Assert.Contains($"--filter \"Category!={smoke}&Category!={sweep}\"", WorkflowText.JobText(workflow, leg), StringComparison.Ordinal);
            Assert.Contains($"--filter \"Category={sweep}\"", WorkflowText.JobText(workflow, $"{leg}-sweeps"), StringComparison.Ordinal);
        }
    }

    /// <summary>The first engine physics or navigation name in a source text, as a message, or null when there is none.</summary>
    private static string? EnginePhysicsDefect(string source, string fileName)
    {
        foreach (string name in EnginePhysicsNames)
        {
            if (source.Contains(name, StringComparison.Ordinal))
            {
                return $"{fileName} names the engine type {name}, and Core owns every collision and every path (G-3).";
            }
        }

        return null;
    }

    /// <summary>The text of one section of a project file: from its header to the next header, or to the end.</summary>
    private static string SectionAfter(string project, int header)
    {
        int next = project.IndexOf("\n[", header + 1, StringComparison.Ordinal);
        return next < 0 ? project[header..] : project[header..next];
    }

    /// <summary>The count of times one text holds another.</summary>
    private static int Count(string text, string needle)
    {
        int count = 0;
        int index = text.IndexOf(needle, StringComparison.Ordinal);
        while (index >= 0)
        {
            count++;
            index = text.IndexOf(needle, index + needle.Length, StringComparison.Ordinal);
        }

        return count;
    }
}
