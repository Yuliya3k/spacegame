using UnityEngine;
using System.Collections.Generic;

public class CharacterPersonalitySetup : MonoBehaviour
{
    [System.Serializable]
    public class BlendShapeSetting
    {
        public string blendShapeName;
        [Range(0, 100)]
        public float value;
    }

    [Header("Skinned Mesh Renderer")]
    [Tooltip("Assign the Skinned Mesh Renderer containing the blend shapes.")]
    public SkinnedMeshRenderer skinnedMeshRenderer;

    [Header("Personality Blend Shapes")]
    public List<BlendShapeSetting> personalityBlendShapes = new List<BlendShapeSetting>();

    private BlendShapeSyncAutoExtract syncScript;

    void Start()
    {
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer is not assigned.");
            return;
        }

        // Apply the personality blend shapes
        ApplyPersonalityBlendShapes();

        // Find and trigger the synchronization script
        syncScript = GetComponent<BlendShapeSyncAutoExtract>();
        if (syncScript != null)
        {
            syncScript.shouldSync = true;
        }
        else
        {
            Debug.LogWarning("BlendShapeSyncAutoExtract script not found on the GameObject.");
        }
    }

    private void ApplyPersonalityBlendShapes()
    {
        foreach (var setting in personalityBlendShapes)
        {
            int blendShapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(setting.blendShapeName);
            if (blendShapeIndex >= 0)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, setting.value);
            }
            else
            {
                Debug.LogWarning($"Blend shape '{setting.blendShapeName}' not found on the mesh.");
            }
        }
    }
}

