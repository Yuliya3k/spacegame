using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCDefecationManager : MonoBehaviour
{
    private InGameTimeManager timeManager;
    private NPCController npcController;
    public CharacterStats characterStats;

    [Header("Defecation Settings")]
    public float defecationDurationInGameMinutes = 5f;

    [Header("Animation Settings")]
    public string sitAnimationTrigger = "SitOnToilet";
    public string standUpAnimationTrigger = "StandUp";

    [Header("NPC Components to Disable")]
    public GameObject npcPhysicsObject; // The NPC's physics object (with Rigidbody and Collider)
    private Rigidbody npcRigidbody;
    private Collider npcCollider;

    [Header("Defecation Position")]
    [Tooltip("The designated defecation position for the NPC.")]
    public Transform defecationPosition;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public bool IsDefecating { get; private set; } = false;

    private void Awake()
    {
        npcController = GetComponent<NPCController>();
        if (npcController == null)
        {
            Debug.LogError("NPCDefecationManager: NPCController not found on this GameObject.");
        }
        timeManager = FindObjectOfType<InGameTimeManager>();
        if (npcPhysicsObject != null)
        {
            npcRigidbody = npcPhysicsObject.GetComponent<Rigidbody>();
            npcCollider = npcPhysicsObject.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("NPCDefecationManager: npcPhysicsObject is not assigned.");
        }
    }

    public void StartDefecation()
    {
        if (IsDefecating)
            return;

        IsDefecating = true;
        Debug.Log("[NPCDefecationManager] Starting defecation.");

        // Save original position and rotation.
        originalPosition = npcController.transform.position;
        originalRotation = npcController.transform.rotation;

        // Stop NPC movement.
        if (npcController.navAgent != null)
        {
            npcController.navAgent.isStopped = true;
        }

        // If a defecation position is set, move there.
        if (defecationPosition != null)
        {
            npcController.transform.position = defecationPosition.position;
            npcController.transform.rotation = defecationPosition.rotation;
        }
        else
        {
            Debug.LogWarning("NPCDefecationManager: defecationPosition is not assigned. Using current position.");
        }

        // Disable physics.
        if (npcRigidbody != null)
        {
            npcRigidbody.isKinematic = true;
            npcRigidbody.linearVelocity = Vector3.zero;
            npcRigidbody.angularVelocity = Vector3.zero;
        }
        if (npcCollider != null)
        {
            npcCollider.enabled = false;
        }

        // Play sit animation.
        Animator anim = npcController.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger(sitAnimationTrigger);
        }

        // Start the defecation coroutine.
        StartCoroutine(DefecationCoroutine(anim));
    }

    private IEnumerator DefecationCoroutine(Animator anim)
    {
        // Wait for the sit animation to finish.
        yield return new WaitForSeconds(2f);

        // Fast-forward in-game time.
        float originalTimeMultiplier = timeManager.timeMultiplier;
        float targetTimeMultiplier = 1000f; // Fast-forward factor.
        float accelerationDuration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < accelerationDuration)
        {
            elapsedTime += Time.deltaTime;
            timeManager.timeMultiplier = Mathf.Lerp(originalTimeMultiplier, targetTimeMultiplier, elapsedTime / accelerationDuration);
            yield return null;
        }
        timeManager.timeMultiplier = targetTimeMultiplier;

        // Wait for defecation duration.
        DateTime startTime = timeManager.GetCurrentInGameTime();
        DateTime endTime = startTime.AddMinutes(defecationDurationInGameMinutes);
        while (timeManager.GetCurrentInGameTime() < endTime)
        {
            yield return null;
        }

        // Gradually restore time multiplier.
        elapsedTime = 0f;
        while (elapsedTime < accelerationDuration)
        {
            elapsedTime += Time.deltaTime;
            timeManager.timeMultiplier = Mathf.Lerp(targetTimeMultiplier, originalTimeMultiplier, elapsedTime / accelerationDuration);
            yield return null;
        }
        timeManager.timeMultiplier = originalTimeMultiplier;

        // Play stand-up animation.
        if (anim != null)
        {
            anim.SetTrigger(standUpAnimationTrigger);
        }
        yield return null;
        anim.ResetTrigger(sitAnimationTrigger);

        // Re-enable physics.
        if (npcRigidbody != null)
        {
            npcRigidbody.isKinematic = false;
        }
        if (npcCollider != null)
        {
            npcCollider.enabled = true;
        }

        // Restore NPC's original position and rotation.
        npcController.transform.position = originalPosition;
        npcController.transform.rotation = originalRotation;
        if (npcController.navAgent != null)
        {
            npcController.navAgent.isStopped = false;
            npcController.navAgent.ResetPath();
        }

        // Update NPC stats (simulate defecation).
        UpdateStatsAfterDefecation();

        Debug.Log("[NPCDefecationManager] Defecation completed.");
        IsDefecating = false;
    }

    private void UpdateStatsAfterDefecation()
    {
        // For example, reset currentIntFullness to zero.
        float relievedAmount = characterStats.currentIntFullness;
        characterStats.currentIntFullness = 0f;
        characterStats.UpdateIntestinesFullnessBlendShapes();
        Debug.Log("[NPCDefecationManager] Relieved " + relievedAmount + " ml of faeces.");
    }
}
