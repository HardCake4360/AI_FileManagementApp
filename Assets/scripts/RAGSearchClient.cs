using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;
using TMPro;


/*
 * RAGSearchClient
 * - Flask 서버(/search)를 호출해 의미 검색을 수행한다.
 * - InputField/TMP_InputField의 텍스트를 쿼리로 사용한다.
 * - 결과는 (이름, 경로, 태그, 유사도) 구조로 파싱되어 콜백 및 로컬 저장이 가능하다.
 */

[Serializable]
public class SearchResultItem
{
    public string name;             // 파일 이름
    public string path;             // 파일 경로
    public List<string> tags;     // 태그들
    public float score;             // 유사도
    public string description;      // 짧은 1문장 묘사
    public string thumbnail; // ← 서버에서 내려주는 썸네일 URL
}

[Serializable]
public class SearchResponse
{
    public List<SearchResultItem> results;
    public int count;
    public bool need_index;
    public string msg;
}

public class RAGSearchClient : MonoBehaviour
{
    public static RAGSearchClient Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("Server")]
    [SerializeField] private string baseUrl = "http://localhost:5001";
    [SerializeField] private int topK = 20;

    [Header("Query Input")]
    public TMP_InputField tmpInputField;

    [Header("Root Path Input")]
    public TMP_InputField rootTMPInput;

    [Header("Indexing")]
    public TMP_InputField rootPathInput;
    public bool chunk = true;
    public int maxTextKB = 512;
    public bool llmDesc = true; // LLM 한줄요약 사용 여부

    [Header("Optional Filters")]
    [Tooltip("예: .cs, .md, .pdf 등. 빈 리스트면 필터 미적용")]
    public List<string> extFilters = new List<string>();

    [Header("Events")]
    public UnityEvent<List<SearchResultItem>> onResults;

    [Header("Save Settings")]
    public string savedFileName = "last_search_results.json";

    [Header("Properties")]
    [SerializeField] GameObject resultPrefab;
    [SerializeField] RectTransform refreshUI;
    [SerializeField] Transform resultViewTransform;
    [SerializeField] ScrollRect scrollRect;

    int testInt = 0;

    [System.Serializable]
    class IndexScanOptions
    {
        public int max_text_kb;
        public bool chunk;
        public bool scan_images = true;
        public bool llm_desc;
    }
    [System.Serializable]
    class IndexScanRequest
    {
        public string root;
        public IndexScanOptions options;
    }
    [System.Serializable]
    class IndexScanResponse
    {
        public string root;
        public int scanned_files;
        public int indexed_files;
        public int indexed_chunks;
        public int index_size;
        public int removed;
        public int modified;
        public int added;
        public float elapsed_sec;
        public string error; // 실패시 서버가 보낼 수 있음
    }


    // --------------------------
    // File Delete (Soft delete)
    // --------------------------
    [System.Serializable]
    public class DeleteFileRequest
    {
        public string path;
    }

    [System.Serializable]
    public class DeleteFileResponse
    {
        public bool ok;
        public string path;
        public bool deleted;
        public bool thumbnail_deleted;
        public string msg;
    }




    [Serializable]
    public class SearchRequest
    {
        public string query;
        public int top_k;
        public Filters filters; // null 가능
    }

    [Serializable]
    public class Filters
    {
        public List<string> ext;
    }

    private void Start()
    {
        clearResultList();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && tmpInputField.text != "")
        {
            SearchButton();
        }
    }

    public void RequestIndexing()
    {
        if (rootPathInput == null || string.IsNullOrWhiteSpace(rootPathInput.text))
        {
            Debug.LogWarning("[RAGSearchClient] Root path is empty.");
            return;
        }

        StartCoroutine(IndexScanCoroutine(rootPathInput.text.Trim()));
    }

    public void SearchButton()
    {
        string q = GetCurrentQuery();
        if (string.IsNullOrWhiteSpace(q))
        {
            Debug.LogWarning("[RAGSearchClient] Query is empty.");
            return;
        }
        StartCoroutine(SearchCoroutine(q));
    }

    string GetCurrentQuery()
    {
        if (tmpInputField != null) return tmpInputField.text;

        return string.Empty;
    }

    IEnumerator<UnityWebRequestAsyncOperation> SearchCoroutine(string query)
    {
        var reqBody = BuildRequestJson(query);
        var url = $"{baseUrl}/search";

        var uwr = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(reqBody);
        uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        var op = uwr.SendWebRequest();
        yield return op;

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[RAGSearchClient] /search error: {uwr.error}\n{uwr.downloadHandler.text}");
            yield break;
        }

        var json = uwr.downloadHandler.text;
        var resp = JsonUtility.FromJson<SearchResponse>(json);
        if (resp == null || resp.results == null)
        {
            Debug.LogWarning("[RAGSearchClient] Empty or invalid response.");
            yield break;
        }

        if (resp.need_index)
        {
            Debug.LogWarning($"[RAGSearchClient] Server says indexing required. Message: {resp.msg}");
            yield break;
        }

        // 결과 저장
        SaveResults(resp.results);
        setResultUI(resp);

        LayoutRebuilder.ForceRebuildLayoutImmediate(refreshUI);

        yield return null;

        scrollRect.normalizedPosition = new Vector2(0, 1);

        // 이벤트 콜백
        onResults?.Invoke(resp.results);
    }

    public void OnClick_IndexFromInput()
    {
        // 1) 인풋에서 루트 경로 읽기
        string rootPath = "";
        if (rootTMPInput != null) rootPath = rootTMPInput.text;

        if (string.IsNullOrWhiteSpace(rootPath))
        {
            Debug.LogWarning("[RAGSearchClient] 루트 경로가 비어있음");
            NotificationManager.Instance.Show("루트 경로가 비어있습니다. 유효한 경로를 입력해주세요.");
            return;
        }

        // 윈도우 경로 정리(선택)
        rootPath = rootPath.Trim().Replace('/', '\\');

        // 2) 인덱싱 시작
        StartCoroutine(IndexScanCoroutine(rootPath));
    }

    System.Collections.IEnumerator IndexScanCoroutine(string rootPath)
    {
        NotificationManager.Instance.Show("인덱싱 시작");
        // 요청 바디 구성
        var req = new IndexScanRequest
        {
            root = rootPath,
            options = new IndexScanOptions
            {
                max_text_kb = this.maxTextKB,
                chunk = this.chunk,
                scan_images = true,
                llm_desc = this.llmDesc
            }
        };

        var url = $"{baseUrl}/index/scan";
        var json = JsonUtility.ToJson(req);
        var uwr = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        uwr.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        uwr.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[RAGSearchClient] /index/scan 실패: {uwr.error}\n{uwr.downloadHandler.text}");
            NotificationManager.Instance.Show("요청을 실패했습니다. 다시 시도해주세요.");
            yield break;
        }

        var respJson = uwr.downloadHandler.text;
        var resp = JsonUtility.FromJson<IndexScanResponse>(respJson);

        if (resp == null || !string.IsNullOrEmpty(resp.error))
        {
            Debug.LogError($"[RAGSearchClient] 인덱싱 실패: {respJson}");
            NotificationManager.Instance.Show("인덱스 생성 실패");
            yield break;
        }

        Debug.Log(
            $"[IndexScan OK]\n" +
            $"- Root: {resp.root}\n" +
            $"- Scanned: {resp.scanned_files}\n" +
            $"- Indexed Files: {resp.indexed_files}, Chunks: {resp.indexed_chunks}\n" +
            $"- Index Size: {resp.index_size}\n" +
            $"- Added:{resp.added} Modified:{resp.modified} Removed:{resp.removed}\n" +
            $"- Time: {resp.elapsed_sec:F2}s"
        );
        NotificationManager.Instance.Show("인덱스 생성 성공했습니다");
    }




    // --------------------------
    // Public API: Delete file on server (soft delete)
    // --------------------------
    public void RequestDeleteFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Debug.LogWarning("[RAGSearchClient] Delete path is empty.");
            NotificationManager.Instance.Show("삭제하려는 경로가 비어있습니다");
            return;
        }
        StartCoroutine(DeleteFileCoroutine(filePath.Trim(), null));
    }

    /// <summary>
    /// Calls POST /file/delete with JSON { path }.
    /// </summary>
    public IEnumerator DeleteFileCoroutine(string filePath, UnityAction<DeleteFileResponse> onDone)
    {
        var req = new DeleteFileRequest { path = filePath };
        var url = $"{baseUrl}/file/delete";
        var json = JsonUtility.ToJson(req);

        var uwr = new UnityWebRequest(url, "POST");
        uwr.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[RAGSearchClient] /file/delete 실패: {uwr.error}\n{uwr.downloadHandler.text}");
            NotificationManager.Instance.Show("삭제 요청을 실패했습니다.");
            onDone?.Invoke(new DeleteFileResponse
            {
                ok = false,
                path = filePath,
                msg = uwr.error
            });
            yield break;
        }

        var respJson = uwr.downloadHandler.text;
        var resp = JsonUtility.FromJson<DeleteFileResponse>(respJson);

        if (resp == null)
        {
            Debug.LogWarning($"[RAGSearchClient] /file/delete 응답 파싱 실패: {respJson}");
            NotificationManager.Instance.Show("유효하지 않은 경로입니다");
            onDone?.Invoke(new DeleteFileResponse
            {
                ok = false,
                path = filePath,
                msg = "parse failed"
            });
            yield break;
        }

        if (resp.ok)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception e)
            {
                resp.msg = (resp.msg ?? "") + $" | local delete failed: {e.GetType().Name}";
            }
        }


        onDone?.Invoke(resp);
    }

    void clearResultList()
    {
        foreach (Transform child in resultViewTransform)
        {
            Destroy(child.gameObject);
        }
    }

    void setResultUI(SearchResponse resp)
    {
        clearResultList();
        foreach (var item in resp.results)
        {
            // --- PATCH: ensure absolute thumbnail URL ---
            if (!string.IsNullOrEmpty(item.thumbnail))
            {
                if (item.thumbnail.StartsWith("/"))
                    item.thumbnail = $"{baseUrl}{item.thumbnail}";
                else if (!item.thumbnail.StartsWith("http"))
                    item.thumbnail = $"{baseUrl}/{item.thumbnail.TrimStart('/')}";
            }
            // --------------------------------------------

            GameObject go = Instantiate(resultPrefab, resultViewTransform);
            go.GetComponent<ResultPrefab>().SetMembers(item);
        }
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(refreshUI);

        // 이제 스크롤 위치를 조정 (예: 맨 위로)
        scrollRect.normalizedPosition = new Vector2(0, 1);
    }

    string BuildRequestJson(string query)
    {
        var req = new SearchRequest
        {
            query = query,
            top_k = Mathf.Max(1, topK),
            filters = (extFilters != null && extFilters.Count > 0) ? new Filters { ext = extFilters } : null
        };
        // JsonUtility는 null 필드를 생략하지 않으므로, 적당히 직렬화
        // 간단히 한번 직렬화 후, filters가 null이면 "filters":null 이 포함되어도 서버는 무시한다.
        return JsonUtility.ToJson(req);
    }

    void SaveResults(List<SearchResultItem> items)
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, savedFileName);
            string json = JsonUtility.ToJson(new Wrapper { results = items }, prettyPrint: true);
            File.WriteAllText(path, json, Encoding.UTF8);
            Debug.Log($"[RAGSearchClient] Saved {items.Count} results → {path}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[RAGSearchClient] SaveResults error: {e.Message}");
        }
    }

    [Serializable]
    private class Wrapper
    {
        public List<SearchResultItem> results;
    }

    //디버그용 함수
    public void PopupTest()
    {
        testInt++;
        NotificationManager.Instance.Show("테스트 팝업 생성: "+testInt,999);
    }
}
