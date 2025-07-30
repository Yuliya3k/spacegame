using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;



//[System.Serializable]
//public class BlendShapeSetting
//{
//    public string blendShapeName;
//    public float value;
//}


public class NPCSleepManager : MonoBehaviour
{
    public static NPCSleepManager instance;

    public static NPCSleepManager Instance
    {
        get { return instance; }
    }


    private InGameTimeManager timeManager;
    
    private PlayerControllerCode playerController;
    private CinemachineCameraController cameraController;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalCameraTransform;

    private PlayerInteraction playerInteraction;

    [Header("Sleep Settings")]
    public float staminaRestoredPerHour = 20f; // Configurable in Inspector

    [Header("Capacity Increase Settings")]
    public float capacityIncreaseAmount = 100f; // Amount to increase capacities
    public float capacityIncreaseThreshold = 0.8f; // Threshold (80%)

    [Header("Animation Settings")]
    public string sleepAnimationTrigger = "Sleep"; // Name of the trigger in Animator
    public string wakeUpAnimationTrigger = "WakeUp"; // Name of the wake-up trigger
    public int numberOfSleepAnimations = 1; // Set this to the number of sleep animations you have

    [Header("Blend Shape Settings")]
    public List<BlendShapeSetting> sleepBlendShapes = new List<BlendShapeSetting>();

    // Add this field to SleepManager
    private Dictionary<string, float> originalBlendShapeValues = new Dictionary<string, float>();

    [Header("References")]
    public CharacterStats characterStats;





    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        timeManager = FindObjectOfType<InGameTimeManager>();
        
        playerController = PlayerControllerCode.instance;
        cameraController = FindObjectOfType<CinemachineCameraController>();
        playerInteraction = playerController.GetComponent<PlayerInteraction>(); // Add this line

        if (characterStats == null)
        {
            Debug.LogError("CharacterStats reference is not assigned in SleepManager.");
        }

    }

    public void StartSleep(int sleepHours, InteractableBed bed)
    {
        

        ApplySleepBlendShapes();
        TooltipManager.instance.HideTooltip();
        // Save original position and rotation
        originalPosition = playerController.transform.position;
        originalRotation = playerController.transform.rotation;
        originalCameraTransform = cameraController.transform;

        // Move player to bed's sleep position
        playerController.transform.position = bed.sleepPosition.position;
        playerController.transform.rotation = bed.sleepPosition.rotation;

        // Switch camera to bed camera position
        cameraController.SwitchToSleepCamera(bed.bedCamera);

        // Play sleep animation
        Animator anim = playerController.GetComponent<Animator>();

        //if (playerInteraction != null)
        //    playerInteraction.enabled = false;

        // Set random sleep animation index if multiple animations are used
        if (numberOfSleepAnimations > 1)
        {
            int randomIndex = UnityEngine.Random.Range(0, numberOfSleepAnimations);
            anim.SetFloat("SleepAnimIndex", randomIndex);
        }


        // Increase capacities based on conditions
        if (characterStats.currentFullness >= capacityIncreaseThreshold * characterStats.stomachCapacity)
        {
            characterStats.stomachCapacity = Mathf.Min(characterStats.stomachCapacity + capacityIncreaseAmount, characterStats.maxStomachCapacity);
        }

        if (characterStats.currentIntFullness >= capacityIncreaseThreshold * characterStats.intestinesCapacity)
        {
            characterStats.intestinesCapacity = Mathf.Min(characterStats.intestinesCapacity + capacityIncreaseAmount, characterStats.intestinesMaxCapacity);
        }


        anim.SetTrigger(sleepAnimationTrigger);

        // Start the sleep coroutine
        StartCoroutine(SleepCoroutine(sleepHours, anim));
    }


    private IEnumerator SleepCoroutine(int sleepHours, Animator anim)
    {
        float originalTimeMultiplier = timeManager.timeMultiplier;
        float fastForwardMultiplier = 1000f; // A large number for fast forwarding

        // Immediately set time multiplier high
        timeManager.timeMultiplier = fastForwardMultiplier;

        // Calculate end time
        DateTime startTime = timeManager.GetCurrentInGameTime();
        DateTime endTime = startTime.AddHours(sleepHours);

        // Wait until currentInGameTime >= endTime
        while (timeManager.GetCurrentInGameTime() < endTime)
        {
            yield return null; // Check each frame
        }

        // Once done, revert time multiplier instantly
        timeManager.timeMultiplier = originalTimeMultiplier;

        // Proceed with wake-up logic...
        anim.SetTrigger("WakeUp"); // Optional: Trigger wake-up animation

        // Allow a frame for the Animator to process the trigger
        yield return null;

        anim.ResetTrigger(sleepAnimationTrigger);

        // Restore player position and rotation
        playerController.transform.position = originalPosition;
        playerController.transform.rotation = originalRotation;

        // Switch camera back to normal
        cameraController.SwitchToNormalCamera();

        // Reset player control
        playerController.EnablePlayerControl();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerInteraction != null)
            playerInteraction.enabled = true;

        RestoreBlendShapes();

        SleepUIManager.instance.CloseSleepUI();

        // Re-enable opening CharacterUI
        UIManager.instance.EnableCharacterUIOpening();

        // NEW: also re-enable in-game menu
        InGameMenuController.instance.EnableMenuOpening();

        UpdateStatsAfterSleep(sleepHours);

        // Show sleep notification
        NotificationManager.instance.ShowNotification(
            $"You slept for {sleepHours} hours.",
            textColor: new Color(255f, 255f, 255f, 255f),
            backgroundColor: new Color(255f, 255f, 255f, 255f),
            backgroundSprite: null,
            startPos: new Vector2(0f, 200f),
            endPos: new Vector2(0f, 200f),
            moveDur: 4f,
            fadeDur: 2f,
            displayDur: 0f
        );
    }


    private void UpdateStatsAfterSleep(int sleepHours)
    {
        // Restore stamina
        float staminaIncrease = staminaRestoredPerHour * sleepHours;
        characterStats.stamina = Mathf.Min(characterStats.stamina + staminaIncrease, characterStats.maxStamina);

        

        // Ensure capacities don't exceed maximum values
        characterStats.stomachCapacity = Mathf.Min(characterStats.stomachCapacity, characterStats.maxStomachCapacity);
        characterStats.intestinesCapacity = Mathf.Min(characterStats.intestinesCapacity, characterStats.intestinesMaxCapacity);
    }



    private void ApplySleepBlendShapes()
    {
        foreach (var blendShapeSetting in sleepBlendShapes)
        {
            // Get the current value of the blend shape from CharacterStats
            float currentValue = characterStats.GetBlendShapeValue(blendShapeSetting.blendShapeName);

            // Store the original value
            if (!originalBlendShapeValues.ContainsKey(blendShapeSetting.blendShapeName))
            {
                originalBlendShapeValues.Add(blendShapeSetting.blendShapeName, currentValue);
            }

            // Set the facial expression
            characterStats.SetFacialExpression(blendShapeSetting.blendShapeName, blendShapeSetting.value, blendShapeSetting.durationInGameMinutes);
        }
    }

    private void RestoreBlendShapes()
    {
        foreach (var kvp in originalBlendShapeValues)
        {
            // Restore the facial expression
            characterStats.SetFacialExpression(kvp.Key, kvp.Value, -1f);
        }

        // Clear the dictionary
        originalBlendShapeValues.Clear();
    }








}
