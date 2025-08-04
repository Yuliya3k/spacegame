using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public CharacterData characterData;
    public InventoryData inventoryData;
    public List<StorageContainerData> storageContainers;
    public List<DisposableContainerData> disposableContainers;
    public Vector3Data playerPosition;
    public string currentAnimationState;
    public DateTimeData inGameTime;
    public CharacterRuntimeStateData runtimeState;
    public CharacterProfileData chosenProfileData;
    public List<NPCSaveData> npcSaveData;

}