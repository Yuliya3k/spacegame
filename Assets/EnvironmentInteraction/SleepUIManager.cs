using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SleepUIManager : MonoBehaviour
{
    public static SleepUIManager instance;

    [Header("UI Elements")]
    public GameObject sleepUIPanel;
    public Slider sleepSlider;
    public TextMeshProUGUI sleepHoursText;
    public Button sleepButton;
    public Button cancelButton;

    [Header("UI Management")]
    public List<GameObject> uisToHideOnSleep; // Assign in the inspector


    private InteractableBed currentBed;

    private PlayerControllerCode playerController;
    private PlayerInteraction playerInteraction;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        sleepUIPanel.SetActive(false); // Ensure UI is hidden at start

        // Initialize slider
        sleepSlider.minValue = 2;
        sleepSlider.maxValue = 10;
        sleepSlider.wholeNumbers = true;
        sleepSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // Initialize playerController and playerInteraction here
        playerController = PlayerControllerCode.instance;
        if (playerController != null)
        {
            playerInteraction = playerController.GetComponent<PlayerInteraction>();
        }
        else
        {
            Debug.LogError("SleepUIManager: PlayerControllerCode.instance is null. Ensure the PlayerControllerCode script sets its instance correctly.");
        }
    }

    private void Start()
    {
        


        // Get references to PlayerController and PlayerInteraction
        //playerController = PlayerControllerCode.instance;
        //playerInteraction = playerController.GetComponent<PlayerInteraction>();


        // Button listeners
        sleepButton.onClick.AddListener(OnSleepButtonClicked);
        cancelButton.onClick.AddListener(CloseSleepUI);
    }

    private void OnSliderValueChanged(float value)
    {
        sleepHoursText.text = $"Sleep Hours: {value}";
    }

    public void OpenSleepUI(InteractableBed bed)
    {
        // Disable opening CharacterUI
        UIManager.instance.DisableCharacterUIOpening();

        // NEW: also disable in-game menu opening
        InGameMenuController.instance.DisableMenuOpening();

        currentBed = bed;
        // Activate the UI panel first
        sleepUIPanel.SetActive(true);

        sleepSlider.value = sleepSlider.minValue; // Default sleep hours
        OnSliderValueChanged(sleepSlider.value);


        if (playerController == null)
        {
            playerController = PlayerControllerCode.instance;
            if (playerController == null)
            {
                Debug.LogError("OpenSleepUI: PlayerControllerCode.instance is null.");
                return;
            }
        }

        // Disable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = false;

        // Hide the tooltip
        TooltipManager.instance.HideTooltip();

        // Disable player control
        playerController.DisablePlayerControl();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Hide specified UIs
        foreach (var ui in uisToHideOnSleep)
        {
            if (ui != null)
                ui.SetActive(false);
        }
    }

    public void CloseSleepUI()
    {
        sleepUIPanel.SetActive(false);
        currentBed = null;

        // Re-enable PlayerInteraction
        if (playerInteraction != null)
            playerInteraction.enabled = true;

        // Enable player control
        playerController.EnablePlayerControl();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        // NEW: also re-enable in-game menu
        InGameMenuController.instance.EnableMenuOpening();

        // Show specified UIs
        foreach (var ui in uisToHideOnSleep)
        {
            if (ui != null)
                ui.SetActive(true);
        }
    }

    private void Update()
    {
        //if (playerInteraction != null)
        //    playerInteraction.enabled = false;
        // Close UI if player presses E again
        //if (sleepUIPanel.activeSelf && Input.GetKeyDown(KeyCode.E))
        //{
        //    CloseSleepUI();
        //}
    }


    private void OnSleepButtonClicked()
    {
        int sleepHours = (int)sleepSlider.value;
        sleepUIPanel.SetActive(false);

        // Start the sleep process
        SleepManager.instance.StartSleep(sleepHours, currentBed);
    }

}
