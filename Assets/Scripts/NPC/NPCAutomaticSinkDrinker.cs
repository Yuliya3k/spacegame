//using UnityEngine;

//public class NPCAutomaticSinkDrinker : MonoBehaviour
//{
//    [Header("Detection Settings")]
//    [Tooltip("Distance in front of the NPC from which to start detection.")]
//    public float detectionDistance = 2f;

//    [Tooltip("Radius of the detection sphere.")]
//    public float detectionRadius = 1f;

//    [Tooltip("Layer mask to filter sink objects (e.g., set this to the Sink layer).")]
//    public LayerMask sinkLayerMask;

//    [Header("Trigger Cooldown")]
//    [Tooltip("Time (in seconds) before the NPC can trigger drinking again.")]
//    public float triggerCooldown = 5f;
//    private float cooldownTimer = 0f;

//    private NPCSinkManager sinkManager;
//    private CharacterStats characterStats;

//    private void Awake()
//    {
//        // Get the NPCSinkManager from this NPC.
//        sinkManager = GetComponent<NPCSinkManager>();
//        if (sinkManager == null)
//        {
//            Debug.LogError("NPCAutomaticSinkDrinker: NPCSinkManager component not found on this NPC.");
//        }

//        characterStats = GetComponent<CharacterStats>();
//        if (characterStats == null)
//        {
//            Debug.LogError("NPCAutomaticSinkDrinker: CharacterStats component not found on this NPC.");
//        }
//    }

//    private void Update()
//    {
//        // Use a cooldown so we don't trigger drinking repeatedly.
//        if (cooldownTimer > 0f)
//        {
//            cooldownTimer -= Time.deltaTime;
//            return;
//        }

//        // Only trigger if the NPC is not already performing a sink action.
//        if (sinkManager != null && !sinkManager.IsPerformingAction())
//        {
//            // Define an origin point in front of the NPC.
//            Vector3 origin = transform.position + transform.forward * detectionDistance;

//            // OverlapSphere using the specified layer mask (e.g., the Sink layer).
//            Collider[] hits = Physics.OverlapSphere(origin, detectionRadius, sinkLayerMask);
//            foreach (Collider hit in hits)
//            {
//                // Check if the hit object has an InteractableSink component.
//                var sink = hit.GetComponent<InteractableSink>();
//                if (sink != null)
//                {
//                    // Optionally, you might check if the NPC is in a state to drink,
//                    // for example, if its fullness is below its stomach capacity.
//                    if (characterStats.currentFullness < characterStats.stomachCapacity)
//                    {
//                        Debug.Log("NPCAutomaticSinkDrinker: Sink detected: " + hit.gameObject.name);
//                        sinkManager.StartDrinkingForNPC();
//                        cooldownTimer = triggerCooldown;
//                        break;
//                    }
//                }
//            }
//        }
//    }

//    // Draw the detection sphere in the editor for visualization.
//    private void OnDrawGizmosSelected()
//    {
//        Vector3 origin = transform.position + transform.forward * detectionDistance;
//        Gizmos.color = Color.cyan;
//        Gizmos.DrawWireSphere(origin, detectionRadius);
//    }
//}
