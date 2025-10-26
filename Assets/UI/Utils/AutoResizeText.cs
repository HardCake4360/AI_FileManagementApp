using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AutoResizeText : MonoBehaviour
{
    public TMP_Text text;

    void LateUpdate()
    {
        if (text == null) return;

        // 텍스트의 예상 크기를 계산한다냥
        Vector2 preferredSize = new Vector2(
            text.preferredWidth,
            text.preferredHeight
        );

        // RectTransform의 크기를 갱신한다냥
        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta = preferredSize;
    }
}
