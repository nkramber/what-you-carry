using System.Collections.Generic;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Game.Models;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The model of every enemy of a floor (D-401). Each one draws with the body model of PR-13 and the sword of
/// PR-15, at the position of the simulation, and PR-62 gives the family a model of its own.
/// </summary>
/// <remarks>
/// <para>
/// PR-16 poses no enemy, so every enemy stands in the rest pose of the body model. The position comes from the
/// simulation between the two newest ticks, like the position of the player, so a frame rate over the tick rate
/// draws a smooth walk (D-329). The rest pose stands on the feet, so the root of a tree sits at the feet of its
/// enemy.
/// </para>
/// <para>
/// A dead enemy holds its place in the list of the loop, so an owner id never moves (D-322). Its node hides
/// instead, and no node is built or freed inside a floor. A descent builds the trees of the next floor.
/// </para>
/// </remarks>
public sealed class EnemyNodes
{
    private readonly Node parent;
    private readonly BlockbenchModel body;
    private readonly BlockbenchModel sword;
    private readonly Material material;
    private readonly List<ModelNodeTree> trees = [];
    private readonly List<CoreVector3> previous = [];
    private readonly List<CoreVector3> current = [];
    private readonly float lowest;

    /// <summary>The trees of the enemies of one floor, under one parent node.</summary>
    /// <param name="parent">The node that holds every enemy tree.</param>
    /// <param name="body">The body model of PR-13, which every enemy draws with (D-401).</param>
    /// <param name="sword">The weapon model of PR-15, which every enemy holds (D-397).</param>
    /// <param name="material">The one model material of the scene (D-85).</param>
    /// <param name="lowest">The lowest box corner of the rest pose, which the root offset reads.</param>
    public EnemyNodes(Node parent, BlockbenchModel body, BlockbenchModel sword, Material material, float lowest)
    {
        this.parent = parent;
        this.body = body;
        this.sword = sword;
        this.material = material;
        this.lowest = lowest;
    }

    /// <summary>The count of trees that stand under the parent now.</summary>
    public int Count => this.trees.Count;

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
        foreach (Enemy enemy in enemies)
        {
            ModelNodeTree tree = ModelNodes.Build(this.body, this.material);
            ModelNodes.Hold(tree, EquipmentSlots.Weapon, ModelNodes.Build(this.sword, this.material).Root);
            this.trees.Add(tree);
            this.previous.Add(enemy.Body.Position);
            this.current.Add(enemy.Body.Position);
            this.parent.AddChild(tree.Root);
        }
    }

    /// <summary>Reads the position of every enemy after a tick, so the next frames draw between the two newest ones.</summary>
    public void AfterTick(IReadOnlyList<Enemy> enemies)
    {
        for (int index = 0; index < this.trees.Count && index < enemies.Count; index++)
        {
            this.previous[index] = this.current[index];
            this.current[index] = enemies[index].Body.Position;
        }
    }

    /// <summary>Places every tree for one frame: the position between the two newest ticks, the yaw of the enemy, and a hidden node for a dead one.</summary>
    /// <param name="enemies">The enemies of the floor, in the order that <see cref="Rebuild"/> read.</param>
    /// <param name="fraction">The part of the tick that the frame stands at, from zero to one.</param>
    public void Draw(IReadOnlyList<Enemy> enemies, float fraction)
    {
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
            tree.Root.Position = RenderInterpolation.ToGodot(feet) + new Vector3(0.0f, -this.lowest, 0.0f);

            // The yaw turns counterclockwise seen from above, as a positive rotation about Y does (D-234).
            tree.Root.RotationDegrees = new Vector3(0.0f, enemy.Yaw / 100.0f, 0.0f);
        }
    }
}
