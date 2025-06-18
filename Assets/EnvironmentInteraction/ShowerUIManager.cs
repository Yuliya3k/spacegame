using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShowerUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject showerUIPanel;      // Panel for choosing shower actions
    public Button takeShowerButton;
    public Button rinseBodyButton;
    public Button cancelButton;

    // UI that appears when shower is in progress, with a "Stop" button
    public GameObject actionUIPanel;
    public Button stopButton;

    [Header("UI Management")]
    public List<GameObject> uisToHideOnShowerUse; // Assign in the Inspector

    [Header("References")]
    public ShowerManager showerManager; // Assign via the Inspector

    private InteractableShower currentShower;

    private PlayerControllerCode playerController;
    private PlayerInteraction playerInteraction;
    private CameraController cameraController;

    private void Awake()
    {
        showerUIPanel.SetActive(false);
        actionUIPanel.SetActive(false);

        playerController = PlayerControllerCode.instance;
        if (playerController != null)
        {
            playerInteraction = playerController.GetComponent<PlayerInteraction>();
        }
        else
        {
            Debug.LogError("ShowerUIManager: PlayerControllerCode.instance is null.");
        }

        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("ShowerUIManager: CameraController not found in the scene.");
        }
    }

    private void Start()
    {
        takeShowerButton.onClick.AddListener(OnTakeShowerButtonClicked);
        rinseBodyButton.onClick.AddListener(OnRinseBodyButtonClicked);
        cancelButton.onClick.AddListener(CloseShowerUI);

        stopButton.onClick.AddListener(OnStopButtonClicked);
    }

    public void OpenShowerUI(InteractableShower shower)
    {
        currentShower = shower;
        showerUIPanel.SetActive(true);

        // Disable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = false;

        // Hide tooltip if needed
        TooltipManager.instance.HideTooltip();

        // Disable player control and enable cursor
        playerController.DisablePlayerControl();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Disable camera control
        cameraController.DisableCameraControl();

        // Hide specified UIs
        foreach (var ui in uisToHideOnShowerUse)
        {
            if (ui != null)
                ui.SetActive(false);
        }
    }

    public void CloseShowerUI()
    {
        showerUIPanel.SetActive(false);
        currentShower = null;

        // Re-enable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = true;

        // Enable player control and lock cursor
        playerController.EnablePlayerControl();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Enable camera control
        cameraController.EnableCameraControl();

        // Restore hidden UI
        foreach (var ui in uisToHideOnShowerUse)
        {
            if (ui != null)
                ui.SetActive(true);
        }
    }

    private void OnTakeShowerButtonClicked()
    {
        if (showerManager != null)
        {
            showerManager.StartTakeShower(currentShower);
        }
        else
        {
            Debug.LogError("ShowerUIManager: showerManager reference is not assigned.");
        }

        showerUIPanel.SetActive(false);
        actionUIPanel.SetActive(true);
    }

    private void OnRinseBodyButtonClicked()
    {
        if (showerManager != null)
        {
            showerManager.StartRinseBody(currentShower);
        }
        else
        {
            Debug.LogError("ShowerUIManager: showerManager reference is not assigned.");
        }

        showerUIPanel.SetActive(false);
        actionUIPanel.SetActive(true);
    }

    public void OnStopButtonClicked()
    {
        // Stop current action in showerManager
        if (showerManager != null)
        {
            showerManager.StopCurrentAction();
        }

        actionUIPanel.SetActive(false);

        // Re-enable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = true;

        // Enable player control and lock cursor
        playerController.EnablePlayerControl();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Enable camera control
        cameraController.EnableCameraControl();

        // Restore hidden UI
        foreach (var ui in uisToHideOnShowerUse)
        {
            if (ui != null)
                ui.SetActive(true);
        }
    }
}
