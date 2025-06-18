using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "NPC/Tasks/Door Open Task")]
public class NPCDoorOpenTask : NPCActionTask
{
    [Tooltip("The name of the door GameObject to open (must be present in the scene).")]
    public string doorObjectName;

    // Optional: You can add a waiting duration if you want the NPC to wait after opening.
    [Tooltip("Optional wait time (in in-game minutes) after opening the door.")]
    public float waitAfterOpenInGameMinutes = 0f;

    public override IEnumerator Execute(NPCController npc)
    {
        ResetTask();
        Debug.Log("NPCDoorOpenTask: Starting execution for door " + doorObjectName);

        if (npc == null)
        {
            Debug.LogWarning("NPCDoorOpenTask: NPC is null.");
            IsComplete = true;
            yield break;
        }

        // Resolve the target door object by name.
        GameObject doorObject = GameObject.Find(doorObjectName);
        if (doorObject == null)
        {
            Debug.LogWarning("NPCDoorOpenTask: Door object with name '" + doorObjectName + "' not found in the scene.");
            IsComplete = true;
            yield break;
        }

        // Get the SlidingDoorController component from the door.
        SlidingDoorController door = doorObject.GetComponent<SlidingDoorController>();
        if (door == null)
        {
            Debug.LogWarning("NPCDoorOpenTask: SlidingDoorController not found on " + doorObjectName);
            IsComplete = true;
            yield break;
        }

        // If the door is not already open, open it.
        if (!door.AreDoorsOpen)
        {
            door.OpenDoors();
        }

        // Wait until the door reports that it is open.
        while (!door.AreDoorsOpen)
        {
            yield return null;
        }

        // Optionally wait for a duration after the door has opened.
        if (waitAfterOpenInGameMinutes > 0f && npc.characterStats != null && npc.characterStats.timeManager != null)
        {
            float waitDuration = npc.characterStats.timeManager.GetRealTimeDurationForGameMinutes(waitAfterOpenInGameMinutes);
            yield return new WaitForSeconds(waitDuration);
        }

        Debug.Log("NPCDoorOpenTask: Completed execution for door " + doorObjectName);
        IsComplete = true;
    }
}
