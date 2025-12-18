using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationItem : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] RectTransform rect;
    [SerializeField] TextMeshProUGUI messageText;

    [SerializeField] Button actionButton;
    [SerializeField] TextMeshProUGUI actionLabel;

    [Header("Sizes")]
    [SerializeField] float expandedHeight = 110f;

    [Header("Animation")]
    [SerializeField] float inDuration = 0.25f;
    [SerializeField] float outDuration = 0.20f;
    [SerializeField] float slidePixels = 40f; // 오른쪽 아래에서 살짝 밀고 들어오는 거리

    NotificationData data;

    Coroutine running;

    void Reset()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Setup(NotificationData data)
    {
        this.data = data;

        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!rect) rect = GetComponent<RectTransform>();

        messageText.text = data.message ?? "";

        bool hasAction = data.onClick != null;
        actionButton.gameObject.SetActive(hasAction);

        if (hasAction)
        {
            actionLabel.text = string.IsNullOrWhiteSpace(data.actionLabel) ? "열기" : data.actionLabel;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => data.onClick?.Invoke());
        }

        // 시작 상태
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void PlayIn(Action onDone = null)
    {
        Restart(routine: CoPlayIn(onDone));
    }

    public IEnumerator PlayOutAndThen(Action onDone)
    {
        // 매니저에서 yield return 할 수 있게 IEnumerator 제공
        yield return CoPlayOut(onDone);
    }

    IEnumerator CoPlayIn(Action onDone)
    {
        // 등장: 살짝 오른쪽에서 왼쪽으로 + 페이드 인
        Vector2 end = rect.anchoredPosition;
        Vector2 start = end + new Vector2(slidePixels, 0f);

        rect.anchoredPosition = start;
        canvasGroup.alpha = 0f;

        float t = 0f;
        while (t < inDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = EaseOutCubic(Mathf.Clamp01(t / inDuration));
            rect.anchoredPosition = Vector2.LerpUnclamped(start, end, k);
            canvasGroup.alpha = k;
            yield return null;
        }

        rect.anchoredPosition = end;
        canvasGroup.alpha = 1f;
        onDone?.Invoke();
    }

    IEnumerator CoPlayOut(Action onDone)
    {
        // 퇴장: 살짝 아래/오른쪽으로 빠지며 페이드 아웃 (취향)
        Vector2 start = rect.anchoredPosition;
        Vector2 end = start + new Vector2(slidePixels, -10f);

        float t = 0f;
        while (t < outDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = EaseInCubic(Mathf.Clamp01(t / outDuration));
            rect.anchoredPosition = Vector2.LerpUnclamped(start, end, k);
            canvasGroup.alpha = 1f - k;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        onDone?.Invoke();
    }

    void Restart(IEnumerator routine)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(routine);
    }

    static float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
    static float EaseInCubic(float x) => x * x * x;
}
