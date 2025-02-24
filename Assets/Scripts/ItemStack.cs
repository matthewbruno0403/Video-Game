using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
    public Item item;
    public int quantity;

    public ItemStack(Item newItem, int qty)
    {
        item = newItem;
        quantity = qty;
    }
}
