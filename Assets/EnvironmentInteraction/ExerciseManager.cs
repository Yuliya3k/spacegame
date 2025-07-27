using UnityEngine;
using System.Collections;
using System;

public class ExerciseManager : MonoBehaviour
{
    [Header("References")]
    public CharacterStats characterStats; // Assign via Inspector
    public PlayerControllerCode playerController;
    private CinemachineCameraController cameraController;
    public InGameTimeManager timeManager;
    public ExerciseUIManager exerciseUIManager;
    private PlayerInteraction playerInteraction;

    [Header("Exercise Settings")]
    public float musclesGainedPerHour = 3f;
    public string exerciseAnimationTrigger = "Exercise";
    public string finishExerciseAnimationTrigger = "FinishExercise";
    public int numberOfExerciseAnimations = 1;
    public float staminaDecreasedPerHour = 15f;

    [Header("Time Settings")]
    public float timeAccelerationMultiplier = 1000f;
    public float accelerationDuration = 1f;

    [Header("Player Components to Disable")]
    public GameObject playerPhysicsObject; // Assign the GameObject with Rigidbody and Collider
    private Rigidbody playerRigidbody;
    private Collider playerCollider;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private InteractableExerciseMachine currentMachine;
    private int exerciseHours;
    private bool stopRequested = false; // To handle early stop

    

    private void Start()
    {
        if (characterStats == null)
        {
            characterStats = FindObjectOfType<CharacterStats>();
            if (characterStats == null)
            {
                Debug.LogError("ExerciseManager: CharacterStats not found.");
            }
        }

        if (playerController == null)
        {
            playerController = PlayerControllerCode.instance;
        }

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CinemachineCameraController>();
        }

        if (timeManager == null)
        {
            timeManager = FindObjectOfType<InGameTimeManager>();
        }

        if (playerController != null)
        {
            playerInteraction = playerController.GetComponent<PlayerInteraction>();
        }

        if (playerPhysicsObject != null)
        {
            playerRigidbody = playerPhysicsObject.GetComponent<Rigidbody>();
            playerCollider = playerPhysicsObject.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("ExerciseManager: playerPhysicsObject is not assigned.");
        }

        //exerciseUIManager = FindObjectOfType<ExerciseUIManager>();
        if (exerciseUIManager == null)
        {
            Debug.LogError("ExerciseManager: ExerciseUIManager not found.");
        }
    }

    public void StartExercise(int hours, InteractableExerciseMachine machine)
    {
        currentMachine = machine;
        exerciseHours = hours;
        stopRequested = false;

        // Check if enough stamina
        float requiredStamina = staminaDecreasedPerHour * exerciseHours;
        if (characterStats.stamina < requiredStamina)
        {
            // Not enough stamina, show a tooltip or notification
            
            NotificationManager.instance.ShowNotification(
                "Not enough Stamina to complete this training!",
                textColor: Color.red,
                backgroundColor: Color.red,
                backgroundSprite: null,
                startPos: new Vector2(0f, 200f),
                endPos: new Vector2(0f, 200f),
                moveDur: 2f,
                fadeDur: 0.5f,
                displayDur: 2f
            );
            // Re-enable player control
            playerController.EnablePlayerControl();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (playerInteraction != null)
                playerInteraction.enabled = true;
            new WaitForSeconds(2f);
            exerciseUIManager.RestoreHiddenUI();
            return;
        }

        TooltipManager.instance.HideTooltip();

        // Save original position and rotation
        originalPosition = playerController.transform.position;
        originalRotation = playerController.transform.rotation;

        // Move player to exercise machine's position
        playerController.transform.position = machine.exercisePosition.position;
        playerController.transform.rotation = machine.exercisePosition.rotation;

        // Switch camera
        cameraController.SwitchToRestCamera(machine.exerciseCameraPosition);

        //// Disable Rigidbody and Collider
        //if (playerRigidbody != null)
        //{
        //    playerRigidbody.isKinematic = true;
        //    playerRigidbody.linearVelocity = Vector3.zero;
        //    playerRigidbody.angularVelocity = Vector3.zero;
        //}

        //if (playerCollider != null)
        //{
        //    playerCollider.enabled = false;
        //}

        // Play exercise animation
        Animator anim = playerController.GetComponent<Animator>();

        // If multiple exercise animations, pick a random one
        if (numberOfExerciseAnimations > 1)
        {
            int randomIndex = UnityEngine.Random.Range(0, numberOfExerciseAnimations);
            anim.SetFloat("ExerciseAnimIndex", randomIndex);
        }

        anim.SetTrigger(exerciseAnimationTrigger);

        // Show In-Progress UI
        if (exerciseUIManager != null)
        {
            exerciseUIManager.ShowInProgressUI();
        }

        StartCoroutine(ExerciseCoroutine(anim));
    }

    public void RequestStopExercise()
    {
        stopRequested = true;
    }

    private IEnumerator ExerciseCoroutine(Animator anim)
    {
        // Wait a second for the animation to start (adjust if needed)
        yield return new WaitForSeconds(1f);

        float originalTimeMultiplier = timeManager.timeMultiplier;
        float targetTimeMultiplier = timeAccelerationMultiplier;

        // Accelerate time
        float elapsedTime = 0f;
        while (elapsedTime < accelerationDuration)
        {
            elapsedTime += Time.deltaTime;
            timeManager.timeMultiplier = Mathf.Lerp(originalTimeMultiplier, targetTimeMultiplier, elapsedTime / accelerationDuration);
            yield return null;
        }

        timeManager.timeMultiplier = targetTimeMultiplier;

        // Wait for in-game hours to pass or until stop is requested
        DateTime startTime = timeManager.GetCurrentInGameTime();
        DateTime plannedEndTime = startTime.AddHours(exerciseHours);

        bool finishedNaturally = false;

        while (!stopRequested && timeManager.GetCurrentInGameTime() < plannedEndTime)
        {
            yield return null;
        }

        if (!stopRequested && timeManager.GetCurrentInGameTime() >= plannedEndTime)
        {
            // Completed full exercise hours
            finishedNaturally = true;
        }

        // Decelerate time back to normal
        elapsedTime = 0f;
        while (elapsedTime < accelerationDuration)
        {
            elapsedTime += Time.deltaTime;
            timeManager.timeMultiplier = Mathf.Lerp(targetTimeMultiplier, originalTimeMultiplier, elapsedTime / accelerationDuration);
            yield return null;
        }

        timeManager.timeMultiplier = originalTimeMultiplier;

        // Trigger finish exercise animation
        anim.SetTrigger(finishExerciseAnimationTrigger);
        yield return null;
        anim.ResetTrigger(exerciseAnimationTrigger);

        // Restore player and camera
        playerController.transform.position = originalPosition;
        playerController.transform.rotation = originalRotation;
        cameraController.SwitchToNormalCamera();

        // Re-enable physics
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        // Re-enable player control
        playerController.EnablePlayerControl();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (playerInteraction != null)
            playerInteraction.enabled = true;

        // Close In-Progress UI
        if (exerciseUIManager != null)
        {
            exerciseUIManager.CloseInProgressUI();
        }

        // Calculate how many hours actually exercised
        DateTime endTime = timeManager.GetCurrentInGameTime();
        double totalHoursExercised = (endTime - startTime).TotalHours;
        if (stopRequested && totalHoursExercised > exerciseHours)
        {
            // If stopping early after surpassing planned hours due to some timing glitch, clamp
            totalHoursExercised = exerciseHours;
        }
        else if (stopRequested && totalHoursExercised < 0)
        {
            totalHoursExercised = 0; // Just a sanity check
        }

        UpdateStatsAfterExercise((float)totalHoursExercised);

        // After updating stats (exercise ended), re-show all previously hidden UI
        // This covers both stop and natural end scenarios.
        
        
        

    }

    private void UpdateStatsAfterExercise(float hoursExercised)
    {
        //exerciseUIManager.RestoreHiddenUI();

        float musclesGained = musclesGainedPerHour * hoursExercised;
        characterStats.sessionIncreasedMuscles += musclesGained;

        // Optionally clamp if you never want effective muscles to exceed the max:
        characterStats.sessionIncreasedMuscles = Mathf.Min(characterStats.sessionIncreasedMuscles, characterStats.musclesMaxState - characterStats.loadedMusclesState);

        // Decrease stamina based on hoursExercised
        float staminaDecrease = staminaDecreasedPerHour * hoursExercised;
        characterStats.stamina = Mathf.Max(characterStats.stamina - staminaDecrease, characterStats.minStamina);

        characterStats.UpdateMuscleBlendShapeContributions();

        // Show notification
        NotificationManager.instance.ShowNotification(
            $"You exercised for {hoursExercised:0.00} hours and gained {musclesGained:0.00} muscle points.\n" +
            $"You lost {staminaDecrease:0.00} stamina.",
            textColor: Color.green,
            backgroundColor: Color.black,
            backgroundSprite: null,
            startPos: new Vector2(0f, 200f),
            endPos: new Vector2(0f, 200f),
            moveDur: 2f,
            fadeDur: 0.5f,
            displayDur: 2f
        );

        exerciseUIManager.RestoreHiddenUI();

    }
}
