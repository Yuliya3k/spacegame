using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractableDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Distance in front of the NPC at which to check for interactables.")]
    public float detectionDistance = 3f;

    [Tooltip("Radius of the detection sphere.")]
    public float detectionRadius = 1f;

    [Tooltip("Layer mask for interactable objects.")]
    public LayerMask interactableLayerMask;

    [Header("Debug")]
    [Tooltip("Enable verbose logging for detected interactables.")]
    public bool verboseLogging = false;

    // We'll store any detected interactable components here.
    public List<MonoBehaviour> detectedInteractables = new List<MonoBehaviour>();

    [Tooltip("How often to run the detection logic in seconds.")]
    public float detectionInterval = 0.5f;

    private Coroutine detectionCoroutine;

    private void OnEnable()
    {
        // Start the detection coroutine so we are not checking every frame.
        detectionCoroutine = StartCoroutine(DetectionRoutine());
    }

    private void OnDisable()
    {
        if (detectionCoroutine != null)
        {
            StopCoroutine(detectionCoroutine);
        }
    }

    private IEnumerator DetectionRoutine()
    {
        var wait = new WaitForSeconds(detectionInterval);
        while (true)
        {
            DetectInteractables();
            yield return wait;
        }
    }

    private void DetectInteractables()
    {
        // Calculate the center point of the detection sphere.
        Vector3 detectionCenter = transform.position + transform.forward * detectionDistance;

        // Get all colliders in that sphere on the specified layers.
        Collider[] hits = Physics.OverlapSphere(detectionCenter, detectionRadius, interactableLayerMask);

        // Clear previous detections.
        detectedInteractables.Clear();

        // Check each collider for known interactable components.
        foreach (Collider hit in hits)
        {
            MonoBehaviour interactable = hit.GetComponent<InteractableBed>();
            if (interactable == null)
                interactable = hit.GetComponent<InteractableExerciseMachine>();
            if (interactable == null)
                interactable = hit.GetComponent<InteractableObject>();
            if (interactable == null)
                interactable = hit.GetComponent<InteractableRestPlace>();
            if (interactable == null)
                interactable = hit.GetComponent<InteractableShower>();
            if (interactable == null)
                interactable = hit.GetComponent<NPCSinkTrigger>();
            if (interactable == null)
                interactable = hit.GetComponent<InteractableToilet>();

            if (interactable != null)
            {
                detectedInteractables.Add(interactable);
                if (verboseLogging)
                {
                    Debug.Log("Detected interactable: " + hit.gameObject.name);
                }
            }
        }
    }

    // Optionally allow interactable objects with trigger colliders to
    // notify this detector when an NPC enters or exits them.
    private void OnTriggerEnter(Collider other)
    {
        MonoBehaviour interactable = GetInteractableFromCollider(other);
        if (interactable != null && !detectedInteractables.Contains(interactable))
        {
            detectedInteractables.Add(interactable);
            Debug.Log("Interactable entered trigger: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MonoBehaviour interactable = GetInteractableFromCollider(other);
        if (interactable != null)
        {
            detectedInteractables.Remove(interactable);
            Debug.Log("Interactable exited trigger: " + other.gameObject.name);
        }
    }

    // Helper method to fetch any known interactable component from a collider.
    private MonoBehaviour GetInteractableFromCollider(Collider hit)
    {
        MonoBehaviour interactable = hit.GetComponent<InteractableBed>();
        if (interactable == null)
            interactable = hit.GetComponent<InteractableExerciseMachine>();
        if (interactable == null)
            interactable = hit.GetComponent<InteractableObject>();
        if (interactable == null)
            interactable = hit.GetComponent<InteractableRestPlace>();
        if (interactable == null)
            interactable = hit.GetComponent<InteractableShower>();
        if (interactable == null)
            interactable = hit.GetComponent<NPCSinkTrigger>();
        if (interactable == null)
            interactable = hit.GetComponent<InteractableToilet>();
        return interactable;
    }

    // Draw a visual representation of the detection area in the editor.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 detectionCenter = transform.position + transform.forward * detectionDistance;
        Gizmos.DrawWireSphere(detectionCenter, detectionRadius);
    }
}
