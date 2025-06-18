//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class MeshVisibilityManager : MonoBehaviour
//{
//    [Header("Character Stats")]
//    [Tooltip("Reference to the CharacterStats component.")]
//    public CharacterStats characterStats;

//    // Dictionary to map mesh IDs to their SkinnedMeshRenderer components
//    private Dictionary<string, List<SkinnedMeshRenderer>> meshRenderers = new Dictionary<string, List<SkinnedMeshRenderer>>();

//    // List to keep track of all active visibility conditions
//    private List<MeshVisibilityConditionInstance> activeConditions = new List<MeshVisibilityConditionInstance>();

//    // Dictionary to track active mesh per meshID
//    private Dictionary<string, MeshVisibilityConditionInstance> activeMeshesPerGroup = new Dictionary<string, MeshVisibilityConditionInstance>();

//    private void Start()
//    {
//        if (characterStats == null)
//        {
//            Debug.LogError("MeshVisibilityManager: CharacterStats reference is not assigned.");
//            enabled = false;
//            return;
//        }

//        // Initialize the meshRenderers dictionary with existing meshes
//        RegisterAllExistingMeshes();

//        // Subscribe to the blend shape change event
//        characterStats.OnBlendShapeValueChanged += HandleBlendShapeChange;
//    }

//    private void OnDestroy()
//    {
//        if (characterStats != null)
//        {
//            characterStats.OnBlendShapeValueChanged -= HandleBlendShapeChange;
//        }
//    }

//    /// <summary>
//    /// Registers all existing meshes with the same meshID.
//    /// </summary>
//    private void RegisterAllExistingMeshes()
//    {
//        SkinnedMeshRenderer[] skinnedMeshRenderers = FindObjectsOfType<SkinnedMeshRenderer>();

//        foreach (var renderer in skinnedMeshRenderers)
//        {
//            MeshIdentifier identifier = renderer.GetComponent<MeshIdentifier>();
//            if (identifier != null && !string.IsNullOrEmpty(identifier.MeshID))
//            {
//                string meshID = identifier.MeshID;
//                if (!meshRenderers.ContainsKey(meshID))
//                {
//                    meshRenderers[meshID] = new List<SkinnedMeshRenderer>();
//                }
//                meshRenderers[meshID].Add(renderer);
//                Debug.Log($"MeshVisibilityManager: Registered mesh '{renderer.gameObject.name}' with meshID '{meshID}'.");
//            }
//            else
//            {
//                Debug.LogWarning($"SkinnedMeshRenderer on '{renderer.gameObject.name}' does not have a MeshIdentifier component or has an empty MeshID.");
//            }
//        }

//        // Optional: List all registered mesh IDs and their renderers
//        Debug.Log("MeshVisibilityManager: All Registered Mesh IDs and Renderers:");
//        foreach (var kvp in meshRenderers)
//        {
//            Debug.Log($"MeshID: {kvp.Key}");
//            foreach (var renderer in kvp.Value)
//            {
//                Debug.Log($" - {renderer.gameObject.name}");
//            }
//        }
//    }

//    /// <summary>
//    /// Handles blend shape value changes and evaluates relevant conditions.
//    /// </summary>
//    /// <param name="blendShapeName">The name of the blend shape that changed.</param>
//    /// <param name="newValue">The new value of the blend shape.</param>
//    private void HandleBlendShapeChange(string blendShapeName, float newValue)
//    {
//        // Evaluate conditions related to this blend shape, sorted by priority descending
//        foreach (var conditionInstance in activeConditions
//            .Where(c => c.condition.blendShapeName == blendShapeName)
//            .OrderByDescending(c => c.condition.priority))
//        {
//            conditionInstance.EvaluateCondition(ref activeMeshesPerGroup, meshRenderers);
//        }
//    }

//    /// <summary>
//    /// Registers visibility conditions from an equipped item.
//    /// </summary>
//    /// <param name="equipment">The equipment item being equipped.</param>
//    public void RegisterVisibilityConditions(ItemDataEquipment equipment)
//    {
//        foreach (var condition in equipment.meshVisibilityConditions)
//        {
//            if (string.IsNullOrEmpty(condition.meshID))
//            {
//                Debug.LogWarning($"MeshVisibilityManager: meshID not set in condition for equipment '{equipment.objectName}'. Skipping condition.");
//                continue;
//            }

//            if (!meshRenderers.TryGetValue(condition.meshID, out List<SkinnedMeshRenderer> renderers))
//            {
//                Debug.LogWarning($"MeshVisibilityManager: Renderers for meshID '{condition.meshID}' not found. Skipping condition.");
//                continue;
//            }

//            // Ensure exactly two renderers per meshID (base and alternate)
//            if (renderers.Count < 2)
//            {
//                Debug.LogWarning($"MeshVisibilityManager: MeshID '{condition.meshID}' does not have both base and alternate renderers. Found {renderers.Count} renderer(s). Skipping condition.");
//                continue;
//            }

//            MeshVisibilityConditionInstance instance = new MeshVisibilityConditionInstance(condition, characterStats, equipment, renderers);
//            activeConditions.Add(instance);
//            Debug.Log($"MeshVisibilityManager: Registered condition for meshID '{condition.meshID}' with blend shape '{condition.blendShapeName}'.");

//            // Immediately evaluate the condition based on current blend shape value
//            instance.EvaluateCondition(ref activeMeshesPerGroup, meshRenderers);
//        }
//    }

//    /// <summary>
//    /// Unregisters visibility conditions from an unequipped item.
//    /// </summary>
//    /// <param name="equipment">The equipment item being unequipped.</param>
//    public void UnregisterVisibilityConditions(ItemDataEquipment equipment)
//    {
//        // Find all conditions associated with this equipment
//        var conditionsToRemove = activeConditions.Where(c => c.originatingEquipment == equipment).ToList();

//        foreach (var condition in conditionsToRemove)
//        {
//            // If this condition is the active one in its group, reset to base mesh
//            if (activeMeshesPerGroup.TryGetValue(condition.condition.meshID, out MeshVisibilityConditionInstance activeInstance) && activeInstance == condition)
//            {
//                // Toggle visibility: enable base mesh, disable alternate mesh
//                condition.baseMeshRenderer.enabled = true;
//                condition.alternateMeshRenderer.enabled = false;

//                // Remove from activeMeshesPerGroup
//                activeMeshesPerGroup.Remove(condition.condition.meshID);

//                Debug.Log($"MeshVisibilityManager: Resetting meshID '{condition.condition.meshID}' to base mesh due to unequipping '{equipment.objectName}'.");
//            }

//            activeConditions.Remove(condition);
//            Debug.Log($"MeshVisibilityManager: Unregistered condition for meshID '{condition.condition.meshID}' with blend shape '{condition.condition.blendShapeName}'.");
//        }
//    }

//    /// <summary>
//    /// Inner class to handle individual mesh visibility conditions.
//    /// </summary>
//    private class MeshVisibilityConditionInstance
//    {
//        public MeshVisibilityCondition condition;
//        public CharacterStats characterStats;
//        public ItemDataEquipment originatingEquipment;

//        public SkinnedMeshRenderer baseMeshRenderer;
//        public SkinnedMeshRenderer alternateMeshRenderer;

//        public MeshVisibilityConditionInstance(MeshVisibilityCondition condition, CharacterStats characterStats, ItemDataEquipment equipment, List<SkinnedMeshRenderer> renderers)
//        {
//            this.condition = condition;
//            this.characterStats = characterStats;
//            this.originatingEquipment = equipment;

//            // Assign base and alternate renderers based on naming conventions or order
//            // Assuming first renderer is base, second is alternate
//            this.baseMeshRenderer = renderers[0];
//            this.alternateMeshRenderer = renderers[1];
//        }

//        /// <summary>
//        /// Evaluates the condition and toggles mesh visibility accordingly.
//        /// </summary>
//        /// <param name="activeMeshesPerGroup">Reference to the active meshes per group dictionary.</param>
//        /// <param name="meshRenderersDict">Dictionary mapping meshIDs to their renderers.</param>
//        public void EvaluateCondition(ref Dictionary<string, MeshVisibilityConditionInstance> activeMeshesPerGroup, Dictionary<string, List<SkinnedMeshRenderer>> meshRenderersDict)
//        {
//            float currentValue = characterStats.GetBlendShapeValue(condition.blendShapeName);

//            bool conditionMet = currentValue >= condition.threshold;

//            if (conditionMet)
//            {
//                // Check if another condition is active in the same group
//                if (activeMeshesPerGroup.TryGetValue(condition.meshID, out MeshVisibilityConditionInstance activeInstance))
//                {
//                    // If the active instance is different, hide its alternate mesh and show current
//                    if (activeInstance != this)
//                    {
//                        activeInstance.alternateMeshRenderer.enabled = false;
//                        activeInstance.baseMeshRenderer.enabled = true;
//                        Debug.Log($"MeshVisibilityManager: Hiding alternate mesh '{activeInstance.alternateMeshRenderer.gameObject.name}' for meshID '{condition.meshID}' due to higher priority mesh '{condition.alternateMeshRenderer.gameObject.name}' being activated.");
//                    }
//                }

//                // Enable alternate mesh and disable base mesh based on 'visibleOnCondition'
//                alternateMeshRenderer.enabled = condition.visibleOnCondition;
//                baseMeshRenderer.enabled = !condition.visibleOnCondition;

//                // Update active mesh in group
//                activeMeshesPerGroup[condition.meshID] = this;

//                Debug.Log($"MeshVisibilityManager: Set alternate mesh '{alternateMeshRenderer.gameObject.name}' to {condition.visibleOnCondition} for meshID '{condition.meshID}' based on blend shape '{condition.blendShapeName}' value {currentValue}.");
//            }
//            else
//            {
//                // If this condition is the active one in its group, reset to base mesh
//                if (activeMeshesPerGroup.TryGetValue(condition.meshID, out MeshVisibilityConditionInstance activeInstance) && activeInstance == this)
//                {
//                    // Enable base mesh, disable alternate mesh
//                    baseMeshRenderer.enabled = true;
//                    alternateMeshRenderer.enabled = false;

//                    // Remove from activeMeshesPerGroup
//                    activeMeshesPerGroup.Remove(condition.meshID);

//                    Debug.Log($"MeshVisibilityManager: Reset to base mesh '{baseMeshRenderer.gameObject.name}' for meshID '{condition.meshID}' as condition no longer met.");
//                }
//            }
//        }
//    }
//}
