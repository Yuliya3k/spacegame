using UnityEngine;

public class SunRotation : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform orbitCenter;          // Point around which the Sun orbits
    public float orbitPeriodInMonths = 3f; // Time (in game months) for one full orbit
    public Vector3 orbitAxis = Vector3.up; // Axis of orbit

    [Header("Self-Rotation Settings")]
    public float selfRotationPeriodInMonths = 3f; // Time (in game months) for one full self-rotation
    public Vector3 selfRotationAxis = Vector3.up; // Axis of self-rotation

    private InGameTimeManager timeManager;

    private void Start()
    {
        // Find the InGameTimeManager in the scene
        timeManager = FindObjectOfType<InGameTimeManager>();
        if (timeManager == null)
        {
            Debug.LogError("InGameTimeManager not found in the scene.");
        }

        if (orbitCenter == null)
        {
            Debug.LogError("Orbit Center is not assigned in SunRotation script.");
        }
    }

    private void Update()
    {
        if (timeManager == null || orbitCenter == null) return;

        // Calculate the delta game time in seconds
        float deltaGameTimeInSeconds = Time.deltaTime * timeManager.timeMultiplier * timeManager.timeScale;

        // Convert delta game time to months
        float deltaGameTimeInMonths = deltaGameTimeInSeconds / (3600f * 24f * 30f); // 3600s/hr * 24hr/day * 30day/month

        // Orbit Rotation
        float orbitRotationAngle = (360f / orbitPeriodInMonths) * deltaGameTimeInMonths;
        transform.RotateAround(orbitCenter.position, orbitAxis, orbitRotationAngle);

        // Self Rotation
        float selfRotationAngle = (360f / selfRotationPeriodInMonths) * deltaGameTimeInMonths;
        transform.Rotate(selfRotationAxis, selfRotationAngle, Space.Self);
    }
}
