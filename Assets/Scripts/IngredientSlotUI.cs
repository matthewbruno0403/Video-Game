using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientSlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI quantityText;

    public void Setup(Sprite sprite, int qty)
    {
        Debug.Log($"[IngredientSlotUI] Setup with sprite={sprite}, qty={qty}, icon={icon}, quantityText={quantityText}", this);
        icon.sprite = sprite;
        quantityText.text = qty.ToString();
    }
}
