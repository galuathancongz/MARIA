#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Luzart
{
    /// <summary>
    /// Tools > Localization > Auto Localizer (One-Click)
    ///
    /// Tab 1 - Scan & Edit:  Scan all prefabs + scenes for TMP_Text and serialized
    ///                        string fields (PostQuizBoard, ButtonClickQuiz, etc.).
    ///                        Shows Key / EN / VI columns. Inline edit. Save / Export / Import.
    /// Tab 2 - Apply:        Apply LocalizedText components to TMP_Text that don't have one.
    /// Tab 3 - Scene Setup:  Add LocalizationManager, set execution order, reload JSON.
    /// </summary>
    public class AutoLocalizerTool : EditorWindow
    {
        // ═══════════════════════════════════════════════════════════════════════
        //  STATE
        // ═══════════════════════════════════════════════════════════════════════
        private enum Tab { ScanEdit, Apply, ImportDiff, SceneSetup }
        private Tab currentTab = Tab.ScanEdit;

        private Vector2 scrollPos;
        private Vector2 scrollPos2;
        private string statusMessage = "";
        private string searchFilter = "";

        // ── Scan results ──────────────────────────────────────────────────────
        private List<LocEntry> allEntries = new List<LocEntry>();

        // ── JSON data (loaded from disk) ──────────────────────────────────────
        private Dictionary<string, string> enMap = new Dictionary<string, string>();   // key → EN
        private Dictionary<string, string> viMap = new Dictionary<string, string>();   // key → VI
        private Dictionary<string, string> reverseEn = new Dictionary<string, string>(); // EN value → key

        // ── Filters ───────────────────────────────────────────────────────────
        private bool showOnlyMissing = false;
        private bool showOnlyModified = false;

        // ── Apply tab state ───────────────────────────────────────────────────
        private List<ApplyEntry> applyEntries = new List<ApplyEntry>();
        private bool applyShowOnlyUnmatched = false;

        // ── Resizable columns (Scan & Edit tab) ─────────────────────────────
        //    0=Status  1=Source  2=Key  3=English  4=Vietnamese
        private float[] colW = { 55f, 130f, 210f, 230f, 230f };
        private static readonly string[] colHeaders = { "Status", "Source", "Key", "English", "Vietnamese" };
        private const float COL_MIN = 40f;

        // ── Resizable columns (Apply tab) ────────────────────────────────────
        //    0=Status  1=Path  2=Text  3=Key  4=VI
        private float[] colW2 = { 60f, 180f, 200f, 200f, 150f };
        private static readonly string[] colHeaders2 = { "Status", "Path", "Text", "Key", "VI (new)" };

        // ── Import Diff tab state ────────────────────────────────────────────
        private List<DiffEntry> diffEntries = new List<DiffEntry>();
        private Vector2 scrollPosDiff;
        private string diffLang = "";             // "VI" or "EN"
        private string diffFileName = "";
        private bool diffShowOnlyChanged = true;
        private string diffSearch = "";
        private float[] colWDiff = { 200f, 280f, 280f };
        private static readonly string[] colHeadersDiff = { "Key", "Current", "Imported" };

        // ── Column drag state (shared) ───────────────────────────────────────
        private int resizingCol = -1;
        private float[] resizingTarget = null;
        private float resizeStartMouseX;
        private float resizeStartWidth;

        // ═══════════════════════════════════════════════════════════════════════
        //  DATA CLASSES
        // ═══════════════════════════════════════════════════════════════════════
        private class LocEntry
        {
            public string text;           // original English text from prefab/scene
            public string prefabPath;     // prefab asset path (null = scene)
            public string source;         // e.g. "TMP_Text", "strQuestion", "listStr[2]"
            public string goPath;         // GameObject hierarchy path

            // Localization
            public string key;            // localization key (existing or suggested)
            public string enValue;        // English value (from JSON or = text)
            public string viValue;        // Vietnamese value (from JSON or empty)

            public bool existsInJson;     // true if key already in en.json
            public bool modified;         // user edited key/en/vi in this session
            public bool selected;
        }

        private enum DiffStatus { Same, Changed, NewKey, Removed }
        private class DiffEntry
        {
            public string key;
            public string currentValue;   // value in current JSON (null if new key)
            public string importedValue;  // value in imported file (null if removed)
            public string finalValue;     // editable — what will be saved
            public DiffStatus status;
            public bool accepted;         // user accepted this change
        }

        private class ApplyEntry
        {
            public GameObject go;
            public TMP_Text tmp;
            public string text;
            public string prefabPath;
            public string goPath;
            public string matchedKey;
            public string customKey;
            public string customVI;
            public bool selected;
            public bool isSceneObject;
            public bool IsMatched => !string.IsNullOrEmpty(matchedKey);
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  MENU
        // ═══════════════════════════════════════════════════════════════════════
        [MenuItem("Tools/Localization/Auto Localizer (One-Click)")]
        public static void Open()
        {
            var w = GetWindow<AutoLocalizerTool>("Auto Localizer");
            w.minSize = new Vector2(1050, 600);
            w.LoadAllJson();
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  JSON PATHS
        // ═══════════════════════════════════════════════════════════════════════
        private string EnJsonPath => Path.Combine(Application.dataPath,
            "_GameLuzart/Localization/Resources/Localization/en.json");
        private string ViJsonPath => Path.Combine(Application.dataPath,
            "_GameLuzart/Localization/Resources/Localization/vi.json");

        // ═══════════════════════════════════════════════════════════════════════
        //  LOAD JSON
        // ═══════════════════════════════════════════════════════════════════════
        private void LoadAllJson()
        {
            enMap.Clear();
            viMap.Clear();
            reverseEn.Clear();

            LoadJsonFile(EnJsonPath, enMap);
            LoadJsonFile(ViJsonPath, viMap);

            // Build reverse map (EN value → key)
            foreach (var kv in enMap)
            {
                if (string.IsNullOrEmpty(kv.Value)) continue;
                string exact = kv.Value.Trim();
                string norm = NormalizeText(kv.Value);
                if (!reverseEn.ContainsKey(exact)) reverseEn[exact] = kv.Key;
                if (!reverseEn.ContainsKey(norm)) reverseEn[norm] = kv.Key;
            }

            statusMessage = $"Loaded {enMap.Count} EN keys, {viMap.Count} VI keys";
        }

        private static void LoadJsonFile(string path, Dictionary<string, string> map)
        {
            if (!File.Exists(path)) return;
            var data = JsonUtility.FromJson<LocalizationData>(File.ReadAllText(path));
            if (data?.items == null) return;
            foreach (var item in data.items)
                if (!string.IsNullOrEmpty(item.key))
                    map[item.key] = item.value ?? "";
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text
                .Replace('\u2018', '\'').Replace('\u2019', '\'')
                .Replace('\u201C', '"').Replace('\u201D', '"')
                .Replace('\u2013', '-').Replace('\u2014', '-')
                .Replace("\u2026", "...").Replace("\u200B", "")
                .Trim();
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  SCAN ALL PREFABS + SCENES  (generic: every serialized string field)
        // ═══════════════════════════════════════════════════════════════════════

        // Unity internal namespaces — we only read TMP_Text.text from these
        private static readonly HashSet<string> UnityNamespaces = new HashSet<string>
        {
            "UnityEngine", "UnityEngine.UI", "UnityEngine.EventSystems",
            "UnityEngine.Events", "UnityEngine.Audio", "UnityEngine.Rendering",
            "UnityEngine.Animations", "UnityEngine.Playables", "UnityEngine.Video",
            "TMPro",
        };

        // Field names that are never localizable text
        private static readonly HashSet<string> SkipFields = new HashSet<string>
        {
            "m_Name", "m_Tag", "m_Script", "m_text", "m_TextComponent",
            "name", "tag", "guid", "GUID", "assetPath", "scenePath",
            "KEYLOAD", "loadPath", "animName", "animTrigger",
            "sceneName", "methodName", "functionName", "eventName",
            "prefabName", "resourcePath", "bundleName", "addressableKey",
            "shaderName", "propertyName", "parameterName", "layerName",
            "sortingLayerName", "className", "nameSpace", "assemblyName",
            "locKey", "localizationKey", "languageKey",
        };

        private static bool IsUnityType(System.Type type)
        {
            if (type == null) return false;
            string ns = type.Namespace;
            if (string.IsNullOrEmpty(ns)) return false;
            // Check exact match or starts with "UnityEngine." / "TMPro."
            return UnityNamespaces.Contains(ns) ||
                   ns.StartsWith("UnityEngine.") ||
                   ns.StartsWith("TMPro.");
        }

        private void ScanEverything()
        {
            if (enMap.Count == 0) LoadAllJson();
            allEntries.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Prefab",
                new[] { "Assets/_Game", "Assets/_GameLuzart" });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Scanning",
                    $"Prefabs {i + 1}/{guids.Length}", (float)(i + 1) / guids.Length);

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab) continue;

                // Per-prefab dedup: same text in one prefab = 1 entry,
                // but same text across different prefabs = separate entries
                var seenInPrefab = new HashSet<string>();
                ScanGameObject(prefab, path, seenInPrefab);
            }
            EditorUtility.ClearProgressBar();

            // Scan open scenes
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;
                var seenInScene = new HashSet<string>();
                foreach (var root in scene.GetRootGameObjects())
                    ScanGameObject(root, null, seenInScene);
            }

            allEntries = allEntries.OrderBy(e => e.existsInJson ? 1 : 0).ThenBy(e => e.key).ToList();

            int exists = allEntries.Count(e => e.existsInJson);
            int missing = allEntries.Count - exists;
            statusMessage = $"Scan complete: {allEntries.Count} strings " +
                            $"({exists} in JSON, {missing} missing)";
        }

        /// <summary>
        /// Scan all MonoBehaviour components on a GameObject and its children.
        /// For Unity built-in types, only read TMP_Text.text.
        /// For user scripts, read ALL public/[SerializeField] string fields
        /// and List&lt;string&gt; / string[] fields via Reflection.
        /// </summary>
        private void ScanGameObject(GameObject go, string prefabPath, HashSet<string> seen)
        {
            string suffix = prefabPath == null ? " (Scene)" : "";
            var components = go.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var comp in components)
            {
                if (comp == null) continue;
                var type = comp.GetType();

                // ── Skip LocalizedText entirely (it's the localization system itself) ──
                if (comp is LocalizedText) continue;

                // ── Unity / TMP built-in types: only TMP_Text.text ──────────
                if (IsUnityType(type))
                {
                    if (comp is TMP_Text tmp)
                    {
                        // Skip if already has LocalizedText — already managed
                        if (tmp.GetComponent<LocalizedText>() != null) continue;

                        string text = tmp.text?.Trim();
                        if (!ShouldSkip(text))
                            AddEntry(text, prefabPath, "TMP_Text" + suffix,
                                     GoPath(tmp.transform), seen);
                    }
                    continue;
                }

                // ── User scripts: scan all serialized string fields ─────────
                string goPath = GoPath(comp.transform);
                string typeName = type.Name;

                // Walk the type hierarchy to get fields from base classes too
                var allFields = GetAllSerializedFields(type);

                foreach (var field in allFields)
                {
                    // Skip known non-localizable field names
                    if (SkipFields.Contains(field.Name)) continue;

                    // ── string field ─────────────────────────────────────────
                    if (field.FieldType == typeof(string))
                    {
                        string val = (field.GetValue(comp) as string)?.Trim();
                        if (ShouldSkip(val)) continue;
                        AddEntry(val, prefabPath,
                                 $"{typeName}.{field.Name}" + suffix, goPath, seen);
                    }
                    // ── List<string> field ───────────────────────────────────
                    else if (field.FieldType == typeof(List<string>))
                    {
                        var list = field.GetValue(comp) as List<string>;
                        if (list == null) continue;
                        for (int j = 0; j < list.Count; j++)
                        {
                            string val = list[j]?.Trim();
                            if (ShouldSkip(val)) continue;
                            AddEntry(val, prefabPath,
                                     $"{typeName}.{field.Name}[{j}]" + suffix,
                                     goPath, seen);
                        }
                    }
                    // ── string[] field ───────────────────────────────────────
                    else if (field.FieldType == typeof(string[]))
                    {
                        var arr = field.GetValue(comp) as string[];
                        if (arr == null) continue;
                        for (int j = 0; j < arr.Length; j++)
                        {
                            string val = arr[j]?.Trim();
                            if (ShouldSkip(val)) continue;
                            AddEntry(val, prefabPath,
                                     $"{typeName}.{field.Name}[{j}]" + suffix,
                                     goPath, seen);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all serialized fields from a type and its base classes
        /// (public fields + private fields with [SerializeField]).
        /// Stops at MonoBehaviour.
        /// </summary>
        private static List<FieldInfo> GetAllSerializedFields(System.Type type)
        {
            var fields = new List<FieldInfo>();
            var visited = new HashSet<string>(); // avoid duplicate names from overrides

            while (type != null && type != typeof(MonoBehaviour) && type != typeof(Component))
            {
                var declared = type.GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var f in declared)
                {
                    if (visited.Contains(f.Name)) continue;
                    visited.Add(f.Name);

                    // Public fields are serialized by default
                    // Private fields need [SerializeField]
                    if (f.IsPublic || f.IsDefined(typeof(SerializeField), true))
                        fields.Add(f);
                }
                type = type.BaseType;
            }
            return fields;
        }

        private void AddEntry(string text, string prefabPath, string source, string goPath, HashSet<string> seen)
        {
            if (ShouldSkip(text)) return;
            string trimmed = text.Trim();

            // Deduplicate by text
            if (seen.Contains(trimmed)) return;
            seen.Add(trimmed);

            // Try to find existing key
            string existingKey = null;
            if (reverseEn.TryGetValue(trimmed, out string k1))
                existingKey = k1;
            else if (reverseEn.TryGetValue(NormalizeText(trimmed), out string k2))
                existingKey = k2;

            bool exists = existingKey != null;
            string key = existingKey ?? SuggestKey(trimmed, prefabPath);

            allEntries.Add(new LocEntry
            {
                text = trimmed,
                prefabPath = prefabPath,
                source = source,
                goPath = goPath,
                key = key,
                enValue = exists ? (enMap.TryGetValue(key, out string ev) ? ev : trimmed) : trimmed,
                viValue = exists ? (viMap.TryGetValue(key, out string vv) ? vv : "") : "",
                existsInJson = exists,
                modified = false,
                selected = !exists  // auto-select missing
            });
        }

        private static bool ShouldSkip(string t)
        {
            if (string.IsNullOrWhiteSpace(t) || t.Length <= 1) return true;
            // Rich text tags
            if (t.StartsWith("<sprite") || t.StartsWith("<color") || t.StartsWith("<size")) return true;
            // Pure numbers / symbols
            if (t.All(c => char.IsDigit(c) || c == '.' || c == '%' || c == ':' || c == '/' || c == '-' || c == '+' || c == ' '))
                return true;
            // Asset paths (contain "/" or "\" with file extension)
            if ((t.Contains('/') || t.Contains('\\')) && (t.EndsWith(".asset") || t.EndsWith(".prefab") ||
                t.EndsWith(".png") || t.EndsWith(".jpg") || t.EndsWith(".mat") || t.EndsWith(".cs") ||
                t.EndsWith(".shader") || t.EndsWith(".json") || t.EndsWith(".txt") || t.EndsWith(".anim") ||
                t.EndsWith(".controller") || t.EndsWith(".unity") || t.EndsWith(".meta")))
                return true;
            // GUID-like strings (32+ hex chars)
            if (t.Length >= 32 && t.All(c => "0123456789abcdefABCDEF-".Contains(c)))
                return true;
            // Color hex codes
            if (t.StartsWith("#") && t.Length <= 9 && t.Skip(1).All(c => "0123456789abcdefABCDEF".Contains(c)))
                return true;
            // Single word that looks like a code identifier (PascalCase/camelCase, no spaces, no accents)
            // Only skip if very short and no spaces — longer strings might be titles
            if (t.Length <= 3 && !t.Contains(' ')) return true;
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
                if      (prefabPath.Contains("Level4") || prefabPath.Contains("Level 4") || prefabPath.Contains("Quiz")) cat = "level4";
                else if (prefabPath.Contains("Level1") || prefabPath.Contains("Level 1")) cat = "level1";
                else if (prefabPath.Contains("Level2") || prefabPath.Contains("Level 2")) cat = "level2";
                else if (prefabPath.Contains("Level3") || prefabPath.Contains("Level 3")) cat = "level3";
                else if (prefabPath.Contains("Level5") || prefabPath.Contains("Level 5")) cat = "level5";
                else if (prefabPath.Contains("Menu"))      cat = "menu";
                else if (prefabPath.Contains("Tut"))       cat = "tutorial";
                else if (prefabPath.Contains("Profile"))   cat = "profile";
                else if (prefabPath.Contains("Settings"))  cat = "settings";
                else if (prefabPath.Contains("Persona"))   cat = "persona";
            }
            string clean = text.ToLowerInvariant()
                .Replace(" ", "_").Replace("'", "").Replace("\"", "").Replace("\n", "_")
                .Replace(":", "").Replace("?", "").Replace("!", "").Replace(",", "").Replace(".", "")
                .Replace("/", "_").Replace("\\", "_").Replace("(", "").Replace(")", "");
            if (clean.Length > 40) clean = clean.Substring(0, 40);
            clean = clean.Trim('_');
            return $"{cat}.{clean}";
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  SAVE  (write edited keys/values back to en.json & vi.json)
        // ═══════════════════════════════════════════════════════════════════════
        private void SaveToJson()
        {
            // Merge allEntries into existing maps
            int newCount = 0;
            int updateCount = 0;
            var usedKeys = new HashSet<string>(enMap.Keys);

            foreach (var e in allEntries)
            {
                if (string.IsNullOrWhiteSpace(e.key)) continue;

                // Resolve duplicate key
                string finalKey = e.key;
                if (!e.existsInJson && usedKeys.Contains(finalKey))
                {
                    int counter = 2;
                    while (usedKeys.Contains(finalKey + "_" + counter)) counter++;
                    finalKey = finalKey + "_" + counter;
                    e.key = finalKey;
                }

                if (e.existsInJson)
                {
                    // Update existing — save if modified OR if imported
                    if (e.modified)
                    {
                        enMap[e.key] = e.enValue ?? "";
                        viMap[e.key] = e.viValue ?? "";
                        updateCount++;
                    }
                }
                else if (e.selected || e.modified)
                {
                    // Add new
                    enMap[finalKey] = e.enValue;
                    viMap[finalKey] = string.IsNullOrEmpty(e.viValue) ? e.enValue : e.viValue;
                    usedKeys.Add(finalKey);
                    e.existsInJson = true;
                    e.modified = false;
                    newCount++;
                }
            }

            WriteJsonFile(EnJsonPath, enMap);
            WriteJsonFile(ViJsonPath, viMap);

            LoadAllJson(); // reload reverse maps

            statusMessage = $"Saved! {newCount} new keys added, {updateCount} updated.";
            Debug.Log("[AutoLocalizer] " + statusMessage);
            EditorUtility.DisplayDialog("Saved", statusMessage, "OK");
            AssetDatabase.Refresh();
        }

        private static void WriteJsonFile(string path, Dictionary<string, string> map)
        {
            // Group by prefix for readability
            var groups = map.OrderBy(kv => kv.Key)
                .GroupBy(kv => kv.Key.Contains('.') ? kv.Key.Substring(0, kv.Key.IndexOf('.')) : "other");

            var sb = new StringBuilder();
            sb.AppendLine("{ \"items\": [");
            bool first = true;
            foreach (var group in groups)
            {
                if (!first) sb.AppendLine();
                foreach (var kv in group)
                {
                    if (!first) sb.AppendLine(",");
                    string v = kv.Value
                        .Replace("\\", "\\\\").Replace("\"", "\\\"")
                        .Replace("\n", "\\n").Replace("\r", "");
                    sb.Append($"  {{\"key\": \"{kv.Key}\", \"value\": \"{v}\"}}");
                    first = false;
                }
            }
            sb.AppendLine();
            sb.AppendLine("]}");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  EXPORT  (en_fix.json, vi_fix.json with "key": "value" format)
        // ═══════════════════════════════════════════════════════════════════════
        private void ExportFixFiles()
        {
            string folder = EditorUtility.SaveFolderPanel("Export to folder", Application.dataPath, "");
            if (string.IsNullOrEmpty(folder)) return;

            // Build a COMPLETE list: allEntries + any JSON-only keys not in scan
            var exportMap = new Dictionary<string, (string en, string vi)>();

            // 1. From allEntries (scan results)
            foreach (var e in allEntries)
            {
                if (string.IsNullOrWhiteSpace(e.key)) continue;
                exportMap[e.key] = (e.enValue ?? "", e.viValue ?? "");
            }

            // 2. From existing en.json keys not found in scan
            foreach (var kv in enMap)
            {
                if (exportMap.ContainsKey(kv.Key)) continue;
                string vi = viMap.TryGetValue(kv.Key, out string v) ? v : "";
                exportMap[kv.Key] = (kv.Value, vi);
            }

            var sorted = exportMap.OrderBy(kv => kv.Key).ToList();

            // en_fix.json
            var sbEn = new StringBuilder();
            sbEn.AppendLine("{");
            for (int i = 0; i < sorted.Count; i++)
            {
                sbEn.Append($"  \"{sorted[i].Key}\": \"{EscapeJsonValue(sorted[i].Value.en)}\"");
                if (i < sorted.Count - 1) sbEn.Append(",");
                sbEn.AppendLine();
            }
            sbEn.AppendLine("}");
            File.WriteAllText(Path.Combine(folder, "en_fix.json"), sbEn.ToString(), Encoding.UTF8);

            // vi_fix.json  (if VI is empty, use EN as placeholder)
            var sbVi = new StringBuilder();
            sbVi.AppendLine("{");
            for (int i = 0; i < sorted.Count; i++)
            {
                string vi = string.IsNullOrEmpty(sorted[i].Value.vi)
                    ? sorted[i].Value.en : sorted[i].Value.vi;
                sbVi.Append($"  \"{sorted[i].Key}\": \"{EscapeJsonValue(vi)}\"");
                if (i < sorted.Count - 1) sbVi.Append(",");
                sbVi.AppendLine();
            }
            sbVi.AppendLine("}");
            File.WriteAllText(Path.Combine(folder, "vi_fix.json"), sbVi.ToString(), Encoding.UTF8);

            statusMessage = $"Exported {sorted.Count} entries to {folder} (scan: {allEntries.Count} + JSON-only: {sorted.Count - allEntries.Count})";
            EditorUtility.DisplayDialog("Exported",
                $"en_fix.json: {sorted.Count} entries\n" +
                $"vi_fix.json: {sorted.Count} entries\n\n{folder}", "OK");
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  IMPORT  (open file → build diff → switch to Import Diff tab)
        // ═══════════════════════════════════════════════════════════════════════
        private void ImportJsonFile()
        {
            string filePath = EditorUtility.OpenFilePanel("Import JSON (key:value)", Application.dataPath, "json");
            if (string.IsNullOrEmpty(filePath)) return;

            string json = File.ReadAllText(filePath);
            var imported = ParseKeyValueJson(json);
            if (imported == null || imported.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "Could not parse JSON.\nExpected: {\"key\": \"value\", ...}\nor {\"items\": [{\"key\":..., \"value\":...}]}", "OK");
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();
            bool isVi = fileName.Contains("vi") || fileName.Contains("vn");

            string[] options = isVi
                ? new[] { "Compare with VI (vi.json)", "Compare with EN (en.json)", "Cancel" }
                : new[] { "Compare with EN (en.json)", "Compare with VI (vi.json)", "Cancel" };

            int choice = EditorUtility.DisplayDialogComplex("Import Target",
                $"Found {imported.Count} keys in {Path.GetFileName(filePath)}.\n\n" +
                $"Which language file should this compare against?",
                options[0], options[1], options[2]);

            if (choice == 2) return;

            bool targetVi = isVi ? (choice == 0) : (choice == 1);
            var currentMap = targetVi ? viMap : enMap;
            diffLang = targetVi ? "VI" : "EN";
            diffFileName = Path.GetFileName(filePath);

            // Build diff entries
            BuildDiff(currentMap, imported);

            // Switch to Import Diff tab
            currentTab = Tab.ImportDiff;
        }

        private void BuildDiff(Dictionary<string, string> currentMap, Dictionary<string, string> imported)
        {
            diffEntries.Clear();
            var allKeys = new HashSet<string>(currentMap.Keys);
            allKeys.UnionWith(imported.Keys);

            foreach (string key in allKeys.OrderBy(k => k))
            {
                bool inCurrent = currentMap.TryGetValue(key, out string curVal);
                bool inImport = imported.TryGetValue(key, out string impVal);

                DiffStatus status;
                if (inCurrent && inImport)
                    status = (curVal == impVal) ? DiffStatus.Same : DiffStatus.Changed;
                else if (inImport)
                    status = DiffStatus.NewKey;
                else
                    status = DiffStatus.Removed;

                diffEntries.Add(new DiffEntry
                {
                    key = key,
                    currentValue = inCurrent ? curVal : null,
                    importedValue = inImport ? impVal : null,
                    finalValue = inImport ? impVal : curVal,     // default: take imported
                    status = status,
                    accepted = (status == DiffStatus.Changed || status == DiffStatus.NewKey)
                });
            }

            int changed = diffEntries.Count(d => d.status == DiffStatus.Changed);
            int newKeys = diffEntries.Count(d => d.status == DiffStatus.NewKey);
            int same = diffEntries.Count(d => d.status == DiffStatus.Same);
            statusMessage = $"Diff: {changed} changed, {newKeys} new, {same} unchanged (total {diffEntries.Count})";
        }

        /// <summary>
        /// Apply accepted diff changes INTO allEntries (Scan & Edit tab).
        /// Does NOT write to file — user must click "Save to JSON" afterwards.
        /// </summary>
        private void ApplyDiff()
        {
            bool targetVi = diffLang == "VI";

            // Build lookup for allEntries by key
            var entryByKey = new Dictionary<string, LocEntry>();
            foreach (var e in allEntries)
                if (!string.IsNullOrWhiteSpace(e.key))
                    entryByKey[e.key] = e;

            int updated = 0;
            int added = 0;

            foreach (var d in diffEntries)
            {
                if (!d.accepted) continue;
                if (d.status == DiffStatus.Same) continue;

                string val = d.finalValue ?? "";

                if (entryByKey.TryGetValue(d.key, out LocEntry existing))
                {
                    // Update existing entry
                    if (targetVi)
                        existing.viValue = val;
                    else
                        existing.enValue = val;
                    existing.modified = true;
                    updated++;
                }
                else
                {
                    // New key not in scan — add to allEntries
                    string enVal = targetVi
                        ? (enMap.TryGetValue(d.key, out string ev) ? ev : d.key)
                        : val;
                    string viVal = targetVi
                        ? val
                        : (viMap.TryGetValue(d.key, out string vv) ? vv : "");

                    var newEntry = new LocEntry
                    {
                        text = enVal,
                        prefabPath = null,
                        source = "(imported)",
                        goPath = "",
                        key = d.key,
                        enValue = enVal,
                        viValue = viVal,
                        existsInJson = enMap.ContainsKey(d.key),
                        modified = true,
                        selected = true
                    };
                    allEntries.Add(newEntry);
                    added++;
                }
            }

            // Sort
            allEntries = allEntries.OrderBy(e => e.existsInJson && !e.modified ? 1 : 0)
                                   .ThenBy(e => e.key).ToList();

            // Clear diff and switch to Scan & Edit
            diffEntries.Clear();
            currentTab = Tab.ScanEdit;

            statusMessage = $"Import applied: {updated} updated + {added} new entries added to table. " +
                            $"Click 'Save to JSON' to write to en.json & vi.json.";
            Debug.Log("[AutoLocalizer] " + statusMessage);
            EditorUtility.DisplayDialog("Import Applied",
                $"{updated} entries updated\n{added} new entries added\n\n" +
                "Changes are in the Scan & Edit table now.\n" +
                "Click 'SAVE to en.json & vi.json' when ready.", "OK");
        }

        /// <summary>
        /// Parse simple {"key": "value", ...} JSON format.
        /// Also supports the items array format as fallback.
        /// </summary>
        private static Dictionary<string, string> ParseKeyValueJson(string json)
        {
            var result = new Dictionary<string, string>();

            // Try items array format first
            try
            {
                var data = JsonUtility.FromJson<LocalizationData>(json);
                if (data?.items != null && data.items.Length > 0)
                {
                    foreach (var item in data.items)
                        if (!string.IsNullOrEmpty(item.key))
                            result[item.key] = item.value ?? "";
                    return result;
                }
            }
            catch { }

            // Parse simple key:value JSON manually
            // Handles: { "key1": "value1", "key2": "value2" }
            json = json.Trim();
            if (!json.StartsWith("{") || !json.EndsWith("}")) return null;
            json = json.Substring(1, json.Length - 2);

            int i = 0;
            while (i < json.Length)
            {
                // Find key
                int keyStart = json.IndexOf('"', i);
                if (keyStart < 0) break;
                int keyEnd = FindClosingQuote(json, keyStart + 1);
                if (keyEnd < 0) break;
                string key = UnescapeJsonValue(json.Substring(keyStart + 1, keyEnd - keyStart - 1));

                // Find colon
                int colon = json.IndexOf(':', keyEnd + 1);
                if (colon < 0) break;

                // Find value
                int valStart = json.IndexOf('"', colon + 1);
                if (valStart < 0) break;
                int valEnd = FindClosingQuote(json, valStart + 1);
                if (valEnd < 0) break;
                string value = UnescapeJsonValue(json.Substring(valStart + 1, valEnd - valStart - 1));

                result[key] = value;
                i = valEnd + 1;
            }

            return result.Count > 0 ? result : null;
        }

        private static int FindClosingQuote(string s, int start)
        {
            for (int i = start; i < s.Length; i++)
            {
                if (s[i] == '\\') { i++; continue; }
                if (s[i] == '"') return i;
            }
            return -1;
        }

        private static string EscapeJsonValue(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                    .Replace("\n", "\\n").Replace("\r", "").Replace("\t", "\\t");
        }

        private static string UnescapeJsonValue(string s)
        {
            if (s == null) return "";
            return s.Replace("\\n", "\n").Replace("\\t", "\t")
                    .Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  GUI - MAIN
        // ═══════════════════════════════════════════════════════════════════════
        private void OnGUI()
        {
            // ── Global: handle column drag (MouseDrag / MouseUp anywhere) ────
            HandleGlobalDrag();

            // Tab bar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawTabButton("  Scan & Edit  ", Tab.ScanEdit);
            DrawTabButton("  Apply LocalizedText  ", Tab.Apply);

            // Import Diff tab — only show when diff data exists
            if (diffEntries.Count > 0)
                DrawTabButton("  Import Diff  ", Tab.ImportDiff);

            DrawTabButton("  Scene Setup  ", Tab.SceneSetup);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            switch (currentTab)
            {
                case Tab.ScanEdit:   DrawScanEdit(); break;
                case Tab.Apply:      DrawApply(); break;
                case Tab.ImportDiff: DrawImportDiff(); break;
                case Tab.SceneSetup: DrawSceneSetup(); break;
            }
        }

        private void DrawTabButton(string label, Tab tab)
        {
            bool active = currentTab == tab;
            if (GUILayout.Toggle(active, label, EditorStyles.toolbarButton) && !active)
                currentTab = tab;
        }

        // ─── Column resize infrastructure ────────────────────────────────────

        private void HandleGlobalDrag()
        {
            Event e = Event.current;
            if (resizingCol < 0 || resizingTarget == null) return;

            if (e.type == EventType.MouseDrag)
            {
                float delta = e.mousePosition.x - resizeStartMouseX;
                resizingTarget[resizingCol] = Mathf.Max(COL_MIN, resizeStartWidth + delta);
                e.Use();
                Repaint();
            }
            else if (e.type == EventType.MouseUp)
            {
                resizingCol = -1;
                resizingTarget = null;
                e.Use();
            }
        }

        /// <summary>
        /// Draw a row of resizable column headers. Returns rects for alignment reference.
        /// </summary>
        private void DrawResizableHeaders(float[] widths, string[] headers)
        {
            var btnStyle = EditorStyles.toolbarButton;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("", GUILayout.Width(22)); // checkbox

            for (int c = 0; c < headers.Length; c++)
            {
                bool isLast = (c == headers.Length - 1);

                if (isLast)
                    GUILayout.Label(headers[c], btnStyle, GUILayout.MinWidth(widths[c]));
                else
                    GUILayout.Label(headers[c], btnStyle, GUILayout.Width(widths[c]));

                // Draw resize handle on the right edge of each column (except last)
                if (!isLast)
                {
                    Rect labelRect = GUILayoutUtility.GetLastRect();
                    Rect handle = new Rect(labelRect.xMax - 3, labelRect.y, 6, labelRect.height);
                    EditorGUIUtility.AddCursorRect(handle, MouseCursor.ResizeHorizontal);

                    if (Event.current.type == EventType.MouseDown && handle.Contains(Event.current.mousePosition))
                    {
                        resizingCol = c;
                        resizingTarget = widths;
                        resizeStartMouseX = Event.current.mousePosition.x;
                        resizeStartWidth = widths[c];
                        Event.current.Use();
                    }
                }
            }

            GUILayout.Label("", GUILayout.Width(22)); // ping/action button
            EditorGUILayout.EndHorizontal();
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  TAB 1: SCAN & EDIT
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawScanEdit()
        {
            // ── Toolbar row 1: Actions ───────────────────────────────────────
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var btnStyle = EditorStyles.toolbarButton;
            if (GUILayout.Button("Scan All Prefabs", btnStyle, GUILayout.Width(120)))
                ScanEverything();

            GUILayout.Space(8);

            GUI.enabled = allEntries.Count > 0;
            if (GUILayout.Button("Save to JSON", btnStyle, GUILayout.Width(100)))
                SaveToJson();
            if (GUILayout.Button("Export (en_fix / vi_fix)", btnStyle, GUILayout.Width(160)))
                ExportFixFiles();
            if (GUILayout.Button("Import JSON...", btnStyle, GUILayout.Width(110)))
                ImportJsonFile();
            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // ── Toolbar row 2: Filters & selection ───────────────────────────
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (allEntries.Count > 0)
            {
                if (GUILayout.Button("Select All", btnStyle, GUILayout.Width(70)))
                    FilteredEntries().ForEach(e => e.selected = true);
                if (GUILayout.Button("Select Missing", btnStyle, GUILayout.Width(95)))
                {
                    allEntries.ForEach(e => e.selected = false);
                    allEntries.Where(e => !e.existsInJson).ToList().ForEach(e => e.selected = true);
                }
                if (GUILayout.Button("Deselect All", btnStyle, GUILayout.Width(80)))
                    allEntries.ForEach(e => e.selected = false);

                GUILayout.Space(8);
                showOnlyMissing = GUILayout.Toggle(showOnlyMissing, "Only Missing", btnStyle, GUILayout.Width(90));
                showOnlyModified = GUILayout.Toggle(showOnlyModified, "Only Modified", btnStyle, GUILayout.Width(95));
            }

            GUILayout.FlexibleSpace();
            searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(220));
            EditorGUILayout.EndHorizontal();

            // ── Status ───────────────────────────────────────────────────────
            if (!string.IsNullOrEmpty(statusMessage))
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);

            if (allEntries.Count == 0)
            {
                GUILayout.Space(30);
                EditorGUILayout.LabelField(
                    "Press 'Scan All Prefabs' to find all text strings in the project.",
                    EditorStyles.centeredGreyMiniLabel);
                return;
            }

            // ── Stats bar ────────────────────────────────────────────────────
            int total = allEntries.Count;
            int exists = allEntries.Count(e => e.existsInJson);
            int missing = total - exists;
            int modified = allEntries.Count(e => e.modified);
            int selected = allEntries.Count(e => e.selected);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label($"Total: {total}", EditorStyles.miniLabel);
            GUILayout.Label($"In JSON: {exists}", ColorLabel(new Color(0.1f, 0.8f, 0.1f)));
            GUILayout.Label($"Missing: {missing}", ColorLabel(new Color(1f, 0.5f, 0f)));
            GUILayout.Label($"Modified: {modified}", ColorLabel(new Color(0.3f, 0.6f, 1f)));
            GUILayout.Label($"Selected: {selected}", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // ── Column headers (resizable) ──────────────────────────────────
            DrawResizableHeaders(colW, colHeaders);

            // ── Rows ─────────────────────────────────────────────────────────
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            var filtered = FilteredEntries();
            string lastPrefab = null;

            foreach (var e in filtered)
            {
                // Prefab group header
                string group = e.prefabPath ?? "(Scene)";
                if (group != lastPrefab)
                {
                    lastPrefab = group;
                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField(group, EditorStyles.boldLabel);
                }

                Color rowColor = e.existsInJson
                    ? (e.modified ? new Color(0.3f, 0.5f, 1f, 0.08f) : new Color(0.2f, 0.7f, 0.2f, 0.06f))
                    : new Color(1f, 0.5f, 0f, 0.08f);

                Rect r = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, r.height + 1), rowColor);

                // Checkbox
                e.selected = EditorGUILayout.Toggle(e.selected, GUILayout.Width(22));

                // Status
                string st = e.existsInJson ? (e.modified ? "MOD" : "OK") : "NEW";
                Color stColor = e.existsInJson
                    ? (e.modified ? new Color(0.3f, 0.6f, 1f) : new Color(0.1f, 0.75f, 0.1f))
                    : new Color(1f, 0.55f, 0f);
                GUILayout.Label(st, ColorLabel(stColor), GUILayout.Width(colW[0]));

                // Source type
                GUILayout.Label(e.source, EditorStyles.miniLabel, GUILayout.Width(colW[1]));

                // Key (editable)
                EditorGUI.BeginChangeCheck();
                e.key = EditorGUILayout.TextField(e.key, GUILayout.Width(colW[2]));
                if (EditorGUI.EndChangeCheck()) e.modified = true;

                // EN (editable)
                EditorGUI.BeginChangeCheck();
                e.enValue = EditorGUILayout.TextField(e.enValue, GUILayout.Width(colW[3]));
                if (EditorGUI.EndChangeCheck()) e.modified = true;

                // VI (editable)
                EditorGUI.BeginChangeCheck();
                e.viValue = EditorGUILayout.TextField(e.viValue, GUILayout.MinWidth(colW[4]));
                if (EditorGUI.EndChangeCheck()) e.modified = true;

                // Ping
                if (GUILayout.Button("P", GUILayout.Width(22)))
                {
                    if (e.prefabPath != null)
                    {
                        var p = AssetDatabase.LoadAssetAtPath<GameObject>(e.prefabPath);
                        EditorGUIUtility.PingObject(p);
                        Selection.activeObject = p;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            // ── Bottom: Save button ──────────────────────────────────────────
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();

            int selMissing = allEntries.Count(e => e.selected && !e.existsInJson);
            int selModified = allEntries.Count(e => e.modified);

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            GUI.enabled = selMissing > 0 || selModified > 0;
            if (GUILayout.Button(
                $"SAVE to en.json & vi.json  ({selMissing} new + {selModified} modified)",
                GUILayout.Height(34)))
            {
                SaveToJson();
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private List<LocEntry> FilteredEntries()
        {
            IEnumerable<LocEntry> q = allEntries;

            if (showOnlyMissing) q = q.Where(e => !e.existsInJson);
            if (showOnlyModified) q = q.Where(e => e.modified);

            if (!string.IsNullOrEmpty(searchFilter))
            {
                string f = searchFilter.ToLowerInvariant();
                q = q.Where(e =>
                    (e.key != null && e.key.ToLower().Contains(f)) ||
                    (e.enValue != null && e.enValue.ToLower().Contains(f)) ||
                    (e.viValue != null && e.viValue.ToLower().Contains(f)) ||
                    (e.text != null && e.text.ToLower().Contains(f)) ||
                    (e.prefabPath != null && e.prefabPath.ToLower().Contains(f)));
            }

            return q.ToList();
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  TAB 2: APPLY LocalizedText components
        // ═══════════════════════════════════════════════════════════════════════
        private void ScanForApply()
        {
            if (enMap.Count == 0) LoadAllJson();
            applyEntries.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Prefab",
                new[] { "Assets/_Game", "Assets/_GameLuzart" });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Scanning for Apply",
                    $"{i + 1}/{guids.Length}", (float)(i + 1) / guids.Length);

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab) continue;

                foreach (var tmp in prefab.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (tmp.GetComponent<LocalizedText>() != null) continue;

                    // Skip TMP managed by PostQuizBoard (Loc.T() handles them)
                    var board = tmp.GetComponentInParent<PostQuizBoard>();
                    if (board != null && (board.txtQuestion == tmp || board.txtSections == tmp)) continue;
                    var btnQuiz = tmp.GetComponentInParent<ButtonClickQuiz>();
                    if (btnQuiz != null) continue;

                    string text = tmp.text?.Trim();
                    if (ShouldSkip(text)) continue;

                    string key = null;
                    if (!reverseEn.TryGetValue(text, out key))
                        reverseEn.TryGetValue(NormalizeText(text), out key);

                    applyEntries.Add(new ApplyEntry
                    {
                        go = tmp.gameObject,
                        tmp = tmp,
                        text = text,
                        prefabPath = path,
                        goPath = GoPath(tmp.transform),
                        matchedKey = key,
                        customKey = key ?? SuggestKey(text, path),
                        customVI = key != null ? "" : text,
                        isSceneObject = false,
                        selected = true
                    });
                }
            }
            EditorUtility.ClearProgressBar();

            // Scene objects
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (tmp.GetComponent<LocalizedText>() != null) continue;
                        var board = tmp.GetComponentInParent<PostQuizBoard>();
                        if (board != null && (board.txtQuestion == tmp || board.txtSections == tmp)) continue;
                        var btnQuiz = tmp.GetComponentInParent<ButtonClickQuiz>();
                        if (btnQuiz != null) continue;

                        string text = tmp.text?.Trim();
                        if (ShouldSkip(text)) continue;

                        string key = null;
                        if (!reverseEn.TryGetValue(text, out key))
                            reverseEn.TryGetValue(NormalizeText(text), out key);

                        applyEntries.Add(new ApplyEntry
                        {
                            go = tmp.gameObject,
                            tmp = tmp,
                            text = text,
                            prefabPath = null,
                            goPath = GoPath(tmp.transform),
                            matchedKey = key,
                            customKey = key ?? SuggestKey(text, null),
                            customVI = key != null ? "" : text,
                            isSceneObject = true,
                            selected = true
                        });
                    }
                }
            }

            int matched = applyEntries.Count(e => e.IsMatched);
            int unmatched = applyEntries.Count - matched;
            statusMessage = $"Found {applyEntries.Count} TMP_Text needing LocalizedText: " +
                            $"{matched} matched, {unmatched} new. " +
                            "(PostQuizBoard/ButtonClickQuiz texts are skipped — they use Loc.T())";
        }

        private void ApplySelectedComponents()
        {
            var toApply = applyEntries.Where(e => e.selected).ToList();
            if (toApply.Count == 0)
            {
                EditorUtility.DisplayDialog("Nothing", "Select at least one entry.", "OK");
                return;
            }

            // Add unmatched keys to JSON
            var newOnes = toApply.Where(e => !e.IsMatched).ToList();
            if (newOnes.Count > 0)
            {
                var usedKeys = new HashSet<string>(enMap.Keys);
                foreach (var e in newOnes)
                {
                    string k = e.customKey;
                    if (usedKeys.Contains(k))
                    {
                        int c = 2;
                        while (usedKeys.Contains(k + "_" + c)) c++;
                        k = k + "_" + c;
                        e.customKey = k;
                    }
                    enMap[k] = e.text;
                    viMap[k] = string.IsNullOrEmpty(e.customVI) ? e.text : e.customVI;
                    usedKeys.Add(k);
                }
                WriteJsonFile(EnJsonPath, enMap);
                WriteJsonFile(ViJsonPath, viMap);
                LoadAllJson();
            }

            // Apply LocalizedText to prefabs
            int applied = 0, saved = 0;
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
                    applied++;
                }
                if (dirty) { PrefabUtility.SaveAsPrefabAsset(root, group.Key); saved++; }
                PrefabUtility.UnloadPrefabContents(root);
            }

            // Scene objects
            foreach (var entry in toApply.Where(e => e.isSceneObject))
            {
                if (!entry.go || entry.go.GetComponent<LocalizedText>() != null) continue;
                Undo.RecordObject(entry.go, "Add LocalizedText");
                var comp = Undo.AddComponent<LocalizedText>(entry.go);
                SetLocKey(comp, entry.IsMatched ? entry.matchedKey : entry.customKey);
                EditorSceneManager.MarkSceneDirty(entry.go.scene);
                applied++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            applyEntries.RemoveAll(e => toApply.Contains(e));

            statusMessage = $"Applied LocalizedText to {applied} objects ({saved} prefabs saved). " +
                            (newOnes.Count > 0 ? $"{newOnes.Count} new keys added." : "");
            EditorUtility.DisplayDialog("Done", statusMessage, "OK");
        }

        private static void SetLocKey(LocalizedText comp, string key)
        {
            var so = new SerializedObject(comp);
            var prop = so.FindProperty("locKey");
            if (prop != null) { prop.stringValue = key; so.ApplyModifiedPropertiesWithoutUndo(); }
        }

        private static TMP_Text MatchTmp(TMP_Text[] candidates, string text, string goPath)
        {
            foreach (var t in candidates)
                if (t.text?.Trim() == text && GoPath(t.transform) == goPath && !t.GetComponent<LocalizedText>())
                    return t;
            foreach (var t in candidates)
                if (t.text?.Trim() == text && !t.GetComponent<LocalizedText>())
                    return t;
            return null;
        }

        private void DrawApply()
        {
            var btnStyle = EditorStyles.toolbarButton;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Scan", btnStyle, GUILayout.Width(60))) ScanForApply();
            if (applyEntries.Count > 0)
            {
                GUILayout.Space(6);
                if (GUILayout.Button("Select All", btnStyle, GUILayout.Width(70)))
                    applyEntries.ForEach(e => e.selected = true);
                if (GUILayout.Button("Deselect All", btnStyle, GUILayout.Width(80)))
                    applyEntries.ForEach(e => e.selected = false);
                GUILayout.Space(6);
                applyShowOnlyUnmatched = GUILayout.Toggle(applyShowOnlyUnmatched,
                    "Only unmatched", btnStyle, GUILayout.Width(100));
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(statusMessage))
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);

            if (applyEntries.Count == 0)
            {
                GUILayout.Space(30);
                EditorGUILayout.LabelField(
                    "Press 'Scan' to find TMP_Text objects that need a LocalizedText component.\n" +
                    "(PostQuizBoard and ButtonClickQuiz texts are skipped — they use Loc.T() directly)",
                    EditorStyles.centeredGreyMiniLabel);
                return;
            }

            // Stats
            int matched = applyEntries.Count(e => e.IsMatched);
            int unmatched = applyEntries.Count - matched;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label($"Total: {applyEntries.Count}", EditorStyles.miniLabel);
            GUILayout.Label($"Matched: {matched}", ColorLabel(new Color(0.1f, 0.8f, 0.1f)));
            GUILayout.Label($"Unmatched: {unmatched}", ColorLabel(new Color(1f, 0.55f, 0f)));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Headers (resizable)
            DrawResizableHeaders(colW2, colHeaders2);

            scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2);
            string lastGroup = null;

            foreach (var e in applyEntries.OrderByDescending(x => x.IsMatched).ThenBy(x => x.prefabPath ?? "(Scene)"))
            {
                if (applyShowOnlyUnmatched && e.IsMatched) continue;

                string group = e.prefabPath ?? "(Scene)";
                if (group != lastGroup)
                {
                    lastGroup = group;
                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField(group, EditorStyles.boldLabel);
                }

                Color row = e.IsMatched
                    ? new Color(0.2f, 0.7f, 0.2f, 0.06f)
                    : new Color(1f, 0.5f, 0f, 0.06f);
                Rect r = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, r.height + 1), row);

                e.selected = EditorGUILayout.Toggle(e.selected, GUILayout.Width(22));
                GUILayout.Label(e.IsMatched ? "Match" : "New",
                    ColorLabel(e.IsMatched ? new Color(0.1f, 0.8f, 0.1f) : new Color(1f, 0.55f, 0f)),
                    GUILayout.Width(colW2[0]));

                GUILayout.Label(e.goPath, EditorStyles.miniLabel, GUILayout.Width(colW2[1]));

                string preview = e.text.Replace("\n", " ");
                GUILayout.Label(preview, EditorStyles.miniLabel, GUILayout.Width(colW2[2]));

                if (e.IsMatched)
                {
                    GUILayout.Label(e.matchedKey, EditorStyles.miniLabel, GUILayout.Width(colW2[3]));
                    GUILayout.Label("(auto)", EditorStyles.centeredGreyMiniLabel, GUILayout.MinWidth(colW2[4]));
                }
                else
                {
                    e.customKey = EditorGUILayout.TextField(e.customKey, GUILayout.Width(colW2[3]));
                    e.customVI = EditorGUILayout.TextField(e.customVI, GUILayout.MinWidth(colW2[4]));
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4);
            int sel = applyEntries.Count(e => e.selected);
            GUI.enabled = sel > 0;
            GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);
            if (GUILayout.Button($"Apply LocalizedText to {sel} selected", GUILayout.Height(32)))
                ApplySelectedComponents();
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  TAB: IMPORT DIFF
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawImportDiff()
        {
            if (diffEntries.Count == 0)
            {
                GUILayout.Space(30);
                EditorGUILayout.LabelField(
                    "No import data. Use 'Import JSON...' from the Scan & Edit tab.",
                    EditorStyles.centeredGreyMiniLabel);
                return;
            }

            var btnStyle = EditorStyles.toolbarButton;

            // ── Toolbar ──────────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label($"Comparing: {diffFileName} -> {diffLang}", EditorStyles.miniLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Accept All Changed", btnStyle, GUILayout.Width(130)))
                diffEntries.ForEach(d => { if (d.status != DiffStatus.Same) d.accepted = true; });
            if (GUILayout.Button("Reject All", btnStyle, GUILayout.Width(80)))
                diffEntries.ForEach(d => d.accepted = false);
            if (GUILayout.Button("Accept Only Changed", btnStyle, GUILayout.Width(130)))
            {
                diffEntries.ForEach(d => d.accepted = false);
                diffEntries.Where(d => d.status == DiffStatus.Changed).ToList().ForEach(d => d.accepted = true);
            }

            GUILayout.Space(8);
            diffShowOnlyChanged = GUILayout.Toggle(diffShowOnlyChanged,
                "Hide unchanged", btnStyle, GUILayout.Width(100));

            GUILayout.FlexibleSpace();
            diffSearch = EditorGUILayout.TextField(diffSearch, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            // ── Status ───────────────────────────────────────────────────────
            if (!string.IsNullOrEmpty(statusMessage))
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);

            // ── Stats ────────────────────────────────────────────────────────
            int changed = diffEntries.Count(d => d.status == DiffStatus.Changed);
            int newKeys = diffEntries.Count(d => d.status == DiffStatus.NewKey);
            int same = diffEntries.Count(d => d.status == DiffStatus.Same);
            int accepted = diffEntries.Count(d => d.accepted);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label($"Changed: {changed}", ColorLabel(new Color(1f, 0.8f, 0.2f)));
            GUILayout.Label($"New: {newKeys}", ColorLabel(new Color(0.3f, 0.8f, 0.3f)));
            GUILayout.Label($"Same: {same}", EditorStyles.miniLabel);
            GUILayout.Label($"Accepted: {accepted}", ColorLabel(new Color(0.4f, 0.7f, 1f)));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // ── Column headers (resizable) ───────────────────────────────────
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("", GUILayout.Width(22));     // accept checkbox
            GUILayout.Label("", GUILayout.Width(50));     // status label

            for (int c = 0; c < colHeadersDiff.Length; c++)
            {
                bool isLast = (c == colHeadersDiff.Length - 1);
                if (isLast)
                    GUILayout.Label(colHeadersDiff[c], btnStyle, GUILayout.MinWidth(colWDiff[c]));
                else
                    GUILayout.Label(colHeadersDiff[c], btnStyle, GUILayout.Width(colWDiff[c]));

                if (!isLast)
                {
                    Rect lr = GUILayoutUtility.GetLastRect();
                    Rect sep = new Rect(lr.xMax - 3, lr.y, 6, lr.height);
                    EditorGUIUtility.AddCursorRect(sep, MouseCursor.ResizeHorizontal);
                    if (Event.current.type == EventType.MouseDown && sep.Contains(Event.current.mousePosition))
                    {
                        resizingCol = c;
                        resizingTarget = colWDiff;
                        resizeStartMouseX = Event.current.mousePosition.x;
                        resizeStartWidth = colWDiff[c];
                        Event.current.Use();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // ── Rows ─────────────────────────────────────────────────────────
            scrollPosDiff = EditorGUILayout.BeginScrollView(scrollPosDiff);

            foreach (var d in diffEntries)
            {
                // Filters
                if (diffShowOnlyChanged && d.status == DiffStatus.Same) continue;
                if (!string.IsNullOrEmpty(diffSearch))
                {
                    string f = diffSearch.ToLowerInvariant();
                    if (!(d.key?.ToLower().Contains(f) == true ||
                          d.currentValue?.ToLower().Contains(f) == true ||
                          d.importedValue?.ToLower().Contains(f) == true))
                        continue;
                }

                // Row color
                Color rowColor;
                string statusLabel;
                Color statusColor;
                switch (d.status)
                {
                    case DiffStatus.Changed:
                        rowColor = new Color(1f, 0.8f, 0.2f, 0.08f);
                        statusLabel = "DIFF";
                        statusColor = new Color(1f, 0.7f, 0.1f);
                        break;
                    case DiffStatus.NewKey:
                        rowColor = new Color(0.3f, 0.9f, 0.3f, 0.08f);
                        statusLabel = "NEW";
                        statusColor = new Color(0.2f, 0.8f, 0.2f);
                        break;
                    case DiffStatus.Removed:
                        rowColor = new Color(1f, 0.3f, 0.3f, 0.06f);
                        statusLabel = "DEL";
                        statusColor = new Color(1f, 0.4f, 0.4f);
                        break;
                    default:
                        rowColor = new Color(0.5f, 0.5f, 0.5f, 0.03f);
                        statusLabel = "SAME";
                        statusColor = new Color(0.6f, 0.6f, 0.6f);
                        break;
                }

                Rect r = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, r.height + 1), rowColor);

                // Accept checkbox
                d.accepted = EditorGUILayout.Toggle(d.accepted, GUILayout.Width(22));

                // Status
                GUILayout.Label(statusLabel, ColorLabel(statusColor), GUILayout.Width(50));

                // Key
                GUILayout.Label(d.key, EditorStyles.miniLabel, GUILayout.Width(colWDiff[0]));

                // Current value (read-only, grey if null)
                if (d.currentValue != null)
                {
                    string cur = d.currentValue.Replace("\n", " ");
                    GUILayout.Label(cur, EditorStyles.miniLabel, GUILayout.Width(colWDiff[1]));
                }
                else
                {
                    GUILayout.Label("(not in current)", ColorLabel(new Color(0.5f, 0.5f, 0.5f)),
                        GUILayout.Width(colWDiff[1]));
                }

                // Imported / Final value (editable)
                if (d.status == DiffStatus.Same)
                {
                    GUILayout.Label(d.importedValue?.Replace("\n", " ") ?? "",
                        EditorStyles.miniLabel, GUILayout.MinWidth(colWDiff[2]));
                }
                else
                {
                    d.finalValue = EditorGUILayout.TextField(d.finalValue ?? "",
                        GUILayout.MinWidth(colWDiff[2]));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            // ── Bottom: Apply button ─────────────────────────────────────────
            EditorGUILayout.Space(4);
            int acceptCount = diffEntries.Count(d => d.accepted && d.status != DiffStatus.Same);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.4f);
            GUI.enabled = acceptCount > 0;
            if (GUILayout.Button($"Apply {acceptCount} accepted changes to {diffLang}", GUILayout.Height(34)))
            {
                ApplyDiff();
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;

            GUILayout.Space(8);

            if (GUILayout.Button("Discard & Close", GUILayout.Height(34), GUILayout.Width(130)))
            {
                diffEntries.Clear();
                currentTab = Tab.ScanEdit;
                statusMessage = "Import diff discarded.";
            }

            EditorGUILayout.EndHorizontal();
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  TAB: SCENE SETUP
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawSceneSetup()
        {
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Scene & Project Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Run these once to prepare the scene for the localization system.",
                MessageType.Info);
            EditorGUILayout.Space(8);

            bool hasLM = FindObjectOfType<LocalizationManager>() != null;
            EditorGUI.BeginDisabledGroup(hasLM);
            if (GUILayout.Button(
                hasLM ? "LocalizationManager already in scene"
                      : "Add LocalizationManager to scene",
                GUILayout.Height(34)))
            {
                var go = new GameObject("LocalizationManager");
                go.AddComponent<LocalizationManager>();
                Undo.RegisterCreatedObjectUndo(go, "Add LocalizationManager");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Set Script Execution Order = -200", GUILayout.Height(34)))
            {
                foreach (var ms in MonoImporter.GetAllRuntimeMonoScripts())
                {
                    if (ms.GetClass() == typeof(LocalizationManager))
                    {
                        MonoImporter.SetExecutionOrder(ms, -200);
                        EditorUtility.DisplayDialog("Done", "Execution order set to -200.", "OK");
                        break;
                    }
                }
            }

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Reload JSON files", GUILayout.Height(34)))
            {
                LoadAllJson();
                Repaint();
            }

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════════════════
        private static GUIStyle ColorLabel(Color c)
        {
            var s = new GUIStyle(EditorStyles.miniLabel);
            s.normal.textColor = c;
            return s;
        }
    }
}
#endif
