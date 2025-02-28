using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public CursorItem cursorItem;
    
    [Header("Slot Setup")]
    public GameObject slotPrefab;
    public Transform slotParent;
    public int slotCount = 10;

    [Header("Highlight")]
    public RectTransform highlightRect;
    public int activeSlotIndex = 0;
    
    private SlotUI[] hotbarSlots;

    void Start()
    {
        hotbarSlots = new SlotUI[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            // 1) Instantiate
            GameObject slotGO = Instantiate(slotPrefab, slotParent);
            hotbarSlots[i] = slotGO.GetComponent<SlotUI>();

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
        // Force the UI to rebuild so slots are positioned
        Canvas.ForceUpdateCanvases();
        // Ensure highlight is placed on initial active slot
        SetActiveSlot(0);
    }

    void Update()
    {
        HandleHotbarInput();

        // Press Q to drop 1 item from the active slot
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropOneFromActiveSlot();
        }
    }

    public void RefreshHotbar()
    {
        
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

    private void HandleHotbarInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { SetActiveSlot(0); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { SetActiveSlot(1); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { SetActiveSlot(2); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { SetActiveSlot(3); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { SetActiveSlot(4); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { SetActiveSlot(5); }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { SetActiveSlot(6); }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { SetActiveSlot(7); }
        if (Input.GetKeyDown(KeyCode.Alpha9)) { SetActiveSlot(8); }
        if (Input.GetKeyDown(KeyCode.Alpha0)) { SetActiveSlot(9); }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            int newIndex = activeSlotIndex - 1;
            if (newIndex < 0) newIndex = slotCount - 1;
            SetActiveSlot(newIndex);
        }
        else if (scroll < 0f)
        {
            int newIndex = activeSlotIndex + 1;
            if (newIndex >= slotCount) newIndex = 0;
            SetActiveSlot(newIndex);
        }
    }

    private void SetActiveSlot(int index)
    {
        if (index < 0) index = 0;
        if (index >= slotCount) index = slotCount - 1;

        activeSlotIndex = index;

        // Move highlightRect over chosen slot
        if (highlightRect != null && hotbarSlots != null && hotbarSlots.Length > index)
        {
            RectTransform slotRect = hotbarSlots[index].GetComponent<RectTransform>();
            highlightRect.position = slotRect.position;
        }
    }
    public ItemStack GetActiveSlotItem()
    {
        if (hotbarSlots == null || hotbarSlots.Length == 0) return null;
        int slotIndex = activeSlotIndex;
        return inventoryManager.itemStacks[slotIndex];
    }

    private void DropOneFromActiveSlot()
    {
        // 1) Get the ItemStack in the active slot
        int slotIndex = activeSlotIndex;
        ItemStack stack = inventoryManager.itemStacks[slotIndex];

        // 2) If it’s null or empty, do nothing
        if (stack == null || stack.item == null || stack.quantity <= 0)
            return;

        // 3) Create a new stack of quantity 1 to drop
        ItemStack single = new ItemStack(stack.item, 1);

        // 4) Use WorldItemDropper
        WorldItemDropper.DropItem(single, Vector3.zero); 
        // (We ignore dropPosition since the dropper spawns at the player's position anyway)

        // 5) Decrement the inventory stack by 1
        stack.quantity -= 1;
        if (stack.quantity <= 0)
        {
            inventoryManager.itemStacks[slotIndex] = null;
        }

        // 6) Refresh the UI so you see the updated quantity
        inventoryManager.RefreshUI();
}

}