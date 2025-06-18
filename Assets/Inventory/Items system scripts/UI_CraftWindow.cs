using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CraftWindow : MonoBehaviour
{
    [Header("Item Info")]
    [SerializeField] private TextMeshProUGUI objectName; // Name of the item
    [SerializeField] private TextMeshProUGUI itemDescription; // Description of the item
    [SerializeField] private Image itemIcon; // Icon of the item

    [Header("Resources")]
    [SerializeField] private Transform materialSlotParent; // Parent object to hold material slots
    [SerializeField] private GameObject materialSlotPrefab; // Prefab for material/ingredient slots

    [Header("Craft Button")]
    [SerializeField] private Button craftButton; // Button to craft the item


    [Header("Progress Bar")]
    [SerializeField] private Slider progressBar; // Assign via inspector

    private CanvasGroup progressBarCanvasGroup;


    private List<GameObject> currentMaterialSlots = new List<GameObject>(); // List to store current material slots




    // Setup for Equipment
    public void SetupCraftWindow(ItemDataEquipment _data)
    {
        // Clear and setup the UI for the selected equipment
        SetupCraftUI(_data.craftingMaterials, _data.icon, _data.objectName, _data.description);

        //NEW
        craftButton.onClick.AddListener(() =>
        {
            PlayerInventory.instance.CraftItem(_data);
        });


        //craftButton.onClick.AddListener(() =>
        //{
        //    if (PlayerInventory.instance.CanCraft(_data, _data.craftingMaterials))
        //    {
        //        Debug.Log($"Successfully crafted: {_data.objectName}");
        //    }
        //    else
        //    {
        //        Debug.Log("Failed to craft, not enough resources.");
        //    }
        //});
    }

    // Setup for Dishes
    public void SetupCraftWindow(EatableItemData _data)
    {
        // Clear and setup the UI for the selected dish
        SetupCraftUI(_data.craftingMaterials, _data.icon, _data.objectName, _data.description);

        //NEW
        craftButton.onClick.AddListener(() =>
        {
            PlayerInventory.instance.CookDish(_data);
        });

        //craftButton.onClick.AddListener(() =>
        //{
        //    if (PlayerInventory.instance.CanCraftDish(_data, _data.craftingMaterials))
        //    {
        //        Debug.Log($"Successfully cooked: {_data.objectName}");
        //    }
        //    else
        //    {
        //        Debug.Log("Failed to cook, not enough ingredients.");
        //    }
        //});
    }

    // Common method to setup the crafting UI for both Equipment and Dishes
    private void SetupCraftUI(List<InventoryItem> materials, Sprite icon, string name, string description)
    {
        // Clear previous listeners and material slots
        craftButton.onClick.RemoveAllListeners();
        ClearMaterialSlots();

        // Setup the item icon, name, and description
        itemIcon.sprite = icon;
        objectName.text = name;
        itemDescription.text = description;

        // Instantiate and setup material slots
        for (int i = 0; i < materials.Count; i++)
        {
            // Create a new material slot
            GameObject newSlot = Instantiate(materialSlotPrefab, materialSlotParent);

            // Get the references for MaterialSlot UI elements
            Image materialIcon = newSlot.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI materialAmountText = newSlot.transform.Find("MaterialStackAmount").GetComponent<TextMeshProUGUI>();

            // Assign the material data (icon and stack size)
            materialIcon.sprite = materials[i].data.icon;
            materialAmountText.text = materials[i].stackSize.ToString();

            // Store the instantiated slot for cleanup later
            currentMaterialSlots.Add(newSlot);
        }
    }

    // Clears all the material slots before setting new ones
    private void ClearMaterialSlots()
    {
        // Destroy each material slot in the list
        foreach (GameObject slot in currentMaterialSlots)
        {
            Destroy(slot);
        }
        currentMaterialSlots.Clear(); // Clear the list after destroying all slots
    }


    public void SetCraftButtonState(bool interactable)
    {
        if (craftButton != null)
        {
            craftButton.interactable = interactable;
        }
    }


    private void Awake()
    {
        // Initialize the progress bar canvas group
        if (progressBar != null)
        {
            progressBarCanvasGroup = progressBar.GetComponent<CanvasGroup>();
            if (progressBarCanvasGroup == null)
            {
                progressBarCanvasGroup = progressBar.gameObject.AddComponent<CanvasGroup>();
            }
            progressBarCanvasGroup.alpha = 0f;
            progressBar.gameObject.SetActive(false);
        }
    }

    public void ShowProgressBar()
    {
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = 0f;
            StartCoroutine(FadeCanvasGroup(progressBarCanvasGroup, 0f, 1f, 0.5f)); // Fade in over 0.5 seconds
        }
    }

    public void HideProgressBar()
    {
        if (progressBar != null)
        {
            StartCoroutine(FadeCanvasGroup(progressBarCanvasGroup, 1f, 0f, 0.5f, () =>
            {
                progressBar.gameObject.SetActive(false);
            })); // Fade out over 0.5 seconds
        }
    }

    public void UpdateProgressBar(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        float elapsedTime = 0f;
        canvasGroup.alpha = startAlpha;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
        if (onComplete != null)
        {
            onComplete();
        }
    }




}
