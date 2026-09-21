using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Ui;

/// <summary>One damage number: the owner of the hit entity, the health it lost, whether the player lost it, and its age in seconds.</summary>
public sealed record DamageNumber(int Owner, int Amount, bool Taken, float Age);

/// <summary>
/// The damage numbers that live (D-36, D-444). Each hit that takes health gets a number: white for an enemy, and red
/// for the player. A number lives 0.8 seconds, rises, and fades.
/// </summary>
/// <remarks>
/// <para>
/// A number stands above the screen box of the hit entity and never over it (F-24). The box holds every point of the
/// entity on the screen, so a number outside the box never covers the silhouette. When the space above the box is
/// off the screen, the number stands under the box. When neither place fits, or the box is off the screen, the number
/// hides for that frame.
/// </para>
/// <para>
/// Two numbers on one entity stack, and the newest stands nearest to the box. The rise moves a number away from the
/// box, so it never moves a number onto the box. The size of a number comes from a width per digit of the default
/// font at 18 pixels (D-441).
/// </para>
/// </remarks>
public sealed class DamageNumbers
{
    /// <summary>The lifetime of a number, in seconds (D-444).</summary>
    public const float LifetimeSeconds = 0.8f;

    /// <summary>The font size of a number at 800p (D-444).</summary>
    public const int FontPixels = 18;

    /// <summary>The height of the box of a number: one line at <see cref="FontPixels"/>.</summary>
    public const float High = 26.0f;

    /// <summary>The width of one digit of the default font at <see cref="FontPixels"/>, with space to spare.</summary>
    public const float DigitWide = 12.0f;

    /// <summary>The space at each side of the digits.</summary>
    public const float Padding = 6.0f;

    /// <summary>The space between the entity box and the nearest number.</summary>
    public const float Gap = 6.0f;

    /// <summary>The distance that a number rises over its lifetime.</summary>
    public const float RisePixels = 30.0f;

    /// <summary>The message of the error for a number with no health lost.</summary>
    public const string EmptyHitMessage = "A damage number shows a loss of one health or more.";

    /// <summary>The message of the error for a screen box of no points.</summary>
    public const string NoPointsMessage = "A screen box holds one point or more.";

    private const string AmountField = "amount";
    private const string OwnerField = "owner";

    private readonly List<DamageNumber> live = [];

    /// <summary>The numbers that live, oldest first.</summary>
    public IReadOnlyList<DamageNumber> Live => this.live;

    /// <summary>Adds a number of age zero for one hit.</summary>
    /// <exception cref="ContextException">The amount is below one.</exception>
    public void Add(int owner, int amount, bool taken)
    {
        if (amount < 1)
        {
            ContextException error = new(EmptyHitMessage);
            error.AddContext(AmountField, amount.ToString(CultureInfo.InvariantCulture));
            error.AddContext(OwnerField, owner.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        this.live.Add(new DamageNumber(owner, amount, taken, 0.0f));
    }

    /// <summary>Ages every number by the time of one frame, and removes each number at the end of its lifetime.</summary>
    public void Advance(float seconds)
    {
        for (int index = this.live.Count - 1; index >= 0; index--)
        {
            DamageNumber older = this.live[index] with { Age = this.live[index].Age + seconds };
            if (older.Age >= LifetimeSeconds)
            {
                this.live.RemoveAt(index);
            }
            else
            {
                this.live[index] = older;
            }
        }
    }

    /// <summary>Removes every number, as a descent does, because the entities of the old floor go.</summary>
    public void Clear()
    {
        this.live.Clear();
    }

    /// <summary>The count of numbers on the same owner that came after the number at one index of <see cref="Live"/>.</summary>
    public int NewerOnOwner(int index)
    {
        int newer = 0;
        for (int later = index + 1; later < this.live.Count; later++)
        {
            if (this.live[later].Owner == this.live[index].Owner)
            {
                newer++;
            }
        }

        return newer;
    }

    /// <summary>The opacity of a number of one age: one at the start, and zero at the end of the lifetime.</summary>
    public static float Opacity(float age)
    {
        return Math.Clamp(1.0f - (age / LifetimeSeconds), 0.0f, 1.0f);
    }

    /// <summary>The smallest box that holds every point.</summary>
    /// <exception cref="ContextException">The list holds no point.</exception>
    public static Rect2 BoxOf(IReadOnlyList<Vector2> points)
    {
        if (points.Count == 0)
        {
            throw new ContextException(NoPointsMessage);
        }

        Rect2 box = new(points[0], Vector2.Zero);
        foreach (Vector2 point in points)
        {
            box = box.Expand(point);
        }

        return box;
    }

    /// <summary>
    /// The box of one number on the screen, or null when it has no place in this frame. The number stands above the
    /// entity box, or under it when the space above is off the screen, and never over it (F-24).
    /// </summary>
    /// <param name="entity">The screen box of the hit entity.</param>
    /// <param name="screen">The layout size of the screen.</param>
    /// <param name="amount">The health lost, which sets the width.</param>
    /// <param name="age">The age of the number, which sets the rise.</param>
    /// <param name="newer">The count of newer numbers on the same entity, which stand nearer to the box.</param>
    public static Rect2? Place(Rect2 entity, Vector2 screen, int amount, float age, int newer)
    {
        Rect2 whole = new(Vector2.Zero, screen);
        if (!whole.Intersects(entity))
        {
            return null;
        }

        float wide = (Padding * 2.0f) + (DigitWide * amount.ToString(CultureInfo.InvariantCulture).Length);
        float left = Math.Clamp(entity.GetCenter().X - (wide / 2.0f), 0.0f, Math.Max(0.0f, screen.X - wide));
        float away = Gap + (RisePixels * Math.Clamp(age / LifetimeSeconds, 0.0f, 1.0f)) + (High * newer);

        Rect2 above = new(left, entity.Position.Y - away - High, wide, High);
        if (whole.Encloses(above))
        {
            return above;
        }

        Rect2 under = new(left, entity.End.Y + away, wide, High);
        if (whole.Encloses(under))
        {
            return under;
        }

        return null;
    }
}
