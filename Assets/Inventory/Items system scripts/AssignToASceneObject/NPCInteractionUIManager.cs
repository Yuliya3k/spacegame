using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class NPCInteractionUIManager : MonoBehaviour
{
    public static NPCInteractionUIManager instance;

    [Header("UI Elements")]
    public GameObject npcInteractionUI;  // The main NPC interaction UI panel
    public Transform npcInventorySlotParent;  // Parent object for NPC inventory slots
    public Transform playerInventorySlotParent;  // Parent object for player inventory slots
    public Button closeButton; // Close button to close the UI

    [Header("Transfer Dialogue UI")]
    // Define your transfer dialogue UI elements here
    public GameObject transferDialogueWindow;
    public Image npcItemImage;
    public TextMeshProUGUI npcItemQuantityText;
    public Image playerItemImage;
    public TextMeshProUGUI playerItemQuantityText;
    public Slider quantitySlider;
    public TextMeshProUGUI quantitySliderText;
    public Button confirmTransferButton;
    public Button cancelTransferButton;

    [Header("Game Screen UI Panels")]
    public List<GameObject> gameScreenUIs;  // Register UIs to be hidden when opening the NPC UI

    private NPCInventory currentNPCInventory;  // Reference to the current NPC's inventory
    private NPCController currentNPCController;  // Reference to the active NPC
    private List<GameObject> npcSlots = new List<GameObject>();  // UI slots for NPC items
    private List<GameObject> inventorySlots = new List<GameObject>();  // UI slots for player inventory items
    private InventoryItem selectedItem;  // Reference to the selected item
    private bool isTransferringToNPC;

    [Header("Slot Prefabs")]
    public GameObject inventorySlotPrefab;
    public GameObject dishSlotPrefab;
    public GameObject ingredientSlotPrefab;
    public GameObject resourceSlotPrefab;

    private int initialPlayerQuantity;
    private int initialNPCQuantity;

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

        // Ensure the NPC interaction UI is hidden when the game starts
        if (npcInteractionUI != null)
        {
            npcInteractionUI.SetActive(false);

        }

        // Initialize UI elements
        if (transferDialogueWindow != null)
        {
            transferDialogueWindow.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseNPCInteractionUI);
        }
        else
        {
            Debug.LogError("Close Button is not assigned in the inspector.");
        }
    }

    private void OnDisable()
    {
        if (instance == this) instance = null;
    }

    public void OpenNPCInteraction(NPCController npcController)
    {
        currentNPCController = npcController;
        // Get the NPC's inventory
        currentNPCInventory = npcController.GetComponent<NPCInventory>();
        if (currentNPCInventory == null)
        {
            Debug.LogError("NPC does not have an NPCInventory component.");
            return;
        }

        npcInteractionUI.SetActive(true);  // Activate the UI

        HideGameScreenUIs();

        if (InputFreezeManager.instance != null)
        {
            InputFreezeManager.instance.FreezePlayerAndCursor();
        }

        if (currentNPCController != null)
        {
            currentNPCController.Freeze();
            currentNPCController.SetInteractionAnimation("Trade");
        }

        ClearUI();

        // Create NPC inventory slots
        foreach (InventoryItem item in currentNPCInventory.GetAllItems())
        {
            if (item != null && item.data != null)  // Ensure the item is not null
            {
                GameObject slotPrefab = GetSlotPrefabForItem(item.data);
                GameObject newSlot = Instantiate(slotPrefab, npcInventorySlotParent);
                // Try to setup using UI_NPCSlot first
                UI_NPCSlot itemSlot = newSlot.GetComponent<UI_NPCSlot>();
                if (itemSlot != null)
                {
                    itemSlot.SetupSlot(item, true);  // Set true for NPC slot
                }
                else
                {
                    // Fallback to generic UI_ItemSlot if specific component is missing
                    UI_ItemSlot genericSlot = newSlot.GetComponent<UI_ItemSlot>();
                    if (genericSlot != null)
                    {
                        genericSlot.SetupSlot(item, true);
                    }
                    else
                    {
                        Debug.LogError("Slot prefab lacks UI_NPCSlot/UI_ItemSlot component.");
                    }
                }

                npcSlots.Add(newSlot);
            }
            else
            {
                Debug.LogError("Invalid InventoryItem found in NPC inventory.");
            }
        }

        // Create player inventory slots
        List<InventoryItem> allInventoryItems = PlayerInventory.instance.GetAllItems();
        foreach (InventoryItem item in allInventoryItems)
        {
            if (item != null)
            {
                GameObject slotPrefab = GetSlotPrefabForItem(item.data);
                GameObject newSlot = Instantiate(slotPrefab, playerInventorySlotParent);
                // Try to setup using UI_NPCSlot first
                UI_NPCSlot itemSlot = newSlot.GetComponent<UI_NPCSlot>();
                if (itemSlot != null)
                {
                    itemSlot.SetupSlot(item, false);  // Set false for player inventory slot
                }
                else
                {
                    // Fallback to generic UI_ItemSlot if specific component is missing
                    UI_ItemSlot genericSlot = newSlot.GetComponent<UI_ItemSlot>();
                    if (genericSlot != null)
                    {
                        genericSlot.SetupSlot(item, false);
                    }
                    else
                    {
                        Debug.LogError("Slot prefab lacks UI_NPCSlot/UI_ItemSlot component.");
                    }
                }

                inventorySlots.Add(newSlot);
            }
        }
    }

    public void CloseNPCInteractionUI()
    {
        npcInteractionUI.SetActive(false);  // Deactivate the UI
        ClearUI();

        ShowGameScreenUIs();

        if (InputFreezeManager.instance != null)
        {
            InputFreezeManager.instance.UnfreezePlayerAndCursor();
        }

        if (currentNPCInventory != null)
        {
            var npcCtrl = currentNPCInventory.GetComponent<NPCController>();
            npcCtrl?.Unfreeze();
        }

        if (currentNPCController != null)
        {
            currentNPCController.Unfreeze();
            currentNPCController = null;
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
    }

    private void ClearUI()
    {
        foreach (GameObject slot in npcSlots)
        {
            Destroy(slot);
        }
        npcSlots.Clear();

        foreach (GameObject slot in inventorySlots)
        {
            Destroy(slot);
        }
        inventorySlots.Clear();
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

    private GameObject GetSlotPrefabForItem(ItemData itemData)
    {
        // Similar to StorageUIManager
        // Return appropriate slot prefab based on item type
        switch (itemData.itemType)
        {
            case ItemType.Eatable:
                // Check if it's an ingredient or dish
                EatableItemData eatableItem = itemData as EatableItemData;
                if (eatableItem != null)
                {
                    if (eatableItem.eatableType == EatableType.Ingredient)
                        return ingredientSlotPrefab;
                    else if (eatableItem.eatableType == EatableType.Dish)
                        return dishSlotPrefab;
                }
                break;
            case ItemType.Resources:
                return resourceSlotPrefab;
            case ItemType.Equipment:
                return inventorySlotPrefab; // For equipment
        }
        return inventorySlotPrefab; // Default prefab
    }

    // Methods for transferring items between player and NPC

    public void ShowTransferOptions(InventoryItem item, bool transferringToNPC)
    {
        selectedItem = item;
        isTransferringToNPC = transferringToNPC;

        // Open the transfer dialogue window
        transferDialogueWindow.SetActive(true);

        // Determine the max quantity and set up images and quantities
        int maxQuantity;

        if (isTransferringToNPC)
        {
            // Transferring from player to NPC
            InventoryItem playerItem = PlayerInventory.instance.GetInventoryItem(item.data);
            initialPlayerQuantity = playerItem != null ? playerItem.stackSize : 0;
            maxQuantity = initialPlayerQuantity;

            // Update player side
            playerItemImage.sprite = item.data.icon;
            playerItemImage.color = Color.white; // Ensure the image is visible
            playerItemQuantityText.text = initialPlayerQuantity.ToString();

            // Update NPC side
            InventoryItem npcItem = currentNPCInventory.GetInventoryItem(item.data);
            initialNPCQuantity = npcItem != null ? npcItem.stackSize : 0;
            npcItemImage.sprite = item.data.icon;
            npcItemImage.color = Color.white; // Ensure the image is visible
            npcItemQuantityText.text = initialNPCQuantity.ToString();
        }
        else
        {
            // Transferring from NPC to player
            InventoryItem npcItem = currentNPCInventory.GetInventoryItem(item.data);
            initialNPCQuantity = npcItem != null ? npcItem.stackSize : 0;
            maxQuantity = initialNPCQuantity;

            // Update NPC side
            npcItemImage.sprite = item.data.icon;
            npcItemImage.color = Color.white; // Ensure the image is visible
            npcItemQuantityText.text = initialNPCQuantity.ToString();

            // Update player side
            InventoryItem playerItem = PlayerInventory.instance.GetInventoryItem(item.data);
            initialPlayerQuantity = playerItem != null ? playerItem.stackSize : 0;
            playerItemImage.sprite = item.data.icon;
            playerItemImage.color = Color.white; // Ensure the image is visible
            playerItemQuantityText.text = initialPlayerQuantity.ToString();
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

    private void OnQuantitySliderValueChanged()
    {
        int transferQuantity = (int)quantitySlider.value;
        quantitySliderText.text = transferQuantity.ToString();

        int newNPCQuantity;
        int newPlayerQuantity;

        if (isTransferringToNPC)
        {
            // Transferring from player to NPC
            newPlayerQuantity = initialPlayerQuantity - transferQuantity;
            newNPCQuantity = initialNPCQuantity + transferQuantity;
        }
        else
        {
            // Transferring from NPC to player
            newNPCQuantity = initialNPCQuantity - transferQuantity;
            newPlayerQuantity = initialPlayerQuantity + transferQuantity;
        }

        // Update the displayed quantities
        npcItemQuantityText.text = newNPCQuantity.ToString();
        playerItemQuantityText.text = newPlayerQuantity.ToString();

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

        if (isTransferringToNPC)
        {
            MoveItemToNPC(selectedItem.data, quantity);
        }
        else
        {
            MoveItemToPlayer(selectedItem.data, quantity);
        }

        // Close the transfer dialogue window
        transferDialogueWindow.SetActive(false);

        // Refresh the NPC interaction UI to reflect the updated quantities
        OpenNPCInteraction(currentNPCInventory.GetComponent<NPCController>());
    }

    private void CloseTransferDialogue()
    {
        // Reset the displayed quantities to initial values
        npcItemQuantityText.text = initialNPCQuantity.ToString();
        playerItemQuantityText.text = initialPlayerQuantity.ToString();

        transferDialogueWindow.SetActive(false);
    }

    private void MoveItemToNPC(ItemData item, int quantity)
    {
        PlayerInventory.instance.RemoveItem(item, quantity);  // Remove item from player inventory
        currentNPCInventory.AddItem(item, quantity);  // Add item to NPC inventory
    }

    private void MoveItemToPlayer(ItemData item, int quantity)
    {
        currentNPCInventory.RemoveItem(item, quantity);  // Remove item from NPC inventory
        PlayerInventory.instance.AddItem(item, quantity);  // Add item to player inventory
    }
}
