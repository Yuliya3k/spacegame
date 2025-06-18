using System.Collections;
using UnityEngine;

public interface INPCInteraction
{
    /// <summary>
    /// Execute the interaction for the given NPC.
    /// </summary>
    IEnumerator ExecuteInteraction(NPCController npc);
}
