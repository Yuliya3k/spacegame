using UnityEngine;
using System.Collections.Generic;
using System;
using ModifiersNamespace;


public enum EquipmentType
{
    Hair,
    Headdress,
    Glasses,
    Neck,
    Body,
    Hands,
    Arms,
    Pants,
    Shoes,
    Tools,
    Thighs,
    Stockings,
    Accessories
}

[Serializable]
public class EquipmentModifier
{
    public string blendShapeName;
    public float modifierValue;  // Modifier applied to the blend shape
}

[CreateAssetMenu(fileName = "New Equipment Item", menuName = "Items/Equipment")]
public class ItemDataEquipment : ItemData
{
    [Header("Slots")]
    public List<EquipmentType> equipmentSlots;

    [Header("Collider Adjustment")]
    public float colliderHeightAdjustment = 0f;  // Amount to adjust the collider height


    [Header("Blend Shape Modifiers")]
    //public List<EquipmentModifier> blendShapeModifiers = new List<EquipmentModifier>();  // Equipment modifies blend shapes

    [Header("Major stats")]
    public int strength;
    public int agility;
    public int intelligence;
    public int vitality;

    [Header("Craft")]
    public float staminaSpentToCraft;
    public float caloriesSpentToCraft;

    [Header("Crafting Details")]
    public float timeToCraft;


    // Offensive, Defensive, Magic stats, etc., are omitted for brevity...
    [Header("Stat Modifiers")]  // Add stat modifiers
    public List<StatModifier> statModifiers; // List of stat modifiers that will affect the character

    //[Header("Blend Shape Modifiers")]
    public List<BlendShapeModifier> blendShapeModifiers; // List of blend shape changes for character's body


    [Header("Craft requirements")]
    public List<InventoryItem> craftingMaterials;

    // No need for itemMeshPrefab here anymore, we'll use itemMesh and itemMaterial from the base class
}


