using UnityEngine;
using UnityEngine.UI;

public class RadialMenuUI : MonoBehaviour
{
    public static RadialMenuUI instance;

    [Header("UI Elements")]
    public GameObject menuRoot;
    public Button talkButton;
    public Button tradeButton;

    [Header("Camera Settings")]
    [SerializeField] private int cameraProfileIndex = 2;

    private int previousCameraProfileIndex = -1;

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
        
        var cameraController = CinemachineCameraController.instance;
        if (cameraController != null)
        {
            previousCameraProfileIndex = cameraController.CurrentProfileIndex;
            cameraController.SwitchToProfile(cameraProfileIndex);
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
        var cameraController = CinemachineCameraController.instance;
        if (previousCameraProfileIndex != -1 && cameraController != null)
        {
            cameraController.SwitchToProfile(previousCameraProfileIndex);
            previousCameraProfileIndex = -1;
        }
        currentNPC = null;
    }

    private void OnTalkSelected()
    {
        var npc = currentNPC;
        CloseMenu(true);

        if (npc != null)
        {
            npc.Freeze();
            npc.SetInteractionAnimation(npc.talkAnimationTrigger);
            var dialogueComponent = npc.GetComponent<NPCDialogueComponent>();
            if (dialogueComponent != null && DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(dialogueComponent.startingLine, npc);
            }
        }
        // if (InputFreezeManager.instance != null)
        // {
        //     InputFreezeManager.instance.UnfreezePlayerAndCursor();
        // }
        
    }

    private void OnTradeSelected()
    {
        NPCController npcToTrade = currentNPC;
        CloseMenu(true);
        if (npcToTrade != null)
        {
            npcToTrade.Freeze();
            npcToTrade.SetInteractionAnimation(npcToTrade.tradeAnimationTrigger);
        }
        if (NPCInteractionUIManager.instance != null &&
            NPCInteractionUIManager.instance.isActiveAndEnabled &&
            npcToTrade != null)
        {
            NPCInteractionUIManager.instance.OpenNPCInteraction(npcToTrade);
        }

        // if (InputFreezeManager.instance != null)
        // {
        //     InputFreezeManager.instance.UnfreezePlayerAndCursor();
        // }
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