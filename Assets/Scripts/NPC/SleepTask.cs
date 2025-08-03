using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SleepTask : NPCActionTask
{
    private InteractableBed bed;
    private int sleepHours;

    public SleepTask(InteractableBed bed, int sleepHours)
    {
        this.bed = bed;
        this.sleepHours = sleepHours;
    }

    public override IEnumerator Execute(NPCController npc)
    {
        // Stage 0: move to bed
        if (waypointIndex == 0)
        {
            npc.navAgent.SetDestination(bed.sleepPosition.position);
            while (Vector3.Distance(npc.transform.position, bed.sleepPosition.position) > 0.5f)
            {
                yield return null;
            }
            waypointIndex = 1;
        }

        // Stage 1: sleep for duration
        if (waypointIndex == 1)
        {
            NPCSleepManager.instance.StartSleep(sleepHours, bed);
            float sleepDuration = 5f; // placeholder
            while (elapsedTime < sleepDuration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            IsComplete = true;
        }
        
    }
}
