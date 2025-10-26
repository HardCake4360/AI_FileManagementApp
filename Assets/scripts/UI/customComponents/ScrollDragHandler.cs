using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private ScrollRect scrollRect;

    private Vector2 lastDragPosition;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastDragPosition;
        lastDragPosition = eventData.position;

        // y축 이동 (delta.y가 위로 드래그하면 음수이므로 부호 반전)
        scrollRect.verticalNormalizedPosition += delta.y / Screen.height;
    }
}
