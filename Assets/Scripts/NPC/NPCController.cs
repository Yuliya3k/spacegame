using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum DietPreference
{
    HighVolumeLowCalories,
    HighCalorieDensity
}

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
    public DietPreference dietPreference = DietPreference.HighVolumeLowCalories;

    [Header("Movement Settings")]
    public float baseMaxForwardSpeed = 3f;
    public float sprintMaxForwardSpeed = 5f;
    public float rotationSpeed = 180f;
    public float walkRotationOffset = 10f;
    public float runRotationOffset = 5f;
    public float groundAccel = 5f;
    public float groundDecel = 25f;

    public Vector3 plannedDestination;

    [Header("Animation Settings")]
    public string defaultAnimationTrigger = "Idle";

    [Tooltip("Animator trigger name for Talk interactions")]
    public string talkAnimationTrigger = "Talk";
    [Tooltip("Animator trigger name for Trade interactions")]
    public string tradeAnimationTrigger = "Trade";

    private NPCBehaviour npcBehaviour;
    private NPCBehaviour.NPCState previousState;
    private int previousAnimHash;
    private float previousAnimNormalizedTime;

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

    // New: Extend GetSaveData to include nav destination, inventory and runtime state
    public NPCSaveData GetSaveData()
    {
        NPCSaveData data = new NPCSaveData();
        data.npcName = npcName;
        // Save the current transform position (in case we need it as a fallback)
        data.playerPosition = new Vector3Data(transform.position);
        data.navDestination = new Vector3Data(navAgent.destination);
        if (navAgent != null && navAgent.hasPath)
        {
            data.navPathCorners = new List<Vector3Data>();
            foreach (Vector3 corner in navAgent.path.corners)
            {
                data.navPathCorners.Add(new Vector3Data(corner));
            }
        }
        data.currentAnimationState = GetCurrentAnimationState();

        // Use the same helper method as for the player so that all stats are saved consistently.
        data.characterData = SaveSystem.instance.GetCharacterData(characterStats);

        // Save runtime state so that NPC timers and statuses persist
        data.runtimeState = SaveSystem.instance.GetRuntimeStateData(characterStats);

        // Save the NPC inventory
        if (inventory != null)
        {
            data.inventoryData = SaveSystem.instance.GetInventoryData(inventory);
            data.equippedItems = SaveSystem.instance.GetEquippedItemData(inventory.equipment);
        }

        // Save container data using existing helper methods
        data.storageContainers = SaveSystem.instance.GetStorageContainerData(SaveSystem.instance.storageContainers);
        data.disposableContainers = SaveSystem.instance.GetDisposableContainerData(SaveSystem.instance.disposableContainers);

        // Save global in-game time
        data.inGameTime = new DateTimeData(SaveSystem.instance.timeManager.GetCurrentInGameTime());

        // Save chosen character profile if available
        CharacterProfile chosenProfile = CharacterProfileManager.instance.chosenProfile;
        if (chosenProfile != null)
        {
            CharacterProfileData profileData = new CharacterProfileData();
            profileData.profileName = chosenProfile.characterName;
            profileData.enableBoobGain = chosenProfile.enableBoobGain;
            profileData.enableTorsoGain = chosenProfile.enableTorsoGain;
            profileData.enableThighsGain = chosenProfile.enableThighsGain;
            profileData.enableShinsGain = chosenProfile.enableShinsGain;
            profileData.enableArmsGain = chosenProfile.enableArmsGain;
            profileData.enableWholeBodyGain = chosenProfile.enableWholeBodyGain;
            profileData.enableGlutesGain = chosenProfile.enableGlutesGain;

            foreach (var blendShapeSetting in chosenProfile.baseBlendShapes)
            {
                BlendShapeSettingData bsd = new BlendShapeSettingData();
                bsd.blendShapeName = blendShapeSetting.blendShapeName;
                bsd.value = blendShapeSetting.value;
                profileData.baseBlendShapes.Add(bsd);
            }

            data.chosenProfileData = profileData;
        }

        // Save planner state if attached
        NPCPlanner planner = GetComponent<NPCPlanner>();
        if (planner != null)
        {
            data.currentTaskIndex = planner.currentTaskIndex;
            data.lastTaskPosition = new Vector3Data(planner.lastTaskPosition);
            data.taskStates = planner.GetTaskStates();
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

    private Coroutine facePlayerCoroutine;
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

        FacePlayerSmoothly();
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

        if (animator != null && previousAnimHash != 0)
        {
            animator.Play(previousAnimHash, 0, previousAnimNormalizedTime);
            previousAnimHash = 0;
            previousAnimNormalizedTime = 0f;
        }
    }



    private void FacePlayer()
    {
        var player = PlayerControllerCode.instance;
        if (player == null)
            return;

        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void FacePlayerSmoothly()
    {
        if (facePlayerCoroutine != null)
        {
            StopCoroutine(facePlayerCoroutine);
        }
        facePlayerCoroutine = StartCoroutine(RotateTowardsPlayer());
    }

    private IEnumerator RotateTowardsPlayer()
    {
        var player = PlayerControllerCode.instance;
        if (player == null)
            yield break;

        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
            yield break;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
            direction = player.transform.position - transform.position;
            direction.y = 0f;
            targetRotation = Quaternion.LookRotation(direction);
        }
        transform.rotation = targetRotation;
    }

    public void SetInteractionAnimation(string trigger)
    {
        if (animator == null)
            return;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        previousAnimHash = info.fullPathHash;
        previousAnimNormalizedTime = info.normalizedTime;
        animator.SetTrigger(trigger);

    }

    public void ReturnToDefaultAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(defaultAnimationTrigger))
        {
            animator.SetTrigger(defaultAnimationTrigger);
        }
    }

    //public void SetInteractionAnimation(string trigger)
    //{
    //    Freeze();
    //    if (animator != null && !string.IsNullOrEmpty(trigger))
    //    {
    //        animator.SetTrigger(trigger);
    //    }
    //}

    //public void ReturnToDefaultAnimation()
    //{
    //    if (animator != null && !string.IsNullOrEmpty(defaultAnimationTrigger))
    //    {
    //        animator.SetTrigger(defaultAnimationTrigger);
    //    }
    //}

    //public void SetInteractionAnimation(string animationName)
    //{
    //    if (animator != null && !string.IsNullOrEmpty(animationName))
    //    {
    //        animator.Play(animationName);
    //    }
    //}




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
