using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RecipeSlotUI : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler
{
    public Image resultIcon;
    public TextMeshProUGUI resultQuantityText;
    public Transform ingredientParent; 
    public GameObject ingredientSlotPrefab;

    [Header("Crafting Settings")]
    public float craftInterval = 10f; // seconds between crafts

    private Recipe currentRecipe;
    private Coroutine craftRoutine;
    private bool isPointerOver = false;

    public void Setup(Recipe recipe)
    {
        currentRecipe = recipe;

        // 1) Assign sprite
        resultIcon.sprite = recipe.resultItem.icon;
        resultQuantityText.text = (recipe.resultQuantity > 1) 
            ? recipe.resultQuantity.ToString() 
            : "";

        // 2) Crisp downscale if bigger than 16×16
        Vector2 spriteSize = recipe.resultItem.icon.rect.size;
        float maxIconSize = 24f;
        float largestDim = Mathf.Max(spriteSize.x, spriteSize.y);
        if (largestDim > maxIconSize)
        {
            float scaleFactor = maxIconSize / largestDim;
            spriteSize *= scaleFactor;
        }

        // 3) Fit to the parent slot if needed
        RectTransform slotRect = resultIcon.transform.parent.GetComponent<RectTransform>();
        if (slotRect != null)
        {
            Vector2 slotSize = slotRect.rect.size;
            if (spriteSize.x > slotSize.x || spriteSize.y > slotSize.y)
            {
                float scaleX = slotSize.x / spriteSize.x;
                float scaleY = slotSize.y / spriteSize.y;
                float scale = Mathf.Min(scaleX, scaleY);
                spriteSize *= scale;
            }
        }

        // 4) Apply final size
        resultIcon.rectTransform.sizeDelta = spriteSize;

        // 5) Clear old ingredient icons
        foreach (Transform child in ingredientParent)
        {
            Destroy(child.gameObject);
        }

        // 6) Create small icons for each ingredient.
        // For each ingredient, check if it's a single item or a group.
        foreach (var ing in recipe.ingredients)
        {
            GameObject ingSlot = Instantiate(ingredientSlotPrefab, ingredientParent);
            IngredientSlotUI ingUI = ingSlot.GetComponent<IngredientSlotUI>();
            if (ing.useGroup)
            {
                ingUI.SetupGroup(ing.itemGroup, ing.quantity);
            }
            else
            {
                ingUI.Setup(ing.item, ing.quantity);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        if (currentRecipe != null && currentRecipe.resultItem != null)
        {
            // Build the tooltip for the result item
            string tooltip = currentRecipe.resultItem.itemName;
            if (!string.IsNullOrEmpty(currentRecipe.resultItem.itemType))
            {
                tooltip += $"\n({currentRecipe.resultItem.itemType})";
            }

            TooltipUI.instance.ShowTooltip(tooltip);
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        TooltipUI.instance.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentRecipe == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                CraftAllPossible();
            }
            else
            {
                InventoryManager.instance.CraftItem(currentRecipe);
            }
        }
    }

    private void Update()
    {
        // Right-click hold for continuous crafting
        if (isPointerOver && Input.GetMouseButton(1))
        {
            if (craftRoutine == null)
            {
                craftRoutine = StartCoroutine(CraftContinuously());
            }
        }
        else
        {
            if (craftRoutine != null)
            {
                StopCoroutine(craftRoutine);
                craftRoutine = null;
            }
        }
    }

    private IEnumerator CraftContinuously()
    {
        while (isPointerOver && Input.GetMouseButton(1))
        {
            bool success = InventoryManager.instance.CraftItem(currentRecipe);
            if (!success) break;
            yield return new WaitForSeconds(craftInterval);
        }
        craftRoutine = null;
    }

    private void CraftAllPossible()
    {
        if (!currentRecipe.resultItem.stackable)
        {
            InventoryManager.instance.CraftItem(currentRecipe);
            return;
        }
        while (InventoryManager.instance.CraftItem(currentRecipe))
        {
            // Keep crafting until out of ingredients or inventory is full
        }
    }
}
