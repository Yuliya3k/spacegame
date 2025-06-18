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
        // Here we assume that "ToggleDoors" simulates pressing E.
        door.ToggleDoors();
        // Wait for the door to finish moving (adjust the wait time as needed).
        yield return new WaitForSeconds(door.openTime);
        IsComplete = true;
    }
}
