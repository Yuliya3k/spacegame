using System.Collections;
using UnityEngine;
using TMPro;  // For tooltips

public class SlidingDoor : MonoBehaviour
{
    [Header("Door Movement Settings")]
    public Vector3 openPositionOffset = new Vector3(1f, 0, 0); // Local position offset for the door
    public float openTime = 1f;  // Time for the door to fully open

    [Header("Auto Close Settings")]
    public bool autoClose = true;  // Should the door close automatically?
    public float autoCloseDelay = 3f;  // Delay before auto-closing the door

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private bool isOpen = false;
    private bool isMoving = false;
    private Coroutine autoCloseCoroutine;

    [Header("Interaction Settings")]
    public bool canInteract = true;  // Enable pressing E to open the door
    public bool autoOpenOnLook = true;  // Automatically open when the player looks at the door

    public float interactionDistance = 3f;  // Max distance to open the door
    public LayerMask playerLayer;  // Player layer for raycast detection

    [Header("UI Settings")]
    public TextMeshProUGUI tooltipText;  // UI element for showing tooltips
    public string interactTooltip = "Press E to open the door";
    public string autoOpenTooltip = "Door opening...";

    private Camera playerCamera;  // Reference to the player's camera

    private void Start()
    {
        // Store the door's initial position and calculate the open position
        initialPosition = transform.localPosition;
        targetPosition = initialPosition + openPositionOffset;

        // Find the player's camera
        playerCamera = Camera.main;

        if (tooltipText != null)
        {
            tooltipText.text = "";  // Clear tooltip text at the start
        }
    }

    private void Update()
    {
        if (playerCamera == null) return;

        // Create a ray from the center of the player's camera
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // Perform the raycast to check if the door is in front of the player
        if (Physics.Raycast(ray, out hit, interactionDistance, playerLayer))
        {
            if (hit.collider == GetComponent<Collider>()) // Check if we're hitting this door's collider
            {
                if (canInteract && Input.GetKeyDown(KeyCode.E) && !isMoving)
                {
                    ToggleDoor();
                }

                // Automatically open the door if it's within view and close enough
                if (autoOpenOnLook && !isOpen && !isMoving)
                {
                    OpenDoor();
                }

                // Display tooltips if the player can interact or auto-open
                if (tooltipText != null)
                {
                    if (canInteract)
                    {
                        tooltipText.text = interactTooltip;  // Show "Press E to open" tooltip
                    }
                    else if (autoOpenOnLook)
                    {
                        tooltipText.text = autoOpenTooltip;  // Show "Door opening..." tooltip
                    }
                }

                return; // Exit early to avoid resetting the tooltip
            }
        }

        // Clear the tooltip text when the player is not looking at the door
        if (tooltipText != null)
        {
            tooltipText.text = "";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Auto open when player enters the trigger zone (if auto-open based on look is not used)
        if (!autoOpenOnLook && !isOpen && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            OpenDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Auto close when the player leaves the trigger zone
        if (autoClose && isOpen && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
            }
            autoCloseCoroutine = StartCoroutine(CloseAfterDelay());
        }
    }

    private void ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    public void OpenDoor()
    {
        if (!isOpen && !isMoving)
        {
            StartCoroutine(MoveDoor(targetPosition));
            isOpen = true;

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

    public void CloseDoor()
    {
        if (isOpen && !isMoving)
        {
            StartCoroutine(MoveDoor(initialPosition));
            isOpen = false;
        }
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseDoor();
    }

    private IEnumerator MoveDoor(Vector3 destination)
    {
        isMoving = true;

        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < openTime)
        {
            transform.localPosition = Vector3.Lerp(startPosition, destination, elapsedTime / openTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = destination;
        isMoving = false;
    }
}
