using UnityEngine;
using UnityEngine.EventSystems;

public class UICursorDebugger : MonoBehaviour
{
    [SerializeField] RectTransform canvasRect;

    void Update()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvasRect.GetComponentInParent<Canvas>().worldCamera,
            out localPoint
        );

        Debug.Log($"UI Local Point: {localPoint}");
    }
}
