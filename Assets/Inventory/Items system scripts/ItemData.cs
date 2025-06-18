using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public enum ItemType
{
    Eatable,
    Equipment,
    Resources
    
    // Add other classes as needed
}
public class ItemData : ScriptableObject
{
    [SerializeField] public string _itemID;
    public string itemID => _itemID;

    public string objectName;
    [TextArea]
    public string description;
    public Sprite icon;
    public int stackSize;
    public float weight;
    public int volume;
    public Mesh itemMesh;
    // The material to use for rendering the item
    public Material itemMaterial;
    public ItemType itemType;
    //public EatableItemData script;


    //private void OnValidate()
    //{
    //    // Ensure that _itemID is assigned and serialized
    //    if (string.IsNullOrEmpty(_itemID))
    //    {
    //        _itemID = name; // Use the asset name as the itemID
    //        EditorUtility.SetDirty(this);
    //        AssetDatabase.SaveAssets();
    //    }
    //}



#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure that _itemID is assigned and serialized
        if (string.IsNullOrEmpty(_itemID))
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            _itemID = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }
#endif



}
