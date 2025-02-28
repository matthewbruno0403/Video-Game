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
    public float craftInterval = 10f; // seconds between crafts (adjust as needed)

    private Recipe currentRecipe;
    private Coroutine craftRoutine;

    // Track whether the pointer is over this slot
    private bool isPointerOver = false;

    public void Setup(Recipe recipe)
    {
        currentRecipe = recipe;

        // Display the result item
        resultIcon.sprite = recipe.resultItem.icon;
        resultQuantityText.text = (recipe.resultQuantity > 1) 
            ? recipe.resultQuantity.ToString() 
            : "";

        // Clear old ingredient icons
        foreach (Transform child in ingredientParent)
        {
            Destroy(child.gameObject);
        }

        // Create a small icon for each ingredient
        foreach (var ing in recipe.ingredients)
        {
            GameObject ingSlot = Instantiate(ingredientSlotPrefab, ingredientParent);
            IngredientSlotUI ingUI = ingSlot.GetComponent<IngredientSlotUI>();
            ingUI.Setup(ing.item.icon, ing.quantity);
        }
    }

    // -------------------------------
    // Pointer Tracking
    // -------------------------------
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        TooltipUI.instance.ShowTooltip(currentRecipe?.resultItem.itemName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        TooltipUI.instance.HideTooltip();
    }

    // -------------------------------
    // Detect Left-Click (Single Click)
    // -------------------------------
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentRecipe == null) return;

        // Only handle Left Mouse button
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Check if Shift is held for "craft all"
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                CraftAllPossible();
            }
            else
            {
                // Craft exactly one item
                InventoryManager.instance.CraftItem(currentRecipe);
            }
        }
    }

    // -------------------------------
    // Continuous Crafting via Update
    // -------------------------------
    private void Update()
    {
        // If pointer is over this slot and right mouse button is held...
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
            if (!success)
            {
                // Not enough ingredients or inventory full—stop crafting
                break;
            }
            yield return new WaitForSeconds(craftInterval);
        }
        craftRoutine = null;
    }

    private void CraftAllPossible()
    {
        // Optionally, only allow "craft all" for stackable items
        if (!currentRecipe.resultItem.stackable)
        {
            InventoryManager.instance.CraftItem(currentRecipe);
            return;
        }
        while (InventoryManager.instance.CraftItem(currentRecipe))
        {
            // Keep crafting until ingredients run out or inventory is full
        }
    }
}
