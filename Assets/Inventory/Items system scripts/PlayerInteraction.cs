using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;   // Camera reference
    public float interactionDistance = 10f;  // Interaction range

    [Tooltip("Layer mask for interactable raycasts.")]
    public LayerMask interactionLayerMask = Physics.DefaultRaycastLayers;


    private GameObject lastInteractableObject; // Keep track of the last interactable GameObject

    // List of GameObjects to ignore in the raycast
    public List<GameObject> objectsToIgnore = new List<GameObject>();

    // Input Actions
    private PlayerInputActions inputActions;
    private bool interactPressed;

    public static PlayerInteraction instance;



    private void Update()
    {
        // Begin profiler sample to measure allocations in this method
        Profiler.BeginSample("PlayerInteraction.Update");

        // Create a ray from the center of the camera's viewport
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayerMask))

            
        {
            GameObject hitObject = hit.collider.gameObject;

            // Check if the hit object or its parent is in the ignore list
            if (objectsToIgnore.Contains(hitObject) || IsInIgnoreList(hitObject))
            {
                if (lastInteractableObject != null)
                {
                    lastInteractableObject = null;
                    TooltipManager.instance.HideTooltip();
                }
                interactPressed = false;
                Profiler.EndSample();
                return;
            }
        

            //// Check for InteractableUIOpener first
            //var uiOpener = hit.collider.GetComponent<InteractableUIOpener>();
            //if (uiOpener != null)
            //{
            //    if (lastInteractableObject != hitObject)
            //    {
            //        lastInteractableObject = hitObject;
            //        TooltipManager.instance.ShowTooltipIndefinitely(
            //            uiOpener.actionText,
            //            uiOpener.locationText,
            //            uiOpener.objectName
            //        );
            //    }

            //    if (interactPressed)
            //    {
            //        uiOpener.Interact();
            //    }
            //    interactPressed = false;
            //    Profiler.EndSample();
            //    return;  // Exit to avoid further checks
            //}

            //// Check for StorageContainer
            //var storageContainer = hit.collider.GetComponent<StorageContainer>();
            //if (storageContainer != null)
            //{
            //    if (lastInteractableObject != hitObject)
            //    {
            //        lastInteractableObject = hitObject;
            //        TooltipManager.instance.ShowTooltipIndefinitely(
            //            "Press E to open",
            //            storageContainer.locationText,
            //            storageContainer.objectName
            //        );
            //    }

            //    if (interactPressed)
            //    {
            //        storageContainer.Interact();  // Open storage UI
            //    }
            //    interactPressed = false;
            //    Profiler.EndSample();
            //    return;  // Exit to avoid further checks
            //}

            //// Check for DisposableContainer
            //var disposableContainer = hit.collider.GetComponent<DisposableContainer>();
            //if (disposableContainer != null)
            //{
            //    if (lastInteractableObject != hitObject)
            //    {
            //        lastInteractableObject = hitObject;
            //        TooltipManager.instance.ShowTooltipIndefinitely(
            //            "Press E to open",
            //            disposableContainer.locationText,
            //            disposableContainer.objectName
            //        );
            //    }

            //    if (interactPressed)
            //    {
            //        disposableContainer.Interact();
            //    }
            //    interactPressed = false;
            //    Profiler.EndSample();
            //    return;  // Exit after detecting interaction
            //}

            // Check for InteractableObject
            var interactableObject = hit.collider.GetComponent<InteractableObject>();
            if (interactableObject != null)
            {
                if (lastInteractableObject != hitObject)
                {
                    lastInteractableObject = hitObject;
                    TooltipManager.instance.ShowTooltipIndefinitely(
                        "Press E to pick up",
                        interactableObject.locationText,
                        interactableObject.objectName
                    );
                }

                if (interactPressed)
                {
                    interactableObject.OnPickup(); // Handle pickup
                }
                interactPressed = false;
                Profiler.EndSample();
                return;  // Exit to avoid further checks
            }

            //// Check for InteractableToilet
            //var toilet = hit.collider.GetComponent<InteractableToilet>();
            //if (toilet != null)
            //{
            //    if (lastInteractableObject != hitObject)
            //    {
            //        lastInteractableObject = hitObject;
            //        TooltipManager.instance.ShowTooltipIndefinitely(
            //            toilet.actionText,
            //            toilet.locationText,
            //            toilet.objectName
            //        );
            //    }

            //    if (interactPressed)
            //    {
            //        toilet.Interact();
            //    }
            //    interactPressed = false;
            //    Profiler.EndSample();
            //    return; // Exit to avoid further checks
            //}


            //// Check for Interactablesink
            //var sink = hit.collider.GetComponent<InteractableSink>();
            //if (sink != null)
            //{
            //    if (lastInteractableObject != hitObject)
            //    {
            //        lastInteractableObject = hitObject;
            //        TooltipManager.instance.ShowTooltipIndefinitely(
            //            sink.actionText,
            //            sink.locationText,
            //            sink.objectName
            //        );
            //    }

            //    if (interactPressed)
            //    {
            //        sink.Interact();
            //    }
            //    Profiler.EndSample();
            //    interactPressed = false;
            //    return; // Exit to avoid further checks
            //}

            //var MuscleTraining = hit.collider.GetComponent<InteractableExerciseMachine>();
            //if (MuscleTraining != null)
            //{
            //    if (lastInteractableObject != hitObject)
            //    {
            //        lastInteractableObject = hitObject;
            //        TooltipManager.instance.ShowTooltipIndefinitely(
            //            MuscleTraining.actionText,
            //            MuscleTraining.locationText,
            //            MuscleTraining.objectName
            //        );
            //    }

            //    if (interactPressed)
            //    {
            //        MuscleTraining.Interact();
            //    }
            //    Profiler.EndSample();
            //    interactPressed = false;
            //    return; // Exit to avoid further checks
            //}


            // Check for SlidingDoorController
            var slidingDoor = hit.collider.GetComponentInParent<SlidingDoorController>();
            if (slidingDoor != null)
            {
                GameObject slidingDoorObject = slidingDoor.gameObject;
                if (lastInteractableObject != slidingDoorObject)
                {
                    lastInteractableObject = slidingDoorObject;
                    string actionText = slidingDoor.canInteract ? slidingDoor.interactTooltip : slidingDoor.autoOpenTooltip;
                    TooltipManager.instance.ShowTooltipIndefinitely(
                        actionText,
                        slidingDoor.locationText,
                        slidingDoor.objectName
                    );
                }

                if (slidingDoor.canInteract && interactPressed)
                {
                    slidingDoor.ToggleDoors();
                }
                interactPressed = false;
                // Auto open doors if player looks at them
                if (slidingDoor.autoOpenOnLook && !slidingDoor.AreDoorsOpen && !slidingDoor.IsMoving)
                {
                    slidingDoor.OpenDoors();
                }

                return;  // Exit after detecting interaction
            }

            // Check for NPCController
            var npcController = hit.collider.GetComponent<NPCController>();
            if (npcController != null)
            {
                if (lastInteractableObject != hitObject)
                {
                    lastInteractableObject = hitObject;
                    NPCDialogueComponent npcDialogue = npcController.GetComponent<NPCDialogueComponent>();
                    if (npcDialogue != null)
                    {
                        TooltipManager.instance.ShowTooltipIndefinitely(
                            npcDialogue.actionText,
                            npcDialogue.locationText,
                            npcDialogue.objectName
                        );
                    }
                    else
                    {
                        TooltipManager.instance.ShowTooltipIndefinitely(
                            "Press E to interact",
                            npcController.npcName,
                            npcController.locationText != null ? npcController.locationText.text : npcController.npcName
                        );
                    }
                }

                if (interactPressed)
                {
                    if (RadialMenuUI.instance != null && !RadialMenuUI.instance.IsOpen)
                    {
                        RadialMenuUI.instance.OpenMenu(npcController);
                    }
                }
                interactPressed = false;
                Profiler.EndSample();
                return;  // Exit to avoid further checks
            }

            // Generic IInteractable handling
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)

            //// If raycast hits something else, hide the tooltip if it was previously showing
            //if (lastInteractableObject != null)
            //{
            //    lastInteractableObject = null;
            //    TooltipManager.instance.HideTooltip();
            //}



            //var shower = hit.collider.GetComponent<InteractableShower>();
            //if (shower != null)
            {
                if (lastInteractableObject != hitObject)
                {
                    lastInteractableObject = hitObject;
                    string actionText = GetStringField(interactable, "actionText", "Press E to interact");
                    string locationText = GetStringField(interactable, "locationText", "");
                    string objectName = GetStringField(interactable, "objectName", hitObject.name);
                    TooltipManager.instance.ShowTooltipIndefinitely(actionText, locationText, objectName);
                    //TooltipManager.instance.ShowTooltipIndefinitely(
                    //    shower.actionText,
                    //    shower.locationText,
                    //    shower.objectName
                    //);
                }

                if (interactPressed)
                {
                    interactable.Interact();
                    //shower.Interact();
                }
                interactPressed = false;
                Profiler.EndSample();
                return;
            }

            //var bed = hit.collider.GetComponent<InteractableBed>();
            //if (bed != null)
            //{

                //if (lastInteractableObject != hitObject)
                //{
                //    lastInteractableObject = hitObject;
                //    TooltipManager.instance.ShowTooltipIndefinitely(
                //        bed.actionText,
                //        bed.locationText,
                //        bed.objectName
                //    );
                //}

                //if (interactPressed)
                //{
                //    bed.Interact();
                //}
                //interactPressed = false;
                //Profiler.EndSample();
                //return;

                // If raycast hits something else, hide the tooltip if it was previously showing
                if (lastInteractableObject != null)
                {
                    lastInteractableObject = null;
                    TooltipManager.instance.HideTooltip();
                }


            //var sofa = hit.collider.GetComponent<InteractableRestPlace>();
            //if (sofa != null)
            //{
            //    if (lastInteractableObject != hitObject)
            //    {
            //        lastInteractableObject = hitObject;
            //        TooltipManager.instance.ShowTooltipIndefinitely(
            //            sofa.actionText,
            //            sofa.locationText,
            //            sofa.objectName
            //        );
            //    }

            //    if (interactPressed)
            //    {
            //        sofa.Interact();
            //    }
            //    interactPressed = false;
            //    Profiler.EndSample();
            //    return;
            //}

            }
        else
        {
            // No valid hit found; hide the tooltip if it was previously showing
            if (lastInteractableObject != null)
            {
                lastInteractableObject = null;
                TooltipManager.instance.HideTooltip();
            }
            Profiler.EndSample();
        }
    }

    // Helper method to check if any parent object is in the ignore list
    private bool IsInIgnoreList(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (objectsToIgnore.Contains(current.gameObject))
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }

    private string GetStringField(object obj, string fieldName, string defaultValue)
    {
        var type = obj.GetType();
        var field = type.GetField(fieldName);
        if (field != null && field.FieldType == typeof(string))
        {
            return field.GetValue(obj) as string ?? defaultValue;
        }
        var prop = type.GetProperty(fieldName);
        if (prop != null && prop.PropertyType == typeof(string))
        {
            return prop.GetValue(obj) as string ?? defaultValue;
        }
        return defaultValue;
    }

    // Existing methods...
    public void SetInteractionDistance(float newDistance)
    {
        interactionDistance = newDistance;
    }


   

    private void Awake()
    {
        

        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        inputActions = new PlayerInputActions();
        inputActions.Player.Interact.performed += OnInteract;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        interactPressed = context.performed;
    }

    public void ForceInteract()
    {
        interactPressed = true;
    }



}
