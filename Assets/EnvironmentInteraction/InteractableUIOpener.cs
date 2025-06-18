using UnityEngine;
using UnityEngine.Events;

public class InteractableUIOpener : MonoBehaviour
{
    public string objectName = "Object";
    public string locationText = "Location";
    public string actionText = "Press E to interact";
    public UnityEvent onInteract; // Allows you to assign any method to be called on interaction

    private void Start()
    {
        if (onInteract == null)
            onInteract = new UnityEvent();
    }

    public void Interact()
    {
        onInteract.Invoke();
    }
}
