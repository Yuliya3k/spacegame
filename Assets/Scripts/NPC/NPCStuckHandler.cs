using UnityEngine;
using UnityEngine.AI;

public class NPCStuckHandler : MonoBehaviour
{
    [Header("Stuck Detection Settings")]
    [Tooltip("Time (in seconds) between movement checks.")]
    public float checkInterval = 2f;
    [Tooltip("Minimum distance the NPC must move during the interval to not be considered stuck.")]
    public float minimalMovement = 0.5f;

    [Header("Correction Settings")]
    [Tooltip("Distance to try to move when correcting a stuck situation.")]
    public float correctionDistance = 2f;

    private Vector3 lastPosition;
    private float timer;
    private NavMeshAgent agent;
    private NPCController npcController;

    private void Start()
    {
        npcController = GetComponent<NPCController>();
        if (npcController == null)
        {
            Debug.LogError("NPCStuckHandler: NPCController not found on this GameObject.");
            return;
        }
        agent = npcController.navAgent;
        if (agent == null)
        {
            Debug.LogError("NPCStuckHandler: NavMeshAgent not found in NPCController.");
            return;
        }
        lastPosition = transform.position;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            float movedDistance = Vector3.Distance(transform.position, lastPosition);
            if (movedDistance < minimalMovement)
            {
                Debug.Log("NPCStuckHandler: NPC appears stuck. Attempting to correct path.");

                // First, try a simple correction (rotate current direction by ±90°).
                if (agent.hasPath)
                {
                    Vector3 currentDirection = (agent.destination - transform.position).normalized;
                    float angle = Random.value > 0.5f ? 90f : -90f;
                    Vector3 newDirection = Quaternion.Euler(0, angle, 0) * currentDirection;
                    Vector3 newDestination = transform.position + newDirection * correctionDistance;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(newDestination, out hit, correctionDistance, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                        Debug.Log("NPCStuckHandler: Temporary destination set: " + hit.position);
                    }
                }
                else
                {
                    Vector3 randomOffset = Random.insideUnitSphere * correctionDistance;
                    randomOffset.y = 0f;
                    Vector3 randomDestination = transform.position + randomOffset;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomDestination, out hit, correctionDistance, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                        Debug.Log("NPCStuckHandler: No path; warped to: " + hit.position);
                    }
                }

                // Attempt to reapply the original planned destination.
                NavMeshPath newPath = new NavMeshPath();
                if (agent.CalculatePath(npcController.plannedDestination, newPath))
                {
                    if (newPath.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.SetDestination(npcController.plannedDestination);
                        Debug.Log("NPCStuckHandler: Reapplying planned destination: " + npcController.plannedDestination);
                    }
                    else if (newPath.corners.Length > 0)
                    {
                        Vector3 fallbackDestination = newPath.corners[newPath.corners.Length - 1];
                        agent.SetDestination(fallbackDestination);
                        Debug.Log("NPCStuckHandler: Reapplying fallback destination: " + fallbackDestination);
                    }
                }
            }
            lastPosition = transform.position;
            timer = 0f;
        }
    }
}
