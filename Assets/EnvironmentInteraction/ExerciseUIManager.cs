using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ExerciseUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject exerciseUIPanel;              // Panel for choosing exercise hours
    public Slider exerciseSlider;
    public TextMeshProUGUI exerciseHoursText;
    public Button startExerciseButton;
    public Button cancelButton;


    // UI that appears when exercise is in progress, with a "Stop" button
    public GameObject exerciseInProgressUIPanel;    // Panel shown during exercise
    public Button stopExerciseButton;

    [Header("UI Management")]
    public List<GameObject> uisToHideOnExercise; // Assign in the Inspector

    [Header("References")]
    public ExerciseManager exerciseManager; // Assign via the Inspector

    private InteractableExerciseMachine currentExerciseMachine;

    private PlayerControllerCode playerController;
    private PlayerInteraction playerInteraction;

    private void Awake()
    {
        if (exerciseUIPanel == null || exerciseInProgressUIPanel == null)
        {
            if (exerciseUIPanel == null)
                Debug.LogError("ExerciseUIManager: exerciseUIPanel reference is not assigned.");

            if (exerciseInProgressUIPanel == null)
                Debug.LogError("ExerciseUIManager: exerciseInProgressUIPanel reference is not assigned.");

            return;
        }

        exerciseUIPanel.SetActive(false);
        exerciseInProgressUIPanel.SetActive(false);

        exerciseSlider.minValue = 1;
        exerciseSlider.maxValue = 5;
        exerciseSlider.wholeNumbers = true;
        exerciseSlider.onValueChanged.AddListener(OnSliderValueChanged);

        playerController = PlayerControllerCode.instance;
        if (playerController != null)
        {
            playerInteraction = playerController.GetComponent<PlayerInteraction>();
        }
        else
        {
            Debug.LogError("ExerciseUIManager: PlayerControllerCode.instance is null.");
        }
    }

    private void Start()
    {
        // Button listeners
        startExerciseButton.onClick.AddListener(OnStartExerciseButtonClicked);
        cancelButton.onClick.AddListener(CloseExerciseUI);
        stopExerciseButton.onClick.AddListener(OnStopExerciseButtonClicked);
    }

    private void OnSliderValueChanged(float value)
    {
        exerciseHoursText.text = $"Exercise Hours: {value}";
    }

    public void OpenExerciseUI(InteractableExerciseMachine exerciseMachine)
    {
        currentExerciseMachine = exerciseMachine;
        exerciseUIPanel.SetActive(true);
        exerciseInProgressUIPanel.SetActive(false);

        

        exerciseSlider.value = exerciseSlider.minValue;
        OnSliderValueChanged(exerciseSlider.value);

        if (playerController == null)
        {
            playerController = PlayerControllerCode.instance;
            if (playerController == null)
            {
                Debug.LogError("OpenExerciseUI: PlayerControllerCode.instance is null.");
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
        foreach (var ui in uisToHideOnExercise)
        {
            if (ui != null)
                ui.SetActive(false);
        }
    }

    public void CloseExerciseUI()
    {
        exerciseUIPanel.SetActive(false);

        // If we are closing the selection UI, re-enable stuff
        if (playerInteraction != null)
            playerInteraction.enabled = true;

        playerController.EnablePlayerControl();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        foreach (var ui in uisToHideOnExercise)
        {
            if (ui != null)
                ui.SetActive(true);
        }

        currentExerciseMachine = null;
    }

    private void OnStartExerciseButtonClicked()
    {
        int exerciseHours = (int)exerciseSlider.value;
        exerciseUIPanel.SetActive(false);

        // Start the exercise process via ExerciseManager
        if (exerciseManager != null)
        {
            exerciseManager.StartExercise(exerciseHours, currentExerciseMachine);
        }
        else
        {
            Debug.LogError("ExerciseUIManager: exerciseManager reference is not assigned.");
        }
    }

    // Called by ExerciseManager when exercise actually starts
    public void ShowInProgressUI()
    {
        exerciseInProgressUIPanel.SetActive(true);
    }

    private void OnStopExerciseButtonClicked()
    {
        // This tells ExerciseManager to stop early
        if (exerciseManager != null)
        {
            exerciseManager.RequestStopExercise();
        }
    }

    public void CloseInProgressUI()
    {
        exerciseInProgressUIPanel.SetActive(false);
    }

    private void Update()
    {
        // Close UI if player presses E again and if in selection UI
        if (exerciseUIPanel.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            CloseExerciseUI();
        }
    }


    // New method to restore all hidden UIs
    public void RestoreHiddenUI()
    {
        foreach (var ui in uisToHideOnExercise)
        {
            if (ui != null)
                ui.SetActive(true);
        }
    }


}

