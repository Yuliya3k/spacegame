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
        npc.navAgent.SetDestination(bed.sleepPosition.position);
        while (Vector3.Distance(npc.transform.position, bed.sleepPosition.position) > 0.5f)
        {
            yield return null;
        }
        NPCSleepManager.instance.StartSleep(sleepHours, bed);
        yield return new WaitForSeconds(5f);
        IsComplete = true;
    }
}
