using UnityEngine;
using TMPro;

public class UI_EatableInfoPanel : MonoBehaviour
{
    public GameObject eatableInfoPanel; // Reference to the panel itself
    public TextMeshProUGUI objectNameText; // Reference to the text for the object name
    public TextMeshProUGUI descriptionText; // Reference to the text for the description
    public TextMeshProUGUI itemTypeText; // Reference to the text for the item type
    public TextMeshProUGUI itemWeightText; // Reference to the text for the item type

    private RectTransform panelRectTransform;

    void Start()
    {
        panelRectTransform = eatableInfoPanel.GetComponent<RectTransform>();
        HidePanel(); // Hide the panel at the start
    }

    public void ShowPanel(ItemData itemData, Vector2 slotPosition)
    {
        // Update the text fields with item information
        objectNameText.text = itemData.objectName;
        descriptionText.text = itemData.description;
        itemTypeText.text = itemData.itemType.ToString();
        itemWeightText.text = itemData.weight.ToString();


        // Set the pivot to the bottom-right of the panel (1, 0)
        panelRectTransform.pivot = new Vector2(1, 0);

        // Move the panel to the slot's position
        panelRectTransform.position = slotPosition;

        // Ensure the popup stays within the screen
        ClampToScreen(panelRectTransform);

        // Show the panel
        eatableInfoPanel.SetActive(true);
    }

    private void ClampToScreen(RectTransform panel)
    {
        // Get the screen dimensions
        Vector3[] panelCorners = new Vector3[4];
        panel.GetWorldCorners(panelCorners); // Get the world corners of the panel

        float panelWidth = panelCorners[2].x - panelCorners[0].x;
        float panelHeight = panelCorners[2].y - panelCorners[0].y;

        // Get screen dimensions (considering the screen scaling)
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Get the current panel position
        Vector3 position = panel.position;

        // Clamp the x position to stay within the screen width
        if (position.x + panelWidth > screenWidth) // Right edge out of screen
        {
            position.x = screenWidth - panelWidth;
        }
        if (position.x < 0) // Left edge out of screen
        {
            position.x = 0;
        }

        // Clamp the y position to stay within the screen height
        if (position.y + panelHeight > screenHeight) // Top edge out of screen
        {
            position.y = screenHeight - panelHeight;
        }
        if (position.y < 0) // Bottom edge out of screen
        {
            position.y = 0;
        }

        // Update the panel position
        panel.position = position;
    }

    public void HidePanel()
    {
        // Hide the panel
        eatableInfoPanel.SetActive(false);
    }
}