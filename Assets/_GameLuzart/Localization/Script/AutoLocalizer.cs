namespace Luzart
{
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Attach to any root GameObject (prefab root, panel, etc.)
    /// It will find all TMP_Text children and auto-translate them at runtime
    /// by matching their English text to localization keys.
    /// </summary>
    public class AutoLocalizer : MonoBehaviour
    {
        [Tooltip("If true, scan children on Enable. If false, call Localize() manually.")]
        [SerializeField] private bool localizeOnEnable = true;

        // Cache: English text → localization key
        private static Dictionary<string, string> reverseMap;

        private void OnEnable()
        {
            if (localizeOnEnable)
            {
                Localize();
            }
            Observer.Instance?.AddObserver(ObserverKey.OnLanguageChanged, OnLanguageChanged);
        }

        private void OnDisable()
        {
            Observer.Instance?.RemoveObserver(ObserverKey.OnLanguageChanged, OnLanguageChanged);
        }

        private void OnLanguageChanged(object data)
        {
            Localize();
        }

        public void Localize()
        {
            if (LocalizationManager.Instance == null) return;

            BuildReverseMap();

            var tmpTexts = GetComponentsInChildren<TMP_Text>(true);
            foreach (var tmp in tmpTexts)
            {
                // Skip if already has LocalizedText component
                if (tmp.GetComponent<LocalizedText>() != null) continue;

                string text = tmp.text;
                if (string.IsNullOrWhiteSpace(text) || text.Length <= 1) continue;

                // Try exact match
                if (reverseMap.TryGetValue(text.Trim(), out string key))
                {
                    tmp.text = LocalizationManager.Instance.Get(key);
                }
            }
        }

        private static void BuildReverseMap()
        {
            if (reverseMap != null) return;

            reverseMap = new Dictionary<string, string>();

            // Load English JSON to build reverse mapping
            TextAsset enAsset = Resources.Load<TextAsset>("Localization/en");
            if (enAsset == null) return;

            var data = JsonUtility.FromJson<LocalizationData>(enAsset.text);
            if (data?.items == null) return;

            foreach (var item in data.items)
            {
                if (!string.IsNullOrEmpty(item.value) && !reverseMap.ContainsKey(item.value.Trim()))
                {
                    reverseMap[item.value.Trim()] = item.key;
                }
            }
        }

        /// <summary>
        /// Call this when language changes to rebuild the reverse map
        /// </summary>
        public static void ClearCache()
        {
            reverseMap = null;
        }
    }
}
