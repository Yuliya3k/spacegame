using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class BlendShapeData
{
    public string blendShapeName = "BlendShape";
    [Tooltip("Speed at which the blend shape value changes.")]
    public float morphSpeed = 1f;
    [Tooltip("Categories that affect this blend shape (e.g., Weight, Fullness, Equipment, Muscles).")]
    public List<string> categories = new List<string>();

    [HideInInspector] public int blendShapeIndex;
    [HideInInspector] public Coroutine blendShapeCoroutine;
    [HideInInspector] public SkinnedMeshRenderer renderer;
    [HideInInspector] public Dictionary<string, float> categoryContributions = new Dictionary<string, float>();
}

public class CharacterStats : MonoBehaviour
{
    [Header("Skinned Mesh Renderers")]
    public SkinnedMeshRenderer[] skinnedMeshRenderers;

    [Header("Blend Shape Settings")]
    public List<BlendShapeData> blendShapes = new List<BlendShapeData>()
    {
        new BlendShapeData()
        {
            blendShapeName = "IntestinesFullness",
            morphSpeed = 1f,
            categories = new List<string>() { "IntestinesFullness" }
        },
    };

    [Header("Facial Expression Blend Shapes")]
    public List<BlendShapeData> facialExpressionBlendShapes = new List<BlendShapeData>()
    {
        new BlendShapeData()
        {
            blendShapeName = "Genesis9__facs_ctrl_SmileFullFace",
            morphSpeed = 1f,
            categories = new List<string>() { "Expression" }
        }
    };

    public BlendShapeSyncAutoExtract blendShapeSyncScript;

    public float GetStat(string statName)
    {
        switch (statName)
        {
            case "currentFullness": return currentFullness;
            case "currentHydration": return currentHydration;
            case "maxHydration": return maxHydration;
            case "weight": return weight;
            case "stamina": return stamina;
            case "currentIntFullness": return currentIntFullness;
            case "intestinesCapacity": return intestinesCapacity;

            // Add more cases for other stats as needed
            default:
                Debug.LogWarning("GetStat: Stat name not found: " + statName);
                return 0f;
        }
    }


    [Header("Fullness")]
    public float stomachCapacity;
    public float minStomachCapacity;
    public float maxStomachCapacity;
    public float peristalticVelocity, minPeristalticVelocity, maxPeristalticVelocity, dividerPeristalticVelocity;
    public float intestinesCapacity, intestinesMaxCapacity, intestinesMinCapacity;
    public float currentFullness, minFullness, borderFullness, currentIntFullness;
    public InGameTimeManager timeManager;
    public int defaultFullnessDecreasePerHour = 500;
    private DateTime lastFullnessDecreaseTime;
    private Dictionary<string, float> stats = new Dictionary<string, float>();
    public bool eatenFood;

    [Header("Weight")]
    public float weight;
    public float maxWeight;
    public float minWeight;
    public float startWeight;
    public float caloriesPerKg;
    public float calories, minCalories;
    public float caloriesDecreaseRest = 1f;
    public float caloriesDecreaseWalk = 2f;
    public float caloriesDecreaseRun = 3f;
    public float obesityGenes, minObesityGenes, maxObesityGenes;
    public float skinnyGenes, minSkinnyGenes, maxSkinnyGenes;
    [NonSerialized] public float sessionConsumedCalories = 0f;
    [NonSerialized] public float sessionDecreasedCalories = 0f;
    public float loadedCalories;

    [Header("Weight distribution")]
    public bool boobsGainEnabled;
    public bool torsoGainEnabled;
    public bool thighsGainEnabled;
    public bool shinsGainEnabled;
    public bool armsGainEnabled;
    public bool wholeBodyGainEnabled;
    public bool glutesGainEnabled;

    [Header("Weight Gain by Distribution")]
    public List<BlendShapeData> boobsWeightGainBlendShapes = new List<BlendShapeData>();
    public List<BlendShapeData> torsoWeightGainBlendShapes = new List<BlendShapeData>();
    public List<BlendShapeData> thighsWeightGainBlendShapes = new List<BlendShapeData>();
    public List<BlendShapeData> shinsWeightGainBlendShapes = new List<BlendShapeData>();
    public List<BlendShapeData> armsWeightGainBlendShapes = new List<BlendShapeData>();
    public List<BlendShapeData> wholeBodyWeightGainBlendShapes = new List<BlendShapeData>();
    public List<BlendShapeData> glutesWeightGainBlendShapes = new List<BlendShapeData>();

    [Header("Weight Blend Shape Update Settings")]
    [Tooltip("How long should it take to update weight-related blend shapes when weight changes? 0 for instant.")]
    public float weightBlendShapeUpdateDuration = 0f;

    [Header("Weight Loss Blend Shapes")]
    public List<BlendShapeData> weightLossBlendShapes = new List<BlendShapeData>()
    {
        new BlendShapeData()
        {
            blendShapeName = "WeightLossBlendShape",
            morphSpeed = 1f,
            categories = new List<string>() { "Weight" }
        }
    };

    [Header("Hydration")]
    public float currentHydration;
    public float minHydration, maxHydration;
    public float hydrationDecreaseRest = 1f;
    public float hydrationDecreaseWalk = 2f;
    public float hydrationDecreaseRun = 3f;

    [Header("Muscles")]
    public float musclesState;
    public float musclesMinState, musclesMaxState, musclesStartState;
    public float muscleDecreaseIntervalMinutes = 1f; // how often to decrease muscles in game minutes (adjustable)
    public float currentMuscles;
    public float loadedMusclesState; // From save
    [NonSerialized] public float sessionIncreasedMuscles = 0f;
    [NonSerialized] public float sessionDecreasedMuscles = 0f;
    public float musclesDecreasePerGameMinute = 0.1f; // adjustable in the inspector
    public float muscleBlendShapeUpdateDuration = 100f; // how long it takes for muscle blend shapes to update, adjustable
    
    public List<BlendShapeData> muscleGainBlendShapes = new List<BlendShapeData>()
    {
        new BlendShapeData()
        {
            blendShapeName = "MuscleGainBlendShape",
            morphSpeed = 1f,
            categories = new List<string>() { "Muscles" }
        }
    };
    public List<BlendShapeData> muscleLossBlendShapes = new List<BlendShapeData>()
    {
        new BlendShapeData()
        {
            blendShapeName = "MuscleLossBlendShape",
            morphSpeed = 1f,
            categories = new List<string>() { "Muscles" }
        }
    };

    [Header("Stamina")]
    public float stamina;
    public float maxStamina, minStamina;
    public float baseStaminaDecreasePerGameMinute = 1f;
    public float sprintMultiplier = 1.5f;
    private bool isMoving = false;
    private bool isSprinting = false;
    private DateTime lastStaminaDecreaseTime;

    public float connectiveTissueMobility, minConnectiveTissueMobility, maxConnectiveTissueMobility;

    [Header("Cooking")]
    public float cooking;
    public float minCooking, maxCooking;

    [Header("Agriculture")]
    public float agriculture;
    public float minAgriculture, maxAgriculture;
    public float foodVolume = 200f;

    [Header("Health")]
    public float currentHealth;
    public float minHealth, maxHealth;
    private float borderHazardousLowWeight;
    private float borderHazardousHighWeight;
    private Coroutine weightHealthCoroutine;

    public float urineVolume, minUrineVolume, maxUrineVolume;

    [Header("Reputation")]
    public float reputation;
    
    [Header("Urine Blend Shape")]
    public string urineBlendShapeName = "UrineVolume";

    

    private DateTime lastWeightUpdateTime;
    private Dictionary<string, List<float>> equipmentModifiers = new Dictionary<string, List<float>>();
    public CharacterEquipmentManager equipmentManager;

    private float targetWeight;
    private Coroutine fullnessDecayCoroutine;

    public DateTime? firstMealTime = null;
    public DateTime? lastMealTime = null;
    private Coroutine fullnessDecayTimerCoroutine = null;
    public bool isDecayEnabled = false;

    

    public bool GetIsDecayEnabled() { return isDecayEnabled; }
    public void SetIsDecayEnabled(bool value) { isDecayEnabled = value; }

    public bool IsMoving() { return isMoving; }
    public void SetIsMoving(bool value) { isMoving = value; }

    public bool IsSprinting() { return isSprinting; }
    public void SetIsSprinting(bool value) { isSprinting = value; }

    public DateTime GetLastFullnessDecreaseTime() { return lastFullnessDecreaseTime; }
    public void SetLastFullnessDecreaseTime(DateTime dt) { lastFullnessDecreaseTime = dt; }

    public DateTime GetLastStaminaDecreaseTime() { return lastStaminaDecreaseTime; }
    public void SetLastStaminaDecreaseTime(DateTime dt) { lastStaminaDecreaseTime = dt; }

    private DateTime lastCaloriesDecreaseTime;
    public void SetLastCaloriesDecreaseTime(DateTime dt) { lastCaloriesDecreaseTime = dt; }

    private DateTime lastHydrationDecreaseTime;
    public DateTime GetLastHydrationDecreaseTime() { return lastHydrationDecreaseTime; }
    public void SetLastHydrationDecreaseTime(DateTime dt) { lastHydrationDecreaseTime = dt; }

    public DateTime? GetFirstMealTime() { return firstMealTime; }
    public void SetFirstMealTime(DateTime? dt) { firstMealTime = dt; }

    public DateTime? GetLastMealTime() { return lastMealTime; }
    public void SetLastMealTime(DateTime? dt) { lastMealTime = dt; }

    public float GetEffectiveCalories()
    {
        return loadedCalories + sessionConsumedCalories - sessionDecreasedCalories;
    }

    public void SetLoadedCalories(float loadedCal)
    {
        this.loadedCalories = loadedCal;
    }

    public void RestoreCoroutinesAndTimers()
    {
        if (isDecayEnabled)
            StartCoroutine(FullnessDecayRoutine());

        StartCoroutine(StaminaDecreaseRoutine());
        StartCoroutine(CaloriesDecreaseRoutine());
        StartCoroutine(HydrationDecreaseRoutine());
    }

    public float GetPhysACoefficient()
    {
        return 1.2f + ((currentMuscles / 100f) * 0.75f);
    }







    //[Header("NPC conditional tasks variables")]    
    


    void Start()
    {
        if (SaveSystem.loadedFromSave) return;

        if (timeManager == null)
        {
            timeManager = FindObjectOfType<InGameTimeManager>();
            if (timeManager == null)
            {
                Debug.LogError("CharacterStats: InGameTimeManager not found.");
            }
        }

        lastCaloriesDecreaseTime = timeManager.GetCurrentInGameTime();
        lastHydrationDecreaseTime = timeManager.GetCurrentInGameTime();

        stats.Add("StomachCapacity", stomachCapacity);
        stats.Add("Cooking", cooking);
        stats.Add("Agriculture", agriculture);
        stats.Add("Max health", maxHealth);

        maxHealth = 100f;
        maxHydration = 100f;

        currentHealth = maxHealth;
        currentHydration = maxHydration;

        musclesState = 50f; musclesMinState = 0f; musclesMaxState = 100f; musclesStartState = 50f;
        loadedMusclesState = musclesStartState;
        sessionIncreasedMuscles = 0f;
        sessionDecreasedMuscles = 0f;
        stamina = 100f; maxStamina = 100f; minStamina = 0f;
        //stomachCapacity = 1000f;
        borderFullness = stomachCapacity * 0.8f;
        calories = 0f;
        minCalories = (minWeight - startWeight) * caloriesPerKg * 2f;
        peristalticVelocity = 100f; minPeristalticVelocity = 1f; maxPeristalticVelocity = 200f; dividerPeristalticVelocity = 100f;

        obesityGenes = 0; minObesityGenes = 0; maxObesityGenes = 100f;
        skinnyGenes = 0; minSkinnyGenes = 0; maxSkinnyGenes = 100f;
        connectiveTissueMobility = 0; minConnectiveTissueMobility = 0; maxConnectiveTissueMobility = 100f;
        cooking = 10f; minCooking = 0; maxCooking = 100f;
        agriculture = 1; minAgriculture = 0; maxAgriculture = 100f;
        minFullness = 0;
        minHydration = 0f;

        borderHazardousLowWeight = (startWeight - minWeight) * 0.5f + minWeight;
        borderHazardousHighWeight = (maxWeight - startWeight) * 0.5f + startWeight;

        if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
        {
            Debug.LogError("Skinned Mesh Renderers are not assigned.");
            enabled = false; return;
        }

        foreach (var renderer in skinnedMeshRenderers)
        {
            if (renderer == null || renderer.sharedMesh == null)
            {
                Debug.LogError("SkinnedMeshRenderer or mesh not assigned.");
                enabled = false; return;
            }
        }

        if (equipmentManager == null)
        {
            equipmentManager = GetComponent<CharacterEquipmentManager>();
            if (equipmentManager == null)
                Debug.LogError("CharacterEquipmentManager not found on the character.");
        }

        if (equipmentManager != null && equipmentManager.equippedMeshRenderers != null)
        {
            foreach (var rend in equipmentManager.equippedMeshRenderers)
            {
                SyncBlendShapesToRenderer(rend);
            }
        }

        weight = startWeight;
        targetWeight = startWeight;

        lastFullnessDecreaseTime = timeManager.GetCurrentInGameTime();

        fullnessDecayCoroutine = StartCoroutine(FullnessDecayRoutine());
        StartCoroutine(StaminaDecreaseRoutine());
        StartCoroutine(CaloriesDecreaseRoutine());
        StartCoroutine(HydrationDecreaseRoutine());
        StartCoroutine(MuscleDecreaseRoutine());

        InitializeBlendShapes();
        UpdateWeightBlendShapeContributions();
        StartWeightHealthCheck();
    }



    private void InitializeBlendShapes()
    {
        // Combine all your weight-gain lists into one sequence:
        var allWeightGainShapes = boobsWeightGainBlendShapes
            .Concat(torsoWeightGainBlendShapes)
            .Concat(thighsWeightGainBlendShapes)
            .Concat(shinsWeightGainBlendShapes)
            .Concat(armsWeightGainBlendShapes)
            .Concat(wholeBodyWeightGainBlendShapes)
            .Concat(glutesWeightGainBlendShapes);

        // Now combine that with your base blendShapes and weightLossBlendShapes:
        var allShapes = blendShapes
            .Concat(allWeightGainShapes)
            .Concat(weightLossBlendShapes)
            .Concat(facialExpressionBlendShapes); // optional if you want them here too

        foreach (var blendShape in allShapes)
        {
            bool found = false;
            foreach (var renderer in skinnedMeshRenderers)
            {
                int index = renderer.sharedMesh.GetBlendShapeIndex(blendShape.blendShapeName);
                if (index >= 0)
                {
                    blendShape.blendShapeIndex = index;
                    blendShape.renderer = renderer;
                    // Initialize category contributions
                    foreach (string category in blendShape.categories)
                        blendShape.categoryContributions[category] = 0f;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Debug.LogError($"Blend shape '{blendShape.blendShapeName}' not found in any renderer.");
            }
        }

        //foreach (var blendShape in facialExpressionBlendShapes)
        //{
        //    bool found = false;
        //    foreach (var renderer in skinnedMeshRenderers)
        //    {
        //        int index = renderer.sharedMesh.GetBlendShapeIndex(blendShape.blendShapeName);
        //        if (index >= 0)
        //        {
        //            blendShape.blendShapeIndex = index;
        //            blendShape.renderer = renderer;
        //            foreach (string category in blendShape.categories)
        //                blendShape.categoryContributions[category] = 0f;
        //            found = true;
        //            break;
        //        }
        //    }

        //    if (!found)
        //    {
        //        Debug.LogError($"Facial blend shape '{blendShape.blendShapeName}' not found.");
        //    }
        //}
    }

    

    private void InitializeBlendShape(BlendShapeData blendShape)
    {
        bool found = false;
        foreach (var renderer in skinnedMeshRenderers)
        {
            int index = renderer.sharedMesh.GetBlendShapeIndex(blendShape.blendShapeName);
            if (index >= 0)
            {
                blendShape.blendShapeIndex = index;
                blendShape.renderer = renderer; // Store the renderer
                Debug.Log($"Found blend shape '{blendShape.blendShapeName}' in renderer '{renderer.name}'.");
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogError($"Blend shape '{blendShape.blendShapeName}' not found in any assigned renderer.");
        }

        // Initialize category contributions
        foreach (string category in blendShape.categories)
        {
            blendShape.categoryContributions[category] = 0f;
        }
    }

    // Method to update category contribution for a blend shape
    public void UpdateWeightBlendShapeContributions()
    {
        float weightValue = weight;
        if (weightValue > startWeight)
        {
            float pct = Mathf.Clamp((weightValue - startWeight) / (maxWeight - startWeight) * 100f, 0f, 100f);

            // For each distribution that’s enabled, set those shapes to 'pct'
            if (boobsGainEnabled)
            {
                foreach (var bs in boobsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", pct, weightBlendShapeUpdateDuration);
            }
            if (torsoGainEnabled)
            {
                foreach (var bs in torsoWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", pct, weightBlendShapeUpdateDuration);
            }
            if (thighsGainEnabled)
            {
                foreach (var bs in thighsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", pct, weightBlendShapeUpdateDuration);
            }
            if (shinsGainEnabled)
            {
                foreach (var bs in shinsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", pct, weightBlendShapeUpdateDuration);
            }
            if (armsGainEnabled)
            {
                foreach (var bs in armsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", pct, weightBlendShapeUpdateDuration);
            }
            if (wholeBodyGainEnabled)
            {
                foreach (var bs in wholeBodyWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", pct, weightBlendShapeUpdateDuration);
            }
            if (glutesGainEnabled)
            {
                foreach (var bs in glutesWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", pct, weightBlendShapeUpdateDuration);
            }

            // Then ensure all weightLoss blend shapes are set to 0
            foreach (var bs in weightLossBlendShapes)
                UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
        }
        else if (weightValue < startWeight)
        {
            float pct = Mathf.Clamp((startWeight - weightValue) / (startWeight - minWeight) * 100f, 0f, 100f);
            foreach (var bs in weightLossBlendShapes)
                UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", pct, weightBlendShapeUpdateDuration);

            if (boobsGainEnabled)
            {
                foreach (var bs in boobsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }
            if (torsoGainEnabled)
            {
                foreach (var bs in torsoWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }
            if (thighsGainEnabled)
            {
                foreach (var bs in thighsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }
            if (shinsGainEnabled)
            {
                foreach (var bs in shinsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }
            if (armsGainEnabled)
            {
                foreach (var bs in armsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }
            if (wholeBodyGainEnabled)
            {
                foreach (var bs in wholeBodyWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }
            if (glutesGainEnabled)
            {
                foreach (var bs in glutesWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }

        }
        else
        {
            if (boobsGainEnabled)
            {
                foreach (var bs in boobsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }
            if (torsoGainEnabled)
            {
                foreach (var bs in torsoWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }
            if (thighsGainEnabled)
            {
                foreach (var bs in thighsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }
            if (shinsGainEnabled)
            {
                foreach (var bs in shinsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }
            if (armsGainEnabled)
            {
                foreach (var bs in armsWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }
            if (wholeBodyGainEnabled)
            {
                foreach (var bs in wholeBodyWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }

            if (glutesGainEnabled)
            {
                foreach (var bs in glutesWeightGainBlendShapes)
                    UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
            }

            foreach (var bs in weightLossBlendShapes)
                UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Weight", 0f, weightBlendShapeUpdateDuration);
        }
    }


   

    public void UpdateWeight(float caloriesChange)
    {
        float weightChange = caloriesChange / caloriesPerKg;
        targetWeight += weightChange;
        targetWeight = Mathf.Clamp(targetWeight, minWeight, maxWeight);

        // Instead of a coroutine, just recalculate immediately
        weight = targetWeight;
        UpdateWeightBlendShapeContributions();
    }   


    // Updates only weight-related blend shapes
    private void UpdateWeightBlendShapes()
    {
        var allWeightGainShapes = boobsWeightGainBlendShapes
        .Concat(torsoWeightGainBlendShapes)
        .Concat(thighsWeightGainBlendShapes)
        .Concat(shinsWeightGainBlendShapes)
        .Concat(armsWeightGainBlendShapes)
        .Concat(wholeBodyWeightGainBlendShapes)
        .Concat(glutesWeightGainBlendShapes);

        var allShapes = allWeightGainShapes.Concat(weightLossBlendShapes);

        foreach (var blendShape in allShapes)
        {
            UpdateBlendShape(blendShape.blendShapeName);
        }
    }

    // Method to update fullness blend shapes
    public void UpdateFullnessBlendShapes(float customDuration = 5f)
    {
        float fullnessPercentage = Mathf.Clamp((currentFullness / maxStomachCapacity) * 100f, 0f, 100f);
        Debug.Log($"fullnessPercentage {fullnessPercentage}");

        foreach (var blendShape in blendShapes.Where(bs => bs.categories.Contains("Fullness")))
        {
            UpdateBlendShapeCategoryContribution(blendShape.blendShapeName, "Fullness", fullnessPercentage, customDuration);
        }
    }

    // Method to update intestines fullness blend shapes
    public void UpdateIntestinesFullnessBlendShapes()
    {
        // Calculate the percentage of intestines fullness
        float intestinesFullnessPercentage = Mathf.Clamp((currentIntFullness / intestinesMaxCapacity) * 100f, 0f, 100f);

        // Update blend shapes that are affected by "IntestinesFullness" category
        foreach (var blendShape in blendShapes.Where(bs => bs.categories.Contains("IntestinesFullness")))
        {
            UpdateBlendShapeCategoryContribution(blendShape.blendShapeName, "IntestinesFullness", intestinesFullnessPercentage);
        }
    }

    // UpdateBlendShape method remains the same
    private void UpdateBlendShape(string blendShapeName, float customDuration = 5f)
    {
        float targetValue = CalculateTargetBlendShapeValue(blendShapeName);
        Debug.Log($"targetBlendShapeValue {targetValue}");

        BlendShapeData blendShape = GetBlendShapeData(blendShapeName);

        if (blendShape != null)
        {
            // Stop any existing coroutine
            if (blendShape.blendShapeCoroutine != null)
            {
                StopCoroutine(blendShape.blendShapeCoroutine);
            }

            // Check if the blend shape is affected by the Equipment category
            if (blendShape.categories.Contains("Equipment"))
            {
                // Set blend shape weight instantly on all renderers
                SetBlendShapeWeightOnAllRenderers(blendShapeName, targetValue);

                if (blendShapeSyncScript != null)
                {
                    blendShapeSyncScript.shouldSync = true;
                }
            }
            else
            {
                // Use smooth transition for other categories
                // Use customDuration if provided and valid, else use blendShape.morphSpeed
                float duration = customDuration >= 0f ? customDuration : blendShape.morphSpeed;
                blendShape.blendShapeCoroutine = StartCoroutine(SmoothBlendShapeTransition(blendShape, targetValue, duration));
            }
        }
        else
        {
            Debug.LogWarning($"Blend shape '{blendShapeName}' not found.");
        }
    }

    // CalculateTargetBlendShapeValue method remains the same
    private float CalculateTargetBlendShapeValue(string blendShapeName)
    {
        BlendShapeData blendShape = GetBlendShapeData(blendShapeName);

        if (blendShape != null)
        {
            // Sum all category contributions
            float totalValue = 0f;
            foreach (var contribution in blendShape.categoryContributions.Values)
            {
                totalValue += contribution;
            }

            // Clamp the final value between 0 and 100
            totalValue = Mathf.Clamp(totalValue, 0f, 100f);
            return totalValue;
        }
        else
        {
            Debug.LogWarning($"Blend shape '{blendShapeName}' not found.");
            return 0f;
        }
    }

    // Coroutine to smoothly change a blend shape value over time
    //private IEnumerator SmoothBlendShapeTransition(BlendShapeData blendShape, float targetBlendShapeValue, float duration)
    //{
    //    Debug.Log($"Starting transition for blend shape '{blendShape.blendShapeName}' from {blendShape.renderer.GetBlendShapeWeight(blendShape.blendShapeIndex)} to {targetBlendShapeValue} over {duration} seconds.");

    //    float elapsedTime = 0f;
    //    float startingBlendShapeValue = blendShape.renderer.GetBlendShapeWeight(blendShape.blendShapeIndex);
    //    float updateInterval = 0.1f;

    //    // If duration is zero or negative, set the value instantly
    //    if (duration <= 0f)
    //    {
    //        SetBlendShapeWeightOnAllRenderers(blendShape.blendShapeName, targetBlendShapeValue);

    //        if (blendShapeSyncScript != null)
    //        {
    //            blendShapeSyncScript.shouldSync = true;
    //        }

    //        blendShape.blendShapeCoroutine = null;
    //        yield break;
    //    }

    //    while (elapsedTime < duration)
    //    {
    //        float newValue = Mathf.Lerp(startingBlendShapeValue, targetBlendShapeValue, elapsedTime / duration);
    //        SetBlendShapeWeightOnAllRenderers(blendShape.blendShapeName, newValue);

    //        if (blendShapeSyncScript != null)
    //        {
    //            blendShapeSyncScript.shouldSync = true;
    //        }

    //        elapsedTime += updateInterval;
    //        yield return new WaitForSeconds(updateInterval);
    //    }

    //    SetBlendShapeWeightOnAllRenderers(blendShape.blendShapeName, targetBlendShapeValue);

    //    if (blendShapeSyncScript != null)
    //    {
    //        blendShapeSyncScript.shouldSync = true;
    //    }

    //    blendShape.blendShapeCoroutine = null;

    //    Debug.Log($"Completed transition for blend shape '{blendShape.blendShapeName}' to {targetBlendShapeValue}.");
    //}



    private IEnumerator SmoothBlendShapeTransition(BlendShapeData blendShape, float targetBlendShapeValue, float durationInGameSeconds)
    {
        // 1) Grab the starting in-game time
        DateTime startTime = timeManager.GetCurrentInGameTime();
        // 2) We'll end after 'durationInGameSeconds' of in-game time
        DateTime endTime = startTime.AddSeconds(durationInGameSeconds);

        // Starting weight
        float startWeight = blendShape.renderer.GetBlendShapeWeight(blendShape.blendShapeIndex);

        Debug.Log($"Starting transition for blend shape '{blendShape.blendShapeName}' " +
                  $"from {startWeight} to {targetBlendShapeValue} over {durationInGameSeconds} *in-game seconds*.");

        // Keep going until we pass endTime (in game time)
        while (true)
        {
            // Current in-game time
            DateTime now = timeManager.GetCurrentInGameTime();
            if (now >= endTime)
            {
                // We've reached or exceeded our end time
                break;
            }

            // Fraction from 0..1 indicating how far along we are
            double secondsSoFar = (now - startTime).TotalSeconds;
            float t = (float)(secondsSoFar / durationInGameSeconds); // 0 to 1

            // Interpolate
            float newWeight = Mathf.Lerp(startWeight, targetBlendShapeValue, t);

            // Apply to *all* relevant renderers
            SetBlendShapeWeightOnAllRenderers(blendShape.blendShapeName, newWeight);

            // Let the sync script know we changed something
            if (blendShapeSyncScript != null)
                blendShapeSyncScript.shouldSync = true;

            // Wait **one frame** in your normal loop, not real-time 
            yield return null;
        }

        // Final set to target
        SetBlendShapeWeightOnAllRenderers(blendShape.blendShapeName, targetBlendShapeValue);
        if (blendShapeSyncScript != null)
            blendShapeSyncScript.shouldSync = true;

        Debug.Log($"Completed transition for blend shape '{blendShape.blendShapeName}' to {targetBlendShapeValue}.");
        blendShape.blendShapeCoroutine = null;
    }



    // Method to set blend shape weight on all renderers
    public void SetBlendShapeWeightOnAllRenderers(string blendShapeName, float value)
    {
        // List of renderers to update
        List<SkinnedMeshRenderer> renderersToUpdate = new List<SkinnedMeshRenderer>(skinnedMeshRenderers);


        // Add equipped mesh renderers
        if (equipmentManager != null)
        {
            renderersToUpdate.AddRange(equipmentManager.equippedMeshRenderers);
        }

        foreach (var renderer in renderersToUpdate)
        {
            int blendShapeIndex = renderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
            if (blendShapeIndex >= 0)
            {
                renderer.SetBlendShapeWeight(blendShapeIndex, value);
            }
            else
            {
                // Optional: Log if the blend shape is not found on the renderer
                //Debug.LogWarning($"Blend shape '{blendShapeName}' not found on renderer '{renderer.name}'.");
            }
        }
    }

    // Method to synchronize blend shapes to a specific renderer
    public void SyncBlendShapesToRenderer(SkinnedMeshRenderer renderer)
    {
        var allWeightGainShapes = boobsWeightGainBlendShapes
        .Concat(torsoWeightGainBlendShapes)
        .Concat(thighsWeightGainBlendShapes)
        .Concat(shinsWeightGainBlendShapes)
        .Concat(armsWeightGainBlendShapes)
        .Concat(wholeBodyWeightGainBlendShapes)
        .Concat(glutesWeightGainBlendShapes);

        var allShapes = blendShapes
            .Concat(allWeightGainShapes)
            .Concat(weightLossBlendShapes)
            .Concat(facialExpressionBlendShapes);

        foreach (var blendShape in allShapes)
        {
            int blendShapeIndex = renderer.sharedMesh.GetBlendShapeIndex(blendShape.blendShapeName);
            if (blendShapeIndex >= 0)
            {
                float targetValue = CalculateTargetBlendShapeValue(blendShape.blendShapeName);
                renderer.SetBlendShapeWeight(blendShapeIndex, targetValue);
            }
        }
    }

    // Method to apply equipment modifiers
    public void ApplyEquipmentModifier(ItemDataEquipment equipment)
    {
        foreach (BlendShapeModifier blendShapeModifier in equipment.blendShapeModifiers)
        {
            UpdateBlendShapeCategoryContribution(blendShapeModifier.blendShapeName, "Equipment", blendShapeModifier.blendShapeValueChange);
            Debug.Log("Equipment blend shape modifiers applied.");
        }
    }

    // Method to remove equipment modifiers
    public void RemoveEquipmentModifier(ItemDataEquipment equipment)
    {
        foreach (BlendShapeModifier blendShapeModifier in equipment.blendShapeModifiers)
        {
            UpdateBlendShapeCategoryContribution(blendShapeModifier.blendShapeName, "Equipment", 0f);
        }
    }

    

    // Method to modify a specific stat
    public void ModifyStat(string statName, float valueChange)
    {
        if (stats.ContainsKey(statName))
        {
            stats[statName] += valueChange;
            Debug.Log($"Modified {statName} by {valueChange}. New value: {stats[statName]}");
        }
        else
        {
            Debug.LogWarning($"Stat {statName} not found!");
        }
    }

    

    // Method to calculate decay amount based on fullnessDecreasePerHour and interval
    public float CalculateDecayAmount()
    {
        float decayIntervalMinutes = 0.5f; // Decay every 0.5 in-game minutes
        float fullnessDecreasePerMinute = (defaultFullnessDecreasePerHour / 60f) * (peristalticVelocity / 100f);
        return fullnessDecreasePerMinute * decayIntervalMinutes;
    }

    // Method to decrease fullness
    private void DecreaseFullness(float amount)
    {
        // Decrease current fullness
        float previousFullness = currentFullness;
        currentFullness = Mathf.Max(0, currentFullness - amount);


        // Calculate the actual amount decreased (in case currentFullness was already low)
        float actualDecrease = previousFullness - currentFullness;

        // **Add to intestines fullness by 20% of the actual decrease**
        float intestinesIncreaseAmount = actualDecrease * 0.2f;
        float previousIntFullness = currentIntFullness;
        currentIntFullness = Mathf.Min(currentIntFullness + intestinesIncreaseAmount, intestinesCapacity);

        // Update fullness blend shapes with a transition duration of 2 in-game minutes
        UpdateFullnessBlendShapes(timeManager.GetRealTimeDurationForGameMinutes(2f));

        // Update intestines fullness blend shapes
        UpdateIntestinesFullnessBlendShapes();

        Debug.Log($"Fullness decreased by {actualDecrease}. Current Fullness: {currentFullness}. Intestines Fullness: {currentIntFullness}.");
    }

    // Coroutine to handle eating: starts the coroutine to update fullness and calories
    //public void UpdateStatsOnEating(float volumeToAdd, float caloriesToAdd, float duration)
    //{
    //    StartCoroutine(FullnessAndCaloriesTransition(volumeToAdd, caloriesToAdd, duration));
    //}

    private IEnumerator FullnessAndCaloriesTransition(float volumeToAdd, float caloriesToAdd, float duration)
    {
        float startingFullness = currentFullness;
        float targetFullness = Mathf.Min(currentFullness + volumeToAdd, stomachCapacity);

        float startingCalories = calories;
        float targetCalories = calories + caloriesToAdd;


        float elapsedTime = 0f;
        float updateInterval = 60f; // Adjust as needed for smoothness

        Debug.Log($"Starting Fullness and Calories Transition: Volume={volumeToAdd}, Calories={caloriesToAdd}, Duration={duration}s");

        while (elapsedTime < duration)
        {
            // Smoothly interpolate fullness
            currentFullness = Mathf.Lerp(startingFullness, targetFullness, elapsedTime / duration);

            // Smoothly interpolate calories
            calories = Mathf.Lerp(startingCalories, targetCalories, elapsedTime / duration);

            // Update blend shapes based on current stats with customDuration
            UpdateFullnessBlendShapes(timeManager.GetRealTimeDurationForGameMinutes(2f)); // 2 in-game minutes

            elapsedTime += updateInterval;
            yield return new WaitForSeconds(updateInterval);
        }

        // Ensure final values are set
        currentFullness = targetFullness;
        calories = targetCalories;



        // Update weight with the total calories added
        UpdateWeight(caloriesToAdd);

        // Final blend shape update with customDuration
        UpdateFullnessBlendShapes(timeManager.GetRealTimeDurationForGameMinutes(2f));

        Debug.Log($"Completed Fullness and Calories Transition: Fullness={currentFullness}, Calories={calories}");
    }

    // Method to decrease weight
    public void DecreaseWeight(float weightToSubtract)
    {
        targetWeight -= weightToSubtract;
        targetWeight = Mathf.Clamp(targetWeight, minWeight, maxWeight);

        // Immediately update weight and blend shapes
        weight = targetWeight;
        UpdateWeightBlendShapeContributions();
    }


    // Helper method to set blend shape weight on all relevant renderers
    private void SetBlendShapeWeight(string blendShapeName, float value)
    {
        List<SkinnedMeshRenderer> renderersToUpdate = new List<SkinnedMeshRenderer>(skinnedMeshRenderers);


        // Add equipped mesh renderers
        if (equipmentManager != null)
        {
            renderersToUpdate.AddRange(equipmentManager.equippedMeshRenderers);
        }

        foreach (var renderer in renderersToUpdate)
        {
            int blendShapeIndex = renderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
            if (blendShapeIndex >= 0)
            {
                renderer.SetBlendShapeWeight(blendShapeIndex, value);
            }
            else
            {
                // Optional: Log if the blend shape is not found on the renderer
                // Debug.LogWarning($"Blend shape '{blendShapeName}' not found on renderer '{renderer.name}'.");
            }
        }
    }

    private void SetBlendShapeWeightOnSource(string blendShapeName, float value)
    {
        if (skinnedMeshRenderers != null && skinnedMeshRenderers.Length > 0)
        {
            SkinnedMeshRenderer sourceRenderer = skinnedMeshRenderers[0];
            int blendShapeIndex = sourceRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);

            if (blendShapeIndex >= 0)
            {
                sourceRenderer.SetBlendShapeWeight(blendShapeIndex, value);
                Debug.Log($"[Weight Transition] Set blend shape '{blendShapeName}' on source to {value}.");
            }
            else
            {
                Debug.LogWarning($"[Weight Transition] Blend shape '{blendShapeName}' not found on source renderer.");
            }
        }
        else
        {
            Debug.LogWarning("[Weight Transition] Source SkinnedMeshRenderer is not assigned.");
        }
    }


    // **New: Coroutine for Fullness Decay with Delay After Eating**
    private IEnumerator FullnessDecayRoutine()
    {
        Debug.Log("FullnessDecayRoutine started.");
        while (true)
        {
            if (isDecayEnabled)
            {
                DateTime currentInGameTime = timeManager.GetCurrentInGameTime();
                TimeSpan timeElapsed = currentInGameTime - lastFullnessDecreaseTime;

                if (timeElapsed.TotalMinutes >= 0.5f)
                {
                    int decayIntervals = Mathf.FloorToInt((float)(timeElapsed.TotalMinutes / 0.5f));
                    float decayAmount = CalculateDecayAmount();

                    for (int i = 0; i < decayIntervals; i++)
                    {
                        // Only decrease fullness if currentFullness > 0
                        if (currentFullness > 0f)
                        {
                            // Only add to intestines if currentIntFullness < intestinesCapacity
                            if (currentIntFullness < intestinesCapacity)
                            {
                                DecreaseFullness(decayAmount);
                            }
                            else
                            {
                                Debug.Log($"Intestines fullness has reached capacity ({intestinesCapacity}). Fullness decay halted.");
                                break;
                            }
                        }
                        else
                        {
                            Debug.Log($"Current fullness is 0. Fullness decay halted.");
                            break;
                        }
                    }
                    lastFullnessDecreaseTime = lastFullnessDecreaseTime.AddMinutes(0.5f * decayIntervals);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }



    // **Modified FullnessDecayTimer Method**
    public IEnumerator FullnessDecayTimer()
    {
        Debug.Log("Fullness decay will start after 30 game minutes.");
        DateTime startTime = timeManager.GetCurrentInGameTime();
        DateTime targetTime = startTime.AddMinutes(30);

        // Wait until the in-game time reaches the target time
        yield return new WaitUntil(() => timeManager.GetCurrentInGameTime() >= targetTime);

        isDecayEnabled = true;

        // Reset lastFullnessDecreaseTime to current time
        lastFullnessDecreaseTime = timeManager.GetCurrentInGameTime();

        Debug.Log("Fullness decay enabled.");
    }


    // **New: Method to Handle Meal Timing**
    public void HandleMealTime()
    {
        DateTime currentTime = timeManager.GetCurrentInGameTime();

        if (firstMealTime == null || lastMealTime == null || (currentTime - lastMealTime.Value).TotalHours >= 3)
        {
            // New meal detected
            firstMealTime = currentTime;

            // Start the fullness decay timer
            if (fullnessDecayTimerCoroutine != null)
            {
                StopCoroutine(fullnessDecayTimerCoroutine);
            }
            fullnessDecayTimerCoroutine = StartCoroutine(FullnessDecayTimer());

            // Disable existing decay if any
            isDecayEnabled = false;

            Debug.Log("New meal detected. Fullness decay timer started.");
        }
        else
        {
            Debug.Log("Meal within 2 hours of last meal. Continuing current meal group.");
        }

        // Update the lastMealTime
        lastMealTime = currentTime;
    }

    // **New: Modify UpdateStatsOnEating to Include Meal Handling**
    // Coroutine for fullness decay is already managed by FullnessDecayRoutine()
    // Start the FullnessDecayRoutine in Start() if not already running
    // But as we modified Start() to remove starting fullnessDecayCoroutine, we need to start it here
    public void UpdateStatsOnEating(float volumeToAdd, float caloriesToAdd, float hydrationToAdd, float peristalticVelocityImpact, float healthImpact, float duration)
    {
        // We don't directly modify calories now, we increase sessionConsumedCalories
        sessionConsumedCalories += caloriesToAdd;
        // The fullness and hydration transitions remain the same
        // and at the end of these transitions, we recalculate weight.
        StartCoroutine(FullnessCaloriesHydrationTransition(volumeToAdd, caloriesToAdd, hydrationToAdd, duration));

        if (peristalticVelocityImpact != 0f)
            StartCoroutine(AdjustPeristalticVelocity(peristalticVelocityImpact));

        if (healthImpact != 0f)
            StartCoroutine(AdjustHealth(healthImpact));

        HandleMealTime();
    }




    public IEnumerator DecreaseStaminaOverTime(float amount, float duration)
    {
        float startingStamina = stamina;
        float targetStamina = Mathf.Max(minStamina, stamina - amount);

        float elapsedTime = 0f;
        float updateInterval = 0.1f;

        while (elapsedTime < duration)
        {
            stamina = Mathf.Lerp(startingStamina, targetStamina, elapsedTime / duration);
            elapsedTime += updateInterval;
            yield return new WaitForSeconds(updateInterval);
        }

        stamina = targetStamina;
    }


    public void SetMovementState(bool moving, bool sprinting)
    {
        isMoving = moving;
        isSprinting = sprinting;
    }

    private IEnumerator StaminaDecreaseRoutine()
    {
        lastStaminaDecreaseTime = timeManager.GetCurrentInGameTime();

        while (true)
        {
            DateTime currentInGameTime = timeManager.GetCurrentInGameTime();
            TimeSpan timeElapsed = currentInGameTime - lastStaminaDecreaseTime;

            if (timeElapsed.TotalMinutes >= 1f)
            {
                int intervals = Mathf.FloorToInt((float)(timeElapsed.TotalMinutes / 1f));

                for (int i = 0; i < intervals; i++)
                {
                    if (isMoving)
                    {
                        // Calculate staminaspeedmodifier
                        float staminaspeedmodifier = (weight - minWeight) / (maxWeight - minWeight);
                        staminaspeedmodifier = Mathf.Clamp01(staminaspeedmodifier);

                        // Calculate stamina decrease speed
                        float staminaDecreaseSpeed = staminaspeedmodifier * staminaspeedmodifier;

                        // Adjust for sprinting
                        if (isSprinting)
                        {
                            staminaDecreaseSpeed *= sprintMultiplier;
                        }

                        // Decrease stamina
                        float decreaseAmount = baseStaminaDecreasePerGameMinute * staminaDecreaseSpeed;
                        stamina = Mathf.Max(minStamina, stamina - decreaseAmount);

                        Debug.Log($"Stamina decreased by {decreaseAmount} due to movement. Current stamina: {stamina}");
                    }
                }

                lastStaminaDecreaseTime = lastStaminaDecreaseTime.AddMinutes(intervals * 1f);
            }

            yield return null;
        }
    }



    private IEnumerator CaloriesDecreaseRoutine()
    {
        DateTime lastCheck = timeManager.GetCurrentInGameTime();
        while (true)
        {
            DateTime currentInGameTime = timeManager.GetCurrentInGameTime();
            TimeSpan timeElapsed = currentInGameTime - lastCheck;

            if (timeElapsed.TotalMinutes >= 1f)
            {
                int intervals = Mathf.FloorToInt((float)(timeElapsed.TotalMinutes / 1f));
                for (int i = 0; i < intervals; i++)
                {
                    float physACoefficient = GetPhysACoefficient();
                    float caloriesPerMinuteDecrease = 0f;

                    if (!isMoving)
                        caloriesPerMinuteDecrease = caloriesDecreaseRest * physACoefficient;
                    else if (isMoving && !isSprinting)
                        caloriesPerMinuteDecrease = caloriesDecreaseWalk * physACoefficient;
                    else if (isMoving && isSprinting)
                        caloriesPerMinuteDecrease = caloriesDecreaseRun * physACoefficient;

                    // Instead of directly modifying calories:
                    sessionDecreasedCalories += caloriesPerMinuteDecrease;

                    // Now recalculate weight since calories changed
                    RecalculateTargetWeightAndStartTransition();
                }

                lastCheck = lastCheck.AddMinutes(intervals);
            }

            yield return null;
        }
    }



    public IEnumerator DecreaseCaloriesOverTime(float amount, float duration)
    {
        float startingCalories = calories;
        float targetCalories = startingCalories - amount;

        // Ensure targetCalories does not go below minCalories
        if (targetCalories < minCalories)
            targetCalories = minCalories;

        float elapsedTime = 0f;
        float updateInterval = 0.1f;

        while (elapsedTime < duration)
        {
            calories = Mathf.Lerp(startingCalories, targetCalories, elapsedTime / duration);

            if (calories <= minCalories)
            {
                calories = minCalories;
                // Character dies
                Debug.Log("Character has died due to starvation.");
                NotificationManager.instance.ShowNotification(
                    "You have died of starvation",
                    textColor: new Color(255f, 0f, 0f, 255f),
                    backgroundColor: new Color(255f, 0f, 0f, 255f),
                    backgroundSprite: null, // Assign a Sprite if you have one
                    startPos: new Vector2(0f, 200f), // Starting position
                    endPos: new Vector2(0f, 200f),      // Ending position (center)
                    moveDur: 4f,                      // Move over 2 seconds
                    fadeDur: 2f,                    // Fade in/out over 0.5 seconds
                    displayDur: 0f                    // Display for 2 seconds
                );
                // Implement death logic here
                // Optionally, you can stop the coroutine
                // yield break;
            }

            elapsedTime += updateInterval;
            yield return new WaitForSeconds(updateInterval);
        }

        calories = targetCalories;
    }


    private IEnumerator HydrationDecreaseRoutine()
    {
        DateTime lastHydrationDecreaseTime = timeManager.GetCurrentInGameTime();

        while (true)
        {
            DateTime currentInGameTime = timeManager.GetCurrentInGameTime();
            TimeSpan timeElapsed = currentInGameTime - lastHydrationDecreaseTime;

            if (timeElapsed.TotalMinutes >= 1f)
            {
                int intervals = Mathf.FloorToInt((float)(timeElapsed.TotalMinutes / 1f));

                for (int i = 0; i < intervals; i++)
                {
                    float weightCoefficient = GetWeightCoefficient(); // Coefficient based on weight
                    float hydrationPerMinuteDecrease = 0f;

                    if (!isMoving)
                    {
                        // Resting
                        hydrationPerMinuteDecrease = hydrationDecreaseRest * weightCoefficient;
                    }
                    else if (isMoving && !isSprinting)
                    {
                        // Walking
                        hydrationPerMinuteDecrease = hydrationDecreaseWalk * weightCoefficient;
                    }
                    else if (isMoving && isSprinting)
                    {
                        // Running
                        hydrationPerMinuteDecrease = hydrationDecreaseRun * weightCoefficient;
                    }

                    // Decrease hydration
                    currentHydration -= hydrationPerMinuteDecrease;
                    currentHydration = Mathf.Clamp(currentHydration, minHydration, maxHydration);

                    if (currentHydration <= minHydration)
                    {
                        currentHydration = minHydration;
                        // Handle effects of dehydration (e.g., health decrease)
                        Debug.Log("Character is dehydrated.");
                        // Implement dehydration logic here
                    }

                    //Debug.Log($"Hydration decreased by {hydrationPerMinuteDecrease} due to {(!isMoving ? "resting" : (isSprinting ? "running" : "walking"))}. Current hydration: {currentHydration}");
                }

                lastHydrationDecreaseTime = lastHydrationDecreaseTime.AddMinutes(intervals * 1f);
            }

            yield return null;
        }
    }


    public float GetWeightCoefficient()
    {
        // Returns a coefficient between 0.5 (at minWeight) and 2 (at maxWeight)
        float weightRange = maxWeight - minWeight;
        if (weightRange == 0f) return 1f; // Prevent division by zero

        float weightRatio = (weight - minWeight) / weightRange; // 0 at minWeight, 1 at maxWeight
        float coefficient = Mathf.Lerp(0.5f, 2f, weightRatio);
        return coefficient;
    }



    private IEnumerator FullnessCaloriesHydrationTransition(float volumeToAdd, float caloriesToAdd, float hydrationToAdd, float duration)
    {
        float startingFullness = currentFullness;
        float targetFullness = Mathf.Min(currentFullness + volumeToAdd, stomachCapacity);

        float startingCalories = GetEffectiveCalories(); // just for logging
        float targetEffectiveCalories = startingCalories; // we do not directly modify calories here except by session vars

        float startingHydration = currentHydration;
        float targetHydration = Mathf.Clamp(currentHydration + hydrationToAdd, minHydration, maxHydration);

        float elapsedTime = 0f;
        float updateInterval = 0.1f;

        while (elapsedTime < duration)
        {
            currentFullness = Mathf.Lerp(startingFullness, targetFullness, elapsedTime / duration);
            currentHydration = Mathf.Lerp(startingHydration, targetHydration, elapsedTime / duration);

            UpdateFullnessBlendShapes(timeManager.GetRealTimeDurationForGameMinutes(2f));
            elapsedTime += updateInterval;
            yield return new WaitForSeconds(updateInterval);
        }

        currentFullness = targetFullness;
        currentHydration = targetHydration;

        // After eating, we already updated sessionConsumedCalories, so recalculate weight now:
        RecalculateTargetWeightAndStartTransition();

        Debug.Log($"Completed Fullness/Calories/Hydration Transition: Fullness={currentFullness}, Calories={GetEffectiveCalories()}, Hydration={currentHydration}");
    }



    private void AddUrineOverTime(float urineAmount, float durationInHours)
    {
        // Start coroutine to add urine over game time
        StartCoroutine(UrineIncreaseCoroutine(urineAmount, durationInHours));
    }

    private IEnumerator UrineIncreaseCoroutine(float urineAmount, float durationInHours)
    {
        float startingUrineVolume = urineVolume;
        float targetUrineVolume = urineVolume + urineAmount;
        targetUrineVolume = Mathf.Clamp(targetUrineVolume, minUrineVolume, maxUrineVolume);

        float elapsedTime = 0f;
        float durationInRealTimeSeconds = timeManager.GetRealTimeDurationForGameHours(durationInHours);
        float updateInterval = 0.1f;

        while (elapsedTime < durationInRealTimeSeconds)
        {
            urineVolume = Mathf.Lerp(startingUrineVolume, targetUrineVolume, elapsedTime / durationInRealTimeSeconds);

            // Update urine blend shape
            UpdateUrineBlendShape();

            elapsedTime += updateInterval;
            yield return new WaitForSeconds(updateInterval);
        }

        urineVolume = targetUrineVolume;
        UpdateUrineBlendShape();
    }


    private void UpdateUrineBlendShape()
    {
        float urinePercentage = Mathf.Clamp((urineVolume - minUrineVolume) / (maxUrineVolume - minUrineVolume) * 100f, 0f, 100f);

        UpdateBlendShapeCategoryContribution(urineBlendShapeName, "UrineVolume", urinePercentage);
    }




    private IEnumerator AdjustPeristalticVelocity(float peristalticVelocityImpact)
    {
        
        float originalPeristalticVelocity = peristalticVelocity;
        float targetPeristalticVelocity = peristalticVelocity + peristalticVelocityImpact;
        targetPeristalticVelocity = Mathf.Clamp(targetPeristalticVelocity, minPeristalticVelocity, maxPeristalticVelocity);

        // Adjust peristalticVelocity over 3 game minutes
        DateTime startTime = timeManager.GetCurrentInGameTime();
        DateTime endTime = startTime.AddMinutes(3f);

        while (timeManager.GetCurrentInGameTime() < endTime)
        {
            float t = (float)(timeManager.GetCurrentInGameTime() - startTime).TotalMinutes / 3f;
            peristalticVelocity = Mathf.Lerp(originalPeristalticVelocity, targetPeristalticVelocity, t);
            peristalticVelocity = Mathf.Clamp(peristalticVelocity, minPeristalticVelocity, maxPeristalticVelocity);
            yield return null;
        }

        peristalticVelocity = targetPeristalticVelocity;

        // Wait for 24 game hours
        DateTime waitEndTime = timeManager.GetCurrentInGameTime().AddHours(24f);

        while (timeManager.GetCurrentInGameTime() < waitEndTime)
        {
            yield return null;
        }

        // Revert peristalticVelocity back to original over 3 game minutes
        DateTime revertStartTime = timeManager.GetCurrentInGameTime();
        DateTime revertEndTime = revertStartTime.AddMinutes(3f);

        while (timeManager.GetCurrentInGameTime() < revertEndTime)
        {
            float t = (float)(timeManager.GetCurrentInGameTime() - revertStartTime).TotalMinutes / 3f;
            peristalticVelocity = Mathf.Lerp(targetPeristalticVelocity, originalPeristalticVelocity, t);
            peristalticVelocity = Mathf.Clamp(peristalticVelocity, minPeristalticVelocity, maxPeristalticVelocity);
            yield return null;
        }

        peristalticVelocity = originalPeristalticVelocity;
    }



    private IEnumerator AdjustHealth(float healthImpact)
    {
        float startingHealth = currentHealth;
        float targetHealth = currentHealth + healthImpact;
        targetHealth = Mathf.Clamp(targetHealth, minHealth, maxHealth);

        // Adjust health over 3 game minutes
        DateTime startTime = timeManager.GetCurrentInGameTime();
        DateTime endTime = startTime.AddMinutes(3f);

        while (timeManager.GetCurrentInGameTime() < endTime)
        {
            float t = (float)(timeManager.GetCurrentInGameTime() - startTime).TotalMinutes / 3f;
            currentHealth = Mathf.Lerp(startingHealth, targetHealth, t);
            currentHealth = Mathf.Clamp(currentHealth, minHealth, maxHealth);
            yield return null;
        }

        currentHealth = targetHealth;

        // Check for death if health drops to minHealth
        if (currentHealth <= minHealth)
        {
            currentHealth = minHealth;
            Debug.Log("Character has died due to health reaching minimum.");
            // Implement death logic here
        }
    }



    private void StartWeightHealthCheck()
    {
        if (weightHealthCoroutine == null)
        {
            weightHealthCoroutine = StartCoroutine(WeightHealthCheckRoutine());
        }
    }

    private IEnumerator WeightHealthCheckRoutine()
    {
        while (true)
        {
            // Wait for 24 game hours
            DateTime nextCheckTime = timeManager.GetCurrentInGameTime().AddHours(24f);
            yield return new WaitUntil(() => timeManager.GetCurrentInGameTime() >= nextCheckTime);

            // Check weight and adjust health
            if (weight <= borderHazardousLowWeight)
            {
                // Calculate health decrease amount
                float healthDecrease = CalculateHealthDecreaseLowWeight(weight, minWeight, borderHazardousLowWeight, 2f);
                StartCoroutine(DecreaseHealthOverTime(healthDecrease, 3f)); // Decrease over 3 game minutes
            }
            else if (weight >= borderHazardousHighWeight)
            {
                // Calculate health decrease amount
                float healthDecrease = CalculateHealthDecreaseHighWeight(weight, maxWeight, borderHazardousHighWeight, 2f);
                StartCoroutine(DecreaseHealthOverTime(healthDecrease, 3f)); // Decrease over 3 game minutes
            }
        }
    }

    // Helper methods to calculate health decrease based on weight
    private float CalculateHealthDecreaseLowWeight(float currentWeight, float minWeight, float borderWeight, float maxHealthDecrease)
    {
        float weightRatio = Mathf.Clamp01((borderWeight - currentWeight) / (borderWeight - minWeight));
        return weightRatio * maxHealthDecrease;
    }

    private float CalculateHealthDecreaseHighWeight(float currentWeight, float maxWeight, float borderWeight, float maxHealthDecrease)
    {
        float weightRatio = Mathf.Clamp01((currentWeight - borderWeight) / (maxWeight - borderWeight));
        return weightRatio * maxHealthDecrease;
    }

    // Coroutine to decrease health over time
    private IEnumerator DecreaseHealthOverTime(float amount, float durationInGameMinutes)
    {
        float startingHealth = currentHealth;
        float targetHealth = Mathf.Max(minHealth, currentHealth - amount);

        DateTime startTime = timeManager.GetCurrentInGameTime();
        DateTime endTime = startTime.AddMinutes(durationInGameMinutes);

        while (timeManager.GetCurrentInGameTime() < endTime)
        {
            float t = (float)(timeManager.GetCurrentInGameTime() - startTime).TotalMinutes / durationInGameMinutes;
            currentHealth = Mathf.Lerp(startingHealth, targetHealth, t);
            currentHealth = Mathf.Clamp(currentHealth, minHealth, maxHealth);
            yield return null;
        }

        currentHealth = targetHealth;

        // Check for death
        if (currentHealth <= minHealth)
        {
            currentHealth = minHealth;
            Debug.Log("Character has died due to health reaching minimum.");
            // Implement death logic here
        }
    }

    //public float GetBlendShapeValue(string blendShapeName)
    //{
    //    int index = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
    //    if (index >= 0)
    //    {
    //        return skinnedMeshRenderer.GetBlendShapeWeight(index);
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"Blend shape '{blendShapeName}' not found.");
    //        return 0f;
    //    }
    //}

    public float GetBlendShapeValue(string blendShapeName)
    {
        BlendShapeData blendShape = GetBlendShapeData(blendShapeName) ?? facialExpressionBlendShapes.FirstOrDefault(bs => bs.blendShapeName == blendShapeName);
        if (blendShape != null && blendShape.renderer != null)
        {
            int blendShapeIndex = blendShape.blendShapeIndex;
            return blendShape.renderer.GetBlendShapeWeight(blendShapeIndex);
        }
        else
        {
            Debug.LogWarning($"Blend shape '{blendShapeName}' not found.");
            return 0f;
        }
    }

    public void SetBlendShapeValue(string blendShapeName, float value)
    {
        foreach (var renderer in skinnedMeshRenderers)
        {
            int index = renderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
            if (index >= 0)
            {
                renderer.SetBlendShapeWeight(index, value);
            }

            // Update the blend shape on all relevant renderers
            SetBlendShapeWeightOnAllRenderers(blendShapeName, value);

            // Ensure blend shapes are synchronized
            if (blendShapeSyncScript != null)
            {
                blendShapeSyncScript.shouldSync = true;
            }
        }
        //else
        //{
        //    Debug.LogWarning($"Blend shape '{blendShapeName}' not found.");
        //}
    }


    


    public void SetFacialExpression(string blendShapeName, float targetValue)
    {
        // Find the blend shape data for the expression
        BlendShapeData blendShape = facialExpressionBlendShapes.FirstOrDefault(bs => bs.blendShapeName == blendShapeName);

        if (blendShape != null)
        {
            // Stop any existing coroutine
            if (blendShape.blendShapeCoroutine != null)
            {
                StopCoroutine(blendShape.blendShapeCoroutine);
            }

            // Start coroutine to transition the blend shape value
            blendShape.blendShapeCoroutine = StartCoroutine(FacialExpressionTransition(blendShape, targetValue));
        }
        else
        {
            Debug.LogWarning($"Facial expression '{blendShapeName}' not found.");
        }
    }

    private IEnumerator FacialExpressionTransition(BlendShapeData blendShape, float targetValue)
    {
        // Get the real-time duration for 0.5 game minutes
        float duration = timeManager.GetRealTimeDurationForGameMinutes(0.5f);

        float elapsedTime = 0f;
        float startingValue = blendShape.renderer.GetBlendShapeWeight(blendShape.blendShapeIndex);
        float updateInterval = 0.1f;

        while (elapsedTime < duration)
        {
            float newValue = Mathf.Lerp(startingValue, targetValue, elapsedTime / duration);
            blendShape.renderer.SetBlendShapeWeight(blendShape.blendShapeIndex, newValue);

            if (blendShapeSyncScript != null)
            {
                blendShapeSyncScript.shouldSync = true;
            }

            elapsedTime += updateInterval;
            yield return new WaitForSeconds(updateInterval);
        }

        blendShape.renderer.SetBlendShapeWeight(blendShape.blendShapeIndex, targetValue);

        if (blendShapeSyncScript != null)
        {
            blendShapeSyncScript.shouldSync = true;
        }

        blendShape.blendShapeCoroutine = null;
    }



    public void ConsumeItem(EatableItemData eatableData)
    {
        if (currentFullness + eatableData.volume > stomachCapacity)
        {
            Debug.Log("Player is too full.");
            return;
        }
        if (eatableData.hydration < 0f && currentHydration + eatableData.hydration < minHydration)
        {
            Debug.Log("Cannot consume: hydration too low.");
            return;
        }

        // Add to sessionConsumedCalories indirectly via UpdateStatsOnEating
        UpdateStatsOnEating(
            eatableData.volume,
            eatableData.calories,
            eatableData.hydration,
            eatableData.peristalticVelocityImpact,
            eatableData.healthImpact,
            eatableData.timeToEat
        );

        StartCoroutine(DecreaseStaminaOverTime(eatableData.staminaSpentToEat, eatableData.timeToEat));
    }



    public void RecalculateTargetWeightAndStartTransition()
    {
        float effectiveCalories = GetEffectiveCalories();
        float newWeight = Mathf.Clamp(startWeight + (effectiveCalories / caloriesPerKg), minWeight, maxWeight);
        weight = newWeight; // Set weight immediately
        UpdateWeightBlendShapeContributions();
    }


    


    private BlendShapeData GetBlendShapeData(string blendShapeName)
    {
        // Combine them
        var allWeightGainShapes = boobsWeightGainBlendShapes
            .Concat(torsoWeightGainBlendShapes)
            .Concat(thighsWeightGainBlendShapes)
            .Concat(shinsWeightGainBlendShapes)
            .Concat(armsWeightGainBlendShapes)
            .Concat(wholeBodyWeightGainBlendShapes)
            .Concat(glutesWeightGainBlendShapes);

        // Merge everything
        var allShapes = blendShapes
            .Concat(allWeightGainShapes)
            .Concat(weightLossBlendShapes);

        // Try to find the blend shape in that set
        var blendShape = allShapes.FirstOrDefault(bs => bs.blendShapeName == blendShapeName);
        if (blendShape != null) return blendShape;

        // If not found, also check facial expressions
        var facial = facialExpressionBlendShapes.FirstOrDefault(bs => bs.blendShapeName == blendShapeName);
        return facial;
    }

    private void UpdateBlendShapeCategoryContribution(string blendShapeName, string category, float value, float customDuration = 60f)
    {
        var blendShape = GetBlendShapeData(blendShapeName);
        if (blendShape == null)
        {
            Debug.LogWarning($"Blend shape '{blendShapeName}' not found.");
            return;
        }
        blendShape.categoryContributions[category] = value;
        UpdateBlendShape(blendShapeName, customDuration);
    }



    private IEnumerator MuscleDecreaseRoutine()
    {
        DateTime lastCheck = timeManager.GetCurrentInGameTime();
        while (true)
        {
            DateTime currentInGameTime = timeManager.GetCurrentInGameTime();
            TimeSpan timeElapsed = currentInGameTime - lastCheck;

            if (timeElapsed.TotalMinutes >= muscleDecreaseIntervalMinutes)
            {
                int intervals = Mathf.FloorToInt((float)(timeElapsed.TotalMinutes / muscleDecreaseIntervalMinutes));
                for (int i = 0; i < intervals; i++)
                {
                    // Decrease muscles
                    currentMuscles = GetEffectiveMusclesState();

                    if (currentMuscles > musclesMinState)
                    {
                        // Decrease muscles proportionally
                        sessionDecreasedMuscles += musclesDecreasePerGameMinute;
                        // Recalculate and update blend shapes
                        UpdateMuscleBlendShapeContributions();
                    }
                }

                lastCheck = lastCheck.AddMinutes(intervals * muscleDecreaseIntervalMinutes);
            }

            yield return null;
        }
    }

    public void UpdateMuscleBlendShapeContributions()
    {
        currentMuscles = GetEffectiveMusclesState();
        currentMuscles = Mathf.Clamp(currentMuscles, musclesMinState, musclesMaxState);

        // If > 50, we use muscle gain blend shapes
        // Map 50 (no gain) to 0% and 100 (max) to 100%
        if (currentMuscles > 50f)
        {
            float pct = Mathf.Clamp((currentMuscles - 50f) / 50f * 100f, 0f, 100f);
            // Set muscle gain blend shape
            foreach (var bs in muscleGainBlendShapes)
                UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Muscles", pct, muscleBlendShapeUpdateDuration);

            // Set muscle loss blend shape to 0
            foreach (var bs in muscleLossBlendShapes)
                UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Muscles", 0f, muscleBlendShapeUpdateDuration);
        }
        else if (currentMuscles < 50f)
        {
            // If < 50, we use muscle loss blend shapes
            // Map 50 (no loss) to 0% and 0 (min) to 100%
            float pct = Mathf.Clamp((50f - currentMuscles) / 50f * 100f, 0f, 100f);
            foreach (var bs in muscleLossBlendShapes)
                UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Muscles", pct, muscleBlendShapeUpdateDuration);

            // Set muscle gain blend shape to 0
            foreach (var bs in muscleGainBlendShapes)
                UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Muscles", 0f, muscleBlendShapeUpdateDuration);
        }
        else
        {
            // Exactly 50 means no gain/loss
            foreach (var bs in muscleGainBlendShapes)
                UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Muscles", 0f, muscleBlendShapeUpdateDuration);
            foreach (var bs in muscleLossBlendShapes)
                UpdateBlendShapeCategoryContribution(bs.blendShapeName, "Muscles", 0f, muscleBlendShapeUpdateDuration);
        }
    }

    public float GetEffectiveMusclesState()
    {
        return loadedMusclesState + sessionIncreasedMuscles - sessionDecreasedMuscles;
    }



    public void ApplyBaseCharacterProfile(CharacterProfile profile)
    {
        if (profile == null)
        {
            Debug.LogWarning("ApplyBaseCharacterProfile called with null profile!");
            return;
        }

        // For each base blend shape in the chosen profile...
        foreach (var setting in profile.baseBlendShapes)
        {
            // Use the existing method that sets the blend shape on *every* sub-mesh.
            SetBlendShapeWeightOnAllRenderers(setting.blendShapeName, setting.value);
        }

        boobsGainEnabled = profile.enableBoobGain;
        torsoGainEnabled = profile.enableTorsoGain;
        thighsGainEnabled = profile.enableThighsGain;
        shinsGainEnabled = profile.enableShinsGain;
        armsGainEnabled = profile.enableArmsGain;
        wholeBodyGainEnabled = profile.enableWholeBodyGain;
        glutesGainEnabled = profile.enableGlutesGain;

        // If you want to ensure the BlendShapeSyncAutoExtract sees these changes:
        if (blendShapeSyncScript != null)
            blendShapeSyncScript.shouldSync = true;
    }



    public float GetWeightSpeedMultiplier()
    {
        // For weight = startWeight => multiplier = 1.0
        // For weight = maxWeight   => multiplier = 0.5
        float range = maxWeight - startWeight;
        if (range <= 0f) return 1f; // avoid division by zero

        float ratio = (weight - startWeight) / range;
        ratio = Mathf.Clamp01(ratio); // 0..1
        float multiplier = Mathf.Lerp(1.0f, 0.8f, ratio); // from 1 down to 0.8
        return multiplier;
    }

    public bool CanSprintBasedOnWeight()
    {
        // If weight is more than 80% from start to max, disable sprint
        float threshold = startWeight + 0.8f * (maxWeight - startWeight);
        return (weight < threshold);
    }






}
