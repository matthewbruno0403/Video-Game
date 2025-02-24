using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI instance;       // Singleton for easy access

    public TextMeshProUGUI tooltipText;     // The text component for the tooltip

    [Header("Canvas References")]
    public Canvas uiCanvas;                 // Assign the same "Screen Space - Camera" Canvas
    public Camera uiCamera;                 // The camera used to render that Canvas

    void Awake()
    {
        instance = this;
        HideTooltip();
    }

    void Update()
    {
        // Follow the mouse if visible
        if (gameObject.activeSelf)
        {
            // We'll add a small offset so it's not directly under the mouse
            Vector2 localPoint;
            RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                uiCamera,
                out localPoint
            );

            // Add an offset
            // localPoint += new Vector2(1f, -15f);

            // Move the tooltip
            GetComponent<RectTransform>().anchoredPosition = localPoint;
        }
    }

    public void ShowTooltip(string message)
    {
        gameObject.SetActive(true);
        tooltipText.text = message;
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
