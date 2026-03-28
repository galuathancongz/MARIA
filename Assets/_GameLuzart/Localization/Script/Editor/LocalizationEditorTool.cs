#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Luzart
{
    public class LocalizationEditorTool : EditorWindow
    {
        private Vector2 scrollPos;
        private List<TMPTextEntry> entries = new List<TMPTextEntry>();
        private string searchFilter = "";
        private bool showOnlyUnlocalized = true;
        private string exportPath = "";

        private class TMPTextEntry
        {
            public string prefabPath;
            public string gameObjectPath;
            public string currentText;
            public string suggestedKey;
            public TMP_Text tmpComponent;
            public LocalizedText localizedText;
            public bool hasLocalizedText;
            public bool selected;
        }

        [MenuItem("Tools/Localization/Scan All TMP Text")]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalizationEditorTool>("Localization Scanner");
            window.minSize = new Vector2(800, 600);
            window.ScanProject();
        }

        [MenuItem("Tools/Localization/Export All Text to CSV")]
        public static void ExportCSV()
        {
            var window = GetWindow<LocalizationEditorTool>("Localization Scanner");
            window.ScanProject();
            window.DoExportCSV();
        }

        [MenuItem("Tools/Localization/Add LocalizedText to Selected Prefab")]
        public static void AddToSelected()
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a GameObject with TMP_Text", "OK");
                return;
            }

            var tmpTexts = Selection.activeGameObject.GetComponentsInChildren<TMP_Text>(true);
            int count = 0;
            foreach (var tmp in tmpTexts)
            {
                if (tmp.GetComponent<LocalizedText>() == null && !string.IsNullOrWhiteSpace(tmp.text) && tmp.text.Length > 1)
                {
                    var loc = tmp.gameObject.AddComponent<LocalizedText>();
                    count++;
                }
            }
            EditorUtility.DisplayDialog("Done", $"Added LocalizedText to {count} TMP_Text components", "OK");
        }

        private void ScanProject()
        {
            entries.Clear();

            // Scan all prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game", "Assets/_GameLuzart" });
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var tmpTexts = prefab.GetComponentsInChildren<TMP_Text>(true);
                foreach (var tmp in tmpTexts)
                {
                    string text = tmp.text;
                    if (string.IsNullOrWhiteSpace(text) || text.Length <= 1) continue;
                    if (text.StartsWith("<sprite") || text.StartsWith("+") || text.All(c => char.IsDigit(c) || c == '.' || c == '%' || c == ':')) continue;

                    var entry = new TMPTextEntry
                    {
                        prefabPath = path,
                        gameObjectPath = GetGameObjectPath(tmp.transform, prefab.transform),
                        currentText = text,
                        suggestedKey = GenerateKey(text, path),
                        tmpComponent = tmp,
                        localizedText = tmp.GetComponent<LocalizedText>(),
                        hasLocalizedText = tmp.GetComponent<LocalizedText>() != null,
                        selected = false
                    };
                    entries.Add(entry);
                }
            }

            // Scan scenes
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_GameLuzart" });
            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // Scenes need to be opened to scan, skip for now
            }

            entries = entries.OrderBy(e => e.prefabPath).ThenBy(e => e.gameObjectPath).ToList();
        }

        private string GetGameObjectPath(Transform current, Transform root)
        {
            List<string> path = new List<string>();
            while (current != null && current != root)
            {
                path.Insert(0, current.name);
                current = current.parent;
            }
            return string.Join("/", path);
        }

        private string GenerateKey(string text, string prefabPath)
        {
            // Generate a key based on text content
            string cleanText = text.ToLowerInvariant()
                .Replace(" ", "_")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("\n", "_")
                .Replace(":", "")
                .Replace("?", "")
                .Replace("!", "")
                .Replace(",", "")
                .Replace(".", "");

            if (cleanText.Length > 40)
                cleanText = cleanText.Substring(0, 40);

            // Determine category from prefab path
            string category = "ui";
            if (prefabPath.Contains("Level1") || prefabPath.Contains("Level 1")) category = "level1";
            else if (prefabPath.Contains("Level2") || prefabPath.Contains("Level 2")) category = "level2";
            else if (prefabPath.Contains("Level3") || prefabPath.Contains("Level 3")) category = "level3";
            else if (prefabPath.Contains("Level4") || prefabPath.Contains("Level 4")) category = "level4";
            else if (prefabPath.Contains("Tut")) category = "tutorial";
            else if (prefabPath.Contains("Menu")) category = "menu";
            else if (prefabPath.Contains("Settings")) category = "settings";
            else if (prefabPath.Contains("Profile")) category = "profile";

            return $"{category}.{cleanText}";
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Scan Project", EditorStyles.toolbarButton, GUILayout.Width(100)))
                ScanProject();
            if (GUILayout.Button("Export CSV", EditorStyles.toolbarButton, GUILayout.Width(100)))
                DoExportCSV();
            if (GUILayout.Button("Select All", EditorStyles.toolbarButton, GUILayout.Width(80)))
                entries.ForEach(e => e.selected = true);
            if (GUILayout.Button("Deselect All", EditorStyles.toolbarButton, GUILayout.Width(80)))
                entries.ForEach(e => e.selected = false);
            GUILayout.FlexibleSpace();
            showOnlyUnlocalized = GUILayout.Toggle(showOnlyUnlocalized, "Only Unlocalized", EditorStyles.toolbarButton, GUILayout.Width(120));
            searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            // Stats
            int total = entries.Count;
            int localized = entries.Count(e => e.hasLocalizedText);
            EditorGUILayout.HelpBox($"Found {total} TMP texts | {localized} localized | {total - localized} need localization", MessageType.Info);

            // List
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            string lastPrefab = "";

            foreach (var entry in entries)
            {
                if (showOnlyUnlocalized && entry.hasLocalizedText) continue;
                if (!string.IsNullOrEmpty(searchFilter) &&
                    !entry.currentText.ToLower().Contains(searchFilter.ToLower()) &&
                    !entry.prefabPath.ToLower().Contains(searchFilter.ToLower())) continue;

                // Prefab header
                if (entry.prefabPath != lastPrefab)
                {
                    lastPrefab = entry.prefabPath;
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(entry.prefabPath, EditorStyles.boldLabel);
                }

                EditorGUILayout.BeginHorizontal(entry.hasLocalizedText ? "box" : EditorStyles.helpBox);

                entry.selected = EditorGUILayout.Toggle(entry.selected, GUILayout.Width(20));

                // Status icon
                GUILayout.Label(entry.hasLocalizedText ? "V" : "X", GUILayout.Width(15));

                // GameObject path
                EditorGUILayout.LabelField(entry.gameObjectPath, GUILayout.Width(200));

                // Text preview (truncated)
                string preview = entry.currentText.Length > 60 ? entry.currentText.Substring(0, 60) + "..." : entry.currentText;
                preview = preview.Replace("\n", " ");
                EditorGUILayout.LabelField(preview, GUILayout.MinWidth(200));

                // Key
                EditorGUILayout.LabelField(entry.suggestedKey, EditorStyles.miniLabel, GUILayout.Width(200));

                // Ping button
                if (GUILayout.Button("Ping", GUILayout.Width(40)))
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.prefabPath);
                    EditorGUIUtility.PingObject(prefab);
                    Selection.activeObject = prefab;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DoExportCSV()
        {
            string path = EditorUtility.SaveFilePanel("Export CSV", Application.dataPath, "localization_texts", "csv");
            if (string.IsNullOrEmpty(path)) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Key,English,Vietnamese,PrefabPath,GameObjectPath");

            // Deduplicate by text content
            var uniqueTexts = entries
                .GroupBy(e => e.currentText.Trim())
                .Select(g => g.First())
                .OrderBy(e => e.suggestedKey)
                .ToList();

            foreach (var entry in uniqueTexts)
            {
                string text = entry.currentText.Replace("\"", "\"\"").Replace("\n", "\\n");
                string prefab = entry.prefabPath;
                string goPath = entry.gameObjectPath;
                sb.AppendLine($"\"{entry.suggestedKey}\",\"{text}\",\"\",\"{prefab}\",\"{goPath}\"");
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            EditorUtility.DisplayDialog("Export Done", $"Exported {uniqueTexts.Count} unique texts to:\n{path}", "OK");
            Debug.Log($"[Localization] Exported {uniqueTexts.Count} texts to {path}");
        }
    }
}
#endif
