using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlendShapeValue
{
    public string blendShapeName; // Name of the blend shape
    public float blendShapeValue; // Value to set (0-100)
}

public class ClothesBlendShapeController : MonoBehaviour
{
    [Header("Blend Shape Settings")]
    [Tooltip("List of blend shapes to apply on this clothes object.")]
    public List<BlendShapeValue> blendShapes = new List<BlendShapeValue>(); // List of blend shapes and their values

    private SkinnedMeshRenderer skinnedMeshRenderer; // Reference to the SkinnedMeshRenderer on the clothes object

    // Store the base values for blend shapes to use in sync adjustments
    private Dictionary<string, float> baseBlendShapeValues = new Dictionary<string, float>();

    void Start()
    {
        // Get the SkinnedMeshRenderer component from the current game object
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null)
        {
            Debug.LogError("Skinned Mesh Renderer or its mesh is not assigned on the clothes object.");
            return;
        }

        // Apply the blend shape values at the start of the game
        ApplyBlendShapes();
    }

    // Apply the blend shapes to the clothes' Skinned Mesh Renderer
    void ApplyBlendShapes()
    {
        foreach (BlendShapeValue blendShape in blendShapes)
        {
            // Get the index of the blend shape by its name
            int blendShapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(blendShape.blendShapeName);

            // If blend shape exists, apply the value
            if (blendShapeIndex >= 0)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, blendShape.blendShapeValue);

                // Store the base value
                if (!baseBlendShapeValues.ContainsKey(blendShape.blendShapeName))
                {
                    baseBlendShapeValues.Add(blendShape.blendShapeName, blendShape.blendShapeValue);
                }
                else
                {
                    baseBlendShapeValues[blendShape.blendShapeName] = blendShape.blendShapeValue;
                }

                Debug.Log($"Applied blend shape '{blendShape.blendShapeName}' with value {blendShape.blendShapeValue}");
            }
            else
            {
                Debug.LogWarning($"Blend shape '{blendShape.blendShapeName}' not found on the clothes object.");
            }
        }
    }

    // Getter for base blend shape values
    public Dictionary<string, float> GetBaseBlendShapeValues()
    {
        return baseBlendShapeValues;
    }
}
