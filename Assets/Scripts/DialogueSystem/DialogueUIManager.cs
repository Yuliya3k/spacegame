//using UnityEngine;

//public class DialogueUIManager : MonoBehaviour
//{
//    public static DialogueUIManager instance;

//    [Header("UI Elements")]
//    public GameObject dialoguePanel;

//    private NPCController currentNPC;

//    private void Awake()
//    {
//        if (instance == null)
//        {
//            instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//            return;
//        }

//        if (dialoguePanel != null)
//        {
//            dialoguePanel.SetActive(false);
//        }
//    }

//    public void OpenDialogue(NPCController npc)
//    {
//        currentNPC = npc;
//        if (dialoguePanel != null)
//        {
//            dialoguePanel.SetActive(true);
//        }
//    }

//    public void CloseDialogue()
//    {
//        if (dialoguePanel != null)
//        {
//            dialoguePanel.SetActive(false);
//        }
//        currentNPC = null;
//    }
//}