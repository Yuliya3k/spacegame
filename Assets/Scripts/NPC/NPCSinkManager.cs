using System.Collections;
using UnityEngine;

public class NPCSinkManager : MonoBehaviour
{
    [Tooltip("Assign the NPCController (if not assigned, it will be taken from this GameObject).")]
    public NPCController npcController;
    [Tooltip("Assign the CharacterStats component (if not assigned, it will be taken from NPCController).")]
    public CharacterStats characterStats;
    [Tooltip("Assign the NPCInventory component (if not assigned, it will be taken from this GameObject).")]
    public NPCInventory npcInventory;
    [Tooltip("Assign the GameObject that contains the NPC's Rigidbody and Collider.")]
    public GameObject npcPhysicsObject;

    [Header("Animation Settings")]
    public string drinkAnimationTrigger = "DrinkFromSink";
    public string stopAnimTrigger = "StopAnimTrigger";

    [Header("Positions and Transforms")]
    [Tooltip("The designated drink position (set in the Inspector).")]
    public Transform drinkPosition;

    [Header("Sounds")]
    [Tooltip("Assign the AudioSource for sink sounds.")]
    public AudioSource sinkAudioSource;
    [Tooltip("Assign the water running sound clip.")]
    public AudioClip waterRunningSound;

    [Header("Drinking Settings")]
    [Tooltip("Assign the water item (EatableItemData).")]
    public EatableItemData waterItem;
    [Tooltip("Drinking cycle duration (in in-game minutes).")]
    public float drinkingIntervalInGameMinutes = 0.5f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody npcRigidbody;
    private Collider npcCollider;
    private bool isPerformingAction = false;
    private Coroutine currentActionCoroutine;

    private void Start()
    {
        if (npcController == null)
        {
            npcController = GetComponent<NPCController>();
            if (npcController == null)
            {
                Debug.LogError("NPCSinkManager: NPCController not found on this GameObject.");
            }
        }
        if (characterStats == null && npcController != null)
        {
            characterStats = npcController.characterStats;
            if (characterStats == null)
            {
                Debug.LogError("NPCSinkManager: CharacterStats not assigned and not found on NPCController.");
            }
        }
        if (npcInventory == null)
        {
            npcInventory = GetComponent<NPCInventory>();
            if (npcInventory == null)
            {
                Debug.LogError("NPCSinkManager: NPCInventory not found on this GameObject.");
            }
        }
        if (npcPhysicsObject != null)
        {
            npcRigidbody = npcPhysicsObject.GetComponent<Rigidbody>();
            npcCollider = npcPhysicsObject.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("NPCSinkManager: npcPhysicsObject is not assigned.");
        }
        if (sinkAudioSource == null)
        {
            Debug.LogError("NPCSinkManager: sinkAudioSource is not assigned.");
        }
    }

    public bool IsPerformingAction()
    {
        return isPerformingAction;
    }

    public void StartDrinkingForNPC()
    {
        if (isPerformingAction)
            return;

        isPerformingAction = true;

        // Save the NPC's original position and rotation.
        originalPosition = npcController.transform.position;
        originalRotation = npcController.transform.rotation;

        // Stop NPC movement.
        if (npcController.navAgent != null)
        {
            npcController.navAgent.isStopped = true;
        }

        // Move NPC to the designated drink position.
        npcController.transform.position = drinkPosition.position;
        npcController.transform.rotation = drinkPosition.rotation;

        // Disable physics by setting Rigidbody to kinematic.
        if (npcRigidbody != null)
        {
            npcRigidbody.isKinematic = true;
        }
        if (npcCollider != null)
        {
            npcCollider.enabled = false;
        }

        // Play the drink animation.
        Animator anim = npcController.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger(drinkAnimationTrigger);
        }

        // Play water running sound.
        if (sinkAudioSource != null && waterRunningSound != null)
        {
            sinkAudioSource.clip = waterRunningSound;
            sinkAudioSource.loop = true;
            sinkAudioSource.Play();
        }

        currentActionCoroutine = StartCoroutine(DrinkingCoroutine());
    }

    private IEnumerator DrinkingCoroutine()
    {
        int maxCycles = 20;
        int cycleCount = 0;
        float previousFullness = characterStats.currentFullness;

        while (isPerformingAction &&
               characterStats.currentFullness < characterStats.stomachCapacity &&
               cycleCount < maxCycles)
        {
            ConsumeWater();

            System.DateTime startTime = characterStats.timeManager.GetCurrentInGameTime();
            System.DateTime endTime = startTime.AddMinutes(drinkingIntervalInGameMinutes);
            while (characterStats.timeManager.GetCurrentInGameTime() < endTime)
            {
                yield return null;
            }
            cycleCount++;

            if (Mathf.Approximately(characterStats.currentFullness, previousFullness))
            {
                Debug.Log("NPCSinkManager: Fullness did not increase this cycle; ending drinking loop.");
                break;
            }
            previousFullness = characterStats.currentFullness;
        }

        EndDrinkingActionForNPC();
    }

    private void ConsumeWater()
    {
        if (waterItem == null)
        {
            Debug.LogError("NPCSinkManager: waterItem is not assigned.");
            return;
        }

        npcInventory.AddItem(waterItem);
        npcInventory.EatItem(waterItem);
        characterStats.UpdateFullnessBlendShapes();
    }

    private void EndDrinkingActionForNPC()
    {
        if (sinkAudioSource != null)
        {
            sinkAudioSource.Stop();
        }

        Animator anim = npcController.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger(stopAnimTrigger);
        }
        else
        {
            Debug.LogWarning("NPCSinkManager: Animator not found on NPC.");
        }

        if (npcRigidbody != null)
        {
            npcRigidbody.isKinematic = false;
        }
        if (npcCollider != null)
        {
            npcCollider.enabled = true;
        }

        npcController.transform.position = originalPosition;
        npcController.transform.rotation = originalRotation;

        if (npcController.navAgent != null)
        {
            npcController.navAgent.isStopped = false;
            npcController.navAgent.ResetPath();
        }

        isPerformingAction = false;
        Debug.Log("NPCSinkManager: Drinking action ended for NPC.");
    }
}
