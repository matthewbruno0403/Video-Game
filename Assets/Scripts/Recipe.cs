using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class Recipe : ScriptableObject
{
    public Item resultItem;
    public int resultQuantity = 1;

    // Ingredients needed
    public Ingredient[] ingredients;
}

[System.Serializable]
public class Ingredient
{
    public Item item;
    public int quantity;
}
