using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NPCController : MonoBehaviour
{
    public NavMeshAgent navAgent { get; private set; }
    public Animator animator;
    public string npcName; // Unique name identifier for saving/loading
    public Text locationText;
    public CharacterStats characterStats;
    public Inventory inventory;

    [Header("Movement Settings")]
    public float baseMaxForwardSpeed = 3f;
    public float sprintMaxForwardSpeed = 5f;
    public float rotationSpeed = 180f;
    public float walkRotationOffset = 10f;
    public float runRotationOffset = 5f;
    public float groundAccel = 5f;
    public float groundDecel = 25f;

    public Vector3 plannedDestination;

    private NPCBehaviour npcBehaviour;
    private NPCBehaviour.NPCState previousState;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (characterStats == null)
        {
            characterStats = GetComponent<CharacterStats>();
        }

        npcBehaviour = GetComponent<NPCBehaviour>();
        if (npcBehaviour != null)
        {
            previousState = npcBehaviour.currentState;
        }

    }

    private void Start()
    {
        if (navAgent != null)
        {
            navAgent.speed = baseMaxForwardSpeed;
            // Let the NavMeshAgent update position and rotation automatically.
            navAgent.updatePosition = true;
            navAgent.updateRotation = true;
        }

        if (navAgent != null && !navAgent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                navAgent.Warp(hit.position);
                Debug.Log("NPCController: Agent warped to valid NavMesh position: " + hit.position);
            }
            else
            {
                Debug.LogError("NPCController: No valid NavMesh position found near starting position.");
            }
        }
    }

    private void Update()
    {
        if (animator != null && navAgent != null)
        {
            // Pass the current speed to the Animator (for blending animations).
            animator.SetFloat("ForwardSpeed", navAgent.velocity.magnitude);

            // Rotate to face the direction of movement.
            Vector3 desiredVel = navAgent.desiredVelocity;
            desiredVel.y = 0f;
            if (desiredVel.sqrMagnitude > 0.001f)
            {
                // Use a rotation offset based on speed.
                float offset = (navAgent.velocity.magnitude > baseMaxForwardSpeed * 0.8f) ? runRotationOffset : walkRotationOffset;
                Quaternion targetRotation = Quaternion.LookRotation(desiredVel.normalized) * Quaternion.Euler(0, offset, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public bool IsMoving()
    {
        return navAgent != null && navAgent.velocity.magnitude > 0.1f;
    }

    public bool IsFrozen => navAgent != null && navAgent.isStopped;

    //public IEnumerator WaitWhileFrozen()
    //{
    //    while (IsFrozen)
    //        yield return null;
    //}

    // New: Extend GetSaveData to include nav destination and current animation
    public NPCSaveData GetSaveData()
    {
        NPCSaveData data = new NPCSaveData();
        data.npcName = npcName;
        // Save the current transform position (in case we need it as a fallback)
        data.playerPosition = new Vector3Data(transform.position);
        data.navDestination = new Vector3Data(navAgent.destination);
        data.currentAnimationState = GetCurrentAnimationState();

        // Use the same helper method as for the player so that all stats are saved consistently.
        data.characterData = SaveSystem.instance.GetCharacterData(characterStats);

        // If you want to also save runtime state for the NPC, you could do:
        // data.runtimeState = SaveSystem.instance.GetRuntimeStateData(characterStats);

        // Save planner state if attached
        NPCPlanner planner = GetComponent<NPCPlanner>();
        if (planner != null)
        {
            data.currentTaskIndex = planner.currentTaskIndex;
            data.lastTaskPosition = new Vector3Data(planner.lastTaskPosition);
        }
        else
        {
            data.lastTaskPosition = new Vector3Data(transform.position);
        }
        return data;
    }

    private string GetCurrentAnimationState()
    {
        // A simple helper that returns a string based on the current animator state.
        // In a full solution you might map state hash codes to meaningful names.
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime < 0.1f)
            return "Idle";
        else
            return "Moving";
    }

    // NEW: Coroutine to move the NPC to a destination using the NavMeshAgent.
    public IEnumerator MoveToPositionRoutine(Vector3 destination, float stoppingDistance = 0.5f)
    {
        if (navAgent != null)
        {
            plannedDestination = destination;
            navAgent.SetDestination(destination);
            while (navAgent.pathPending || navAgent.remainingDistance > stoppingDistance)
            {
                yield return WaitWhileFrozen();
                yield return null;
            }
        }
        else
        {
            Debug.LogError("NPCController: NavMeshAgent not assigned.");
            yield break;
        }
    }

    public void AttemptOpenNearbyDoor()
    {
        // Example logic: use Physics.OverlapSphere or Raycast to detect a door and call its OpenDoors() method.
        Debug.Log("NPCController: Attempting to open nearby door.");
        // Your door-opening logic goes here.
    }

    /// <summary>
    /// Freeze the NPC in place.
    /// </summary>
    /// <param name="disableAnimator">If true the animator will be disabled while frozen.</param>
    /// <param name="facePlayer">If true the NPCBehaviour will switch to the Interact state so the NPC faces the player.</param>
    //public void Freeze(bool disableAnimator = false, bool facePlayer = false)
    //{
    //    if (navAgent != null)
    //    {
    //        navAgent.isStopped = true;
    //    }

    //    if (disableAnimator && animator != null)
    //    {
    //        animator.enabled = false;
    //    }

    //    if (facePlayer)
    //    {
    //        var behaviour = GetComponent<NPCBehaviour>();
    //        if (behaviour != null)
    //        {
    //            behaviour.ChangeState(NPCBehaviour.NPCState.Interact);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Resume normal behaviour after being frozen.
    ///// </summary>
    //public void Unfreeze()
    //{
    //    if (navAgent != null)
    //    {
    //        navAgent.isStopped = false;
    //    }

    //    if (animator != null)
    //    {
    //        animator.enabled = true;
    //    }
    //}


    public void Freeze()
    {
        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }

        if (npcBehaviour != null)
        {
            previousState = npcBehaviour.currentState;
            npcBehaviour.currentState = NPCBehaviour.NPCState.Interact;
        }
    }

    public void Unfreeze()
    {
        if (navAgent != null)
        {
            navAgent.isStopped = false;
        }

        if (npcBehaviour != null)
        {
            npcBehaviour.currentState = previousState;
        }
    }


    /// <summary>
    /// Waits until the global input freeze is lifted.
    /// </summary>
    public IEnumerator WaitWhileFrozen()
    {
        if (InputFreezeManager.instance == null)
            yield break;

        yield return new WaitUntil(() => !InputFreezeManager.instance.IsFrozen);


    }

}
