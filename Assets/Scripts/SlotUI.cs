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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventoryManager == null) return;
        if (inventoryManager.fullInventoryPanel == null) return;

        // Only show tooltip if the full inventory panel is open
        if (!inventoryManager.fullInventoryPanel.activeSelf) return;
        if (CursorItem.heldItem != null) return; // if we're holding an item, skip

        ItemStack slotStack = inventoryManager.itemStacks[slotIndex];
        if (slotStack != null && slotStack.item != null)
        {
            string tooltipText = slotStack.item.itemName;
            if (!string.IsNullOrEmpty(slotStack.item.itemType))
            {
                tooltipText += $"\n{slotStack.item.itemType}";
            }
            TooltipUI.instance.ShowTooltip(tooltipText);
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
            return;
        }

        iconImage.enabled = true;
        iconImage.color = Color.white;
        iconImage.sprite = stack.item.icon;

        // 1) Clamp icon size if bigger than 24x24
        Vector2 spriteSize = stack.item.icon.rect.size; 
        float maxIconSize = 24f;
        float largestDim = Mathf.Max(spriteSize.x, spriteSize.y);
        bool scaledDown = false;
        if (largestDim > maxIconSize)
        {
            float scaleFactor = maxIconSize / largestDim;
            spriteSize *= scaleFactor; // e.g. (32×32) → (24×24)
            scaledDown = true;
        }

        // 2) Ensure it fits the slot's RectTransform
        RectTransform slotRect = iconImage.transform.parent.GetComponent<RectTransform>();
        if (slotRect != null)
        {
            Vector2 slotSize = slotRect.rect.size; // e.g. 25×25
            if (spriteSize.x > slotSize.x || spriteSize.y > slotSize.y)
            {
                float scaleX = slotSize.x / spriteSize.x;
                float scaleY = slotSize.y / spriteSize.y;
                float scale = Mathf.Min(scaleX, scaleY);
                spriteSize *= scale;
            }
        }

        // 3) Apply final size
        iconImage.rectTransform.sizeDelta = spriteSize;

        // 4) If scaled down, optionally nudge it
        if (scaledDown)
        {
            iconImage.rectTransform.anchoredPosition = new Vector2(1f, 1f);
        }
        else
        {
            iconImage.rectTransform.anchoredPosition = Vector2.zero;
        }

        // 5) Quantity
        quantityText.text = (stack.quantity > 1) ? stack.quantity.ToString() : "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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

        // If still hovering, re-check tooltip
        bool isStillOverSlot = RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(),
            Input.mousePosition,
            null // no dedicated camera, works fine in Screen Space - Overlay
        );

        if (isStillOverSlot)
        {
            if (CursorItem.heldItem == null)
            {
                ItemStack slotStack = inventoryManager.itemStacks[slotIndex];
                if (slotStack != null && slotStack.item != null)
                {
                    TooltipUI.instance.ShowTooltip(slotStack.item.itemName);
                }
            }
        }
    }

    private void HandleLeftClick()
    {
        ItemStack held = CursorItem.heldItem;
        ItemStack slotStack = inventoryManager.itemStacks[slotIndex];

        // Merge if same item & slot not full
        if (held != null && slotStack != null &&
            held.item == slotStack.item &&
            slotStack.quantity < slotStack.item.maxStack)
        {
            int availableSpace = slotStack.item.maxStack - slotStack.quantity;
            int transferAmount = Mathf.Min(availableSpace, held.quantity);
            slotStack.quantity += transferAmount;
            held.quantity -= transferAmount;
            if (held.quantity <= 0) CursorItem.heldItem = null;
        }
        // Swap if slot is empty or different item
        else if (held != null)
        {
            inventoryManager.itemStacks[slotIndex] = held;
            CursorItem.heldItem = slotStack;
        }
        // Pick up if not holding anything
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
            if (slotStack.quantity <= 0) inventoryManager.itemStacks[slotIndex] = null;
        }
        // B) Already holding a stack → place 1 item in the clicked slot (if possible)
        else if (held != null)
        {
            if (slotStack == null)
            {
                inventoryManager.itemStacks[slotIndex] = new ItemStack(held.item, 1);
                held.quantity -= 1;
                if (held.quantity <= 0)
                {
                    CursorItem.heldItem = null;
                }
            }
            else if (slotStack.item == held.item && slotStack.quantity < slotStack.item.maxStack)
            {
                slotStack.quantity += 1;
                held.quantity -= 1;
                if (held.quantity <= 0)
                {
                    CursorItem.heldItem = null;
                }
            }
        }
    }
}
