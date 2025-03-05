using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI instance;       // Singleton for easy access

    public TextMeshProUGUI tooltipText;     // The text component for the tooltip

    [Header("Canvas Reference")]
    public Canvas uiCanvas;  // For Screen Space - Overlay or Screen Space - Camera

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

            // Use null for the camera if your Canvas is Screen Space - Overlay
            // or if you have a single camera and want to rely on Overlay behavior
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                null, // Removed uiCamera
                out localPoint
            );

            // Optional offset if desired:
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
