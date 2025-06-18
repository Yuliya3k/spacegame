using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UI_ItemSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] public Image itemImage;  // Changed to public for access
    [SerializeField] protected TextMeshProUGUI itemText;  // Text showing the amount of stacked items

    protected UIManager ui;
    public InventoryItem item;  // Reference to the item in this slot

    private bool isTooltipVisible = false;
    private Coroutine tooltipCoroutine;
    public float tooltipDelay = 0.5f; // Delay in seconds before showing the tooltip

    private bool isInteractable = true; // To control interaction

    protected virtual void Start()
    {
        ui = UIManager.instance;

        if (ui == null)
        {
            Debug.LogError("UIManager is not found in the scene.");
        }

        // Ensure InventoryTooltipManager is assigned
        if (InventoryTooltipManager.instance == null)
        {
            Debug.LogError("InventoryTooltipManager instance is not set in the scene.");
        }
    }

    public virtual void SetupSlot(InventoryItem _item, bool _isContainerSlot)
    {
        item = _item;
        UpdateSlot(item);
    }

    public void UpdateSlot(InventoryItem _newItem)
    {
        item = _newItem;

        if (itemImage != null)
        {
            itemImage.color = item != null ? Color.white : Color.clear;
            itemImage.sprite = item != null ? item.data.icon : null;
        }

        if (itemText != null)
        {
            itemText.text = item != null && item.stackSize > 1 ? item.stackSize.ToString() : "";
        }
    }

    public void CleanUpSlot()
    {
        item = null;

        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.color = Color.clear;
        }

        if (itemText != null)
        {
            itemText.text = "";
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable)
        {
            Debug.Log("This slot is currently inactive.");
            return;
        }

        if (item == null || item.data == null) return;

        //if (Input.GetKey(KeyCode.LeftControl))
        //{
        //    PlayerInventory.instance.RemoveItem(item.data);
        //    return;
        //}

        if (item.data.itemType == ItemType.Equipment)
        {
            PlayerInventory.instance.EquipItem(item.data);
        }
        else if (item.data.itemType == ItemType.Eatable)
        {
            PlayerInventory.instance.EatItem(item.data);
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null && item.data != null && !isTooltipVisible && InventoryTooltipManager.instance != null)
        {
            tooltipCoroutine = StartCoroutine(ShowTooltipAfterDelay(eventData.position));
        }
    }

    private IEnumerator ShowTooltipAfterDelay(Vector2 screenPosition)
    {
        yield return new WaitForSeconds(tooltipDelay);
        isTooltipVisible = true;
        InventoryTooltipManager.instance.ShowTooltip(item.data, screenPosition);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
            tooltipCoroutine = null;
        }

        if (isTooltipVisible && InventoryTooltipManager.instance != null)
        {
            isTooltipVisible = false;
            InventoryTooltipManager.instance.HideTooltip();
        }
    }

    public void SetSlotState(bool interactable, Color iconColor)
    {
        isInteractable = interactable;

        if (itemImage != null)
        {
            itemImage.color = iconColor;
        }
    }
}
