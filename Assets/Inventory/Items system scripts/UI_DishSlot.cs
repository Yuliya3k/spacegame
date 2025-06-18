using UnityEngine.EventSystems;
using UnityEngine;

public class UI_DishSlot : UI_ItemSlot
{
    protected override void Start()
    {
        base.Start();
        // Additional initialization if necessary
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null)
        {
            Debug.Log("UI_DishSlot: No item assigned to this slot.");
            return;
        }

        // Ensure the item is of type EatableItemData
        if (item.data is EatableItemData eatableData)
        {
            Debug.Log($"UI_DishSlot: Attempting to eat item {eatableData.objectName}.");
            PlayerInventory.instance.EatItem(item.data);  // Use the player's inventory
        }
        else
        {
            Debug.LogWarning($"UI_DishSlot: Item {item.data.objectName} is not eatable.");
        }
    }
}
