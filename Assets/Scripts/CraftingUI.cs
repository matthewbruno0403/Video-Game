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
        // Clear out any existing UI slots
        foreach (Transform child in recipeListParent)
        {
            Destroy(child.gameObject);
        }

        // Safety checks
        if (CraftingManager.instance == null || 
            CraftingManager.instance.allRecipes == null)
        {
            return;
        }

        // Create a UI slot for each craftable recipe
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

    /// <summary>
    /// Checks if the player has enough items to craft the given recipe.
    /// </summary>
    private bool CanCraftRecipe(Recipe recipe)
    {
        foreach (Ingredient ing in recipe.ingredients)
        {
            int totalInventory = 0;

            if (ing.useGroup)
            {
                // If we require an ItemGroup, sum the inventory counts for all items in that group
                if (ing.itemGroup == null)
                {
                    Debug.LogWarning($"[CraftingUI] Ingredient is set to useGroup but itemGroup is null.");
                    return false;
                }

                foreach (Item groupItem in ing.itemGroup.items)
                {
                    totalInventory += InventoryManager.instance.CountItem(groupItem);
                }
            }
            else
            {
                // If we require a single Item
                if (ing.item == null)
                {
                    Debug.LogWarning($"[CraftingUI] Ingredient is set to use a single item but item is null.");
                    return false;
                }

                totalInventory = InventoryManager.instance.CountItem(ing.item);
            }

            // If totalInventory is below what's needed, we can't craft this recipe
            if (totalInventory < ing.quantity)
            {
                return false;
            }
        }
        // If we reach here, we have enough of every ingredient
        return true;
    }
}
