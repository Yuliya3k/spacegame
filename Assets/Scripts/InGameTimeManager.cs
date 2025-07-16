using UnityEngine;
using System;

public class InGameTimeManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float timeMultiplier = 20f;  // Base speed up time (e.g., 20x real-time)
    [Range(0f, 10f)]
    public float timeScale = 1f;        // Additional dynamic time scaling (e.g., for sleep)
    private bool isTimePaused = false;   // Flag to pause time progression

    [Header("Time Persistence")]
    private const string START_TIME_KEY = "startTime"; // Key for saving start time in PlayerPrefs
    private DateTime inGameDateTime;                   // In-game date and time

    void Start()
    {
        // Initialize in-game time based on saved data or current real time
        if (PlayerPrefs.HasKey(START_TIME_KEY))
        {
            // Retrieve saved start time
            string savedStartTime = PlayerPrefs.GetString(START_TIME_KEY);
            if (DateTime.TryParse(savedStartTime, out DateTime parsedTime))
            {
                inGameDateTime = parsedTime;
            }
            else
            {
                Debug.LogWarning("Failed to parse saved start time. Initializing with current real time.");
                inGameDateTime = DateTime.Now;
                PlayerPrefs.SetString(START_TIME_KEY, inGameDateTime.ToString());
                PlayerPrefs.Save();
            }
        }
        else
        {
            // First time game is started; initialize with current real time
            inGameDateTime = DateTime.Now;
            PlayerPrefs.SetString(START_TIME_KEY, inGameDateTime.ToString());
            PlayerPrefs.Save();
        }
    }

    void Update()
    {
        if (!isTimePaused)
        {
            // Calculate time to add based on timeMultiplier and timeScale
            double secondsToAdd = Time.deltaTime * timeMultiplier * timeScale;
            inGameDateTime = inGameDateTime.AddSeconds(secondsToAdd);
        }

        // Optional: Update the saved start time periodically or upon certain events
        // This ensures time progression is persistent across game sessions
    }

    // Public method to get the current in-game time
    public DateTime GetCurrentInGameTime()
    {
        return inGameDateTime;
    }

    // Methods to get real-time durations corresponding to in-game minutes and hours
    public float GetRealTimeDurationForGameMinutes(float inGameMinutes)
    {
        return (inGameMinutes * 60f) / (timeMultiplier * timeScale);
    }

    public float GetRealTimeDurationForGameHours(float inGameHours)
    {
        return (inGameHours * 60f) / (timeMultiplier * timeScale);
    }

    // Overloads for integer parameters
    public float GetRealTimeDurationForGameMinutes(int inGameMinutes)
    {
        return (inGameMinutes * 60) / (timeMultiplier * timeScale);
    }

    public float GetRealTimeDurationForGameHours(int inGameHours)
    {
        return (inGameHours * 3600f) / (timeMultiplier * timeScale);
    }

    // Method to set a new time scale
    public void SetTimeScale(float newScale)
    {
        timeScale = Mathf.Clamp(newScale, 0f, 10f); // Clamp to prevent extreme scaling
    }

    // Methods to pause and resume time progression
    public void PauseTime()
    {
        isTimePaused = true;
    }

    public void ResumeTime()
    {
        isTimePaused = false;
    }

    // Optional: Method to reset in-game time (useful for testing)
    public void ResetInGameTime()
    {
        inGameDateTime = DateTime.Now;
        PlayerPrefs.SetString(START_TIME_KEY, inGameDateTime.ToString());
        PlayerPrefs.Save();
    }


    public void SetInGameTime(DateTime dateTime)
    {
        inGameDateTime = dateTime;
    }


    //void FixedUpdate()
    //{
    //    // Use fixed time step instead of deltaTime
    //    double secondsToAdd = Time.fixedDeltaTime * timeMultiplier * timeScale;
    //    inGameDateTime = inGameDateTime.AddSeconds(secondsToAdd);
    //}



}
