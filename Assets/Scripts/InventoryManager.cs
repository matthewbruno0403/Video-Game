using UnityEngine;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    
    [Header("UI References")]
    public GameObject hotbarPanel;
    public GameObject fullInventoryPanel;
    public InventoryUI inventoryUI;
    public HotbarUI hotbarUI; // refresh the hotbar too

    [Header("Inventory Data")]
    [HideInInspector]
    public ItemStack[] itemStacks = new ItemStack[30];

    private bool[] tempUsedSlots;

    void Awake()
    {
        instance = this;
        Debug.Log($"[InventoryManager] Awake: hotbarPanel={hotbarPanel}, fullInventoryPanel={fullInventoryPanel}, " +
                  $"inventoryUI={inventoryUI}, hotbarUI={hotbarUI}", this);
        // Initialize itemStacks to null
        for (int i = 0; i < itemStacks.Length; i++)
        {
            itemStacks[i] = null;
        }
    }

    void Start()
    {
        Debug.Log("[InventoryManager] Start called", this);
        // Show hotbar, hide inventory at start
        if (hotbarPanel != null)  hotbarPanel.SetActive(true);
        if (fullInventoryPanel != null) fullInventoryPanel.SetActive(false);
    }

    void Update()
    {
        // Press E to toggle inventory
        if (Input.GetKeyDown(KeyCode.E))
        {
            bool isInventoryOpen = fullInventoryPanel.activeSelf;
            Debug.Log($"[InventoryManager] Toggling inventory. isInventoryOpen={isInventoryOpen}", this);
            if (isInventoryOpen)
            {
                // Closing inventory → show hotbar
                fullInventoryPanel.SetActive(false);
                hotbarPanel.SetActive(true);

                // Hide the tooltip
                TooltipUI.instance.HideTooltip();
            }
            else
            {
                // Opening inventory → hide hotbar
                fullInventoryPanel.SetActive(true);
                hotbarPanel.SetActive(false);

                // Refresh items in the inventory UI
                inventoryUI.RefreshFullInventory();
            }
        }
    }

    // ------------------------------------------------
    // AddItem now returns the slot index used (or -1 if no space)
    // ------------------------------------------------
    public int AddItem(Item newItem, int quantity)
    {
        Debug.Log($"[InventoryManager] AddItem called with item={newItem}, qty={quantity}", this);

        int leftover = quantity;
        int usedSlotIndex = -1;  // Will store the first slot where we place or merge items

        // 1) If item is stackable, fill partial stacks first
        if (newItem.stackable)
        {
            for (int i = 0; i < itemStacks.Length && leftover > 0; i++)
            {
                // If this slot has the same item and is not at max
                if (itemStacks[i] != null 
                    && itemStacks[i].item == newItem 
                    && itemStacks[i].quantity < newItem.maxStack)
                {
                    int canAdd = newItem.maxStack - itemStacks[i].quantity;
                    int toAdd = Mathf.Min(canAdd, leftover);

                    if (toAdd > 0)
                    {
                        itemStacks[i].quantity += toAdd;
                        leftover -= toAdd;
                        // Record the slot we used if we haven't yet
                        if (usedSlotIndex < 0) usedSlotIndex = i;
                    }
                }
            }
        }

        // 2) If leftover remains, place in empty slots
        while (leftover > 0)
        {
            int emptyIndex = FindEmptySlot();
            if (emptyIndex == -1)
            {
                // No more space in inventory
                Debug.Log("[InventoryManager] Inventory is full!", this);
                return -1;
            }

            // Place as many as we can (up to maxStack)
            int amountToPlace = Mathf.Min(leftover, newItem.maxStack);
            itemStacks[emptyIndex] = new ItemStack(newItem, amountToPlace);
            leftover -= amountToPlace;

            // Record the slot we used if we haven't yet
            if (usedSlotIndex < 0) usedSlotIndex = emptyIndex;
        }

        // 3) Everything placed successfully
        RefreshUI();
        Debug.Log($"[InventoryManager] AddItem finished. {newItem.itemName} placed in slot {usedSlotIndex}");
        return usedSlotIndex;
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < itemStacks.Length; i++)
        {
            if (itemStacks[i] == null)
                return i;
        }
        return -1;
    }

    public void RefreshUI()
    {
        Debug.Log("[InventoryManager] RefreshUI called", this);
        // Refresh both full inventory and hotbar
        if (inventoryUI != null) inventoryUI.RefreshFullInventory();
        if (hotbarUI != null) hotbarUI.RefreshHotbar();

        if (CraftingUI.instance != null && CraftingUI.instance.gameObject.activeSelf)
        {
            CraftingUI.instance.RefreshRecipeList();
        }
    }

    public bool HasSpaceFor(Item newItem, int quantity)
    {
        Debug.Log($"[InventoryManager] HasSpaceFor item={newItem}, qty={quantity}", this);
        // Create a local boolean array so we don't permanently change itemStacks
        tempUsedSlots = new bool[itemStacks.Length];

        int leftover = quantity;

        // Fill partial stacks (no changes to itemStacks, just reduce leftover)
        if (newItem.stackable)
        {
            for (int i = 0; i < itemStacks.Length && leftover > 0; i++)
            {
                if (itemStacks[i] != null 
                    && itemStacks[i].item == newItem 
                    && itemStacks[i].quantity < newItem.maxStack)
                {
                    int canAdd = newItem.maxStack - itemStacks[i].quantity;
                    int toAdd = Mathf.Min(canAdd, leftover);
                    leftover -= toAdd;
                }
            }
        }

        // Now place leftover in empty slots
        while (leftover > 0)
        {
            int emptyIndex = FindEmptySlotTemp(); // uses tempUsedSlots
            if (emptyIndex == -1)
            {
                return false;
            }
            int amountToPlace = Mathf.Min(leftover, newItem.maxStack);
            leftover -= amountToPlace;

            // Mark that slot as used
            tempUsedSlots[emptyIndex] = true;
        }

        return true;
    }

    private int FindEmptySlotTemp()
    {
        for (int i = 0; i < itemStacks.Length; i++)
        {
            if (!tempUsedSlots[i] && itemStacks[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    // ------------------------------------------------
    // Crafting logic
    // ------------------------------------------------
    public bool CraftItem(Recipe recipe)
    {
        Debug.Log($"[InventoryManager] CraftItem called with recipe={recipe}, resultItem={recipe.resultItem}", this);
        
        // 1) Prevent infinite duplication if result is in the group
        foreach (var ing in recipe.ingredients)
        {
            if (ing.useGroup && ing.itemGroup != null && ing.itemGroup.items.Contains(recipe.resultItem))
            {
                Debug.LogError($"[InventoryManager] Infinite duplication prevented: The result item '{recipe.resultItem.itemName}' is in the ingredient group '{ing.itemGroup.groupName}'.");
                return false;
            }
        }
        
        // 2) Check if we have enough of each ingredient
        foreach (var ing in recipe.ingredients)
        {
            int totalInInventory = 0;
            if (ing.useGroup)
            {
                totalInInventory = CountItemsInGroup(ing.itemGroup);
            }
            else
            {
                totalInInventory = CountItem(ing.item);
            }
            if (totalInInventory < ing.quantity)
            {
                Debug.Log("[InventoryManager] Not enough ingredients!", this);
                return false;
            }
        }

        // 3) Remove the required items
        foreach (var ing in recipe.ingredients)
        {
            bool removed = false;
            if (ing.useGroup)
            {
                removed = RemoveItemsFromGroup(ing.itemGroup, ing.quantity);
            }
            else
            {
                removed = RemoveItem(ing.item, ing.quantity);
            }
            if (!removed)
            {
                Debug.LogWarning("[InventoryManager] Failed to remove required items for crafting!", this);
                return false;
            }
        }

        // 4) Add the crafted result (now returns the slot index, not bool)
        int usedSlot = AddItem(recipe.resultItem, recipe.resultQuantity);
        if (usedSlot >= 0)
        {
            RefreshUI();

            Debug.Log($"[CraftItem] Crafted {recipe.resultItem.itemName} placed in slot {usedSlot}. (Slot now contains: {itemStacks[usedSlot]?.item?.itemName})");
            HotbarUI hotbarUI = FindObjectOfType<HotbarUI>();
                
            // --- KEY CHANGE: Instead of ForceReequipSlot(...) alone, call SetActiveSlot(...) if it's the same slot.
            if (hotbarUI != null && usedSlot == hotbarUI.activeSlotIndex)
            {
                Debug.Log($"[InventoryManager] CraftItem: usedSlot == activeSlotIndex ({usedSlot}). " +
                        $"Calling SetActiveSlot so it re‐equips immediately.");
                hotbarUI.SetActiveSlot(usedSlot);
            }
        }
        else
        {
            Debug.LogWarning("[InventoryManager] Not enough space for the crafted item!");
        }

        if (usedSlot >= 0 && hotbarUI != null)
        {
            // Only do it if that slot is already your active slot:
            if (usedSlot == hotbarUI.activeSlotIndex)
            {
                StartCoroutine(ForceEquipNextFrame(usedSlot));
            }
        }

        // 5) Done
        return true;
    }


    public int CountItem(Item item)
    {
        int count = 0;
        for (int i = 0; i < itemStacks.Length; i++)
        {
            if (itemStacks[i] != null && itemStacks[i].item == item)
            {
                count += itemStacks[i].quantity;
            }
        }
        return count;
    }

    public bool RemoveItem(Item item, int amount)
    {
        Debug.Log($"[InventoryManager] RemoveItem item={item}, amount={amount}", this);
        int remaining = amount;
        for (int i = 0; i < itemStacks.Length; i++)
        {
            if (itemStacks[i] != null && itemStacks[i].item == item)
            {
                int removeFromSlot = Mathf.Min(remaining, itemStacks[i].quantity);
                itemStacks[i].quantity -= removeFromSlot;
                remaining -= removeFromSlot;
                if (itemStacks[i].quantity <= 0)
                {
                    itemStacks[i] = null;
                }
                if (remaining <= 0) break;
            }
        }
        return (remaining <= 0);
    }

    // New: Count total items in a group.
    public int CountItemsInGroup(ItemGroup group)
    {
        int total = 0;
        for (int i = 0; i < itemStacks.Length; i++)
        {
            if (itemStacks[i] != null && group.items.Contains(itemStacks[i].item))
            {
                total += itemStacks[i].quantity;
            }
        }
        return total;
    }

    // New: Remove items belonging to a group.
    public bool RemoveItemsFromGroup(ItemGroup group, int amount)
    {
        int remaining = amount;
        for (int i = 0; i < itemStacks.Length; i++)
        {
            if (itemStacks[i] != null && group.items.Contains(itemStacks[i].item))
            {
                int removeFromSlot = Mathf.Min(remaining, itemStacks[i].quantity);
                itemStacks[i].quantity -= removeFromSlot;
                remaining -= removeFromSlot;
                if (itemStacks[i].quantity <= 0)
                {
                    itemStacks[i] = null;
                }
                if (remaining <= 0) break;
            }
        }
        return (remaining <= 0);
    }

    private IEnumerator ForceEquipNextFrame(int slotIndex)
    {
        // Wait one frame so any “Equip(null)” calls finish first
        yield return null;
        hotbarUI.SetActiveSlot(slotIndex);
    }
}
