using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Group")]
public class ItemGroup : ScriptableObject
{
    [Tooltip("Name or label for the group (e.g. 'Any (Rocks)')")]
    public string groupName;

    [Tooltip("All items that qualify as part of this group")]
    public List<Item> items;
}
