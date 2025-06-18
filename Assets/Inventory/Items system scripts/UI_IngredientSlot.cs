using UnityEngine.EventSystems;
using UnityEngine;

public class UI_IngredientSlot : UI_ItemSlot
{
    protected override void Start()
    {
        base.Start();
        // Additional initialization if necessary
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null) return;
        PlayerInventory.instance.EatItem(item.data);  // Use the player's inventory
    }
}
