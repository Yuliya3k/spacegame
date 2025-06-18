#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class ItemDataIDAssigner
{
    #if UNITY_EDITOR
    [MenuItem("Tools/Assign ItemData IDs")]
    public static void AssignItemDataIDs()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (itemData != null)
            {
                if (string.IsNullOrEmpty(itemData.itemID))
                {
                    itemData._itemID = guid;
                    EditorUtility.SetDirty(itemData);
                }
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Assigned item IDs to all ItemData assets.");
    }
    #endif
}
