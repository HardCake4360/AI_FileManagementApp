using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PieSegmentData
{
    public float value = 1f;
    public Color color = Color.white;
    public string label; // ���û���
}

public class PieChart : MonoBehaviour
{
    [Header("Data")]
    public List<PieSegmentData> segments = new List<PieSegmentData>();

    [Header("Appearance")]
    public Sprite circleSprite;            // ���� ��������Ʈ
    public Vector2 size = new Vector2(256, 256);
    [Range(0f, 20f)] public float gapDegrees = 0f; // ���� ���� ƴ
    public bool clockwise = true;
    public float startAngleDegrees = 0f;  // 0��=������, 90��=���� ����

    [Header("Donut (ring)")]
    public bool donut = false;
    [Range(0f, 0.9f)] public float innerHoleRatio = 0.5f; // ���� ũ�� ��� ���� ���� ����

    [Header("Animation")]
    public float buildDuration = 0.6f;     // ��ü ���� �ִϸ��̼� �ð�
    [Range(0.5f, 5f)] public float easeStrength = 1.6f; // ���ӵ�(����)

    [Header("Optional Prefab")]
    public Image segmentPrefab; // ������ ������(Image). ����θ� �ڵ�� ����
    public string[] names;

    private readonly List<Image> _segmentImages = new List<Image>();
    private RectTransform _rt;
    private Coroutine _animCo;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        if (_rt == null) _rt = gameObject.AddComponent<RectTransform>();
        _rt.sizeDelta = size;
    }

    private void Start()
    {
        AnimateBuild();
    }

    [ContextMenu("Rebuild Now")]
    public void RebuildNow()
    {
        if (_animCo != null) StopCoroutine(_animCo);
        ClearSegments();
        BuildImmediate();
        if (donut) EnsureDonut();
    }

    [ContextMenu("Animate Build")]
    public void AnimateBuild()
    {
        if (_animCo != null) StopCoroutine(_animCo);
        ClearSegments();
        BuildImmediate(initialFillZero: true);
        if (donut) EnsureDonut();
        _animCo = StartCoroutine(AnimateSegments(buildDuration));
    }

    // --- core ---
    void BuildImmediate(bool initialFillZero = false)
    {
        float total = 0f;
        foreach (var s in segments) total += Mathf.Max(0f, s.value);
        if (total <= 0f) return;

        float dir = clockwise ? -1f : 1f; // UI�� Zȸ���� �ð������ ����
        float cursorDeg = startAngleDegrees;

        int i = 0;
        foreach (var s in segments)
        {
            float portion = Mathf.Max(0f, s.value) / total;
            float angle = portion * 360f;

            // ƴ(gap) ����
            float sliceAngle = Mathf.Max(0f, angle - gapDegrees);

            var img = CreateSegmentImage(s.color);
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
            img.fillOrigin = 2; // 0=Right,1=Top,2=Left,3=Bottom (Radial360 ����)
            img.fillClockwise = clockwise;

            img.gameObject.GetComponent<PieSegment>().propertyName = names[i];

            // ���� ���� ��ġ
            var rt = (RectTransform)img.transform;
            rt.sizeDelta = size;
            rt.localEulerAngles = new Vector3(0, 0, dir * cursorDeg);

            // ä����
            img.fillAmount = initialFillZero ? 0f : (sliceAngle / 360f);

            _segmentImages.Add(img);

            // ���� ���� ���� ������ �̵� (���� angle��ŭ �̵�, gap��ŭ �߰� �̵�)
            cursorDeg += (angle);
            i++;
        }
    }

    Image CreateSegmentImage(Color col)
    {
        Image img;
        if (segmentPrefab != null)
        {
            img = Instantiate(segmentPrefab, transform);
        }
        else
        {
            var go = new GameObject("Segment", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            img = go.GetComponent<Image>();
        }
        img.sprite = circleSprite;
        img.color = col;
        var rt = (RectTransform)img.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;

        img.gameObject.AddComponent<PieSegment>();
        return img;
    }

    IEnumerator AnimateSegments(float duration)
    {
        float t = 0f;
        // ��ǥ fillAmounts ���
        var targets = new List<float>();
        float total = 0f; foreach (var s in segments) total += Mathf.Max(0f, s.value);
        foreach (var s in segments)
        {
            float portion = Mathf.Max(0f, s.value) / total;
            float angle = portion * 360f;
            float sliceAngle = Mathf.Max(0f, angle - gapDegrees);
            targets.Add(sliceAngle / 360f);
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            p = EaseInOutCustom(p, easeStrength);

            for (int i = 0; i < _segmentImages.Count; i++)
            {
                _segmentImages[i].fillAmount = Mathf.Lerp(0f, targets[i], p);
            }
            yield return null;
        }

        for (int i = 0; i < _segmentImages.Count; i++)
            _segmentImages[i].fillAmount = targets[i];

        _animCo = null;
    }

    void EnsureDonut()
    {
        // ���� �Ͼ�(�Ǵ� ����) ���� �ϳ� �� ��� ����ó�� ���̰� ��
        var holeName = "DonutHole";
        var exist = transform.Find(holeName) as RectTransform;
        if (exist == null)
        {
            var go = new GameObject(holeName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            exist = go.GetComponent<RectTransform>();
            var img = go.GetComponent<Image>();
            img.sprite = circleSprite;
            img.color = new Color(1, 1, 1, 1); // ���� ���� ����(�Ǵ� ���� public Color�� ����)
            img.raycastTarget = false;
        }
        exist.SetAsLastSibling(); // �� ���� ������
        exist.sizeDelta = size * (1f - innerHoleRatio); // ��� ���� ũ��
        exist.anchorMin = exist.anchorMax = new Vector2(0.5f, 0.5f);
        exist.pivot = new Vector2(0.5f, 0.5f);
        exist.localScale = Vector3.one;
        exist.localEulerAngles = Vector3.zero;
    }

    void ClearSegments()
    {
        foreach (var img in _segmentImages)
            if (img != null) DestroyImmediate(img.gameObject);
        _segmentImages.Clear();

        var hole = transform.Find("DonutHole");
        if (hole != null) DestroyImmediate(hole.gameObject);
    }

    // ��¡: ���ӵ� ���� ���� (0.5~5)
    float EaseInOutCustom(float x, float strength)
    {
        if (x < 0.5f)
            return 0.5f * Mathf.Pow(2f * x, strength);
        else
            return 1f - 0.5f * Mathf.Pow(2f * (1f - x), strength);
    }
}
