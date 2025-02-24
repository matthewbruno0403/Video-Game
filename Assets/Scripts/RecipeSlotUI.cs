using UnityEngine;
using UnityEngine.EventSystems; // for IPointerDownHandler, IPointerUpHandler
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RecipeSlotUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image resultIcon;
    public TextMeshProUGUI resultQuantityText;
    public Transform ingredientParent; 
    public GameObject ingredientSlotPrefab;

    [Header("Crafting Settings")]
    public float craftInterval = 0.2f; // how many seconds between crafts

    private Recipe currentRecipe;
    private bool isCrafting;       // are we currently holding the mouse?
    private Coroutine craftRoutine;

    public void Setup(Recipe recipe)
    {
        Debug.Log($"[RecipeSlotUI] Setup called with recipe={recipe}, resultItem={recipe.resultItem}, " +
                  $"ingredientParent={ingredientParent}, ingredientSlotPrefab={ingredientSlotPrefab}", this);
        
        currentRecipe = recipe;

        // Display the result item
        resultIcon.sprite = recipe.resultItem.icon;
        resultQuantityText.text = (recipe.resultQuantity > 1) ? recipe.resultQuantity.ToString() : "";

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

    // Called when mouse/touch is pressed down on this slot
    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentRecipe == null) return;

        // Start continuous crafting
        isCrafting = true;
        craftRoutine = StartCoroutine(CraftContinuously());
    }

    // Called when mouse/touch is released
    public void OnPointerUp(PointerEventData eventData)
    {
        // Stop continuous crafting
        isCrafting = false;
        if (craftRoutine != null)
        {
            StopCoroutine(craftRoutine);
            craftRoutine = null;
        }
    }

    private IEnumerator CraftContinuously()
    {
        while (isCrafting)
        {
            // Attempt to craft one item
            bool success = InventoryManager.instance.CraftItem(currentRecipe);
            if (!success)
            {
                // No more ingredients or inventory is full
                break;
            }

            // Wait before crafting another
            yield return new WaitForSeconds(craftInterval);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentRecipe == null) return;

        // Show recipe result item name
        string tooltipText = currentRecipe.resultItem.itemName;
        TooltipUI.instance.ShowTooltip(tooltipText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.instance.HideTooltip();
    }
}
