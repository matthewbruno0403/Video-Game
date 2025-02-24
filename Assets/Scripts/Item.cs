using UnityEngine;

[CreateAssetMenu(menuName = "Items/New Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;         // The 16x16 or bigger sprite
    public bool stackable;      // Can it stack?
    public int maxStack = 99;   // Default stack size
    // ... add more fields later (e.g. item type, etc.)

}
