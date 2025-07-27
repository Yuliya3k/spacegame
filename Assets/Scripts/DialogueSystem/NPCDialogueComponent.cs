using UnityEngine;

public class NPCDialogueComponent : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public string objectName = "NPC";
    public string locationText = "";
    public string actionText = "Press E to talk";
    public Sprite npcIcon;
    public DialogueLine startingLine;

    public void Interact()
    {
        if (DialogueManager.instance != null)
        {
            var npc = GetComponent<NPCController>();
            DialogueManager.instance.StartDialogue(startingLine, npc);
        }
    }
}