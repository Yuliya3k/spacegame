using System;
using UnityEngine;

public class InteractableBed : MonoBehaviour, IInteractable
{
    [Header("Bed Interaction Settings")]
    public string actionText = "Press E to sleep";
    public string objectName = "Bed";
    public string locationText = "Bedroom";

    [Header("Sleep Settings")]
    public Transform sleepPosition; // Assign in the inspector
    public Transform bedCameraPosition; // Assign in the inspector
    public Camera bedCamera; // Camera used when the player sleeps
    private InGameTimeManager timeManager;

    private void Start()
    {
        timeManager = FindObjectOfType<InGameTimeManager>();
        if (timeManager == null)
        {
            Debug.LogError("InGameTimeManager not found in the scene.");
        }
        // Ensure the bed camera starts disabled and the position reference is valid
        if (bedCamera != null)
        {
            bedCamera.enabled = false;
            if (bedCameraPosition == null)
            {
                bedCameraPosition = bedCamera.transform;
            }
        }
    }

    public void Interact()
    {
        // Check if interaction is allowed based on in-game time
        DateTime currentTime = timeManager.GetCurrentInGameTime();
        int hour = currentTime.Hour;

        // Sleep is allowed between 22:00 and 08:00
        if (hour >= 22 || hour < 8)
        {
            // Open the Sleep UI
            SleepUIManager.instance.OpenSleepUI(this);
        }
        else
        {
            // Show a notification that sleep is not available
            NotificationManager.instance.ShowNotification(
                "You can't sleep now.",
                textColor: new Color(255f, 255f, 255f, 255f),
                backgroundColor: new Color(255f, 255f, 255f, 255f),
                backgroundSprite: null, // Assign a Sprite if you have one
                startPos: new Vector2(0f, 200f), // Starting position
                endPos: new Vector2(0f, 200f),      // Ending position (center)
                2f, // Move duration
                0.5f, // Fade duration
                2f // Display duration
            );
        }
    }
}
