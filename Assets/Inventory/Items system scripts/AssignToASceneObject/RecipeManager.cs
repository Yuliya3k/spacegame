using UnityEngine;
using System.Collections.Generic;

public class RecipeManager : MonoBehaviour
{
    public List<EatableItemData> allDishes; // Assign all dish ScriptableObjects via the Inspector

    void Awake()
    {
        InitializeDishProperties();
    }

    void InitializeDishProperties()
    {
        foreach (var dish in allDishes)
        {
            ComputeDishProperties(dish);
        }
    }

    void ComputeDishProperties(EatableItemData dishToCook)
    {
        List<InventoryItem> ingredientsUsed = dishToCook.craftingMaterials;

        // Initialize properties
        float dishCalories = 0f;
        float dishHealthImpact = 0f;
        float dishHydration = 0f;
        float dishPeristalticVelocityImpact = 0f;
        float dishVolume = 0f;
        float dishWeight = 0f;
        //float dishStaminaSpentToEat = 0f;
        //float dishStaminaSpentToCraft = 0f;
        //float dishCaloriesSpentToCraft = 0f;

        // Sum up the properties
        foreach (var ingredient in ingredientsUsed)
        {
            EatableItemData eatableIngredient = ingredient.data as EatableItemData;
            if (eatableIngredient != null)
            {
                int quantity = ingredient.stackSize;

                dishCalories += eatableIngredient.calories * quantity;
                dishHealthImpact += eatableIngredient.healthImpact * quantity;
                dishHydration += eatableIngredient.hydration * quantity;
                dishPeristalticVelocityImpact += eatableIngredient.peristalticVelocityImpact * quantity;
                dishVolume += eatableIngredient.volume * quantity;
                dishWeight += eatableIngredient.weight * quantity;
                //dishStaminaSpentToEat += eatableIngredient.staminaSpentToEat * quantity;
                //dishStaminaSpentToCraft += eatableIngredient.staminaSpentToCraft * quantity;
                //dishCaloriesSpentToCraft += eatableIngredient.caloriesSpentToCraft * quantity;
            }
        }

        // Multiply properties by 0.6 to account for cooking loss
        dishCalories *= 0.6f;
        dishHealthImpact *= 0.6f;
        dishHydration *= 0.6f;
        dishPeristalticVelocityImpact *= 0.6f;
        dishVolume *= 0.6f;
        dishWeight *= 0.6f;
        //dishStaminaSpentToEat *= 0.6f;
        //dishStaminaSpentToCraft *= 0.6f;
        //dishCaloriesSpentToCraft *= 0.6f;

        // Round the final values to one decimal place
        dishCalories = Mathf.Round(dishCalories * 10f) / 10f;
        dishHealthImpact = Mathf.Round(dishHealthImpact * 10f) / 10f;
        dishHydration = Mathf.Round(dishHydration * 10f) / 10f;
        dishPeristalticVelocityImpact = Mathf.Round(dishPeristalticVelocityImpact * 10f) / 10f;
        dishVolume = Mathf.Round(dishVolume * 10f) / 10f;
        dishWeight = Mathf.Round(dishWeight * 10f) / 10f;
        //dishStaminaSpentToEat = Mathf.Round(dishStaminaSpentToEat * 10f) / 10f;
        //dishStaminaSpentToCraft = Mathf.Round(dishStaminaSpentToCraft * 10f) / 10f;
        //dishCaloriesSpentToCraft = Mathf.Round(dishCaloriesSpentToCraft * 10f) / 10f;

        // Assign the computed properties to the dish's EatableItemData
        dishToCook.calories = dishCalories;
        dishToCook.healthImpact = dishHealthImpact;
        dishToCook.hydration = dishHydration;
        dishToCook.peristalticVelocityImpact = dishPeristalticVelocityImpact;
        dishToCook.volume = Mathf.RoundToInt(dishVolume); // Since volume is an integer
        dishToCook.weight = dishWeight;
        //dishToCook.staminaSpentToEat = dishStaminaSpentToEat;
        //dishToCook.staminaSpentToCraft = dishStaminaSpentToCraft;
        //dishToCook.caloriesSpentToCraft = dishCaloriesSpentToCraft;

        Debug.Log($"Initialized dish properties for: {dishToCook.objectName}");
    }
}
