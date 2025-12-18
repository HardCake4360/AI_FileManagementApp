using UnityEngine;

public class FavoriteMenu : MonoBehaviour
{
    public static FavoriteMenu Instance { get; private set; }
    [SerializeField] Transform addTarget;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void AddFavorite(GameObject obj)
    {
        if (obj == null || addTarget == null)
        {
            Debug.LogWarning("FavoriteMenu: 대상이 없습니다!");
            return;
        }

        var res = obj.GetComponent<ResultPrefab>();
        if (!res)
        {
            Debug.LogWarning("FavoriteMenu: ResultPrefab 컴포넌트가 없습니다!");
            return;
        }

        foreach (Transform child in addTarget)
        {
            var comp = child.GetComponent<ResultPrefab>();
            if (!comp) continue;
            if (comp.pathString == res.pathString)
                return; // 이미 같은 항목 존재
        }

        Instantiate(obj, addTarget);
    }
}
