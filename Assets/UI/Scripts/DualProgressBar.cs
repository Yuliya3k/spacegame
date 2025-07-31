using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Remove if using legacy Text

[System.Serializable]
public class DualProgressBar : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The background image of the progress bar.")]
    public Image background;

    [Tooltip("The fill image for positive values.")]
    public Image positiveFill;

    [Tooltip("The fill image for negative values.")]
    public Image negativeFill;

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

    [Header("Update Settings")]
    [Tooltip("Interval in seconds between each UI update.")]
    public float updateInterval = 0.5f;

    [Header("Transition Settings")]
    [Tooltip("Speed at which the fill transitions.")]
    public float transitionSpeed = 5f;

    private float lastCurrentValue; // To detect changes
    private float timer;            // Timer to manage update intervals
    private Coroutine transitionCoroutine; // Reference to the active transition coroutine

    void Start()
    {
        // Initial update
        UpdateProgressBar();
        timer = updateInterval;
        
        // Initialize fill amounts to 0
        if (positiveFill != null)
            positiveFill.fillAmount = 0f;
        if (negativeFill != null)
            negativeFill.fillAmount = 0f;

        // Initial update
        UpdateProgressBar();
        timer = updateInterval;
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
            if (currentValueFloat != lastCurrentValue)
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
            // Debug.LogError("MinValueRef or MaxValueRef is not assigned.");
            return;
        }

        // Retrieve min and max values from VariableReferences
        float minValue = minValueRef.GetValue();
        float maxValue = maxValueRef.GetValue();
        float currentVal = currentValue.GetValue();

        // Debug.Log($"UpdateProgressBar called. minValue: {minValue}, maxValue: {maxValue}, currentValue: {currentVal}");

        if (minValue >= maxValue)
        {
            // Debug.LogError("minValue should be less than maxValue.");
            return;
        }

        // Clamp the current value within min and max
        float clampedValue = Mathf.Clamp(currentVal, minValue, maxValue);
        // Debug.Log($"clampedValue: {clampedValue}");

        // Calculate the normalized value (-1 to 1)
        float normalizedValue = 0f;
        if (clampedValue < 0)
        {
            normalizedValue = clampedValue / minValue; // Negative side
        }
        else
        {
            normalizedValue = clampedValue / maxValue; // Positive side
        }
        // Debug.Log($"normalizedValue: {normalizedValue}");

        // Start smooth transition
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(SmoothFillTransition(clampedValue, normalizedValue));

        // Update associated text if assigned
        if (valueText != null)
        {
            string formatString = "F" + Mathf.Clamp(decimalPlaces, 0, 2).ToString();
            valueText.text = clampedValue.ToString(formatString);
        }
    }




    private IEnumerator SmoothFillTransition(float clampedValue, float normalizedValue)
    {
        float targetPositiveFill = 0f;
        float targetNegativeFill = 0f;

        if (clampedValue < 0)
        {
            targetNegativeFill = Mathf.Abs(normalizedValue);
            targetPositiveFill = 0f;
        }
        else
        {
            targetPositiveFill = normalizedValue;
            targetNegativeFill = 0f;
        }

        // Debug.Log($"Starting SmoothFillTransition. targetPositiveFill: {targetPositiveFill}, targetNegativeFill: {targetNegativeFill}");

        while (!Mathf.Approximately(positiveFill.fillAmount, targetPositiveFill) || !Mathf.Approximately(negativeFill.fillAmount, targetNegativeFill))
        {
            positiveFill.fillAmount = Mathf.MoveTowards(positiveFill.fillAmount, targetPositiveFill, transitionSpeed * Time.deltaTime);
            negativeFill.fillAmount = Mathf.MoveTowards(negativeFill.fillAmount, targetNegativeFill, transitionSpeed * Time.deltaTime);
            // Debug.Log($"positiveFill.fillAmount: {positiveFill.fillAmount}, negativeFill.fillAmount: {negativeFill.fillAmount}");
            yield return null;
        }

        // Debug.Log("SmoothFillTransition completed.");
    }

}
