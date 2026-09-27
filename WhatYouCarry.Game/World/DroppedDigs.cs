using System.Collections.Generic;
using System.Threading.Tasks;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The dig tasks that a descent passed while they still ran (F-152). The loop dug the floor itself, so the result of
/// such a task is never read, but its failure still reaches the log. <see cref="ChunkSwap"/> checks the list before
/// each tick, so no failure of the worker is lost (T-2).
/// </summary>
/// <remarks>It calls no engine API, so a test drives it with plain tasks.</remarks>
public sealed class DroppedDigs
{
    private readonly List<Task> tasks = [];
    private readonly List<int> floors = [];

    /// <summary>The count of tasks that did not end yet or were not checked since they ended.</summary>
    public int Count => this.tasks.Count;

    /// <summary>Keeps a task that digs one floor, when it did not end with a result. A task that ended with a result needs no check.</summary>
    public void Add(Task task, int floor)
    {
        if (task.IsCompletedSuccessfully)
        {
            return;
        }

        this.tasks.Add(task);
        this.floors.Add(floor);
    }

    /// <summary>Forgets each task that ended with a result, and keeps each task that still runs.</summary>
    /// <exception cref="WhatYouCarry.Core.Logging.ContextException">A task failed, or the engine cancelled it. The error names the seed, the floor, and the cause.</exception>
    public void Check(ulong seed)
    {
        for (int index = this.tasks.Count - 1; index >= 0; index--)
        {
            Task task = this.tasks[index];
            if (!task.IsCompleted)
            {
                continue;
            }

            if (!task.IsCompletedSuccessfully)
            {
                throw ChunkSwap.DigError(task, seed, this.floors[index]);
            }

            this.tasks.RemoveAt(index);
            this.floors.RemoveAt(index);
        }
    }
}
