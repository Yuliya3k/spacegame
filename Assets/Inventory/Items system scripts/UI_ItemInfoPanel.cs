using UnityEngine;
using TMPro;

public class UI_ItemInfoPanel : MonoBehaviour
{
    public GameObject itemInfoPanel; // Reference to the panel itself
    public TextMeshProUGUI objectNameText; // Reference to the text for the object name
    public TextMeshProUGUI descriptionText; // Reference to the text for the description
    public TextMeshProUGUI itemTypeText; // Reference to the text for the item type

    private RectTransform panelRectTransform;

    void Start()
    {
        panelRectTransform = itemInfoPanel.GetComponent<RectTransform>();
        HidePanel(); // Hide the panel at the start
    }

    public void ShowPanel(ItemData itemData, Vector2 slotPosition)
    {
        // Update the text fields with item information
        objectNameText.text = itemData.objectName;
        descriptionText.text = itemData.description;
        itemTypeText.text = itemData.itemType.ToString();

        // Set the pivot to the bottom-right of the panel (1, 0)
        panelRectTransform.pivot = new Vector2(1, 0);

        // Move the panel to the top-left of the slot and account for the pivot being bottom-right
        panelRectTransform.position = slotPosition;

        // Show the panel
        itemInfoPanel.SetActive(true);
    }

    public void HidePanel()
    {
        // Hide the panel
        itemInfoPanel.SetActive(false);
    }
}