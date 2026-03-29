#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Luzart
{
    public class TooltipPrefabCreator : EditorWindow
    {
        private string tooltipKey = "tooltip.accept";
        private string savePath = "Assets/_Game/Prefabs/Tooltip";
        private string prefabName = "TooltipTrigger";
        private Color bgColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        private float fontSize = 20f;
        private Vector2 padding = new Vector2(20f, 10f);
        private float animDuration = 0.2f;
        private Ease animEase = Ease.OutBack;

        [MenuItem("Luzart/LuzartTool/Tooltip Prefab Creator")]
        public static void ShowWindow()
        {
            GetWindow<TooltipPrefabCreator>("Tooltip Creator");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Tooltip Prefab Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawSettings();
            EditorGUILayout.Space(10);
            DrawCreateButtons();
        }

        private void DrawSettings()
        {
            GUILayout.Label("Save Settings", EditorStyles.boldLabel);
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);

            EditorGUILayout.Space(5);
            GUILayout.Label("Tooltip Settings", EditorStyles.boldLabel);
            tooltipKey = EditorGUILayout.TextField("Default Key", tooltipKey);
            bgColor = EditorGUILayout.ColorField("Background Color", bgColor);
            fontSize = EditorGUILayout.FloatField("Font Size", fontSize);
            padding = EditorGUILayout.Vector2Field("Padding (X, Y)", padding);

            EditorGUILayout.Space(5);
            GUILayout.Label("Animation Settings", EditorStyles.boldLabel);
            animDuration = EditorGUILayout.FloatField("Duration", animDuration);
            animEase = (Ease)EditorGUILayout.EnumPopup("Ease", animEase);
        }

        private void DrawCreateButtons()
        {
            if (GUILayout.Button("Create Tooltip Prefab", GUILayout.Height(30)))
            {
                CreateTooltipPrefab();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Add TooltipTrigger to Selected", GUILayout.Height(25)))
            {
                AddTooltipToSelected();
            }
        }

        private void CreateTooltipPrefab()
        {
            if (!AssetDatabase.IsValidFolder(savePath))
            {
                CreateFolderRecursive(savePath);
            }

            // Root GameObject
            var root = new GameObject(prefabName);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(200, 50);

            // Add TooltipTrigger
            var trigger = root.AddComponent<TooltipTrigger>();

            // Tooltip Panel (child, default inactive)
            var panel = CreateTooltipPanel(root.transform);

            // Wire references
            trigger.tooltipKey = tooltipKey;
            trigger.tooltipPanel = panel.gameObject;
            trigger.tooltipText = panel.GetComponentInChildren<TMP_Text>();
            trigger.showAnimation = panel.GetComponent<TweenAnimation>();

            // Panel starts inactive
            panel.gameObject.SetActive(false);

            // Save as prefab
            string fullPath = $"{savePath}/{prefabName}.prefab";
            fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);
            PrefabUtility.SaveAsPrefabAsset(root, fullPath);
            DestroyImmediate(root);

            // Select the created prefab
            var created = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            Selection.activeObject = created;
            EditorGUIUtility.PingObject(created);

            Debug.Log($"Tooltip prefab created: {fullPath}");
        }

        private RectTransform CreateTooltipPanel(Transform parent)
        {
            // Panel
            var panelGo = new GameObject("TooltipPanel");
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelGo.AddComponent<CanvasGroup>();
            panelRect.SetParent(parent, false);

            // Position above button
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0, 10f);

            // Background
            var bg = panelGo.AddComponent<Image>();
            bg.color = bgColor;
            bg.raycastTarget = false;

            // Layout
            var hlg = panelGo.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset((int)padding.x, (int)padding.x, (int)padding.y, (int)padding.y);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Content Size Fitter
            var csf = panelGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Text
            var textGo = new GameObject("Text");
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.SetParent(panelRect, false);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "Tooltip text";
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = false;

            // TweenAnimation (Scale)
            var tweenAnim = panelGo.AddComponent<TweenAnimation>();
            var so = new SerializedObject(tweenAnim);
            so.FindProperty("typeAnimation").enumValueIndex = (int)EAnimation.Scale;

            var settings = so.FindProperty("tweenAnimationSettings");
            var general = settings.FindPropertyRelative("General");
            general.FindPropertyRelative("Target").objectReferenceValue = panelGo;
            general.FindPropertyRelative("Duration").floatValue = animDuration;
            general.FindPropertyRelative("Easing").enumValueIndex = (int)animEase;

            var values = settings.FindPropertyRelative("Values");
            values.FindPropertyRelative("IsSetFromInInit").boolValue = true;
            values.FindPropertyRelative("Vector3From").vector3Value = new Vector3(0.8f, 0.8f, 1f);
            values.FindPropertyRelative("Vector3To").vector3Value = Vector3.one;

            so.ApplyModifiedPropertiesWithoutUndo();

            return panelRect;
        }

        private void AddTooltipToSelected()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select a GameObject in the scene or prefab.", "OK");
                return;
            }

            Undo.RegisterCompleteObjectUndo(selected, "Add Tooltip");

            // Check if already has TooltipTrigger
            var existing = selected.GetComponent<TooltipTrigger>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Already Exists", "This GameObject already has a TooltipTrigger.", "OK");
                return;
            }

            // Add trigger
            var trigger = Undo.AddComponent<TooltipTrigger>(selected);

            // Create panel as child
            var panel = CreateTooltipPanel(selected.transform);

            // Wire
            trigger.tooltipKey = tooltipKey;
            trigger.tooltipPanel = panel.gameObject;
            trigger.tooltipText = panel.GetComponentInChildren<TMP_Text>();
            trigger.showAnimation = panel.GetComponent<TweenAnimation>();

            panel.gameObject.SetActive(false);

            EditorUtility.SetDirty(selected);
            Debug.Log($"TooltipTrigger added to: {selected.name}");
        }

        private void CreateFolderRecursive(string path)
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
#endif
