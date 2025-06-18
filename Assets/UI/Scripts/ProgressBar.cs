using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("UI Elements")]
    public Image background;   // Background image for the progress bar
    public Image fillImage;    // Fill image that will scale based on the progress

    [Header("Variable References")]
    public VariableReference currentValue;
    public VariableReference maxValue;

    private float lastCurrentValue;        // Stored current value to detect changes
    private float lastMaxValue;            // Stored max value to detect changes

    private void Start()
    {
        UpdateProgressBar();
    }

    private void Update()
    {
        // Get the current and max values
        float currentValueFloat = currentValue.GetValue();
        float maxValueFloat = maxValue.GetValue();

        // Check if values have changed
        if (currentValueFloat != lastCurrentValue || maxValueFloat != lastMaxValue)
        {
            lastCurrentValue = currentValueFloat;
            lastMaxValue = maxValueFloat;

            UpdateProgressBar();
        }
    }

    private void UpdateProgressBar()
    {
        float currentValueFloat = currentValue.GetValue();
        float maxValueFloat = maxValue.GetValue();

        if (maxValueFloat == 0)
        {
            Debug.LogWarning("Max value is zero, unable to calculate progress.");
            return;
        }

        float progress = Mathf.Clamp01(currentValueFloat / maxValueFloat);

        fillImage.fillAmount = progress;
    }
}
