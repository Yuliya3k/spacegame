//using UnityEngine;

//[System.Serializable]
//public class MeshVisibilityCondition
//{
//    [Header("Blend Shape Settings")]
//    [Tooltip("Name of the blend shape to monitor.")]
//    public string blendShapeName = "Genesis9__countrygirl2";

//    [Tooltip("Threshold value to toggle visibility. If the blend shape value is >= Threshold, toggle visibility.")]
//    [Range(0f, 100f)]
//    public float threshold = 80f;

//    [Header("Mesh Settings")]
//    [Tooltip("Shared meshID for both base and alternate meshes.")]
//    public string meshID = "Jeans"; // Example meshID

//    [Header("Alternate Mesh Renderer")]
//    [Tooltip("Reference to the alternate SkinnedMeshRenderer.")]
//    public SkinnedMeshRenderer alternateMeshRenderer;

//    [Header("Visibility Settings")]
//    [Tooltip("If true, the alternate mesh will be visible when the condition is met.")]
//    public bool visibleOnCondition = true;

//    [Header("Condition Priority")]
//    [Tooltip("Priority of this condition. Higher values take precedence over lower ones.")]
//    public int priority = 0;
//}
