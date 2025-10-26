using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AutoResizeText : MonoBehaviour
{
    public TMP_Text text;

    void LateUpdate()
    {
        if (text == null) return;

        // �ؽ�Ʈ�� ���� ũ�⸦ ����Ѵٳ�
        Vector2 preferredSize = new Vector2(
            text.preferredWidth,
            text.preferredHeight
        );

        // RectTransform�� ũ�⸦ �����Ѵٳ�
        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta = preferredSize;
    }
}
