using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Tasks/Defecation Task")]
public class NPCDefecationTask : NPCActionTask
{
    public override IEnumerator Execute(NPCController npc)
    {
        
        Debug.Log("NPCDefecationTask: Starting defecation task.");

        
        NPCDefecationManager defecationManager = npc.GetComponentInChildren<NPCDefecationManager>();
        if (defecationManager == null)
        {
            Debug.LogWarning("NPCDefecationTask: NPCDefecationManager not found on NPC.");
            IsComplete = true;
            yield break;
        }

        if (waypointIndex == 0)
        {
            defecationManager.StartDefecation();
            waypointIndex = 1;
        }

        while (defecationManager.IsDefecating)
        {
            yield return null;
        }

        Debug.Log("NPCDefecationTask: Defecation task completed.");
        IsComplete = true;
    }
}
