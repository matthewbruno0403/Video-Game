using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public Recipe[] allRecipes;

    public static CraftingManager instance;

    void Awake()
    {
        instance = this;
        Debug.Log($"[CraftingManager] Awake: allRecipes length={allRecipes?.Length}", this);
    }
}
