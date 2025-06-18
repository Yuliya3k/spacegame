using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StorageSlot : UI_ItemSlot, IPointerDownHandler
{
    private bool isContainerSlot;

    public override void SetupSlot(InventoryItem _item, bool _isContainerSlot)
    {
        base.SetupSlot(_item, _isContainerSlot);
        isContainerSlot = _isContainerSlot;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isContainerSlot)
            {
                // Transfer from container to inventory
                StorageUIManager.instance.ShowTransferOptions(item, false);
            }
            else
            {
                // Transfer from inventory to container
                StorageUIManager.instance.ShowTransferOptions(item, true);
            }
        }
    }
}
