using UnityEngine;
using UnityEngine.AI;

public class NPCBehaviour : MonoBehaviour
{
    public enum NPCState
    {
        Idle,
        Wander,
        Interact
    }

    [Header("Behavior Settings")]
    public NPCState currentState = NPCState.Idle;
    public float idleTime = 3f;           // How long to stay idle
    public float wanderRadius = 10f;      // Radius around home for wandering
    public float interactDistance = 5f;   // Distance at which NPC switches to interact state

    private NavMeshAgent agent;
    private float stateTimer;
    private Vector3 homePosition;
    private Transform player;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        homePosition = transform.position;
        stateTimer = idleTime;

        // Find the player by tag (ensure your player GameObject has the tag "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        // Check for state transitions based on distance to the player.
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < interactDistance && currentState != NPCState.Interact)
            {
                ChangeState(NPCState.Interact);
            }
            else if (distance >= interactDistance && currentState == NPCState.Interact)
            {
                // Exit Interact state and return to idle
                ChangeState(NPCState.Idle);
            }
        }

        // Process current state
        switch (currentState)
        {
            case NPCState.Idle:
                HandleIdleState();
                break;
            case NPCState.Wander:
                HandleWanderState();
                break;
            case NPCState.Interact:
                HandleInteractState();
                break;
        }
    }

    private void HandleIdleState()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(NPCState.Wander);
        }
    }

    private void HandleWanderState()
    {
        // If the NPC is close to its destination, switch to idle state.
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            stateTimer = idleTime;
            ChangeState(NPCState.Idle);
        }
    }

    private void HandleInteractState()
    {
        // In the interact state, have the NPC face the player.
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            // Only rotate on the Y-axis
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            // You can add additional behavior here (like triggering dialogue or an animation)
        }
    }

    public void ChangeState(NPCState newState)
    {
        currentState = newState;
        stateTimer = idleTime;

        if (newState == NPCState.Wander)
        {
            Vector3 wanderTarget = GetRandomWanderPoint();
            agent.SetDestination(wanderTarget);
        }
    }

    private Vector3 GetRandomWanderPoint()
    {
        // Choose a random point inside a sphere with radius 'wanderRadius' around the home position.
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += homePosition;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, NavMesh.AllAreas))
        {
            return navHit.position;
        }
        return transform.position;
    }
}
