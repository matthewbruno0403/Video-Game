using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CursorItem : MonoBehaviour
{
    public static ItemStack heldItem;

    [Header("Cursor UI")]
    public Image itemImage;
    public TextMeshProUGUI quantityText;

    [Header("Canvas References")]
    public Canvas uiCanvas;        // Assign your "Screen Space - Camera" Canvas here
    public Camera uiCamera;        // Assign the Camera that renders this Canvas

    void Update()
    {
        // Move the cursor icon if it's active (existing code)
        if (gameObject.activeSelf)
        {
            Vector2 localPoint;
            RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                uiCamera,
                out localPoint
            );
            GetComponent<RectTransform>().anchoredPosition = localPoint;
        }

        // Check for dropping items only if we're holding something
        if (heldItem != null && heldItem.item != null)
        {
            // Left-click (mouse button 0) => Drop entire stack
            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    DropStack();
                }
            }
            // Right-click (mouse button 1) => Drop one item (or entire stack if only 1 left)
            else if (Input.GetMouseButtonDown(1))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    DropOne();
                }
            }
        }
    }

    private void DropStack()
    {
        // Example: spawn the entire stack in the world
        WorldItemDropper.DropItem(heldItem, GetDropPosition());
        // Clear the cursor
        heldItem = null;
        UpdateCursor();
    }

    private void DropOne()
    {
        // If we only have 1 in the stack, drop the entire stack
        if (heldItem.quantity <= 1)
        {
            DropStack();
            return;
        }

        // Otherwise, drop just 1 item
        ItemStack single = new ItemStack(heldItem.item, 1);
        WorldItemDropper.DropItem(single, GetDropPosition());

        // Decrement our held stack
        heldItem.quantity -= 1;
        UpdateCursor();
    }

    private Vector3 GetDropPosition()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        return worldPos;
    }

    public void UpdateCursor()
    {
        if (heldItem != null && heldItem.item != null)
        {
            itemImage.sprite = heldItem.item.icon;
            itemImage.enabled = true;
            if (heldItem.quantity > 1)
            {
                quantityText.text = heldItem.quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.text = "";
                quantityText.enabled = true;
            }
        }
        else
        {
            itemImage.enabled = false;
            quantityText.text = "";
            quantityText.enabled = false;
        }
    }
}
