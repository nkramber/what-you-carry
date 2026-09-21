using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Simulation;

/// <summary>
/// The waves of one floor after expiry (D-45, D-410, D-418, D-424): which posts take a wave enemy on a tick, and
/// how many spawns the cap or the sight of the player skips.
/// </summary>
/// <remarks>
/// <para>
/// Wave n comes <c>n</c> intervals after expiry and holds n enemies. A wave that is larger than the room under the
/// cap of living wave enemies spawns up to the cap, and the wave number still rises (D-424).
/// </para>
/// <para>
/// A wave enemy spawns at a post of the floor plan, an enemy spawn cell of D-398, with the family of that post. The
/// posts go in spawn order from a cursor that goes on from wave to wave, and a post that the player can see is
/// skipped. When no post is out of sight, the spawns of the wave that remain are skipped, and the loop logs the
/// count (D-418). A floor with no post, such as a floor that no family covers yet, skips every wave (D-410).
/// </para>
/// <para>
/// The choice draws no random number, so the Enemy stream of D-159 stays unused (D-418).
/// </para>
/// </remarks>
public sealed class Escalation
{
    private readonly long intervalTicks;
    private readonly long cap;

    /// <summary>The waves of a floor of one template, before expiry.</summary>
    /// <exception cref="ContextException">The interval of the template is below one second.</exception>
    public Escalation(FloorTemplate template)
    {
        if (template.WaveIntervalSeconds < 1)
        {
            ContextException error = new($"The wave interval of the floor template '{template.Id}' is {template.WaveIntervalSeconds} seconds, and two waves cannot spawn on one tick (D-410).");
            error.AddContext("floorTemplate", template.Id);
            error.AddContext("waveIntervalSeconds", template.WaveIntervalSeconds.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        this.intervalTicks = template.WaveIntervalSeconds * SimulationLoop.TicksPerSecond;
        this.cap = template.WaveCap;
    }

    /// <summary>The count of waves that came due so far (D-424).</summary>
    public int Waves { get; private set; }

    /// <summary>The index of the post that the next spawn tries first (D-418).</summary>
    public int Cursor { get; private set; }

    /// <summary>
    /// The spawns of one tick. A wave comes due on each whole interval after expiry. The result is empty on any
    /// other tick.
    /// </summary>
    /// <param name="grid">The grid of the floor.</param>
    /// <param name="ticksAfterExpiry">The ticks after the expiry of the floor timer.</param>
    /// <param name="livingWaveEnemies">The count of wave enemies of the floor that still have health.</param>
    /// <param name="posts">The enemy spawns of the floor plan, in spawn order (D-398).</param>
    /// <param name="playerFeet">The feet center of the player, in meters. A post that the player sees is skipped.</param>
    public WaveSpawns Step(VoxelGrid grid, long ticksAfterExpiry, int livingWaveEnemies, IReadOnlyList<EnemySpawn> posts, Vector3 playerFeet)
    {
        if (ticksAfterExpiry < 1 || ticksAfterExpiry % this.intervalTicks != 0)
        {
            return new WaveSpawns(0, [], 0);
        }

        this.Waves++;
        long room = this.cap - livingWaveEnemies;
        if (room < 0)
        {
            room = 0;
        }

        int count = this.Waves;
        if (room < count)
        {
            count = (int)room;
        }

        List<EnemySpawn> chosen = [];
        for (int spawn = 0; spawn < count; spawn++)
        {
            if (!this.TryNextPost(grid, posts, playerFeet, out EnemySpawn post))
            {
                break;
            }

            chosen.Add(post);
        }

        return new WaveSpawns(this.Waves, chosen, count - chosen.Count);
    }

    /// <summary>Folds the waves into the hash, in the declared order (D-160): the wave count and the cursor.</summary>
    public void AddTo(ref StateHash hash)
    {
        hash.Add(this.Waves);
        hash.Add(this.Cursor);
    }

    /// <summary>
    /// The next post from the cursor that the player cannot see, and the cursor moves past it. A walk over every
    /// post that finds none gives false, and the cursor stays.
    /// </summary>
    private bool TryNextPost(VoxelGrid grid, IReadOnlyList<EnemySpawn> posts, Vector3 playerFeet, out EnemySpawn post)
    {
        for (int step = 0; step < posts.Count; step++)
        {
            int index = (this.Cursor + step) % posts.Count;
            EnemySpawn candidate = posts[index];
            Vector3 feet = new(candidate.Cell.X + 0.5f, candidate.Cell.Y + 1.0f, candidate.Cell.Z + 0.5f);
            if (PathWalk.Sees(grid, playerFeet, feet))
            {
                continue;
            }

            this.Cursor = (index + 1) % posts.Count;
            post = candidate;
            return true;
        }

        post = default;
        return false;
    }
}

/// <summary>The spawns of one wave on one tick: the wave number, the posts that take an enemy, and the count of spawns that no post out of sight could take (D-418).</summary>
/// <param name="Wave">The wave number, from one. Zero means no wave came due on the tick.</param>
/// <param name="Posts">The posts that take a wave enemy, in spawn order.</param>
/// <param name="Skipped">The spawns of the wave that found no post out of the sight of the player.</param>
public sealed record WaveSpawns(int Wave, IReadOnlyList<EnemySpawn> Posts, int Skipped);
