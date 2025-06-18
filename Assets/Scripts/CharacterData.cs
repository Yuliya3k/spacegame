using System;

[Serializable]
public class CharacterData
{
    public float currentHealth;
    public float currentFullness;
    public float currentHydration;
    public float currentIntFullness;
    public float stamina;
    public float calories;
    public float weight;
    public float peristalticVelocity;
    public float musclesState;
    public float cookingSkill;
    public float intestinesCapacity;
    public float stomachCapacity;

    // Add other necessary stats
}


[Serializable]
public class CharacterRuntimeStateData
{
    public float currentHealth;
    public float currentFullness;
    public float currentHydration;
    public float currentIntFullness;
    public float stamina;
    public float calories;
    public float weight;
    public float peristalticVelocity;
    public float musclesState;
    public float cookingSkill;
    public float intestinesCapacity;
    public float stomachCapacity;
    public bool isMoving;
    public bool isSprinting;
    // Add the missing variables:
    public bool isDecayEnabled;   // <--- Add this
    public float urineVolume;     // <--- Add this

    public DateTimeData lastFullnessDecreaseTime;
    public DateTimeData lastStaminaDecreaseTime;
    public DateTimeData lastCaloriesDecreaseTime;
    public DateTimeData lastHydrationDecreaseTime;

    public DateTimeData firstMealTime;
    public DateTimeData lastMealTime;
}