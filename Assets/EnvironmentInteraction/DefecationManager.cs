using UnityEngine;
using System.Collections;
using System;

public class DefecationManager : MonoBehaviour
{
    public static DefecationManager instance;

    private InGameTimeManager timeManager;

    private PlayerControllerCode playerController;
    private CameraController cameraController;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private PlayerInteraction playerInteraction;

    [Header("Defecation Settings")]
    public float defecationDurationInGameMinutes = 5f;

    [Header("Animation Settings")]
    public string sitAnimationTrigger = "SitOnToilet"; // Name of the trigger in Animator
    public string standUpAnimationTrigger = "StandUp"; // Name of the trigger in Animator

    [Header("References")]
    public CharacterStats characterStats;

    // New fields for disabling physics
    [Header("Player Components to Disable")]
    public GameObject playerPhysicsObject; // Assign the GameObject containing the Rigidbody and Collider
    private Rigidbody playerRigidbody;
    private Collider playerCollider;

    // New fields for adjusting the toilet component
    [Header("Toilet Component to Adjust")]
    public GameObject toiletComponentToMove; // Assign the toilet component to move (e.g., seat or lid)
    public Transform toiletComponentTargetTransform; // Assign the target transform during defecation

    private Vector3 toiletComponentOriginalPosition;
    private Quaternion toiletComponentOriginalRotation;

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
        cameraController = FindObjectOfType<CameraController>();
        playerInteraction = playerController.GetComponent<PlayerInteraction>();

        if (characterStats == null)
        {
            characterStats = FindObjectOfType<CharacterStats>();
            if (characterStats == null)
            {
                Debug.LogError("CharacterStats reference is not assigned in DefecationManager.");
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
            Debug.LogError("DefecationManager: playerPhysicsObject is not assigned.");
        }
    }

    public void StartDefecation(InteractableToilet toilet)
    {
        if (playerController == null)
        {
            Debug.LogError("DefecationManager: playerController is null.");
            return;
        }

        if (toilet == null)
        {
            Debug.LogError("DefecationManager: toilet parameter is null.");
            return;
        }

        if (toilet.sitPosition == null)
        {
            Debug.LogError("DefecationManager: toilet's sitPosition is not assigned.");
            return;
        }

        if (cameraController == null)
        {
            Debug.LogError("DefecationManager: cameraController is null.");
            return;
        }

        // Save original position and rotation
        originalPosition = playerController.transform.position;
        originalRotation = playerController.transform.rotation;

        // Move player to toilet's sit position
        playerController.transform.position = toilet.sitPosition.position;
        playerController.transform.rotation = toilet.sitPosition.rotation;

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

        // Switch camera to toilet camera position
        cameraController.SwitchToDefecationCamera(toilet.toiletCameraPosition);

        // Play sit animation
        Animator anim = playerController.GetComponent<Animator>();

        // Disable player interaction
        if (playerInteraction != null)
            playerInteraction.enabled = false;

        anim.SetTrigger(sitAnimationTrigger);

        // Move and rotate the toilet component
        if (toiletComponentToMove != null && toiletComponentTargetTransform != null)
        {
            // Save original position and rotation
            toiletComponentOriginalPosition = toiletComponentToMove.transform.position;
            toiletComponentOriginalRotation = toiletComponentToMove.transform.rotation;

            // Move to target position and rotation
            toiletComponentToMove.transform.position = toiletComponentTargetTransform.position;
            toiletComponentToMove.transform.rotation = toiletComponentTargetTransform.rotation;

            Debug.Log("Moved toilet component to target position.");
        }
        else
        {
            Debug.LogWarning("toiletComponentToMove or toiletComponentTargetTransform is null.");
        }

        // Start the defecation coroutine
        StartCoroutine(DefecationCoroutine(anim));
    }

    private IEnumerator DefecationCoroutine(Animator anim)
    {
        // Wait for the sit animation to finish (adjust as necessary)
        yield return new WaitForSeconds(2f);

        // Fast-forward in-game time
        float originalTimeMultiplier = timeManager.timeMultiplier;
        float targetTimeMultiplier = 1000f; // Arbitrary large number for fast-forward effect
        float accelerationDuration = 1f; // Seconds to reach target time multiplier

        float elapsedTime = 0f;
        while (elapsedTime < accelerationDuration)
        {
            elapsedTime += Time.deltaTime;
            timeManager.timeMultiplier = Mathf.Lerp(originalTimeMultiplier, targetTimeMultiplier, elapsedTime / accelerationDuration);
            yield return null;
        }

        timeManager.timeMultiplier = targetTimeMultiplier;

        // Wait for defecation duration in game minutes
        DateTime startTime = timeManager.GetCurrentInGameTime();
        DateTime endTime = startTime.AddMinutes(defecationDurationInGameMinutes);
        while (timeManager.GetCurrentInGameTime() < endTime)
        {
            yield return null;
        }

        // Exponentially decrease time multiplier back to normal
        elapsedTime = 0f;
        while (elapsedTime < accelerationDuration)
        {
            elapsedTime += Time.deltaTime;
            timeManager.timeMultiplier = Mathf.Lerp(targetTimeMultiplier, originalTimeMultiplier, elapsedTime / accelerationDuration);
            yield return null;
        }

        timeManager.timeMultiplier = originalTimeMultiplier;

        anim.SetTrigger(standUpAnimationTrigger);

        // Allow a frame for the Animator to process the trigger
        yield return null;

        anim.ResetTrigger(sitAnimationTrigger);

        // Restore toilet component to original position and rotation
        if (toiletComponentToMove != null)
        {
            toiletComponentToMove.transform.position = toiletComponentOriginalPosition;
            toiletComponentToMove.transform.rotation = toiletComponentOriginalRotation;
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

        // Reset player control
        playerController.EnablePlayerControl();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Re-enable opening CharacterUI
        UIManager.instance.EnableCharacterUIOpening();

        // NEW: also re-enable in-game menu
        InGameMenuController.instance.EnableMenuOpening();


        if (playerInteraction != null)
            playerInteraction.enabled = true;

        // Update character stats
        UpdateStatsAfterDefecation();
    }

    private void UpdateStatsAfterDefecation()
    {
        // Calculate fullness lost
        float fullnessLost = characterStats.currentIntFullness;

        // Decrease currentIntFullness to zero
        characterStats.currentIntFullness = 0f;

        // Update intestines fullness blend shapes
        characterStats.UpdateIntestinesFullnessBlendShapes();

        // Show notification
        NotificationManager.instance.ShowNotification(
            $"You have relieved yourself and lost {fullnessLost} ml of faeces.",
            textColor: Color.green,
            backgroundColor: Color.black,
            backgroundSprite: null,
            startPos: new Vector2(0f, 200f),
            endPos: new Vector2(0f, 200f),
            moveDur: 2f,
            fadeDur: 0.5f,
            displayDur: 2f
        );
    }
}
