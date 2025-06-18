using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public ItemData itemData;

    // Rotation and floating effect settings
    public float rotationSpeed = 10f; // Speed of rotation
    public float floatAmplitude = 0.05f; // How much the object floats up and down
    public float floatFrequency = 1f; // How fast the object floats up and down
    public string objectName = "Default Object Name";
    public string locationText = "Default Location";

    private Vector3 initialPosition;

    void Start()
    {
        InitializeItemMesh();
        AddCapsuleColliderToMesh();
        initialPosition = transform.position; // Store the initial position for the floating effect
    }

    private void InitializeItemMesh()
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData is not assigned to " + gameObject.name);
            return;
        }

        // Initialize the MeshFilter and MeshRenderer for the pickup object
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        if (itemData.itemMesh != null)
        {
            // Assign a unique instance of the mesh
            meshFilter.mesh = Instantiate(itemData.itemMesh);
        }
        else
        {
            Debug.LogWarning("ItemMesh is not assigned in itemData for " + itemData.itemID);
        }

        // Add a standard MeshRenderer (not SkinnedMeshRenderer) for pickup items in the world
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Set the material
        if (itemData.itemMaterial != null)
        {
            meshRenderer.material = itemData.itemMaterial;
        }
        else
        {
            Debug.LogWarning("ItemMaterial is not assigned in itemData for " + itemData.itemID);
        }
    }

    private void AddCapsuleColliderToMesh()
    {
        CapsuleCollider capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
        if (capsuleCollider == null)
        {
            capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        }

        // Set the height to match the mesh bounds, and reduce the radius by 50%
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.mesh != null)
        {
            Bounds bounds = meshFilter.mesh.bounds;

            capsuleCollider.height = bounds.size.y;
            capsuleCollider.radius = bounds.size.x * 0.25f; // Reduce radius by 50%
            capsuleCollider.center = bounds.center;
        }
    }

    // Pickup logic
    public void OnPickup()
    {
        PlayerInventory.instance.AddItem(itemData);
        Destroy(gameObject);
    }

    void Update()
    {
        // Rotation logic
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Floating logic
        float floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = initialPosition + new Vector3(0, floatOffset, 0);
    }
}
