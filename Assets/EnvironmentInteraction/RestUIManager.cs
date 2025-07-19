using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RestUIManager : MonoBehaviour
{
    // Removed singleton pattern
    // public static RestUIManager instance;

    [Header("UI Elements")]
    public GameObject restUIPanel;
    public Slider restSlider;
    public TextMeshProUGUI restHoursText;
    public Button restButton;
    public Button cancelButton;

    [Header("UI Management")]
    public List<GameObject> uisToHideOnRest; // Assign in the Inspector

    [Header("References")]
    public RestManager restManager; // Assign via the Inspector

    private InteractableRestPlace currentRestPlace;

    private PlayerControllerCode playerController;
    private PlayerInteraction playerInteraction;
    private PlayerInputActions inputActions;
    private void Awake()
    {
        // Removed singleton pattern
        // if (instance == null)
        //     instance = this;
        // else
        //     Destroy(gameObject);

        restUIPanel.SetActive(false); // Ensure UI is hidden at start

        // Initialize slider
        restSlider.minValue = 2;
        restSlider.maxValue = 10;
        restSlider.wholeNumbers = true;
        restSlider.onValueChanged.AddListener(OnSliderValueChanged);

        inputActions = new PlayerInputActions();
        inputActions.Player.Interact.performed += OnInteract;

        // Initialize playerController and playerInteraction here
        playerController = PlayerControllerCode.instance;
        if (playerController != null)
        {
            playerInteraction = playerController.GetComponent<PlayerInteraction>();
        }
        else
        {
            Debug.LogError("RestUIManager: PlayerControllerCode.instance is null.");
        }
    }

    private void Start()
    {
        // Button listeners
        restButton.onClick.AddListener(OnRestButtonClicked);
        cancelButton.onClick.AddListener(CloseRestUI);
    }

    private void OnSliderValueChanged(float value)
    {
        restHoursText.text = $"Rest Hours: {value}";
    }

    public void OpenRestUI(InteractableRestPlace restPlace)
    {
        currentRestPlace = restPlace;
        // Activate the UI panel first
        restUIPanel.SetActive(true);

        restSlider.value = restSlider.minValue; // Default rest hours
        OnSliderValueChanged(restSlider.value);

        if (playerController == null)
        {
            playerController = PlayerControllerCode.instance;
            if (playerController == null)
            {
                Debug.LogError("OpenRestUI: PlayerControllerCode.instance is null.");
                return;
            }
        }

        // Disable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = false;

        // Hide the tooltip
        TooltipManager.instance.HideTooltip();

        if (InputFreezeManager.instance != null)
        {
            InputFreezeManager.instance.FreezePlayerAndCursor();
        }

        UIManager.instance.DisableCharacterUIOpening();

        // NEW: also disable in-game menu opening
        InGameMenuController.instance.DisableMenuOpening();


        // Hide specified UIs
        foreach (var ui in uisToHideOnRest)
        {
            if (ui != null)
                ui.SetActive(false);
        }
    }

    public void CloseRestUI()
    {
        restUIPanel.SetActive(false);
        currentRestPlace = null;

        // Re-enable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = true;

        if (InputFreezeManager.instance != null)
        {
            InputFreezeManager.instance.UnfreezePlayerAndCursor();
        }

        // Re-enable opening CharacterUI
        UIManager.instance.EnableCharacterUIOpening();

        // NEW: also re-enable in-game menu
        InGameMenuController.instance.EnableMenuOpening();


        // Show specified UIs
        foreach (var ui in uisToHideOnRest)
        {
            if (ui != null)
                ui.SetActive(true);
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (restUIPanel.activeSelf)
        {
            CloseRestUI();
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void OnRestButtonClicked()
    {
        int restHours = (int)restSlider.value;
        restUIPanel.SetActive(false);

        // Start the rest process
        if (restManager != null)
        {
            restManager.StartRest(restHours, currentRestPlace);
        }
        else
        {
            Debug.LogError("RestUIManager: restManager reference is not assigned.");
        }
    }
}
