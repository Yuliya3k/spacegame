using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private string saveFilePath;

    [Header("References")]
    public CharacterStats characterStats;
    public PlayerInventory playerInventory;  // PlayerInventory derives from Inventory
    public InGameTimeManager timeManager;
    public PlayerControllerCode playerController;
    public ItemDatabase itemDatabase; // Assign in the Editor

    [Header("Containers")]
    public List<StorageContainer> storageContainers;      // Assign in the Editor
    public List<DisposableContainer> disposableContainers = new List<DisposableContainer>();

    public static SaveSystem instance; // Singleton pattern for static access
    private const int MAX_STACK_SIZE = 9999;
    public static bool loadedFromSave = false;

    public List<NPCSaveData> npcSaveData;

    private void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Define the save file path
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        // Initialize the ItemDatabase
        if (itemDatabase != null)
        {
            itemDatabase.InitializeDatabase();
        }
        else
        {
            Debug.LogError("ItemDatabase is not assigned in SaveSystem.");
        }
    }

    public void SaveGame()
    {
        // Ensure we have the latest list of storage containers in the scene
        RefreshStorageContainers();

        SaveData data = new SaveData();

        // Populate player data
        data.characterData = GetCharacterData(characterStats);
        data.inventoryData = GetInventoryData(playerInventory);
        data.storageContainers = GetStorageContainerData(storageContainers);
        data.disposableContainers = GetDisposableContainerData(disposableContainers);
        data.playerPosition = new Vector3Data(playerController.transform.position);
        data.currentAnimationState = GetCurrentAnimationState(playerController);
        data.inGameTime = new DateTimeData(timeManager.GetCurrentInGameTime());
        data.equippedItems = ConvertInventoryItems(playerInventory.GetEquippedItems());
        data.runtimeState = GetRuntimeStateData(characterStats);

        // Note: Removed the undefined block that referenced "npc.inventory"

        // Save chosen profile (if any)
        CharacterProfile chosenProfile = CharacterProfileManager.instance.chosenProfile;
        if (chosenProfile != null)
        {
            CharacterProfileData profileData = new CharacterProfileData();
            profileData.profileName = chosenProfile.characterName;
            profileData.enableBoobGain = chosenProfile.enableBoobGain;
            profileData.enableTorsoGain = chosenProfile.enableTorsoGain;
            profileData.enableThighsGain = chosenProfile.enableThighsGain;
            profileData.enableShinsGain = chosenProfile.enableShinsGain;
            profileData.enableArmsGain = chosenProfile.enableArmsGain;
            profileData.enableWholeBodyGain = chosenProfile.enableWholeBodyGain;
            profileData.enableGlutesGain = chosenProfile.enableGlutesGain;

            foreach (var blendShapeSetting in chosenProfile.baseBlendShapes)
            {
                BlendShapeSettingData bsd = new BlendShapeSettingData();
                bsd.blendShapeName = blendShapeSetting.blendShapeName;
                bsd.value = blendShapeSetting.value;
                profileData.baseBlendShapes.Add(bsd);
            }

            data.chosenProfileData = profileData;
        }

        // Save all NPC data
        data.npcSaveData = GetAllNPCSaveData();

        // Serialize to JSON and write to file
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("Game Saved to " + saveFilePath);

        // Optional: Notify the player
        NotificationManager.instance.ShowNotification(
            "Game Saved Successfully!",
            textColor: new Color(255f, 255f, 255f, 255f),
            backgroundColor: new Color(255f, 255f, 255f, 255f),
            backgroundSprite: null,
            startPos: new Vector2(0f, 200f),
            endPos: new Vector2(0f, 200f),
            moveDur: 4f,
            fadeDur: 2f,
            displayDur: 0f
        );
    }

    public void LoadGame()
    {
        Debug.Log("LoadGame() called.");
        Debug.Log($"LoadGame() called. ItemDatabase has {itemDatabase.ItemCount} items.");

        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase is null in SaveSystem. Cannot load items.");
            return;
        }

        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                Debug.LogError("Failed to parse save data.");
                return;
            }

            // Apply player/global data
            timeManager.SetInGameTime(data.inGameTime.ToDateTime());
            SetRuntimeStateData(data.runtimeState, characterStats);
            SetCharacterData(data.characterData, characterStats);
            StartCoroutine(characterStats.FullnessDecayTimer());

            if (data.chosenProfileData != null)
            {
                CharacterProfile loadedProfile = new CharacterProfile();
                loadedProfile.characterName = data.chosenProfileData.profileName;
                loadedProfile.enableBoobGain = data.chosenProfileData.enableBoobGain;
                loadedProfile.enableTorsoGain = data.chosenProfileData.enableTorsoGain;
                loadedProfile.enableThighsGain = data.chosenProfileData.enableThighsGain;
                loadedProfile.enableShinsGain = data.chosenProfileData.enableShinsGain;
                loadedProfile.enableArmsGain = data.chosenProfileData.enableArmsGain;
                loadedProfile.enableWholeBodyGain = data.chosenProfileData.enableWholeBodyGain;
                loadedProfile.enableGlutesGain = data.chosenProfileData.enableGlutesGain;

                loadedProfile.baseBlendShapes = new List<BlendShapeSetting>();
                foreach (var bsd in data.chosenProfileData.baseBlendShapes)
                {
                    BlendShapeSetting setting = new BlendShapeSetting();
                    setting.blendShapeName = bsd.blendShapeName;
                    setting.value = bsd.value;
                    loadedProfile.baseBlendShapes.Add(setting);
                }

                CharacterProfileManager.instance.SetChosenCharacter(loadedProfile);
                characterStats.ApplyBaseCharacterProfile(loadedProfile);
                Debug.Log("Loaded chosenProfile toggles from save.");
            }

            characterStats.UpdateFullnessBlendShapes();
            characterStats.UpdateIntestinesFullnessBlendShapes();
            characterStats.UpdateWeightBlendShapeContributions();

            SetInventoryData(data.inventoryData, playerInventory);

            // Refresh container references before applying their saved content
            RefreshStorageContainers();

            SetStorageContainerData(data.storageContainers, storageContainers);
            SetDisposableContainerData(data.disposableContainers);
            playerController.transform.position = data.playerPosition.ToVector3();
            SetCurrentAnimationState(playerController, data.currentAnimationState);
            SetEquippedItems(data.equippedItems, playerInventory);

            // The NPC data is loaded via the helper method below.
            if (data.npcSaveData != null && data.npcSaveData.Count > 0)
            {
                LoadAllNPCData(data.npcSaveData);
            }

            // Removed extra undefined code that referenced "npcController" and "npcData"

            foreach (var renderer in characterStats.skinnedMeshRenderers)
            {
                characterStats.SyncBlendShapesToRenderer(renderer);
            }

            loadedFromSave = true;

            Debug.Log("Game Loaded from " + saveFilePath);

            PlayerInventory.instance.OnGameLoaded();

            NotificationManager.instance.ShowNotification(
                "Game Loaded Successfully!",
                textColor: new Color(255f, 255f, 255f, 255f),
                backgroundColor: new Color(255f, 255f, 255f, 255f),
                backgroundSprite: null,
                startPos: new Vector2(0f, 200f),
                endPos: new Vector2(0f, 200f),
                moveDur: 4f,
                fadeDur: 2f,
                displayDur: 0f
            );
            playerInventory.RebuildDictionaries();
        }
        else
        {
            Debug.LogWarning("Save file not found at " + saveFilePath);

            NotificationManager.instance.ShowNotification("No save file found!", textColor: Color.red);
            NotificationManager.instance.ShowNotification(
                "No file was found!",
                textColor: new Color(255f, 0f, 0f, 255f),
                backgroundColor: new Color(255f, 0f, 0f, 255f),
                backgroundSprite: null,
                startPos: new Vector2(0f, 200f),
                endPos: new Vector2(0f, 200f),
                moveDur: 4f,
                fadeDur: 2f,
                displayDur: 0f
            );
        }
    }

    // ---------------------------
    // Data Conversion & Helper Methods
    // ---------------------------

    public CharacterData GetCharacterData(CharacterStats stats)
    {
        return new CharacterData
        {
            currentHealth = stats.currentHealth,
            currentFullness = stats.currentFullness,
            currentHydration = stats.currentHydration,
            currentIntFullness = stats.currentIntFullness,
            stamina = stats.stamina,
            calories = stats.GetEffectiveCalories(),
            weight = stats.weight,
            peristalticVelocity = stats.peristalticVelocity,
            musclesState = stats.GetEffectiveMusclesState(),
            cookingSkill = stats.cooking,
            intestinesCapacity = stats.intestinesCapacity,
            stomachCapacity = stats.stomachCapacity
        };
    }

    private void SetCharacterData(CharacterData data, CharacterStats stats)
    {
        stats.currentHealth = data.currentHealth;
        stats.currentFullness = data.currentFullness;
        stats.currentHydration = data.currentHydration;
        stats.currentIntFullness = data.currentIntFullness;
        stats.stamina = data.stamina;
        stats.SetLoadedCalories(data.calories);
        stats.weight = data.weight;
        stats.peristalticVelocity = data.peristalticVelocity;
        stats.loadedMusclesState = data.musclesState;
        stats.sessionIncreasedMuscles = 0f;
        stats.sessionDecreasedMuscles = 0f;
        stats.UpdateMuscleBlendShapeContributions();
        stats.cooking = data.cookingSkill;
        stats.intestinesCapacity = data.intestinesCapacity;
        stats.stomachCapacity = data.stomachCapacity;
    }

    // InventoryData using the base Inventory type
    public InventoryData GetInventoryData(Inventory inventory)
    {
        return new InventoryData
        {
            ingredients = ConvertInventoryItems(inventory.ingredients),
            resources = ConvertInventoryItems(inventory.resources),
            dishes = ConvertInventoryItems(inventory.dishes),
            equipment = ConvertInventoryItems(inventory.equipment),
            inventory = ConvertInventoryItems(inventory.inventory),
        };
    }

    // Note: Changed parameter from PlayerInventory to Inventory
    private void SetInventoryData(InventoryData data, Inventory inventory)
    {
        inventory.ingredients = ConvertToInventoryItems(data.ingredients);
        inventory.resources = ConvertToInventoryItems(data.resources);
        inventory.dishes = ConvertToInventoryItems(data.dishes);
        inventory.equipment = ConvertToInventoryItems(data.equipment);
        inventory.inventory = ConvertToInventoryItems(data.inventory);

        inventory.RebuildDictionaries();

        // If it's a PlayerInventory, update the UI:
        if (inventory is PlayerInventory playerInv)
        {
            playerInv.UpdateSlotUI();
        }

        Debug.Log("Inventory Data Loaded Successfully.");
    }

    public List<StorageContainerData> GetStorageContainerData(List<StorageContainer> containers)
    {
        List<StorageContainerData> containerDataList = new List<StorageContainerData>();

        foreach (StorageContainer container in containers)
        {
            StorageContainerData data = new StorageContainerData
            {
                containerID = container.containerID,
                items = ConvertInventoryItems(container.containerItems),
                position = new Vector3Data(container.transform.position),
            };
            Debug.Log($"Saving StorageContainer ID: {data.containerID}");
            containerDataList.Add(data);
        }
        return containerDataList;
    }

    private void SetStorageContainerData(List<StorageContainerData> dataList, List<StorageContainer> containers)
    {
        foreach (StorageContainerData data in dataList)
        {
            StorageContainer container = containers.Find(c => c.containerID == data.containerID);
            if (container != null)
            {
                container.containerItems = ConvertToInventoryItems(data.items);
                container.UpdateStorageUI();
            }
            else
            {
                Debug.LogWarning($"StorageContainer with ID {data.containerID} not found in the scene.");
            }
        }
    }

    public List<DisposableContainerData> GetDisposableContainerData(List<DisposableContainer> containers)
    {
        List<DisposableContainerData> containerDataList = new List<DisposableContainerData>();

        foreach (DisposableContainer container in containers)
        {
            DisposableContainerData data = new DisposableContainerData
            {
                containerID = container.containerID,
                items = ConvertContainerItems(container.containerItems),
                position = new Vector3Data(container.transform.position),
            };
            containerDataList.Add(data);
        }
        return containerDataList;
    }

    private void SetDisposableContainerData(List<DisposableContainerData> dataList)
    {
        foreach (DisposableContainer container in disposableContainers)
        {
            Destroy(container.gameObject);
        }
        disposableContainers.Clear();

        foreach (DisposableContainerData data in dataList)
        {
            GameObject containerPrefab = Resources.Load<GameObject>("DisposableContainer");
            if (containerPrefab == null)
            {
                Debug.LogError("DisposableContainer prefab not found in Resources folder.");
                continue;
            }

            GameObject newContainer = Instantiate(containerPrefab, data.position.ToVector3(), Quaternion.identity);
            DisposableContainer container = newContainer.GetComponent<DisposableContainer>();
            if (container == null)
            {
                Debug.LogError("DisposableContainer component missing from the instantiated prefab.");
                Destroy(newContainer);
                continue;
            }

            container.containerItems = ConvertToContainerItems(data.items);
            container.containerID = data.containerID;
            disposableContainers.Add(container);
        }

        Debug.Log("Disposable containers loaded successfully.");
    }

    private string GetCurrentAnimationState(PlayerControllerCode playerController)
    {
        return playerController.GetCurrentAnimationState();
    }

    private void SetCurrentAnimationState(PlayerControllerCode playerController, string animationState)
    {
        playerController.SetAnimationState(animationState);
    }

    public List<InventoryItemData> ConvertInventoryItems(List<InventoryItem> items)
    {
        Dictionary<string, InventoryItemData> uniqueItems = new Dictionary<string, InventoryItemData>();

        foreach (var item in items)
        {
            if (item == null || item.data == null)
            {
                Debug.LogWarning("Null item or item.data during saving.");
                continue;
            }

            string itemID = item.data.itemID;
            if (uniqueItems.ContainsKey(itemID))
            {
                uniqueItems[itemID].quantity += item.stackSize;
            }
            else
            {
                uniqueItems[itemID] = new InventoryItemData
                {
                    itemID = itemID,
                    quantity = item.stackSize
                };
            }
        }

        Debug.Log("Saving unique items to prevent duplication.");
        return uniqueItems.Values.ToList();
    }

    private List<InventoryItem> ConvertToInventoryItems(List<InventoryItemData> dataList)
    {
        List<InventoryItem> items = new List<InventoryItem>();
        foreach (var data in dataList)
        {
            int quantity = Mathf.Clamp(data.quantity, 0, MAX_STACK_SIZE);
            ItemData itemData = itemDatabase.GetItemByID(data.itemID);
            if (itemData != null)
            {
                InventoryItem item = new InventoryItem(itemData)
                {
                    stackSize = quantity
                };
                items.Add(item);
            }
            else
            {
                Debug.LogWarning($"Failed to load item with ID: {data.itemID}. ItemData is null.");
            }
        }
        return items;
    }

    private List<InventoryItemData> ConvertContainerItems(List<ContainerItem> items)
    {
        List<InventoryItemData> itemDataList = new List<InventoryItemData>();
        foreach (var item in items)
        {
            InventoryItemData data = new InventoryItemData
            {
                itemID = item.itemData.itemID,
                quantity = item.quantity,
            };
            itemDataList.Add(data);
        }
        return itemDataList;
    }

    private List<ContainerItem> ConvertToContainerItems(List<InventoryItemData> dataList)
    {
        List<ContainerItem> items = new List<ContainerItem>();
        foreach (var data in dataList)
        {
            ItemData itemData = itemDatabase.GetItemByID(data.itemID);
            if (itemData != null)
            {
                ContainerItem item = new ContainerItem(itemData, data.quantity);
                items.Add(item);
            }
            else
            {
                Debug.LogWarning($"Failed to load item with ID: {data.itemID}. ItemData is null.");
            }
        }
        return items;
    }

    public static void RegisterDisposableContainer(DisposableContainer container)
    {
        if (instance != null && !instance.disposableContainers.Contains(container))
        {
            instance.disposableContainers.Add(container);
        }
    }

    public static void UnregisterDisposableContainer(DisposableContainer container)
    {
        if (instance != null && instance.disposableContainers.Contains(container))
        {
            instance.disposableContainers.Remove(container);
        }
    }

    // Helper to rebuild the list of storage containers currently in the scene
    private void RefreshStorageContainers()
    {
        storageContainers = FindObjectsOfType<StorageContainer>().ToList();
    }

    private void SetEquippedItems(List<InventoryItemData> dataList, Inventory inventory)
    {
        if (dataList == null || inventory == null)
            return;

        foreach (var data in dataList)
        {
            ItemData itemData = itemDatabase.GetItemByID(data.itemID);
            if (itemData != null)
            {
                if (inventory is PlayerInventory playerInv)
                {
                    playerInv.EquipItem(itemData);
                }
                else if (inventory is NPCInventory npcInv)
                {
                    npcInv.EquipItem(itemData);
                }
            }
            else
            {
                Debug.LogWarning($"Failed to load equipped item with ID: {data.itemID}. ItemData is null.");
            }
        }
    }

    public CharacterRuntimeStateData GetRuntimeStateData(CharacterStats stats)
    {
        CharacterRuntimeStateData rtData = new CharacterRuntimeStateData();
        rtData.isDecayEnabled = stats.GetIsDecayEnabled();
        rtData.currentFullness = stats.currentFullness;
        rtData.currentIntFullness = stats.currentIntFullness;
        rtData.urineVolume = stats.urineVolume;
        rtData.stamina = stats.stamina;
        rtData.calories = stats.calories;
        rtData.currentHydration = stats.currentHydration;
        rtData.peristalticVelocity = stats.peristalticVelocity;
        rtData.currentHealth = stats.currentHealth;
        rtData.isMoving = stats.IsMoving();
        rtData.isSprinting = stats.IsSprinting();

        rtData.lastFullnessDecreaseTime = new DateTimeData(stats.GetLastFullnessDecreaseTime());
        rtData.lastStaminaDecreaseTime = new DateTimeData(stats.GetLastStaminaDecreaseTime());
        rtData.lastHydrationDecreaseTime = new DateTimeData(stats.GetLastHydrationDecreaseTime());

        DateTime? firstMeal = stats.GetFirstMealTime();
        if (firstMeal.HasValue)
            rtData.firstMealTime = new DateTimeData(firstMeal.Value);

        DateTime? lastMeal = stats.GetLastMealTime();
        if (lastMeal.HasValue)
            rtData.lastMealTime = new DateTimeData(lastMeal.Value);

        return rtData;
    }

    private void SetRuntimeStateData(CharacterRuntimeStateData data, CharacterStats stats)
    {
        stats.SetIsDecayEnabled(data.isDecayEnabled);
        stats.peristalticVelocity = data.peristalticVelocity;
        stats.currentHealth = data.currentHealth;
        stats.SetIsMoving(data.isMoving);
        stats.SetIsSprinting(data.isSprinting);

        stats.SetLastFullnessDecreaseTime(data.lastFullnessDecreaseTime.ToDateTime());
        stats.SetLastStaminaDecreaseTime(data.lastStaminaDecreaseTime.ToDateTime());
        stats.SetLastCaloriesDecreaseTime(data.lastCaloriesDecreaseTime.ToDateTime());
        stats.SetLastHydrationDecreaseTime(data.lastHydrationDecreaseTime.ToDateTime());

        DateTime? firstMeal = data.firstMealTime != null ? (DateTime?)data.firstMealTime.ToDateTime() : null;
        stats.SetFirstMealTime(firstMeal);

        DateTime? lastMeal = data.lastMealTime != null ? (DateTime?)data.lastMealTime.ToDateTime() : null;
        stats.SetLastMealTime(lastMeal);

        stats.RecalculateTargetWeightAndStartTransition();
        stats.RestoreCoroutinesAndTimers();
    }

    private List<NPCSaveData> GetAllNPCSaveData()
    {
        List<NPCSaveData> npcDataList = new List<NPCSaveData>();
        NPCController[] npcControllers = FindObjectsOfType<NPCController>();
        foreach (NPCController npc in npcControllers)
        {
            NPCSaveData data = npc.GetSaveData();
            npcDataList.Add(data);
        }
        return npcDataList;
    }

    private void LoadAllNPCData(List<NPCSaveData> npcDataList)
    {
        foreach (NPCSaveData npcData in npcDataList)
        {
            NPCController npcController = FindNPCControllerByName(npcData.npcName);
            if (npcController != null)
            {
                // Use the saved lastTaskPosition if it's not zero; otherwise, use playerPosition.
                Vector3 loadPos = npcData.lastTaskPosition.ToVector3();
                if (loadPos == Vector3.zero)
                {
                    Debug.LogWarning($"[NPC LOAD] {npcData.npcName} - lastTaskPosition is (0,0,0). Using saved transform position instead: {npcData.playerPosition.ToVector3()}");
                    loadPos = npcData.playerPosition.ToVector3();
                }

                // Warp the NPC to the chosen load position
                npcController.navAgent.Warp(loadPos);
                Debug.Log($"[NPC LOAD] {npcController.npcName} warped to: {npcController.transform.position}");

                // Set destination to the saved navDestination
                Vector3 dest = npcData.navDestination.ToVector3();
                npcController.navAgent.SetDestination(dest);
                Debug.Log($"[NPC LOAD] {npcController.npcName} destination set to: {dest}");

                // Ensure the collider is enabled (if applicable)
                Collider col = npcController.GetComponent<Collider>();
                if (col != null && !col.enabled)
                {
                    col.enabled = true;
                    Debug.LogWarning($"[NPC LOAD] {npcController.npcName} collider was disabled. Enabling it now.");
                }

                // Restart the NPC's animation from the saved state
                npcController.animator.Play(npcData.currentAnimationState);
                Debug.Log($"[NPC LOAD] {npcController.npcName} playing animation: {npcData.currentAnimationState}");

                // For the planner: restart the current task from the beginning.
                NPCPlanner planner = npcController.GetComponent<NPCPlanner>();
                if (planner != null)
                {
                    planner.currentTaskIndex = npcData.currentTaskIndex;
                    planner.SetTaskStates(npcData.taskStates);
                    planner.StopAllCoroutines();
                    planner.StartCoroutine(planner.RunFromCurrent());
                    Debug.Log($"[NPC LOAD] {npcController.npcName} planner task index restored to: {planner.currentTaskIndex}");
                }

                // Restore character stats and inventory as before
                SetCharacterData(npcData.characterData, npcController.characterStats);
                SetRuntimeStateData(npcData.runtimeState, npcController.characterStats);
                if (npcController.inventory != null && npcData.inventoryData != null)
                    SetInventoryData(npcData.inventoryData, npcController.inventory);

                if (npcController.inventory != null && npcData.equippedItems != null)
                    SetEquippedItems(npcData.equippedItems, npcController.inventory);

                if (npcData.storageContainers != null)
                    SetStorageContainerData(npcData.storageContainers, storageContainers);

                if (npcData.disposableContainers != null)
                    SetDisposableContainerData(npcData.disposableContainers);

                if (npcData.inGameTime != null)
                    timeManager.SetInGameTime(npcData.inGameTime.ToDateTime());

                if (npcData.chosenProfileData != null)
                {
                    CharacterProfile loadedProfile = new CharacterProfile();
                    loadedProfile.characterName = npcData.chosenProfileData.profileName;
                    loadedProfile.enableBoobGain = npcData.chosenProfileData.enableBoobGain;
                    loadedProfile.enableTorsoGain = npcData.chosenProfileData.enableTorsoGain;
                    loadedProfile.enableThighsGain = npcData.chosenProfileData.enableThighsGain;
                    loadedProfile.enableShinsGain = npcData.chosenProfileData.enableShinsGain;
                    loadedProfile.enableArmsGain = npcData.chosenProfileData.enableArmsGain;
                    loadedProfile.enableWholeBodyGain = npcData.chosenProfileData.enableWholeBodyGain;
                    loadedProfile.enableGlutesGain = npcData.chosenProfileData.enableGlutesGain;

                    loadedProfile.baseBlendShapes = new List<BlendShapeSetting>();
                    foreach (var bsd in npcData.chosenProfileData.baseBlendShapes)
                    {
                        BlendShapeSetting setting = new BlendShapeSetting();
                        setting.blendShapeName = bsd.blendShapeName;
                        setting.value = bsd.value;
                        loadedProfile.baseBlendShapes.Add(setting);
                    }

                    npcController.characterStats.ApplyBaseCharacterProfile(loadedProfile);
                }
            }
            else
            {
                Debug.LogWarning($"[NPC LOAD] NPC with name {npcData.npcName} not found in the scene. Consider instantiating an NPC prefab.");
            }
        }
    }




    private NPCController FindNPCControllerByName(string npcName)
    {
        NPCController[] npcControllers = FindObjectsOfType<NPCController>();
        foreach (NPCController npc in npcControllers)
        {
            if (npc.npcName == npcName)
                return npc;
        }
        return null;
    }
}
