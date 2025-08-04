#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

public static class ItemDataIDAssigner
{
    #if UNITY_EDITOR
    [MenuItem("Tools/Assign ItemData IDs")]
    public static void AssignItemDataIDs()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        HashSet<string> assignedIDs = new HashSet<string>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (itemData == null)
                continue;

            string id = itemData.itemID;
            if (string.IsNullOrEmpty(id))
            {
                id = guid;
                itemData._itemID = id;
                EditorUtility.SetDirty(itemData);
            }

            if (!assignedIDs.Add(id))
            {
                if (string.IsNullOrEmpty(itemData.itemID))
                {
                    string newId = System.Guid.NewGuid().ToString();
                    Debug.LogWarning($"Duplicate ItemData ID '{id}' found in asset at {path}. Assigned new ID '{newId}'.");
                    itemData._itemID = newId;
                    assignedIDs.Add(newId);
                    EditorUtility.SetDirty(itemData);
                }
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Assigned item IDs to all ItemData assets.");
    }
    #endif
}
