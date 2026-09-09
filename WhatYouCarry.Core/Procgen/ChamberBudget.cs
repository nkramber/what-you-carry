using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// The draw of chamber kinds for one floor (D-167, D-255). The sum of the weights lands inside the budget
/// window, one tenth of the budget on each side, and the count lands inside the room count range of the floor.
/// </summary>
/// <remarks>
/// <para>
/// Each draw takes one kind at random among the kinds that can still complete: the sum stays at or under the
/// window top, the lightest kinds can still fill the count up to the minimum without a sum past the top, and the
/// heaviest kinds can still lift the sum to the window bottom before the count reaches the maximum. A draw that
/// lands inside the window stops, or it goes on with a coin flip while the count allows, so a floor holds more
/// chambers on some seeds than on others.
/// </para>
/// <para>
/// The two checks are bounds and not a proof, so a draw can run out of kinds. It then starts over, up to
/// <see cref="MaxAttempts"/> times. A floor whose budget no draw can fill is an error that names the floor
/// and its window, never a floor with the wrong count (T-2).
/// </para>
/// </remarks>
public static class ChamberBudget
{
    /// <summary>The count of draws before the budget is an error.</summary>
    public const int MaxAttempts = 1000;

    /// <summary>The window bottom of a budget: the budget less one tenth (D-167).</summary>
    public static long WindowBottom(long budget)
    {
        return budget - (budget / 10);
    }

    /// <summary>The window top of a budget: the budget plus one tenth (D-167).</summary>
    public static long WindowTop(long budget)
    {
        return budget + (budget / 10);
    }

    /// <summary>The kinds of the chambers of one floor, in dig order.</summary>
    /// <exception cref="ContextException">The kinds are empty, or no draw of <see cref="MaxAttempts"/> fills the window inside the count range.</exception>
    public static IReadOnlyList<ChamberKind> Draw(Rng rng, FloorTemplate floor, IReadOnlyList<ChamberKind> kinds)
    {
        if (kinds.Count == 0)
        {
            ContextException empty = new($"The content set holds no chamber kind, and the floor template '{floor.Id}' needs at least one to fill its budget (D-255).");
            empty.AddContext("floorTemplate", floor.Id);
            throw empty;
        }

        long bottom = WindowBottom(floor.DifficultyBudget);
        long top = WindowTop(floor.DifficultyBudget);
        long lightest = kinds[0].Weight;
        long heaviest = kinds[0].Weight;
        foreach (ChamberKind kind in kinds)
        {
            if (kind.Weight < lightest)
            {
                lightest = kind.Weight;
            }

            if (kind.Weight > heaviest)
            {
                heaviest = kind.Weight;
            }
        }

        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            List<ChamberKind> drawn = [];
            long sum = 0;
            while (true)
            {
                bool inside = drawn.Count >= floor.RoomCountMin && sum >= bottom && sum <= top;
                if (inside && (drawn.Count >= floor.RoomCountMax || rng.NextInt(2) == 0))
                {
                    return drawn;
                }

                if (drawn.Count >= floor.RoomCountMax)
                {
                    break;
                }

                List<ChamberKind> candidates = Candidates(kinds, floor, sum, drawn.Count, bottom, top, lightest, heaviest);
                if (candidates.Count == 0)
                {
                    break;
                }

                ChamberKind pick = candidates[rng.NextInt(candidates.Count)];
                drawn.Add(pick);
                sum += pick.Weight;
            }
        }

        ContextException error = new($"No draw of {MaxAttempts} fills the budget window {bottom} to {top} of the floor template '{floor.Id}' with {floor.RoomCountMin} to {floor.RoomCountMax} chambers from the {kinds.Count} chamber kinds (D-167).");
        error.AddContext("floorTemplate", floor.Id);
        error.AddContext("windowBottom", bottom.ToString(CultureInfo.InvariantCulture));
        error.AddContext("windowTop", top.ToString(CultureInfo.InvariantCulture));
        error.AddContext("roomCountMin", floor.RoomCountMin.ToString(CultureInfo.InvariantCulture));
        error.AddContext("roomCountMax", floor.RoomCountMax.ToString(CultureInfo.InvariantCulture));
        throw error;
    }

    /// <summary>The kinds that the next draw can take without a sum past the top, and with a completion still in reach.</summary>
    private static List<ChamberKind> Candidates(IReadOnlyList<ChamberKind> kinds, FloorTemplate floor, long sum, int count, long bottom, long top, long lightest, long heaviest)
    {
        List<ChamberKind> candidates = [];
        foreach (ChamberKind kind in kinds)
        {
            long next = sum + kind.Weight;
            if (next > top)
            {
                continue;
            }

            // The draws still needed to reach the minimum count add at least the lightest weight each.
            long belowMinimum = floor.RoomCountMin - count - 1;
            if (belowMinimum > 0 && next + (belowMinimum * lightest) > top)
            {
                continue;
            }

            // The draws still allowed before the maximum count add at most the heaviest weight each.
            long room = floor.RoomCountMax - count - 1;
            if (next < bottom && next + (room * heaviest) < bottom)
            {
                continue;
            }

            candidates.Add(kind);
        }

        return candidates;
    }
}
