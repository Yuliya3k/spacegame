using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Remove if using legacy Text

// Enumeration for comparison types
public enum ComparisonType
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterOrEqual,
    LessOrEqual
}

// Serializable class for managing value sources
[System.Serializable]
public class ValueSource
{
    public enum SourceType
    {
        Constant,
        Variable
    }

    [Tooltip("Choose whether to use a constant value or a VariableReference.")]
    public SourceType sourceType = SourceType.Constant;

    [Tooltip("The constant value to use if SourceType is Constant.")]
    public float constantValue = 0f;

    [Tooltip("The VariableReference to use if SourceType is Variable.")]
    public VariableReference variableReference;

    // Method to get the current value based on the source type
    public float GetValue()
    {
        switch (sourceType)
        {
            case SourceType.Constant:
                return constantValue;
            case SourceType.Variable:
                if (variableReference != null)
                    return variableReference.GetValue();
                else
                {
                    Debug.LogWarning("VariableReference is not assigned.");
                    return 0f;
                }
            default:
                return 0f;
        }
    }
}

// Serializable class for managing conditional display of UI elements
[System.Serializable]
public class ConditionDisplay
{
    [Tooltip("The VariableReference to evaluate.")]
    public VariableReference variableReference;

    [Tooltip("The comparison type.")]
    public ComparisonType comparisonType;

    [Tooltip("The value to compare against (constant or variable).")]
    public ValueSource compareValue;

    [Tooltip("The GameObject to show when the condition is met.")]
    public GameObject targetGameObject;

    [Tooltip("The sound to play when the condition is met.")]
    public AudioClip conditionSound; // New field for sound
}

// Serializable class for managing conditional color changes
[System.Serializable]
public class ColorCondition
{
    [Tooltip("The VariableReference to evaluate.")]
    public VariableReference variableReference;

    [Tooltip("The comparison type.")]
    public ComparisonType comparisonType;

    [Tooltip("The value to compare against (constant or variable).")]
    public ValueSource compareValue;

    [Tooltip("The color to apply when the condition is met.")]
    public Color conditionColor;

    [Tooltip("Which fill image to apply the color to.")]
    public Image targetFillImage;

    [Tooltip("The sound to play when the condition is met.")]
    public AudioClip conditionSound;
}

// CircularProgressBar class
[System.Serializable]
public class CircularProgressBar : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The background image of the progress bar.")]
    public Image background;

    [Tooltip("The fill image of the progress bar.")]
    public Image fillImage;

    [Header("Variable References")]
    [Tooltip("The VariableReference to fetch the current value from.")]
    public VariableReference currentValue;

    [Tooltip("The VariableReference to fetch the minimum value from.")]
    public VariableReference minValueRef;

    [Tooltip("The VariableReference to fetch the maximum value from.")]
    public VariableReference maxValueRef;

    [Header("Text Display Settings (Optional)")]
    [Tooltip("The TextMeshProUGUI component to display the current value.")]
    public TextMeshProUGUI valueText; // Use Text if not using TextMeshPro

    [Tooltip("Number of decimal places to display in the associated text.")]
    [Range(0, 2)]
    public int decimalPlaces = 0;

    [Header("Conditional Display Settings")]
    [Tooltip("List of conditions to show or hide UI elements.")]
    public ConditionDisplay[] conditionDisplays;

    [Header("Conditional Color Settings")]
    [Tooltip("List of conditions to change fill image colors.")]
    public ColorCondition[] colorConditions;

    [Header("Sound Settings")]
    [Tooltip("The AudioSource component for playing condition sounds.")]
    public AudioSource audioSource;

    [Header("Update Settings")]
    [Tooltip("Interval in seconds between each UI update.")]
    public float updateInterval = 0.5f;

    [Header("Transition Settings")]
    [Tooltip("Speed at which the fill transitions.")]
    public float transitionSpeed = 5f;

    // Store original colors to reset when conditions are not met
    private Color originalFillColor;

    // Track if sounds have been played for color conditions
    private bool[] colorConditionSoundPlayed;

    // Track if sounds have been played for display conditions
    private bool[] displayConditionSoundPlayed;

    private float lastCurrentValue; // To detect changes
    private float timer;            // Timer to manage update intervals
    private Coroutine transitionCoroutine; // Reference to the active transition coroutine

    void Start()
    {
        // Initialize fill amount to 0
        if (fillImage != null)
            fillImage.fillAmount = 0f;

        // Store the original color of the fill image
        if (fillImage != null)
            originalFillColor = fillImage.color;

        // Initialize conditional displays (hide all initially)
        foreach (var conditionDisplay in conditionDisplays)
        {
            if (conditionDisplay.targetGameObject != null)
                conditionDisplay.targetGameObject.SetActive(false);
        }

        // Initialize sound played flags for color conditions
        if (colorConditions != null && colorConditions.Length > 0)
        {
            colorConditionSoundPlayed = new bool[colorConditions.Length];
        }

        // Initialize sound played flags for display conditions
        if (conditionDisplays != null && conditionDisplays.Length > 0)
        {
            displayConditionSoundPlayed = new bool[conditionDisplays.Length];
        }

        // Initial update
        UpdateProgressBar();
        timer = updateInterval;
    }

    void OnValidate()
    {
        if (minValueRef != null && maxValueRef != null)
        {
            float min = minValueRef.GetValue();
            float max = maxValueRef.GetValue();
            if (min >= max)
            {
                Debug.LogWarning($"{name}: minValueRef ({min}) should be less than maxValueRef ({max}). Values will be adjusted at runtime.");
            }
        }
    }

    void Update()
    {
        // Increment the timer
        timer += Time.deltaTime;

        // Check if it's time to update
        if (timer >= updateInterval)
        {
            timer = 0f; // Reset the timer

            // Get the current value
            float currentValueFloat = currentValue.GetValue();

            // Check if the value has changed
            if (!Mathf.Approximately(currentValueFloat, lastCurrentValue))
            {
                lastCurrentValue = currentValueFloat;
                UpdateProgressBar();
            }
        }
    }

    private void UpdateProgressBar()
    {
        if (minValueRef == null || maxValueRef == null)
        {
            Debug.LogError("MinValueRef or MaxValueRef is not assigned.");
            return;
        }

        // Retrieve min, max, and current values from VariableReferences
        float minValue = minValueRef.GetValue();
        float maxValue = maxValueRef.GetValue();
        float currentVal = currentValue.GetValue();

        // Ensure valid min and max values and compute normalized progress
        float normalizedValue = ProgressBarUtils.Normalize(currentVal, ref minValue, ref maxValue, out float clampedValue);

        // Start smooth transition
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(SmoothFillTransition(normalizedValue));

        // Update associated text if assigned
        if (valueText != null)
        {
            string formatString = "F" + Mathf.Clamp(decimalPlaces, 0, 2).ToString();
            valueText.text = clampedValue.ToString(formatString);
        }

        // Evaluate conditions for displaying additional graphics
        EvaluateConditionDisplays();

        // Evaluate conditions for changing fill colors
        EvaluateColorConditions();
    }

    private IEnumerator SmoothFillTransition(float targetFillAmount)
    {
        //Debug.Log($"Starting SmoothFillTransition. targetFillAmount: {targetFillAmount}");

        while (!Mathf.Approximately(fillImage.fillAmount, targetFillAmount))
        {
            fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, targetFillAmount, transitionSpeed * Time.deltaTime);
            //Debug.Log($"fillImage.fillAmount: {fillImage.fillAmount}");
            yield return null;
        }

        //Debug.Log("SmoothFillTransition completed.");
    }

    // Method to evaluate and handle conditional display of UI elements
    private void EvaluateConditionDisplays()
    {
        for (int i = 0; i < conditionDisplays.Length; i++)
        {
            var condition = conditionDisplays[i];
            bool conditionMet = EvaluateCondition(condition.variableReference, condition.comparisonType, condition.compareValue.GetValue());

            if (condition.targetGameObject != null)
            {
                bool isActive = condition.targetGameObject.activeSelf;
                condition.targetGameObject.SetActive(conditionMet);

                // Check if the state has changed to play sound
                if (conditionMet && !isActive)
                {
                    // Play sound if not already played
                    if (condition.conditionSound != null && audioSource != null && !displayConditionSoundPlayed[i])
                    {
                        audioSource.PlayOneShot(condition.conditionSound);
                        displayConditionSoundPlayed[i] = true;
                    }
                }
                else if (!conditionMet && isActive)
                {
                    // Reset the sound played flag when condition is no longer met
                    if (displayConditionSoundPlayed != null && displayConditionSoundPlayed.Length > i)
                    {
                        displayConditionSoundPlayed[i] = false;
                    }
                }
            }
        }
    }

    // Method to evaluate and handle conditional color changes and play sounds
    private void EvaluateColorConditions()
    {
        if (colorConditions == null || colorConditions.Length == 0)
            return;

        for (int i = 0; i < colorConditions.Length; i++)
        {
            var colorCondition = colorConditions[i];
            bool conditionMet = EvaluateCondition(colorCondition.variableReference, colorCondition.comparisonType, colorCondition.compareValue.GetValue());

            if (colorCondition.targetFillImage != null)
            {
                if (conditionMet)
                {
                    // Ensure the conditionColor has full opacity
                    Color adjustedColor = new Color(colorCondition.conditionColor.r, colorCondition.conditionColor.g, colorCondition.conditionColor.b, 1f);
                    colorCondition.targetFillImage.color = adjustedColor;

                    // Play sound if not already played
                    if (colorCondition.conditionSound != null && audioSource != null && !colorConditionSoundPlayed[i])
                    {
                        audioSource.PlayOneShot(colorCondition.conditionSound);
                        colorConditionSoundPlayed[i] = true;
                    }
                }
                else
                {
                    // Reset to original color
                    colorCondition.targetFillImage.color = originalFillColor;

                    // Reset the sound played flag
                    if (colorConditionSoundPlayed != null && colorConditionSoundPlayed.Length > i)
                    {
                        colorConditionSoundPlayed[i] = false;
                    }
                }
            }
        }
    }

    // Helper method to evaluate a condition based on VariableReference, ComparisonType, and compareValue
    private bool EvaluateCondition(VariableReference variableRef, ComparisonType comparison, float compareValue)
    {
        if (variableRef == null)
        {
            Debug.LogWarning("VariableReference is not assigned.");
            return false;
        }

        float variableValue = variableRef.GetValue();

        switch (comparison)
        {
            case ComparisonType.Equal:
                return Mathf.Approximately(variableValue, compareValue);
            case ComparisonType.NotEqual:
                return !Mathf.Approximately(variableValue, compareValue);
            case ComparisonType.GreaterThan:
                return variableValue > compareValue;
            case ComparisonType.LessThan:
                return variableValue < compareValue;
            case ComparisonType.GreaterOrEqual:
                return variableValue >= compareValue || Mathf.Approximately(variableValue, compareValue);
            case ComparisonType.LessOrEqual:
                return variableValue <= compareValue || Mathf.Approximately(variableValue, compareValue);
            default:
                Debug.LogWarning("Unknown ComparisonType.");
                return false;
        }
    }
}
