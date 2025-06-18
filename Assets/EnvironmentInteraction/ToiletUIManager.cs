using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ToiletUIManager : MonoBehaviour
{
    public static ToiletUIManager instance;

    [Header("UI Elements")]
    public GameObject toiletUIPanel;
    public Button defecateButton;
    public Button cancelButton;

    [Header("UI Management")]
    public List<GameObject> uisToHideOnToiletUse; // Assign in the inspector

    private InteractableToilet currentToilet;

    private PlayerControllerCode playerController;
    private PlayerInteraction playerInteraction;

    private CameraController cameraController;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        toiletUIPanel.SetActive(false); // Ensure UI is hidden at start

        // Initialize playerController and playerInteraction here
        playerController = PlayerControllerCode.instance;
        if (playerController != null)
        {
            playerInteraction = playerController.GetComponent<PlayerInteraction>();
        }
        else
        {
            Debug.LogError("ToiletUIManager: PlayerControllerCode.instance is null.");
        }

        // Find the CameraController
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("ToiletUIManager: CameraController not found in the scene.");
        }
    }

    private void Start()
    {
        // Button listeners
        defecateButton.onClick.AddListener(OnDefecateButtonClicked);
        cancelButton.onClick.AddListener(CloseToiletUI);
    }

    public void OpenToiletUI(InteractableToilet toilet)
    {
        currentToilet = toilet;
        // Activate the UI panel first
        toiletUIPanel.SetActive(true);

        // Disable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = false;

        // Hide the tooltip
        TooltipManager.instance.HideTooltip();

        // Disable player control
        playerController.DisablePlayerControl();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        UIManager.instance.DisableCharacterUIOpening();

        // NEW: also disable in-game menu opening
        InGameMenuController.instance.DisableMenuOpening();


        // Disable camera control
        if (cameraController != null)
        {
            cameraController.DisableCameraControl();
        }

        // Hide specified UIs
        foreach (var ui in uisToHideOnToiletUse)
        {
            if (ui != null)
                ui.SetActive(false);
        }
    }

    public void CloseToiletUI()
    {
        toiletUIPanel.SetActive(false);
        currentToilet = null;

        // Re-enable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = true;

        // Enable player control
        playerController.EnablePlayerControl();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        UIManager.instance.EnableCharacterUIOpening();
        // NEW: also re-enable in-game menu
        InGameMenuController.instance.EnableMenuOpening();
        // Enable camera control
        if (cameraController != null)
        {
            cameraController.EnableCameraControl();
        }

        // Show specified UIs
        foreach (var ui in uisToHideOnToiletUse)
        {
            if (ui != null)
                ui.SetActive(true);
        }
    }

    private void Update()
    {
        // Close UI if player presses E again
        if (toiletUIPanel.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            CloseToiletUI();
        }
    }

    private void OnDefecateButtonClicked()
    {
        

        // Start the defecation process
        DefecationManager.instance.StartDefecation(currentToilet);

        CloseToiletUI();
        UIManager.instance.DisableCharacterUIOpening();

        // NEW: also disable in-game menu opening
        InGameMenuController.instance.DisableMenuOpening();
    }
}
