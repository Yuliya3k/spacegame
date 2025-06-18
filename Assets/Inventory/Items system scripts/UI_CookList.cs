using UnityEngine;
using System.Collections.Generic;

public class UI_CookList : MonoBehaviour
{
    [SerializeField] private Transform craftSlotParent; // Parent where the slots will be added
    [SerializeField] private GameObject craftSlotButton; // Prefab for each slot

    [SerializeField] private List<EatableItemData> craftDishes; // List of cookable dishes
    private List<UI_CraftSlot> craftSlots = new List<UI_CraftSlot>(); // Active slots in the UI

    void Start()
    {
        SetupCookList();
    }

    private void SetupCookList()
    {
        // Clear existing slots
        foreach (Transform child in craftSlotParent)
        {
            Destroy(child.gameObject);
        }
        craftSlots.Clear();

        // Populate the list with cookable dishes
        foreach (var dish in craftDishes)
        {
            GameObject newSlot = Instantiate(craftSlotButton, craftSlotParent);
            newSlot.GetComponent<UI_CraftSlot>().SetupCraftSlot(dish);
            craftSlots.Add(newSlot.GetComponent<UI_CraftSlot>());
        }

        // Disable character UI opening
        UIManager.instance.DisableCharacterUIOpening();

        // Also disable the in-game menu
        InGameMenuController.instance.DisableMenuOpening();

    }
}
