using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Tasks/Scheduled Meal Task")]
public class NPCScheduledMealTask : NPCActionTask
{
    [Serializable]
    public struct MealWindow
    {
        public int startHour;
        public int endHour;
    }

    [Tooltip("List of meal windows during the day.")]
    public MealWindow[] mealWindows;

    [Tooltip("Interval between fullness checks in game minutes.")]
    public float checkIntervalMinutes = 30f;

    private IEnumerator EatUntilFull(NPCController npc)
    {
        CharacterStats stats = npc.characterStats;
        NPCInventory inventory = npc.inventory as NPCInventory;
        if (stats == null || inventory == null)
        {
            yield break;
        }

        while (stats.currentFullness < stats.stomachCapacity)
        {
            InventoryItem foodItem = null;
            if (inventory.dishes != null && inventory.dishes.Count > 0)
            {
                foodItem = inventory.dishes[0];
            }
            else if (inventory.ingredients != null && inventory.ingredients.Count > 0)
            {
                foodItem = inventory.ingredients[0];
            }

            if (foodItem == null)
            {
                break; // No food available
            }

            float eatTime = 0f;
            if (foodItem.data is EatableItemData eatable)
            {
                eatTime = eatable.timeToEat;
            }

            inventory.EatItem(foodItem.data);

            float timer = 0f;
            while (timer < eatTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }

    private IEnumerator WaitForRealSeconds(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public override IEnumerator Execute(NPCController npc)
    {
        if (npc == null || npc.characterStats == null)
        {
            IsComplete = true;
            yield break;
        }

        InGameTimeManager timeManager = npc.characterStats.timeManager;
        if (timeManager == null)
        {
            Debug.LogWarning("NPCScheduledMealTask: InGameTimeManager not found.");
            IsComplete = true;
            yield break;
        }

        for (int i = waypointIndex; i < mealWindows.Length; i++)
        {
            MealWindow window = mealWindows[i];

            // Wait until the start of the window
            while (timeManager.GetCurrentInGameTime().Hour < window.startHour)
            {
                yield return null;
            }

            // Initial check at start of window
            yield return EatUntilFull(npc);

            // Checks during the window
            while (timeManager.GetCurrentInGameTime().Hour < window.endHour)
            {
                float interval = timeManager.GetRealTimeDurationForGameMinutes(checkIntervalMinutes);
                yield return WaitForRealSeconds(interval);
                yield return EatUntilFull(npc);
            }

            // Final check at endHour
            yield return EatUntilFull(npc);

            waypointIndex = i + 1;
        }

        IsComplete = true;
    }

    public override void ResetTask()
    {
        base.ResetTask();
    }
}