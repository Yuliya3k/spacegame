using System;
using System.Collections.Generic;

[Serializable]
public class NPCActionTaskState
{
    public string taskName;
    public float elapsedTime;
    public int waypointIndex;
    public bool isComplete;
    public List<NPCActionTaskState> subStates;
}