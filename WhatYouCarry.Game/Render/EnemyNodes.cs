using System.Collections.Generic;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Entities;
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
/// PR-16 poses no enemy, so every enemy stands in the rest pose of its model. The position comes from the
/// simulation between the two newest ticks, like the position of the player, so a frame rate over the tick rate
/// draws a smooth walk (D-329). The rest pose stands on the feet, so the root of a tree sits at the feet of its
/// enemy, at the lowest corner of its own model.
/// </para>
/// <para>
/// A dead enemy holds its place in the list of the loop, so an owner id never moves (D-322). Its node hides
/// instead, and no node is freed inside a floor. A wave enemy joins the end of the list after expiry, and the tick
/// after it builds its tree (D-410). The Overseer gets its tree on the tick that it spawns (D-415). A descent frees
/// every tree and builds the trees of the next floor.
/// </para>
/// <para>
/// The constructor builds one hidden template tree of each model: each family model, the model of the Overseer,
/// and the sword. Every tree after it shares the meshes of its template, so a descent builds nodes and no
/// mesh. The Deck trace measured 16.7 ms for the meshes of eight enemies in the tick of a descent (F-192).
/// </para>
/// </remarks>
public sealed class EnemyNodes
{
    private readonly Node parent;
    private readonly IReadOnlyDictionary<string, EnemyModel> familyModels;
    private readonly EnemyModel hunterModel;
    private readonly BlockbenchModel sword;
    private readonly Material material;
    private readonly Dictionary<string, ModelNodeTree> familyTemplates = new(System.StringComparer.Ordinal);
    private readonly ModelNodeTree hunterTemplate;
    private readonly ModelNodeTree swordTemplate;
    private readonly List<ModelNodeTree> trees = [];
    private readonly List<CoreVector3> previous = [];
    private readonly List<CoreVector3> current = [];
    private readonly List<float> lowests = [];
    private ModelNodeTree? hunterTree;
    private CoreVector3 hunterPrevious;
    private CoreVector3 hunterCurrent;

    /// <summary>The trees of the enemies of one floor, under one parent node.</summary>
    /// <param name="parent">The node that holds every enemy tree.</param>
    /// <param name="familyModels">The model of each family, by the model path that the family names (D-673).</param>
    /// <param name="hunterModel">The model that the hunter file names, which the Overseer draws with (D-698).</param>
    /// <param name="sword">The weapon model of PR-15, which every enemy holds (D-397).</param>
    /// <param name="material">The one model material of the scene (D-85).</param>
    /// <param name="layout">The texture layout, which places each face of every model (D-505).</param>
    public EnemyNodes(Node parent, IReadOnlyDictionary<string, EnemyModel> familyModels, EnemyModel hunterModel, BlockbenchModel sword, Material material, TextureLayout layout)
    {
        this.parent = parent;
        this.familyModels = familyModels;
        this.hunterModel = hunterModel;
        this.sword = sword;
        this.material = material;

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

    /// <summary>
    /// Builds one tree for each enemy of a floor, and frees the trees of the floor before it. A descent calls it
    /// once with the enemies of the new floor.
    /// </summary>
    public void Rebuild(IReadOnlyList<Enemy> enemies)
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
        if (this.hunterTree is not null)
        {
            this.parent.RemoveChild(this.hunterTree.Root);
            this.hunterTree.Root.QueueFree();
            this.hunterTree = null;
        }

        this.Grow(enemies);
    }

    /// <summary>
    /// Reads the position of every enemy and of the Overseer after a tick, so the next frames draw between the two
    /// newest ones. A wave enemy or an Overseer with no tree yet gets one, at its position of this tick.
    /// </summary>
    public void AfterTick(IReadOnlyList<Enemy> enemies, Hunter? hunter)
    {
        for (int index = 0; index < this.trees.Count && index < enemies.Count; index++)
        {
            this.previous[index] = this.current[index];
            this.current[index] = enemies[index].Body.Position;
        }

        this.Grow(enemies);
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
    }

    /// <summary>Builds one tree for each enemy past the last tree, in list order, at its position now.</summary>
    private void Grow(IReadOnlyList<Enemy> enemies)
    {
        for (int index = this.trees.Count; index < enemies.Count; index++)
        {
            EnemyModel model = EnemyModels.Of(this.familyModels, enemies[index].Definition);
            this.trees.Add(this.BuildTree(this.familyTemplates[model.Path], model.Model));
            this.lowests.Add(model.Lowest);
            this.previous.Add(enemies[index].Body.Position);
            this.current.Add(enemies[index].Body.Position);
        }
    }

    /// <summary>One tree of a model with the sword in the weapon slot, under the parent (D-397, D-673). The tree and the sword share the meshes of their templates (F-192).</summary>
    private ModelNodeTree BuildTree(ModelNodeTree template, BlockbenchModel model)
    {
        ModelNodeTree tree = ModelNodes.Share(template, model, this.material);
        ModelNodes.Hold(tree, EquipmentSlots.Weapon, ModelNodes.Share(this.swordTemplate, this.sword, this.material).Root);
        this.parent.AddChild(tree.Root);
        return tree;
    }

    /// <summary>Places every tree for one frame: the position between the two newest ticks, the yaw of the enemy, and a hidden node for a dead one.</summary>
    /// <param name="enemies">The enemies of the floor, in the order that <see cref="Rebuild"/> read.</param>
    /// <param name="hunter">The Overseer of the floor, or null before expiry.</param>
    /// <param name="fraction">The part of the tick that the frame stands at, from zero to one.</param>
    public void Draw(IReadOnlyList<Enemy> enemies, Hunter? hunter, float fraction)
    {
        if (this.hunterTree is not null && hunter is not null)
        {
            CoreVector3 hunterFeet = RenderInterpolation.Between(this.hunterPrevious, this.hunterCurrent, fraction);
            this.hunterTree.Root.Position = RenderInterpolation.ToGodot(hunterFeet) + new Vector3(0.0f, -this.hunterModel.Lowest, 0.0f);
            this.hunterTree.Root.RotationDegrees = new Vector3(0.0f, hunter.Yaw / 100.0f, 0.0f);
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
            CoreVector3 feet = RenderInterpolation.Between(this.previous[index], this.current[index], fraction);
            tree.Root.Position = RenderInterpolation.ToGodot(feet) + new Vector3(0.0f, -this.lowests[index], 0.0f);

            // The yaw turns counterclockwise seen from above, as a positive rotation about Y does (D-234).
            tree.Root.RotationDegrees = new Vector3(0.0f, enemy.Yaw / 100.0f, 0.0f);
        }
    }
}
