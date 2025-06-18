using System.Collections.Generic;
using System;

[Serializable]
public class InventoryData
{
    public List<InventoryItemData> ingredients;
    public List<InventoryItemData> resources;
    public List<InventoryItemData> dishes;
    public List<InventoryItemData> equipment;
    public List<InventoryItemData> inventory;
}
