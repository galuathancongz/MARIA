namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Static helper: translates any English string at runtime.
    /// Uses reverse-lookup from en.json to find the key, then returns the localized value.
    /// If no match found, returns the original string unchanged.
    ///
    /// Usage: string localized = Loc.T("Continue");
    /// </summary>
    public static class Loc
    {
        private static Dictionary<string, string> reverseMap;
        private static bool isBuilt = false;

        /// <summary>
        /// Translate a string. If it matches an English value in en.json,
        /// returns the current language version. Otherwise returns original.
        /// </summary>
        public static string T(string englishText)
        {
            if (string.IsNullOrEmpty(englishText)) return englishText;
            if (LocalizationManager.Instance == null) return englishText;

            BuildReverseMap();

            // Try exact match first, then normalized (handles curly quotes, em-dashes, etc.)
            string trimmed = englishText.Trim();
            if (reverseMap.TryGetValue(trimmed, out string key))
                return LocalizationManager.Instance.Get(key);

            string normalized = Normalize(englishText);
            if (reverseMap.TryGetValue(normalized, out key))
                return LocalizationManager.Instance.Get(key);

            return englishText;
        }

        /// <summary>
        /// Translate with format args: Loc.TF("Reach to level {0}!", 5)
        /// </summary>
        public static string TF(string englishText, params object[] args)
        {
            string translated = T(englishText);
            try { return string.Format(translated, args); }
            catch { return translated; }
        }

        /// <summary>
        /// Force get by key (shorthand for LocalizationManager.Instance.Get)
        /// </summary>
        public static string K(string key)
        {
            if (LocalizationManager.Instance == null) return key;
            return LocalizationManager.Instance.Get(key);
        }

        /// <summary>
        /// Normalizes a string for reverse-lookup comparison.
        /// Converts smart/curly quotes, em/en dashes, ellipsis, and zero-width spaces
        /// to their plain ASCII equivalents, then trims whitespace.
        /// This allows TMP_Text values from prefabs (which often use Unicode typography)
        /// to match en.json entries written with plain ASCII characters.
        /// </summary>
        public static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text
                .Replace('\u2018', '\'').Replace('\u2019', '\'')  // ' ' → '
                .Replace('\u201C', '"') .Replace('\u201D', '"')   // " " → "
                .Replace('\u2013', '-') .Replace('\u2014', '-')   // – — → -
                .Replace("\u2026", "...").Replace("\u200B", "")   // … → ..., ZWSP → ""
                .Trim();
        }

        private static void BuildReverseMap()
        {
            if (isBuilt && reverseMap != null) return;

            reverseMap = new Dictionary<string, string>();
            TextAsset enAsset = Resources.Load<TextAsset>("Localization/en");
            if (enAsset == null) return;

            var data = JsonUtility.FromJson<LocalizationData>(enAsset.text);
            if (data?.items == null) return;

            foreach (var item in data.items)
            {
                if (!string.IsNullOrEmpty(item.value))
                {
                    // Store both exact and normalized forms so either can be looked up
                    string exact = item.value.Trim();
                    string normalized = Normalize(item.value);
                    if (!reverseMap.ContainsKey(exact))
                        reverseMap[exact] = item.key;
                    if (!reverseMap.ContainsKey(normalized))
                        reverseMap[normalized] = item.key;
                }
            }
            isBuilt = true;
        }

        /// <summary>
        /// Call when language changes to allow rebuild on next T() call
        /// </summary>
        public static void ClearCache()
        {
            isBuilt = false;
            reverseMap = null;
        }
    }
}
