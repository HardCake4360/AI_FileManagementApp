using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [SerializeField] private RectTransform rect;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector2 offset = new Vector2(16, -16); // ���콺 ���� delta

    void Awake()
    {
        Instance = this;
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (rect == null) rect = GetComponent<RectTransform>();
        Hide();
    }

    public void Show(string message)
    {
        text.text = message;
        rect.gameObject.SetActive(true);
    }

    public void Hide()
    {
        rect.gameObject.SetActive(false);
    }

    public void UpdatePosition(Vector2 screenPosition)
    {
        // ���콺 ��ġ + offset�� canvas ��ǥ�� ��ȯ
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition + offset,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );
        rect.anchoredPosition = localPoint;
    }
}
