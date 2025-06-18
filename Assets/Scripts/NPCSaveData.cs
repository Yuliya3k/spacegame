using System;
using System.Collections.Generic;

[Serializable]
public class NPCSaveData
{
    public string npcName; // Unique identifier

    public CharacterData characterData;
    public InventoryData inventoryData;
    public List<InventoryItemData> equippedItems;
    public List<StorageContainerData> storageContainers;
    public List<DisposableContainerData> disposableContainers;
    public Vector3Data playerPosition; // Current transform position

    // NEW: Fields for additional NPC state
    public Vector3Data navDestination;      // The saved destination of the NPC's NavMeshAgent
    public string currentAnimationState;      // The name of the current animation state
    public int currentTaskIndex;              // The index of the current task in the planner
    public Vector3Data lastTaskPosition;        // The position where the last task ended

    public DateTimeData inGameTime;
    public CharacterRuntimeStateData runtimeState;
    public CharacterProfileData chosenProfileData;
}
