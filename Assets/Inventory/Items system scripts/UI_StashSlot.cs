using UnityEngine.EventSystems;
using UnityEngine;

public class UI_StashSlot : UI_ItemSlot
{
    // No additional tooltip handling needed

    protected override void Start()
    {
        base.Start();
        // Additional initialization if necessary
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null) return;
        // Logic for interacting with stash items (e.g., moving to inventory)
        // Example:
        // Inventory.instance.AddItem(item.data);
        // currentContainer.RemoveItem(item.data);
        // CleanUpSlot();
    }
}
