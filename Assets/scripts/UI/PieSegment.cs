using UnityEngine;
using UnityEngine.EventSystems;

public class PieSegment : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public string propertyName; 
    [SerializeField] private Vector2 tooltipOffset = new Vector2(20, -20);

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipUI.Instance.Show(propertyName);
        TooltipUI.Instance.UpdatePosition(eventData.position + tooltipOffset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        TooltipUI.Instance.UpdatePosition(eventData.position + tooltipOffset);
    }
}
