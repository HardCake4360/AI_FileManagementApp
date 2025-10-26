using UnityEngine;

[ExecuteAlways]
public class MatchChildSize : MonoBehaviour
{
    [SerializeField] private Vector2 delta; // �ڽĺ��� �󸶳� �� ũ�� ������

    private void OnValidate()
    {
        ApplySize();
    }

    private void Update()
    {
        ApplySize();
    }

    private void ApplySize()
    {
        if (transform.childCount == 0)
            return;

        RectTransform childRect = transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform selfRect = GetComponent<RectTransform>();

        if (childRect == null || selfRect == null)
            return;

        // �ڽ��� ���� ũ�� + delta ����
        Vector2 targetSize = new Vector2(childRect.rect.width, childRect.rect.height) + delta;
        selfRect.sizeDelta = targetSize;
    }
}
