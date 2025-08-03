using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Conditional Task", menuName = "NPC Tasks/Conditional Task")]
public class NPCConditionalTask : NPCActionTask
{
    [Header("Condition Settings")]
    public string statName; // e.g., "currentFullness" (the stat to check)
    public ConditionOperator conditionOperator;

    [Tooltip("If false, the targetValue field is used as the fixed value. " +
             "If true, then targetStatName, multiplier, and offset will be used to compute the target value.")]
    public bool useDynamicTarget = false;

    [Tooltip("Fixed target value (used if useDynamicTarget is false).")]
    public float targetValue;

    [Tooltip("Name of the stat to use for dynamic target (used if useDynamicTarget is true).")]
    public string targetStatName;
    public float multiplier = 1f;
    public float offset = 0f;

    [Header("Task To Execute")]
    public NPCActionTask underlyingTask;

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

        Debug.Log($"[Conditional Task] Checking condition: {statName} ({statValue}) {conditionOperator} {effectiveTarget} = {conditionMet}");

        // If the condition is met, execute the underlying task.
        if (conditionMet)
        {
            if (underlyingTask != null)
            {
                yield return npc.StartCoroutine(underlyingTask.Execute(npc));
            }
            else
            {
                Debug.LogWarning("[Conditional Task] Underlying task is null.");
            }
        }
        else
        {
            Debug.Log("[Conditional Task] Condition not met. Skipping task.");
        }

        IsComplete = true;
        yield break;
    }

    public override void ResetTask()
    {
        base.ResetTask();
        if (underlyingTask != null)
        {
            underlyingTask.ResetTask();
        }
    }

    public override NPCActionTaskState GetState()
    {
        NPCActionTaskState state = base.GetState();
        if (underlyingTask != null)
        {
            state.subStates = new List<NPCActionTaskState> { underlyingTask.GetState() };
        }
        return state;
    }

    public override void SetState(NPCActionTaskState state)
    {
        base.SetState(state);
        if (underlyingTask != null && state != null && state.subStates != null && state.subStates.Count > 0)
        {
            underlyingTask.SetState(state.subStates[0]);
        }
    }
}
