using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInventory : Inventory
{
    // NPC-specific variables
    public CharacterStats characterStats;
    public CharacterEquipmentManager equipmentManager;
    public AudioSource audioSource;
    public List<ItemDataEquipment> startingEquipment;
    public List<ItemData> startingResources;
    public List<ItemData> startingIngredients;

    protected override void Awake()
    {
        base.Awake();
        // Any NPC-specific initialization can be added here
    }

    void Start()
    {
        // Add starting ingredients
        if (startingIngredients != null && startingIngredients.Count > 0)
        {
            foreach (var item in startingIngredients)
            {
                AddItem(item);
            }
        }

        // Add starting resources
        if (startingResources != null && startingResources.Count > 0)
        {
            foreach (var item in startingResources)
            {
                AddItem(item);
            }
        }

        // Rebuild dictionaries in case lists were modified in the editor or loaded from save data
        RebuildDictionaries();

        // Equip starting equipment
        if (startingEquipment != null && startingEquipment.Count > 0)
        {
            foreach (var item in startingEquipment)
            {
                EquipItem(item);
            }
        }

        // Since NPCs don't have UI, we don't need to update it
    }

    // Implement methods for equipping and unequipping items
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

            // Attach the item mesh to the NPC
            equipmentManager.EquipItem(newEquipment);

            // Apply equipment modifiers
            characterStats.ApplyEquipmentModifier(newEquipment);

            // Remove the newly equipped item from the inventory
            RemoveItem(_item);

            // Since NPCs don't have UI, we don't need to update it
        }
    }

    public void UnequipItem(ItemDataEquipment itemToRemove)
    {
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

        // Detach the item mesh from the NPC
        equipmentManager.UnequipItem(itemToRemove);

        // Since NPCs don't have UI, we don't need to update it
    }

    // Implement method for consuming items
    public void EatItem(ItemData _item)
    {
        if (_item is EatableItemData eatableData)
        {
            // Check if the NPC can eat based on current fullness and item volume
            if (characterStats.currentFullness + eatableData.volume <= characterStats.stomachCapacity)
            {
                // Check if consuming negative hydration would drop below minHydration
                if (eatableData.hydration < 0f && characterStats.currentHydration + eatableData.hydration < characterStats.minHydration)
                {
                    Debug.Log($"{gameObject.name} cannot consume item: Hydration would fall below minimum.");
                    return;
                }

                // Start the eating coroutine
                StartCoroutine(EatingCoroutine(_item, eatableData));
            }
            else
            {
                Debug.Log($"{gameObject.name} is too full to eat!");
            }
        }
        else
        {
            Debug.LogError($"Item '{_item.objectName}' is not eatable!");
        }
    }


    private IEnumerator EatingCoroutine(ItemData _item, EatableItemData eatableData)
    {
        Debug.Log("EatingCoroutine started.");

        // Play the eating sound at the start
        if (eatableData.eatingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(eatableData.eatingSound);
            Debug.Log("Eating sound played.");
        }

        // Start updating the fullness, calories, hydration, and peristalticVelocity over the duration of timeToEat
        characterStats.UpdateStatsOnEating(
            eatableData.volume,
            eatableData.calories,
            eatableData.hydration,
            eatableData.peristalticVelocityImpact,
            eatableData.healthImpact,
            eatableData.timeToEat
        );

        Debug.Log($"Started updating stats: Volume={eatableData.volume}, Calories={eatableData.calories}, Hydration={eatableData.hydration}, PeristalticVelocityImpact={eatableData.peristalticVelocityImpact}, Duration={eatableData.timeToEat}s");

        // Wait for the timeToEat duration
        yield return new WaitForSeconds(eatableData.timeToEat);
        Debug.Log("Waited for timeToEat duration.");

        // Handle item consumption
        HandleConsumptionAfterEating(_item);

        Debug.Log("EatingCoroutine finished.");
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
            // Decrease stack size and log the remaining amount
            itemToConsume.RemoveStack();  // Reduce the stack size by 1
            Debug.Log($"Consumed one {_item.objectName}, remaining in stack: {itemToConsume.stackSize}");
        }
        else
        {
            // Fully remove the item from the list and dictionary if it's the last one
            itemList.Remove(itemToConsume);  // Remove the item from the list
            itemDictionary.Remove(_item);    // Remove the item from the dictionary
            Debug.Log($"Consumed the last {_item.objectName}, it has been removed.");
            // Optionally, hide the corresponding slot if needed
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RebuildDictionaries();
    }
#endif

    // Additional NPC-specific methods can be added here

    public void OnGameLoaded()
    {
        RebuildDictionaries();
        foreach (InventoryItem equippedItem in equipment)
        {
            EquipItemOnLoad(equippedItem.data);
        }
    }

    public void EquipItemOnLoad(ItemData _item)
    {
        ItemDataEquipment newEquipment = _item as ItemDataEquipment;
        InventoryItem newItem = new InventoryItem(newEquipment);

        if (newEquipment != null)
        {
            foreach (EquipmentType slot in newEquipment.equipmentSlots)
            {
                equipmentDictionary[slot] = newItem;
            }

            equipmentManager.EquipItem(newEquipment);
            characterStats.ApplyEquipmentModifier(newEquipment);
        }
    }
    
}
