using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Tasks/Drink From Sink Task")]
public class NPCDrinkFromSinkTask : NPCActionTask
{
    [Tooltip("The fullness threshold (as a percentage of stomachCapacity) at which the NPC stops drinking. For example, 95 means 95%.")]
    public float fullnessThresholdPercent = 95f;

    [Tooltip("The amount to increase stomachCapacity after the drinking session.")]
    public float capacityIncreaseAfterDrinking = 100f;


    public override IEnumerator Execute(NPCController npc)
    {
        // Reset the task state.
        ResetTask();
        Debug.Log("NPCDrinkFromSinkTask: Starting execution. Will drink until fullness reaches " + fullnessThresholdPercent + "%.");

        if (npc == null)
        {
            Debug.LogWarning("NPCDrinkFromSinkTask: NPC is null.");
            IsComplete = true;
            yield break;
        }

        // Try to get the NPCSinkManager from the NPC's children.
        NPCSinkManager sinkManager = npc.GetComponentInChildren<NPCSinkManager>();
        if (sinkManager == null)
        {
            Debug.LogWarning("NPCDrinkFromSinkTask: NPCSinkManager not found on the NPC.");
            IsComplete = true;
            yield break;
        }

        // Start the drinking action.
        sinkManager.StartDrinkingForNPC();

        // Calculate the fullness target.
        float targetFullness = npc.characterStats.stomachCapacity * (fullnessThresholdPercent / 100f);

        // Wait until either:
        // (a) The sink manager is no longer performing the drinking action,
        // or (b) The character's current fullness reaches or exceeds the target.
        while (sinkManager.IsPerformingAction() && npc.characterStats.currentFullness < targetFullness)
        {
            yield return null;
        }

        npc.characterStats.stomachCapacity += capacityIncreaseAfterDrinking;

        // Clamp stomach capacity to maxStomachCapacity if needed.
        if (npc.characterStats.stomachCapacity > npc.characterStats.maxStomachCapacity)
        {
            npc.characterStats.stomachCapacity = npc.characterStats.maxStomachCapacity;
        }

        Debug.Log("NPCDrinkFromSinkTask: Increased stomachCapacity by " + capacityIncreaseAfterDrinking + " to " + npc.characterStats.stomachCapacity);



        Debug.Log("NPCDrinkFromSinkTask: Fullness threshold reached. Current fullness: " + npc.characterStats.currentFullness + " / " + npc.characterStats.stomachCapacity);
        IsComplete = true;
    }
}
