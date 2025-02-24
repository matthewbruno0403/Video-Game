using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventoryManager;
    public CursorItem cursorItem;
    public GameObject slotPrefab;
    public Transform slotParent; // The parent transform with a GridLayoutGroup

    [Header("Settings")]
    public int slotCount = 30;

    private SlotUI[] slotUIArray;

    void Start()
    {
        Debug.Log($"[InventoryUI] Start: inventoryManager={inventoryManager}, cursorItem={cursorItem}, " +
                  $"slotPrefab={slotPrefab}, slotParent={slotParent}, slotCount={slotCount}", this);
        
        // create local array to hold SlotUI references
        slotUIArray = new SlotUI[slotCount];

        // 1) Spawn slotCount copies of the slot prefab under slotParent
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotGo = Instantiate(slotPrefab, slotParent);
            SlotUI slotUI = slotGo.GetComponent<SlotUI>();
            
            slotUI.slotIndex = i;
            slotUI.inventoryManager = inventoryManager;
            slotUI.cursorItem = cursorItem;

            // Store this SlotUI in array
            slotUIArray[i] = slotUI;
        }
        // 3) Refresh them once at startup
        RefreshFullInventory();
    }

    public void RefreshFullInventory()
    {
        Debug.Log("[InventoryUI] RefreshFullInventory called", this);
        
        if (slotUIArray == null || slotUIArray.Length == 0) return;

        // We assume inventoryManager.itemStacks is at least slotCount
        for (int i = 0; i < slotUIArray.Length; i++)
        {
            if (i < inventoryManager.itemStacks.Length)
                slotUIArray[i].SetItem(inventoryManager.itemStacks[i]);
            else
                slotUIArray[i].SetItem(null);
        }
    }
}
