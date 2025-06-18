//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using UnityEngine.EventSystems;  // For using TextMeshPro

//public class UI_ContainerSlot : MonoBehaviour, IPointerDownHandler
//{
//    public Image itemImage;  // This will display the item icon
//    public TextMeshProUGUI itemQuantityText;  // This will display the item quantity

//    private InventoryItem item;  // The InventoryItem being displayed
//    private bool isContainerSlot;  // Is this a storage container slot or an inventory slot?

//    // Setup the UI slot for the item
//    public void SetupSlot(InventoryItem _item, bool _isContainerSlot)
//    {
//        item = _item;
//        isContainerSlot = _isContainerSlot;

//        if (item == null || item.data == null)
//        {
//            Debug.LogError("Invalid InventoryItem or ItemData in SetupSlot.");
//            return;
//        }

//        // Set the image and quantity
//        if (itemImage != null && item.data.icon != null)
//        {
//            itemImage.sprite = item.data.icon;
//        }
//        else
//        {
//            Debug.LogError("Item Image is not assigned in the UI_ContainerSlot script or item icon is missing.");
//        }

//        if (itemQuantityText != null)
//        {
//            itemQuantityText.text = item.stackSize.ToString();
//        }
//        else
//        {
//            Debug.LogError("Item Quantity Text is not assigned in the UI_ContainerSlot script.");
//        }
//    }


//    public void OnPointerDown(PointerEventData eventData)
//    {
//        if (item == null || item.data == null) return;

//        if (Input.GetKey(KeyCode.LeftControl))
//        {
//            // For removing items quickly (optional)
//            return;
//        }

//        if (eventData.button == PointerEventData.InputButton.Left)
//        {
//            if (isContainerSlot)
//            {
//                // Transfer from container to inventory
//                StorageUIManager.instance.ShowTransferOptions(item, false);
//            }
//            else
//            {
//                // Transfer from inventory to container
//                StorageUIManager.instance.ShowTransferOptions(item, true);
//            }
//        }
//    }



//}
