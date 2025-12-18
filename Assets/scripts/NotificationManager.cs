using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("Containers")]
    [SerializeField] RectTransform popupStack;        // PopupStackContainer

    [Header("Prefab")]
    [SerializeField] NotificationItem itemPrefab;

    [Header("Policy")]
    [SerializeField] int maxVisible = 3;

    readonly Queue<NotificationData> queue = new();
    readonly List<NotificationItem> visible = new();

    bool trayOpen = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Push(NotificationData data)
    {
        queue.Enqueue(data);
        tryDequeueToVisible();
    }

    public void Show(string message, float duration = 2f)
    {
        Push(new NotificationData
        {
            message = message,
            duration = duration,
            onClick = null,
            actionLabel = null
        });
    }

    public void Show(
        string message,
        string actionLabel,
        Action onClick,
        float duration = 3f
    )
    {
        Push(new NotificationData
        {
            message = message,
            duration = duration,
            actionLabel = actionLabel,
            onClick = onClick
        });
    }

    public void SpawnPopup(NotificationData data)
    {
        var item = Instantiate(itemPrefab, popupStack);
        item.Setup(data);

        ForceLayout(popupStack);//레이아웃 업데이트

        visible.Add(item);

        item.PlayIn(() =>
        {
            StartCoroutine(AutoExpire(item, data));
        });
    }

    void tryDequeueToVisible()
    {
        while (visible.Count < maxVisible && queue.Count > 0)
        {
            var data = queue.Dequeue();
            SpawnPopup(data);
        }
    }

    IEnumerator AutoExpire(NotificationItem item, NotificationData data)
    {
        yield return new WaitForSeconds(data.duration);
        yield return item.PlayOutAndThen(() => destroyVisible(item));
    }

    void destroyVisible(NotificationItem item)
    {
        visible.Remove(item);
        Destroy(item.gameObject);
        tryDequeueToVisible();
    }

    void ForceLayout(RectTransform root)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
    }

}

[Serializable]
public class NotificationData
{
    public string message;
    public float duration;
    public string actionLabel;
    public Action onClick;
}
