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
        Assert.DoesNotContain("Main.cs _Notification", callbacks);
    }

    /// <summary>
    /// F-177. Each mesh goes to its node through <c>ArrayMeshBuilder.BuildInto</c>, which disposes the managed wrapper of
    /// the mesh at once, and no other code builds a mesh. A wrapper that lived to the exit went to the .NET finalizer after
    /// the engine shut down. Under load, 3 of 100 smoke sessions then crashed at exit with signal 11 or abort 134, and 0
    /// of 200 crashed with the dispose.
    /// </summary>
    [Fact]
    public void EachMeshWrapperGoesWhenItsNodeTakesTheMesh()
    {
        string builder = RepositoryRoot.ReadFile("WhatYouCarry.Game/Render/ArrayMeshBuilder.cs");
        int into = builder.IndexOf("public static void BuildInto(MeshInstance3D node, MeshData data)", StringComparison.Ordinal);
        int take = builder.IndexOf("node.Mesh = mesh;", into, StringComparison.Ordinal);
        int dispose = builder.IndexOf("mesh.Dispose();", into, StringComparison.Ordinal);
        Assert.True(into >= 0 && take > into && dispose > take, "BuildInto gives the node the mesh, then disposes the wrapper.");
        Assert.Contains("private static ArrayMesh Build(MeshData data)", builder, StringComparison.Ordinal);
        Assert.Contains("using Godot.Collections.Array arrays = [];", builder, StringComparison.Ordinal);

        int callers = 0;
        foreach (string file in GameStringScan.SourceFiles(RepositoryRoot.Find()))
        {
            string text = File.ReadAllText(file);
            Assert.DoesNotContain("new ArrayMesh", Path.GetFileName(file) == "ArrayMeshBuilder.cs" ? string.Empty : text, StringComparison.Ordinal);
            Assert.DoesNotContain("Mesh = ArrayMeshBuilder", text, StringComparison.Ordinal);
            callers += text.Split("ArrayMeshBuilder.BuildInto(").Length - 1;
        }

        Assert.Equal(3, callers);
    }

    /// <summary>
    /// F-161. The boot turns off the automatic quit of the engine, and the close request of the root window reaches the
    /// end path, which writes the end line and quits through <c>Quit</c>. The old session let the engine quit on a close
    /// with no end line, no frame log, and no release of the sounds. The handler is a signal and not an override of
    /// <c>_Notification</c>: the first form of the fix took every engine notification into managed code, the shutdown
    /// ones too. The abort at exit that one smoke session then showed came from the mesh wrappers of F-177.
    /// </summary>
    [Fact]
    public void TheCloseOfTheWindowEndsTheSession()
    {
        string main = RepositoryRoot.ReadFile("WhatYouCarry.Game/Main.cs");
        int ready = main.IndexOf("public override void _Ready()", StringComparison.Ordinal);
        int boot = main.IndexOf("this.Boot();", ready, StringComparison.Ordinal);
        int manual = main.IndexOf("this.GetTree().AutoAcceptQuit = false;", ready, StringComparison.Ordinal);
        int signal = main.IndexOf("this.GetTree().Root.CloseRequested += this.OnCloseRequested;", ready, StringComparison.Ordinal);
        Assert.True(ready >= 0 && manual > ready && signal > manual && signal < boot, "_Ready turns off the automatic quit and connects the close request before the boot.");
        Assert.DoesNotContain("override void _Notification", main, StringComparison.Ordinal);

        int handler = main.IndexOf("private void OnCloseRequested()", StringComparison.Ordinal);
        int call = main.IndexOf("this.CloseWindow();", handler, StringComparison.Ordinal);
        int guard = main.IndexOf("catch (Exception error)", handler, StringComparison.Ordinal);
        Assert.True(handler >= 0 && call > handler && guard > call, "The handler sends the close to CloseWindow inside a catch of every exception.");

        int body = main.IndexOf("private void CloseWindow()", StringComparison.Ordinal);
        int end = main.IndexOf("WindowClosedMessage, fields);", body, StringComparison.Ordinal);
        int quit = main.IndexOf("this.Quit(", body, StringComparison.Ordinal);
        Assert.True(body >= 0 && end > body && quit > end, "CloseWindow writes the end line and then quits.");
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
    /// F-122. The quit of the session reaches the quit of the engine in every case. The engine call sits in the finally
    /// block of a try statement that holds the rest of the body, and the write of the frame log catches every exception
    /// with no filter. A write error of another kind left the old method before the engine call, and the session never
    /// ended.
    /// </summary>
    [Fact]
    public void TheQuitAlwaysReachesTheEngine()
    {
        const string EngineQuit = "GetTree().Quit(";
        Microsoft.CodeAnalysis.SyntaxNode tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(RepositoryRoot.ReadFile("WhatYouCarry.Game/Main.cs")).GetRoot();
        Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax quit = Assert.Single(tree.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>(), method => method.Identifier.Text == "Quit");
        Assert.NotNull(quit.Body);

        Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax outer = Assert.IsType<Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax>(quit.Body.Statements[^1]);
        Assert.NotNull(outer.Finally);
        Assert.Contains(EngineQuit, outer.Finally.Block.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(EngineQuit, outer.Block.ToString(), StringComparison.Ordinal);

        Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax write = Assert.Single(
            outer.Block.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax>(),
            attempt => attempt.Block.ToString().Contains("File.WriteAllText(", StringComparison.Ordinal));
        Assert.Contains(write.Catches, clause => clause.Filter is null && clause.Declaration?.Type.ToString() == "Exception");
    }

    /// <summary>
    /// F-122. The relative outputs of the Game commands land in the project directory that the path flag of the engine
    /// sets, so the ignore file covers each one there, and not in every directory.
    /// </summary>
    [Fact]
    public void TheRelativeOutputsOfTheGameCommandsAreIgnored()
    {
        string[] lines = RepositoryRoot.ReadFile(".gitignore").Split('\n');
        foreach (string output in new[] { "frames.txt", "sheet.png", "hud.png" })
        {
            Assert.Contains(lines, line => line.Trim() == "/WhatYouCarry.Game/" + output);
            Assert.DoesNotContain(lines, line => line.Trim() == output);
        }
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
        Assert.Equal(WorkflowText.HostedMacosLabel, runsOnByJob["macos-arm64"]);

        Assert.Contains("GODOT_VERSION: 4.7.2-stable", workflow, StringComparison.Ordinal);
        Assert.Equal(3, Count(workflow, $"uses: {ActionDecisionTests.Cache}"));
        Assert.Equal(3, Count(workflow, $"{SmokeSessionTests.GodotVariable}:"));
        Assert.Equal(3, Count(workflow, $"--filter \"Category={SmokeSessionTests.SmokeCategory}\""));
    }

    /// <summary>
    /// F-159. Each download of the engine fails on an HTTP error and checks the SHA-512 that the workflow pins, and the
    /// cache key holds that value, so a cache hit is a checked binary (D-626). The old steps checked no hash, and curl
    /// wrote an error page to the zip.
    /// </summary>
    [Fact]
    public void EachEngineDownloadChecksItsPinnedHash()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/smoke.yml");
        (string Job, string Variable, string Check)[] legs =
        [
            ("linux-x64", "GODOT_SHA512_LINUX", "echo \"${GODOT_SHA512_LINUX}  godot.zip\" | sha512sum -c -"),
            ("windows-x64", "GODOT_SHA512_WINDOWS", "if ($hash -ne $env:GODOT_SHA512_WINDOWS) { throw"),
            ("macos-arm64", "GODOT_SHA512_MACOS", "echo \"${GODOT_SHA512_MACOS}  godot.zip\" | shasum -a 512 -c -"),
        ];
        foreach ((string job, string variable, string check) in legs)
        {
            string value = EnvValue(workflow, variable);
            Assert.Matches("^[0-9a-f]{128}$", value);
            string text = WorkflowText.JobText(workflow, job);
            Assert.Contains($"-${{{{ env.{variable} }}}}", text, StringComparison.Ordinal);
            int download = text.IndexOf("godot.zip", StringComparison.Ordinal);
            int hashCheck = text.IndexOf(check, StringComparison.Ordinal);
            int unpack = text.IndexOf(job == "windows-x64" ? "Expand-Archive" : "unzip -q godot.zip", StringComparison.Ordinal);
            Assert.True(download >= 0 && hashCheck > download && unpack > hashCheck, $"The job '{job}' checks the hash after the download and before the unpack.");
            Assert.DoesNotContain("curl -sSL -o", text, StringComparison.Ordinal);
        }
    }

    /// <summary>The value of one variable of the top <c>env</c> block of a workflow.</summary>
    private static string EnvValue(string workflow, string name)
    {
        string prefix = $"  {name}: ";
        foreach (string line in workflow.Split('\n'))
        {
            if (line.StartsWith(prefix, StringComparison.Ordinal))
            {
                return line[prefix.Length..].Trim();
            }
        }

        Assert.Fail($"The workflow holds no env value '{name}'.");
        return string.Empty;
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
