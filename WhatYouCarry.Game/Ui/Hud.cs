using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Game.Render;
using Aabb = WhatYouCarry.Core.Physics.Aabb;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Ui;

/// <summary>
/// The HUD of a run (D-36, D-90): the health, the timer, the damage numbers, the boss bar placeholder, and the
/// stairwell prompt, as Control nodes built in C#. Every text comes from the string table through
/// <see cref="HudText"/> (G-8), in the default font of the engine (D-441).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="HudLayout"/> places each element on the base of 1280 by 800, and the canvas layer takes the one scale
/// of <see cref="UiScale"/> (D-446). The health shows at the bottom left (D-442), the timer at the top center with a
/// paused mark while the stairwell pauses it (D-443), and the prompt at the lower center with the buttons of the last
/// device (D-447, D-448). The boss bar hides until a boss exists, and the screenshot fixture shows it (D-445).
/// </para>
/// <para>
/// After each tick, the HUD reads the drop of health of the player and of each enemy, and each drop gets a damage
/// number (D-444). The box of a number reads the entity box of the last two ticks, grown by a margin for the arms
/// and the weapon of the model, so it holds the silhouette at each point between the two ticks (F-24). A descent
/// clears the numbers, because the enemies of the old floor go.
/// </para>
/// </remarks>
public sealed class Hud
{
    /// <summary>The growth of the entity box on each side, in meters, so the box holds the model and not the body box alone.</summary>
    public const float SilhouetteMargin = 0.3f;

    /// <summary>The opacity of the timer while the stairwell pauses the countdown (D-443).</summary>
    public const float PausedOpacity = 0.5f;

    /// <summary>The thickness of the dark outline of each text, so a text reads on a bright floor.</summary>
    public const int OutlinePixels = 4;

    /// <summary>The message of the error for a bar whose most health is below one.</summary>
    public const string NoMostHealthMessage = "A health bar has a most health of one or more.";

    private const string MostField = "most";

    private static readonly Color TextColor = new(1.0f, 1.0f, 1.0f);
    private static readonly Color TakenColor = new(1.0f, 0.3f, 0.3f);
    private static readonly Color OutlineColor = new(0.0f, 0.0f, 0.0f);
    private static readonly Color BarBackColor = new(0.0f, 0.0f, 0.0f, 0.6f);
    private static readonly Color HealthFillColor = new(0.8f, 0.15f, 0.15f);
    private static readonly Color BossFillColor = new(0.55f, 0.1f, 0.1f);

    private readonly Strings strings;
    private readonly Label healthText;
    private readonly ColorRect healthBack;
    private readonly ColorRect healthFill;
    private readonly Label timer;
    private readonly Label paused;
    private readonly Label bossName;
    private readonly Label bossNumber;
    private readonly ColorRect bossBack;
    private readonly ColorRect bossFill;
    private readonly Label descend;
    private readonly Label ascend;
    private readonly LabelSettings dealtFont;
    private readonly LabelSettings takenFont;
    private readonly List<Label> numberLabels = [];
    private readonly DamageNumbers numbers = new();
    private readonly Dictionary<int, Aabb> lastBoxes = [];
    private readonly Dictionary<int, Aabb> boxes = [];
    private readonly List<int> enemyHealth = [];
    private int playerHealth = -1;
    private int floor = -1;
    private float bossFraction;

    private Hud(Strings strings, CanvasLayer layer)
    {
        this.strings = strings;
        this.Layer = layer;
        LabelSettings text = Font(HudLayout.TextFontPixels, TextColor);
        this.healthText = this.AddLabel(text, HorizontalAlignment.Left);
        this.healthBack = this.AddRect(BarBackColor);
        this.healthFill = this.AddRect(HealthFillColor);
        this.timer = this.AddLabel(Font(HudLayout.TimerFontPixels, TextColor), HorizontalAlignment.Center);
        this.paused = this.AddLabel(Font(HudLayout.PausedFontPixels, TextColor), HorizontalAlignment.Center);
        this.paused.Text = strings.Get(HudText.PausedId);
        this.bossName = this.AddLabel(text, HorizontalAlignment.Left);
        this.bossNumber = this.AddLabel(text, HorizontalAlignment.Right);
        this.bossBack = this.AddRect(BarBackColor);
        this.bossFill = this.AddRect(BossFillColor);
        LabelSettings prompt = Font(HudLayout.PromptFontPixels, TextColor);
        this.descend = this.AddLabel(prompt, HorizontalAlignment.Center);
        this.ascend = this.AddLabel(prompt, HorizontalAlignment.Center);
        this.dealtFont = Font(DamageNumbers.FontPixels, TextColor);
        this.takenFont = Font(DamageNumbers.FontPixels, TakenColor);
        this.ShowBoss(false);
    }

    /// <summary>The canvas layer that holds every element of the HUD.</summary>
    public CanvasLayer Layer { get; }

    /// <summary>The damage numbers that live, for the tests of the smoke session and the screenshot fixture.</summary>
    public DamageNumbers Numbers => this.numbers;

    /// <summary>The HUD with every text from the string table, in a new canvas layer under one parent.</summary>
    /// <exception cref="ContextException">The string table holds no text for an id of the HUD.</exception>
    public static Hud Build(Strings strings, Node parent)
    {
        CanvasLayer layer = new();
        parent.AddChild(layer);
        return new Hud(strings, layer);
    }

    /// <summary>Shows the boss bar with a name, the health, and the most health, or hides it (D-445).</summary>
    public void ShowBoss(string name, int health, int most)
    {
        this.bossName.Text = name;
        this.bossNumber.Text = HudText.Health(this.strings, health, most);
        this.bossFraction = Fraction(health, most);
        this.ShowBoss(true);
    }

    /// <summary>Hides the boss bar (D-445).</summary>
    public void HideBoss()
    {
        this.ShowBoss(false);
    }

    /// <summary>The placeholder name of the boss bar (D-445).</summary>
    public string BossPlaceholderName()
    {
        return this.strings.Get(HudText.BossPlaceholderId);
    }

    /// <summary>
    /// Reads the state after one tick: a damage number for each drop of health of the player and of each enemy, and
    /// the entity boxes of the last two ticks. A new floor clears the numbers and starts the health of its enemies.
    /// </summary>
    public void AfterTick(SimulationLoop loop)
    {
        bool newFloor = loop.Floor != this.floor;
        this.floor = loop.Floor;
        if (newFloor)
        {
            this.numbers.Clear();
            this.enemyHealth.Clear();
            this.boxes.Clear();
        }

        int health = loop.Player.Health;
        if (this.playerHealth > health)
        {
            this.numbers.Add(SimulationLoop.PlayerOwner, this.playerHealth - health, true);
        }

        this.playerHealth = health;

        for (int index = 0; index < loop.Enemies.Count; index++)
        {
            Enemy enemy = loop.Enemies[index];
            if (index == this.enemyHealth.Count)
            {
                this.enemyHealth.Add(enemy.Health);
            }
            else if (this.enemyHealth[index] > enemy.Health)
            {
                this.numbers.Add(enemy.Owner, this.enemyHealth[index] - enemy.Health, false);
                this.enemyHealth[index] = enemy.Health;
            }
        }

        this.lastBoxes.Clear();
        foreach (KeyValuePair<int, Aabb> box in this.boxes)
        {
            this.lastBoxes[box.Key] = box.Value;
        }

        this.boxes.Clear();
        this.boxes[SimulationLoop.PlayerOwner] = loop.Body.Box;
        foreach (Enemy enemy in loop.Enemies)
        {
            this.boxes[enemy.Owner] = enemy.Body.Box;
        }
    }

    /// <summary>Adds one damage number with no tick, for the screenshot fixture.</summary>
    public void AddNumber(int owner, int amount, bool taken)
    {
        this.numbers.Add(owner, amount, taken);
    }

    /// <summary>
    /// Draws one frame: the scale of the screen, the health, the timer and its paused mark, the prompt with the buttons
    /// of the last device, and each damage number above its entity on the screen of the camera.
    /// </summary>
    public void Draw(HudState state, Camera3D camera, float delta)
    {
        Vector2 screen = this.Layer.GetViewport().GetVisibleRect().Size;
        float scale = UiScale.Factor(screen);
        Vector2 size = screen / scale;
        this.Layer.Scale = new Vector2(scale, scale);
        HudLayout layout = HudLayout.For(size);

        this.healthText.Text = HudText.Health(this.strings, state.Health, state.MostHealth);
        Place(this.healthText, layout.HealthText);
        Place(this.healthBack, layout.HealthBar);
        PlaceFill(this.healthFill, layout.HealthBar, Fraction(state.Health, state.MostHealth));

        this.timer.Text = HudText.Timer(this.strings, state.RemainingTicks);
        this.timer.Modulate = new Color(1.0f, 1.0f, 1.0f, state.Paused ? PausedOpacity : 1.0f);
        Place(this.timer, layout.Timer);
        this.paused.Visible = state.Paused;
        Place(this.paused, layout.Paused);

        Place(this.bossName, layout.BossName);
        Place(this.bossNumber, layout.BossName);
        Place(this.bossBack, layout.BossBar);
        PlaceFill(this.bossFill, layout.BossBar, this.bossFraction);

        this.descend.Visible = state.PromptOpen;
        this.ascend.Visible = state.PromptOpen;
        this.descend.Text = HudText.Descend(this.strings, state.Controller);
        this.ascend.Text = HudText.Ascend(this.strings, state.Controller);
        float line = layout.Prompt.Size.Y / 2.0f;
        Place(this.descend, new Rect2(layout.Prompt.Position, layout.Prompt.Size.X, line));
        Place(this.ascend, new Rect2(layout.Prompt.Position.X, layout.Prompt.Position.Y + line, layout.Prompt.Size.X, line));

        this.numbers.Advance(delta);
        this.DrawNumbers(camera, scale, size);
    }

    /// <summary>Places a label for each damage number that lives, and hides each label that has no number or no place.</summary>
    private void DrawNumbers(Camera3D camera, float scale, Vector2 size)
    {
        IReadOnlyList<DamageNumber> live = this.numbers.Live;
        while (this.numberLabels.Count < live.Count)
        {
            this.numberLabels.Add(this.AddLabel(this.dealtFont, HorizontalAlignment.Center));
        }

        for (int index = 0; index < this.numberLabels.Count; index++)
        {
            Label label = this.numberLabels[index];
            Rect2? place = index < live.Count ? this.PlaceNumber(camera, scale, size, index) : null;
            if (place is not Rect2 box)
            {
                label.Visible = false;
                continue;
            }

            DamageNumber number = live[index];
            label.Text = number.Amount.ToString(CultureInfo.InvariantCulture);
            label.LabelSettings = number.Taken ? this.takenFont : this.dealtFont;
            label.Modulate = new Color(1.0f, 1.0f, 1.0f, DamageNumbers.Opacity(number.Age));
            label.Visible = true;
            Place(label, box);
        }
    }

    /// <summary>The box of one damage number on the layout, or null when its entity has no box or stands behind the camera.</summary>
    private Rect2? PlaceNumber(Camera3D camera, float scale, Vector2 size, int index)
    {
        DamageNumber number = this.numbers.Live[index];
        if (!this.boxes.TryGetValue(number.Owner, out Aabb now))
        {
            return null;
        }

        Aabb before = this.lastBoxes.TryGetValue(number.Owner, out Aabb last) ? last : now;
        CoreVector3 min = new(
            MathF.Min(now.Min.X, before.Min.X) - SilhouetteMargin,
            MathF.Min(now.Min.Y, before.Min.Y) - SilhouetteMargin,
            MathF.Min(now.Min.Z, before.Min.Z) - SilhouetteMargin);
        CoreVector3 max = new(
            MathF.Max(now.Max.X, before.Max.X) + SilhouetteMargin,
            MathF.Max(now.Max.Y, before.Max.Y) + SilhouetteMargin,
            MathF.Max(now.Max.Z, before.Max.Z) + SilhouetteMargin);

        List<Vector2> points = [];
        for (int corner = 0; corner < 8; corner++)
        {
            CoreVector3 point = new(
                (corner & 1) == 0 ? min.X : max.X,
                (corner & 2) == 0 ? min.Y : max.Y,
                (corner & 4) == 0 ? min.Z : max.Z);
            Godot.Vector3 world = RenderInterpolation.ToGodot(point);
            if (camera.IsPositionBehind(world))
            {
                return null;
            }

            points.Add(camera.UnprojectPosition(world) / scale);
        }

        return DamageNumbers.Place(DamageNumbers.BoxOf(points), size, number.Amount, number.Age, this.numbers.NewerOnOwner(index));
    }

    /// <summary>Shows or hides every node of the boss bar.</summary>
    private void ShowBoss(bool shown)
    {
        this.bossName.Visible = shown;
        this.bossNumber.Visible = shown;
        this.bossBack.Visible = shown;
        this.bossFill.Visible = shown;
    }

    /// <summary>A new label under the layer, with one font and one alignment.</summary>
    private Label AddLabel(LabelSettings font, HorizontalAlignment alignment)
    {
        Label label = new()
        {
            LabelSettings = font,
            HorizontalAlignment = alignment,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        this.Layer.AddChild(label);
        return label;
    }

    /// <summary>A new colored box under the layer.</summary>
    private ColorRect AddRect(Color color)
    {
        ColorRect rect = new() { Color = color, MouseFilter = Control.MouseFilterEnum.Ignore };
        this.Layer.AddChild(rect);
        return rect;
    }

    /// <summary>The font of one size and one color, with the dark outline, in the default font of the engine (D-441).</summary>
    private static LabelSettings Font(int pixels, Color color)
    {
        return new LabelSettings { FontSize = pixels, FontColor = color, OutlineSize = OutlinePixels, OutlineColor = OutlineColor };
    }

    /// <summary>The part of the most health that the health is, from zero to one.</summary>
    /// <exception cref="ContextException">The most health is below one, so a bar has no scale.</exception>
    private static float Fraction(int health, int most)
    {
        if (most < 1)
        {
            ContextException error = new(NoMostHealthMessage);
            error.AddContext(MostField, most.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        return Math.Clamp((float)health / most, 0.0f, 1.0f);
    }

    /// <summary>Gives a control the place and the size of one box of the layout.</summary>
    private static void Place(Control control, Rect2 box)
    {
        control.Position = box.Position;
        control.Size = box.Size;
    }

    /// <summary>Gives the fill of a bar the left part of the bar box, as wide as the fraction.</summary>
    private static void PlaceFill(Control fill, Rect2 bar, float fraction)
    {
        Place(fill, new Rect2(bar.Position, bar.Size.X * fraction, bar.Size.Y));
    }
}
