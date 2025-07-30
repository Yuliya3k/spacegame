using UnityEngine;
using System.Collections.Generic;

public class StorageContainer : MonoBehaviour, IInteractable
{
    [Header("Container Settings")]
    public List<InventoryItem> containerItems; // Items inside the container
    public GameObject storageUI; // UI for displaying container items and player's inventory

    private bool isUIActive = false; // To prevent multiple UI activations

    public string objectName = "Storage";
    public string locationText = "Warehouse";
    public string containerID; // Assign a unique ID in the Editor

    private void Awake()
    {
        if (string.IsNullOrEmpty(containerID))
        {
            containerID = System.Guid.NewGuid().ToString();
        }
    }

    private void Start()
    {
        // Ensure the storage UI is hidden when the game starts
        if (storageUI != null)
            storageUI.SetActive(false);
    }

    // Method called when the player interacts with the container
    public void Interact()
    {
        if (!isUIActive)
        {
            Debug.Log("Interacting with storage: UI should open.");
            OpenStorageUI();  // This should open the UI
        }
        else
        {
            Debug.Log("Closing storage: UI should close.");
            CloseStorageUI();  // This should close the UI
        }
    }

    // Open the UI to allow the player to transfer items
    private void OpenStorageUI()
    {
        // Ensure StorageUIManager sets up the UI correctly before changing UI state
        if (storageUI != null)
        {
            // Setup the UI before setting it active to prevent delays
            StorageUIManager.instance.SetupStorageUI(this);
            storageUI.SetActive(true);  // Ensure UI is active after setup
                                        // Disable opening CharacterUI
            UIManager.instance.DisableCharacterUIOpening();
            // NEW: also disable in-game menu opening
            InGameMenuController.instance.DisableMenuOpening();
        }
        else
        {
            Debug.LogError("Storage UI is not assigned in the inspector.");
            return;
        }

        // After ensuring the UI is active, update the state
        isUIActive = true;
        Debug.Log("Opened storage UI.");
    }

    private void CloseStorageUI()
    {
        isUIActive = false;
        StorageUIManager.instance.CloseStorageUI(); // Close the storage UI
        Debug.Log("Closed storage UI.");
        // Re-enable opening CharacterUI
        UIManager.instance.EnableCharacterUIOpening();
        // NEW: also re-enable in-game menu
        InGameMenuController.instance.EnableMenuOpening();
    }

    public void OnUIClose()
    {
        isUIActive = false;
    }

    //// Method to add an item to the storage container with a specific quantity
    //public void AddItemToStorage(ItemData itemData, int quantity)
    //{
    //    InventoryItem existingItem = containerItems.Find(i => i.data == itemData);

    //    if (existingItem != null)
    //    {
    //        // If the item exists, increment its quantity
    //        existingItem.AddStack(quantity);
    //    }
    //    else
    //    {
    //        // Add a new item to the container
    //        InventoryItem newItem = new InventoryItem(itemData);

    //        // Check if the new item's data is valid before adding it to the container
    //        if (newItem.data != null)
    //        {
    //            newItem.AddStack(quantity - 1); // Initialize with the correct quantity
    //            containerItems.Add(newItem);  // Add the item to the container list
    //            Debug.Log($"Added {quantity} of {itemData.objectName} to the container.");
    //        }
    //        else
    //        {
    //            Debug.LogError("Failed to add item to container: ItemData is null.");
    //        }
    //    }
    //}

    //// Method to remove an item from the storage container with a specific quantity
    //public void RemoveItemFromStorage(ItemData itemData, int quantity)
    //{
    //    InventoryItem existingItem = containerItems.Find(i => i.data == itemData);

    //    if (existingItem != null)
    //    {
    //        if (existingItem.stackSize <= quantity)
    //        {
    //            containerItems.Remove(existingItem); // Remove the item if stack is empty or smaller than the quantity
    //            Debug.Log($"Removed {itemData.objectName} completely from the container.");
    //        }
    //        else
    //        {
    //            existingItem.RemoveStack(quantity); // Decrease the stack size by the specified quantity
    //            Debug.Log($"Removed {quantity} of {itemData.objectName} from the container.");
    //        }
    //    }
    //}

    public void AddItemToStorage(ItemData itemData, int quantity)
    {
        // Check if the item already exists in the container
        InventoryItem existingItem = containerItems.Find(i => i.data.itemID == itemData.itemID);

        if (existingItem != null)
        {
            existingItem.stackSize += quantity;
        }
        else
        {
            InventoryItem newItem = new InventoryItem(itemData)
            {
                stackSize = quantity
            };
            containerItems.Add(newItem);
        }
    }

    public void RemoveItemFromStorage(ItemData itemData, int quantity)
    {
        InventoryItem existingItem = containerItems.Find(i => i.data.itemID == itemData.itemID);

        if (existingItem != null)
        {
            existingItem.stackSize -= quantity;
            if (existingItem.stackSize <= 0)
            {
                containerItems.Remove(existingItem);
            }
        }
        else
        {
            Debug.LogWarning($"Item {itemData.objectName} not found in storage.");
        }
    }



    public InventoryItem GetContainerItem(ItemData itemData)
    {
        return containerItems.Find(i => i.data == itemData);
    }

    public void UpdateStorageUI()
    {
        if (StorageUIManager.instance != null)
        {
            StorageUIManager.instance.RefreshStorageUI(this);
            Debug.Log("Storage UI Updated.");
        }
        else
        {
            Debug.LogError("StorageUIManager instance is not assigned.");
        }
    }


}
