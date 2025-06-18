//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MeshSwapManager : MonoBehaviour
//{
//    [Header("Character Stats")]
//    [Tooltip("Reference to the CharacterStats component.")]
//    public CharacterStats characterStats;

//    private SkinnedMeshRenderer defaultRenderer;

//    // List to keep track of all active mesh swap conditions
//    private List<MeshSwapConditionInstance> activeConditions = new List<MeshSwapConditionInstance>();

//    private void Start()
//    {
//        if (characterStats == null)
//        {
//            Debug.LogError("MeshSwapManager: CharacterStats reference is not assigned.");
//            enabled = false;
//            return;
//        }

//        defaultRenderer = characterStats.skinnedMeshRenderer;
//        if (defaultRenderer == null)
//        {
//            Debug.LogError("MeshSwapManager: No SkinnedMeshRenderer found in CharacterStats.");
//            enabled = false;
//            return;
//        }
//    }

//    private void Update()
//    {
//        // Iterate through a copy of the list to prevent modification during iteration
//        foreach (var conditionInstance in new List<MeshSwapConditionInstance>(activeConditions))
//        {
//            conditionInstance.CheckAndSwapMesh();
//        }
//    }

//    /// <summary>
//    /// Registers mesh swap conditions from an equipped item.
//    /// </summary>
//    /// <param name="equipment">The equipment item being equipped.</param>
//    public void RegisterMeshSwapConditions(ItemDataEquipment equipment)
//    {
//        foreach (var condition in equipment.meshSwapConditions)
//        {
//            MeshSwapConditionInstance instance = new MeshSwapConditionInstance(condition, defaultRenderer, characterStats);
//            activeConditions.Add(instance);
//        }
//    }

//    /// <summary>
//    /// Unregisters mesh swap conditions from an unequipped item.
//    /// </summary>
//    /// <param name="equipment">The equipment item being unequipped.</param>
//    public void UnregisterMeshSwapConditions(ItemDataEquipment equipment)
//    {
//        activeConditions.RemoveAll(cond => cond.IsFromEquipment(equipment));
//    }

//    /// <summary>
//    /// Inner class to handle individual mesh swap conditions.
//    /// </summary>
//    private class MeshSwapConditionInstance
//    {
//        private MeshSwapCondition condition;
//        private SkinnedMeshRenderer renderer;
//        private CharacterStats characterStats;
//        private ItemDataEquipment originatingEquipment;

//        // Track whether the mesh is currently swapped
//        private bool isSwapped = false;

//        public MeshSwapConditionInstance(MeshSwapCondition condition, SkinnedMeshRenderer renderer, CharacterStats characterStats, ItemDataEquipment equipment = null)
//        {
//            this.condition = condition;
//            this.renderer = condition.overrideRenderer != null ? condition.overrideRenderer : renderer;
//            this.characterStats = characterStats;
//            this.originatingEquipment = equipment;
//        }

//        /// <summary>
//        /// Checks the blend shape value and swaps the mesh if conditions are met.
//        /// </summary>
//        public void CheckAndSwapMesh()
//        {
//            if (renderer == null || renderer.sharedMesh == null)
//            {
//                Debug.LogError("MeshSwapConditionInstance: Renderer or mesh is null.");
//                return;
//            }

//            int blendShapeIndex = renderer.sharedMesh.GetBlendShapeIndex(condition.blendShapeName);
//            if (blendShapeIndex == -1)
//            {
//                Debug.LogError($"MeshSwapConditionInstance: Blend shape '{condition.blendShapeName}' not found in mesh '{renderer.sharedMesh.name}'.");
//                return;
//            }

//            float currentValue = renderer.GetBlendShapeWeight(blendShapeIndex);

//            if (currentValue >= condition.threshold && !isSwapped)
//            {
//                SwapMesh(condition.targetMesh);
//                isSwapped = true;
//            }
//            else if (currentValue < condition.threshold && isSwapped)
//            {
//                SwapMesh(condition.originalMesh != null ? condition.originalMesh : characterStats.GetOriginalMesh());
//                isSwapped = false;
//            }
//        }

//        /// <summary>
//        /// Swaps the mesh of the renderer.
//        /// </summary>
//        /// <param name="newMesh">The mesh to switch to.</param>
//        private void SwapMesh(Mesh newMesh)
//        {
//            if (newMesh == null)
//            {
//                Debug.LogWarning($"MeshSwapConditionInstance: New mesh is not assigned for blend shape '{condition.blendShapeName}'.");
//                return;
//            }

//            renderer.sharedMesh = newMesh;
//            Debug.Log($"MeshSwapConditionInstance: Swapped mesh to '{newMesh.name}' based on blend shape '{condition.blendShapeName}'.");
//        }

//        /// <summary>
//        /// Checks if this condition instance originated from a specific equipment.
//        /// </summary>
//        /// <param name="equipment">The equipment to check against.</param>
//        /// <returns>True if it originated from the specified equipment; otherwise, false.</returns>
//        public bool IsFromEquipment(ItemDataEquipment equipment)
//        {
//            // In this implementation, we don't store originating equipment, but if needed, you can extend the class to do so.
//            return true; // Simplified for this example
//        }
//    }
//}
