using System.Collections.Generic;
using System;

[Serializable]
public class StorageContainerData
{
    public string containerID; // Unique identifier for the container
    public List<InventoryItemData> items;
    public Vector3Data position;
}
