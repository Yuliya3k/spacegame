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
    }

    public void CloseMenu()
    {
        if (menuRoot == null)
            return;

        menuRoot.SetActive(false);
        if (InputFreezeManager.instance != null)
        {
            InputFreezeManager.instance.UnfreezePlayerAndCursor();
        }
        currentNPC = null;
    }

    private void OnTalkSelected()
    {
        CloseMenu();
        if (currentNPC != null)
        {
            var dialogueComponent = currentNPC.GetComponent<NPCDialogueComponent>();
            if (dialogueComponent != null && DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(dialogueComponent.startingLine);
            }
        }
    }

    private void OnTradeSelected()
    {
        CloseMenu();
        if (NPCInteractionUIManager.instance != null && currentNPC != null)
        {
            NPCInteractionUIManager.instance.OpenNPCInteraction(currentNPC);
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