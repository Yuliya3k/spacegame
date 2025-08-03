using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Conditional Sequence Task", menuName = "NPC Tasks/Conditional Sequence Task")]
public class NPCConditionalSequenceTask : NPCActionTask
{
    [Header("Condition Settings")]
    public string statName; // The stat to check (e.g., "currentFullness")
    public ConditionOperator conditionOperator;
    [Tooltip("If false, the targetValue field is used. If true, the target is computed dynamically.")]
    public bool useDynamicTarget = false;
    [Tooltip("Fixed target value (used if useDynamicTarget is false).")]
    public float targetValue;

    [Tooltip("Name of the stat to use for dynamic target (used if useDynamicTarget is true).")]
    public string targetStatName;
    public float multiplier = 1f;
    public float offset = 0f;

    [Header("Task Sequence")]
    [Tooltip("The list of tasks to execute in sequence if the condition is met.")]
    public List<NPCActionTask> sequenceTasks;

    public override IEnumerator Execute(NPCController npc)
    {
        // Get the stat value to check from the NPC's CharacterStats.
        float statValue = npc.characterStats.GetStat(statName);

        // Determine the effective target value.
        float effectiveTarget = useDynamicTarget
            ? multiplier * npc.characterStats.GetStat(targetStatName) + offset
            : targetValue;

        bool conditionMet = false;
        switch (conditionOperator)
        {
            case ConditionOperator.Greater:
                conditionMet = statValue > effectiveTarget;
                break;
            case ConditionOperator.GreaterOrEqual:
                conditionMet = statValue >= effectiveTarget;
                break;
            case ConditionOperator.Less:
                conditionMet = statValue < effectiveTarget;
                break;
            case ConditionOperator.LessOrEqual:
                conditionMet = statValue <= effectiveTarget;
                break;
            case ConditionOperator.Equal:
                conditionMet = Mathf.Approximately(statValue, effectiveTarget);
                break;
            case ConditionOperator.NotEqual:
                conditionMet = !Mathf.Approximately(statValue, effectiveTarget);
                break;
        }

        Debug.Log($"[Conditional Sequence Task] Checking condition: {statName} ({statValue}) {conditionOperator} {effectiveTarget} = {conditionMet}");

        if (conditionMet)
        {
            if (sequenceTasks != null && sequenceTasks.Count > 0)
            {
                for (int i = waypointIndex; i < sequenceTasks.Count; i++)
                {
                    var task = sequenceTasks[i];
                    if (task != null)
                    {
                        Debug.Log($"[Conditional Sequence Task] Executing task: {task.name}");
                        yield return npc.StartCoroutine(task.Execute(npc));
                    }
                    else
                    {
                        Debug.LogWarning("[Conditional Sequence Task] A task in the sequence is null.");
                    }
                    waypointIndex = i + 1;
                }
            }
            else
            {
                Debug.LogWarning("[Conditional Sequence Task] No sequence tasks defined.");
            }
        }
        else
        {
            Debug.Log("[Conditional Sequence Task] Condition not met. Skipping sequence.");
        }

        IsComplete = true;
        yield break;
    }

    public override void ResetTask()
    {
        base.ResetTask();
        waypointIndex = 0;
        if (sequenceTasks != null)
        {
            foreach (var task in sequenceTasks)
            {
                if (task != null)
                    task.ResetTask();
            }
        }
    }

    public override NPCActionTaskState GetState()
    {
        NPCActionTaskState state = base.GetState();
        if (sequenceTasks != null)
        {
            state.subStates = new List<NPCActionTaskState>();
            foreach (var task in sequenceTasks)
            {
                if (task != null)
                    state.subStates.Add(task.GetState());
            }
        }
        return state;
    }

    public override void SetState(NPCActionTaskState state)
    {
        base.SetState(state);
        if (sequenceTasks != null && state != null && state.subStates != null)
        {
            for (int i = 0; i < sequenceTasks.Count && i < state.subStates.Count; i++)
            {
                sequenceTasks[i].SetState(state.subStates[i]);
            }
        }
    }

}
