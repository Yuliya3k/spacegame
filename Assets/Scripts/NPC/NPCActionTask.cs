using System.Collections;
using UnityEngine;

public abstract class NPCActionTask : ScriptableObject
{
    // This flag indicates when the task has finished.
    public bool IsComplete { get; protected set; }

    // Generic progress tracking fields. Individual tasks can use these
    // however they wish (e.g. elapsed time, waypoint index).
    public float elapsedTime;
    public int waypointIndex;

    /// <summary>
    /// Resets this task's state.
    /// </summary>
    public virtual void ResetTask()
    {
        IsComplete = false;
        elapsedTime = 0f;
        waypointIndex = 0;
    }

    /// <summary>
    /// Each task must implement its own Execute method.
    /// </summary>
    public abstract IEnumerator Execute(NPCController npc);

    // Serialize this task's progress into a state object.
    public virtual NPCActionTaskState GetState()
    {
        return new NPCActionTaskState
        {
            taskName = name,
            elapsedTime = elapsedTime,
            waypointIndex = waypointIndex,
            isComplete = IsComplete
        };
    }

    // Restore this task's progress from a state object.
    public virtual void SetState(NPCActionTaskState state)
    {
        if (state == null) return;
        elapsedTime = state.elapsedTime;
        waypointIndex = state.waypointIndex;
        IsComplete = state.isComplete;
    }
}
