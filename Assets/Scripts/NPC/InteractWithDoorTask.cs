using System.Collections;
using UnityEngine;

public class InteractWithDoorTask : NPCActionTask
{
    private SlidingDoorController door;

    public InteractWithDoorTask(SlidingDoorController doorController)
    {
        door = doorController;
    }

    public override IEnumerator Execute(NPCController npc)
    {
        if (waypointIndex == 0)
        {
            door.ToggleDoors();
            waypointIndex = 1;
        }
        while (elapsedTime < door.openTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
