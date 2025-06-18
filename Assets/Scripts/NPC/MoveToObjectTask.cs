using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MoveToObjectTask : NPCActionTask
{
    private Transform targetTransform;
    private float stoppingDistance;

    public MoveToObjectTask(Transform target, float stoppingDistance = 0.5f)
    {
        targetTransform = target;
        this.stoppingDistance = stoppingDistance;
    }

    public override IEnumerator Execute(NPCController npc)
    {
        Debug.Log("Executing MoveToObjectTask");
        NavMeshAgent agent = npc.navAgent;
        if (agent == null)
        {
            Debug.LogWarning("NPC does not have a NavMeshAgent!");
            yield break;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("NPC agent is not on a NavMesh. Attempting to sample a valid position...");
            NavMeshHit hit;
            if (NavMesh.SamplePosition(npc.transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                Debug.Log("Agent warped to nearest NavMesh position: " + hit.position);
            }
            else
            {
                Debug.LogError("Could not find a valid NavMesh position near the NPC.");
                yield break;
            }
        }

        // Store the destination in NPCController.
        npc.plannedDestination = targetTransform.position;
        agent.SetDestination(npc.plannedDestination);
        Debug.Log("Destination set to: " + npc.plannedDestination);

        while (Vector3.Distance(npc.transform.position, targetTransform.position) > stoppingDistance)
        {
            yield return null;
        }

        IsComplete = true;
        Debug.Log("Finished moving to target.");
    }
}
