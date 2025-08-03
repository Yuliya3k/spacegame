using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Tasks/Wait Task")]
public class NPCWaitTask : NPCActionTask
{
    [Tooltip("Wait time in in-game minutes.")]
    public float waitInGameMinutes = 1f;

    public override IEnumerator Execute(NPCController npc)
    {
        float waitDuration;
        if (npc.characterStats != null && npc.characterStats.timeManager != null)
        {
            waitDuration = npc.characterStats.timeManager.GetRealTimeDurationForGameMinutes(waitInGameMinutes);
        }
        else
        {
            Debug.LogWarning("NPCWaitTask: InGameTimeManager not found. Falling back to real time wait.");
            waitDuration = waitInGameMinutes;
        }

        Debug.Log($"NPCWaitTask: Waiting for {waitDuration} seconds. Resumed at {elapsedTime}.");
        while (elapsedTime < waitDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("NPCWaitTask: Wait complete.");
        IsComplete = true;
    }
}
