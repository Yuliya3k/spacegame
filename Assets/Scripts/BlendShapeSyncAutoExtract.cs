using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.PostProcessing.HistogramMonitor;


/// <summary>
/// Synchronises blend shape weights from a source <see cref="SkinnedMeshRenderer"/>
/// to one or more targets.
///
/// Renderers are expected to be named using the pattern "&lt;Tag&gt;.Shape". The
/// same tag prefix is also applied to their blend shape names. When a renderer
/// does not contain ".Shape", <see cref="defaultRendererTagPrefix"/> can be used
/// as the fallback tag.
/// </summary>
/// 

public class BlendShapeSyncAutoExtract : MonoBehaviour
{
    [Header("Source Skinned Mesh Renderer")]
    [Tooltip("The Skinned Mesh Renderer to copy blend shape weights from.")]
    public SkinnedMeshRenderer sourceRenderer;

    [Header("Target Skinned Mesh Renderers")]
    [Tooltip("List of target renderers to synchronize.")]
    public List<SkinnedMeshRenderer> targetRenderers = new List<SkinnedMeshRenderer>();

    [Header("Naming")]
    [Tooltip("Optional prefix to use when a renderer name does not contain '.Shape'.")]
    public string defaultRendererTagPrefix = string.Empty;


    // Add this flag to control synchronization
    [HideInInspector]
    public bool shouldSync = false;

    [Tooltip("When enabled, synchronization runs every frame")]
    public bool alwaysSync = false;

    private float[] previousWeights;


    // Cache mapping from source blend shape names to indices
    private Dictionary<string, int> sourceBlendShapeMapping = new Dictionary<string, int>();

    // Cache mapping from target renderers to the corresponding source indices
    private Dictionary<SkinnedMeshRenderer, int[]> targetIndexMapping = new Dictionary<SkinnedMeshRenderer, int[]>();


    void Start()
    {
        if (sourceRenderer == null || sourceRenderer.sharedMesh == null)
        {
            Debug.LogWarning("Source Renderer or its mesh is not assigned.");
            return;
        }

        // Build mapping from source blend shape names to indices
        string sourceTag = GetTagFromRendererName(sourceRenderer.name);
        int blendShapeCount = sourceRenderer.sharedMesh.blendShapeCount;
        sourceBlendShapeMapping.Clear();
        for (int i = 0; i < blendShapeCount; i++)
        {
            string sourceBlendShapeName = sourceRenderer.sharedMesh.GetBlendShapeName(i);
            string modifiedSourceName = RemovePrefix(sourceBlendShapeName, sourceTag);
            if (!sourceBlendShapeMapping.ContainsKey(modifiedSourceName))
            {
                sourceBlendShapeMapping.Add(modifiedSourceName, i);
            }
        }

        // Build index mapping for each target renderer
        targetIndexMapping.Clear();
        foreach (SkinnedMeshRenderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null || targetRenderer.sharedMesh == null)
            {
                Debug.LogWarning("Target Renderer or its mesh is not assigned.");
                continue;
            }

            string targetTag = GetTagFromRendererName(targetRenderer.name);
            int targetBlendShapeCount = targetRenderer.sharedMesh.blendShapeCount;
            int[] mapping = new int[targetBlendShapeCount];
            for (int i = 0; i < targetBlendShapeCount; i++)
            {
                string targetBlendShapeName = targetRenderer.sharedMesh.GetBlendShapeName(i);
                string modifiedTargetName = RemovePrefix(targetBlendShapeName, targetTag);
                if (sourceBlendShapeMapping.TryGetValue(modifiedTargetName, out int sourceIndex))
                {
                    mapping[i] = sourceIndex;
                }
                else
                {
                    mapping[i] = -1;
                }
            }
            targetIndexMapping[targetRenderer] = mapping;
        }


        // Set shouldSync to true to synchronize once at the start
        shouldSync = true;

        if (sourceRenderer != null && sourceRenderer.sharedMesh != null)
        {
            int count = sourceRenderer.sharedMesh.blendShapeCount;
            previousWeights = new float[count];
            for (int i = 0; i < count; i++)
            {
                previousWeights[i] = sourceRenderer.GetBlendShapeWeight(i);
            }
        }


    }

    void LateUpdate()
    {

        // Run when explicitly requested or when alwaysSync is enabled
        if (!alwaysSync && !shouldSync)
            return;

        bool changed = shouldSync;
        // Reset the flag
        shouldSync = false;

        if (sourceRenderer == null || sourceRenderer.sharedMesh == null)
        {
            Debug.LogWarning("Source Renderer or its mesh is not assigned.");
            return;
        }

        int blendShapeCount = sourceRenderer.sharedMesh.blendShapeCount;

        // If we didn't explicitly request a sync, check if weights changed
        if (!changed && previousWeights != null && previousWeights.Length == blendShapeCount)
        {
            for (int i = 0; i < blendShapeCount; i++)
            {
                float weight = sourceRenderer.GetBlendShapeWeight(i);
                if (!Mathf.Approximately(weight, previousWeights[i]))
                {
                    changed = true;
                    break;
                }
            }
        }

        if (!changed)
            return;

        if (targetRenderers == null || targetRenderers.Count == 0)
            return;

        //// Extract the sourceTag from the sourceRenderer's name
        //string sourceRendererName = sourceRenderer.name;
        //string sourceTag = GetTagFromRendererName(sourceRendererName);

        ////int blendShapeCount = sourceRenderer.sharedMesh.blendShapeCount;

        //// Create a mapping from modified blend shape names to their indices in the source mesh
        //Dictionary<string, int> sourceBlendShapeMapping = new Dictionary<string, int>();

        //// Build the source blend shape mapping
        //for (int i = 0; i < blendShapeCount; i++)
        //{
        //    string sourceBlendShapeName = sourceRenderer.sharedMesh.GetBlendShapeName(i);
        //    string modifiedSourceName = RemovePrefix(sourceBlendShapeName, sourceTag);
        //    if (!sourceBlendShapeMapping.ContainsKey(modifiedSourceName))
        //    {
        //        sourceBlendShapeMapping.Add(modifiedSourceName, i);
        //    }
        //}

        //// Iterate over each target renderer
        foreach (SkinnedMeshRenderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null || targetRenderer.sharedMesh == null)
            {
                Debug.LogWarning("Target Renderer or its mesh is not assigned.");
                continue;
            }

            if (!targetIndexMapping.TryGetValue(targetRenderer, out int[] mapping))
                continue;

            int targetBlendShapeCount = targetRenderer.sharedMesh.blendShapeCount;


            for (int i = 0; i < targetBlendShapeCount; i++)
            {
                int sourceIndex = mapping[i];
                if (sourceIndex >= 0)
                {
                    
                    float weight = sourceRenderer.GetBlendShapeWeight(sourceIndex);

                    
                    targetRenderer.SetBlendShapeWeight(i, weight);
                }
            }
        }
        // Store current weights for change detection next frame
        if (previousWeights == null || previousWeights.Length != blendShapeCount)
            previousWeights = new float[blendShapeCount];

        for (int i = 0; i < blendShapeCount; i++)
            previousWeights[i] = sourceRenderer.GetBlendShapeWeight(i);
    }

    // Helper method to extract the tag from the renderer's name
    private string GetTagFromRendererName(string rendererName)
    {
        int index = rendererName.IndexOf(".Shape");
        if (index > 0)
        {
            return rendererName.Substring(0, index);
        }
        if (!string.IsNullOrEmpty(defaultRendererTagPrefix))
        {
            Debug.LogWarning($"Renderer name '{rendererName}' does not contain '.Shape'. Using configured prefix '{defaultRendererTagPrefix}'.");
            return defaultRendererTagPrefix;
        }

        Debug.LogWarning($"Renderer name '{rendererName}' does not contain '.Shape'. Using the full name as tag.");
        return rendererName;
        
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
