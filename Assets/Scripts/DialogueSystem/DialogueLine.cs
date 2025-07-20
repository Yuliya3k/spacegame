using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Line")]
public class DialogueLine : ScriptableObject
{
    [TextArea]
    public string lineText;
    [Header("Animation")] public string animationTrigger;

    [Header("Facial Expressions")]
    public FacialExpressionSetting[] facialExpressions;

    public DialogueResponse[] responses;
}

[System.Serializable]
public class DialogueResponse
{
    public string responseText;
    public string animationTrigger;
    public FacialExpressionSetting[] facialExpressions;
    public DialogueLine nextLine;
}

[System.Serializable]
public class FacialExpressionSetting
{
    public string blendShapeName;
    public float value = 0f;
}