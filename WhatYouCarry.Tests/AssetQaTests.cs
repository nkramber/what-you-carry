using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Tools.AssetQa;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The asset QA gate v1 (D-135, D-149, D-300, D-301, D-302; PR-57 exit tests 1 to 5).</summary>
[Collection(ConsoleCollection.Name)]
public sealed class AssetQaTests
{
    private const string Rig = "models/rig.bbmodel";
    private const string Attack = "models/rig.attack.json";

    /// <summary>PR-57 exit test 1. Two boxes of sibling bones touch at rest and overlap at one keyframe, and the one finding names the keyframe.</summary>
    [Fact]
    public void ClipIsDetected()
    {
        using TemporaryContentDirectory content = new();
        content.Write(Rig, ModelJson.SiblingRig());
        content.Write(Attack, ModelJson.Animation(Rig, "arm_bone", 10, "[0, 0, -90]"));

        IReadOnlyList<AssetFinding> findings = Findings(content);

        AssetFinding finding = Assert.Single(findings);
        Assert.Equal(Rig, finding.Path);
        Assert.Contains("tick 10 of 'models/rig.attack.json'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("'torso'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("'arm'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("D-301", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>The same boxes at rest, with no animation, touch on one face and give no finding: a touch is not a clip (D-301).</summary>
    [Fact]
    public void TouchingBoxesPass()
    {
        using TemporaryContentDirectory content = new();
        content.Write(Rig, ModelJson.SiblingRig());

        Assert.Empty(Findings(content));
    }

    /// <summary>A box that overlaps another at the rest pose is a finding that names the rest pose.</summary>
    [Fact]
    public void ClipAtRestIsDetected()
    {
        using TemporaryContentDirectory content = new();
        string json = ModelJson.Model(
            elements: ModelJson.Cube("torso", "e1", from: "[-4, 0, -4]", to: "[4, 16, 4]") + ", " + ModelJson.Cube("arm", "e2", from: "[3, 8, -2]", to: "[8, 16, 2]"),
            groups: ModelJson.Group("body", "g1") + ", " + ModelJson.Group("torso_bone", "g2") + ", " + ModelJson.Group("arm_bone", "g3"),
            outliner: "[{\"uuid\": \"g1\", \"children\": [{\"uuid\": \"g2\", \"children\": [\"e1\"]}, {\"uuid\": \"g3\", \"children\": [\"e2\"]}]}]");
        content.Write(Rig, json);

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Contains("at the rest pose", finding.Message, StringComparison.Ordinal);
        Assert.Contains("0.0625 meters", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>A box under a bone and a box under the parent of that bone may overlap at any pose: that overlap is the joint (D-301).</summary>
    [Fact]
    public void ParentAndChildBonesAreAJoint()
    {
        using TemporaryContentDirectory content = new();
        content.Write(Rig, ModelJson.JointRig());
        content.Write(Attack, ModelJson.Animation(Rig, "arm_bone", 10, "[0, 0, -90]"));

        Assert.Empty(Findings(content));
    }

    /// <summary>A track that names no bone of the model is one finding on the animation, and the check goes on.</summary>
    [Fact]
    public void UnknownBoneInAnAnimationIsAFinding()
    {
        using TemporaryContentDirectory content = new();
        content.Write(Rig, ModelJson.SiblingRig());
        content.Write(Attack, ModelJson.Animation(Rig, "wing", 10, "[0, 0, -90]"));

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Equal(Attack, finding.Path);
        Assert.Contains("wing", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>A body clip is one finding, and not one per overlay: two body boxes count once, on the pass with no overlay.</summary>
    [Fact]
    public void BodyClipIsReportedOnce()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.BodyModel, ModelJson.SiblingRig());
        content.Write("models/player.attack.json", ModelJson.Animation(AssetPaths.BodyModel, "arm_bone", 10, "[0, 0, -90]"));
        content.Write("models/armor/sleeve.bbmodel", ModelJson.Overlay("arm", "[4, 8, -2]", "[8, 16, 2]"));
        content.Write("models/armor/glove.bbmodel", ModelJson.Overlay("arm", "[4, 8, -2]", "[8, 16, 2]"));

        IReadOnlyList<AssetFinding> findings = Findings(content);

        Assert.Equal(3, findings.Count);
        Assert.Single(findings, finding => finding.Message.Contains("the box 'arm'", StringComparison.Ordinal));
        Assert.Equal(2, findings.Count(finding => finding.Message.Contains("the overlay box 'arm'", StringComparison.Ordinal)));
    }

    /// <summary>An unknown bone in an animation is one finding when overlays exist too, and no pose reads that animation.</summary>
    [Fact]
    public void UnknownBoneIsOneFindingWithOverlays()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.BodyModel, ModelJson.TorsoBody());
        content.Write("models/player.attack.json", ModelJson.Animation(AssetPaths.BodyModel, "wing", 10, "[0, 0, -90]"));
        content.Write("models/armor/chest.bbmodel", ModelJson.Overlay("torso", "[-5, -1, -5]", "[5, 17, 5]"));

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Equal("models/player.attack.json", finding.Path);
        Assert.Contains("'wing'", finding.Message, StringComparison.Ordinal);
        Assert.Contains(AssetPaths.BodyModel, finding.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A model or an animation that does not load is a finding with the loader message, and not a stop (T-2).
    /// A file that is not JSON is a finding of the case check too, because that check reads every file on its own.
    /// </summary>
    [Fact]
    public void AFileThatDoesNotLoadIsAFinding()
    {
        using TemporaryContentDirectory content = new();
        content.Write(Rig, "{\"meta\": ");
        content.Write(Attack, ModelJson.Animation(Rig, "arm_bone", 10, "[0, 0, 0]"));
        content.Write("models/rig.walk.json", "{\"model\": \"models/rig.bbmodel\"}");

        IReadOnlyList<AssetFinding> findings = Findings(content);

        Assert.Equal(3, findings.Count);
        Assert.Equal(2, findings.Count(finding => finding.Path == Rig && finding.Message.Contains("not valid JSON", StringComparison.Ordinal)));
        Assert.Contains(findings, finding => finding.Path == "models/rig.walk.json" && finding.Message.Contains("length", StringComparison.Ordinal));
    }

    /// <summary>
    /// F-119. A model and its animation in a subdirectory of the model directory are checked like a model at the top.
    /// The content loader accepts such a model, and the old gate read the top directory alone, so it passed the clip.
    /// </summary>
    [Fact]
    public void AModelInASubdirectoryIsChecked()
    {
        const string NestedRig = "models/weapons/rig.bbmodel";
        using TemporaryContentDirectory content = new();
        content.Write(NestedRig, ModelJson.SiblingRig());
        content.Write("models/weapons/rig.attack.json", ModelJson.Animation(NestedRig, "arm_bone", 10, "[0, 0, -90]"));

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Equal(NestedRig, finding.Path);
        Assert.Contains("tick 10 of 'models/weapons/rig.attack.json'", finding.Message, StringComparison.Ordinal);
        Assert.Equal(1, AssetQaCommand.Run(["--root", content.Root]));
    }

    /// <summary>The boundary beside F-119: a model under the armor directory, at any depth, stays an overlay and never becomes a body (D-300).</summary>
    [Fact]
    public void AModelUnderTheArmorDirectoryStaysAnOverlay()
    {
        using TemporaryContentDirectory content = new();
        content.Write("models/armor/heads/cap.bbmodel", ModelJson.SiblingRig());
        content.Write(Rig, ModelJson.SiblingRig());

        AssetSet set = AssetSet.Read(content.Content);

        Assert.Equal(Rig, Assert.Single(set.Bodies).Path);
        Assert.Equal("models/armor/heads/cap.bbmodel", Assert.Single(set.Overlays).Path);
    }

    /// <summary>
    /// F-119. An animation whose model is no body of the set is a finding, because no pose reads it. Here the model
    /// is an armor overlay, a file that exists, so the file case check gives no finding on it. The old gate gave none.
    /// </summary>
    [Fact]
    public void AnAnimationOfNoBodyIsAFinding()
    {
        const string Cap = "models/armor/cap.bbmodel";
        using TemporaryContentDirectory content = new();
        content.Write(Cap, ModelJson.SiblingRig());
        content.Write("models/armor/cap.attack.json", ModelJson.Animation(Cap, "arm_bone", 10, "[0, 0, -90]"));

        IReadOnlyList<AssetFinding> findings = Findings(content);

        // The overlay check gives its own finding too: the set has no body for the overlay to cover.
        AssetFinding finding = Assert.Single(findings, finding => finding.Path == "models/armor/cap.attack.json");
        Assert.Contains($"'{Cap}'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("no pose reads the animation", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>F-119. The model value of an animation ends in the model extension. A value with no extension passed the name rule, and no check read the animation.</summary>
    [Fact]
    public void AModelValueWithoutTheExtensionIsAFinding()
    {
        using TemporaryContentDirectory content = new();
        content.Write(Rig, ModelJson.SiblingRig());
        content.Write(Attack, ModelJson.Animation("models/rig", "arm_bone", 10, "[0, 0, -90]"));

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Equal(Attack, finding.Path);
        Assert.Contains("'.bbmodel' file", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-57 exit test 2. An overlay box smaller than the body box of its name is one finding that names the axis.</summary>
    [Fact]
    public void OverlayMustEnclose()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.BodyModel, ModelJson.TorsoBody());
        content.Write("models/armor/chest.bbmodel", ModelJson.Overlay("torso", "[-3, -1, -5]", "[5, 17, 5]"));

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Equal("models/armor/chest.bbmodel", finding.Path);
        Assert.Contains("'torso'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("X axis", finding.Message, StringComparison.Ordinal);
        Assert.Contains("D-300", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>An overlay that encloses its body box passes both the overlay check and the clip check, because the cover is not a clip.</summary>
    [Fact]
    public void AnEnclosingOverlayPasses()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.BodyModel, ModelJson.TorsoBody());
        content.Write("models/armor/chest.bbmodel", ModelJson.Overlay("torso", "[-5, -1, -5]", "[5, 17, 5]"));

        Assert.Empty(Findings(content));
    }

    /// <summary>An overlay box whose name is not a body box name is a finding (D-300).</summary>
    [Fact]
    public void OverlayBoxMustNameABodyBox()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.BodyModel, ModelJson.TorsoBody());
        content.Write("models/armor/chest.bbmodel", ModelJson.Overlay("plate", "[-5, -1, -5]", "[5, 17, 5]"));

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Contains("'plate'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("names no box", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>An overlay clips a body box of another name when it reaches into it at a pose, and the arm of a joint stays exempt.</summary>
    [Fact]
    public void OverlayClipsAnotherBodyBox()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.BodyModel, ModelJson.SiblingRig());
        content.Write("models/armor/chest.bbmodel", ModelJson.Overlay("torso", "[-5, -1, -5]", "[5, 17, 5]"));

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Equal(AssetPaths.BodyModel, finding.Path);
        Assert.Contains("the overlay box 'torso'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("'arm'", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-57 exit test 3. A reference to `Sword.json` where the file is `sword.json` is one finding that names both.</summary>
    [Fact]
    public void WrongCaseIsDetected()
    {
        using TemporaryContentDirectory content = new();
        content.Write("items/sword.json", "{\"id\": \"sword\"}");
        content.Write("items/list.json", "{\"items\": [\"items/Sword.json\"]}");

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Equal("items/list.json", finding.Path);
        Assert.Contains("'items/Sword.json'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("'sword.json'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("case", finding.Message, StringComparison.Ordinal);
        Assert.Contains("D-302", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>A reference with the right case, at any depth of the JSON, passes.</summary>
    [Fact]
    public void RightCaseReferencePasses()
    {
        using TemporaryContentDirectory content = new();
        content.Write("items/sword.json", "{\"id\": \"sword\", \"icon\": {\"file\": \"textures/sword.png\"}}");
        content.Write("textures/sword.png", "not a real image");

        Assert.Empty(Findings(content));
    }

    /// <summary>
    /// A trace spec names screenshots outside the content directory, so the check skips the trace directory alone (D-617).
    /// The same reference in another directory stays a finding.
    /// </summary>
    [Fact]
    public void TraceSpecIsNotRead()
    {
        const string Spec = "{\"images\": {\"front\": \"artifacts/reference/sword/30-unlit-front.png\"}}";
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.TraceDirectory + "sword.json", Spec);
        content.Write("items/sword.json", Spec);

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Equal("items/sword.json", finding.Path);
        Assert.Contains("no directory 'artifacts'", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>A reference to no file, a rooted reference, a backslash, and a dot segment are findings (D-302).</summary>
    [Theory]
    [InlineData("items/axe.json", "no file 'axe.json'")]
    [InlineData("weapons/sword.json", "no directory 'weapons'")]
    [InlineData("/items/sword.json", "never rooted")]
    [InlineData("items\\\\sword.json", "forward slashes")]
    [InlineData("items/../items/sword.json", "no dot segment")]
    public void BadReferenceIsAFinding(string reference, string reason)
    {
        using TemporaryContentDirectory content = new();
        content.Write("items/sword.json", "{\"id\": \"sword\"}");
        content.Write("items/list.json", "{\"items\": [\"" + reference + "\"]}");

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Contains(reason, finding.Message, StringComparison.Ordinal);
    }

    /// <summary>A string is a reference when it ends in a reference extension, in any case, and an id is not one.</summary>
    [Theory]
    [InlineData("models/player.bbmodel", true)]
    [InlineData("Sword.JSON", true)]
    [InlineData("atlas.png", true)]
    [InlineData("working-mine", false)]
    [InlineData("sword.json.bak", false)]
    public void ReferenceEndsInAReferenceExtension(string value, bool expected)
    {
        Assert.Equal(expected, FileCaseCheck.IsReference(value));
    }

    /// <summary>The file case check reads the model files too, so a texture reference in a model counts.</summary>
    [Fact]
    public void ModelFileReferencesCount()
    {
        using TemporaryContentDirectory content = new();
        string model = ModelJson.TorsoBody().Replace("\"textures\": []", "\"textures\": [{\"name\": \"Player.png\"}]", StringComparison.Ordinal);
        content.Write(AssetPaths.BodyModel, model);
        content.Write("player.png", "not a real image");

        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Equal(AssetPaths.BodyModel, finding.Path);
        Assert.Contains("'Player.png'", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>A paint file next to a model is not an animation, so the asset set skips it, and its model reference still counts for the case check (D-508).</summary>
    [Fact]
    public void PaintFileIsNotAnAnimation()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.BodyModel, ModelJson.TorsoBody());
        content.Write("models/player.paint.json", "{\"model\": \"models/Player.bbmodel\", \"boxes\": {}}");

        AssetSet set = AssetSet.Read(content.Content);

        Assert.Empty(set.Animations);
        Assert.Empty(set.LoadFindings);
        AssetFinding finding = Assert.Single(Findings(content));
        Assert.Equal("models/player.paint.json", finding.Path);
        Assert.Contains("'models/Player.bbmodel'", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// PR-57 exit test 4, PR-76 exit test 1, and PR-93 exit test 1. Every model, overlay, and animation of the checkout
    /// passes, and the checkout holds the body, the model of each enemy family, and the model of the Overseer (D-673,
    /// D-698).
    /// </summary>
    [Fact]
    public void RepositoryModelsPass()
    {
        string contentRoot = Path.Combine(RepositoryRoot.Find(), "content");
        AssetSet set = AssetSet.Read(contentRoot);

        IReadOnlyList<AssetFinding> findings = AssetQaCommand.Findings(set, contentRoot);

        Assert.Empty(findings.Select(finding => finding.Line()));
        Assert.Contains(set.Bodies, body => body.Path == AssetPaths.BodyModel);
        Assert.NotEmpty(TestWorld.Content.Enemies);
        foreach (Core.Content.EnemyDefinition family in TestWorld.Content.Enemies)
        {
            Assert.Contains(set.Bodies, body => body.Path == family.Model);
        }

        Assert.Contains(set.Bodies, body => body.Path == TestWorld.Content.Hunter.Model);
    }

    /// <summary>
    /// PR-100 exit test 2. The check reads the swing clip of each enemy: the scavenger swings the clip of the player on
    /// its own model, and the Overseer the clip of its pick (D-741, D-742). <see cref="RepositoryModelsPass"/> holds each
    /// pose free of a clip.
    /// </summary>
    [Fact]
    public void RepositoryEnemiesSwingTheirClips()
    {
        AssetSet set = AssetSet.Read(Path.Combine(RepositoryRoot.Find(), "content"));

        Assert.Equal(
            [
                new WieldedClip("enemies/scavenger.json", "models/scavenger.bbmodel", "models/player.sword-swing.json"),
                new WieldedClip("hunter/overseer.json", "models/overseer.bbmodel", "models/overseer.pick-swing.json"),
            ],
            set.Wielded);
    }

    /// <summary>
    /// D-742 regression. A family swings a clip that names another model with the same bones, and the clip clips the
    /// model of the family alone. The check posed a clip on the model that its file names, so it gave no finding.
    /// </summary>
    [Fact]
    public void TheSwingClipOfAnEnemyIsCheckedOnItsModel()
    {
        using TemporaryContentDirectory content = WieldedContent("models/arm.attack.json");
        content.Write("models/arm.bbmodel", ModelJson.LoneArm());
        content.Write("models/arm.attack.json", ModelJson.Animation("models/arm.bbmodel", "arm_bone", 10, "[0, 0, -90]"));

        AssetFinding finding = Assert.Single(Findings(content));

        Assert.Equal(Rig, finding.Path);
        Assert.Contains("tick 10 of 'models/arm.attack.json', the swing of 'enemies/scavenger.json'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("D-301", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>A swing clip that names the model of the family has its check once, in the loop of the models (D-742).</summary>
    [Fact]
    public void AClipOfTheSameModelIsCheckedOnce()
    {
        using TemporaryContentDirectory content = WieldedContent(Attack);
        content.Write(Attack, ModelJson.Animation(Rig, "arm_bone", 10, "[0, 0, -90]"));

        AssetFinding finding = Assert.Single(Findings(content));

        Assert.Contains("tick 10 of 'models/rig.attack.json'", finding.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("the swing of", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>A family that names a weapon that no weapon file holds is a finding on the family file, because no pose reads its clip (D-742, T-2).</summary>
    [Fact]
    public void AnAbsentWeaponOfAnEnemyIsAFinding()
    {
        using TemporaryContentDirectory content = WieldedContent(Attack);
        content.Write(Attack, ModelJson.Animation(Rig, "arm_bone", 10, "[0, 0, 0]"));
        File.Delete(Path.Combine(content.Content, "weapons", "sword-basic.json"));

        AssetFinding finding = Assert.Single(Findings(content));

        Assert.Equal("enemies/scavenger.json", finding.Path);
        Assert.Contains("'sword-basic'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("D-742", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>A family whose model is not a body of the set is a finding on the family file (D-742, T-2).</summary>
    [Fact]
    public void AnEnemyModelThatIsNoBodyIsAFinding()
    {
        const string Cap = "models/armor/cap.bbmodel";
        using TemporaryContentDirectory content = WieldedContent(Attack, Cap);
        content.Write(Cap, ModelJson.SiblingRig());
        content.Write(Attack, ModelJson.Animation(Rig, "arm_bone", 10, "[0, 0, 0]"));

        IReadOnlyList<AssetFinding> findings = Findings(content);

        // The overlay check gives its own finding too: the set has no body for the overlay to cover.
        AssetFinding finding = Assert.Single(findings, finding => finding.Path == "enemies/scavenger.json");
        Assert.Contains($"'{Cap}'", finding.Message, StringComparison.Ordinal);
        Assert.Contains("no pose reads its swing clip", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A content directory with the rig of <see cref="ModelJson.SiblingRig"/>, the scavenger family of the checkout on a
    /// model, and the sword of the checkout, whose swing clip is the path given. The weapon holds the rig as its model.
    /// </summary>
    private static TemporaryContentDirectory WieldedContent(string clip, string familyModel = Rig)
    {
        string checkout = Path.Combine(RepositoryRoot.Find(), "content");
        string family = File.ReadAllText(Path.Combine(checkout, "enemies", "scavenger.json")).Replace("models/scavenger.bbmodel", familyModel, StringComparison.Ordinal);
        string weapon = File.ReadAllText(Path.Combine(checkout, "weapons", "sword-basic.json"))
            .Replace("models/sword-basic.bbmodel", Rig, StringComparison.Ordinal)
            .Replace("models/player.sword-swing.json", clip, StringComparison.Ordinal);
        Assert.Contains(familyModel, family, StringComparison.Ordinal);
        Assert.Contains(clip, weapon, StringComparison.Ordinal);

        TemporaryContentDirectory content = new();
        content.Write(Rig, ModelJson.SiblingRig());
        content.Write("enemies/scavenger.json", family);
        content.Write("weapons/sword-basic.json", weapon);
        return content;
    }

    /// <summary>A hunter file that names an absent model file is a finding on the hunter file (D-302, D-698).</summary>
    [Fact]
    public void AnAbsentHunterModelIsAFinding()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.BodyModel, ModelJson.TorsoBody());
        content.Write("hunter/overseer.json", "{\"id\": \"overseer\", \"model\": \"models/gone.bbmodel\"}");

        // The file names no weapon, so the read of the swing clip gives a load finding of its own (D-742).
        AssetFinding finding = Assert.Single(Findings(content), finding => finding.Message.Contains("'models/gone.bbmodel'", StringComparison.Ordinal));

        Assert.Equal("hunter/overseer.json", finding.Path);
        Assert.Contains("'models/gone.bbmodel'", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>An enemy family that names an absent model file is a finding on the family file (D-302, D-673).</summary>
    [Fact]
    public void AnAbsentEnemyModelIsAFinding()
    {
        using TemporaryContentDirectory content = new();
        content.Write(AssetPaths.BodyModel, ModelJson.TorsoBody());
        content.Write("enemies/scavenger.json", "{\"id\": \"scavenger\", \"model\": \"models/gone.bbmodel\"}");

        // The file names no weapon, so the read of the swing clip gives a load finding of its own (D-742).
        AssetFinding finding = Assert.Single(Findings(content), finding => finding.Message.Contains("'models/gone.bbmodel'", StringComparison.Ordinal));

        Assert.Equal("enemies/scavenger.json", finding.Path);
        Assert.Contains("'models/gone.bbmodel'", finding.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-57 exit test 5. The asset-qa workflow runs on every pull request and calls the command on the checkout (G-19).</summary>
    [Fact]
    public void AssetQaWorkflowRunsTheCommand()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/asset-qa.yml");

        Assert.Contains("pull_request:", workflow, StringComparison.Ordinal);
        Assert.Contains("-- asset-qa --root .", workflow, StringComparison.Ordinal);
        Assert.Equal("ubuntu-latest", WorkflowText.RunsOnByJob(workflow)["asset-qa"]);
    }

    /// <summary>The command exits 0 on a clean set, 1 on a finding, and 2 on a bad command line or an absent directory.</summary>
    [Fact]
    public void CommandExitCodesFollowTheFindings()
    {
        using TemporaryContentDirectory clean = new();
        clean.Write(Rig, ModelJson.SiblingRig());
        using TemporaryContentDirectory dirty = new();
        dirty.Write(Rig, ModelJson.SiblingRig());
        dirty.Write(Attack, ModelJson.Animation(Rig, "arm_bone", 10, "[0, 0, -90]"));

        Assert.Equal(0, AssetQaCommand.Run(["--root", clean.Root]));
        Assert.Equal(1, AssetQaCommand.Run(["--root", dirty.Root]));
        Assert.Equal(2, AssetQaCommand.Run([]));
        Assert.Equal(2, AssetQaCommand.Run(["--root", clean.Root, "--fast"]));
        Assert.Equal(2, AssetQaCommand.Run(["--root", Path.Combine(clean.Root, "no-such-checkout")]));
    }

    /// <summary>The summary line names the counts, and a content directory with no model directory is an empty set.</summary>
    [Fact]
    public void SummaryNamesTheCounts()
    {
        using TemporaryContentDirectory content = new();
        content.Write("items/sword.json", "{\"id\": \"sword\"}");

        AssetSet set = AssetSet.Read(content.Content);

        Assert.Empty(set.Bodies);
        Assert.Equal("asset-qa: 0 finding(s). 0 model(s), 0 overlay(s), 0 animation(s).", AssetQaCommand.Summary(set, 0));
    }

    /// <summary>An absent content directory is an error that names it, and never an empty set (T-2).</summary>
    [Fact]
    public void AbsentContentDirectoryIsAnError()
    {
        string absent = Path.Combine(Path.GetTempPath(), "wyc-no-such-content-" + Guid.NewGuid().ToString("N"));

        ContextException error = Assert.Throws<ContextException>(() => AssetSet.Read(absent));

        Assert.Contains(absent, error.Message, StringComparison.Ordinal);
    }

    private static IReadOnlyList<AssetFinding> Findings(TemporaryContentDirectory content)
    {
        return AssetQaCommand.Findings(AssetSet.Read(content.Content), content.Content);
    }
}
