using UnityEngine;

public class NPCFloatingStatusDisplay : MonoBehaviour
{
    [Header("Position Settings")]
    [Tooltip("Vertical offset above the NPC's head for the floating UI.")]
    public float verticalOffset = 2.5f;

    [Header("UI Containers")]
    [Tooltip("Container for primary status icons (e.g., fullness, reputation, weight).")]
    public Transform topContainer;
    [Tooltip("Container for secondary status icons (e.g., full, hungry, etc.).")]
    public Transform bottomContainer;

    [Header("Camera Reference")]
    [Tooltip("Reference to the camera that should be used for billboard rotation. If unassigned, Camera.main is used.")]
    public Camera mainCamera;

    void Start()
    {
        // Use the assigned camera, or fallback to Camera.main.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogError("NPCFloatingStatusDisplay: No camera is assigned and Camera.main could not be found.");
        }

        // Adjust the position of this display so it's above the NPC's head.
        Vector3 pos = transform.position;
        pos.y += verticalOffset;
        transform.position = pos;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
            return;

        // Billboard the top container so it always faces the camera.
        if (topContainer != null)
        {
            Vector3 directionToCamera = topContainer.position - mainCamera.transform.position;
            if (directionToCamera.sqrMagnitude > 0.001f)
            {
                topContainer.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }

        // Billboard the bottom container so it always faces the camera.
        if (bottomContainer != null)
        {
            Vector3 directionToCamera = bottomContainer.position - mainCamera.transform.position;
            if (directionToCamera.sqrMagnitude > 0.001f)
            {
                bottomContainer.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
    }
}
