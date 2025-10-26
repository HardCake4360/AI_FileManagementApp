using UnityEngine;

[ExecuteAlways]
public class MatchChildSize : MonoBehaviour
{
    [SerializeField] private Vector2 delta; // 자식보다 얼마나 더 크게 만들지

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

        // 자식의 실제 크기 + delta 적용
        Vector2 targetSize = new Vector2(childRect.rect.width, childRect.rect.height) + delta;
        selfRect.sizeDelta = targetSize;
    }
}
