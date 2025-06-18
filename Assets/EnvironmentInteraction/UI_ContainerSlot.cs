//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;

//public class UI_ContainerSlot : MonoBehaviour, IPointerClickHandler
//{
//    [Header("UI Elements")]
//    [SerializeField] private Image itemIcon;             // The icon image of the item
//    [SerializeField] private TextMeshProUGUI itemText;   // The text displaying the item name or stack size

//    private ItemData itemData;                           // The item assigned to this slot
//    private bool isContainerSlot;                        // Is this slot part of the container or the player's inventory?

//    /// <summary>
//    /// Sets up the container slot with the item and its information.
//    /// </summary>
//    /// <param name="_itemData">The item data to display in this slot.</param>
//    /// <param name="_isContainerSlot">Indicates if this slot belongs to the container or the player's inventory.</param>
//    public void SetupSlot(ItemData _itemData, bool _isContainerSlot)
//    {
//        itemData = _itemData;
//        isContainerSlot = _isContainerSlot;

//        // Set icon and text
//        itemIcon.sprite = itemData.icon;
//        itemText.text = itemData.stackSize > 1 ? itemData.stackSize.ToString() : ""; // Display stack size only if greater than 1
//    }

//    /// <summary>
//    /// Called when the player clicks on the slot.
//    /// </summary>
//    /// <param name="eventData">Pointer event data from the UI system.</param>
//    public void OnPointerClick(PointerEventData eventData)
//    {
//        if (itemData != null)
//        {
//            if (isContainerSlot)
//            {
//                // Move item from container to player's inventory
//                StorageUIManager.instance.MoveItemToInventory(itemData);
//            }
//            else
//            {
//                // Move item from player's inventory to container
//                StorageUIManager.instance.MoveItemToContainer(itemData);
//            }
//        }
//    }
//}
