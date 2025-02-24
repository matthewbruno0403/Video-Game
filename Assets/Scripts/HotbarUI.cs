using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public CursorItem cursorItem;
    public GameObject slotPrefab;
    public Transform slotParent;
    public int slotCount = 10;

    private SlotUI[] hotbarSlots;

    void Start()
    {
        Debug.Log($"[HotbarUI] Start: inventoryManager={inventoryManager}, cursorItem={cursorItem}, " +
                  $"slotPrefab={slotPrefab}, slotParent={slotParent}", this);
        hotbarSlots = new SlotUI[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            // 1) Instantiate
            GameObject slotGO = Instantiate(slotPrefab, slotParent);

            // 2) Get the SlotUI
            SlotUI slotUI = slotGO.GetComponent<SlotUI>();

            // 3) Assign references
            slotUI.slotIndex = i;
            slotUI.inventoryManager = inventoryManager;
            slotUI.cursorItem = cursorItem;             

            // 4) Store in the array
            hotbarSlots[i] = slotUI;
        }

        // Now that each SlotUI has a manager/cursor, we can refresh
        RefreshHotbar();
    }

    public void RefreshHotbar()
    {
        Debug.Log("[HotbarUI] RefreshHotbar called", this);
        
        if (hotbarSlots == null || hotbarSlots.Length == 0) return;

        // Display itemStacks[0..9]
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            int slotIndex = i; // same as i
            if (slotIndex < inventoryManager.itemStacks.Length)
            {
                hotbarSlots[i].SetItem(inventoryManager.itemStacks[slotIndex]);
            }
            else
            {
                hotbarSlots[i].SetItem(null);
            }
        }
    }
}
