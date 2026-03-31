// ============================================================
//  SkillDatabaseBuilder.cs  (Editor only — lives in Editor/)
//  Menu:  Tools ▸ Luzart ▸ 🎖 Skill Database Builder
// ============================================================
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Luzart;

public class SkillDatabaseBuilder : EditorWindow
{
    // ── Menu entry ────────────────────────────────────────────────────────────
    [MenuItem("Tools/Luzart/🎖 Skill Database Builder")]
    public static void Open()
        => GetWindow<SkillDatabaseBuilder>(true, "🎖 Skill Database Builder", true);

    // ── Constants ─────────────────────────────────────────────────────────────
    private const string RESOURCES_FOLDER = "Assets/_Game/Resources";
    private const string ASSET_PATH       = "Assets/_Game/Resources/SkillConfigDatabase.asset";

    // ── Window state ──────────────────────────────────────────────────────────
    private Vector2          _scroll;
    private SkillConfigDatabase _existing;

    // ── Default colour palette (one per badge) ────────────────────────────────
    // Order must match ESkillId definition order in SkillDefinition.All
    private static readonly Color[] _defaultColors =
    {
        // ── Tutorial ──────────────────────────────────────────────
        Hex("#FFD600"),   // TutorialComplete     — gold
        Hex("#00BCD4"),   // FirstAIPrompt        — cyan
        Hex("#4CAF50"),   // QuizAce              — green

        // ── Level 1 · Persona ─────────────────────────────────────
        Hex("#FF9800"),   // PersonaCreative      — orange
        Hex("#2196F3"),   // PersonaLogical       — blue
        Hex("#8BC34A"),   // PersonaEmpathic      — lime green
        Hex("#9C27B0"),   // PersonaStructured    — purple
        Hex("#795548"),   // ReflectionJournal    — warm brown
        Hex("#E91E63"),   // PersonalTouch        — pink

        // ── Level 2 · AI Tools ────────────────────────────────────
        Hex("#00ACC1"),   // AIMentorTools        — teal-cyan
        Hex("#FFC107"),   // CreativeIdeaGenerator— amber
        Hex("#009688"),   // InquiryBasedLearning — teal
        Hex("#3F51B5"),   // TeachbackWithAI      — indigo

        // ── Level 3 · Badges ──────────────────────────────────────
        Hex("#1976D2"),   // LessonCoCreator      — medium blue
        Hex("#FF5722"),   // IterationChampion    — deep orange
        Hex("#388E3C"),   // InclusivePlanner     — dark green
        Hex("#7B1FA2"),   // PersonaAligned       — dark purple
        Hex("#F44336"),   // FeedbackArchitect    — red
        Hex("#FF6D00"),   // SeekingImprovement   — orange-red
        Hex("#0288D1"),   // ForwardLookingDesigner — light blue
    };

    // Popup-on-unlock defaults (same order as SkillDefinition.All)
    // Level-2 shows 4 badges at once → keep all true, queue handles it
    private static readonly bool[] _defaultPopup =
    {
        true,  // TutorialComplete
        true,  // FirstAIPrompt
        true,  // QuizAce
        true,  // PersonaCreative
        true,  // PersonaLogical
        true,  // PersonaEmpathic
        true,  // PersonaStructured
        true,  // ReflectionJournal
        true,  // PersonalTouch
        true,  // AIMentorTools
        true,  // CreativeIdeaGenerator
        true,  // InquiryBasedLearning
        true,  // TeachbackWithAI
        true,  // LessonCoCreator
        true,  // IterationChampion
        true,  // InclusivePlanner
        true,  // PersonaAligned
        true,  // FeedbackArchitect
        true,  // SeekingImprovement
        true,  // ForwardLookingDesigner
    };

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        _existing = AssetDatabase.LoadAssetAtPath<SkillConfigDatabase>(ASSET_PATH);
        minSize    = new Vector2(480, 560);
    }

    // ── GUI ───────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        _existing = AssetDatabase.LoadAssetAtPath<SkillConfigDatabase>(ASSET_PATH);

        DrawHeader();
        DrawStatusBar();
        EditorGUILayout.Space(8);
        DrawActionButtons();
        EditorGUILayout.Space(12);
        DrawPreviewTable();
    }

    // ── Header ────────────────────────────────────────────────────────────────
    private void DrawHeader()
    {
        var style = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 16,
            alignment = TextAnchor.MiddleCenter,
            padding   = new RectOffset(0, 0, 10, 6),
        };
        EditorGUILayout.LabelField("🎖  Skill Database Builder", style);
        DrawSeparator();
    }

    // ── Status bar ────────────────────────────────────────────────────────────
    private void DrawStatusBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        bool exists = _existing != null;
        var  icon   = exists ? "✅" : "❌";
        var  msg    = exists
            ? $"Asset exists: {ASSET_PATH}  ({_existing.Entries.Count} entries)"
            : $"No asset found at:  {ASSET_PATH}";

        GUIStyle s = new GUIStyle(EditorStyles.label) { wordWrap = true };
        EditorGUILayout.LabelField($"{icon}  {msg}", s);
        EditorGUILayout.EndHorizontal();
    }

    // ── Action buttons ────────────────────────────────────────────────────────
    private void DrawActionButtons()
    {
        // ── Create / Rebuild ──────────────────────────────────────────────────
        var createStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 13,
            fontStyle = FontStyle.Bold,
            fixedHeight = 40,
        };

        string btnLabel = _existing == null
            ? "⚡  Create SkillConfigDatabase (20 badges)"
            : "🔄  Rebuild — reset all entries to defaults";

        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = _existing == null
            ? new Color(0.35f, 0.85f, 0.45f)   // green  → create
            : new Color(0.35f, 0.65f, 0.95f);   // blue   → rebuild

        if (GUILayout.Button(btnLabel, createStyle))
        {
            bool doIt = _existing == null || EditorUtility.DisplayDialog(
                "Rebuild SkillConfigDatabase?",
                "This will overwrite all current entries with defaults.\n" +
                "Your custom icons and colours will be lost.",
                "Yes, rebuild", "Cancel");

            if (doIt) BuildDatabase();
        }
        GUI.backgroundColor = prev;

        EditorGUILayout.Space(4);

        // ── Select asset ──────────────────────────────────────────────────────
        EditorGUI.BeginDisabledGroup(_existing == null);
        if (GUILayout.Button("📂  Select asset in Project", GUILayout.Height(28)))
        {
            Selection.activeObject = _existing;
            EditorGUIUtility.PingObject(_existing);
        }
        EditorGUI.EndDisabledGroup();
    }

    // ── Preview table ─────────────────────────────────────────────────────────
    private void DrawPreviewTable()
    {
        DrawSeparator();
        EditorGUILayout.LabelField(
            "Preview — 20 badge defaults",
            EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // Column headers
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Em",   GUILayout.Width(24));
        GUILayout.Label("ID",   GUILayout.Width(180));
        GUILayout.Label("Colour",    GUILayout.Width(70));
        GUILayout.Label("Popup", GUILayout.Width(44));
        GUILayout.Label("Level", GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();

        _scroll = EditorGUILayout.BeginScrollView(_scroll,
            GUILayout.ExpandHeight(true));

        var all = SkillDefinition.All;
        for (int i = 0; i < all.Length; i++)
        {
            var info    = all[i];
            var color   = i < _defaultColors.Length ? _defaultColors[i] : Color.white;
            bool popup  = i < _defaultPopup.Length  && _defaultPopup[i];

            // Alternate row tint
            if (i % 2 == 0)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                DrawRow(info, color, popup);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRow(info, color, popup);
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawRow(SkillInfo info, Color color, bool popup)
    {
        GUILayout.Label(info.emoji, GUILayout.Width(24));
        GUILayout.Label(info.id.ToString(), GUILayout.Width(180));

        // Colour swatch
        var swatchRect = GUILayoutUtility.GetRect(70, 18, GUILayout.Width(70));
        EditorGUI.DrawRect(swatchRect, color);

        GUILayout.Label(popup ? "✅" : "—",
            new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter },
            GUILayout.Width(44));

        string lvlLabel = info.forLevel == 0 ? "Tut" : $"L{info.forLevel}";
        GUILayout.Label(lvlLabel, GUILayout.Width(50));
    }

    // ── Build logic ───────────────────────────────────────────────────────────
    private void BuildDatabase()
    {
        // 1. Ensure Resources folder exists
        if (!AssetDatabase.IsValidFolder(RESOURCES_FOLDER))
        {
            Directory.CreateDirectory(RESOURCES_FOLDER);
            AssetDatabase.Refresh();
        }

        // 2. Load or create the SO
        var db = AssetDatabase.LoadAssetAtPath<SkillConfigDatabase>(ASSET_PATH);
        if (db == null)
        {
            db = CreateInstance<SkillConfigDatabase>();
            AssetDatabase.CreateAsset(db, ASSET_PATH);
        }

        // 3. Rebuild entries via SerializedObject (safe Undo + dirty)
        var so      = new SerializedObject(db);
        var listProp = so.FindProperty("_entries");
        listProp.ClearArray();

        var all = SkillDefinition.All;
        for (int i = 0; i < all.Length; i++)
        {
            listProp.InsertArrayElementAtIndex(i);
            var elem = listProp.GetArrayElementAtIndex(i);

            elem.FindPropertyRelative("skillId")
                .enumValueIndex = GetEnumIndex(all[i].id);

            // icon — null by default (user sets it manually)
            elem.FindPropertyRelative("icon").objectReferenceValue = null;

            // badge colour
            Color c = i < _defaultColors.Length ? _defaultColors[i] : Color.white;
            elem.FindPropertyRelative("badgeColor").colorValue = c;

            // popup flag
            bool p = i < _defaultPopup.Length && _defaultPopup[i];
            elem.FindPropertyRelative("showPopupOnUnlock").boolValue = p;
        }

        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _existing = db;

        // 4. Ping & select in Project
        Selection.activeObject = db;
        EditorGUIUtility.PingObject(db);

        Debug.Log($"[SkillDatabaseBuilder] ✅ Created/rebuilt at {ASSET_PATH}  ({all.Length} entries)");
        EditorUtility.DisplayDialog(
            "Done!",
            $"SkillConfigDatabase created with {all.Length} entries.\n\n" +
            $"Path: {ASSET_PATH}\n\n" +
            "Next: assign Sprite icons in the Inspector for each badge.",
            "OK");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        c.a = 1f;
        return c;
    }

    /// Returns the index of an ESkillId value in the enum (for SerializedProperty).
    private static int GetEnumIndex(ESkillId id)
    {
        var values = (ESkillId[])System.Enum.GetValues(typeof(ESkillId));
        for (int i = 0; i < values.Length; i++)
            if (values[i] == id) return i;
        return 0;
    }

    private static void DrawSeparator()
    {
        var r = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(r, new Color(0.5f, 0.5f, 0.5f, 0.4f));
        EditorGUILayout.Space(4);
    }
}
