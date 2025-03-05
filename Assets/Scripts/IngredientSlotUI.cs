using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class IngredientSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI quantityText;

    // Use a lower max icon size for ingredient slots
    private const float maxIngredientIconSize = 8f;

    // Store a reference to the actual item for tooltip purposes
    private Item ingredientItem;
    
    // For group-based ingredients, use this placeholder icon (assign in Inspector)
    public Sprite groupPlaceholderSprite;

    // --- NEW FIELDS FOR CYCLING ---
    private ItemGroup currentGroup;
    private Coroutine cycleRoutine;
    private float cycleInterval = 2f;  // how many seconds per icon

    /// <summary>
    /// Original setup method for single items.
    /// </summary>
    public void Setup(Item item, int qty)
    {
        // Stop any cycling if we were previously set up for a group
        if (cycleRoutine != null)
        {
            StopCoroutine(cycleRoutine);
            cycleRoutine = null;
        }
        currentGroup = null; // no group for single-item

        ingredientItem = item;  // store for tooltip
        icon.sprite = item.icon;
        quantityText.text = qty.ToString();

        ScaleIcon(item.icon);
    }

    /// <summary>
    /// Setup method for group-based ingredients.
    /// Instead of a single item, we have a list of possible items.
    /// </summary>
    public void SetupGroup(ItemGroup group, int qty)
    {
        // Stop any old cycling
        if (cycleRoutine != null)
        {
            StopCoroutine(cycleRoutine);
            cycleRoutine = null;
        }

        currentGroup = group;
        ingredientItem = null;  // no single item for tooltip
        quantityText.text = qty.ToString();

        // If the group is null or empty, just show the placeholder sprite
        if (group == null || group.items == null || group.items.Count == 0)
        {
            icon.sprite = groupPlaceholderSprite;
            ScaleIcon(groupPlaceholderSprite);
            return;
        }
        
        // 1) Immediately show the first item
        Item firstItem = group.items[0];
        if (firstItem != null && firstItem.icon != null)
        {
            icon.sprite = firstItem.icon;
            ScaleIcon(firstItem.icon);
        }
        else
        {
            // fallback to placeholder if first item is missing an icon
            icon.sprite = groupPlaceholderSprite;
            ScaleIcon(groupPlaceholderSprite);
        }

        // 2) If already active, start the cycle
        if (gameObject.activeInHierarchy)
        {
            cycleRoutine = StartCoroutine(CycleGroupIcons());
        }

    }

        private void OnEnable()
        {
            if (currentGroup != null && cycleRoutine == null)
            {
                cycleRoutine = StartCoroutine(CycleGroupIcons());
            }   
        }

        private void OnDisable()
        {
            if (cycleRoutine != null)
            {
                StopCoroutine(cycleRoutine);
                cycleRoutine = null;
            }
        }

    /// <summary>
    /// Coroutine that loops through each item in the group, updating the icon every few seconds.
    /// </summary>
    private IEnumerator CycleGroupIcons()
    {
        int index = 0;
        while (true)
        {
            // Safety checks
            if (currentGroup == null || currentGroup.items == null || currentGroup.items.Count == 0)
            {
                icon.sprite = groupPlaceholderSprite;
                ScaleIcon(groupPlaceholderSprite);
            }
            else
            {
                // Get the next item in the group
                Item cycleItem = currentGroup.items[index];
                if (cycleItem != null && cycleItem.icon != null)
                {
                    icon.sprite = cycleItem.icon;
                    ScaleIcon(cycleItem.icon);
                }
                else
                {
                    // If item or icon is missing, fallback
                    icon.sprite = groupPlaceholderSprite;
                    ScaleIcon(groupPlaceholderSprite);
                }

                // Move to next item index (wrap around)
                index = (index + 1) % currentGroup.items.Count;
            }

            yield return new WaitForSeconds(cycleInterval);
        }
    }

    // Scale the current icon sprite to fit your 8x8 logic
    private void ScaleIcon(Sprite sprite)
    {
        if (sprite == null) return;

        Vector2 spriteSize = sprite.rect.size;
        // Scale down if larger than the max allowed (8x8)
        float largestDim = Mathf.Max(spriteSize.x, spriteSize.y);
        if (largestDim > maxIngredientIconSize)
        {
            float scaleFactor = maxIngredientIconSize / largestDim;
            spriteSize *= scaleFactor;
        }

        // Optional: ensure it fits within the parent's rect
        RectTransform parentRect = icon.transform.parent.GetComponent<RectTransform>();
        if (parentRect != null)
        {
            Vector2 parentSize = parentRect.rect.size;
            if (spriteSize.x > parentSize.x || spriteSize.y > parentSize.y)
            {
                float scaleX = parentSize.x / spriteSize.x;
                float scaleY = parentSize.y / spriteSize.y;
                float scale = Mathf.Min(scaleX, scaleY);
                spriteSize *= scale;
            }
        }

        // Apply the final size to the icon's RectTransform
        icon.rectTransform.sizeDelta = spriteSize;
    }

    // Tooltip functionality
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ingredientItem != null)
        {
            TooltipUI.instance.ShowTooltip(ingredientItem.itemName);
        }
        else
        {
            // For a group, you might show something like the group's name
            if (currentGroup != null && !string.IsNullOrEmpty(currentGroup.groupName))
            {
                TooltipUI.instance.ShowTooltip("Any " + currentGroup.groupName);
            }
            else
            {
                TooltipUI.instance.ShowTooltip("Any Item in Group");
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.instance.HideTooltip();
    }
}
