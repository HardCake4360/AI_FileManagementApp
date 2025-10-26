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

        // y�� �̵� (delta.y�� ���� �巡���ϸ� �����̹Ƿ� ��ȣ ����)
        scrollRect.verticalNormalizedPosition += delta.y / Screen.height;
    }
}
