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

    [Header("Canvas Reference")]
    public Canvas uiCanvas;  // For Screen Space - Overlay or Screen Space - Camera

    private const float maxIconSize = 24f;

    void Update()
    {
        // Move the cursor icon if it's active
        if (gameObject.activeSelf)
        {
            Vector2 localPoint;
            // For Screen Space - Overlay, pass null as the camera
            RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                null,
                out localPoint
            );
            GetComponent<RectTransform>().anchoredPosition = localPoint;
        }

        // Check for dropping items only if we're holding something
        if (heldItem != null && heldItem.item != null)
        {
            // Left-click => Drop entire stack
            if (Input.GetMouseButtonDown(0))
            {
                // If not over a UI element
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    DropStack();
                }
            }
            // Right-click => Drop one item
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
        WorldItemDropper.DropItem(heldItem, GetDropPosition());
        heldItem = null;
        UpdateCursor();
    }

    private void DropOne()
    {
        if (heldItem.quantity <= 1)
        {
            DropStack();
            return;
        }

        ItemStack single = new ItemStack(heldItem.item, 1);
        WorldItemDropper.DropItem(single, GetDropPosition());
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

            // 1) Clamp icon size if bigger than 24x24
            Vector2 spriteSize = heldItem.item.icon.rect.size;
            float largestDim = Mathf.Max(spriteSize.x, spriteSize.y);
            bool scaledDown = false;
            if (largestDim > maxIconSize)
            {
                float scaleFactor = maxIconSize / largestDim;
                spriteSize *= scaleFactor;
                scaledDown = true;
            }

            // 2) Removed the second clamp that tries to fit the cursor's RectTransform.
            //    This prevents the icon from shrinking again if the cursor rect is smaller.

            // 3) Apply final size
            itemImage.rectTransform.sizeDelta = spriteSize;

            // 4) Optional nudge if scaled
            if (scaledDown)
            {
                itemImage.rectTransform.anchoredPosition = new Vector2(1f, 1f);
            }
            else
            {
                itemImage.rectTransform.anchoredPosition = Vector2.zero;
            }

            // Quantity text
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
