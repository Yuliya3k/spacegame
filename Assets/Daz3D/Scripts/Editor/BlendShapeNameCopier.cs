//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(SkinnedMeshRenderer))]
//public class BlendShapeNameCopier : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        // Draw the default inspector
//        DrawDefaultInspector();

//        // Reference to the SkinnedMeshRenderer component
//        SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)target;

//        if (GUILayout.Button("Copy Blend Shape Names"))
//        {
//            CopyBlendShapeNames(skinnedMeshRenderer);
//        }
//    }

//    private void CopyBlendShapeNames(SkinnedMeshRenderer skinnedMeshRenderer)
//    {
//        if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
//        {
//            Mesh mesh = skinnedMeshRenderer.sharedMesh;
//            int blendShapeCount = mesh.blendShapeCount;

//            if (blendShapeCount == 0)
//            {
//                Debug.LogWarning("No blend shapes found.");
//                return;
//            }

//            // Prepare the list of blend shape names
//            string blendShapeNames = "";
//            for (int i = 0; i < blendShapeCount; i++)
//            {
//                blendShapeNames += mesh.GetBlendShapeName(i) + "\n";
//            }

//            // Copy blend shape names to the clipboard
//            EditorGUIUtility.systemCopyBuffer = blendShapeNames;

//            // Log the operation
//            Debug.Log($"Copied {blendShapeCount} blend shape names to clipboard:\n{blendShapeNames}");
//        }
//        else
//        {
//            Debug.LogError("Skinned Mesh Renderer or Mesh is not assigned.");
//        }
//    }
//}
