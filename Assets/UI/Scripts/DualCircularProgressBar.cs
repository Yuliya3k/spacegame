using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Remove if using legacy Text

[System.Serializable]
public class DualCircularProgressBar : MonoBehaviour
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

    [Tooltip("The VariableReference to fetch the divider value from.")]
    public VariableReference dividerValueRef;

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
        // Initialize fill amounts to 0
        if (positiveFill != null)
            positiveFill.fillAmount = 0f;
        if (negativeFill != null)
            negativeFill.fillAmount = 0f;

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

        if (minValueRef != null && maxValueRef != null && dividerValueRef != null)
        {
            float min = minValueRef.GetValue();
            float max = maxValueRef.GetValue();
            float divider = dividerValueRef.GetValue();
            if (divider <= min || divider >= max)
            {
                Debug.LogWarning($"{name}: dividerValueRef ({divider}) should be between minValueRef ({min}) and maxValueRef ({max}). Values will be adjusted at runtime.");
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
        if (minValueRef == null || maxValueRef == null || dividerValueRef == null)
        {
            // Debug.LogError("MinValueRef, MaxValueRef, or DividerValueRef is not assigned.");
            return;
        }

        // Retrieve min, max, divider, and current values from VariableReferences
        float minValue = minValueRef.GetValue();
        float maxValue = maxValueRef.GetValue();
        float dividerValue = dividerValueRef.GetValue();
        float currentVal = currentValue.GetValue();

        // Ensure valid min and max values
        ProgressBarUtils.ValidateMinMax(ref minValue, ref maxValue);

        // Ensure divider is between min and max
        if (dividerValue <= minValue || dividerValue >= maxValue)
        {
            Debug.LogWarning("Invalid dividerValue: Clamping to be within min and max values.");
            dividerValue = Mathf.Clamp(dividerValue, minValue + Mathf.Epsilon, maxValue - Mathf.Epsilon);
        }

        // Clamp the current value within min and max
        float clampedValue = Mathf.Clamp(currentVal, minValue, maxValue);
        

        // Calculate the normalized value (0 to 1) from minValue to maxValue
        float normalizedValue = (clampedValue - minValue) / (maxValue - minValue);
       

        // Calculate the divider position in normalized space
        float dividerPosition = (dividerValue - minValue) / (maxValue - minValue);
        

        // Start smooth transition
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(SmoothFillTransition(normalizedValue, dividerPosition));

        // Update associated text if assigned
        if (valueText != null)
        {
            string formatString = "F" + Mathf.Clamp(decimalPlaces, 0, 2).ToString();
            valueText.text = clampedValue.ToString(formatString);
        }
    }

    private IEnumerator SmoothFillTransition(float normalizedValue, float dividerPosition)
    {
        float targetPositiveFill = 5f;
        float targetNegativeFill = 5f;

        if (normalizedValue <= dividerPosition)
        {
            // Negative fill
            // **Corrected Logic: Invert the normalization for negative values**
            // When normalizedValue is 0 (minValue), fillAmount should be 0.5
            // When normalizedValue approaches dividerPosition, fillAmount approaches 0

            float negativeNormalized = 1f - (normalizedValue / dividerPosition);
            float adjustedNormalizedValue = negativeNormalized * 0.5f;
            targetNegativeFill = adjustedNormalizedValue;
            targetPositiveFill = 0f;

            //Debug.Log($"Negative Fill - normalizedValue: {normalizedValue}, dividerPosition: {dividerPosition}, negativeNormalized: {negativeNormalized}, adjustedNormalizedValue: {adjustedNormalizedValue}");
        }
        else
        {
            // Positive fill
            // Normalized from dividerPosition to 1
            float positiveNormalized = (normalizedValue - dividerPosition) / (1f - dividerPosition);
            float adjustedNormalizedValue = positiveNormalized * 0.5f;
            targetPositiveFill = adjustedNormalizedValue;
            targetNegativeFill = 0f;

            // Debug.Log($"Positive Fill - normalizedValue: {normalizedValue}, dividerPosition: {dividerPosition}, positiveNormalized: {positiveNormalized}, adjustedNormalizedValue: {adjustedNormalizedValue}");
        }

        //Debug.Log($"Starting SmoothFillTransition. targetPositiveFill: {targetPositiveFill}, targetNegativeFill: {targetNegativeFill}");

        while (!Mathf.Approximately(positiveFill.fillAmount, targetPositiveFill) || !Mathf.Approximately(negativeFill.fillAmount, targetNegativeFill))
        {
            positiveFill.fillAmount = Mathf.MoveTowards(positiveFill.fillAmount, targetPositiveFill, transitionSpeed * Time.deltaTime);
            negativeFill.fillAmount = Mathf.MoveTowards(negativeFill.fillAmount, targetNegativeFill, transitionSpeed * Time.deltaTime);
            // Debug.Log($"positiveFill.fillAmount: {positiveFill.fillAmount}, negativeFill.fillAmount: {negativeFill.fillAmount}");
            yield return null;
        }

        //Debug.Log("SmoothFillTransition completed.");
    }
}
