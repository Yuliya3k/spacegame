using UnityEngine;
using System.Collections;
using System;

public class SinkManager : MonoBehaviour
{
    // Removed the singleton instance
    // public static SinkManager instance;

    private PlayerControllerCode playerController;
    private CinemachineCameraController cameraController;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private PlayerInteraction playerInteraction;

    [Header("Animation Settings")]
    public string washHandsAnimationTrigger = "WashHands";
    public string drinkAnimationTrigger = "DrinkFromSink";
    public string overfullAnimationTrigger = "OverfullReaction";
    public string StopAnimTrigger = "StopAnimTrigger";

    [Header("References")]
    public CharacterStats characterStats; // Assign in the Inspector
    public PlayerInventory playerInventory; // Assign in the Inspector
    public SinkUIManager sinkUIManager; // Assign in the Inspector

    [Header("Player Components to Disable")]
    public GameObject playerPhysicsObject; // Assign the GameObject containing the Rigidbody and Collider
    private Rigidbody playerRigidbody;
    private Collider playerCollider;

    [Header("Positions and Transforms")]
    public Transform washHandsPosition;       // Assign in the Inspector
    public Transform washHandsCameraPosition; // Assign in the Inspector

    public Transform drinkPosition;           // Assign in the Inspector
    public Transform drinkCameraPosition;     // Assign in the Inspector

    [Header("Dedicated Cameras (optional)")]
    public Camera washHandsCamera;            // Optional dedicated camera
    public Camera drinkCamera;                // Optional dedicated camera
    public Transform overfullPosition;        // Assign in the Inspector

    [Header("Sounds")]
    public AudioSource sinkAudioSource;       // Assign in the Inspector
    public AudioClip waterRunningSound;       // Assign in the Inspector
    public AudioClip overfullSound;           // Assign in the Inspector

    [Header("Drinking Settings")]
    public EatableItemData waterItem;         // Assign the water EatableItemData scriptable object
    public float drinkingIntervalInGameMinutes = 0.5f; // Adjustable interval
    public int overfullIgnoredTimesLimit = 10;

    private bool isPerformingAction = false;
    private Coroutine currentActionCoroutine;

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
        playerController = PlayerControllerCode.instance;
        cameraController = FindObjectOfType<CinemachineCameraController>();
        playerInteraction = playerController.GetComponent<PlayerInteraction>();

        if (characterStats == null)
        {
            Debug.LogError("CharacterStats reference is not assigned in SinkManager.");
        }

        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<PlayerInventory>();
            if (playerInventory == null)
            {
                Debug.LogError("PlayerInventory not found in the scene.");
            }
        }

        // Get player Rigidbody and Collider
        if (playerPhysicsObject != null)
        {
            playerRigidbody = playerPhysicsObject.GetComponent<Rigidbody>();
            playerCollider = playerPhysicsObject.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("SinkManager: playerPhysicsObject is not assigned.");
        }

        if (sinkAudioSource == null)
        {
            Debug.LogError("SinkManager: sinkAudioSource is not assigned.");
        }
    }

    public void StartWashHands(InteractableSink sink)
    {
        if (isPerformingAction)
            return;

        isPerformingAction = true;

        // Disable opening CharacterUI
        UIManager.instance.DisableCharacterUIOpening();

        // NEW: also disable in-game menu opening
        InGameMenuController.instance.DisableMenuOpening();

        // Save original position and rotation
        originalPosition = playerController.transform.position;
        originalRotation = playerController.transform.rotation;

        // Move player to wash hands position
        playerController.transform.position = washHandsPosition.position;
        playerController.transform.rotation = washHandsPosition.rotation;

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

        // Switch camera to wash hands view
        originalCameraPosition = cameraController.transform.position;
        originalCameraRotation = cameraController.transform.rotation;
        if (washHandsCamera != null)
        {
            if (washHandsCameraPosition != null)
            {
                washHandsCamera.transform.SetPositionAndRotation(
                    washHandsCameraPosition.position,
                    washHandsCameraPosition.rotation);
            }
            cameraController.SwitchToSleepCamera(washHandsCamera);
        }
        else if (washHandsCameraPosition != null)
        {
            cameraController.transform.position = washHandsCameraPosition.position;
            cameraController.transform.rotation = washHandsCameraPosition.rotation;
            cameraController.DisableCameraControl();
        }
        else
        {
            cameraController.DisableCameraControl();
        }

        // Play wash hands animation
        Animator anim = playerController.GetComponent<Animator>();
        anim.SetTrigger(washHandsAnimationTrigger);

        // Play water running sound
        if (sinkAudioSource != null && waterRunningSound != null)
        {
            sinkAudioSource.clip = waterRunningSound;
            sinkAudioSource.loop = true;
            sinkAudioSource.Play();
        }

        // Start coroutine to wait until action is stopped
        currentActionCoroutine = StartCoroutine(WashHandsCoroutine());
    }

    private IEnumerator WashHandsCoroutine()
    {
        // Wait indefinitely until the action is stopped
        while (isPerformingAction)
        {
            yield return null;
        }

        EndCurrentAction();
    }

    public void StartDrinking(InteractableSink sink)
    {
        if (isPerformingAction)
            return;

        isPerformingAction = true;

        // Save original position and rotation
        originalPosition = playerController.transform.position;
        originalRotation = playerController.transform.rotation;

        // Move player to drink position
        playerController.transform.position = drinkPosition.position;
        playerController.transform.rotation = drinkPosition.rotation;

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

        // Switch camera to drink view
        originalCameraPosition = cameraController.transform.position;
        originalCameraRotation = cameraController.transform.rotation;
        if (drinkCamera != null)
        {
            if (drinkCameraPosition != null)
            {
                drinkCamera.transform.SetPositionAndRotation(
                    drinkCameraPosition.position,
                    drinkCameraPosition.rotation);
            }
            cameraController.SwitchToSleepCamera(drinkCamera);
        }
        else if (drinkCameraPosition != null)
        {
            cameraController.transform.position = drinkCameraPosition.position;
            cameraController.transform.rotation = drinkCameraPosition.rotation;
            cameraController.DisableCameraControl();
        }
        else
        {
            cameraController.DisableCameraControl();
        }
        // Play drink animation
        Animator anim = playerController.GetComponent<Animator>();
        anim.SetTrigger(drinkAnimationTrigger);

        // Play water running sound
        if (sinkAudioSource != null && waterRunningSound != null)
        {
            sinkAudioSource.clip = waterRunningSound;
            sinkAudioSource.loop = true;
            sinkAudioSource.Play();
        }

        // Start drinking coroutine
        currentActionCoroutine = StartCoroutine(DrinkingCoroutine());
    }

    private IEnumerator DrinkingCoroutine()
    {
        int overfullIgnoredTimes = 0;

        while (isPerformingAction)
        {
            // Check if character is too full to drink more
            if (characterStats.currentFullness >= characterStats.stomachCapacity)
            {
                overfullIgnoredTimes++;

                if (overfullIgnoredTimes >= overfullIgnoredTimesLimit)
                {
                    // Trigger overfull event
                    StartCoroutine(OverfullEvent());
                    yield break;
                }
            }
            else
            {
                overfullIgnoredTimes = 0;

                // Consume water item using PlayerInventory
                ConsumeWater();

                // Wait for the specified in-game time interval
                DateTime startTime = characterStats.timeManager.GetCurrentInGameTime();
                DateTime endTime = startTime.AddMinutes(drinkingIntervalInGameMinutes);

                while (characterStats.timeManager.GetCurrentInGameTime() < endTime)
                {
                    yield return null;
                }
            }

            yield return null;
        }

        EndCurrentAction();
    }

    private void ConsumeWater()
    {
        if (waterItem == null)
        {
            Debug.LogError("SinkManager: waterItem is not assigned.");
            return;
        }

        // Temporarily add the water item to the inventory
        playerInventory.AddItem(waterItem);

        // Use the EatItem method to consume the water
        playerInventory.EatItem(waterItem);

        // Optionally remove the water item if necessary
        characterStats.UpdateFullnessBlendShapes();
    }

    private IEnumerator OverfullEvent()
    {
        // Stop current action
        isPerformingAction = false;

        // Move player to overfull position
        playerController.transform.position = overfullPosition.position;
        playerController.transform.rotation = overfullPosition.rotation;

        // Play overfull animation
        Animator anim = playerController.GetComponent<Animator>();
        anim.SetTrigger(overfullAnimationTrigger);

        // Play overfull sound
        if (sinkAudioSource != null && overfullSound != null)
        {
            sinkAudioSource.clip = overfullSound;
            sinkAudioSource.loop = false;
            sinkAudioSource.Play();
        }

        // Decrease currentFullness to zero
        characterStats.currentFullness = 0f;
        characterStats.UpdateFullnessBlendShapes();

        // Wait for animation duration (adjust as needed)
        yield return new WaitForSeconds(3f);


        EndCurrentAction();
    }

    public void StopCurrentAction()
    {
        if (isPerformingAction)
        {
            isPerformingAction = false;

            if (currentActionCoroutine != null)
            {
                StopCoroutine(currentActionCoroutine);
                currentActionCoroutine = null;
            }

            EndCurrentAction();
        }
    }

    private void EndCurrentAction()
    {
        // Reset animations
        Animator anim = playerController.GetComponent<Animator>();
        anim.ResetTrigger(washHandsAnimationTrigger);
        anim.ResetTrigger(drinkAnimationTrigger);
        anim.ResetTrigger(overfullAnimationTrigger);

        anim.SetTrigger(StopAnimTrigger);

        if (sinkUIManager != null)
        {
            sinkUIManager.OnStopButtonClicked();
        }

        // Stop sounds
        if (sinkAudioSource != null)
        {
            sinkAudioSource.Stop();
        }

        // Re-enable Rigidbody and Collider
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        // Restore player position and rotation
        playerController.transform.position = originalPosition;
        playerController.transform.rotation = originalRotation;

        // Switch camera back to normal
        cameraController.SwitchToNormalCamera();
        cameraController.transform.position = originalCameraPosition;
        cameraController.transform.rotation = originalCameraRotation;
        originalCameraPosition = Vector3.zero;
        originalCameraRotation = Quaternion.identity;
        cameraController.EnableCameraControl();
        // Reset flags
        isPerformingAction = false;

        // Re-enable opening CharacterUI
        UIManager.instance.EnableCharacterUIOpening();

        // NEW: also re-enable in-game menu
        InGameMenuController.instance.EnableMenuOpening();
    }
}
