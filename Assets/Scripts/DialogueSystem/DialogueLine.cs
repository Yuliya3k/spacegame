using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Line")]
public class DialogueLine : ScriptableObject
{
    [TextArea]
    public string lineText;
    [Header("Animation")] public string animationTrigger;

    [Header("Facial Expressions")]
    public BlendShapeSetting[] facialExpressions;

    public DialogueResponse[] responses;
}

[System.Serializable]
public class DialogueResponse
{
    public string responseText;
    public string animationTrigger;
    public BlendShapeSetting[] facialExpressions;
    public DialogueLine nextLine;
}

//[System.Serializable]
//public class FacialExpressionSetting
//{
//    public string blendShapeName;
//    public float value = 0f;
//    public float durationInGameMinutes = 0.5f;
//}