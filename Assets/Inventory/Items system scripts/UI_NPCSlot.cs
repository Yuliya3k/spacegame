using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_NPCSlot : UI_ItemSlot, IPointerDownHandler
{
    private bool isNPCSlot;

    public override void SetupSlot(InventoryItem _item, bool _isNPCSlot)
    {
        base.SetupSlot(_item, _isNPCSlot);
        isNPCSlot = _isNPCSlot;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isNPCSlot)
            {
                // Transfer from NPC to player inventory
                NPCInteractionUIManager.instance.ShowTransferOptions(item, false);
            }
            else
            {
                // Transfer from player inventory to NPC
                NPCInteractionUIManager.instance.ShowTransferOptions(item, true);
            }
        }
    }
}
