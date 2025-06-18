using System.Collections.Generic;
using UnityEngine;

public enum EatableType
{
    Ingredient,
    Dish
}

[CreateAssetMenu(fileName = "New Eatable Item", menuName = "Items/Eatable")]
public class EatableItemData : ItemData
{
    public EatableType eatableType;
    public float calories;
    public float healthImpact;
    public float hydration;

    public float staminaSpentToEat;
    public float staminaSpentToCraft;
    public float caloriesSpentToCraft;

    public float peristalticVelocityImpact;
    public float timeToEat;
    public bool canEat;
    public AudioClip eatingSound;

    [Header("Cooking Details")]
    public float timeToCraft;
    public AudioClip cookingSound; // Existing line
    public ItemEffect[] itemEffects;

    [Header("Cooking Requirements (for Dishes)")]
    public float requiredCookingSkill; // **New Field**
    public List<InventoryItem> craftingMaterials; // Ingredients required to cook the dish
}
