using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RadialMenuUI : MonoBehaviour
{
    public static RadialMenuUI instance;

    [Header("UI Elements")]
    public GameObject menuRoot;
    public Button talkButton;
    public Button tradeButton;

    [Header("Camera Settings")]
    [SerializeField] private int cameraProfileIndex = 2;

    [Header("Game Screen UI Panels")]
    public List<GameObject> gameScreenUIs;  // Register UIs to be hidden when opening the radial menu

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
        HideGameScreenUIs();
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
        ShowGameScreenUIs();
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

    private void HideGameScreenUIs()
    {
        foreach (var ui in gameScreenUIs)
        {
            if (ui.activeSelf) ui.SetActive(false);
        }
    }

    private void ShowGameScreenUIs()
    {
        foreach (var ui in gameScreenUIs)
        {
            if (!ui.activeSelf) ui.SetActive(true);
        }
    }

    private void OnTalkSelected()
    {
        var npc = currentNPC;
        // Unfreeze player controls before starting dialogue so that
        // DialogueManager can manage freezing independently.
        // This prevents the player from remaining frozen after the dialogue ends.
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