using UnityEngine;

public class NPCSinkTrigger : MonoBehaviour
{
    // This script should be attached to your sink object.
    // Ensure the sink’s collider has "Is Trigger" enabled.
    [Header("Debug")]
    [Tooltip("Enable verbose logging for sink triggers.")]
    public bool verboseLogging = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("NPCSinkTrigger: Trigger entered by " + other.name);
        // Look for the NPCSinkManager component on the colliding object.
        NPCSinkManager sinkManager = other.GetComponent<NPCSinkManager>();
        if (sinkManager != null && !sinkManager.IsPerformingAction())
        {
            sinkManager.StartDrinkingForNPC();
            if (verboseLogging)
            {
                Debug.Log("NPCSinkTrigger: Trigger entered by " + other.name);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Continuously check if an NPC is inside the trigger and not already performing the action.
        NPCSinkManager sinkManager = other.GetComponent<NPCSinkManager>();
        if (sinkManager != null && !sinkManager.IsPerformingAction())
        {
            sinkManager.StartDrinkingForNPC();
            if (verboseLogging)
            {
                Debug.Log("NPCSinkTrigger: Trigger entered by " + other.name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Optional: Reset the NPC's sink manager state if needed when they leave the trigger.
        NPCSinkManager sinkManager = other.GetComponent<NPCSinkManager>();
        if (sinkManager != null)
        {
            // For example, you might want to force the NPC to end the drinking action when they exit.
            // sinkManager.StopCurrentAction();
            if (verboseLogging)
            {
                Debug.Log("NPCSinkTrigger: Trigger entered by " + other.name);
            }
        }
    }
}
