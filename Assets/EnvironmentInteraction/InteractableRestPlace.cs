using System;
using UnityEngine;

public class InteractableRestPlace : MonoBehaviour, IInteractable
{
    [Header("Rest Interaction Settings")]
    public string actionText = "Press E to rest";
    public string objectName = "Rest Place";
    public string locationText = "Resting Area";

    [Header("Rest Settings")]
    public Transform restPosition; // Assign in the Inspector
    public Transform restCameraPosition; // Assign in the Inspector
    public Camera restCamera; // Optional dedicated camera
    
    [Header("References")]
    public RestUIManager restUIManager; // Assign via the Inspector

    private InGameTimeManager timeManager;

    private void Start()
    {
        timeManager = FindObjectOfType<InGameTimeManager>();
        if (timeManager == null)
        {
            Debug.LogError("InGameTimeManager not found in the scene.");
        }

        if (restUIManager == null)
        {
            Debug.LogError("InteractableRestPlace: restUIManager reference is not assigned.");
        }
    }

    public void Interact()
    {
        // Check if interaction is allowed based on in-game time
        DateTime currentTime = timeManager.GetCurrentInGameTime();
        int hour = currentTime.Hour;
        // Open the Rest UI
        restUIManager.OpenRestUI(this);

        //// Rest is allowed between 22:00 and 08:00
        //if (hour >= 22 || hour < 8)
        //{
        //    if (restUIManager != null)
        //    {
        //        restUIManager.OpenRestUI(this);
        //    }
        //    else
        //    {
        //        Debug.LogError("InteractableRestPlace: restUIManager reference is not assigned.");
        //    }
        //}
        //else
        //{
        //    // Show a notification that rest is not available
        //    NotificationManager.instance.ShowNotification(
        //        "You can't rest now.",
        //        textColor: Color.white,
        //        backgroundColor: Color.black,
        //        backgroundSprite: null,
        //        startPos: new Vector2(0f, 200f),
        //        endPos: new Vector2(0f, 200f),
        //        moveDur: 2f,
        //        fadeDur: 0.5f,
        //        displayDur: 2f
        //    );
        //}
    }
}
