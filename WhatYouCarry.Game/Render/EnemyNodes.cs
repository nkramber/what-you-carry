using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Game.Animation;
using WhatYouCarry.Game.Models;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The model of every enemy of a floor and of the Overseer. Each enemy draws with the model that its family names
/// (D-673), and the Overseer with the model that the hunter file names (D-698). Each one holds the sword of PR-15 (D-397),
/// at the position of the simulation.
/// </summary>
/// <remarks>
/// <para>
/// Each enemy plays the swing clip of its weapon at the swing tick of Core, and stands in the rest pose between
/// swings (D-739, D-741). The swing tick changes only on a tick, so <see cref="AfterTick"/> sets the pose. The
/// position comes from the simulation between the two newest ticks, like the position of the player, so a frame rate
/// over the tick rate draws a smooth walk (D-329). The rest pose stands on the feet, so the root of a tree sits at the
/// feet of its enemy, at the lowest corner of its own model. A swing clip turns no bone that lowers that corner, and a
/// test holds it.
/// </para>
/// <para>
/// A dead enemy holds its place in the list of the loop, so an owner id never moves (D-322). Its node hides
/// instead, and no node is freed inside a floor. A wave enemy joins the end of the list after expiry, and the tick
/// after it builds its tree (D-410). The Overseer gets its tree on the tick that it spawns (D-415). A descent frees
/// every tree and shows the trees of the next floor.
/// </para>
/// <para>
/// The plan of the next floor names its enemy spawns, and the loop places one enemy for each spawn in spawn order.
/// So when the worker offers the plan, <see cref="BuildSome"/> builds the trees of those spawns hidden, one each
/// frame, and the descent shows them. The Deck trace of seed 2 measured 3.2 to 3.5 milliseconds for the seven trees
/// in the tick of a descent, and a collection that their allocations started put that tick over the budget of D-635
/// (F-193). A descent before the upload ends builds the rest of the trees in that tick.
/// </para>
/// <para>
/// The constructor builds one hidden template tree of each model: each family model, the model of the Overseer,
/// and the sword. Every tree after it shares the meshes of its template, so a descent builds nodes and no
/// mesh. The Deck trace measured 16.7 ms for the meshes of eight enemies in the tick of a descent (F-192).
/// </para>
/// <para>
/// A model between the camera and the player, or near the camera, draws with the fade material (D-721). Each tree
/// keeps its fade state, so a tree changes its material only on the frame that its fade changes.
/// </para>
/// </remarks>
public sealed class EnemyNodes
{
    /// <summary>
    /// The count of hidden trees of the next floor that one frame builds. One tree took 0.45 to 0.7 milliseconds on
    /// the Deck (F-193), and a floor offers its plan long before its descent. A later change needs a measurement on
    /// the Deck (D-109).
    /// </summary>
    public const int TreesPerFrame = 1;

    private const string StagedMismatchMessage = "The loop placed an enemy that differs from the spawn of the plan that its tree was built for.";
    private const string IndexField = "index";
    private const string EnemyCountField = "enemies";
    private const string SpawnFamilyField = "spawnFamily";
    private const string EnemyFamilyField = "enemyFamily";

    private readonly Node parent;
    private readonly IReadOnlyDictionary<string, EnemyModel> familyModels;
    private readonly EnemyModel hunterModel;
    private readonly EnemyClips clips;
    private readonly BlockbenchModel sword;
    private readonly Material material;
    private readonly Material fadedMaterial;
    private readonly Dictionary<string, ModelNodeTree> familyTemplates = new(System.StringComparer.Ordinal);
    private readonly ModelNodeTree hunterTemplate;
    private readonly ModelNodeTree swordTemplate;
    private readonly List<ModelNodeTree> trees = [];
    private readonly List<CoreVector3> previous = [];
    private readonly List<CoreVector3> current = [];
    private readonly List<float> lowests = [];
    private readonly List<bool> faded = [];
    private ModelNodeTree? hunterTree;
    private bool hunterFaded;
    private CoreVector3 hunterPrevious;
    private CoreVector3 hunterCurrent;
    private FloorPlan? stagedPlan;
    private readonly List<ModelNodeTree> stagedTrees = [];

    /// <summary>The trees of the enemies of one floor, under one parent node.</summary>
    /// <param name="parent">The node that holds every enemy tree.</param>
    /// <param name="familyModels">The model of each family, by the model path that the family names (D-673).</param>
    /// <param name="hunterModel">The model that the hunter file names, which the Overseer draws with (D-698).</param>
    /// <param name="clips">The swing clip of each family and of the Overseer (D-741).</param>
    /// <param name="sword">The weapon model of PR-15, which every enemy holds (D-397).</param>
    /// <param name="material">The one model material of the scene (D-85).</param>
    /// <param name="fadedMaterial">The material of a faded model (D-721).</param>
    /// <param name="layout">The texture layout, which places each face of every model (D-505).</param>
    public EnemyNodes(Node parent, IReadOnlyDictionary<string, EnemyModel> familyModels, EnemyModel hunterModel, EnemyClips clips, BlockbenchModel sword, Material material, Material fadedMaterial, TextureLayout layout)
    {
        this.parent = parent;
        this.familyModels = familyModels;
        this.hunterModel = hunterModel;
        this.clips = clips;
        this.sword = sword;
        this.material = material;
        this.fadedMaterial = fadedMaterial;

        // The templates hang from one hidden node under the parent, so they never draw, and the engine frees them
        // with the scene at the exit.
        Node3D templates = new() { Visible = false };
        parent.AddChild(templates);
        foreach (KeyValuePair<string, EnemyModel> family in familyModels)
        {
            ModelNodeTree template = ModelNodes.Build(family.Value.Model, material, layout);
            templates.AddChild(template.Root);
            this.familyTemplates.Add(family.Key, template);
        }

        this.hunterTemplate = ModelNodes.Build(hunterModel.Model, material, layout);
        templates.AddChild(this.hunterTemplate.Root);
        this.swordTemplate = ModelNodes.Build(sword, material, layout);
        templates.AddChild(this.swordTemplate.Root);
    }

    /// <summary>The count of enemy trees that stand under the parent now.</summary>
    public int Count => this.trees.Count;

    /// <summary>Answers whether the tree of the Overseer stands under the parent now.</summary>
    public bool HasHunter => this.hunterTree is not null;

    /// <summary>The count of trees that the last <see cref="Rebuild"/> took hidden from the staged plan.</summary>
    public int LastStagedTrees { get; private set; }

    /// <summary>The count of trees that the last <see cref="Rebuild"/> built.</summary>
    public int LastBuiltTrees { get; private set; }

    /// <summary>
    /// Takes the plan of the next floor that the worker offers. <see cref="BuildSome"/> then builds the hidden trees
    /// of its enemy spawns. The hidden trees of a plan before it go.
    /// </summary>
    public void Stage(FloorPlan plan)
    {
        this.FreeStaged();
        this.stagedPlan = plan;
    }

    /// <summary>Builds up to <see cref="TreesPerFrame"/> hidden trees of the staged plan, in spawn order. Call it once each frame.</summary>
    public void BuildSome()
    {
        if (this.stagedPlan is null)
        {
            return;
        }

        IReadOnlyList<EnemySpawn> spawns = this.stagedPlan.EnemySpawns;
        int end = Math.Min(this.stagedTrees.Count + TreesPerFrame, spawns.Count);
        for (int index = this.stagedTrees.Count; index < end; index++)
        {
            EnemyModel model = EnemyModels.Of(this.familyModels, spawns[index].Family);
            ModelNodeTree tree = this.BuildTree(this.familyTemplates[model.Path], model.Model);
            tree.Root.Visible = false;
            this.stagedTrees.Add(tree);
        }
    }

    /// <summary>
    /// Frees the trees of the floor before, and gives one tree to each enemy of a floor. The boot calls it with the
    /// first floor, and a descent calls it once with the new floor. When the plan is the staged plan, the hidden trees
    /// of its spawns go to the first enemies, and the rest of the enemies get a new tree.
    /// </summary>
    /// <exception cref="ContextException">The plan is the staged plan, and an enemy differs from the spawn that its tree was built for.</exception>
    public void Rebuild(FloorPlan plan, IReadOnlyList<Enemy> enemies)
    {
        foreach (ModelNodeTree tree in this.trees)
        {
            this.parent.RemoveChild(tree.Root);
            tree.Root.QueueFree();
        }

        this.trees.Clear();
        this.previous.Clear();
        this.current.Clear();
        this.lowests.Clear();
        this.faded.Clear();
        if (this.hunterTree is not null)
        {
            this.parent.RemoveChild(this.hunterTree.Root);
            this.hunterTree.Root.QueueFree();
            this.hunterTree = null;
            this.hunterFaded = false;
        }

        if (ReferenceEquals(plan, this.stagedPlan))
        {
            this.TakeStaged(enemies);
        }

        this.LastStagedTrees = this.trees.Count;
        this.FreeStaged();
        this.Grow(enemies);
        this.LastBuiltTrees = this.trees.Count - this.LastStagedTrees;
    }

    /// <summary>
    /// Reads the position of every enemy and of the Overseer after a tick, so the next frames draw between the two
    /// newest ones. A wave enemy or an Overseer with no tree yet gets one, at its position of this tick. Each tree then
    /// takes the pose of the swing tick of its enemy (D-739).
    /// </summary>
    /// <exception cref="ContextException">The clips hold no swing clip of the family of an enemy.</exception>
    public void AfterTick(IReadOnlyList<Enemy> enemies, Hunter? hunter)
    {
        for (int index = 0; index < this.trees.Count && index < enemies.Count; index++)
        {
            this.previous[index] = this.current[index];
            this.current[index] = enemies[index].Body.Position;
        }

        this.Grow(enemies);
        for (int index = 0; index < this.trees.Count && index < enemies.Count; index++)
        {
            Enemy enemy = enemies[index];
            ModelNodes.Pose(this.trees[index], EnemyPose.Rotations(enemy.SwingTick, this.clips.Of(enemy.Definition)));
        }

        if (hunter is null)
        {
            return;
        }

        if (this.hunterTree is null)
        {
            this.hunterTree = this.BuildTree(this.hunterTemplate, this.hunterModel.Model);
            this.hunterCurrent = hunter.Body.Position;
        }

        this.hunterPrevious = this.hunterCurrent;
        this.hunterCurrent = hunter.Body.Position;
        ModelNodes.Pose(this.hunterTree, EnemyPose.Rotations(hunter.SwingTick, this.clips.Hunter));
    }

    /// <summary>Builds one tree for each enemy past the last tree, in list order, at its position now.</summary>
    private void Grow(IReadOnlyList<Enemy> enemies)
    {
        for (int index = this.trees.Count; index < enemies.Count; index++)
        {
            EnemyModel model = EnemyModels.Of(this.familyModels, enemies[index].Definition);
            this.trees.Add(this.BuildTree(this.familyTemplates[model.Path], model.Model));
            this.lowests.Add(model.Lowest);
            this.faded.Add(false);
            this.previous.Add(enemies[index].Body.Position);
            this.current.Add(enemies[index].Body.Position);
        }
    }

    /// <summary>
    /// Gives the hidden trees of the staged plan to the first enemies, in spawn order, at the position of each enemy
    /// now. Each enemy has the family of its spawn, because the loop places one enemy for each spawn in spawn order.
    /// </summary>
    /// <exception cref="ContextException">An enemy is absent or has another family than the spawn of its tree.</exception>
    private void TakeStaged(IReadOnlyList<Enemy> enemies)
    {
        IReadOnlyList<EnemySpawn> spawns = this.stagedPlan!.EnemySpawns;
        for (int index = 0; index < this.stagedTrees.Count; index++)
        {
            if (index >= enemies.Count || !ReferenceEquals(enemies[index].Definition, spawns[index].Family))
            {
                ContextException error = new(StagedMismatchMessage);
                error.AddContext(IndexField, index.ToString(CultureInfo.InvariantCulture));
                error.AddContext(EnemyCountField, enemies.Count.ToString(CultureInfo.InvariantCulture));
                error.AddContext(SpawnFamilyField, spawns[index].Family.Id);
                if (index < enemies.Count)
                {
                    error.AddContext(EnemyFamilyField, enemies[index].Definition.Id);
                }

                throw error;
            }

            EnemyModel model = EnemyModels.Of(this.familyModels, enemies[index].Definition);
            this.trees.Add(this.stagedTrees[index]);
            this.lowests.Add(model.Lowest);
            this.faded.Add(false);
            this.previous.Add(enemies[index].Body.Position);
            this.current.Add(enemies[index].Body.Position);
        }

        this.stagedTrees.Clear();
    }

    /// <summary>Frees the hidden trees of the staged plan that no enemy took, and forgets the plan.</summary>
    private void FreeStaged()
    {
        foreach (ModelNodeTree tree in this.stagedTrees)
        {
            this.parent.RemoveChild(tree.Root);
            tree.Root.QueueFree();
        }

        this.stagedTrees.Clear();
        this.stagedPlan = null;
    }

    /// <summary>One tree of a model with the sword in the weapon slot, under the parent (D-397, D-673). The tree and the sword share the meshes of their templates (F-192).</summary>
    private ModelNodeTree BuildTree(ModelNodeTree template, BlockbenchModel model)
    {
        ModelNodeTree tree = ModelNodes.Share(template, model, this.material);
        ModelNodes.Hold(tree, EquipmentSlots.Weapon, ModelNodes.Share(this.swordTemplate, this.sword, this.material).Root);
        this.parent.AddChild(tree.Root);
        return tree;
    }

    /// <summary>
    /// Places every tree for one frame: the position between the two newest ticks, the yaw of the enemy, the fade of
    /// D-721, and a hidden node for a dead one.
    /// </summary>
    /// <param name="enemies">The enemies of the floor, in the order that <see cref="Rebuild"/> read.</param>
    /// <param name="hunter">The Overseer of the floor, or null before expiry.</param>
    /// <param name="fraction">The part of the tick that the frame stands at, from zero to one.</param>
    /// <param name="camera">The drawn camera of the frame (D-720).</param>
    /// <param name="fadeEnd">The player end of the segment of the wall fade (D-292).</param>
    public void Draw(IReadOnlyList<Enemy> enemies, Hunter? hunter, float fraction, Vector3 camera, Vector3 fadeEnd)
    {
        if (this.hunterTree is not null && hunter is not null)
        {
            Vector3 hunterFeet = RenderInterpolation.ToGodot(RenderInterpolation.Between(this.hunterPrevious, this.hunterCurrent, fraction));
            this.hunterTree.Root.Position = hunterFeet + new Vector3(0.0f, -this.hunterModel.Lowest, 0.0f);
            this.hunterTree.Root.RotationDegrees = new Vector3(0.0f, hunter.Yaw / 100.0f, 0.0f);
            bool hunterFades = ModelFade.Fades(camera, fadeEnd, ModelFade.BodyBox(hunterFeet));
            if (hunterFades != this.hunterFaded)
            {
                ModelFade.SetMaterial(this.hunterTree.Root, hunterFades ? this.fadedMaterial : this.material);
                this.hunterFaded = hunterFades;
            }
        }

        for (int index = 0; index < this.trees.Count && index < enemies.Count; index++)
        {
            Enemy enemy = enemies[index];
            ModelNodeTree tree = this.trees[index];
            if (enemy.IsDead)
            {
                tree.Root.Visible = false;
                continue;
            }

            tree.Root.Visible = true;
            Vector3 feet = RenderInterpolation.ToGodot(RenderInterpolation.Between(this.previous[index], this.current[index], fraction));
            tree.Root.Position = feet + new Vector3(0.0f, -this.lowests[index], 0.0f);

            // The yaw turns counterclockwise seen from above, as a positive rotation about Y does (D-234).
            tree.Root.RotationDegrees = new Vector3(0.0f, enemy.Yaw / 100.0f, 0.0f);

            bool fades = ModelFade.Fades(camera, fadeEnd, ModelFade.BodyBox(feet));
            if (fades != this.faded[index])
            {
                ModelFade.SetMaterial(tree.Root, fades ? this.fadedMaterial : this.material);
                this.faded[index] = fades;
            }
        }
    }
}
