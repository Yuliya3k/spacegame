using UnityEngine.EventSystems;
using UnityEngine;

public class UI_InventorySlot : UI_ItemSlot
{
    protected override void Start()
    {
        base.Start();
        // Additional initialization if necessary
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null) return;
        PlayerInventory.instance.EquipItem(item.data);  // Equipment equip logic using player's inventory
    }
}
