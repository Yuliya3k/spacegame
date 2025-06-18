using UnityEngine;

public class InteractableToilet : MonoBehaviour
{
    [Header("Toilet Interaction Settings")]
    public string actionText = "Press E to use toilet";
    public string objectName = "Toilet";
    public string locationText = "Bathroom";

    [Header("Defecation Settings")]
    public Transform sitPosition;       // Assign in the Inspector
    public Transform toiletCameraPosition; // Assign in the Inspector

    [Header("References")]
    public CharacterStats characterStats; // Assign in the Inspector

    public void Interact()
    {
        if (characterStats == null)
        {
            Debug.LogError("CharacterStats reference is not assigned in InteractableToilet.");
            return;
        }

        // Check if the player needs to defecate based on intestines fullness
        if (characterStats.currentIntFullness >= characterStats.intestinesCapacity * 0.2f)
        {
            // Open the Toilet UI
            ToiletUIManager.instance.OpenToiletUI(this);
        }
        else
        {
            // Show a notification that defecation is not necessary
            NotificationManager.instance.ShowNotification(
                "You don't feel the need to use the toilet right now.",
                textColor: Color.red,
                backgroundColor: Color.white,
                backgroundSprite: null,
                startPos: new Vector2(0f, 200f),
                endPos: new Vector2(0f, 200f),
                moveDur: 2f,
                fadeDur: 0.5f,
                displayDur: 2f
            );
        }
    }
}
