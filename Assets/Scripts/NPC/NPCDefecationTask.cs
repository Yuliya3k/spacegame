using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Tasks/Defecation Task")]
public class NPCDefecationTask : NPCActionTask
{
    public override IEnumerator Execute(NPCController npc)
    {
        ResetTask();
        Debug.Log("NPCDefecationTask: Starting defecation task.");

        // Try to get the NPCDefecationManager from the NPC's children.
        NPCDefecationManager defecationManager = npc.GetComponentInChildren<NPCDefecationManager>();
        if (defecationManager == null)
        {
            Debug.LogWarning("NPCDefecationTask: NPCDefecationManager not found on NPC.");
            IsComplete = true;
            yield break;
        }

        // Start defecation.
        defecationManager.StartDefecation();

        // Wait until defecation is complete.
        while (defecationManager.IsDefecating)
        {
            yield return null;
        }

        Debug.Log("NPCDefecationTask: Defecation task completed.");
        IsComplete = true;
    }
}
