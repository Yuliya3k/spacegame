using UnityEngine;

public static class ProgressBarUtils
{
    public static void ValidateMinMax(ref float minValue, ref float maxValue)
    {
        if (minValue > maxValue)
        {
            Debug.LogWarning($"minValue {minValue} greater than maxValue {maxValue}. Values swapped.");
            float temp = minValue;
            minValue = maxValue;
            maxValue = temp;
        }
        else if (Mathf.Approximately(minValue, maxValue))
        {
            Debug.LogWarning($"minValue {minValue} equals maxValue {maxValue}. maxValue incremented slightly.");
            maxValue = minValue + Mathf.Epsilon;
        }
    }

    public static float Normalize(float current, ref float minValue, ref float maxValue, out float clamped)
    {
        ValidateMinMax(ref minValue, ref maxValue);
        clamped = Mathf.Clamp(current, minValue, maxValue);
        return (clamped - minValue) / (maxValue - minValue);
    }
}