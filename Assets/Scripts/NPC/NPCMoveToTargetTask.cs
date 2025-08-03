using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "NPC/Tasks/Move To Target Task")]
public class NPCMoveToTargetTask : NPCActionTask
{
    [Tooltip("The name of the target GameObject the NPC should move to (must be present in the scene).")]
    public string targetObjectName;

    [Tooltip("Distance threshold to consider arrival.")]
    public float stoppingDistance = 0.5f;

    public override IEnumerator Execute(NPCController npc)
    {
        // ResetTask(); // Reset task state.
        Debug.Log("NPCMoveToTargetTask: Starting execution for target " + targetObjectName);

        if (npc == null)
        {
            Debug.LogWarning("NPCMoveToTargetTask: NPC is null.");
            IsComplete = true;
            yield break;
        }

        // Resolve the target object by name.
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject == null)
        {
            Debug.LogWarning("NPCMoveToTargetTask: Target object with name '" + targetObjectName + "' not found in the scene.");
            IsComplete = true;
            yield break;
        }

        // Command the NPCController to move to the target's position with the specified stopping distance.
        yield return npc.MoveToPositionRoutine(targetObject.transform.position, stoppingDistance);

        Debug.Log("NPCMoveToTargetTask: Reached target " + targetObjectName);
        IsComplete = true;
    }
}
