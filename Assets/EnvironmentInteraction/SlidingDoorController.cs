using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoorController : MonoBehaviour
{
    [Header("Doors Settings")]
    public List<DoorSettings> doors = new List<DoorSettings>();  // List of doors and their offsets
    public float openTime = 1f;  // Time for the door to fully open
    public float closeTime = 1f;  // Time for the door to fully close
    public bool autoClose = true;  // Should the doors close automatically?
    public float autoCloseDelay = 3f;  // Delay before auto-closing the doors

    [Header("Interaction Settings")]
    public bool canInteract = true;  // Enable pressing E to open the doors
    public bool autoOpenOnLook = true;  // Automatically open when the player looks at the doors
    public float interactionDistance = 3f;  // Max distance to open the doors
    public LayerMask interactableLayers;  // Specify which layers can be interacted with, including doors

    [Header("UI Settings")]
    public string interactTooltip = "Press E to open the doors";
    public string autoOpenTooltip = "Doors opening...";
    public string doorLocation = "Main Entrance"; // Name of the door to display in the tooltip

    [Header("Audio Settings")]
    public AudioSource audioSource;  // AudioSource component to play sounds
    public AudioClip openingSound;  // Sound for door opening
    public AudioClip closingSound;  // Sound for door closing

    private Camera playerCamera;  // Reference to the player's camera
    private bool areDoorsOpen = false;  // To track door state
    private bool isMoving = false;  // To check if the doors are already moving
    private Coroutine autoCloseCoroutine;

    public string objectName = "Sliding Door";
    public string locationText = "Main Entrance";

    public GameObject tooltipUI; // Root GameObject of the tooltip UI

    public bool AreDoorsOpen { get { return areDoorsOpen; } }
    public bool IsMoving { get { return isMoving; } }

    private void Start()
    {
        // Get the player's camera
        playerCamera = Camera.main;

        // Store initial positions of all doors
        foreach (var door in doors)
        {
            door.initialPosition = door.doorTransform.localPosition;
        }
    }

    private void Update()
    {
        // Implementation for interaction or automatic opening (if required)
    }

    public void ToggleDoors()
    {
        if (areDoorsOpen)
        {
            CloseDoors();
        }
        else
        {
            OpenDoors();
        }
    }

    public void OpenDoors()
    {
        if (!areDoorsOpen && !isMoving)
        {
            StartCoroutine(MoveDoors(true));
            areDoorsOpen = true;

            if (autoClose)
            {
                if (autoCloseCoroutine != null)
                {
                    StopCoroutine(autoCloseCoroutine);
                }
                autoCloseCoroutine = StartCoroutine(CloseAfterDelay());
            }
        }
    }

    public void CloseDoors()
    {
        if (areDoorsOpen && !isMoving)
        {
            StartCoroutine(MoveDoors(false));
            areDoorsOpen = false;
        }
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseDoors();
    }

    // Move the doors based on direction with smooth closing and sound handling
    private IEnumerator MoveDoors(bool opening)
    {
        isMoving = true;

        float elapsedTime = 0f;
        float moveDuration = opening ? openTime : closeTime;  // Use different times for opening and closing

        // Play the appropriate sound
        PlaySound(opening);

        // Get the start and target positions for each door
        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> targetPositions = new List<Vector3>();

        for (int i = 0; i < doors.Count; i++)
        {
            Vector3 startPosition = doors[i].doorTransform.localPosition;
            Vector3 targetPosition;

            if (opening)
            {
                targetPosition = startPosition + doors[i].openPositionOffset;
            }
            else
            {
                targetPosition = doors[i].initialPosition;
            }

            startPositions.Add(startPosition);
            targetPositions.Add(targetPosition);
        }

        // Move doors over time smoothly
        while (elapsedTime < moveDuration)
        {
            for (int i = 0; i < doors.Count; i++)
            {
                doors[i].doorTransform.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], elapsedTime / moveDuration);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure doors are set to final positions at the end
        for (int i = 0; i < doors.Count; i++)
        {
            doors[i].doorTransform.localPosition = targetPositions[i];
        }

        isMoving = false;

        // Stop the sound once the doors finish moving
        StopSound();
    }

    // Play the appropriate sound based on whether the doors are opening or closing
    private void PlaySound(bool opening)
    {
        if (audioSource != null)
        {
            audioSource.clip = opening ? openingSound : closingSound;
            audioSource.Play();
        }
    }

    // Stop the current sound
    private void StopSound()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
