using UnityEngine;

[CreateAssetMenu(fileName = "New Resource Item", menuName = "Items/Resource")]
public class ItemDataResources : ItemData
{
    [Header("Resource-specific attributes")]
    public int rarity; // Optional attribute, can define resource rarity
    public bool isCraftable; // Defines if this resource can be used in crafting
    public float staminaUse;
}
