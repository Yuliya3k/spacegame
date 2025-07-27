using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class StorageUIManager : MonoBehaviour
{
    public static StorageUIManager instance;

    [Header("UI Elements")]
    public GameObject storageUI;  // The main storage UI panel
    public Transform containerSlotParent;  // Parent object for container slots
    public Transform playerInventorySlotParent;  // Parent object for player inventory slots
    public Button closeButton; // Close button to close the storage UI

    [Header("Transfer Dialogue UI")]
    public GameObject transferDialogueWindow;     // The dialogue window GameObject
    public Image containerItemImage;              // Image for the container item
    public TextMeshProUGUI containerItemQuantityText; // Quantity text for the container item
    public Image inventoryItemImage;              // Image for the inventory item
    public TextMeshProUGUI inventoryItemQuantityText; // Quantity text for the inventory item
    public Slider quantitySlider;                 // Slider for adjusting quantity
    public TextMeshProUGUI quantitySliderText;    // Text displaying current slider value
    public Button confirmTransferButton;          // Button to confirm the transfer
    public Button cancelTransferButton;           // Button to cancel the transfer

    [Header("Game Screen UI Panels")]
    public List<GameObject> gameScreenUIs;  // Register UIs to be hidden when opening the storage UI

    private StorageContainer currentContainer;  // Reference to the current storage being accessed
    private List<GameObject> containerSlots = new List<GameObject>();  // UI slots for storage items
    private List<GameObject> inventorySlots = new List<GameObject>();  // UI slots for player inventory items
    private InventoryItem selectedItem;  // Reference to the selected item
    private bool isTransferringToStorage;

    [Header("Slot Prefabs")]
    public GameObject storageInventorySlotPrefab;  // For equipment
    public GameObject storageDishSlotPrefab;
    public GameObject storageIngredientSlotPrefab;
    public GameObject storageResourceSlotPrefab;

    private int initialInventoryQuantity;
    private int initialContainerQuantity;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Ensure the transfer dialogue window is hidden at the start
        if (transferDialogueWindow != null)
        {
            transferDialogueWindow.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseStorageUI);
        }
        else
        {
            Debug.LogError("Close Button is not assigned in the inspector.");
        }
    }

    private void Update()
    {
        // Check if the storage UI is active and the Escape key is pressed
        if (storageUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseStorageUI();
        }
    }

    //public void SetupStorageUI(StorageContainer container)
    //{
    //    transferDialogueWindow.SetActive(false);
    //    Debug.Log("Setting up the storage UI");

    //    currentContainer = container;
    //    storageUI.SetActive(true);  // Activate the UI

    //    HideGameScreenUIs();

    //    // Unlock the cursor and freeze player movement
    //    Cursor.visible = true;
    //    Cursor.lockState = CursorLockMode.None;

    //    DisablePlayerControl();  // Disable player control

    //    ClearUI();

    //    // Create container slots
    //    foreach (InventoryItem item in container.containerItems)
    //    {
    //        if (item != null && item.data != null)  // Ensure the item is not null
    //        {
    //            GameObject slotPrefab = GetSlotPrefabForItem(item.data);
    //            GameObject newSlot = Instantiate(slotPrefab, containerSlotParent);

    //            UI_ItemSlot itemSlot = newSlot.GetComponent<UI_ItemSlot>();
    //            if (itemSlot != null)
    //            {
    //                itemSlot.SetupSlot(item, true);  // Set true for container slot
    //            }
    //            else
    //            {
    //                Debug.LogError("Slot prefab does not have UI_ItemSlot component.");
    //            }

    //            containerSlots.Add(newSlot);
    //        }
    //        else
    //        {
    //            Debug.LogError("Invalid InventoryItem found in container.");
    //        }
    //    }

    //    // Create player inventory slots
    //    List<InventoryItem> allInventoryItems = PlayerInventory.instance.GetAllItems();
    //    foreach (InventoryItem item in allInventoryItems)
    //    {
    //        if (item != null)
    //        {
    //            GameObject slotPrefab = GetSlotPrefabForItem(item.data);
    //            GameObject newSlot = Instantiate(slotPrefab, playerInventorySlotParent);

    //            UI_ItemSlot itemSlot = newSlot.GetComponent<UI_ItemSlot>();
    //            if (itemSlot != null)
    //            {
    //                itemSlot.SetupSlot(item, false);  // Set false for inventory slot
    //            }
    //            else
    //            {
    //                Debug.LogError("Slot prefab does not have UI_ItemSlot component.");
    //            }

    //            inventorySlots.Add(newSlot);
    //        }
    //    }
    //}

    public void SetupStorageUI(StorageContainer container)
    {
        transferDialogueWindow.SetActive(false);
        Debug.Log("Setting up the storage UI");

        currentContainer = container;
        storageUI.SetActive(true);  // Activate the UI

        HideGameScreenUIs();

        // Freeze player and cursor via InputFreezeManager
        if (InputFreezeManager.instance != null)
        {
            InputFreezeManager.instance.FreezePlayerAndCursor();
        }

        // Deactivate existing slots instead of destroying them
        ClearUI();

        // **Step 1: Combine items with the same ID into a single item with cumulative quantity**
        Dictionary<string, InventoryItem> uniqueItems = new Dictionary<string, InventoryItem>();

        foreach (InventoryItem item in container.containerItems)
        {
            if (item != null && item.data != null)
            {
                string itemID = item.data.itemID;

                if (uniqueItems.ContainsKey(itemID))
                {
                    uniqueItems[itemID].stackSize += item.stackSize;
                }
                else
                {
                    // Create a new InventoryItem with the same data and stackSize
                    InventoryItem newItem = new InventoryItem(item.data)
                    {
                        stackSize = item.stackSize
                    };
                    uniqueItems.Add(itemID, newItem);
                }
            }
            else
            {
                Debug.LogError("Invalid InventoryItem found in container.");
            }
        }

        // **Step 2: Create or reuse UI slots for unique items**
        int index = 0;
        foreach (InventoryItem item in uniqueItems.Values)
        {
            UI_ItemSlot itemSlot;

            if (index < containerSlots.Count)
            {
                // Reuse existing slot
                GameObject existingSlot = containerSlots[index];
                existingSlot.SetActive(true);
                itemSlot = existingSlot.GetComponent<UI_ItemSlot>();
            }
            else
            {
                // Create new slot
                GameObject slotPrefab = GetSlotPrefabForItem(item.data);
                GameObject newSlot = Instantiate(slotPrefab, containerSlotParent);
                containerSlots.Add(newSlot);
                itemSlot = newSlot.GetComponent<UI_ItemSlot>();
            }

            if (itemSlot != null)
            {
                itemSlot.SetupSlot(item, true);  // Set true for container slot
            }
            else
            {
                Debug.LogError("Slot prefab does not have UI_ItemSlot component.");
            }

            index++;
        }

        // **Step 3: Deactivate any unused slots**
        for (int i = index; i < containerSlots.Count; i++)
        {
            containerSlots[i].SetActive(false);
        }

        // **Step 4: Repeat similar process for player inventory slots**
        SetupPlayerInventorySlots();
    }


    private void SetupPlayerInventorySlots()
    {
        // Combine player's inventory items
        Dictionary<string, InventoryItem> uniqueInventoryItems = new Dictionary<string, InventoryItem>();
        List<InventoryItem> allInventoryItems = PlayerInventory.instance.GetAllItems();

        foreach (InventoryItem item in allInventoryItems)
        {
            if (item != null && item.data != null)
            {
                string itemID = item.data.itemID;

                if (uniqueInventoryItems.ContainsKey(itemID))
                {
                    uniqueInventoryItems[itemID].stackSize += item.stackSize;
                }
                else
                {
                    InventoryItem newItem = new InventoryItem(item.data)
                    {
                        stackSize = item.stackSize
                    };
                    uniqueInventoryItems.Add(itemID, newItem);
                }
            }
        }

        int index = 0;
        foreach (InventoryItem item in uniqueInventoryItems.Values)
        {
            UI_ItemSlot itemSlot;

            if (index < inventorySlots.Count)
            {
                // Reuse existing slot
                GameObject existingSlot = inventorySlots[index];
                existingSlot.SetActive(true);
                itemSlot = existingSlot.GetComponent<UI_ItemSlot>();
            }
            else
            {
                // Create new slot
                GameObject slotPrefab = GetSlotPrefabForItem(item.data);
                GameObject newSlot = Instantiate(slotPrefab, playerInventorySlotParent);
                inventorySlots.Add(newSlot);
                itemSlot = newSlot.GetComponent<UI_ItemSlot>();
            }

            if (itemSlot != null)
            {
                itemSlot.SetupSlot(item, false);  // Set false for inventory slot
            }
            else
            {
                Debug.LogError("Slot prefab does not have UI_ItemSlot component.");
            }

            index++;
        }

        // Deactivate any unused slots
        for (int i = index; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].SetActive(false);
        }
    }




    public void CloseStorageUI()
    {
        transferDialogueWindow.SetActive(false);
        storageUI.SetActive(false);  // Deactivate the storage UI panel
        ClearUI();

        ShowGameScreenUIs();

        // Unfreeze player and cursor via InputFreezeManager
        if (InputFreezeManager.instance != null)
        {
            InputFreezeManager.instance.UnfreezePlayerAndCursor();
        }

        // Hide any active tooltips
        if (InventoryTooltipManager.instance != null)
        {
            InventoryTooltipManager.instance.HideTooltip();
        }
        else
        {
            Debug.LogWarning("InventoryTooltipManager.instance is null. Ensure it's initialized correctly.");
        }
        ClearUI();
    }

    //public void ClearUI()
    //{
    //    foreach (GameObject slot in containerSlots)
    //    {
    //        Destroy(slot);
    //    }
    //    containerSlots.Clear();

    //    foreach (GameObject slot in inventorySlots)
    //    {
    //        Destroy(slot);
    //    }
    //    inventorySlots.Clear();
    //}

    public void ClearUI()
    {
        foreach (GameObject slot in containerSlots)
        {
            slot.SetActive(false);  // Instead of destroying, deactivate the slot
        }
        // Do not clear the list; reuse the slots
    }

    private void DisablePlayerControl()
    {
        var playerController = FindObjectOfType<PlayerControllerCode>();  // Find your player controller script
        if (playerController != null)
        {
            playerController.DisablePlayerControl();  // Call your custom method
        }
    }

    private void EnablePlayerControl()
    {
        var playerController = FindObjectOfType<PlayerControllerCode>();  // Find your player controller script
        if (playerController != null)
        {
            playerController.EnablePlayerControl();  // Call your custom method
        }
    }

    public void ShowTransferOptions(InventoryItem item, bool transferringToStorage)
    {
        selectedItem = item;
        isTransferringToStorage = transferringToStorage;

        // Open the transfer dialogue window
        transferDialogueWindow.SetActive(true);

        // Determine the max quantity and set up images and quantities
        int maxQuantity;

        if (isTransferringToStorage)
        {
            // Transferring from inventory to storage
            InventoryItem inventoryItem = PlayerInventory.instance.GetInventoryItem(item.data);
            initialInventoryQuantity = inventoryItem != null ? inventoryItem.stackSize : 0;
            maxQuantity = initialInventoryQuantity;

            // Update inventory side
            inventoryItemImage.sprite = item.data.icon;
            inventoryItemImage.color = Color.white; // Ensure the image is visible
            inventoryItemQuantityText.text = initialInventoryQuantity.ToString();

            // Update container side
            InventoryItem containerItem = currentContainer.GetContainerItem(item.data);
            initialContainerQuantity = containerItem != null ? containerItem.stackSize : 0;
            containerItemImage.sprite = item.data.icon;
            containerItemImage.color = Color.white; // Ensure the image is visible
            containerItemQuantityText.text = initialContainerQuantity.ToString();
        }
        else
        {
            // Transferring from storage to inventory
            InventoryItem containerItem = currentContainer.GetContainerItem(item.data);
            initialContainerQuantity = containerItem != null ? containerItem.stackSize : 0;
            maxQuantity = initialContainerQuantity;

            // Update container side
            containerItemImage.sprite = item.data.icon;
            containerItemImage.color = Color.white; // Ensure the image is visible
            containerItemQuantityText.text = initialContainerQuantity.ToString();

            // Update inventory side
            InventoryItem inventoryItem = PlayerInventory.instance.GetInventoryItem(item.data);
            initialInventoryQuantity = inventoryItem != null ? inventoryItem.stackSize : 0;
            inventoryItemImage.sprite = item.data.icon;
            inventoryItemImage.color = Color.white; // Ensure the image is visible
            inventoryItemQuantityText.text = initialInventoryQuantity.ToString();
        }

        // Setup the quantity slider
        quantitySlider.maxValue = maxQuantity;
        quantitySlider.minValue = 0;  // Set minimum value to 0
        quantitySlider.value = 0;     // Set default value to 0
        quantitySlider.wholeNumbers = true;

        quantitySlider.onValueChanged.RemoveAllListeners();
        quantitySlider.onValueChanged.AddListener(delegate { OnQuantitySliderValueChanged(); });

        // Update the quantity text
        quantitySliderText.text = quantitySlider.value.ToString();

        // Set up the confirm button
        confirmTransferButton.onClick.RemoveAllListeners();
        confirmTransferButton.onClick.AddListener(delegate { ConfirmTransfer((int)quantitySlider.value); });

        // Set up the cancel button
        cancelTransferButton.onClick.RemoveAllListeners();
        cancelTransferButton.onClick.AddListener(CloseTransferDialogue);

        // Update the quantity displays immediately
        OnQuantitySliderValueChanged();
    }

    private void MoveItemToStorage(ItemData item, int quantity)
    {
        PlayerInventory.instance.RemoveItem(item, quantity);  // Remove item from inventory
        currentContainer.AddItemToStorage(item, quantity);  // Add item to storage
    }

    private void MoveItemToInventory(ItemData item, int quantity)
    {
        currentContainer.RemoveItemFromStorage(item, quantity);  // Remove item from storage
        PlayerInventory.instance.AddItem(item, quantity);  // Add item to inventory
    }

    private void HideGameScreenUIs()
    {
        foreach (var ui in gameScreenUIs)
        {
            if (ui.activeSelf) ui.SetActive(false);  // Hide any active game screen UIs
        }
    }

    private void ShowGameScreenUIs()
    {
        foreach (var ui in gameScreenUIs)
        {
            if (!ui.activeSelf) ui.SetActive(true);  // Show game screen UIs again
        }
    }

    private void OnQuantitySliderValueChanged()
    {
        int transferQuantity = (int)quantitySlider.value;
        quantitySliderText.text = transferQuantity.ToString();

        int newContainerQuantity;
        int newInventoryQuantity;

        if (isTransferringToStorage)
        {
            // Transferring from inventory to storage
            newInventoryQuantity = initialInventoryQuantity - transferQuantity;
            newContainerQuantity = initialContainerQuantity + transferQuantity;
        }
        else
        {
            // Transferring from storage to inventory
            newContainerQuantity = initialContainerQuantity - transferQuantity;
            newInventoryQuantity = initialInventoryQuantity + transferQuantity;
        }

        // Update the displayed quantities
        containerItemQuantityText.text = newContainerQuantity.ToString();
        inventoryItemQuantityText.text = newInventoryQuantity.ToString();

        // Enable or disable the confirm button based on the transfer quantity
        confirmTransferButton.interactable = (transferQuantity > 0);
    }

    private void ConfirmTransfer(int quantity)
    {
        if (quantity <= 0)
        {
            // Optionally display a message to the player
            Debug.Log("Cannot transfer zero items.");
            return;
        }

        if (isTransferringToStorage)
        {
            MoveItemToStorage(selectedItem.data, quantity);
        }
        else
        {
            MoveItemToInventory(selectedItem.data, quantity);
        }

        // Close the transfer dialogue window
        transferDialogueWindow.SetActive(false);

        // Refresh the storage UI to reflect the updated quantities
        SetupStorageUI(currentContainer);
    }

    private GameObject GetSlotPrefabForItem(ItemData itemData)
    {
        switch (itemData.itemType)
        {
            case ItemType.Eatable:
                // Check if it's an ingredient or dish
                EatableItemData eatableItem = itemData as EatableItemData;
                if (eatableItem != null)
                {
                    if (eatableItem.eatableType == EatableType.Ingredient)
                        return storageIngredientSlotPrefab;
                    else if (eatableItem.eatableType == EatableType.Dish)
                        return storageDishSlotPrefab;
                }
                break;
            case ItemType.Resources:
                return storageResourceSlotPrefab;
            case ItemType.Equipment:
                return storageInventorySlotPrefab; // For equipment
        }
        return storageInventorySlotPrefab; // Default prefab
    }

    public void CloseTransferDialogue()
    {
        // Reset the displayed quantities to initial values
        containerItemQuantityText.text = initialContainerQuantity.ToString();
        inventoryItemQuantityText.text = initialInventoryQuantity.ToString();

        transferDialogueWindow.SetActive(false);
    }


    public void RefreshStorageUI(StorageContainer container)
    {
        // Clear existing UI elements
        foreach (Transform child in containerSlotParent)
        {
            Destroy(child.gameObject);
        }

        // Populate UI with container items
        foreach (InventoryItem item in container.containerItems)
        {
            // Instantiate UI slots and assign item data
            GameObject slot = Instantiate(storageUI, containerSlotParent);
            UI_ItemSlot itemSlot = slot.GetComponent<UI_ItemSlot>();
            if (itemSlot != null)
            {
                itemSlot.UpdateSlot(item);
            }
            else
            {
                Debug.LogError("UI_ItemSlot component missing on storageUIPrefab.");
            }
        }
    }


    //private void SetupPlayerInventorySlots()
    //{
    //    // Combine player's inventory items
    //    Dictionary<string, InventoryItem> uniqueInventoryItems = new Dictionary<string, InventoryItem>();
    //    List<InventoryItem> allInventoryItems = PlayerInventory.instance.GetAllItems();

    //    foreach (InventoryItem item in allInventoryItems)
    //    {
    //        if (item != null && item.data != null)
    //        {
    //            string itemID = item.data.itemID;

    //            if (uniqueInventoryItems.ContainsKey(itemID))
    //            {
    //                uniqueInventoryItems[itemID].stackSize += item.stackSize;
    //            }
    //            else
    //            {
    //                InventoryItem newItem = new InventoryItem(item.data)
    //                {
    //                    stackSize = item.stackSize
    //                };
    //                uniqueInventoryItems.Add(itemID, newItem);
    //            }
    //        }
    //    }

    //    // Create inventory slots for unique items
    //    foreach (InventoryItem item in uniqueInventoryItems.Values)
    //    {
    //        GameObject slotPrefab = GetSlotPrefabForItem(item.data);
    //        GameObject newSlot = Instantiate(slotPrefab, playerInventorySlotParent);

    //        UI_ItemSlot itemSlot = newSlot.GetComponent<UI_ItemSlot>();
    //        if (itemSlot != null)
    //        {
    //            itemSlot.SetupSlot(item, false);  // Set false for inventory slot
    //        }
    //        else
    //        {
    //            Debug.LogError("Slot prefab does not have UI_ItemSlot component.");
    //        }

    //        inventorySlots.Add(newSlot);
    //    }
    //}



}
