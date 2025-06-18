using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PlayerInventory : Inventory
{
    public static PlayerInventory instance;

    [Header("Inventory UI")]
    [SerializeField] private Transform inventorySlotParent;
    [SerializeField] private Transform ingredientsSlotParent;
    [SerializeField] private Transform resourcesSlotParent;
    [SerializeField] private Transform equipmentSlotParent;
    [SerializeField] private Transform dishesSlotParent;

    [Header("Slot Prefabs")]
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private GameObject dishSlotPrefab;
    [SerializeField] private GameObject ingredientSlotPrefab;
    [SerializeField] private GameObject resourceSlotPrefab;

    private UI_ItemSlot[] ingredientsItemSlot;
    private UI_ItemSlot[] resourceItemSlot;
    private UI_ItemSlot[] dishesItemSlot;
    private UI_ItemSlot[] inventoryItemSlot;
    private UI_EquipmentSlot[] equipmentSlot;

    private bool isEating = false;

    
    public CharacterStats characterStats;
    public CharacterEquipmentManager equipmentManager;
    public AudioSource audioSource;

    public List<ItemDataEquipment> startingEquipment;

    public AudioSource cookingAudioSource; 

    public ActionProgressUI actionProgressUI; 

    protected override void Awake()
    {
        base.Awake();

        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Initialize UI slots and assign references
        ingredientsItemSlot = ingredientsSlotParent.GetComponentsInChildren<UI_ItemSlot>();
        resourceItemSlot = resourcesSlotParent.GetComponentsInChildren<UI_ItemSlot>();
        inventoryItemSlot = inventorySlotParent.GetComponentsInChildren<UI_ItemSlot>();
        dishesItemSlot = dishesSlotParent.GetComponentsInChildren<UI_ItemSlot>();
        equipmentSlot = equipmentSlotParent.GetComponentsInChildren<UI_EquipmentSlot>();

        //// Equip starting equipment
        //if (startingEquipment != null && startingEquipment.Count > 0)
        //{
        //    foreach (var item in startingEquipment)
        //    {
        //        EquipItem(item);
        //    }
        //}

        // You can initialize other components or variables here
    }

    // Override methods to include UI updates

    public override void AddItem(ItemData _item, int quantity = 1)
    {
        base.AddItem(_item, quantity);
        UpdateSlotUI();
    }

    public override void RemoveItem(ItemData _item, int quantity = 1)
    {
        base.RemoveItem(_item, quantity);
        UpdateSlotUI();
    }

    public void EquipItem(ItemData _item)
    {
        
        ItemDataEquipment newEquipment = _item as ItemDataEquipment;
        InventoryItem newItem = new InventoryItem(newEquipment);

        if (newEquipment != null)
        {
            // Loop through all slots the item can occupy
            foreach (EquipmentType slot in newEquipment.equipmentSlots)
            {
                // Check if an item is already in that slot
                if (equipmentDictionary.TryGetValue(slot, out InventoryItem oldEquipment))
                {
                    // Unequip the old item in that slot
                    UnequipItem(oldEquipment.data as ItemDataEquipment);
                }

                // Equip the new item in the current slot
                equipmentDictionary[slot] = newItem;
            }

            // **Add the new item to the equipment list**
            equipment.Add(newItem);

            // Attach the item mesh to the character
            equipmentManager.EquipItem(newEquipment);

            // Apply equipment modifiers
            characterStats.ApplyEquipmentModifier(newEquipment);

            // Remove the newly equipped item from the inventory
            RemoveItem(_item);

            // Update UI
            UpdateSlotUI();
        }
    }

    public void UnequipItem(ItemDataEquipment itemToRemove)
    {
        // Remove the call to base.UnequipItem(itemToRemove);
        // Implement the logic directly here
        bool itemAlreadyRemoved = false;  // Flag to ensure the item is only added back to inventory once

        // Remove the item from all the slots it occupies
        foreach (EquipmentType slot in itemToRemove.equipmentSlots)
        {
            if (equipmentDictionary.TryGetValue(slot, out InventoryItem equippedItem) && equippedItem.data == itemToRemove)
            {
                // Remove the item from the slot
                equipmentDictionary.Remove(slot);

                // Only add the item back to the inventory once
                if (!itemAlreadyRemoved)
                {
                    equipment.Remove(equippedItem);
                    AddItem(itemToRemove);  // Return the item to the inventory

                    // Remove equipment modifiers
                    characterStats.RemoveEquipmentModifier(itemToRemove);

                    itemAlreadyRemoved = true;  // Mark that the item has been removed and added to inventory
                }
            }
        }

        // Detach the item mesh from the character
        equipmentManager.UnequipItem(itemToRemove);

        // Update UI
        UpdateSlotUI();
    }

    //public void UpdateSlotUI()
    //{
    //    // Update Ingredients UI
    //    CreateOrUpdateSlots(ref ingredientsItemSlot, ingredientsSlotParent, ingredients.Count, "Ingredients");
    //    for (int i = 0; i < ingredientsItemSlot.Length; i++) ingredientsItemSlot[i].CleanUpSlot();
    //    for (int i = 0; i < ingredients.Count; i++) ingredientsItemSlot[i].UpdateSlot(ingredients[i]);

    //    // Update Resources UI
    //    CreateOrUpdateSlots(ref resourceItemSlot, resourcesSlotParent, resources.Count, "Resources");
    //    for (int i = 0; i < resourceItemSlot.Length; i++) resourceItemSlot[i].CleanUpSlot();
    //    for (int i = 0; i < resources.Count; i++) resourceItemSlot[i].UpdateSlot(resources[i]);

    //    // Update Dishes UI
    //    CreateOrUpdateSlots(ref dishesItemSlot, dishesSlotParent, dishes.Count, "Dishes");
    //    for (int i = 0; i < dishesItemSlot.Length; i++) dishesItemSlot[i].CleanUpSlot();
    //    for (int i = 0; i < dishes.Count; i++) dishesItemSlot[i].UpdateSlot(dishes[i]);

    //    // Update Inventory UI
    //    CreateOrUpdateSlots(ref inventoryItemSlot, inventorySlotParent, inventory.Count, "Inventory");
    //    for (int i = 0; i < inventoryItemSlot.Length; i++) inventoryItemSlot[i].CleanUpSlot();
    //    for (int i = 0; i < inventory.Count; i++) inventoryItemSlot[i].UpdateSlot(inventory[i]);

    //    // Update Equipment Slots
    //    foreach (var slot in equipmentSlot)
    //    {
    //        slot.CleanUpSlot();
    //        if (equipmentDictionary.TryGetValue(slot.slotType, out InventoryItem equippedItem))
    //        {
    //            slot.UpdateSlot(equippedItem);
    //        }
    //    }
    //}

    //New MEthod to reduce memory usage
    public void UpdateSlotUI()
    {
        UpdateItemSlots(ref ingredientsItemSlot, ingredientsSlotParent, ingredients, ingredientSlotPrefab);
        UpdateItemSlots(ref resourceItemSlot, resourcesSlotParent, resources, resourceSlotPrefab);
        UpdateItemSlots(ref dishesItemSlot, dishesSlotParent, dishes, dishSlotPrefab);
        UpdateItemSlots(ref inventoryItemSlot, inventorySlotParent, inventory, inventorySlotPrefab);

        // Update Equipment Slots
        foreach (var slot in equipmentSlot)
        {
            slot.CleanUpSlot();
            if (equipmentDictionary.TryGetValue(slot.slotType, out InventoryItem equippedItem))
            {
                slot.UpdateSlot(equippedItem);
            }
        }
    }

    //helper method for new UpdateSlotUI
    private void UpdateItemSlots(ref UI_ItemSlot[] existingSlots, Transform parentTransform, List<InventoryItem> items, GameObject slotPrefab)
    {
        // Destroy existing slots to prevent duplication
        if (existingSlots != null)
        {
            foreach (var slot in existingSlots)
            {
                Destroy(slot.gameObject);
            }
        }

        existingSlots = new UI_ItemSlot[items.Count];

        for (int i = 0; i < items.Count; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, parentTransform);
            UI_ItemSlot itemSlot = newSlot.GetComponent<UI_ItemSlot>();
            itemSlot.UpdateSlot(items[i]);
            existingSlots[i] = itemSlot;
        }
    }

    private void CreateOrUpdateSlots(ref UI_ItemSlot[] existingSlots, Transform parentTransform, int itemCount, string slotType)
    {
        GameObject slotPrefab = inventorySlotPrefab;

        // Select appropriate prefab based on the slot type
        switch (slotType)
        {
            case "Dishes": slotPrefab = dishSlotPrefab; break;
            case "Ingredients": slotPrefab = ingredientSlotPrefab; break;
            case "Resources": slotPrefab = resourceSlotPrefab; break;
            default: slotPrefab = inventorySlotPrefab; break;
        }

        // If existing slots are not enough, create more
        if (itemCount > existingSlots.Length)
        {
            int additionalSlots = itemCount - existingSlots.Length;
            Debug.Log($"Creating {additionalSlots} additional {slotType} slots...");

            List<UI_ItemSlot> tempSlots = new List<UI_ItemSlot>(existingSlots);
            for (int i = 0; i < additionalSlots; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, parentTransform);
                tempSlots.Add(newSlot.GetComponent<UI_ItemSlot>());
            }

            existingSlots = tempSlots.ToArray();
        }

        // Ensure all slots are cleaned up before updating them with new items
        for (int i = 0; i < existingSlots.Length; i++)
        {
            existingSlots[i].CleanUpSlot();

            // Reactivate the slot if it has been hidden before
            if (!existingSlots[i].gameObject.activeSelf && i < itemCount)
            {
                existingSlots[i].gameObject.SetActive(true);
            }
        }
    }

    // Methods for eating items, handling UI interactions, etc.

    public void EatItem(ItemData _item)
    {
        if (isEating)
        {
            NotificationManager.instance.ShowNotification(
                $"Cannot consume {_item.objectName}: wait until you eat current dish!",
                textColor: new Color(255f, 0f, 0f, 255f),
                backgroundColor: new Color(255f, 0f, 0f, 255f),
                backgroundSprite: null, // Assign a Sprite if you have one
                startPos: new Vector2(0f, 200f), // Starting position
                endPos: new Vector2(0f, 200f),      // Ending position (center)
                moveDur: 4f,                      // Move over 2 seconds
                fadeDur: 2f,                    // Fade in/out over 0.5 seconds
                displayDur: 0f                    // Display for 2 seconds
            );
            return;
        }

        if (_item is EatableItemData eatableData)
        {
            // Use the updated properties directly from eatableData
            if (characterStats.currentFullness + eatableData.volume <= characterStats.stomachCapacity)
            {
                if (eatableData.hydration < 0f && characterStats.currentHydration + eatableData.hydration < characterStats.minHydration)
                {
                    //Debug.Log("");
                    NotificationManager.instance.ShowNotification(
                        $"Cannot consume {eatableData.objectName}: Hydration is too low!",
                        textColor: new Color(255f, 0f, 0f, 255f),
                        backgroundColor: new Color(255f, 0f, 0f, 255f),
                        backgroundSprite: null, // Assign a Sprite if you have one
                        startPos: new Vector2(0f, 200f), // Starting position
                        endPos: new Vector2(0f, 200f),      // Ending position (center)
                        moveDur: 4f,                      // Move over 2 seconds
                        fadeDur: 2f,                    // Fade in/out over 0.5 seconds
                        displayDur: 0f                    // Display for 2 seconds
                    );
                    return;
                }

                // Start the eating coroutine
                StartCoroutine(EatingCoroutine(_item, eatableData));
            }
            else
            {
                //Debug.Log("Player is too full to eat!");
                NotificationManager.instance.ShowNotification(
                    $"Cannot consume {eatableData.objectName}: you are too full!",
                    textColor: new Color(255f, 0f, 0f, 255f),
                    backgroundColor: new Color(255f, 0f, 0f, 255f),
                    backgroundSprite: null, // Assign a Sprite if you have one
                    startPos: new Vector2(0f, 200f), // Starting position
                    endPos: new Vector2(0f, 200f),      // Ending position (center)
                    moveDur: 4f,                      // Move over 2 seconds
                    fadeDur: 2f,                    // Fade in/out over 0.5 seconds
                    displayDur: 0f                    // Display for 2 seconds
                );
            }
        }
        else
        {
            NotificationManager.instance.ShowNotification(
                $"Cannot consume {_item.objectName}: you can't eat this!",
                textColor: new Color(255f, 0f, 0f, 255f),
                backgroundColor: new Color(255f, 0f, 0f, 255f),
                backgroundSprite: null, // Assign a Sprite if you have one
                startPos: new Vector2(0f, 200f), // Starting position
                endPos: new Vector2(0f, 200f),      // Ending position (center)
                moveDur: 4f,                      // Move over 2 seconds
                fadeDur: 2f,                    // Fade in/out over 0.5 seconds
                displayDur: 0f                    // Display for 2 seconds
            );
        }
    }


    private IEnumerator EatingCoroutine(ItemData _item, EatableItemData eatableData)
    {
        isEating = true;

        // Disable eatable item slots and change icon color to gray
        SetEatableSlotsState(false, Color.gray);

        // Play the eating sound at the start
        if (eatableData.eatingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(eatableData.eatingSound);
        }

        // Use updated properties from eatableData
        characterStats.UpdateStatsOnEating(
            eatableData.volume,
            eatableData.calories,
            eatableData.hydration,
            eatableData.peristalticVelocityImpact,
            eatableData.healthImpact,
            eatableData.timeToEat
        );

        // Decrease stamina
        StartCoroutine(characterStats.DecreaseStaminaOverTime(eatableData.staminaSpentToEat, eatableData.timeToEat));

        // Wait for the timeToEat duration
        yield return new WaitForSeconds(eatableData.timeToEat);

        // Handle item consumption
        HandleConsumptionAfterEating(_item);

        // Re-enable eatable item slots and reset icon color to white
        SetEatableSlotsState(true, Color.white);

        isEating = false;

        UpdateSlotUI();

        Debug.Log("EatingCoroutine finished.");
        NotificationManager.instance.ShowNotification(
            $"You have consumed {eatableData.objectName}",
            textColor: new Color(255f, 255f, 255f, 255f),
            backgroundColor: new Color(255f, 255f, 255f, 255f),
            backgroundSprite: null, // Assign a Sprite if you have one
            startPos: new Vector2(0f, 200f), // Starting position
            endPos: new Vector2(0f, 200f),      // Ending position (center)
            moveDur: 4f,                      // Move over 2 seconds
            fadeDur: 2f,                    // Fade in/out over 0.5 seconds
            displayDur: 0f                    // Display for 2 seconds
        );
    }


    private void SetEatableSlotsState(bool interactable, Color iconColor)
    {
        // Ingredients slots
        foreach (var slot in ingredientsItemSlot)
        {
            if (slot != null)
            {
                slot.SetSlotState(interactable, iconColor);
            }
        }

        // Dishes slots
        foreach (var slot in dishesItemSlot)
        {
            if (slot != null)
            {
                slot.SetSlotState(interactable, iconColor);
            }
        }
    }

    private void HandleConsumptionAfterEating(ItemData _item)
    {
        bool itemConsumed = false;

        if (ingredientDictionary.TryGetValue(_item, out InventoryItem ingredientValue) && !itemConsumed)
        {
            HandleItemConsumption(ingredients, ingredientDictionary, ingredientValue, _item);
            itemConsumed = true;
        }
        else if (dishDictionary.TryGetValue(_item, out InventoryItem dishValue) && !itemConsumed)
        {
            HandleItemConsumption(dishes, dishDictionary, dishValue, _item);
            itemConsumed = true;
        }
    }

    private void HandleItemConsumption(List<InventoryItem> itemList, Dictionary<ItemData, InventoryItem> itemDictionary, InventoryItem itemToConsume, ItemData _item)
    {
        if (itemToConsume.stackSize > 1)
        {
            // Decrease stack size and update UI
            itemToConsume.RemoveStack();  // Reduce the stack size by 1
            Debug.Log($"Consumed one {_item.objectName}, remaining in stack: {itemToConsume.stackSize}");
        }
        else
        {
            // Fully remove the item from the list and dictionary if it's the last one
            itemList.Remove(itemToConsume);  // Remove the item from the list
            itemDictionary.Remove(_item);    // Remove the item from the dictionary
            Debug.Log($"Consumed the last {_item.objectName}, it has been removed.");
            // HideCorrespondingSlot(_item); // If needed
        }

        // Update the UI for all item categories
        UpdateSlotUI();
    }

    // Override crafting methods to include UI updates

    public bool CanCraft(ItemDataEquipment itemToCraft, List<InventoryItem> requiredMaterials)
    {
        // Implement the crafting logic directly here
        // Check if the player has the required materials
        if (HasRequiredMaterials(requiredMaterials))
        {
            // Remove required materials from inventory
            foreach (var material in requiredMaterials)
            {
                RemoveItem(material.data, material.stackSize);
            }

            // Add the crafted item to inventory
            AddItem(itemToCraft);

            // Update UI
            UpdateSlotUI();

            return true; // Crafting succeeded
        }
        else
        {

            Debug.Log("Not enough materials to craft " + itemToCraft.objectName);
            return false; // Crafting failed
        }
    }

    public bool CanCraftDish(EatableItemData dishToCook, List<InventoryItem> requiredIngredients)
    {
        // Implement the cooking logic directly here
        // Check if the player has the required ingredients
        if (HasRequiredMaterials(requiredIngredients))
        {
            // Remove required ingredients from inventory
            foreach (var ingredient in requiredIngredients)
            {
                RemoveItem(ingredient.data, ingredient.stackSize);
            }

            // Add the cooked dish to inventory
            AddItem(dishToCook);

            // Update UI
            UpdateSlotUI();

            return true; // Cooking succeeded
        }
        else
        {
            Debug.Log("Not enough ingredients to cook " + dishToCook.objectName);
            NotificationManager.instance.ShowNotification(
                $"Not enough ingredients: {dishToCook.objectName}",
                textColor: new Color(222f, 0f, 0f, 255f),
                backgroundColor: new Color(255f, 255f, 255f, 255f),
                backgroundSprite: null, // Assign a Sprite if you have one
                startPos: new Vector2(0f, 200f), // Starting position
                endPos: new Vector2(0f, 200f),      // Ending position (center)
                moveDur: 4f,                      // Move over 2 seconds
                fadeDur: 2f,                    // Fade in/out over 0.5 seconds
                displayDur: 0f                    // Display for 2 seconds
            );
            return false; // Cooking failed
        }
    }


    private bool HasRequiredMaterials(List<InventoryItem> requiredMaterials)
    {
        foreach (var requiredItem in requiredMaterials)
        {
            InventoryItem playerItem = GetInventoryItem(requiredItem.data);
            if (playerItem == null || playerItem.stackSize < requiredItem.stackSize)
            {
                return false;
            }
        }
        return true;
    }

    public void CraftItem(ItemDataEquipment itemToCraft)
    {
        if (HasRequiredMaterials(itemToCraft.craftingMaterials))
        {
            // Remove required materials from inventory
            foreach (var material in itemToCraft.craftingMaterials)
            {
                RemoveItem(material.data, material.stackSize);
            }

            // Start the crafting coroutine
            StartCoroutine(CraftingCoroutine(itemToCraft));
        }
        else
        {
            Debug.Log("Not enough materials to craft " + itemToCraft.objectName);
        }
    }

    private IEnumerator CraftingCoroutine(ItemDataEquipment itemToCraft)
    {
        float duration = itemToCraft.timeToCraft;

        actionProgressUI.StartAction($"Crafting {itemToCraft.objectName}", duration);

        // Start decreasing stamina over the crafting duration
        StartCoroutine(characterStats.DecreaseStaminaOverTime(itemToCraft.staminaSpentToCraft, duration));

        // Start decreasing calories over the crafting duration
        StartCoroutine(characterStats.DecreaseCaloriesOverTime(itemToCraft.caloriesSpentToCraft, duration));

        // Wait for the crafting duration
        yield return new WaitForSeconds(duration);

        // Stop the progress UI
        actionProgressUI.StopAction();

        NotificationManager.instance.ShowNotification(
            $"Successfully cooked: {itemToCraft.objectName}",
            textColor: new Color(255f, 255f, 255f, 255f),
            backgroundColor: new Color(255f, 255f, 255f, 255f),
            backgroundSprite: null, // Assign a Sprite if you have one
            startPos: new Vector2(0f, 200f), // Starting position
            endPos: new Vector2(0f, 200f),      // Ending position (center)
            moveDur: 4f,                      // Move over 2 seconds
            fadeDur: 2f,                    // Fade in/out over 0.5 seconds
            displayDur: 0f                    // Display for 2 seconds
        );

        // Add the crafted item to inventory
        AddItem(itemToCraft);

        // Update UI
        UpdateSlotUI();

        Debug.Log($"Successfully crafted: {itemToCraft.objectName}");
    }

    public void CookDish(EatableItemData dishToCook)
    {
        if (HasRequiredMaterials(dishToCook.craftingMaterials))
        {
            // Remove required ingredients from inventory
            foreach (var ingredient in dishToCook.craftingMaterials)
            {
                RemoveItem(ingredient.data, ingredient.stackSize);
            }

            // Start the cooking coroutine
            StartCoroutine(CookingCoroutine(dishToCook));
        }
        else
        {
            Debug.Log("Not enough ingredients to cook " + dishToCook.objectName);
            NotificationManager.instance.ShowNotification(
                $"Not enough ingredients: {dishToCook.objectName}",
                textColor: new Color(222f, 0f, 0f, 255f),
                backgroundColor: new Color(255f, 255f, 255f, 255f),
                backgroundSprite: null, // Assign a Sprite if you have one
                startPos: new Vector2(0f, 200f), // Starting position
                endPos: new Vector2(0f, 200f),      // Ending position (center)
                moveDur: 4f,                      // Move over 2 seconds
                fadeDur: 2f,                    // Fade in/out over 0.5 seconds
                displayDur: 0f                    // Display for 2 seconds
            );
        }
    }

    private IEnumerator CookingCoroutine(EatableItemData dishToCook)
    {
        float duration = dishToCook.timeToCraft;
        Debug.Log("Current timeToCraft: " + duration);

        // Disable all slots and craft button
        SetAllSlotsState(false, Color.gray);
        UIManager.instance.craftWindow.SetCraftButtonState(false);

        actionProgressUI.StartAction($"Cooking {dishToCook.objectName}", duration);


        // Play cooking sound
        if (dishToCook.cookingSound != null && cookingAudioSource != null)
        {
            cookingAudioSource.clip = dishToCook.cookingSound;
            cookingAudioSource.Play();
        }

        // Start decreasing stamina and calories over the cooking duration
        StartCoroutine(characterStats.DecreaseStaminaOverTime(dishToCook.staminaSpentToCraft, duration));
        StartCoroutine(characterStats.DecreaseCaloriesOverTime(dishToCook.caloriesSpentToCraft, duration));

        // Wait for the cooking duration
        yield return new WaitForSeconds(duration);


        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledTime;
            float progress = Mathf.Clamp01(elapsedTime / duration) * 100f;
            //UIManager.instance.craftWindow.UpdateProgressBar(progress);
            yield return null;
        }


        // Stop cooking sound
        if (dishToCook.cookingSound != null && cookingAudioSource != null)
        {
            cookingAudioSource.Stop();
        }


        // Stop the progress UI
        actionProgressUI.StopAction();

        // Determine Cooking Success
        float playerCookingSkill = characterStats.cooking;
        float requiredCookingSkill = dishToCook.requiredCookingSkill;
        float successChance;

        if (playerCookingSkill >= requiredCookingSkill + 10f)
        {
            successChance = 100f;
        }
        else if (playerCookingSkill >= requiredCookingSkill)
        {
            successChance = 90f;
        }
        else
        {
            successChance = 10f;
        }

        float randomValue = UnityEngine.Random.Range(0f, 100f);
        bool isSuccess = randomValue <= successChance;

        if (isSuccess)
        {
            // Success: Add dish to inventory and increase cooking skill
            AddItem(dishToCook);
            characterStats.cooking = Mathf.Min(characterStats.cooking + 0.1f, characterStats.maxCooking);
            NotificationManager.instance.ShowNotification(
                $"Successfully cooked: {dishToCook.objectName}",
                textColor: new Color(1f, 1f, 1f, 1f),
                backgroundColor: new Color(1f, 1f, 1f, 1f),
                backgroundSprite: null,
                startPos: new Vector2(0f, 200f),
                endPos: new Vector2(0f, 200f),
                moveDur: 4f,
                fadeDur: 2f,
                displayDur: 0f
            );
            Debug.Log($"Successfully cooked: {dishToCook.objectName}. Cooking skill increased to {characterStats.cooking}.");
        }
        else
        {
            // Failure: Notify the player
            NotificationManager.instance.ShowNotification(
                $"Failed to cook: {dishToCook.objectName}",
                textColor: new Color(222f / 255f, 0f / 255f, 0f / 255f, 1f),
                backgroundColor: new Color(1f, 1f, 1f, 1f),
                backgroundSprite: null,
                startPos: new Vector2(0f, 200f),
                endPos: new Vector2(0f, 200f),
                moveDur: 4f,
                fadeDur: 2f,
                displayDur: 0f
            );
            Debug.Log($"Failed to cook: {dishToCook.objectName}. Cooking skill remains at {characterStats.cooking}.");
        }

        // Re-enable all slots and craft button after cooking
        SetAllSlotsState(true, Color.white);
        UIManager.instance.craftWindow.SetCraftButtonState(true);

        // Update UI
        UpdateSlotUI();

        Debug.Log($"CookingCoroutine finished for: {dishToCook.objectName}");
    }

    //private bool CanCookDish(EatableItemData dishToCook)
    //{
    //    // Check if the player has the required ingredients
    //    return HasRequiredMaterials(dishToCook.craftingMaterials);
    //}



    private void SetAllSlotsState(bool interactable, Color iconColor)
    {
        // Inventory slots
        foreach (var slot in inventoryItemSlot)
        {
            if (slot != null)
            {
                slot.SetSlotState(interactable, iconColor);
            }
        }

        // Ingredients slots
        foreach (var slot in ingredientsItemSlot)
        {
            if (slot != null)
            {
                slot.SetSlotState(interactable, iconColor);
            }
        }

        // Resources slots
        foreach (var slot in resourceItemSlot)
        {
            if (slot != null)
            {
                slot.SetSlotState(interactable, iconColor);
            }
        }

        // Dishes slots
        foreach (var slot in dishesItemSlot)
        {
            if (slot != null)
            {
                slot.SetSlotState(interactable, iconColor);
            }
        }

        // Equipment slots
        foreach (var slot in equipmentSlot)
        {
            if (slot != null)
            {
                slot.SetSlotState(interactable, iconColor);
            }
        }
    }



    //public void RebuildDictionaries()
    //{
    //    // Clear existing dictionaries
    //    ingredientDictionary.Clear();
    //    resourceDictionary.Clear();
    //    dishDictionary.Clear();
    //    inventoryDictionary.Clear();
    //    equipmentDictionary.Clear();

    //    // Rebuild dictionaries
    //    foreach (var item in ingredients)
    //    {
    //        ingredientDictionary[item.data] = item;
    //    }
    //    foreach (var item in resources)
    //    {
    //        resourceDictionary[item.data] = item;
    //    }
    //    foreach (var item in dishes)
    //    {
    //        dishDictionary[item.data] = item;
    //    }
    //    foreach (var item in inventory)
    //    {
    //        inventoryDictionary[item.data] = item;
    //    }
    //    foreach (var item in equipment)
    //    {
    //        if (item.data is ItemDataEquipment equipmentData)
    //        {
    //            foreach (EquipmentType slot in equipmentData.equipmentSlots)
    //            {
    //                equipmentDictionary[slot] = item;
    //            }
    //        }
    //    }
    //}


    public List<InventoryItem> GetEquippedItems()
    {
        return equipmentDictionary.Values.ToList();
    }


    public void InitializeStartingEquipment()
    {
        if (startingEquipment != null && startingEquipment.Count > 0)
        {
            foreach (var item in startingEquipment)
            {
                EquipItem(item);
            }
        }
    }



    public void OnGameLoaded()
    {
        // Rebuild dictionaries if necessary
        RebuildDictionaries();

        // Re-equip all items in the equipment list
        foreach (InventoryItem equippedItem in equipment)
        {
            // Equip the item without adding it to the equipment list again
            EquipItemOnLoad(equippedItem.data);
        }

        // Update UI
        UpdateSlotUI();
    }

    public void EquipItemOnLoad(ItemData _item)
    {
        ItemDataEquipment newEquipment = _item as ItemDataEquipment;
        InventoryItem newItem = new InventoryItem(newEquipment);

        if (newEquipment != null)
        {
            // Equip the new item in the equipment dictionary
            foreach (EquipmentType slot in newEquipment.equipmentSlots)
            {
                equipmentDictionary[slot] = newItem;
            }

            // Attach the item mesh to the character
            equipmentManager.EquipItem(newEquipment);

            // Apply equipment modifiers
            characterStats.ApplyEquipmentModifier(newEquipment);
        }
    }


}
