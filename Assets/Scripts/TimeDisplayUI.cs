using UnityEngine;
using TMPro; // Make sure to include the TextMeshPro namespace

public class TimeDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI timeText; // Reference to the TextMeshProUGUI component
    private InGameTimeManager timeManager; // Reference to the InGameTimeManager

    void Start()
    {
        // Find any instance of the InGameTimeManager in the scene using the optimized method
        timeManager = Object.FindAnyObjectByType<InGameTimeManager>();

        // Check if timeText is assigned, if not, throw an error
        if (timeText == null)
        {
            Debug.LogError("Time TextMeshProUGUI component not assigned!");
        }

        // Optional: Log an error if InGameTimeManager is not found (for debugging purposes)
        if (timeManager == null)
        {
            Debug.LogError("InGameTimeManager not found in the scene!");
        }
    }

    void Update()
    {
        if (timeManager != null)
        {
            // Get the current in-game time from the InGameTimeManager
            System.DateTime currentTime = timeManager.GetCurrentInGameTime();

            // Format the time display without seconds
            string timeString = currentTime.ToString("yyyy MMM dd, dddd - HH:mm");

            // Set the formatted time string to the TextMeshProUGUI component
            timeText.text = timeString;
        }
    }
}
