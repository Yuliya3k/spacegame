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

    // We'll store any detected interactable components here.
    public List<MonoBehaviour> detectedInteractables = new List<MonoBehaviour>();

    private void Update()
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
                Debug.Log("Detected interactable: " + hit.gameObject.name);
            }
        }
    }

    // Draw a visual representation of the detection area in the editor.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 detectionCenter = transform.position + transform.forward * detectionDistance;
        Gizmos.DrawWireSphere(detectionCenter, detectionRadius);
    }
}
