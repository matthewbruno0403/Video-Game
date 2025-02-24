using UnityEngine;

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
    // Adding items to inventory
    // ------------------------------------------------
    public bool AddItem(Item newItem, int quantity)
    {
        Debug.Log($"[InventoryManager] AddItem called with item={newItem}, qty={quantity}", this);
        // 1) If item is stackable, fill partial stacks first
        if (newItem.stackable)
        {
            for (int i = 0; i < itemStacks.Length && quantity > 0; i++)
            {
                // if this slot is the same item AND not at max
                if (itemStacks[i] != null && itemStacks[i].item == newItem 
                    && itemStacks[i].quantity < newItem.maxStack)
                {
                    int canAdd = newItem.maxStack - itemStacks[i].quantity;
                    int toAdd = Mathf.Min(canAdd, quantity);
                    itemStacks[i].quantity += toAdd;
                    quantity -= toAdd;
                }
            }
        }

        // 2) If leftover remains (or not stackable), place in next empty slot
        while (quantity > 0)
        {
            int emptyIndex = FindEmptySlot();
            if (emptyIndex == -1)
            {
                Debug.Log("[InventoryManager] Inventory is full!", this);
                return false;
            }

            int amountToPlace = Mathf.Min(quantity, newItem.maxStack);
            itemStacks[emptyIndex] = new ItemStack(newItem, amountToPlace);
            quantity -= amountToPlace;
        }

        // All placed
        RefreshUI();
        return true;
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
                if (itemStacks[i] != null && itemStacks[i].item == newItem 
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
        // 1) Check if we have enough of each ingredient
        foreach (var ing in recipe.ingredients)
        {
            int totalInInventory = CountItem(ing.item);
            if (totalInInventory < ing.quantity)
            {
                Debug.Log("[InventoryManager] Not enough ingredients!", this);
                // Not enough
                return false;
            }
        }

        // 2) Remove them
        foreach (var ing in recipe.ingredients)
        {
            RemoveItem(ing.item, ing.quantity);
        }

        // 3) Add the result
        AddItem(recipe.resultItem, recipe.resultQuantity);
        RefreshUI();
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
            // Check if slot has this item
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

        // Return true if we removed the full 'amount'
        return (remaining <= 0);
    }
}
