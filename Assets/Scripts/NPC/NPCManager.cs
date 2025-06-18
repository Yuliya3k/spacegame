//using System.Collections.Generic;
//using UnityEngine;

//public class NPCManager : MonoBehaviour
//{
//    public GameObject npcPrefab; // Assign your NPC prefab here
//    public List<NPCController> activeNPCs = new List<NPCController>();

//    /// <summary>
//    /// Spawns an NPC at the given position with the specified initial data.
//    /// </summary>
//    public void SpawnNPC(Vector3 spawnPosition, CharacterProfile profile, CharacterData initialData)
//    {
//        GameObject npcObj = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
//        NPCController npcController = npcObj.GetComponent<NPCController>();
//        npcController.Initialize(initialData, profile);
//        activeNPCs.Add(npcController);
//    }

//    /// <summary>
//    /// Called during saving to retrieve a list of all NPC save data.
//    /// </summary>
//    public List<NPCSaveData> GetAllNPCSaveData()
//    {
//        List<NPCSaveData> saveDataList = new List<NPCSaveData>();
//        foreach (var npc in activeNPCs)
//        {
//            saveDataList.Add(npc.GetSaveData());
//        }
//        return saveDataList;
//    }
//}
