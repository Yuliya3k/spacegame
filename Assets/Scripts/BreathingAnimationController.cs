//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System;
//using System.Linq;

//[System.Serializable]
//public class BreathingBlendShape
//{
//    // The name of the blend shape to animate
//    public string blendShapeName;

//    // Maximum value the blend shape can reach (usually 100)
//    public float maxValue = 100f;

//    // Default time to increase (in game minutes)
//    public float defaultIncreaseTimeInGameMinutes = 1f;

//    // Default time to decrease (in game minutes)
//    public float defaultDecreaseTimeInGameMinutes = 1f;

//    // Time to wait between cycles (in game minutes)
//    public float timeBetweenCyclesInGameMinutes = 1f;

//    // Enum to select the mode of operation
//    public enum OperationMode
//    {
//        Automatic,
//        Triggered
//    }

//    public OperationMode mode = OperationMode.Automatic;

//    // Internal coroutine reference for triggered mode
//    [HideInInspector]
//    public Coroutine currentCoroutine = null;
//}

//public class BreathingAnimationController : MonoBehaviour
//{
//    [Header("Blend Shape Settings")]
//    // List of blend shapes to animate
//    public List<BreathingBlendShape> blendShapes = new List<BreathingBlendShape>();

//    [Header("References")]
//    // Reference to the CharacterStats script
//    public CharacterStats characterStats;

//    // Reference to the InGameTimeManager script
//    public InGameTimeManager timeManager;

//    private void Start()
//    {
//        // Ensure CharacterStats is assigned
//        if (characterStats == null)
//        {
//            characterStats = GetComponent<CharacterStats>();
//            if (characterStats == null)
//            {
//                Debug.LogError("BreathingAnimationController: CharacterStats not assigned and not found on the GameObject.");
//                enabled = false;
//                return;
//            }
//        }

//        // Ensure InGameTimeManager is assigned
//        if (timeManager == null)
//        {
//            timeManager = FindObjectOfType<InGameTimeManager>();
//            if (timeManager == null)
//            {
//                Debug.LogError("BreathingAnimationController: InGameTimeManager not assigned and not found in the scene.");
//                enabled = false;
//                return;
//            }
//        }

//        // Start the breathing cycle for each blend shape in automatic mode
//        foreach (var blendShape in blendShapes)
//        {
//            if (blendShape.mode == BreathingBlendShape.OperationMode.Automatic)
//            {
//                blendShape.currentCoroutine = StartCoroutine(AutomaticBreathingCycle(blendShape));
//            }
//        }
//    }

//    // Coroutine to manage the breathing cycle of a blend shape using game time (Automatic Mode)
//    private IEnumerator AutomaticBreathingCycle(BreathingBlendShape blendShape)
//    {
//        while (true)
//        {
//            // Increase blend shape value to max
//            yield return StartCoroutine(AdjustBlendShapeValueGameTime(
//                blendShape,
//                0f, // Start from 0
//                blendShape.maxValue,
//                blendShape.defaultIncreaseTimeInGameMinutes));

//            // Decrease blend shape value back to zero
//            yield return StartCoroutine(AdjustBlendShapeValueGameTime(
//                blendShape,
//                blendShape.maxValue,
//                0f,
//                blendShape.defaultDecreaseTimeInGameMinutes));

//            // Wait for the specified time between cycles in game time
//            if (blendShape.timeBetweenCyclesInGameMinutes > 0f)
//            {
//                yield return StartCoroutine(WaitForGameMinutes(blendShape.timeBetweenCyclesInGameMinutes));
//            }
//        }
//    }

//    // Coroutine to smoothly adjust a blend shape value over time using game time
//    private IEnumerator AdjustBlendShapeValueGameTime(BreathingBlendShape blendShape, float startValue, float endValue, float durationInGameMinutes)
//    {
//        DateTime startTime = timeManager.GetCurrentInGameTime();
//        DateTime endTime = startTime.AddMinutes(durationInGameMinutes);

//        while (timeManager.GetCurrentInGameTime() < endTime)
//        {
//            double elapsedGameMinutes = (timeManager.GetCurrentInGameTime() - startTime).TotalMinutes;
//            float t = (float)(elapsedGameMinutes / durationInGameMinutes);
//            t = Mathf.Clamp01(t);
//            float currentValue = Mathf.Lerp(startValue, endValue, t);

//            characterStats.SetBlendShapeValue(blendShape.blendShapeName, currentValue);

//            yield return null; // Wait for the next frame
//        }

//        // Ensure the final value is set
//        characterStats.SetBlendShapeValue(blendShape.blendShapeName, endValue);

//        // Reset coroutine reference if in triggered mode
//        if (blendShape.mode == BreathingBlendShape.OperationMode.Triggered)
//        {
//            blendShape.currentCoroutine = null;
//        }
//    }

//    // Public method to increase blend shape value by a specified amount over a specified duration
//    public void TriggerBlendShapeAdjustIncrease(string blendShapeName, float amount, float durationInGameMinutes)
//    {
//        var blendShape = blendShapes.FirstOrDefault(bs => bs.blendShapeName == blendShapeName);

//        if (blendShape != null)
//        {
//            if (blendShape.mode == BreathingBlendShape.OperationMode.Triggered)
//            {
//                // If a coroutine is already running, stop it
//                if (blendShape.currentCoroutine != null)
//                {
//                    StopCoroutine(blendShape.currentCoroutine);
//                }

//                float currentValue = characterStats.GetBlendShapeValue(blendShape.blendShapeName);
//                float targetValue = Mathf.Clamp(currentValue + amount, 0f, blendShape.maxValue);

//                // Start the adjust coroutine
//                blendShape.currentCoroutine = StartCoroutine(AdjustBlendShapeValueGameTime(
//                    blendShape,
//                    currentValue,
//                    targetValue,
//                    durationInGameMinutes));
//            }
//            else
//            {
//                Debug.LogWarning($"Blend shape '{blendShapeName}' is set to Automatic mode and cannot be triggered manually.");
//            }
//        }
//        else
//        {
//            Debug.LogWarning($"Blend shape '{blendShapeName}' not found in the BreathingAnimationController.");
//        }
//    }

//    // Public method to decrease blend shape value by a specified amount over a specified duration
//    public void TriggerBlendShapeAdjustDecrease(string blendShapeName, float amount, float durationInGameMinutes)
//    {
//        var blendShape = blendShapes.FirstOrDefault(bs => bs.blendShapeName == blendShapeName);

//        if (blendShape != null)
//        {
//            if (blendShape.mode == BreathingBlendShape.OperationMode.Triggered)
//            {
//                // If a coroutine is already running, stop it
//                if (blendShape.currentCoroutine != null)
//                {
//                    StopCoroutine(blendShape.currentCoroutine);
//                }

//                float currentValue = characterStats.GetBlendShapeValue(blendShape.blendShapeName);
//                float targetValue = Mathf.Clamp(currentValue - amount, 0f, blendShape.maxValue);

//                // Start the adjust coroutine
//                blendShape.currentCoroutine = StartCoroutine(AdjustBlendShapeValueGameTime(
//                    blendShape,
//                    currentValue,
//                    targetValue,
//                    durationInGameMinutes));
//            }
//            else
//            {
//                Debug.LogWarning($"Blend shape '{blendShapeName}' is set to Automatic mode and cannot be triggered manually.");
//            }
//        }
//        else
//        {
//            Debug.LogWarning($"Blend shape '{blendShapeName}' not found in the BreathingAnimationController.");
//        }
//    }

//    // Coroutine to wait for a specified amount of game time (in minutes)
//    private IEnumerator WaitForGameMinutes(float gameMinutes)
//    {
//        DateTime startTime = timeManager.GetCurrentInGameTime();
//        DateTime targetTime = startTime.AddMinutes(gameMinutes);

//        while (timeManager.GetCurrentInGameTime() < targetTime)
//        {
//            yield return null; // Wait for the next frame
//        }
//    }
//}
