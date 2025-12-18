using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Diagnostics;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class ResultPrefab : MonoBehaviour
{
    [SerializeField] Image thumbnail;
    [SerializeField] Sprite placeholderSprite;

    [SerializeField] TextMeshProUGUI fileName;
    [SerializeField] TextMeshProUGUI path;
    [SerializeField] TextMeshProUGUI score;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] TagsList tagsList;
    [SerializeField] Button copy;
    [SerializeField] Button show;
    [SerializeField] Button delete;

    [SerializeField] GameObject confirmPrefab;

    GameObject confirmActive;
    public string pathString;

    public void SetMembers(SearchResultItem item)
    {
        pathString = item.path;
        fileName.text = item.name;
        path.text = "파일 경로: " + item.path;
        score.text = "일치율: " + ((int)(item.score * 100)).ToString() + "%";
        tagsList.SetList(item.tags);
        description.text = item.description;
        ApplyThumbnail(item.thumbnail);

        copy.onClick.AddListener(() =>
        {
            onCopy();
        });
        show.onClick.AddListener(() =>
        {
            onShow();
        });
        delete.onClick.AddListener(() =>
        {
            onDelete();
        });
    }

    void ApplyThumbnail(string thumbnailUrl)
    {
        if (string.IsNullOrEmpty(thumbnailUrl))
        {
            // 썸네일 없으면 기본 이미지
            thumbnail.sprite = placeholderSprite;
            thumbnail.gameObject.SetActive(true);
            return;
        }

        StartCoroutine(LoadThumbnail(thumbnailUrl));
    }

    IEnumerator LoadThumbnail(string url)
    {
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogWarning($"Thumbnail load failed: {req.error}");
                thumbnail.sprite = placeholderSprite;
                yield break;
            }

            Texture2D tex = DownloadHandlerTexture.GetContent(req);
            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );

            thumbnail.color = new Vector4(255, 255, 255, 255);
            thumbnail.sprite = sprite;
            thumbnail.preserveAspect = true;
            thumbnail.gameObject.SetActive(true);
        }
    }

    void onFavorite()
    {
        
    }

    void onCopy()
    {
        if (string.IsNullOrWhiteSpace(pathString))
        {
            UnityEngine.Debug.LogWarning("[PathActions] 빈 경로입니다.");
            NotificationManager.Instance.Show("경로가 비어있습니다");
            return;
        }

        string normalized = NormalizePath(pathString);
        GUIUtility.systemCopyBuffer = normalized;
        UnityEngine.Debug.Log($"[PathActions] 클립보드로 복사: {normalized}");
        NotificationManager.Instance.Show("경로를 클립보드로 복사했습니다");
    }

    void onShow()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (string.IsNullOrWhiteSpace(pathString))
        {
            UnityEngine.Debug.LogWarning("[PathActions] 빈 경로입니다.");
            NotificationManager.Instance.Show("경로가 비어있습니다");
            return;
        }

        string normalized = NormalizePath(pathString);
        string targetDir = null;

        try
        {
            // 파일 or 디렉터리 존재 체크
            if (File.Exists(normalized))
            {
                // 파일 존재 → 선택
                LaunchExplorerSelect(normalized);
                return;
            }
            else if (Directory.Exists(normalized))
            {
                // 디렉터리 존재 → 해당 폴더 오픈
                LaunchExplorerOpen(normalized);
                return;
            }
            else
            {
                // 없음 → 상위 디렉터리라도 열기 시도
                targetDir = FindExistingAncestor(normalized);
                if (!string.IsNullOrEmpty(targetDir))
                {
                    UnityEngine.Debug.LogWarning($"[PathActions] 대상이 없어 상위 폴더를 엽니다: {targetDir}");
                    NotificationManager.Instance.Show("대상이 없어 상위 폴더를 엽니다");
                    LaunchExplorerOpen(targetDir);
                    return;
                }

                UnityEngine.Debug.LogError($"[PathActions] 경로를 찾을 수 없습니다: {normalized}");
                NotificationManager.Instance.Show("경로를 찾을 수 없습니다");
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[PathActions] 탐색기 열기 실패: {e.Message}\nPath: {normalized}");
            NotificationManager.Instance.Show("탐색기 호출 실패. 다시 시도해주세요");
        }
#else
        Debug.LogWarning("[PathActions] 이 기능은 Windows에서만 지원됩니다.");
#endif
    }

    void onDelete()
    {
        if (!confirmActive)
        {
            confirmActive = Instantiate(confirmPrefab, delete.gameObject.transform);
            confirmActive.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            confirmActive.GetComponent<DeleteConfirmUI>().SetUI(pathString, gameObject);
            confirmActive.transform.SetAsLastSibling();
        }

    }


    private static string NormalizePath(string path)
    {
        // 슬래시 통일 + 불필요한 따옴표 제거 + FullPath
        string p = path.Trim().Trim('"').Replace('/', '\\');
        try
        {
            // UNC/절대 경로가 아니어도 FullPath 변환 시도
            p = Path.GetFullPath(p);
        }
        catch { /* 상대경로 등 예외는 무시하고 원본을 사용 */ }
        return p;
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private static void LaunchExplorerOpen(string directory)
    {
        // explorer "C:\Some Dir"
        StartProcess("explorer.exe", $"\"{directory}\"");
    }

    private static void LaunchExplorerSelect(string filePath)
    {
        // explorer /select,"C:\Some Dir\file.txt"
        StartProcess("explorer.exe", $"/select,\"{filePath}\"");
    }

    private static void StartProcess(string fileName, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true
        };
        Process.Start(psi);
    }
#endif

    private static string FindExistingAncestor(string path)
    {
        try
        {
            string current = path;
            // 파일이면 상위 폴더부터 시작
            if (!string.IsNullOrEmpty(Path.GetExtension(current)))
                current = Path.GetDirectoryName(current);

            while (!string.IsNullOrEmpty(current))
            {
                if (Directory.Exists(current)) return current;
                current = Path.GetDirectoryName(current);
            }
        }
        catch { /* 무시 */ }
        return null;
    }
}
