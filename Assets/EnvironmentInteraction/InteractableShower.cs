using UnityEngine;

public class InteractableShower : MonoBehaviour
{
    [Header("Shower Interaction Settings")]
    public string actionText = "Press E to use shower";
    public string objectName = "Shower";
    public string locationText = "Bathroom";

    [Header("References")]
    public CharacterStats characterStats; // Assign in the Inspector
    public ShowerUIManager showerUIManager;   // Assign in the Inspector

    public void Interact()
    {
        if (characterStats == null)
        {
            Debug.LogError("CharacterStats reference is not assigned in InteractableShower.");
            return;
        }

        if (showerUIManager == null)
        {
            Debug.LogError("InteractableShower: showerUIManager reference is not assigned.");
            return;
        }

        // Open the Shower UI
        showerUIManager.OpenShowerUI(this);
    }
}
