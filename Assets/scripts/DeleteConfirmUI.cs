using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DeleteConfirmUI : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI noticeString;
    [SerializeField] Button yes;
    [SerializeField] Button no;
    bool isHovering;
    string pathString;

    private void Update()
    {
        if ((Input.anyKeyDown ||
            Input.mouseScrollDelta != Vector2.zero)&&
            !isHovering)
        {
            Destroy(gameObject);
        }
    }

    public void SetUI(string path,GameObject mother)
    {
        noticeString.text = path;
        pathString = path;
        
        yes.onClick.AddListener(() =>
        {
            RAGSearchClient.Instance.RequestDeleteFile(pathString);
            Destroy(mother);
            Destroy(gameObject);
            NotificationManager.Instance.Show("파일을 삭제했습니다");
        });
        no.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}
