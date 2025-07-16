using UnityEngine;

public class InteractableSink : MonoBehaviour, IInteractable
{
    [Header("Sink Interaction Settings")]
    public string actionText = "Press E to use sink";
    public string objectName = "Sink";
    public string locationText = "Bathroom";

    [Header("References")]
    public CharacterStats characterStats; // Assign in the Inspector
    public SinkUIManager sinkUIManager;   // Assign in the Inspector

    public void Interact()
    {
        if (characterStats == null)
        {
            Debug.LogError("CharacterStats reference is not assigned in InteractableSink.");
            return;
        }

        if (sinkUIManager == null)
        {
            Debug.LogError("InteractableSink: sinkUIManager reference is not assigned.");
            return;
        }

        // Open the Sink UI
        sinkUIManager.OpenSinkUI(this);
    }
}
