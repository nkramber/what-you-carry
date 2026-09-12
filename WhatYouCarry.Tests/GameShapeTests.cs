using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    /// <summary>The project runs the root scene, and the fixed step of the engine is 60 Hz (D-63, D-73).</summary>
    [Fact]
    public void ProjectRunsTheMainSceneAtSixtyTicks()
    {
        string project = RepositoryRoot.ReadFile("WhatYouCarry.Game/project.godot");
        Assert.Contains("run/main_scene=\"res://Main.tscn\"", project, StringComparison.Ordinal);
        Assert.Contains("common/physics_ticks_per_second=60", project, StringComparison.Ordinal);
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

    /// <summary>Every content source skips the model directory, because an animation file there is JSON that Core never reads (D-298).</summary>
    [Fact]
    public void ContentSourcesSkipTheModelDirectory()
    {
        using TemporaryContentDirectory content = new();
        content.Write("floors/a.json", "{}");
        content.Write("models/rig.attack.json", "{}");
        content.Write("models/armor/chest.json", "{}");

        string[] gamePaths = new DirectoryContentSource(content.Content).Read().Select(file => file.Path).ToArray();
        string[] toolPaths = new WhatYouCarry.Tools.BotRunner.DirectoryContentSource(content.Content).Read().Select(file => file.Path).ToArray();

        Assert.Equal(new[] { "floors/a.json" }, gamePaths);
        Assert.Equal(new[] { "floors/a.json" }, toolPaths);
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
        Assert.Equal(3, runsOnByJob.Count);
        Assert.Equal("ubuntu-latest", runsOnByJob["linux-x64"]);
        Assert.Equal("windows-latest", runsOnByJob["windows-x64"]);
        Assert.Contains("macos-arm64-self-hosted", runsOnByJob["macos-arm64"], StringComparison.Ordinal);

        Assert.Contains("GODOT_VERSION: 4.7.2-stable", workflow, StringComparison.Ordinal);
        Assert.Equal(3, Count(workflow, "uses: actions/cache@v4"));
        Assert.Equal(3, Count(workflow, $"{SmokeSessionTests.GodotVariable}:"));
        Assert.Equal(3, Count(workflow, $"--filter \"Category={SmokeSessionTests.SmokeCategory}\""));
    }

    /// <summary>The three CI jobs hold no engine, so they leave the smoke category to the smoke workflow.</summary>
    [Fact]
    public void CiWorkflowLeavesTheSmokeCategoryToTheSmokeWorkflow()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/ci.yml");
        Assert.Equal(3, Count(workflow, $"dotnet test WhatYouCarry.slnx --no-build --filter \"Category!={SmokeSessionTests.SmokeCategory}\""));
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
