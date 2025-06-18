// MinimapEdgeIndicatorManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class MinimapEdgeIndicatorManager : MonoBehaviour
{
    [System.Serializable]
    public class AreaData
    {
        public Transform areaTransform; // The area's transform (AreaName)
        public Transform areaIconTransform; // The area icon's parent transform (RoomIcon)
        public SpriteRenderer areaBGSpriteRenderer; // Assign in Inspector
        public SpriteRenderer areaIconSpriteRenderer; // Assign in Inspector

        [HideInInspector]
        public GameObject edgeIconInstance; // The edge icon instance
        [HideInInspector]
        public RectTransform edgeIconRectTransform; // Edge icon RectTransform
        [HideInInspector]
        public EdgeIconComponents edgeIconComponents; // Reference to the EdgeIconComponents script
        [HideInInspector]
        public bool isEdgeIconActive; // Whether the edge icon is currently active
    }

    public RectTransform minimapRectTransform; // The RectTransform of the minimap UI element
    public Camera minimapCamera; // The camera rendering the minimap
    public Transform playerTransform; // The player's transform
    public GameObject edgeIconPrefab; // The edge icon prefab (EdgeIcon)
    public float activationDistance = 50f; // Distance within which the icon becomes active

    public List<AreaData> areas = new List<AreaData>(); // List of areas

    [Header("Map UI")]
    public GameObject mapUI; // Assign your Map UI GameObject in the Inspector

    // Input Actions
    private PlayerInputActions inputActions;

    private void Awake()
    {
        // Initialize input actions
        inputActions = new PlayerInputActions();

        // Set up the callback for the "Map" action
        inputActions.UI.Map.performed += ctx => ToggleMapUI();
    }

    private void OnEnable()
    {
        inputActions.UI.Enable();
    }

    private void OnDisable()
    {
        inputActions.UI.Disable();
    }


    void Start()
    {
        // Initialize edge icons for all areas
        foreach (AreaData area in areas)
        {
            if (area.areaBGSpriteRenderer == null || area.areaIconSpriteRenderer == null)
            {
                Debug.LogError("Please assign areaBGSpriteRenderer and areaIconSpriteRenderer in the Inspector for area: " + area.areaIconTransform.name);
                continue;
            }

            // Instantiate edge icon
            GameObject iconInstance = Instantiate(edgeIconPrefab, minimapRectTransform);
            iconInstance.name = "EdgeIcon_" + area.areaIconTransform.name; // Optional: Name the instance for easier identification
            area.edgeIconInstance = iconInstance;
            area.edgeIconRectTransform = iconInstance.GetComponent<RectTransform>();

            // Get the EdgeIconComponents script from the instantiated edge icon
            area.edgeIconComponents = iconInstance.GetComponent<EdgeIconComponents>();

            if (area.edgeIconComponents == null)
            {
                Debug.LogError("EdgeIconComponents script not found on edge icon prefab.");
                continue;
            }

            // Assign sprites to edge icon's Image components
            Sprite areaBGSprite = area.areaBGSpriteRenderer.sprite;
            Sprite areaIconSprite = area.areaIconSpriteRenderer.sprite;

            // Assign sprites to edge icon's Image components
            area.edgeIconComponents.bgImage.sprite = areaBGSprite;
            area.edgeIconComponents.iconImage.sprite = areaIconSprite;

            // Assign colors
            area.edgeIconComponents.bgImage.color = area.areaBGSpriteRenderer.color;
            area.edgeIconComponents.iconImage.color = area.areaIconSpriteRenderer.color;

            //// Assign sizes
            //Vector2 bgSize = Vector2.Scale(areaBGSprite.bounds.size, area.areaBGSpriteRenderer.transform.lossyScale) * 100f;
            //Vector2 iconSize = Vector2.Scale(areaIconSprite.bounds.size, area.areaIconSpriteRenderer.transform.lossyScale) * 100f;

            //area.edgeIconComponents.bgImage.rectTransform.sizeDelta = bgSize;
            //area.edgeIconComponents.iconImage.rectTransform.sizeDelta = iconSize;

            // Initially hide the edge icon
            iconInstance.SetActive(false);
            area.isEdgeIconActive = false;
        }
    }

    void Update()
    {
        foreach (AreaData area in areas)
        {
            UpdateAreaEdgeIcon(area);
        }
    }

    void UpdateAreaEdgeIcon(AreaData area)
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 areaPos = area.areaIconTransform.position; // Use area icon position

        // Calculate the distance between the player and the area
        float distance = Vector3.Distance(playerPos, areaPos);

        // Check if the player has reached the area
        float reachedThreshold = 5f; // Adjust as needed
        if (distance <= reachedThreshold)
        {
            // Player has reached the area
            // Hide the edge icon
            if (area.isEdgeIconActive)
            {
                area.edgeIconInstance.SetActive(false);
                area.isEdgeIconActive = false;
            }
            return;
        }

        // Check if the area is within activation distance
        if (distance > activationDistance)
        {
            // Too far, hide the edge icon
            if (area.isEdgeIconActive)
            {
                area.edgeIconInstance.SetActive(false);
                area.isEdgeIconActive = false;
            }
            return;
        }

        // Area is close enough, proceed
        if (!area.isEdgeIconActive)
        {
            area.edgeIconInstance.SetActive(true);
            area.isEdgeIconActive = true;
        }

        // Convert positions to minimap coordinates
        Vector3 playerViewportPos = minimapCamera.WorldToViewportPoint(playerPos);
        Vector3 areaViewportPos = minimapCamera.WorldToViewportPoint(areaPos);

        // Check if the area is within the minimap view
        bool areaInView = areaViewportPos.z > 0 &&
                          areaViewportPos.x >= 0 && areaViewportPos.x <= 1 &&
                          areaViewportPos.y >= 0 && areaViewportPos.y <= 1;

        // Convert viewport positions to minimap local positions
        Vector2 minimapSize = minimapRectTransform.sizeDelta;
        Vector2 playerMinimapPos = new Vector2(
            (playerViewportPos.x - 0.5f) * minimapSize.x,
            (playerViewportPos.y - 0.5f) * minimapSize.y
        );
        Vector2 areaMinimapPos = new Vector2(
            (areaViewportPos.x - 0.5f) * minimapSize.x,
            (areaViewportPos.y - 0.5f) * minimapSize.y
        );

        // Synchronize rotation with area icon
        Quaternion areaRotation = area.areaIconTransform.rotation;
        area.edgeIconRectTransform.localEulerAngles = new Vector3(0, 0, -areaRotation.eulerAngles.z);

        // Position the edge icon
        if (areaInView)
        {
            // Position edge icon at the area's position
            area.edgeIconRectTransform.anchoredPosition = areaMinimapPos;
        }
        else
        {
            // Position edge icon along the line between player and area
            Vector2 direction = (areaMinimapPos - playerMinimapPos).normalized;
            float maxDistance = minimapSize.magnitude / 2f; // Adjust as needed
            Vector2 edgeIconPos = playerMinimapPos + direction * maxDistance;

            // Clamp the position within the minimap
            edgeIconPos = ClampToMinimapBounds(edgeIconPos, minimapSize);

            area.edgeIconRectTransform.anchoredPosition = edgeIconPos;
        }

        // Hide the edge icon only when the area icon is fully visible
        if (areaInView)
        {
            area.edgeIconInstance.SetActive(false);
            area.isEdgeIconActive = false;
        }
    }

    Vector2 ClampToMinimapBounds(Vector2 position, Vector2 minimapSize)
    {
        Vector2 halfSize = minimapSize / 2f;
        position.x = Mathf.Clamp(position.x, -halfSize.x, halfSize.x);
        position.y = Mathf.Clamp(position.y, -halfSize.y, halfSize.y);
        return position;
    }

    private void ToggleMapUI()
    {
        if (mapUI != null)
        {
            bool isActive = mapUI.activeSelf;
            mapUI.SetActive(!isActive);

            //if (isActive)
            //{
            //    // Map was active, now closing
            //    // Re-enable player controls if you disabled them
            //    PlayerControllerCode.instance.EnablePlayerControl();
            //    Cursor.visible = false;
            //    Cursor.lockState = CursorLockMode.Locked;
            //}
            //else
            //{
            //    // Map was inactive, now opening
            //    // Disable player controls if desired
            //    PlayerControllerCode.instance.DisablePlayerControl();
            //    Cursor.visible = true;
            //    Cursor.lockState = CursorLockMode.None;
            //}
        }
        else
        {
            Debug.LogWarning("Map UI is not assigned in the MinimapEdgeIndicatorManager.");
        }
    }


}
