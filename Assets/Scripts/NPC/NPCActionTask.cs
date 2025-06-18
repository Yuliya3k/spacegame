using System.Collections;
using UnityEngine;

public abstract class NPCActionTask : ScriptableObject
{
    // This flag indicates when the task has finished.
    public bool IsComplete { get; protected set; }

    /// <summary>
    /// Resets this task's state.
    /// </summary>
    public virtual void ResetTask()
    {
        IsComplete = false;
    }

    /// <summary>
    /// Each task must implement its own Execute method.
    /// </summary>
    public abstract IEnumerator Execute(NPCController npc);
}
