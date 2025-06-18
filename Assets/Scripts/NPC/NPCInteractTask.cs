using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Tasks/Interact Task")]
public class NPCInteractTask : NPCActionTask
{
    [Tooltip("The target GameObject that will be interacted with (it must implement INPCInteraction).")]
    public GameObject targetObject;

    public override IEnumerator Execute(NPCController npc)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("NPCInteractTask: targetObject is null.");
            IsComplete = true;
            yield break;
        }
        // (Optional) Move the NPC close to the target before interacting.
        yield return npc.MoveToPositionRoutine(targetObject.transform.position);

        // Get the component that implements INPCInteraction.
        INPCInteraction interactable = targetObject.GetComponent<MonoBehaviour>() as INPCInteraction;
        if (interactable != null)
        {
            yield return interactable.ExecuteInteraction(npc);
        }
        else
        {
            Debug.LogWarning("NPCInteractTask: The target object does not implement INPCInteraction.");
        }
        IsComplete = true;
    }
}
