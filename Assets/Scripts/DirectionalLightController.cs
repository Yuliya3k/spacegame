using UnityEngine;

public class DirectionalLightController : MonoBehaviour
{
    [Header("Light Settings")]
    public Transform orbitCenter;          // Point towards which the light always points
    public float orbitPeriodInMonths = 3f; // Time (in game months) for one full rotation
    public Vector3 orbitAxis = Vector3.up; // Axis of rotation

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
            Debug.LogError("Orbit Center is not assigned in DirectionalLightController script.");
        }
    }

    private void Update()
    {
        if (timeManager == null || orbitCenter == null) return;

        // Calculate the delta game time in seconds
        float deltaGameTimeInSeconds = Time.deltaTime * timeManager.timeMultiplier * timeManager.timeScale;

        // Convert delta game time to months
        float deltaGameTimeInMonths = deltaGameTimeInSeconds / (3600f * 24f * 30f);

        // Calculate rotation angle
        float rotationAngle = (360f / orbitPeriodInMonths) * deltaGameTimeInMonths;

        // Rotate the light around the orbit center
        transform.RotateAround(orbitCenter.position, orbitAxis, rotationAngle);

        // Always point the light towards the orbit center
        Vector3 directionToCenter = orbitCenter.position - transform.position;
        transform.rotation = Quaternion.LookRotation(directionToCenter);
    }
}
