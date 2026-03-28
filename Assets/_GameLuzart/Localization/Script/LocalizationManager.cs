namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class LocalizationManager : Singleton<LocalizationManager>
    {
        private const string PREF_LANGUAGE = "language";
        private const string RESOURCE_PATH = "Localization/";

        private Dictionary<string, string> localizedStrings = new Dictionary<string, string>();
        private string currentLanguage = "en";

        public string CurrentLanguage => currentLanguage;
        public Action OnLanguageChanged;

        private void Awake()
        {
            currentLanguage = PlayerPrefs.GetString(PREF_LANGUAGE, "en");
            LoadLanguage(currentLanguage);
        }

        public void SetLanguage(string lang)
        {
            if (currentLanguage == lang) return;
            currentLanguage = lang;
            PlayerPrefs.SetString(PREF_LANGUAGE, lang);
            PlayerPrefs.Save();
            LoadLanguage(lang);
            OnLanguageChanged?.Invoke();
            Observer.Instance?.Notify(ObserverKey.OnLanguageChanged);
        }

        private void LoadLanguage(string lang)
        {
            localizedStrings.Clear();
            TextAsset textAsset = Resources.Load<TextAsset>(RESOURCE_PATH + lang);
            if (textAsset == null)
            {
                Debug.LogError($"[Localization] File not found: {RESOURCE_PATH}{lang}.json");
                return;
            }
            var data = JsonUtility.FromJson<LocalizationData>(textAsset.text);
            if (data != null && data.items != null)
            {
                foreach (var item in data.items)
                {
                    localizedStrings[item.key] = item.value;
                }
            }
            Debug.Log($"[Localization] Loaded {localizedStrings.Count} keys for '{lang}'");
        }

        /// <summary>
        /// Get localized string by key. Returns key if not found.
        /// </summary>
        public string Get(string key)
        {
            if (localizedStrings.TryGetValue(key, out string value))
            {
                return value;
            }
            Debug.LogWarning($"[Localization] Key not found: {key}");
            return key;
        }

        /// <summary>
        /// Get localized string with format arguments. Uses {0}, {1} etc.
        /// </summary>
        public string GetFormat(string key, params object[] args)
        {
            string template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template;
            }
        }

        /// <summary>
        /// Get prompt with named placeholders like {mentorName}, {subject} etc.
        /// </summary>
        public string GetPrompt(string key, Dictionary<string, string> replacements = null)
        {
            string template = Get(key);
            if (replacements != null)
            {
                foreach (var pair in replacements)
                {
                    template = template.Replace("{" + pair.Key + "}", pair.Value);
                }
            }
            return template;
        }

        public bool HasKey(string key)
        {
            return localizedStrings.ContainsKey(key);
        }
    }

    [Serializable]
    public class LocalizationData
    {
        public LocalizationItem[] items;
    }

    [Serializable]
    public class LocalizationItem
    {
        public string key;
        public string value;
    }
}
