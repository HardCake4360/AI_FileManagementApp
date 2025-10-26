using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ImportFigmaUI : EditorWindow
{
    private const string ImagesPath = "Assets/UI/Images";
    private const string ColorsPath = "Assets/UI/Colors.json";
    private const string MaterialFolder = "Assets/UI/Materials";

    [MenuItem("Tools/Import Figma UI")]
    public static void ImportUI()
    {
        // 1️⃣ Canvas 준비
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("FigmaCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 2️⃣ 이미지 로드 및 생성
        if (Directory.Exists(ImagesPath))
        {
            string[] imageFiles = Directory.GetFiles(ImagesPath, "*.*", SearchOption.AllDirectories);
            int importedCount = 0;

            foreach (string imgPath in imageFiles)
            {
                if (!imgPath.EndsWith(".png") && !imgPath.EndsWith(".jpg") &&
                    !imgPath.EndsWith(".jpeg") && !imgPath.EndsWith(".svg"))
                    continue;

                string relativePath = imgPath.Replace("\\", "/");
                if (relativePath.StartsWith(Application.dataPath))
                    relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);
                if (sprite == null)
                {
                    Debug.LogWarning($"[FigmaImport] Sprite not found at path: {relativePath}");
                    continue;
                }

                GameObject imgObj = new GameObject(Path.GetFileNameWithoutExtension(imgPath));
                imgObj.transform.SetParent(canvas.transform, false);

                Image img = imgObj.AddComponent<Image>();
                img.sprite = sprite;
                img.SetNativeSize();

                importedCount++;
            }

            Debug.Log($"[FigmaImport] Imported {importedCount} images to Canvas '{canvas.name}'.");
        }
        else
        {
            Debug.LogWarning($"[FigmaImport] No image folder found: {ImagesPath}");
        }

        // 3️⃣ Colors.json 파싱
        if (File.Exists(ColorsPath))
        {
            string json = File.ReadAllText(ColorsPath);
            Dictionary<string, string> colorDict = ParseColorJson(json);

            if (colorDict == null || colorDict.Count == 0)
            {
                Debug.LogWarning("[FigmaImport] Colors.json is empty or invalid.");
                return;
            }

            // 색상 팔레트 ScriptableObject 생성
            FigmaColorPalette palette = ScriptableObject.CreateInstance<FigmaColorPalette>();
            palette.colors = new List<FigmaColor>();

            foreach (var kvp in colorDict)
            {
                string key = kvp.Key;
                string val = kvp.Value;

                if (ColorUtility.TryParseHtmlString(val, out Color color))
                {
                    palette.colors.Add(new FigmaColor(key, color));
                    Debug.Log($"[FigmaImport] Color Loaded: {key} = {val}");
                }
                else
                {
                    Debug.LogWarning($"[FigmaImport] Invalid color value skipped: {key} = {val}");
                }
            }

            string assetPath = "Assets/UI/FigmaColorPalette.asset";
            AssetDatabase.CreateAsset(palette, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[FigmaImport] Created Color Palette at {assetPath}");

            // 선택적으로 색상별 머티리얼 생성
            CreateMaterialsFromPalette(palette);
        }
        else
        {
            Debug.LogWarning($"[FigmaImport] Colors.json not found at {ColorsPath}");
        }

        EditorUtility.DisplayDialog("Figma Import", "Import completed successfully!", "OK");
    }

    // JSON을 Dictionary<string, string>으로 파싱
    private static Dictionary<string, string> ParseColorJson(string json)
    {
        var dict = new Dictionary<string, string>();
        var regex = new Regex("\"([^\"]+)\"\\s*:\\s*\"([^\"]+)\"");
        var matches = regex.Matches(json);

        foreach (Match match in matches)
        {
            string key = match.Groups[1].Value;
            string value = match.Groups[2].Value;
            dict[key] = value;
        }

        return dict;
    }

    // 색상 팔레트 ScriptableObject 정의
    [System.Serializable]
    public class FigmaColor
    {
        public string name;
        public Color color;
        public FigmaColor(string n, Color c)
        {
            name = n; color = c;
        }
    }

    public class FigmaColorPalette : ScriptableObject
    {
        public List<FigmaColor> colors;
    }

    // Material 자동 생성
    private static void CreateMaterialsFromPalette(FigmaColorPalette palette)
    {
        if (!Directory.Exists(MaterialFolder))
            Directory.CreateDirectory(MaterialFolder);

        foreach (var figmaColor in palette.colors)
        {
            string matPath = $"{MaterialFolder}/{figmaColor.name}.mat";
            Material mat = new Material(Shader.Find("UI/Default"));
            mat.color = figmaColor.color;
            AssetDatabase.CreateAsset(mat, matPath);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[FigmaImport] Created {palette.colors.Count} UI materials in {MaterialFolder}");
    }
}
