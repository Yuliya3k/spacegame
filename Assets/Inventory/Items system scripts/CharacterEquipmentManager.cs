using System.Collections.Generic;
using UnityEngine;

public class CharacterEquipmentManager : MonoBehaviour
{
    // Dictionary to store references to all the SkinnedMeshRenderers by mesh name
    private Dictionary<string, SkinnedMeshRenderer> allCharacterMeshes = new Dictionary<string, SkinnedMeshRenderer>();

    // Dictionary to store collider height adjustments per EquipmentType
    private Dictionary<EquipmentType, float> colliderHeightAdjustments = new Dictionary<EquipmentType, float>();


    // This will help us keep track of which items are currently equipped
    private Dictionary<string, ItemDataEquipment> equippedItems = new Dictionary<string, ItemDataEquipment>();

    public List<SkinnedMeshRenderer> equippedMeshRenderers = new List<SkinnedMeshRenderer>();

    // Reference to the CharacterStats script
    public CharacterStats characterStats;


    private CapsuleCollider characterCollider;
    private float originalColliderHeight;
    private Vector3 originalColliderCenter;

    //// Add this field to assign starting clothes
    //public List<ItemDataEquipment> startingEquipment;

    void Start()
    {
        // Initialize the dictionary with all SkinnedMeshRenderers in the character
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true); // Include inactive meshes

        // Initialize the capsule collider and store original values
        characterCollider = GetComponent<CapsuleCollider>();

        if (characterCollider != null)
        {
            originalColliderHeight = characterCollider.height;
            originalColliderCenter = characterCollider.center;
        }
        else
        {
            Debug.LogError("CharacterEquipmentManager: No CapsuleCollider found on the character.");
        }

        foreach (var renderer in skinnedMeshRenderers)
        {
            if (renderer.sharedMesh != null)
            {
                string meshName = renderer.sharedMesh.name;

                // Exclude core character meshes from being hidden
                if (!IsCoreCharacterMesh(meshName))
                {
                    allCharacterMeshes[meshName] = renderer;

                    renderer.enabled = false; // Start with all clothes hidden
                    Debug.Log($"Mesh {meshName} added to allCharacterMeshes and disabled.");
                }
                else
                {
                    Debug.Log($"Mesh {meshName} is a core character mesh and will remain enabled.");
                }
            }
        }

        //// Ensure hair is visible at the start (optional)
        //if (allCharacterMeshes.TryGetValue("Teena_Hair_299500.Shape", out SkinnedMeshRenderer hairRenderer))
        //{
        //    hairRenderer.enabled = true;
        //}

        // Initialize characterStats reference if not set
        if (characterStats == null)
        {
            characterStats = GetComponent<CharacterStats>();
            if (characterStats == null)
            {
                Debug.LogError("CharacterStats script not found on the character.");
            }
        }

        //// Equip starting equipment
        //if (startingEquipment != null && startingEquipment.Count > 0)
        //{
        //    foreach (var item in startingEquipment)
        //    {
        //        EquipItem(item);
        //    }
        //}

        if (characterStats == null)
        {
            characterStats = GetComponent<CharacterStats>();
            if (characterStats == null)
            {
                Debug.LogError("CharacterStats script not found on the character.");
            }
        }

    }

    // Helper method to determine if the mesh is a core part of the character (e.g., body, eyes, etc.)
    private bool IsCoreCharacterMesh(string meshName)
    {
        // List of core meshes that should always stay visible
        string[] coreMeshes = {
            "Genesis9.Shape",
            "Genesis9Eyes.Shape",
            "Genesis9EyebrowFibers.Shape",
            "Genesis9Mouth.Shape",
            "Genesis9Tear.Shape",
            "Genesis9Eyelashes.Shape"
        };

        // Return true if the mesh is one of the core character meshes
        return System.Array.Exists(coreMeshes, coreMesh => coreMesh == meshName);
    }

    // Equip logic - enables the SkinnedMeshRenderer based on the item's mesh
    public void EquipItem(ItemDataEquipment item)
    {
        if (item.itemMesh != null)
        {
            string meshName = item.itemMesh.name;

            // Enable the specific mesh associated with this item
            if (allCharacterMeshes.TryGetValue(meshName, out SkinnedMeshRenderer meshRenderer))
            {
                meshRenderer.enabled = true;
                equippedItems[meshName] = item; // Track that this item is equipped

                // Add to the list of equipped mesh renderers if not already present
                if (!equippedMeshRenderers.Contains(meshRenderer))
                {
                    equippedMeshRenderers.Add(meshRenderer);
                }

                // Synchronize blend shapes for the newly equipped item
                characterStats.SyncBlendShapesToRenderer(meshRenderer);

                Debug.Log($"Equipped {item.objectName}, enabling mesh {meshName}.");
            }
            else
            {
                Debug.LogWarning($"Mesh {meshName} not found on the character.");
            }
        }

        // Adjust collider height if necessary
        AdjustColliderHeight(item, isEquipping: true);

    }

    // Unequip logic - disables the SkinnedMeshRenderer based on the item's mesh
    public void UnequipItem(ItemDataEquipment item)
    {
        if (item.itemMesh != null)
        {
            string meshName = item.itemMesh.name;

            // Disable the specific mesh associated with this item
            if (allCharacterMeshes.TryGetValue(meshName, out SkinnedMeshRenderer meshRenderer))
            {
                meshRenderer.enabled = false;
                equippedItems.Remove(meshName); // Remove tracking for this item

                // Remove from the list of equipped mesh renderers
                equippedMeshRenderers.Remove(meshRenderer);

                Debug.Log($"Unequipped {item.objectName}, disabling mesh {meshName}.");
            }
            else
            {
                Debug.LogWarning($"Mesh {meshName} not found on the character.");
            }
        }
        // Adjust collider height if necessary
        AdjustColliderHeight(item, isEquipping: false);
    }

    private void AdjustColliderHeight(ItemDataEquipment item, bool isEquipping)
    {
        if (characterCollider == null)
        {
            Debug.LogError("CharacterEquipmentManager: No CapsuleCollider found on the character.");
            return;
        }

        foreach (EquipmentType slot in item.equipmentSlots)
        {
            if (item.colliderHeightAdjustment != 0f)
            {
                if (isEquipping)
                {
                    // Add or update the adjustment for this slot
                    colliderHeightAdjustments[slot] = item.colliderHeightAdjustment;
                }
                else
                {
                    // Remove the adjustment for this slot
                    colliderHeightAdjustments.Remove(slot);
                }
            }
        }

        // Recalculate the total height adjustment
        float totalColliderHeightAdjustment = 0f;
        foreach (var adjustment in colliderHeightAdjustments.Values)
        {
            totalColliderHeightAdjustment += adjustment;
        }

        // Adjust the collider height and center
        characterCollider.height = originalColliderHeight + totalColliderHeightAdjustment;
        //characterCollider.center = new Vector3(
        //    originalColliderCenter.x,
        //    originalColliderCenter.y + totalColliderHeightAdjustment / 2f,
        //    originalColliderCenter.z
        //);
    }


    

}
