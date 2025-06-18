using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    private Dictionary<string, ItemData> itemDictionary = new Dictionary<string, ItemData>();

    private void Awake()
    {
        InitializeDatabase();
    }

    public void InitializeDatabase()
    {
        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");
        Debug.Log($"Loaded {allItems.Length} items from Resources/Items.");

        foreach (ItemData item in allItems)
        {
            if (item == null)
            {
                Debug.LogWarning("Encountered a null item during database initialization.");
                continue;
            }

            if (string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogWarning($"Item '{item.name}' has an empty itemID. Skipping.");
                continue;
            }

            if (itemDictionary.ContainsKey(item.itemID))
            {
                Debug.LogWarning($"Duplicate itemID detected: {item.itemID}. Skipping.");
                continue;
            }

            itemDictionary[item.itemID] = item;
            Debug.Log($"Added item to database: {item.itemID} ({item.objectName})");
        }

        Debug.Log($"Total items in database: {itemDictionary.Count}");
    }

    public ItemData GetItemByID(string itemID)
    {
        if (itemDictionary.TryGetValue(itemID, out ItemData item))
        {
            return item;
        }
        else
        {
            Debug.LogWarning($"Item ID not found in database: {itemID}");
            // Optionally return a default item or handle the missing item appropriately
            return null;
        }
    }

    public int ItemCount
    {
        get { return itemDictionary.Count; }
    }
}
