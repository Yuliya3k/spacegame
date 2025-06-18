using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationPeriodInHours = 72f; // Time (in game hours) for one full rotation
    public Vector3 rotationAxis = Vector3.up; // Axis of rotation

    private InGameTimeManager timeManager;

    private void Start()
    {
        // Find the InGameTimeManager in the scene
        timeManager = FindObjectOfType<InGameTimeManager>();
        if (timeManager == null)
        {
            Debug.LogError("InGameTimeManager not found in the scene.");
        }
    }

    private void Update()
    {
        if (timeManager == null) return;

        // Calculate the delta game time in seconds
        float deltaGameTimeInSeconds = Time.deltaTime * timeManager.timeMultiplier * timeManager.timeScale;

        // Convert delta game time to hours
        float deltaGameTimeInHours = deltaGameTimeInSeconds / 3600f;

        // Calculate rotation angle based on delta game time
        float rotationAngle = (360f / rotationPeriodInHours) * deltaGameTimeInHours;

        // Apply rotation
        transform.Rotate(rotationAxis, rotationAngle, Space.Self);
    }
}
