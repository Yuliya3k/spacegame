using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPlanner : MonoBehaviour
{
    [Tooltip("List of tasks for the NPC to execute in sequence.")]
    public List<NPCActionTask> tasks = new List<NPCActionTask>();

    [Tooltip("If true, the sequence will loop indefinitely.")]
    public bool loop = true;

    public int currentTaskIndex = 0;

    public Vector3 lastTaskPosition;

    private NPCController npcController;

    private IEnumerator Start()
    {
        npcController = GetComponent<NPCController>();
        if (npcController == null)
        {
            Debug.LogError("NPCPlanner: NPCController not found on this GameObject.");
        }

        Debug.Log("NPCPlanner: Starting task sequence.");
        do
        {
            for (int i = currentTaskIndex; i < tasks.Count; i++)
            {
                currentTaskIndex = i;
                if (tasks[i] != null)
                {
                    tasks[i].ResetTask();
                    Debug.Log("NPCPlanner: Executing task: " + tasks[i].name);
                    yield return StartCoroutine(tasks[i].Execute(npcController));
                }
                else
                {
                    Debug.LogWarning("NPCPlanner: A task in the list is null.");
                }
            }
            currentTaskIndex = 0;
            yield return null;
        } while (loop);
        Debug.Log("NPCPlanner: Task sequence ended.");
    }

    //private void Awake()
    //{
    //    npcController = GetComponent<NPCController>();
    //    if (npcController == null)
    //    {
    //        Debug.LogError("NPCPlanner: NPCController not found on this GameObject.");
    //    }
    //}

    //private IEnumerator Start()
    //{
    //    Debug.Log("NPCPlanner: Starting task sequence.");
    //    do
    //    {
    //        foreach (NPCActionTask task in tasks)
    //        {
    //            if (task != null)
    //            {
    //                task.ResetTask();
    //                Debug.Log("NPCPlanner: Executing task: " + task.name);
    //                yield return StartCoroutine(task.Execute(npcController));
    //            }
    //            else
    //            {
    //                Debug.LogWarning("NPCPlanner: A task in the list is null.");
    //            }
    //        }
    //        yield return null;
    //    } while (loop);
    //    Debug.Log("NPCPlanner: Task sequence ended.");
    //}
}
