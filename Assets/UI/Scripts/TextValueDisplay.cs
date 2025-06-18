using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Remove if using legacy Text
using UnityEngine.UI; // Only required if using legacy Text

[System.Serializable]
public class TextValuePair
{
    [Header("UI Elements")]
    [Tooltip("The TextMeshProUGUI component to display the value.")]
    public TextMeshProUGUI textComponent; // Use Text if not using TextMeshPro

    [Tooltip("The VariableReference to fetch the value from.")]
    public VariableReference variableReference;

    [Tooltip("Number of decimal places to display (0 to 2).")]
    [Range(0, 2)]
    public int decimalPlaces = 0;
}

public class TextValueDisplay : MonoBehaviour
{
    [Header("Text-Value Pairs")]
    [Tooltip("List of text elements and their corresponding variables.")]
    public List<TextValuePair> textValuePairs = new List<TextValuePair>();

    [Header("Update Settings")]
    [Tooltip("Interval in seconds between each UI update.")]
    public float updateInterval = 0.5f; // Adjust as needed

    private float timer;

    private void Start()
    {
        // Initial update
        UpdateAllTexts();
        timer = updateInterval;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateAllTexts();
        }
    }

    private void UpdateAllTexts()
    {
        foreach (var pair in textValuePairs)
        {
            if (pair.textComponent == null || pair.variableReference == null)
            {
                Debug.LogWarning("TextComponent or VariableReference is not assigned in a TextValuePair.");
                continue;
            }

            float value = pair.variableReference.GetValue(); // Assuming GetValue() returns float

            // Format the value based on the number of decimal places
            string formatString = "F" + Mathf.Clamp(pair.decimalPlaces, 0, 2).ToString();
            string formattedValue = value.ToString(formatString);

            // Update the text
            pair.textComponent.text = formattedValue;
        }
    }
}
