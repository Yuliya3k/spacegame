using UnityEngine.EventSystems;
using UnityEngine;

public class UI_EquipmentSlot : UI_ItemSlot
{
    public EquipmentType slotType;

    private void OnValidate()
    {
        gameObject.name = "Equipment slot - " + slotType.ToString();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null)
            return;

        // Unequip item using the player's inventory
        PlayerInventory.instance.UnequipItem(item.data as ItemDataEquipment);
        CleanUpSlot();
    }

    public void CleanUpSlot()
    {
        base.CleanUpSlot(); // Clear the slot and remove icon and text
    }
}
