using UnityEngine;
using UnityEngine.EventSystems;

public class TouchActionButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (PlayerInteraction.instance != null)
        {
            PlayerInteraction.instance.ForceInteract();
        }
    }
}
