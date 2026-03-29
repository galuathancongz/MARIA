#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Luzart
{
    public class AutoLocalizerTool : EditorWindow
    {
        // ─── State ────────────────────────────────────────────────────────────
        private enum Tab { AutoApply, SceneSetup }
        private Tab currentTab = Tab.AutoApply;
        private Vector2 scrollPos;
        private string statusMessage = "";

        private List<LocalizeEntry> entries = new List<LocalizeEntry>();
        private Dictionary<string, string> reverseMap  = new Dictionary<string, string>(); // EN value  → key
        private Dictionary<string, string> keyToValueEn = new Dictionary<string, string>(); // key       → EN value

        private bool showOnlyUnmatched = false;

        private class LocalizeEntry
        {
            public GameObject      go;
            public TMP_Text        tmp;
            public string          text;          // original TMP text
            public string          prefabPath;    // null = scene object
            public string          goPath;
            public string          matchedKey;    // filled when text exists in en.json
            public string          customKey;     // editable by user (for unmatched)
            public string          customVI;      // optional VI translation for new key
            public bool            selected;
            public bool            isSceneObject;
            public bool IsMatched => !string.IsNullOrEmpty(matchedKey);
        }

        // ─── Menu ─────────────────────────────────────────────────────────────
        [MenuItem("Tools/Localization/Auto Localizer (One-Click)")]
        public static void Open()
        {
            var w = GetWindow<AutoLocalizerTool>("Auto Localizer");
            w.minSize = new Vector2(950, 580);
            w.LoadJson();
        }

        // ─── Json ─────────────────────────────────────────────────────────────
        private string EnJsonPath => Path.Combine(Application.dataPath,
            "_GameLuzart/Localization/Resources/Localization/en.json");
        private string ViJsonPath => Path.Combine(Application.dataPath,
            "_GameLuzart/Localization/Resources/Localization/vi.json");

        private void LoadJson()
        {
            reverseMap.Clear();
            keyToValueEn.Clear();

            if (!File.Exists(EnJsonPath))
            {
                statusMessage = "en.json not found: " + EnJsonPath;
                return;
            }

            var data = JsonUtility.FromJson<LocalizationData>(File.ReadAllText(EnJsonPath));
            if (data?.items == null) return;

            foreach (var item in data.items)
            {
                if (string.IsNullOrEmpty(item.value)) continue;
                // Store both exact-trimmed and normalized forms for robust matching
                string exact = item.value.Trim();
                string norm  = NormalizeText(item.value);
                if (!reverseMap.ContainsKey(exact))  reverseMap[exact]  = item.key;
                if (!reverseMap.ContainsKey(norm))   reverseMap[norm]   = item.key;
                keyToValueEn[item.key] = item.value;
            }
            statusMessage = $"Loaded {keyToValueEn.Count} keys from en.json";
        }

        /// <summary>
        /// Normalize smart/curly typography to plain ASCII for robust reverse-lookup.
        /// Mirrors Loc.Normalize() so the editor tool and runtime behave identically.
        /// </summary>
        private static string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text
                .Replace('\u2018', '\'').Replace('\u2019', '\'')
                .Replace('\u201C', '"') .Replace('\u201D', '"')
                .Replace('\u2013', '-') .Replace('\u2014', '-')
                .Replace("\u2026", "...").Replace("\u200B", "")
                .Trim();
        }

        // ─── Scan ─────────────────────────────────────────────────────────────
        private void ScanAll()
        {
            if (reverseMap.Count == 0) LoadJson();
            entries.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Prefab",
                new[] { "Assets/_Game", "Assets/_GameLuzart" });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Auto Localizer",
                    $"Scanning prefabs… {i + 1}/{guids.Length}", (float)(i + 1) / guids.Length);

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab) continue;

                foreach (var tmp in prefab.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (tmp.GetComponent<LocalizedText>() != null) continue;
                    string text = tmp.text?.Trim();
                    if (ShouldSkip(text)) continue;

                    // Try exact match, then normalized (handles curly quotes, em-dashes, etc.)
                    if (!reverseMap.TryGetValue(text, out string key))
                        reverseMap.TryGetValue(NormalizeText(text), out key);

                    entries.Add(new LocalizeEntry
                    {
                        go          = tmp.gameObject,
                        tmp         = tmp,
                        text        = text,
                        prefabPath  = path,
                        goPath      = GoPath(tmp.transform),
                        matchedKey  = key,
                        customKey   = key ?? SuggestKey(text, path),
                        isSceneObject = false,
                        selected    = key != null
                    });
                }
            }
            EditorUtility.ClearProgressBar();

            // Scan open scenes
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (tmp.GetComponent<LocalizedText>() != null) continue;
                        string text = tmp.text?.Trim();
                        if (ShouldSkip(text)) continue;

                        if (!reverseMap.TryGetValue(text, out string key))
                            reverseMap.TryGetValue(NormalizeText(text), out key);

                        entries.Add(new LocalizeEntry
                        {
                            go          = tmp.gameObject,
                            tmp         = tmp,
                            text        = text,
                            prefabPath  = null,
                            goPath      = GoPath(tmp.transform),
                            matchedKey  = key,
                            customKey   = key ?? SuggestKey(text, null),
                            isSceneObject = true,
                            selected    = key != null
                        });
                    }
                }
            }

            int matched   = entries.Count(e => e.IsMatched);
            int unmatched = entries.Count(e => !e.IsMatched);
            statusMessage = $"Scan done — {entries.Count} TMP_Text without LocalizedText: " +
                            $"{matched} matched ({unmatched} unmatched / need manual key)";
        }

        private static bool ShouldSkip(string t)
        {
            if (string.IsNullOrWhiteSpace(t) || t.Length <= 1) return true;
            if (t.StartsWith("<sprite") || t.StartsWith("<color") || t.StartsWith("<size")) return true;
            if (t.All(c => char.IsDigit(c) || c == '.' || c == '%' || c == ':' || c == '/' || c == '-' || c == '+'))
                return true;
            return false;
        }

        private static string GoPath(Transform t)
        {
            var parts = new List<string>();
            while (t != null) { parts.Insert(0, t.name); t = t.parent; }
            return string.Join("/", parts);
        }

        private static string SuggestKey(string text, string prefabPath)
        {
            string cat = "ui";
            if (prefabPath != null)
            {
                if      (prefabPath.Contains("Level1") || prefabPath.Contains("Level 1")) cat = "level1";
                else if (prefabPath.Contains("Level2") || prefabPath.Contains("Level 2")) cat = "level2";
                else if (prefabPath.Contains("Level3") || prefabPath.Contains("Level 3")) cat = "level3";
                else if (prefabPath.Contains("Menu"))     cat = "menu";
                else if (prefabPath.Contains("Tut"))      cat = "tutorial";
                else if (prefabPath.Contains("Profile"))  cat = "profile";
                else if (prefabPath.Contains("Settings")) cat = "settings";
            }
            string clean = text.ToLowerInvariant()
                .Replace(" ", "_").Replace("'", "").Replace("\"", "").Replace("\n", "_")
                .Replace(":", "").Replace("?", "").Replace("!", "").Replace(",", "").Replace(".", "")
                .Replace("/", "_").Replace("\\", "_");
            if (clean.Length > 30) clean = clean.Substring(0, 30);
            return $"{cat}.{clean.Trim('_')}";
        }

        // ─── Apply ─────────────────────────────────────────────────────────────
        private void ApplySelected()
        {
            var toApply = entries.Where(e => e.selected).ToList();
            if (toApply.Count == 0) { EditorUtility.DisplayDialog("Nothing selected", "Tick at least one item.", "OK"); return; }

            // Validate keys for unmatched
            foreach (var e in toApply.Where(x => !x.IsMatched))
            {
                if (string.IsNullOrWhiteSpace(e.customKey))
                {
                    EditorUtility.DisplayDialog("Missing key",
                        $"Enter a localization key for:\n\"{Truncate(e.text, 60)}\"", "OK");
                    return;
                }
            }

            // 1. Write new keys into JSON files for unmatched entries
            var newEntries = toApply.Where(e => !e.IsMatched).ToList();
            if (newEntries.Count > 0)
            {
                AppendToJson(EnJsonPath, newEntries, vi: false);
                AppendToJson(ViJsonPath, newEntries, vi: true);
                // Rebuild reverse map so the keys are found in further passes
                LoadJson();
            }

            // 2. Apply LocalizedText to prefabs (group by prefab path)
            int appliedCount  = 0;
            int prefabsSaved  = 0;

            foreach (var group in toApply.Where(e => !e.isSceneObject).GroupBy(e => e.prefabPath))
            {
                var root = PrefabUtility.LoadPrefabContents(group.Key);
                if (!root) continue;

                bool dirty = false;
                var allTmps = root.GetComponentsInChildren<TMP_Text>(true);

                foreach (var entry in group)
                {
                    var target = MatchTmp(allTmps, entry.text, entry.goPath);
                    if (!target || target.GetComponent<LocalizedText>() != null) continue;

                    var comp = target.gameObject.AddComponent<LocalizedText>();
                    SetLocKey(comp, entry.IsMatched ? entry.matchedKey : entry.customKey);
                    dirty = true;
                    appliedCount++;
                }

                if (dirty) { PrefabUtility.SaveAsPrefabAsset(root, group.Key); prefabsSaved++; }
                PrefabUtility.UnloadPrefabContents(root);
            }

            // 3. Apply to scene objects
            int sceneDirty = 0;
            foreach (var entry in toApply.Where(e => e.isSceneObject))
            {
                if (!entry.go || entry.go.GetComponent<LocalizedText>() != null) continue;
                Undo.RecordObject(entry.go, "Add LocalizedText");
                var comp = Undo.AddComponent<LocalizedText>(entry.go);
                SetLocKey(comp, entry.IsMatched ? entry.matchedKey : entry.customKey);
                EditorSceneManager.MarkSceneDirty(entry.go.scene);
                appliedCount++;
                sceneDirty++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            entries.RemoveAll(e => toApply.Contains(e));
            statusMessage = $"Done — added LocalizedText to {appliedCount} objects " +
                            $"({prefabsSaved} prefabs saved, {sceneDirty} scene objects). " +
                            (newEntries.Count > 0 ? $"{newEntries.Count} new keys added to JSON." : "");

            Debug.Log("[AutoLocalizer] " + statusMessage);
            EditorUtility.DisplayDialog("Applied", statusMessage, "OK");
        }

        private static void SetLocKey(LocalizedText comp, string key)
        {
            var so   = new SerializedObject(comp);
            var prop = so.FindProperty("locKey");
            if (prop != null) { prop.stringValue = key; so.ApplyModifiedPropertiesWithoutUndo(); }
        }

        private static TMP_Text MatchTmp(TMP_Text[] candidates, string text, string goPath)
        {
            // Exact path + text match
            foreach (var t in candidates)
                if (t.text?.Trim() == text && GoPath(t.transform) == goPath && !t.GetComponent<LocalizedText>())
                    return t;
            // Fallback: text match only
            foreach (var t in candidates)
                if (t.text?.Trim() == text && !t.GetComponent<LocalizedText>())
                    return t;
            return null;
        }

        // ─── JSON write ────────────────────────────────────────────────────────
        private void AppendToJson(string filePath, List<LocalizeEntry> newItems, bool vi)
        {
            if (!File.Exists(filePath)) return;

            var data = JsonUtility.FromJson<LocalizationData>(File.ReadAllText(filePath));
            var list = data?.items?.ToList() ?? new List<LocalizationItem>();
            var existingKeys = new HashSet<string>(list.Select(i => i.key));

            foreach (var e in newItems)
            {
                if (existingKeys.Contains(e.customKey)) continue;
                string val = vi && !string.IsNullOrWhiteSpace(e.customVI) ? e.customVI : e.text;
                list.Add(new LocalizationItem { key = e.customKey, value = val });
                existingKeys.Add(e.customKey);
            }

            // Serialize back preserving exact format: { "items": [ ... ] }
            var sb = new StringBuilder();
            sb.AppendLine("{ \"items\": [");
            for (int i = 0; i < list.Count; i++)
            {
                string v = list[i].value
                    .Replace("\\", "\\\\").Replace("\"", "\\\"")
                    .Replace("\n", "\\n").Replace("\r", "");
                sb.Append($"  {{\"key\": \"{list[i].key}\", \"value\": \"{v}\"}}");
                if (i < list.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("]}");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        // ─── GUI ──────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            // Tab bar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Toggle(currentTab == Tab.AutoApply,  "  Auto Apply  ",  EditorStyles.toolbarButton)) currentTab = Tab.AutoApply;
            if (GUILayout.Toggle(currentTab == Tab.SceneSetup, "  Scene Setup  ", EditorStyles.toolbarButton)) currentTab = Tab.SceneSetup;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (currentTab == Tab.AutoApply)  DrawAutoApply();
            else                              DrawSceneSetup();
        }

        // ── Tab: Auto Apply ──────────────────────────────────────────────────
        private void DrawAutoApply()
        {
            // Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Scan All", EditorStyles.toolbarButton, GUILayout.Width(80))) ScanAll();
            GUILayout.Space(6);
            if (entries.Count > 0)
            {
                if (GUILayout.Button("✓ Select Matched", EditorStyles.toolbarButton, GUILayout.Width(110)))
                    entries.ForEach(e => e.selected = e.IsMatched);
                if (GUILayout.Button("Select All",     EditorStyles.toolbarButton, GUILayout.Width(70)))
                    entries.ForEach(e => e.selected = true);
                if (GUILayout.Button("Deselect All",   EditorStyles.toolbarButton, GUILayout.Width(80)))
                    entries.ForEach(e => e.selected = false);
                GUILayout.Space(6);
                showOnlyUnmatched = GUILayout.Toggle(showOnlyUnmatched,
                    "Show only unmatched", EditorStyles.toolbarButton, GUILayout.Width(130));
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(statusMessage))
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);

            if (entries.Count == 0)
            {
                GUILayout.Space(30);
                EditorGUILayout.LabelField(
                    "Press  Scan All  to find all TMP_Text objects that still need a LocalizedText component.",
                    EditorStyles.centeredGreyMiniLabel);
                return;
            }

            // Stats bar
            int matched  = entries.Count(e => e.IsMatched);
            int unmatched = entries.Count - matched;
            int selected = entries.Count(e => e.selected);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label($"Total: {entries.Count}", EditorStyles.miniLabel);
            GUILayout.Label($"✓ Matched: {matched}",  ColorLabel(Color.green));
            GUILayout.Label($"? Unmatched: {unmatched}", ColorLabel(new Color(1f, 0.6f, 0f)));
            GUILayout.Label($"Selected: {selected}", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Column headers
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("",         GUILayout.Width(22));
            GUILayout.Label("Status",   EditorStyles.toolbarButton, GUILayout.Width(65));
            GUILayout.Label("Source",   EditorStyles.toolbarButton, GUILayout.Width(160));
            GUILayout.Label("Text",     EditorStyles.toolbarButton, GUILayout.Width(210));
            GUILayout.Label("Loc Key",  EditorStyles.toolbarButton, GUILayout.Width(210));
            GUILayout.Label("VI (new keys)", EditorStyles.toolbarButton, GUILayout.MinWidth(80));
            GUILayout.Label("",         GUILayout.Width(22));
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            string lastGroup = null;
            foreach (var e in entries.OrderByDescending(x => x.IsMatched).ThenBy(x => x.prefabPath ?? "(Scene)"))
            {
                if (showOnlyUnmatched && e.IsMatched) continue;

                string groupLabel = e.prefabPath ?? "(Scene)";
                if (groupLabel != lastGroup)
                {
                    lastGroup = groupLabel;
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField(groupLabel, EditorStyles.boldLabel);
                }

                Color row = e.IsMatched
                    ? new Color(0.2f, 0.7f, 0.2f, 0.08f)
                    : new Color(0.9f, 0.5f, 0.1f, 0.08f);

                Rect lineRect = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(new Rect(lineRect.x, lineRect.y, lineRect.width, lineRect.height + 1), row);

                e.selected = EditorGUILayout.Toggle(e.selected, GUILayout.Width(22));

                GUILayout.Label(e.IsMatched ? "✓ Match" : "? New",
                    e.IsMatched ? ColorLabel(new Color(0.1f, 0.8f, 0.1f)) : ColorLabel(new Color(1f, 0.55f, 0f)),
                    GUILayout.Width(65));

                string src = e.goPath.Length > 22 ? "…" + e.goPath.Substring(e.goPath.Length - 21) : e.goPath;
                GUILayout.Label(src, EditorStyles.miniLabel, GUILayout.Width(160));

                string preview = e.text.Replace("\n", " ");
                if (preview.Length > 35) preview = preview.Substring(0, 33) + "…";
                GUILayout.Label(preview, EditorStyles.miniLabel, GUILayout.Width(210));

                if (e.IsMatched)
                {
                    GUILayout.Label(e.matchedKey, EditorStyles.miniLabel, GUILayout.Width(210));
                    GUILayout.Label("(auto)", EditorStyles.centeredGreyMiniLabel, GUILayout.MinWidth(80));
                }
                else
                {
                    e.customKey = EditorGUILayout.TextField(e.customKey, GUILayout.Width(210));
                    e.customVI  = EditorGUILayout.TextField(e.customVI,  GUILayout.MinWidth(80));
                }

                if (GUILayout.Button("▶", GUILayout.Width(22)))
                    Ping(e);

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            // Apply button
            EditorGUILayout.Space(4);
            int selCount    = entries.Count(e => e.selected);
            int selMatched  = entries.Count(e => e.selected && e.IsMatched);
            int selUnmatched = entries.Count(e => e.selected && !e.IsMatched);

            GUI.enabled = selCount > 0;
            if (GUILayout.Button(
                $"Apply LocalizedText to {selCount} selected   " +
                $"({selMatched} matched + {selUnmatched} new keys)",
                GUILayout.Height(32)))
            {
                ApplySelected();
            }
            GUI.enabled = true;
        }

        // ── Tab: Scene Setup ─────────────────────────────────────────────────
        private void DrawSceneSetup()
        {
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Scene & Project Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Run these once to prepare the scene for the localization system.",
                MessageType.Info);
            EditorGUILayout.Space(8);

            // 1. LocalizationManager in scene
            bool hasLM = FindObjectOfType<LocalizationManager>() != null;
            EditorGUI.BeginDisabledGroup(hasLM);
            if (GUILayout.Button(
                hasLM ? "✓  LocalizationManager already in scene"
                      : "➕  Add LocalizationManager GameObject to scene",
                GUILayout.Height(34)))
            {
                var go = new GameObject("LocalizationManager");
                go.AddComponent<LocalizationManager>();
                Undo.RegisterCreatedObjectUndo(go, "Add LocalizationManager");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log("[AutoLocalizer] Added LocalizationManager to scene.");
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(4);

            // 2. Script execution order
            if (GUILayout.Button(
                "⚙  Set LocalizationManager Script Execution Order = -200",
                GUILayout.Height(34)))
            {
                bool found = false;
                foreach (var ms in MonoImporter.GetAllRuntimeMonoScripts())
                {
                    if (ms.GetClass() == typeof(LocalizationManager))
                    {
                        MonoImporter.SetExecutionOrder(ms, -200);
                        Debug.Log("[AutoLocalizer] Execution order set to -200.");
                        EditorUtility.DisplayDialog("Done",
                            "LocalizationManager execution order set to -200.", "OK");
                        found = true;
                        break;
                    }
                }
                if (!found)
                    EditorUtility.DisplayDialog("Not found",
                        "Could not find LocalizationManager script.", "OK");
            }

            EditorGUILayout.Space(4);

            // 3. Reload JSON
            if (GUILayout.Button("↺  Reload en.json", GUILayout.Height(34)))
            {
                LoadJson();
                Repaint();
            }

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────
        private static GUIStyle ColorLabel(Color c)
        {
            var s = new GUIStyle(EditorStyles.miniLabel);
            s.normal.textColor = c;
            return s;
        }

        private static string Truncate(string s, int max) =>
            s.Length <= max ? s : s.Substring(0, max - 1) + "…";

        private static void Ping(LocalizeEntry e)
        {
            if (e.prefabPath != null)
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(e.prefabPath);
                EditorGUIUtility.PingObject(p);
                Selection.activeObject = p;
            }
            else if (e.go)
            {
                EditorGUIUtility.PingObject(e.go);
                Selection.activeObject = e.go;
            }
        }
    }
}
#endif
