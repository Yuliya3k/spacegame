using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Inventory : MonoBehaviour
{
    // Core inventory data structures
    public List<InventoryItem> ingredients;
    public List<InventoryItem> resources;
    public List<InventoryItem> dishes;
    public List<InventoryItem> equipment;
    public List<InventoryItem> inventory;

    public Dictionary<ItemData, InventoryItem> ingredientDictionary;
    public Dictionary<ItemData, InventoryItem> resourceDictionary;
    public Dictionary<ItemData, InventoryItem> dishDictionary;
    public Dictionary<EquipmentType, InventoryItem> equipmentDictionary;
    public Dictionary<ItemData, InventoryItem> inventoryDictionary;

    //// References to character components
    //public CharacterStats characterStats;
    //public CharacterEquipmentManager equipmentManager;

    //// Audio source for sounds (if needed)
    //public AudioSource audioSource;

    protected virtual void Awake()
    {
        // Initialize lists and dictionaries
        ingredients = new List<InventoryItem>();
        ingredientDictionary = new Dictionary<ItemData, InventoryItem>();

        resources = new List<InventoryItem>();
        resourceDictionary = new Dictionary<ItemData, InventoryItem>();

        inventory = new List<InventoryItem>();
        inventoryDictionary = new Dictionary<ItemData, InventoryItem>();

        dishes = new List<InventoryItem>();
        dishDictionary = new Dictionary<ItemData, InventoryItem>();

        equipment = new List<InventoryItem>();
        equipmentDictionary = new Dictionary<EquipmentType, InventoryItem>();
    }

    // Core inventory methods (AddItem, RemoveItem, EquipItem, etc.)

    public virtual void AddItem(ItemData _item, int quantity = 1)
    {
        if (_item.itemType == ItemType.Eatable)
        {
            var eatableItem = _item as EatableItemData;

            if (eatableItem != null && eatableItem.eatableType == EatableType.Ingredient)
            {
                AddToIngredients(_item, quantity);  // Adds to Ingredients with quantity
                Debug.Log("Added Ingredient to Ingredients: " + _item.objectName);
            }
            else if (eatableItem != null && eatableItem.eatableType == EatableType.Dish)
            {
                AddToDishes(_item, quantity);  // Adds to Dishes with quantity
                Debug.Log("Added Dish to Dishes: " + _item.objectName);
            }
        }
        else if (_item.itemType == ItemType.Resources)
        {
            AddToResources(_item, quantity);  // Adds to Resources with quantity
            Debug.Log("Added Resource to Resources: " + _item.objectName);
        }
        else if (_item.itemType == ItemType.Equipment)
        {
            AddToInventory(_item, quantity);  // Adds to Inventory with quantity
            Debug.Log("Added Equipment to Inventory: " + _item.objectName);
        }
    }

    protected void AddToIngredients(ItemData _item, int quantity)
    {
        if (ingredientDictionary.TryGetValue(_item, out InventoryItem value))
        {
            value.AddStack(quantity);  // Increment stack by the quantity
        }
        else
        {
            InventoryItem newItem = new InventoryItem(_item);
            newItem.AddStack(quantity - 1);  // Add the remaining quantity (initial stack is 1)
            ingredients.Add(newItem);
            ingredientDictionary.Add(_item, newItem);
            Debug.Log("Added Ingredient to Ingredients List.");
        }
    }

    protected void AddToDishes(ItemData _item, int quantity)
    {
        // Remove existing entry if it exists
        if (dishDictionary.ContainsKey(_item))
        {
            InventoryItem existingItem = dishDictionary[_item];
            dishes.Remove(existingItem);
            dishDictionary.Remove(_item);
        }

        // Add the new dish with updated properties
        InventoryItem newItem = new InventoryItem(_item);
        newItem.AddStack(quantity - 1);  // Add the remaining quantity (initial stack is 1)
        dishes.Add(newItem);
        dishDictionary.Add(_item, newItem);

        //Debug.Log("Added to Dishes: " + _item.objectName);
    }

    protected void AddToResources(ItemData _item, int quantity)
    {
        if (resourceDictionary.TryGetValue(_item, out InventoryItem value))
        {
            value.AddStack(quantity);  // Increment stack by the quantity
        }
        else
        {
            InventoryItem newItem = new InventoryItem(_item);
            newItem.AddStack(quantity - 1);  // Add the remaining quantity (initial stack is 1)
            resources.Add(newItem);
            resourceDictionary.Add(_item, newItem);
            //Debug.Log("Added Resource to Resources List.");
        }
    }

    protected void AddToInventory(ItemData _item, int quantity)
    {
        if (inventoryDictionary.TryGetValue(_item, out InventoryItem value))
        {
            value.AddStack(quantity);  // Increment stack by the quantity
        }
        else
        {
            InventoryItem newItem = new InventoryItem(_item);
            newItem.AddStack(quantity - 1);  // Add the remaining quantity (initial stack is 1)
            inventory.Add(newItem);
            inventoryDictionary.Add(_item, newItem);
        }
        //Debug.Log("Added to Inventory: " + _item.objectName);
    }

    public virtual void RemoveItem(ItemData _item, int quantity = 1)
    {
        bool itemRemoved = false;

        if (ingredientDictionary.TryGetValue(_item, out InventoryItem ingredientValue))
        {
            RemoveItemFromCollection(ingredients, ingredientDictionary, ingredientValue, _item, quantity);
            itemRemoved = true;
        }
        else if (resourceDictionary.TryGetValue(_item, out InventoryItem resourceValue))
        {
            RemoveItemFromCollection(resources, resourceDictionary, resourceValue, _item, quantity);
            itemRemoved = true;
        }
        else if (dishDictionary.TryGetValue(_item, out InventoryItem dishValue))
        {
            RemoveItemFromCollection(dishes, dishDictionary, dishValue, _item, quantity);
            itemRemoved = true;
        }
        else if (inventoryDictionary.TryGetValue(_item, out InventoryItem inventoryValue))
        {
            RemoveItemFromCollection(inventory, inventoryDictionary, inventoryValue, _item, quantity);
            itemRemoved = true;
        }

        if (itemRemoved)
        {
            // Update UI if needed (in player inventory)
        }
        else
        {
            //Debug.LogWarning("Attempted to remove item that was not in inventory: " + _item.objectName);
        }
    }

    protected void RemoveItemFromCollection(List<InventoryItem> itemList, Dictionary<ItemData, InventoryItem> itemDictionary, InventoryItem itemValue, ItemData _item, int quantity)
    {
        if (itemValue.stackSize <= quantity)
        {
            itemList.Remove(itemValue);
            itemDictionary.Remove(_item);
        }
        else
        {
            itemValue.RemoveStack(quantity);  // Reduce stack by the quantity
        }
    }

    //public virtual void EquipItem(ItemData _item)
    //{
    //    ItemDataEquipment newEquipment = _item as ItemDataEquipment;
    //    InventoryItem newItem = new InventoryItem(newEquipment);

    //    if (newEquipment != null)
    //    {
    //        // Loop through all slots the item can occupy
    //        foreach (EquipmentType slot in newEquipment.equipmentSlots)
    //        {
    //            // Check if an item is already in that slot
    //            if (equipmentDictionary.TryGetValue(slot, out InventoryItem oldEquipment))
    //            {
    //                // Unequip the old item in that slot
    //                UnequipItem(oldEquipment.data as ItemDataEquipment);
    //            }

    //            // Equip the new item in the current slot
    //            equipmentDictionary[slot] = newItem;
    //        }

    //        // Attach the item mesh to the character
    //        equipmentManager.EquipItem(newEquipment);

    //        // Apply equipment modifiers
    //        characterStats.ApplyEquipmentModifier(newEquipment);

    //        // Remove the newly equipped item from the inventory
    //        RemoveItem(_item);

    //        // Update UI if needed (in player inventory)
    //    }
    //}

    //public virtual void UnequipItem(ItemDataEquipment itemToRemove)
    //{
    //    bool itemAlreadyRemoved = false;  // Flag to ensure the item is only added back to inventory once

    //    // Remove the item from all the slots it occupies
    //    foreach (EquipmentType slot in itemToRemove.equipmentSlots)
    //    {
    //        if (equipmentDictionary.TryGetValue(slot, out InventoryItem equippedItem) && equippedItem.data == itemToRemove)
    //        {
    //            // Remove the item from the slot
    //            equipmentDictionary.Remove(slot);

    //            // Only add the item back to the inventory once
    //            if (!itemAlreadyRemoved)
    //            {
    //                equipment.Remove(equippedItem);
    //                AddItem(itemToRemove);  // Return the item to the inventory

    //                // Remove equipment modifiers
    //                characterStats.RemoveEquipmentModifier(itemToRemove);

    //                itemAlreadyRemoved = true;  // Mark that the item has been removed and added to inventory
    //            }
    //        }
    //    }

    //    // Detach the item mesh from the character
    //    equipmentManager.UnequipItem(itemToRemove);

    //    // Update UI if needed (in player inventory)
    //}

    public InventoryItem GetInventoryItem(ItemData itemData)
    {
        // Check all dictionaries
        if (ingredientDictionary.TryGetValue(itemData, out InventoryItem item))
            return item;
        if (resourceDictionary.TryGetValue(itemData, out item))
            return item;
        if (dishDictionary.TryGetValue(itemData, out item))
            return item;
        if (inventoryDictionary.TryGetValue(itemData, out item))
            return item;
        return null;
    }

    public List<InventoryItem> GetAllItems()
    {
        List<InventoryItem> allItems = new List<InventoryItem>();
        allItems.AddRange(ingredients);
        allItems.AddRange(resources);
        allItems.AddRange(dishes);
        allItems.AddRange(inventory);
        return allItems;
    }


    // IMPLEMENTATION OF RebuildDictionaries()
    public void RebuildDictionaries()
    {
        // Ensure lists exist before rebuilding
        if (ingredients == null)
            ingredients = new List<InventoryItem>();
        if (resources == null)
            resources = new List<InventoryItem>();
        if (dishes == null)
            dishes = new List<InventoryItem>();
        if (inventory == null)
            inventory = new List<InventoryItem>();
        if (equipment == null)
            equipment = new List<InventoryItem>();

        // Ensure dictionaries exist and then clear them
        if (ingredientDictionary == null)
            ingredientDictionary = new Dictionary<ItemData, InventoryItem>();
        else
            ingredientDictionary.Clear();

        if (resourceDictionary == null)
            resourceDictionary = new Dictionary<ItemData, InventoryItem>();
        else
            resourceDictionary.Clear();

        if (dishDictionary == null)
            dishDictionary = new Dictionary<ItemData, InventoryItem>();
        else
            dishDictionary.Clear();

        if (inventoryDictionary == null)
            inventoryDictionary = new Dictionary<ItemData, InventoryItem>();
        else
            inventoryDictionary.Clear();

        if (equipmentDictionary == null)
            equipmentDictionary = new Dictionary<EquipmentType, InventoryItem>();
        else
            equipmentDictionary.Clear();

        // Rebuild ingredient dictionary
        foreach (var item in ingredients)
        {
            if (item != null && item.data != null)
                ingredientDictionary[item.data] = item;
        }
        // Rebuild resource dictionary
        foreach (var item in resources)
        {
            if (item != null && item.data != null)
                resourceDictionary[item.data] = item;
        }
        // Rebuild dish dictionary
        foreach (var item in dishes)
        {
            if (item != null && item.data != null)
                dishDictionary[item.data] = item;
        }
        // Rebuild inventory dictionary
        foreach (var item in inventory)
        {
            if (item != null && item.data != null)
                inventoryDictionary[item.data] = item;
        }
        // Rebuild equipment dictionary using equipment slots from equipment items
        foreach (var item in equipment)
        {
            if (item != null && item.data is ItemDataEquipment equipmentData)
            {
                foreach (EquipmentType slot in equipmentData.equipmentSlots)
                {
                    equipmentDictionary[slot] = item;
                }
            }
        }
    }

}
