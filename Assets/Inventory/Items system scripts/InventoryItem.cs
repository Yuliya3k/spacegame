using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemData data;  // Reference to the item data
    public int stackSize;  // Number of items in the stack

    public InventoryItem(ItemData _newItemData)
    {
        data = _newItemData;
        stackSize = 1;  // Default to 1 when first created
    }

    // Add to the stack with a flexible quantity
    public void AddStack(int amount = 1)
    {
        stackSize += amount;
    }

    // Remove from the stack with a flexible quantity, ensuring the stack doesn't drop below zero
    public void RemoveStack(int amount = 1)
    {
        stackSize = Mathf.Max(stackSize - amount, 0);  // Ensure stack size never goes below 0
    }
}
