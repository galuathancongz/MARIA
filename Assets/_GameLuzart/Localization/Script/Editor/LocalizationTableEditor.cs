#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Luzart
{
    /// <summary>
    /// Full CRUD editor for localization entries (en.json + vi.json).
    /// Menu: Tools > Localization > Table Editor
    /// </summary>
    public class LocalizationTableEditor : EditorWindow
    {
        // ─── Data ────────────────────────────────────────────────────────────
        private List<LocRow> rows = new List<LocRow>();
        private List<LocRow> filtered = new List<LocRow>();
        private bool isDirty;

        private class LocRow
        {
            public string key;
            public string en;
            public string vi;
            public string originalKey;   // for rename detection
            public string originalEn;
            public string originalVi;
            public bool   isNew;
            public bool   selected;
            public bool   isEditing;     // expand to multiline

            public bool HasChanged =>
                key != originalKey || en != originalEn || vi != originalVi;
        }

        // ─── UI State ────────────────────────────────────────────────────────
        private Vector2 scrollPos;
        private string  searchFilter = "";
        private int     categoryIndex;
        private string  statusMessage = "";
        private MessageType statusType = MessageType.Info;
        private bool    showOnlyModified;
        private bool    showOnlyMissing;   // VI is empty
        private int     sortColumn;        // 0=key, 1=en, 2=vi
        private bool    sortAscending = true;
        private float   lastSaveTime;
        private float   statusSetTime;   // tracks when status message was last set

        // Add-new panel
        private bool   showAddPanel;
        private string newKey = "";
        private string newEn  = "";
        private string newVi  = "";

        // Categories derived from key prefixes
        private static readonly string[] CATEGORIES = new[]
        {
            "All", "ui", "level1", "level2", "level3", "tutorial",
            "thinking", "mentor", "persona", "prompts", "profile",
            "skill", "tooltip", "other"
        };

        // Pre-computed known category set (avoids allocating per-filter call)
        private static readonly HashSet<string> KNOWN_CATEGORIES =
            new HashSet<string>(CATEGORIES.Where(c => c != "All" && c != "other"));

        private void SetStatus(string msg, MessageType type = MessageType.Info)
        {
            statusMessage = msg;
            statusType = type;
            statusSetTime = (float)EditorApplication.timeSinceStartup;
        }

        // ─── Paths ───────────────────────────────────────────────────────────
        private string EnJsonPath => Path.Combine(Application.dataPath,
            "_GameLuzart/Localization/Resources/Localization/en.json");
        private string ViJsonPath => Path.Combine(Application.dataPath,
            "_GameLuzart/Localization/Resources/Localization/vi.json");

        // ─── Menu Entry ──────────────────────────────────────────────────────
        [MenuItem("Tools/Localization/Table Editor %#l")]
        public static void Open()
        {
            var w = GetWindow<LocalizationTableEditor>("Localization Table");
            w.minSize = new Vector2(1100, 600);
            w.LoadAll();
        }

        // ─── Load / Save ─────────────────────────────────────────────────────
        private void LoadAll()
        {
            rows.Clear();
            isDirty = false;

            var enMap = LoadJsonMap(EnJsonPath);
            var viMap = LoadJsonMap(ViJsonPath);

            // Merge all keys from both files
            var allKeys = new HashSet<string>(enMap.Keys);
            allKeys.UnionWith(viMap.Keys);

            foreach (var key in allKeys)
            {
                enMap.TryGetValue(key, out string enVal);
                viMap.TryGetValue(key, out string viVal);
                rows.Add(new LocRow
                {
                    key         = key,
                    en          = enVal ?? "",
                    vi          = viVal ?? "",
                    originalKey = key,
                    originalEn  = enVal ?? "",
                    originalVi  = viVal ?? "",
                });
            }

            SortRows();
            ApplyFilter();
            SetStatus($"Loaded {rows.Count} entries from en.json + vi.json");
        }

        private Dictionary<string, string> LoadJsonMap(string path)
        {
            var map = new Dictionary<string, string>();
            if (!File.Exists(path)) return map;
            var data = JsonUtility.FromJson<LocalizationData>(File.ReadAllText(path));
            if (data?.items == null) return map;
            foreach (var item in data.items)
            {
                if (!string.IsNullOrEmpty(item.key))
                    map[item.key] = item.value ?? "";
            }
            return map;
        }

        private void SaveAll()
        {
            // Validate: no duplicate keys
            var dupes = rows.GroupBy(r => r.key).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (dupes.Count > 0)
            {
                EditorUtility.DisplayDialog("Duplicate Keys",
                    $"Cannot save — duplicate keys found:\n{string.Join("\n", dupes.Take(5))}", "OK");
                return;
            }

            // Validate: no empty keys
            if (rows.Any(r => string.IsNullOrWhiteSpace(r.key)))
            {
                EditorUtility.DisplayDialog("Empty Keys",
                    "Cannot save — some entries have empty keys.", "OK");
                return;
            }

            WriteJson(EnJsonPath, rows.Select(r => (r.key, r.en)).ToList());
            WriteJson(ViJsonPath, rows.Select(r => (r.key, r.vi)).ToList());

            // Update originals
            foreach (var r in rows)
            {
                r.originalKey = r.key;
                r.originalEn  = r.en;
                r.originalVi  = r.vi;
                r.isNew       = false;
            }

            isDirty = false;
            lastSaveTime = (float)EditorApplication.timeSinceStartup;
            SetStatus($"Saved {rows.Count} entries to en.json + vi.json");

            EditorApplication.delayCall += () => AssetDatabase.Refresh();
            Debug.Log("[LocalizationTable] " + statusMessage);
        }

        private static string EscapeJsonString(string s)
        {
            return s
                .Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\n", "\\n").Replace("\r", "")
                .Replace("\t", "\\t");
        }

        private void WriteJson(string path, List<(string key, string value)> items)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{ \"items\": [");

            // Group by prefix for readability
            string lastPrefix = null;
            for (int i = 0; i < items.Count; i++)
            {
                var (key, value) = items[i];

                // Add blank line between different prefixes
                string prefix = key.Contains(".") ? key.Substring(0, key.IndexOf('.')) : key;
                if (lastPrefix != null && prefix != lastPrefix)
                    sb.AppendLine();
                lastPrefix = prefix;

                string k = EscapeJsonString(key);
                string v = EscapeJsonString(value);
                sb.Append($"  {{\"key\": \"{k}\", \"value\": \"{v}\"}}");
                if (i < items.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("]}");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        // ─── Filter & Sort ───────────────────────────────────────────────────
        private void ApplyFilter()
        {
            filtered = rows.ToList(); // always start from a copy to avoid aliasing

            // Category filter
            if (categoryIndex > 0)
            {
                string cat = CATEGORIES[categoryIndex];
                if (cat == "other")
                {
                    filtered = filtered.Where(r =>
                    {
                        string prefix = r.key.Contains(".") ? r.key.Substring(0, r.key.IndexOf('.')) : "";
                        return !KNOWN_CATEGORIES.Contains(prefix);
                    }).ToList();
                }
                else
                {
                    filtered = filtered.Where(r => r.key.StartsWith(cat + ".")).ToList();
                }
            }

            // Search filter
            if (!string.IsNullOrEmpty(searchFilter))
            {
                string lower = searchFilter.ToLowerInvariant();
                filtered = filtered.Where(r =>
                    r.key.ToLowerInvariant().Contains(lower) ||
                    r.en.ToLowerInvariant().Contains(lower) ||
                    r.vi.ToLowerInvariant().Contains(lower)
                ).ToList();
            }

            // Show only modified
            if (showOnlyModified)
                filtered = filtered.Where(r => r.HasChanged || r.isNew).ToList();

            // Show only missing VI
            if (showOnlyMissing)
                filtered = filtered.Where(r => string.IsNullOrWhiteSpace(r.vi)).ToList();
        }

        private void SortRows()
        {
            switch (sortColumn)
            {
                case 0: rows = sortAscending ? rows.OrderBy(r => r.key).ToList()
                                             : rows.OrderByDescending(r => r.key).ToList(); break;
                case 1: rows = sortAscending ? rows.OrderBy(r => r.en).ToList()
                                             : rows.OrderByDescending(r => r.en).ToList();  break;
                case 2: rows = sortAscending ? rows.OrderBy(r => r.vi).ToList()
                                             : rows.OrderByDescending(r => r.vi).ToList();  break;
            }
        }

        // ─── Add / Delete ────────────────────────────────────────────────────
        /// <returns>true if entry was added, false if validation failed</returns>
        private bool AddEntry(string key, string en, string vi)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                SetStatus("Key cannot be empty!", MessageType.Error);
                return false;
            }
            if (rows.Any(r => r.key == key))
            {
                SetStatus($"Key \"{key}\" already exists!", MessageType.Error);
                return false;
            }

            rows.Add(new LocRow
            {
                key = key, en = en, vi = vi,
                originalKey = "", originalEn = "", originalVi = "",
                isNew = true
            });
            isDirty = true;
            SortRows();
            ApplyFilter();
            SetStatus($"Added new entry: {key}");
            return true;
        }

        private void DeleteSelected()
        {
            var toDelete = filtered.Where(r => r.selected).ToList();
            if (toDelete.Count == 0) return;

            if (!EditorUtility.DisplayDialog("Confirm Delete",
                $"Delete {toDelete.Count} selected entries?\n\n" +
                string.Join("\n", toDelete.Take(8).Select(r => r.key)) +
                (toDelete.Count > 8 ? $"\n... and {toDelete.Count - 8} more" : ""),
                "Delete", "Cancel"))
                return;

            foreach (var r in toDelete)
                rows.Remove(r);

            isDirty = true;
            ApplyFilter();
            SetStatus($"Deleted {toDelete.Count} entries", MessageType.Warning);
        }

        private void DuplicateSelected()
        {
            var toDupe = filtered.Where(r => r.selected).ToList();
            if (toDupe.Count == 0) return;

            int added = 0;
            foreach (var r in toDupe)
            {
                string newK = r.key + "_copy";
                int suffix = 1;
                while (rows.Any(x => x.key == newK))
                    newK = r.key + "_copy" + (++suffix);

                rows.Add(new LocRow
                {
                    key = newK, en = r.en, vi = r.vi,
                    originalKey = "", originalEn = "", originalVi = "",
                    isNew = true
                });
                added++;
            }
            isDirty = true;
            SortRows();
            ApplyFilter();
            SetStatus($"Duplicated {added} entries");
        }

        // ─── GUI ─────────────────────────────────────────────────────────────
        // Cached styles (created once)
        private GUIStyle _headerBtnStyle;
        private GUIStyle _modifiedLabelStyle;
        private GUIStyle _newLabelStyle;
        private GUIStyle _missingLabelStyle;
        private GUIStyle _multilineTextStyle;

        private void EnsureStyles()
        {
            if (_headerBtnStyle != null) return;

            _headerBtnStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };
            _modifiedLabelStyle = new GUIStyle(EditorStyles.miniLabel);
            _modifiedLabelStyle.normal.textColor = new Color(0.2f, 0.6f, 1f);

            _newLabelStyle = new GUIStyle(EditorStyles.miniLabel);
            _newLabelStyle.normal.textColor = new Color(0.1f, 0.8f, 0.2f);

            _missingLabelStyle = new GUIStyle(EditorStyles.miniLabel);
            _missingLabelStyle.normal.textColor = new Color(1f, 0.4f, 0.3f);

            _multilineTextStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                richText = false
            };
        }

        private void OnGUI()
        {
            EnsureStyles();

            DrawToolbar();
            DrawStatusBar();
            DrawStatsBar();

            if (showAddPanel)
                DrawAddPanel();

            DrawColumnHeaders();
            DrawTable();
            DrawBottomBar();
        }

        // ── Toolbar ──────────────────────────────────────────────────────────
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Reload
            if (GUILayout.Button("↺ Reload", EditorStyles.toolbarButton, GUILayout.Width(65)))
            {
                if (!isDirty || EditorUtility.DisplayDialog("Unsaved changes",
                    "Discard all unsaved changes and reload from disk?", "Reload", "Cancel"))
                    LoadAll();
            }

            // Save
            GUI.enabled = isDirty;
            GUI.backgroundColor = isDirty ? new Color(0.3f, 0.9f, 0.3f) : Color.white;
            if (GUILayout.Button("💾 Save", EditorStyles.toolbarButton, GUILayout.Width(55)))
                SaveAll();
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            GUILayout.Space(8);

            // Add
            if (GUILayout.Button("+ Add", EditorStyles.toolbarButton, GUILayout.Width(50)))
                showAddPanel = !showAddPanel;

            // Delete selected
            int selCount = filtered.Count(r => r.selected);
            GUI.enabled = selCount > 0;
            if (GUILayout.Button($"✕ Delete ({selCount})", EditorStyles.toolbarButton, GUILayout.Width(90)))
                DeleteSelected();

            // Duplicate
            if (GUILayout.Button("⧉ Duplicate", EditorStyles.toolbarButton, GUILayout.Width(80)))
                DuplicateSelected();
            GUI.enabled = true;

            GUILayout.Space(8);

            // Select / Deselect
            if (GUILayout.Button("All", EditorStyles.toolbarButton, GUILayout.Width(30)))
                filtered.ForEach(r => r.selected = true);
            if (GUILayout.Button("None", EditorStyles.toolbarButton, GUILayout.Width(35)))
                filtered.ForEach(r => r.selected = false);

            GUILayout.FlexibleSpace();

            // Filters
            showOnlyModified = GUILayout.Toggle(showOnlyModified, "Modified", EditorStyles.toolbarButton, GUILayout.Width(62));
            showOnlyMissing  = GUILayout.Toggle(showOnlyMissing,  "Missing VI", EditorStyles.toolbarButton, GUILayout.Width(72));

            GUILayout.Space(4);

            // Category dropdown
            EditorGUI.BeginChangeCheck();
            categoryIndex = EditorGUILayout.Popup(categoryIndex, CATEGORIES,
                EditorStyles.toolbarDropDown, GUILayout.Width(90));
            if (EditorGUI.EndChangeCheck()) ApplyFilter();

            // Search
            EditorGUI.BeginChangeCheck();
            searchFilter = EditorGUILayout.TextField(searchFilter,
                EditorStyles.toolbarSearchField, GUILayout.Width(180));
            if (EditorGUI.EndChangeCheck()) ApplyFilter();

            // Clear search
            if (!string.IsNullOrEmpty(searchFilter) &&
                GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                searchFilter = "";
                ApplyFilter();
            }

            EditorGUILayout.EndHorizontal();
        }

        // ── Status ───────────────────────────────────────────────────────────
        private void DrawStatusBar()
        {
            if (string.IsNullOrEmpty(statusMessage)) return;

            // Auto-clear success messages after 6 seconds
            if (statusType == MessageType.Info &&
                statusSetTime > 0 &&
                (float)EditorApplication.timeSinceStartup - statusSetTime > 6f)
            {
                statusMessage = "";
                return;
            }

            EditorGUILayout.HelpBox(
                (isDirty ? "● Unsaved changes  |  " : "") + statusMessage,
                statusType);
        }

        // ── Stats ────────────────────────────────────────────────────────────
        private void DrawStatsBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            int total     = rows.Count;
            int showing   = filtered.Count;
            int modified  = rows.Count(r => r.HasChanged);
            int newCount  = rows.Count(r => r.isNew);
            int missingVi = rows.Count(r => string.IsNullOrWhiteSpace(r.vi));

            GUILayout.Label($"Total: {total}", EditorStyles.miniLabel);
            GUILayout.Label($"Showing: {showing}", EditorStyles.miniLabel);
            if (modified > 0) GUILayout.Label($"Modified: {modified}", _modifiedLabelStyle);
            if (newCount > 0) GUILayout.Label($"New: {newCount}", _newLabelStyle);
            if (missingVi > 0) GUILayout.Label($"Missing VI: {missingVi}", _missingLabelStyle);

            GUILayout.FlexibleSpace();

            // Category counts
            if (categoryIndex == 0)
            {
                for (int i = 1; i < CATEGORIES.Length; i++)
                {
                    string cat = CATEGORIES[i];
                    int c = cat == "other"
                        ? rows.Count(r => { var p = r.key.Contains(".") ? r.key.Substring(0, r.key.IndexOf('.')) : ""; return !KNOWN_CATEGORIES.Contains(p); })
                        : rows.Count(r => r.key.StartsWith(cat + "."));
                    if (c > 0)
                        GUILayout.Label($"{cat}:{c}", EditorStyles.centeredGreyMiniLabel);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        // ── Add Panel ────────────────────────────────────────────────────────
        private void DrawAddPanel()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add New Entry", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Key", GUILayout.Width(30));
            newKey = EditorGUILayout.TextField(newKey, GUILayout.Width(250));
            GUILayout.Label("EN", GUILayout.Width(22));
            newEn = EditorGUILayout.TextField(newEn);
            GUILayout.Label("VI", GUILayout.Width(22));
            newVi = EditorGUILayout.TextField(newVi);

            if (GUILayout.Button("Add", GUILayout.Width(50)))
            {
                if (AddEntry(newKey.Trim(), newEn, newVi))
                {
                    // Success — clear inputs
                    newKey = ""; newEn = ""; newVi = "";
                    GUI.FocusControl(null);
                }
            }
            if (GUILayout.Button("✕", GUILayout.Width(22)))
                showAddPanel = false;

            EditorGUILayout.EndHorizontal();

            // Quick category buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Prefix:", EditorStyles.miniLabel, GUILayout.Width(40));
            foreach (var cat in new[] { "ui.", "level1.", "level2.", "level3.", "tutorial.", "prompts.", "skill." })
            {
                if (GUILayout.Button(cat, EditorStyles.miniButton, GUILayout.Width(55)))
                {
                    if (!newKey.StartsWith(cat))
                        newKey = cat + newKey;
                    GUI.FocusControl(null);
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        // ── Column Headers ───────────────────────────────────────────────────
        private void DrawColumnHeaders()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("", GUILayout.Width(22)); // checkbox
            GUILayout.Label("", GUILayout.Width(40)); // status

            if (GUILayout.Button(SortLabel("Key", 0), _headerBtnStyle, GUILayout.Width(230)))
                ToggleSort(0);
            if (GUILayout.Button(SortLabel("English (EN)", 1), _headerBtnStyle, GUILayout.MinWidth(260)))
                ToggleSort(1);
            if (GUILayout.Button(SortLabel("Vietnamese (VI)", 2), _headerBtnStyle, GUILayout.MinWidth(260)))
                ToggleSort(2);

            GUILayout.Label("", GUILayout.Width(48)); // actions
            EditorGUILayout.EndHorizontal();
        }

        private string SortLabel(string name, int col)
        {
            if (sortColumn != col) return name;
            return name + (sortAscending ? " ▲" : " ▼");
        }

        private void ToggleSort(int col)
        {
            if (sortColumn == col) sortAscending = !sortAscending;
            else { sortColumn = col; sortAscending = true; }
            SortRows();
            ApplyFilter();
        }

        // ── Table ────────────────────────────────────────────────────────────
        private void DrawTable()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < filtered.Count; i++)
            {
                var r = filtered[i];
                DrawRow(r, i);
            }

            if (filtered.Count == 0)
            {
                GUILayout.Space(30);
                EditorGUILayout.LabelField(
                    rows.Count == 0
                        ? "No data loaded. Press ↺ Reload."
                        : "No entries match current filters.",
                    EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(LocRow r, int index)
        {
            // Row background color
            Color bgColor;
            if (r.isNew)             bgColor = new Color(0.1f, 0.7f, 0.2f, 0.08f);
            else if (r.HasChanged)   bgColor = new Color(0.2f, 0.5f, 1f, 0.08f);
            else if (index % 2 == 0) bgColor = new Color(0f, 0f, 0f, 0.04f);
            else                     bgColor = Color.clear;

            Rect rowRect = EditorGUILayout.BeginHorizontal();
            if (bgColor != Color.clear)
                EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.y, rowRect.width, rowRect.height + 1), bgColor);

            // Checkbox
            r.selected = EditorGUILayout.Toggle(r.selected, GUILayout.Width(22));

            // Status badge
            string badge;
            GUIStyle badgeStyle;
            if (r.isNew)                             { badge = "NEW";  badgeStyle = _newLabelStyle; }
            else if (r.HasChanged)                   { badge = "MOD";  badgeStyle = _modifiedLabelStyle; }
            else if (string.IsNullOrWhiteSpace(r.vi)) { badge = "!VI";  badgeStyle = _missingLabelStyle; }
            else                                     { badge = "  ✓";  badgeStyle = EditorStyles.centeredGreyMiniLabel; }
            GUILayout.Label(badge, badgeStyle, GUILayout.Width(40));

            // Key (editable)
            EditorGUI.BeginChangeCheck();
            r.key = EditorGUILayout.TextField(r.key, GUILayout.Width(230));
            if (EditorGUI.EndChangeCheck()) { isDirty = true; ApplyFilter(); }

            // EN value
            if (r.isEditing)
            {
                // Multiline mode
                EditorGUILayout.BeginVertical(GUILayout.MinWidth(260));
                EditorGUI.BeginChangeCheck();
                r.en = EditorGUILayout.TextArea(r.en, _multilineTextStyle, GUILayout.MinHeight(50), GUILayout.MinWidth(254));
                if (EditorGUI.EndChangeCheck()) isDirty = true;
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUILayout.MinWidth(260));
                EditorGUI.BeginChangeCheck();
                r.vi = EditorGUILayout.TextArea(r.vi, _multilineTextStyle, GUILayout.MinHeight(50), GUILayout.MinWidth(254));
                if (EditorGUI.EndChangeCheck()) isDirty = true;
                EditorGUILayout.EndVertical();
            }
            else
            {
                // Single line mode
                EditorGUI.BeginChangeCheck();
                r.en = EditorGUILayout.TextField(r.en, GUILayout.MinWidth(260));
                if (EditorGUI.EndChangeCheck()) isDirty = true;

                EditorGUI.BeginChangeCheck();
                r.vi = EditorGUILayout.TextField(r.vi, GUILayout.MinWidth(260));
                if (EditorGUI.EndChangeCheck()) isDirty = true;
            }

            // Actions
            // Toggle multiline
            if (GUILayout.Button(r.isEditing ? "▲" : "▼", GUILayout.Width(22)))
                r.isEditing = !r.isEditing;

            // Revert single row
            GUI.enabled = r.HasChanged;
            if (GUILayout.Button("↩", GUILayout.Width(22)))
            {
                r.key = r.originalKey;
                r.en  = r.originalEn;
                r.vi  = r.originalVi;
                isDirty = rows.Any(x => x.HasChanged || x.isNew);
                ApplyFilter();
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        // ── Bottom Bar ───────────────────────────────────────────────────────
        private void DrawBottomBar()
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Save shortcut reminder
            if (isDirty)
            {
                GUILayout.Label("● Unsaved changes — Ctrl+S to save",
                    _modifiedLabelStyle);
            }
            else
            {
                GUILayout.Label("All changes saved.", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.FlexibleSpace();

            // Quick actions
            if (GUILayout.Button("Export CSV", EditorStyles.toolbarButton, GUILayout.Width(75)))
                ExportCSV();

            GUI.backgroundColor = isDirty ? new Color(0.3f, 0.9f, 0.3f) : Color.white;
            if (GUILayout.Button("💾 Save All", EditorStyles.toolbarButton, GUILayout.Width(75)))
                SaveAll();
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        // ─── Export CSV ──────────────────────────────────────────────────────
        private void ExportCSV()
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Localization CSV",
                Application.dataPath,
                "localization_all",
                "csv");
            if (string.IsNullOrEmpty(path)) return;

            var sb = new StringBuilder();
            sb.AppendLine("Key,English,Vietnamese");
            foreach (var r in rows.OrderBy(r => r.key))
            {
                string en = r.en.Replace("\"", "\"\"").Replace("\n", "\\n");
                string vi = r.vi.Replace("\"", "\"\"").Replace("\n", "\\n");
                sb.AppendLine($"\"{r.key}\",\"{en}\",\"{vi}\"");
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            EditorUtility.DisplayDialog("Exported",
                $"Exported {rows.Count} entries to:\n{path}", "OK");
        }

        // ─── Window Lifecycle ────────────────────────────────────────────────
        private void OnEnable()
        {
            if (rows.Count == 0) LoadAll();
        }

        private void OnDestroy()
        {
            if (isDirty)
            {
                if (EditorUtility.DisplayDialog("Unsaved Changes",
                    "You have unsaved localization changes. Save before closing?",
                    "Save", "Discard"))
                {
                    // Write files directly without AssetDatabase.Refresh() (unsafe in OnDestroy)
                    var dupes = rows.GroupBy(r => r.key).Where(g => g.Count() > 1).ToList();
                    if (dupes.Count == 0 && !rows.Any(r => string.IsNullOrWhiteSpace(r.key)))
                    {
                        WriteJson(EnJsonPath, rows.Select(r => (r.key, r.en)).ToList());
                        WriteJson(ViJsonPath, rows.Select(r => (r.key, r.vi)).ToList());
                        Debug.Log("[LocalizationTable] Saved on close.");
                        EditorApplication.delayCall += () => AssetDatabase.Refresh();
                    }
                }
            }
        }

        // Handle Ctrl+S
        private void OnInspectorUpdate()
        {
            // Periodically repaint to update status
            if (isDirty) Repaint();
        }

        [InitializeOnLoad]
        private static class SaveHook
        {
            static SaveHook()
            {
                EditorApplication.projectChanged += () =>
                {
                    // Optional: auto-reload when external changes detected
                };
            }
        }
    }
}
#endif
