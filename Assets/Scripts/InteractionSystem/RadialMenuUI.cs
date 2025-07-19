using UnityEngine;
using UnityEngine.UI;

public class RadialMenuUI : MonoBehaviour
{
    public static RadialMenuUI instance;

    [Header("UI Elements")]
    public GameObject menuRoot;
    public Button talkButton;
    public Button tradeButton;

    private NPCController currentNPC;

    public bool IsOpen => menuRoot != null && menuRoot.activeSelf;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }

        if (talkButton != null)
            talkButton.onClick.AddListener(OnTalkSelected);
        if (tradeButton != null)
            tradeButton.onClick.AddListener(OnTradeSelected);
    }

    public void OpenMenu(NPCController npc)
    {
        if (menuRoot == null)
            return;

        currentNPC = npc;
        
        menuRoot.SetActive(true);
        if (InputFreezeManager.instance != null)
        {
            InputFreezeManager.instance.FreezePlayerAndCursor();
        }
        if (currentNPC != null)
        {
            currentNPC.Freeze();
        }

    }

    public void CloseMenu(bool keepCursorFrozen = false)
    {
        if (menuRoot == null)
            return;

        menuRoot.SetActive(false);
        if (currentNPC != null)
        {
            currentNPC.Unfreeze();
        }
        if (!keepCursorFrozen && InputFreezeManager.instance != null)
        {
            InputFreezeManager.instance.UnfreezePlayerAndCursor();
        }
        currentNPC = null;
    }

    private void OnTalkSelected()
    {
        var npc = currentNPC;
        CloseMenu(true);
        if (npc != null)
        {
            var dialogueComponent = npc.GetComponent<NPCDialogueComponent>();
            if (dialogueComponent != null && DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(dialogueComponent.startingLine);
            }
        }
    }

    private void OnTradeSelected()
    {
        NPCController npcToTrade = currentNPC;
        CloseMenu(true);
        if (NPCInteractionUIManager.instance != null && npcToTrade != null)
        {
            NPCInteractionUIManager.instance.OpenNPCInteraction(npcToTrade);
        }
    }

    private void DisablePlayerControl()
    {
        var playerController = FindObjectOfType<PlayerControllerCode>();
        if (playerController != null)
        {
            playerController.DisablePlayerControl();
        }
    }

    private void EnablePlayerControl()
    {
        var playerController = FindObjectOfType<PlayerControllerCode>();
        if (playerController != null)
        {
            playerController.EnablePlayerControl();
        }
    }
}