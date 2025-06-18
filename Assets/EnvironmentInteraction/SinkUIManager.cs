using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SinkUIManager : MonoBehaviour
{
    // Removed the singleton instance
    // public static SinkUIManager instance;

    [Header("UI Elements")]
    public GameObject sinkUIPanel;        // Main sink UI with options
    public Button washHandsButton;
    public Button drinkButton;
    public Button cancelButton;

    public GameObject actionUIPanel;      // UI panel with the "Stop" button
    public Button stopButton;

    [Header("UI Management")]
    public List<GameObject> uisToHideOnSinkUse; // Assign in the Inspector

    [Header("References")]
    public SinkManager sinkManager; // Assign in the Inspector

    private InteractableSink currentSink;

    private PlayerControllerCode playerController;
    private PlayerInteraction playerInteraction;

    private CameraController cameraController;

    private void Awake()
    {
        // Removed singleton pattern
        // if (instance == null)
        //     instance = this;
        // else
        //     Destroy(gameObject);

        sinkUIPanel.SetActive(false); // Ensure UI is hidden at start
        actionUIPanel.SetActive(false);

        // Initialize playerController and playerInteraction
        playerController = PlayerControllerCode.instance;
        if (playerController != null)
        {
            playerInteraction = playerController.GetComponent<PlayerInteraction>();
        }
        else
        {
            Debug.LogError("SinkUIManager: PlayerControllerCode.instance is null.");
        }

        // Find the CameraController
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("SinkUIManager: CameraController not found in the scene.");
        }
    }

    private void Start()
    {
        // Button listeners
        washHandsButton.onClick.AddListener(OnWashHandsButtonClicked);
        drinkButton.onClick.AddListener(OnDrinkButtonClicked);
        cancelButton.onClick.AddListener(CloseSinkUI);

        stopButton.onClick.AddListener(OnStopButtonClicked);
    }

    public void OpenSinkUI(InteractableSink sink)
    {
        // Disable opening CharacterUI
        UIManager.instance.DisableCharacterUIOpening();

        // NEW: also disable in-game menu opening
        InGameMenuController.instance.DisableMenuOpening();

        currentSink = sink;
        // Activate the UI panel
        sinkUIPanel.SetActive(true);

        // Disable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = false;

        // Hide the tooltip (if you have a TooltipManager)
        TooltipManager.instance.HideTooltip();

        // Disable player control
        playerController.DisablePlayerControl();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Disable camera control
        if (cameraController != null)
        {
            cameraController.DisableCameraControl();
        }

        // Hide specified UIs
        foreach (var ui in uisToHideOnSinkUse)
        {
            if (ui != null)
                ui.SetActive(false);
        }
    }

    public void CloseSinkUI()
    {
        sinkUIPanel.SetActive(false);
        currentSink = null;

        // Re-enable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = true;

        // Enable player control
        playerController.EnablePlayerControl();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Re-enable opening CharacterUI
        UIManager.instance.EnableCharacterUIOpening();

        // NEW: also re-enable in-game menu
        InGameMenuController.instance.EnableMenuOpening();

        // Enable camera control
        if (cameraController != null)
        {
            cameraController.EnableCameraControl();
        }

        // Show specified UIs
        foreach (var ui in uisToHideOnSinkUse)
        {
            if (ui != null)
                ui.SetActive(true);
        }
    }

    private void OnWashHandsButtonClicked()
    {
        // Start the wash hands process
        if (sinkManager != null)
        {
            sinkManager.StartWashHands(currentSink);
        }
        else
        {
            Debug.LogError("SinkUIManager: sinkManager reference is not assigned.");
        }

        sinkUIPanel.SetActive(false);
        actionUIPanel.SetActive(true);
    }

    private void OnDrinkButtonClicked()
    {
        // Start the drink from sink process
        if (sinkManager != null)
        {
            sinkManager.StartDrinking(currentSink);
        }
        else
        {
            Debug.LogError("SinkUIManager: sinkManager reference is not assigned.");
        }

        sinkUIPanel.SetActive(false);
        actionUIPanel.SetActive(true);
    }

    public void OnStopButtonClicked()
    {
        // Stop the current action
        if (sinkManager != null)
        {
            sinkManager.StopCurrentAction();
        }
        else
        {
            Debug.LogError("SinkUIManager: sinkManager reference is not assigned.");
        }

        actionUIPanel.SetActive(false);

        // Re-enable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = true;

        // Enable player control
        playerController.EnablePlayerControl();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Enable camera control
        if (cameraController != null)
        {
            cameraController.EnableCameraControl();
        }

        // Show specified UIs
        foreach (var ui in uisToHideOnSinkUse)
        {
            if (ui != null)
                ui.SetActive(true);
        }
    }
}
