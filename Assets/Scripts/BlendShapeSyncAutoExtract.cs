using UnityEngine;
using System.Collections.Generic;

public class BlendShapeSyncAutoExtract : MonoBehaviour
{
    [Header("Source Skinned Mesh Renderer")]
    [Tooltip("The Skinned Mesh Renderer to copy blend shape weights from.")]
    public SkinnedMeshRenderer sourceRenderer;

    [Header("Target Skinned Mesh Renderers")]
    [Tooltip("List of target renderers to synchronize.")]
    public List<SkinnedMeshRenderer> targetRenderers = new List<SkinnedMeshRenderer>();

    // Add this flag to control synchronization
    [HideInInspector]
    public bool shouldSync = false;

    void Start()
    {
        

        // Set shouldSync to true to synchronize once at the start
        shouldSync = true;
        
    }

    void LateUpdate()
    {

        // Only synchronize when shouldSync is true
        if (!shouldSync)
            return;

        // Reset the flag
        shouldSync = false;

        if (sourceRenderer == null || sourceRenderer.sharedMesh == null)
        {
            Debug.LogWarning("Source Renderer or its mesh is not assigned.");
            return;
        }

        if (targetRenderers == null || targetRenderers.Count == 0)
            return;

        // Extract the sourceTag from the sourceRenderer's name
        string sourceRendererName = sourceRenderer.name;
        string sourceTag = GetTagFromRendererName(sourceRendererName);

        int blendShapeCount = sourceRenderer.sharedMesh.blendShapeCount;

        // Create a mapping from modified blend shape names to their indices in the source mesh
        Dictionary<string, int> sourceBlendShapeMapping = new Dictionary<string, int>();

        // Build the source blend shape mapping
        for (int i = 0; i < blendShapeCount; i++)
        {
            string sourceBlendShapeName = sourceRenderer.sharedMesh.GetBlendShapeName(i);
            string modifiedSourceName = RemovePrefix(sourceBlendShapeName, sourceTag);
            if (!sourceBlendShapeMapping.ContainsKey(modifiedSourceName))
            {
                sourceBlendShapeMapping.Add(modifiedSourceName, i);
            }
        }

        // Iterate over each target renderer
        foreach (SkinnedMeshRenderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null || targetRenderer.sharedMesh == null)
            {
                Debug.LogWarning("Target Renderer or its mesh is not assigned.");
                continue;
            }

            // Extract the targetTag from the targetRenderer's name
            string targetRendererName = targetRenderer.name;
            string targetTag = GetTagFromRendererName(targetRendererName);

            int targetBlendShapeCount = targetRenderer.sharedMesh.blendShapeCount;

            // Iterate over each blend shape in the target mesh
            for (int i = 0; i < targetBlendShapeCount; i++)
            {
                string targetBlendShapeName = targetRenderer.sharedMesh.GetBlendShapeName(i);
                string modifiedTargetName = RemovePrefix(targetBlendShapeName, targetTag);

                // Check if the modified target blend shape exists in the source mapping
                if (sourceBlendShapeMapping.TryGetValue(modifiedTargetName, out int sourceIndex))
                {
                    // Get the weight from the source blend shape
                    float weight = sourceRenderer.GetBlendShapeWeight(sourceIndex);

                    // Set the weight on the target blend shape
                    targetRenderer.SetBlendShapeWeight(i, weight);
                }
            }
        }
    }

    // Helper method to extract the tag from the renderer's name
    private string GetTagFromRendererName(string rendererName)
    {
        int index = rendererName.IndexOf(".Shape");
        if (index > 0)
        {
            return rendererName.Substring(0, index);
        }
        else
        {
            Debug.LogWarning($"Renderer name '{rendererName}' does not contain '.Shape'. Using the full name as tag.");
            return rendererName;
        }
    }

    // Helper method to remove the prefix (tag) from the blend shape name
    private string RemovePrefix(string blendShapeName, string prefix)
    {
        if (blendShapeName.StartsWith(prefix))
        {
            return blendShapeName.Substring(prefix.Length);
        }
        else
        {
            return blendShapeName;
        }
    }
}
