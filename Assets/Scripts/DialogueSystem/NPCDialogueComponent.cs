using UnityEngine;

public class NPCDialogueComponent : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public string objectName = "NPC";
    public string locationText = "";
    public string actionText = "Press E to talk";
    public DialogueLine startingLine;

    public void Interact()
    {
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(startingLine);
        }
    }
}