using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Tasks/Wait Task")]
public class NPCWaitTask : NPCActionTask
{
    [Tooltip("Wait time in in-game minutes.")]
    public float waitInGameMinutes = 1f;

    public override IEnumerator Execute(NPCController npc)
    {
        ResetTask();
        Debug.Log("NPCWaitTask: Waiting for " + waitInGameMinutes + " in-game minutes.");

        // Get the InGameTimeManager from the NPC's CharacterStats.
        if (npc.characterStats != null && npc.characterStats.timeManager != null)
        {
            float waitDuration = npc.characterStats.timeManager.GetRealTimeDurationForGameMinutes(waitInGameMinutes);
            yield return new WaitForSeconds(waitDuration);
        }
        else
        {
            Debug.LogWarning("NPCWaitTask: InGameTimeManager not found. Falling back to real time wait.");
            yield return new WaitForSeconds(waitInGameMinutes); // Fallback if no timeManager is found.
        }

        Debug.Log("NPCWaitTask: Wait complete.");
        IsComplete = true;
    }
}
