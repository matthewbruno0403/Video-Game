using UnityEngine;

public class CraftingUI : MonoBehaviour
{
    public static CraftingUI instance;
    
    public Transform recipeListParent;
    public GameObject recipeSlotPrefab;

    void Awake()
    {
        instance = this;
    }
    
    void OnEnable()
    {
        RefreshRecipeList();
    }
    
    public void RefreshRecipeList()
    {

        foreach (Transform child in recipeListParent)
        {
            Destroy(child.gameObject);
        }

        if (CraftingManager.instance == null || CraftingManager.instance.allRecipes == null)
        {
            return;
        }

        foreach (Recipe recipe in CraftingManager.instance.allRecipes)
        {
            if (CanCraftRecipe(recipe))
            {
                GameObject slotGO = Instantiate(recipeSlotPrefab, recipeListParent);
                RecipeSlotUI slotUI = slotGO.GetComponent<RecipeSlotUI>();
                slotUI.Setup(recipe);
            }
        }
    }
    private bool CanCraftRecipe(Recipe recipe)
    {
        foreach (var ing in recipe.ingredients)
        {
            // Use InventoryManager.CountItem to see how many of the ingredient we have
            int totalInventory = InventoryManager.instance.CountItem(ing.item);
            if (totalInventory < ing.quantity)
            {
                // Not enough of this ingredient, so recipe is not craftable
                return false;
            }
        }
        // We have enough of every ingredient
        return true;
    }
}
