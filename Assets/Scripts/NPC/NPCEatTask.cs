using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Tasks/Eat Task")]
public class NPCEatTask : NPCActionTask
{
    public override IEnumerator Execute(NPCController npc)
    {
        NPCInventory inventory = npc.inventory as NPCInventory;
        CharacterStats stats = npc.characterStats;

        if (inventory == null || stats == null)
        {
            IsComplete = true;
            yield break;
        }

        while (stats.currentFullness < stats.stomachCapacity)
        {
            ItemData food = inventory.ChooseFood(npc.dietPreference);
            if (food == null)
            {
                break;
            }

            float previousFullness = stats.currentFullness;
            inventory.EatItem(food);

            while (stats.currentFullness == previousFullness && stats.currentFullness < stats.stomachCapacity)
            {
                yield return null;
            }
        }

        IsComplete = true;
    }
}