using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Line")]
public class DialogueLine : ScriptableObject
{
    [TextArea]
    public string lineText;
    public DialogueResponse[] responses;
}

[System.Serializable]
public class DialogueResponse
{
    public string responseText;
    public DialogueLine nextLine;
}