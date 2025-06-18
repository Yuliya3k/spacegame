using UnityEngine;
using System.Collections.Generic;

public class UI_CraftList : MonoBehaviour
{
    [SerializeField] private Transform craftSlotParent; // Parent where the slots will be added
    [SerializeField] private GameObject craftSlotButton; // Prefab for each slot

    [SerializeField] private List<ItemDataEquipment> craftEquipment; // List of craftable equipment
    private List<UI_CraftSlot> craftSlots = new List<UI_CraftSlot>(); // Active slots in the UI

    void Start()
    {
        SetupCraftList();
    }

    private void SetupCraftList()
    {
        // Clear existing slots
        foreach (Transform child in craftSlotParent)
        {
            Destroy(child.gameObject);
        }
        craftSlots.Clear();

        // Populate the list with craftable equipment
        foreach (var equipment in craftEquipment)
        {
            GameObject newSlot = Instantiate(craftSlotButton, craftSlotParent);
            newSlot.GetComponent<UI_CraftSlot>().SetupCraftSlot(equipment);
            craftSlots.Add(newSlot.GetComponent<UI_CraftSlot>());
        }
    }
}
