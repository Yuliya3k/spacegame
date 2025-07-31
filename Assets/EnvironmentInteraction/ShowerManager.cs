using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages shower or hose actions entirely in game-time.
/// </summary>
public class ShowerManager : MonoBehaviour
{
    private PlayerControllerCode playerController;
    private CinemachineCameraController cameraController;
    private PlayerInteraction playerInteraction;

    [Header("Shower Actions")]
    public string takeShowerAnimationTrigger = "TakeShower";
    public string rinseBodyAnimationTrigger = "RinseBody";
    public string stopAnimTrigger = "StopAnimTrigger"; // optional stop trigger

    [Header("Positions and Transforms")]
    public Transform takeShowerPosition;
    public Transform takeShowerCameraPosition;
    public Transform rinseBodyPosition;
    public Transform rinseBodyCameraPosition;

    [Header("References")]
    public CharacterStats characterStats;      // Assign in Inspector
    public PlayerInventory playerInventory;    // Assign in Inspector
    public ShowerUIManager showerUIManager;    // Assign in Inspector

    [Header("UI Management")]
    public List<GameObject> uisToHideOnShowerUse; // Assign in Inspector

    [Header("Player Components to Disable")]
    public GameObject playerPhysicsObject;
    private Rigidbody playerRigidbody;
    private Collider playerCollider;

    // ====== NEW: Audio Fields =======
    [Header("Audio")]
    [Tooltip("AudioSource used to play shower/hose loops or one-shots.")]
    public AudioSource showerAudioSource;

    [Tooltip("These clips (1 or more) are used for the shower loop or random pick. (Shower only)")]
    public List<AudioClip> showerLoopClips;

    [Tooltip("These clips (1 or more) are used for the hose loop or random pick. (Hose only)")]
    public List<AudioClip> hoseLoopClips;

    [Tooltip("This clip is used for emptying forcibly. It's single sound (like a 'pop').")]
    public AudioClip emptyingSound;

    [Tooltip("These random clips will play each time capacity is reached, before forcedStopCounter hits 10.")]
    public List<AudioClip> capacityReachedClips;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private bool isPerformingAction = false;
    private Coroutine currentActionCoroutine;

    private InteractableShower currentShower;
    private List<ItemDataEquipment> previouslyUnequippedItems = new List<ItemDataEquipment>();
    private string currentActionTrigger = "";

    private bool isHoseAction = false;
    private Coroutine hoseFullnessCoroutine = null;

    void Start()
    {
        playerController = PlayerControllerCode.instance;
        cameraController = FindObjectOfType<CinemachineCameraController>();
        playerInteraction = playerController.GetComponent<PlayerInteraction>();

        if (characterStats == null)
        {
            Debug.LogError("ShowerManager: CharacterStats reference is not assigned.");
        }

        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<PlayerInventory>();
            if (playerInventory == null)
            {
                Debug.LogError("ShowerManager: PlayerInventory not found in scene.");
            }
        }

        if (showerUIManager == null)
        {
            Debug.LogError("ShowerManager: showerUIManager reference is not assigned.");
        }

        // Physics
        if (playerPhysicsObject != null)
        {
            playerRigidbody = playerPhysicsObject.GetComponent<Rigidbody>();
            playerCollider = playerPhysicsObject.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("ShowerManager: playerPhysicsObject is not assigned.");
        }
    }

    // ==================== PUBLIC ENTRY POINTS ====================

    public void StartTakeShower(InteractableShower shower)
    {
        if (isPerformingAction) return;
        currentShower = shower;
        currentActionTrigger = takeShowerAnimationTrigger;
        isHoseAction = false;
        StartShowerAction(takeShowerPosition, takeShowerCameraPosition, false);
    }

    public void StartRinseBody(InteractableShower shower)
    {
        if (isPerformingAction) return;
        currentShower = shower;
        currentActionTrigger = rinseBodyAnimationTrigger;
        isHoseAction = true;
        StartShowerAction(rinseBodyPosition, rinseBodyCameraPosition, true);
    }

    // ==================== MAIN SETUP ACTION ====================

    private void StartShowerAction(Transform actionPosition, Transform actionCameraPosition, bool isHose)
    {
        isPerformingAction = true;

        // Hide UI
        UIManager.instance.DisableCharacterUIOpening();

        // Save position
        originalPosition = playerController.transform.position;
        originalRotation = playerController.transform.rotation;

        // Move player
        playerController.transform.position = actionPosition.position;
        playerController.transform.rotation = actionPosition.rotation;

        // Disable physics
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

        // Switch camera
        originalCameraPosition = cameraController.transform.position;
        originalCameraRotation = cameraController.transform.rotation;

        cameraController.transform.position = actionCameraPosition.position;
        cameraController.transform.rotation = actionCameraPosition.rotation;
        cameraController.DisableCameraControl();

        // Unequip clothes except hair
        UnequipAllClothesExceptHair();

        // Play chosen animation
        Animator anim = playerController.GetComponent<Animator>();
        anim.SetTrigger(currentActionTrigger);

        // Disable PlayerInteraction
        if (playerInteraction != null) playerInteraction.enabled = false;

        // =========== Play loop or random pick for this action ============
        StartLoopOrRandomSound(isHose);

        // Start main coroutine
        currentActionCoroutine = StartCoroutine(ShowerActionCoroutine());
    }

    // ==================== MAIN COROUTINE ====================

    private IEnumerator ShowerActionCoroutine()
    {
        // If this is the hose action, run the fullness logic
        if (isHoseAction)
        {
            hoseFullnessCoroutine = StartCoroutine(HoseFullnessRoutine());
        }

        // Wait indefinitely until StopCurrentAction is called
        while (isPerformingAction)
        {
            yield return null;
        }

        EndCurrentAction();
    }

    // ==================== PHYSICS & CLOTHES ====================

    private void UnequipAllClothesExceptHair()
    {
        previouslyUnequippedItems.Clear();
        List<InventoryItem> equippedItems = playerInventory.GetEquippedItems();

        foreach (var item in equippedItems)
        {
            if (item.data is ItemDataEquipment eq)
            {
                bool isHair = false;
                foreach (var slotType in eq.equipmentSlots)
                {
                    if (slotType == EquipmentType.Hair)
                    {
                        isHair = true;
                        break;
                    }
                }

                if (!isHair)
                {
                    previouslyUnequippedItems.Add(eq);
                }
            }
        }

        // Unequip all non-hair items
        foreach (var eqItem in previouslyUnequippedItems)
        {
            playerInventory.UnequipItem(eqItem);
        }
    }

    private void ReequipAllPreviouslyUnequippedItems()
    {
        foreach (var eqItem in previouslyUnequippedItems)
        {
            playerInventory.EquipItem(eqItem);
        }
        previouslyUnequippedItems.Clear();
    }

    // ==================== STOP LOGIC ====================

    public void StopCurrentAction()
    {
        if (!isPerformingAction) return;
        isPerformingAction = false;
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }
        EndCurrentAction();
    }

    private void EndCurrentAction()
    {
        // Reset animations
        Animator anim = playerController.GetComponent<Animator>();
        anim.ResetTrigger(takeShowerAnimationTrigger);
        anim.ResetTrigger(rinseBodyAnimationTrigger);
        if (!string.IsNullOrEmpty(stopAnimTrigger)) anim.SetTrigger(stopAnimTrigger);

        // Re-equip clothes
        ReequipAllPreviouslyUnequippedItems();

        // Re-enable physics
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        // Restore position/camera
        playerController.transform.position = originalPosition;
        playerController.transform.rotation = originalRotation;
        cameraController.SwitchToNormalCamera();
        cameraController.EnableCameraControl();

        // Restore camera transform
        cameraController.transform.position = originalCameraPosition;
        cameraController.transform.rotation = originalCameraRotation;

        // Clear stored camera values
        originalCameraPosition = Vector3.zero;
        originalCameraRotation = Quaternion.identity;

        // Re-enable PlayerInteraction + UI
        if (playerInteraction != null) playerInteraction.enabled = true;
        UIManager.instance.EnableCharacterUIOpening();

        // Restore hidden UI
        foreach (var ui in uisToHideOnShowerUse)
        {
            if (ui != null) ui.SetActive(true);
        }

        // Stop looping sound if playing
        if (showerAudioSource != null && showerAudioSource.isPlaying && showerAudioSource.loop)
        {
            showerAudioSource.loop = false;
            showerAudioSource.Stop();
        }

        isPerformingAction = false;

        // Close the in-progress UI
        showerUIManager?.OnStopButtonClicked();
    }

    // ==================== HOSE LOGIC (GAME TIME ONLY!) ====================

    private IEnumerator HoseFullnessRoutine()
    {
        // We'll track how many intervals occur after capacity is reached:
        int forcedStopCounter = 0;
        bool capacityReached = false;

        // This variable accumulates "time" in game-minutes. 
        // We'll do an update each full 1 minute of *game time*, not real time
        DateTime lastCheck = characterStats.timeManager.GetCurrentInGameTime();

        while (isPerformingAction && isHoseAction)
        {
            // Current in-game time
            DateTime currentTime = characterStats.timeManager.GetCurrentInGameTime();
            TimeSpan elapsed = currentTime - lastCheck;

            // If at least 1 in-game minute passed
            if (elapsed.TotalMinutes >= 1.0)
            {
                int intervals = Mathf.FloorToInt((float)(elapsed.TotalMinutes / 1f));
                for (int i = 0; i < intervals; i++)
                {
                    // If the action was stopped mid-loop, bail out
                    if (!isPerformingAction || !isHoseAction) yield break;

                    // If we haven't reached capacity, add fullness
                    if (!capacityReached && characterStats.currentIntFullness < characterStats.intestinesCapacity)
                    {
                        // We'll add 50 total each "minute" of game time
                        float spaceLeft = characterStats.intestinesCapacity - characterStats.currentIntFullness;
                        float fullnessToAdd = Mathf.Min(50f, spaceLeft);

                        // The time block for 1 in-game minute
                        DateTime minuteStart = characterStats.timeManager.GetCurrentInGameTime();
                        DateTime minuteEnd = minuteStart.AddMinutes(1.0);

                        yield return StartCoroutine(AddHoseFullnessGradually(fullnessToAdd, minuteStart, minuteEnd));

                        // If we just reached capacity
                        if (Mathf.Approximately(characterStats.currentIntFullness, characterStats.intestinesCapacity))
                        {
                            capacityReached = true;
                            forcedStopCounter = 0;
                        }
                    }
                    else
                    {
                        // We are at or above capacity
                        forcedStopCounter++;
                        Debug.Log($"[Hose] capacity full. forcedStopCounter={forcedStopCounter}");

                        // If forcedStopCounter < 10, we play a random capacity reached clip
                        if (forcedStopCounter < 10 && capacityReachedClips != null && capacityReachedClips.Count > 0)
                        {
                            int randomIndex = UnityEngine.Random.Range(0, capacityReachedClips.Count);
                            AudioClip randClip = capacityReachedClips[randomIndex];
                            if (randClip != null && showerAudioSource != null && !showerAudioSource.loop)
                            {
                                // play short clip
                                showerAudioSource.PlayOneShot(randClip);
                            }
                        }

                        // If forcedStopCounter hits 10 => forcibly empty
                        if (forcedStopCounter >= 10)
                        {
                            Debug.Log("[Hose] Doing forced emptying (intestines = 0) after 10 intervals at capacity.");

                            // set fullness to zero
                            characterStats.currentIntFullness = 0f;
                            characterStats.UpdateIntestinesFullnessBlendShapes();

                            // Stop the hose looping sound, if it's still looping
                            if (showerAudioSource != null && showerAudioSource.isPlaying && showerAudioSource.loop)
                            {
                                showerAudioSource.loop = false;
                                showerAudioSource.Stop();
                            }

                            // Play single emptyingSound if assigned
                            if (emptyingSound != null && showerAudioSource != null)
                            {
                                // We typically want a one-shot for this:
                                showerAudioSource.PlayOneShot(emptyingSound);
                            }

                            StopCurrentAction();
                            yield break;
                        }

                        // We still wait 1 game minute at capacity
                        DateTime capacityStart = characterStats.timeManager.GetCurrentInGameTime();
                        DateTime capacityEnd = capacityStart.AddMinutes(1.0);
                        // wait until game-time >= capacityEnd
                        while (characterStats.timeManager.GetCurrentInGameTime() < capacityEnd)
                        {
                            if (!isPerformingAction || !isHoseAction) yield break;
                            yield return null;
                        }
                    }
                }
                // Advance our "lastCheck" by the # of intervals we processed
                lastCheck = lastCheck.AddMinutes(intervals);
            }

            yield return null;
        }
    }


    /// <summary>
    /// Gradually adds 'amount' fullness over exactly 1 in-game minute 
    /// from minuteStart to minuteEnd, linearly in *game-time* steps.
    /// </summary>
    private IEnumerator AddHoseFullnessGradually(
        float amount,
        DateTime minuteStart,
        DateTime minuteEnd
    )
    {
        float oldVal = characterStats.currentIntFullness;
        float targetVal = oldVal + amount;

        // We'll loop until the current in-game time >= minuteEnd
        while (true)
        {
            if (!isPerformingAction || !isHoseAction) yield break;

            DateTime now = characterStats.timeManager.GetCurrentInGameTime();
            if (now >= minuteEnd) break;

            // fraction: 0 => 1 across the one minute
            double totalSpan = (minuteEnd - minuteStart).TotalMinutes; // should be 1
            double soFar = (now - minuteStart).TotalMinutes;
            float t = (float)(soFar / totalSpan); // from 0 to 1 across 1 game minute

            float currentF = Mathf.Lerp(oldVal, targetVal, t);
            characterStats.currentIntFullness = currentF;
            characterStats.UpdateIntestinesFullnessBlendShapes();

            Debug.Log("InFullness Updated");

            yield return null; // Wait 1 frame in game-time
        }

        // Once we pass the end time
        characterStats.currentIntFullness = Mathf.Min(targetVal, characterStats.intestinesCapacity);
        characterStats.UpdateIntestinesFullnessBlendShapes();
        Debug.Log("InFullness Updated");
    }

    // ==================== AUDIO HELPERS ====================

    /// <summary>
    /// Called after the animation starts to handle a looping track or random pick
    /// from a list for shower/hose. We either pick a single random clip or first clip,
    /// set loop = true, then Play in the AudioSource.
    /// </summary>
    private void StartLoopOrRandomSound(bool isHose)
    {
        if (showerAudioSource == null) return;

        // choose from showerLoopClips or hoseLoopClips
        List<AudioClip> chosenClips = isHose ? hoseLoopClips : showerLoopClips;
        if (chosenClips == null || chosenClips.Count == 0) return;

        // pick random from the list
        int randIndex = UnityEngine.Random.Range(0, chosenClips.Count);
        AudioClip chosenClip = chosenClips[randIndex];
        if (chosenClip == null) return;

        showerAudioSource.clip = chosenClip;
        showerAudioSource.loop = true;
        showerAudioSource.Play();
    }
}
