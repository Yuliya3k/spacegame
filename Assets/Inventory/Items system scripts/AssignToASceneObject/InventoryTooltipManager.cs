using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryTooltipManager : MonoBehaviour
{
    public static InventoryTooltipManager instance; // Singleton instance

    [Header("Tooltip UI Elements")]
    public GameObject tooltipPanel; // Assign the InventoryTooltipPanel GameObject
    public TextMeshProUGUI objectNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI volumeText;
    public TextMeshProUGUI caloriesText;
    public TextMeshProUGUI healthImpactText;
    public TextMeshProUGUI hydrationText;

    [Header("Tooltip Positioning")]
    public Vector2 tooltipOffset = new Vector2(-10, 10); // Offset for top-left positioning

    private RectTransform panelRectTransform;
    private Canvas canvas;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Optionally, make TooltipManager persistent across scenes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (tooltipPanel == null)
        {
            Debug.LogError("Tooltip Panel is not assigned in the InventoryTooltipManager.");
        }

        panelRectTransform = tooltipPanel.GetComponent<RectTransform>();
        canvas = tooltipPanel.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Tooltip panel is not under a Canvas.");
        }

        HideTooltip();
    }

    /// <summary>
    /// Displays the tooltip with the given item data at the specified screen position.
    /// </summary>
    /// <param name="itemData">Data of the item to display.</param>
    /// <param name="screenPosition">Screen position where the tooltip should appear.</param>
    public void ShowTooltip(ItemData itemData, Vector2 screenPosition)
    {
        if (itemData == null)
        {
            Debug.LogError("ShowTooltip called with null itemData.");
            return;
        }

        // Update the text fields with item information
        objectNameText.text = itemData.objectName;
        descriptionText.text = itemData.description;

        // Initialize optional fields to empty and hide them
        weightText.text = "";
        weightText.gameObject.SetActive(false);
        volumeText.text = "";
        volumeText.gameObject.SetActive(false);
        caloriesText.text = "";
        caloriesText.gameObject.SetActive(false);
        healthImpactText.text = "";
        healthImpactText.gameObject.SetActive(false);
        hydrationText.text = "";
        hydrationText.gameObject.SetActive(false);

        // Check if itemData is of type EatableItemData
        if (itemData is EatableItemData eatableData)
        {
            weightText.text = $"{itemData.weight} g";
            weightText.gameObject.SetActive(true);

            volumeText.text = $"{itemData.volume} ml";
            volumeText.gameObject.SetActive(true);

            caloriesText.text = $"{eatableData.calories} kcal";
            caloriesText.gameObject.SetActive(true);

            healthImpactText.text = $"{eatableData.healthImpact}";
            healthImpactText.gameObject.SetActive(true);

            hydrationText.text = $"{eatableData.hydration}";
            hydrationText.gameObject.SetActive(true);
        }

        // Apply the top-left offset
        Vector2 tooltipPosition = screenPosition + tooltipOffset;

        // Convert screen position to canvas local position
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
        Vector2 localPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            tooltipPosition,
            canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null,
            out localPosition))
        {
            // Set the tooltip position
            panelRectTransform.anchoredPosition = localPosition;

            // Clamp the position to prevent tooltip from going out of the screen
            ClampTooltipPosition();
        }
        else
        {
            Debug.LogError("Failed to convert screen position to canvas local position.");
        }

        // Show the tooltip
        tooltipPanel.SetActive(true);
    }

    /// <summary>
    /// Clamps the tooltip position within the canvas bounds to prevent it from going off-screen.
    /// </summary>
    private void ClampTooltipPosition()
    {
        Vector2 clampedPosition = panelRectTransform.anchoredPosition;

        // Get the size of the tooltip
        Vector2 tooltipSize = panelRectTransform.sizeDelta;

        // Get the canvas size
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRectTransform.sizeDelta;

        // Calculate the bounds within which the tooltip can stay
        float halfCanvasWidth = canvasSize.x / 2;
        float halfCanvasHeight = canvasSize.y / 2;

        // Clamp X position
        if (clampedPosition.x - tooltipSize.x < -halfCanvasWidth)
        {
            clampedPosition.x = -halfCanvasWidth + tooltipSize.x;
        }
        else if (clampedPosition.x + tooltipSize.x > halfCanvasWidth)
        {
            clampedPosition.x = halfCanvasWidth - tooltipSize.x;
        }

        // Clamp Y position
        if (clampedPosition.y + tooltipSize.y > halfCanvasHeight)
        {
            clampedPosition.y = halfCanvasHeight - tooltipSize.y;
        }
        else if (clampedPosition.y - tooltipSize.y < -halfCanvasHeight)
        {
            clampedPosition.y = -halfCanvasHeight + tooltipSize.y;
        }

        panelRectTransform.anchoredPosition = clampedPosition;
    }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}
