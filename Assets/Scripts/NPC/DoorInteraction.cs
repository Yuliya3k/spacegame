//using System.Collections;
//using UnityEngine;

//public class DoorInteraction : MonoBehaviour, INPCInteraction
//{
//    public SlidingDoorController doorController; // Your door logic script

//    public IEnumerator ExecuteInteraction(NPCController npc)
//    {
//        if (doorController != null && !doorController.AreDoorsOpen && !doorController.IsMoving)
//        {
//            doorController.OpenDoors();
//        }
//        // Optionally wait a short duration so the NPC “observes” the door opening.
//        yield return new WaitForSeconds(0.5f);
//    }
//}
