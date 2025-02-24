using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TextMeshProUGUI quantityText;

    // Assigned at runtime by InventoryUI
    public int slotIndex;
    public InventoryManager inventoryManager;
    public CursorItem cursorItem;

    // ------------------------------------------------
    // 1) MOUSE HOVER TOOLTIP LOGIC
    // ------------------------------------------------
    public void OnPointerEnter(PointerEventData eventData)
    {

        if (inventoryManager == null)
        {
            Debug.LogWarning("[SlotUI.OnPointerEnter] inventoryManager is NULL!");
            return;
        }

        Debug.Log($"[SlotUI.OnPointerEnter] inventoryManager.fullInventoryPanel={inventoryManager.fullInventoryPanel}");

        if (inventoryManager.fullInventoryPanel == null)
        {
            Debug.LogWarning("[SlotUI.OnPointerEnter] inventoryManager.fullInventoryPanel is NULL!");
            return;
        }

        // Check if the full inventory panel is active
        if (!inventoryManager.fullInventoryPanel.activeSelf) return;

        // Check if we're holding an item
        Debug.Log($"[SlotUI.OnPointerEnter] CursorItem.heldItem={CursorItem.heldItem}");
        if (CursorItem.heldItem != null) return;

        // Finally, see if this slot has an item
        ItemStack slotStack = inventoryManager.itemStacks[slotIndex];
        Debug.Log($"[SlotUI.OnPointerEnter] slotStack={slotStack}, slotStack?.item={slotStack?.item}, quantity={slotStack?.quantity}");

        if (slotStack != null && slotStack.item != null)
        {
            Debug.Log($"[SlotUI.OnPointerEnter] Showing tooltip for '{slotStack.item.itemName}'");
            TooltipUI.instance.ShowTooltip(slotStack.item.itemName);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.instance.HideTooltip();
    }

    public void SetItem(ItemStack stack)
    {
        if (stack == null || stack.item == null)
        {
            iconImage.enabled = false;
            quantityText.text = "";
        }
        else
        {
            iconImage.enabled = true;
            iconImage.color = Color.white;
            iconImage.sprite = stack.item.icon;
            quantityText.text = (stack.quantity > 1) ? stack.quantity.ToString() : "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        // Hide tooltip as soon as we click
        TooltipUI.instance.HideTooltip();

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightClick();
        }

        inventoryManager.RefreshUI();
        cursorItem.UpdateCursor();

        // If still hovering after slot is placed re-check
        bool isStillOverSlot = RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(),
        Input.mousePosition,
        cursorItem.uiCamera
        );

        if (isStillOverSlot)
        {
            if (CursorItem.heldItem == null)
            {
                ItemStack slotStack = inventoryManager.itemStacks[slotIndex];
                if (slotStack != null && slotStack.item != null)
                {
                    // Show tooltip for item
                    TooltipUI.instance.ShowTooltip(slotStack.item.itemName);
                }
            }
        }
    }

    private void HandleLeftClick()
    {
        ItemStack held = CursorItem.heldItem;
        ItemStack slotStack = inventoryManager.itemStacks[slotIndex];

        // 1) Merge if same item & slot not full
        if (held != null && slotStack != null &&
            held.item == slotStack.item &&
            slotStack.quantity < slotStack.item.maxStack)
        {
            int availableSpace = slotStack.item.maxStack - slotStack.quantity;
            int transferAmount = Mathf.Min(availableSpace, held.quantity);
            slotStack.quantity += transferAmount;
            held.quantity -= transferAmount;

            if (held.quantity <= 0)
                CursorItem.heldItem = null;
        }
        // 2) Swap if slot is empty or different item
        else if (held != null)
        {
            inventoryManager.itemStacks[slotIndex] = held;
            CursorItem.heldItem = slotStack;
        }
        // 3) Pick up if not holding anything
        else if (held == null)
        {
            CursorItem.heldItem = slotStack;
            inventoryManager.itemStacks[slotIndex] = null;
        }
    }

    private void HandleRightClick()
    {
        ItemStack held = CursorItem.heldItem;
        ItemStack slotStack = inventoryManager.itemStacks[slotIndex];

        // A) Not holding anything, but slot has items → pick up half
        if (held == null && slotStack != null)
        {
            int half = Mathf.CeilToInt(slotStack.quantity / 2f);
            CursorItem.heldItem = new ItemStack(slotStack.item, half);

            slotStack.quantity -= half;
            if (slotStack.quantity <= 0)
            {
                inventoryManager.itemStacks[slotIndex] = null;
            }
        }
        // B) Already holding a stack → place 1 item in the clicked slot (if possible)
        else if (held != null)
        {
            // If slot is empty, create a new stack of quantity 1
            if (slotStack == null)
            {
                inventoryManager.itemStacks[slotIndex] = new ItemStack(held.item, 1);
                held.quantity -= 1;
                if (held.quantity <= 0)
                {
                    CursorItem.heldItem = null;
                }
            }
            // If slot has the same item and isn't full, add 1
            else if (slotStack.item == held.item && slotStack.quantity < slotStack.item.maxStack)
            {
                slotStack.quantity += 1;
                held.quantity -= 1;
                if (held.quantity <= 0)
                {
                    CursorItem.heldItem = null;
                }
            }
            // If slot is full or a different item, do nothing on right-click
        }
    }
}
