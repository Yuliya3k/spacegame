using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
    // The desired frame rate. Set this to the maximum frame rate you want.
    public int targetFrameRate = 60;

    void Start()
    {
        // Set the target frame rate for the game
        Application.targetFrameRate = targetFrameRate;

        // Optionally, you can disable VSync to ensure the frame rate is precisely limited
        QualitySettings.vSyncCount = 0;  // Disable VSync if you want precise control
    }

    void Update()
    {
        // Optional: Dynamically change frame rate during gameplay (if needed)
        // Application.targetFrameRate = targetFrameRate;
    }
}
