using UnityEngine.EventSystems;
using UnityEngine;

public class UI_CraftSlot : UI_ItemSlot, IPointerDownHandler
{
    protected override void Start()
    {
        base.Start();
    }

    public void SetupCraftSlot(ItemData _data)
    {
        if (_data == null) return;

        item.data = _data;

        // Assign icon and name
        itemImage.sprite = _data.icon;
        itemText.text = _data.objectName;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item.data == null) return;

        // Open the crafting window with the selected item
        UI_CraftWindow craftWindow = UIManager.instance.craftWindow;

        if (item.data is ItemDataEquipment equipmentData)
        {
            craftWindow.SetupCraftWindow(equipmentData);
        }
        else if (item.data is EatableItemData dishData)
        {
            craftWindow.SetupCraftWindow(dishData);
        }

        // Ensure the craft window is visible
        craftWindow.gameObject.SetActive(true);
    }
}
