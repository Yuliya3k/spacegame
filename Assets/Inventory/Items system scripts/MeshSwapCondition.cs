//using UnityEngine;

//[System.Serializable]
//public class MeshSwapCondition
//{
//    [Header("Blend Shape Settings")]
//    [Tooltip("Name of the blend shape to monitor.")]
//    public string blendShapeName = "Genesis9__countrygirl2";

//    [Tooltip("Threshold value to trigger mesh swap. If the blend shape value is >= Threshold, swap to Target Mesh.")]
//    [Range(0f, 100f)]
//    public float threshold = 80f;

//    [Header("Mesh Settings")]
//    [Tooltip("Mesh to switch to when the condition is met.")]
//    public Mesh targetMesh;

//    [Tooltip("Original mesh to revert to when the condition is not met. If left empty, the current mesh will be used.")]
//    public Mesh originalMesh;

//    [Tooltip("Optional: Override the SkinnedMeshRenderer for mesh swapping. If left empty, it will use the SkinnedMeshRenderer from CharacterStats.")]
//    public SkinnedMeshRenderer overrideRenderer;
//}
