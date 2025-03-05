using UnityEngine;

public class PlayerItemHolder : MonoBehaviour
{
    public SpriteRenderer itemRenderer; 
    private Item currentItem;

    /// <summary>
    /// Called whenever you equip a new item (e.g. via Hotbar).
    /// </summary>
    public void Equip(Item newItem)
    {
        currentItem = newItem;
        Debug.Log($"Equip called with newItem={ (newItem ? newItem.itemName : "null") }," +
              $" stackTrace:\n{System.Environment.StackTrace}");

        // If no item, just clear the sprite.
        if (currentItem == null)
        {
            itemRenderer.sprite = null;
            Debug.Log("[PlayerItemHolder] Equip: Equipping null");
            return;
        }

        // If the item is always visible (like a torch), show it immediately.
        if (currentItem.alwaysVisible)
        {
            itemRenderer.sprite = currentItem.icon;
        }
        else
        {
            // Otherwise, hide it until an "action" occurs (e.g., attack).
            itemRenderer.sprite = null;
        }
        Debug.Log($"[PlayerItemHolder] Equip: Equipping {currentItem.itemName}");
        Debug.Log($"Equip called with newItem={ (newItem ? newItem.itemName : "null") }," +
              $" stackTrace:\n{System.Environment.StackTrace}");
    }

    /// <summary>
    /// Called when an action (attack, mining swing, etc.) starts.
    /// If the item is not alwaysVisible, we show it for the duration of the action.
    /// </summary>
    public void ShowItem()
    {
        // Only show if we actually have an item
        if (currentItem != null)
        {
            itemRenderer.sprite = currentItem.icon;
            Debug.Log($"[PlayerItemHolder] ShowItem: Showing {currentItem.itemName}");
            Debug.Log($"[PlayerItemHolder] ShowItem called: currentItem={(currentItem ? currentItem.itemName : "null")}");
        }
        Debug.Log($"[PlayerItemHolder] ShowItem called: currentItem={(currentItem ? currentItem.itemName : "null")}");
    }

    /// <summary>
    /// Called when the action ends. If the item is not alwaysVisible, hide it again.
    /// </summary>
    public void HideItem()
    {
        if (currentItem != null && !currentItem.alwaysVisible)
        {
            itemRenderer.sprite = null;
            Debug.Log($"[PlayerItemHolder] HideItem: Hiding {currentItem.itemName}");
        }
    }
}
