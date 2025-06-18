//using System.Collections;
//using UnityEngine;
//using UnityEngine.AI;

//public class NPCDrinkingAndWalkingCycle : MonoBehaviour
//{
//    [Header("References")]
//    [Tooltip("Reference to the sink's position (an empty GameObject placed at the sink).")]
//    public Transform sinkTransform;
//    [Tooltip("An array of wander target objects. The NPC will randomly choose one as its next destination.")]
//    public Transform[] wanderTargets;
//    [Tooltip("Assign the CharacterStats component (if not assigned, it will be taken from NPCController).")]
//    public CharacterStats characterStats;
//    [Tooltip("Assign the NPCController component (if not assigned, it will be taken from this GameObject).")]
//    public NPCController npcController;
//    [Tooltip("Assign the NPCSinkManager component (if not assigned, it will be taken from this GameObject or its children).")]
//    public NPCSinkManager sinkManager;

//    [Header("Cycle Settings")]
//    [Tooltip("How long (in seconds) the NPC will wander between drinking cycles.")]
//    public float wanderDuration = 120f;
//    [Tooltip("Fixed amount to increase the NPC's stomach capacity after each drinking cycle.")]
//    public float capacityIncreasePerCycle = 100f;

//    private void Awake()
//    {
//        // Ensure NPCController is assigned.
//        if (npcController == null)
//        {
//            npcController = GetComponent<NPCController>();
//            if (npcController == null)
//            {
//                Debug.LogError("NPCDrinkingAndWalkingCycle: NPCController not found on this GameObject.");
//            }
//        }

//        // Get CharacterStats from NPCController if not manually assigned.
//        if (characterStats == null && npcController != null)
//        {
//            characterStats = npcController.characterStats;
//            if (characterStats == null)
//            {
//                Debug.LogError("NPCDrinkingAndWalkingCycle: CharacterStats not found on NPCController.");
//            }
//        }

//        // Get the NPCSinkManager.
//        if (sinkManager == null)
//        {
//            sinkManager = GetComponent<NPCSinkManager>();
//            if (sinkManager == null)
//            {
//                sinkManager = GetComponentInChildren<NPCSinkManager>();
//                if (sinkManager == null)
//                {
//                    Debug.LogError("NPCDrinkingAndWalkingCycle: NPCSinkManager not found on this GameObject or its children.");
//                }
//            }
//        }

//        // Validate required references.
//        if (sinkTransform == null)
//        {
//            Debug.LogError("NPCDrinkingAndWalkingCycle: sinkTransform is not assigned.");
//        }
//        if (wanderTargets == null || wanderTargets.Length == 0)
//        {
//            Debug.LogError("NPCDrinkingAndWalkingCycle: No wanderTargets assigned. Please assign at least one wander target.");
//        }
//    }

//    private void Start()
//    {
//        StartCoroutine(CycleBehavior());
//    }

//    private IEnumerator CycleBehavior()
//    {
//        while (true)
//        {
//            // Move to the sink using NPCController's movement logic.
//            yield return StartCoroutine(npcController.MoveToPositionRoutine(sinkTransform.position));

//            // Start the drinking action.
//            if (sinkManager != null)
//            {
//                sinkManager.StartDrinkingForNPC();
//            }
//            else
//            {
//                Debug.LogError("NPCDrinkingAndWalkingCycle: Cannot start drinking because sinkManager is null.");
//                yield break;
//            }

//            // Wait until the drinking action finishes.
//            while (sinkManager.IsPerformingAction())
//            {
//                yield return null;
//            }

//            // Increase stomach capacity unconditionally.
//            characterStats.stomachCapacity += capacityIncreasePerCycle;
//            Debug.Log("NPCDrinkingAndWalkingCycle: Increased stomach capacity to " + characterStats.stomachCapacity);

//            // Wander around for the specified duration.
//            yield return StartCoroutine(WanderRoutine(wanderDuration));
//        }
//    }

//    // This routine relies solely on NPCController's movement logic.
//    private IEnumerator WanderRoutine(float duration)
//    {
//        float timer = 0f;
//        while (timer < duration)
//        {
//            // Choose a random wander target from the array.
//            Transform target = wanderTargets[Random.Range(0, wanderTargets.Length)];

//            // Command NPCController to move to the chosen target.
//            yield return StartCoroutine(npcController.MoveToPositionRoutine(target.position));

//            // Wait a random time (between 5 and 15 seconds) before choosing a new target.
//            float waitTime = Random.Range(5f, 15f);
//            float elapsed = 0f;
//            while (elapsed < waitTime && timer < duration)
//            {
//                elapsed += Time.deltaTime;
//                timer += Time.deltaTime;
//                yield return null;
//            }
//        }
//    }
//}
