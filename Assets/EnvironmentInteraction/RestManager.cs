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

public class RestManager : MonoBehaviour
{
    // Removed singleton pattern
    // public static RestManager instance;

    private InGameTimeManager timeManager;

    private PlayerControllerCode playerController;
    private CinemachineCameraController cameraController;


    public GameObject playerPhysicsObject;
    private Rigidbody playerRigidbody;
    private Collider playerCollider;

    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private PlayerInteraction playerInteraction;

    [Header("Rest Settings")]
    public float staminaRestoredPerHour = 20f; // Configurable in Inspector

    [Header("Animation Settings")]
    public string restAnimationTrigger = "Rest"; // Name of the trigger in Animator
    public string wakeUpAnimationTrigger = "WakeUp"; // Name of the wake-up trigger
    public int numberOfRestAnimations = 1; // Set this to the number of rest animations you have

    [Header("Blend Shape Settings")]
    public List<BlendShapeSetting> restBlendShapes = new List<BlendShapeSetting>();

    private Dictionary<string, float> originalBlendShapeValues = new Dictionary<string, float>();

    [Header("References")]
    public CharacterStats characterStats;
    public RestUIManager restUIManager; // Assign via the Inspector

    private void Awake()
    {
        // Removed singleton pattern
        // if (instance == null)
        //     instance = this;
        // else
        //     Destroy(gameObject);
    }

    private void Start()
    {
        timeManager = FindObjectOfType<InGameTimeManager>();

        playerController = PlayerControllerCode.instance;
        cameraController = FindObjectOfType<CinemachineCameraController>();
        playerInteraction = playerController.GetComponent<PlayerInteraction>();

        if (characterStats == null)
        {
            Debug.LogError("CharacterStats reference is not assigned in RestManager.");
        }

        if (restUIManager == null)
        {
            Debug.LogError("RestManager: restUIManager reference is not assigned.");
        }

        // Get player Rigidbody and Collider
        if (playerPhysicsObject != null)
        {
            playerRigidbody = playerPhysicsObject.GetComponent<Rigidbody>();
            playerCollider = playerPhysicsObject.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("RestManager: playerPhysicsObject is not assigned.");
        }

    }

    public void StartRest(int restHours, InteractableRestPlace restPlace)
    {
        ApplyRestBlendShapes();
        TooltipManager.instance.HideTooltip();

        // Save original position and rotation
        originalPosition = playerController.transform.position;
        originalRotation = playerController.transform.rotation;

        // Move player to rest place's position
        playerController.transform.position = restPlace.restPosition.position;
        playerController.transform.rotation = restPlace.restPosition.rotation;

        // Switch camera to rest place camera position
        originalCameraPosition = cameraController.transform.position;
        originalCameraRotation = cameraController.transform.rotation;
        cameraController.SwitchToRestCamera(restPlace.restCameraPosition);

        // Disable Rigidbody and Collider
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }


        // Play rest animation
        Animator anim = playerController.GetComponent<Animator>();

        // Set random rest animation index if multiple animations are used
        if (numberOfRestAnimations > 1)
        {
            int randomIndex = UnityEngine.Random.Range(0, numberOfRestAnimations);
            anim.SetFloat("RestAnimIndex", randomIndex);
        }

        anim.SetTrigger(restAnimationTrigger);

        // Start the rest coroutine
        StartCoroutine(RestCoroutine(restHours, anim));
    }

    private IEnumerator RestCoroutine(int restHours, Animator anim)
    {
        float originalTimeMultiplier = timeManager.timeMultiplier;
        float fastForwardMultiplier = 1000f; // or another large number

        // Immediately set time multiplier high
        timeManager.timeMultiplier = fastForwardMultiplier;

        // Calculate end time
        DateTime startTime = timeManager.GetCurrentInGameTime();
        DateTime endTime = startTime.AddHours(restHours);

        // Wait until currentInGameTime >= endTime
        while (timeManager.GetCurrentInGameTime() < endTime)
        {
            yield return null; // Check each frame
        }

        // Once done, revert time multiplier instantly
        timeManager.timeMultiplier = originalTimeMultiplier;

        // Proceed with wake-up logic...
        anim.SetTrigger(wakeUpAnimationTrigger);
        yield return null;
        anim.ResetTrigger(restAnimationTrigger);

        playerController.transform.position = originalPosition;
        playerController.transform.rotation = originalRotation;
        cameraController.SwitchToNormalCamera();
        cameraController.transform.position = originalCameraPosition;
        cameraController.transform.rotation = originalCameraRotation;
        originalCameraPosition = Vector3.zero;
        originalCameraRotation = Quaternion.identity;
        playerController.EnablePlayerControl();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        // Re-enable opening CharacterUI
        UIManager.instance.EnableCharacterUIOpening();

        // NEW: also re-enable in-game menu
        InGameMenuController.instance.EnableMenuOpening();

        if (playerInteraction != null)
            playerInteraction.enabled = true;

        RestoreBlendShapes();
        UpdateStatsAfterRest(restHours);
        restUIManager.CloseRestUI();

        NotificationManager.instance.ShowNotification(
            $"You rested for {restHours} hours.",
            textColor: Color.white,
            backgroundColor: Color.black,
            null,
            new Vector2(0f, 200f),
            new Vector2(0f, 200f),
            4f,
            2f,
            0f
        );
    }


    private void UpdateStatsAfterRest(int restHours)
    {
        // Re-enable Rigidbody and Collider
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        // Restore stamina
        float staminaIncrease = staminaRestoredPerHour * restHours;
        characterStats.stamina = Mathf.Min(characterStats.stamina + staminaIncrease, characterStats.maxStamina);
    }

    private void ApplyRestBlendShapes()
    {
        foreach (var blendShapeSetting in restBlendShapes)
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
