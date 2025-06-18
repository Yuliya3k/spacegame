using UnityEngine;
using System;

public class InteractableExerciseMachine : MonoBehaviour
{
    [Header("Exercise Machine Interaction Settings")]
    public string actionText = "Press E to exercise";
    public string objectName = "Exercise Machine";
    public string locationText = "Gym";

    [Header("Exercise Settings")]
    public Transform exercisePosition;       // Assign in the Inspector
    public Transform exerciseCameraPosition; // Assign in the Inspector

    [Header("References")]
    public ExerciseUIManager exerciseUIManager; // Assign via the Inspector

    private InGameTimeManager timeManager;

    private void Start()
    {
        timeManager = FindObjectOfType<InGameTimeManager>();
        if (timeManager == null)
        {
            Debug.LogError("InGameTimeManager not found in the scene.");
        }

        if (exerciseUIManager == null)
        {
            Debug.LogError("InteractableExerciseMachine: exerciseUIManager reference is not assigned.");
        }
    }

    public void Interact()
    {
        // Simply open the exercise UI 
        exerciseUIManager.OpenExerciseUI(this);
    }
}
