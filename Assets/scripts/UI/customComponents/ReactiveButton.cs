using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ReactiveButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] bool mindSelected;
    [SerializeField] bool react;

    Image image;
    public Color SelectedColor;
    public Color DeselectedColor;

    public bool isSelected;
    Vector3 originalScale;
    RectTransform rect;

    [SerializeField] Vector3 targetScale;
    [SerializeField] float time = 0.2f;

    // 이징 강도 (1이면 기본, 값이 높을수록 더 강한 가속)
    [SerializeField, Range(0.5f, 5f)] float easeStrength = 1.5f;

    public UnityEvent OnClick;
    public UnityEvent OnDeselect;
    public UnityEvent OnHover;
    
    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Start()
    {
        isSelected = false;
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!react) return;
        StartCoroutine(ScaleCoroutine(targetScale, time));
        OnHover?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!react) return;
        if (isSelected && mindSelected) return;
        StartCoroutine(ScaleCoroutine(originalScale, time));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnClick?.Invoke();
        StartCoroutine(ScaleCoroutine(originalScale, time));
        isSelected = true;
        SetColor();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartCoroutine(ScaleCoroutine(targetScale, time));
    }

    public void Deselect()
    {
        isSelected = false;
        OnDeselect?.Invoke();
        StartCoroutine(ScaleCoroutine(originalScale, time));
    }

    private IEnumerator ScaleCoroutine(Vector3 scale, float duration)
    {
        Vector3 startScale = rect.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            t = EaseInOutCustom(t, easeStrength);

            rect.localScale = Vector3.Lerp(startScale, scale, t);
            yield return null;
        }

        rect.localScale = scale;
    }

    // EaseInOut: easeStrength로 가속도 조절
    private float EaseInOutCustom(float t, float strength)
    {
        // strength가 1보다 크면 빠르게 가속 후 천천히 감속
        // strength가 1보다 작으면 천천히 시작 후 빠르게 끝남
        if (t < 0.5f)
            return 0.5f * Mathf.Pow(2f * t, strength);
        else
            return 1f - 0.5f * Mathf.Pow(2f * (1f - t), strength);
    }

    public void SetColor()
    {
        if (isSelected)
        {
            image.color = SelectedColor;
        }
        else
        {
            image.color = DeselectedColor;
        }
    }
    public void Select()
    {
        OnClick?.Invoke();
        StartCoroutine(ScaleCoroutine(originalScale, time));
        isSelected = true;
        SetColor();
    }
}
