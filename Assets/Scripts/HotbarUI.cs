// HotbarUI.cs
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
            GameObject slotGO = Instantiate(slotPrefab, slotParent);
            SlotUI slotUI = slotGO.GetComponent<SlotUI>();
            slotUI.slotIndex = i;
            slotUI.inventoryManager = inventoryManager;
            slotUI.cursorItem = cursorItem;             
            hotbarSlots[i] = slotUI;
        }

        RefreshHotbar();
        Canvas.ForceUpdateCanvases();
        SetActiveSlot(0);
    }

    void Update()
    {
        HandleHotbarInput();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropOneFromActiveSlot();
        }
    }

    public void RefreshHotbar()
    {
        if (hotbarSlots == null || hotbarSlots.Length == 0) return;

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            int slotIndex = i;
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

    public void SetActiveSlot(int index)
    {
        if (index < 0) index = 0;
        if (index >= slotCount) index = slotCount - 1;

        activeSlotIndex = index;
        ForceReequipSlot(activeSlotIndex);
    }

    /// <summary>
    /// Forces the hotbar to equip whatever is currently in the given slot,
    /// even if that item just changed.
    /// </summary>
    public void ForceReequipSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotCount) return;

        ItemStack activeStack = inventoryManager.itemStacks[slotIndex];
        PlayerItemHolder holder = FindObjectOfType<PlayerItemHolder>();

        if (activeStack != null && activeStack.item != null)
        {
            Debug.Log($"[HotbarUI] ForceReequipSlot: Slot {slotIndex} contains item: {activeStack.item.itemName}");
            holder.Equip(activeStack.item);
        }
        else
        {
            Debug.Log($"[HotbarUI] ForceReequipSlot: Slot {slotIndex} is empty");
            holder.Equip(null);
        }

        // Move highlightRect over chosen slot
        if (highlightRect != null && hotbarSlots != null && hotbarSlots.Length > slotIndex)
        {
            RectTransform slotRect = hotbarSlots[slotIndex].GetComponent<RectTransform>();
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
        int slotIndex = activeSlotIndex;
        ItemStack stack = inventoryManager.itemStacks[slotIndex];
        if (stack == null || stack.item == null || stack.quantity <= 0)
            return;

        ItemStack single = new ItemStack(stack.item, 1);
        WorldItemDropper.DropItem(single, Vector3.zero);
        stack.quantity -= 1;
        if (stack.quantity <= 0)
        {
            inventoryManager.itemStacks[slotIndex] = null;
        }
        inventoryManager.RefreshUI();
    }
}
