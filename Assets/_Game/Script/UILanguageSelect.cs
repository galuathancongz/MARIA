namespace Luzart
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Language selection screen — shown once on first launch.
    /// Tapping a language card immediately applies the language and navigates to Login.
    /// </summary>
    public class UILanguageSelect : UIBase
    {
        private const string PREF_LANG_CHOSEN = "lang_chosen";

        [Header("Language Cards (click to select)")]
        public Button btnEnglish;
        public Button btnVietnamese;

        [Header("Card Frames (swap sprite on select)")]
        public Image imgEnCardBg;
        public Image imgViCardBg;

        [Header("Sprites")]
        public Sprite spriteCardNormal;
        public Sprite spriteCardSelected;

        [Header("Badge Labels (optional tint)")]
        public TMP_Text txtEnBadge;
        public TMP_Text txtViBadge;

        // ── static helper used by UISplash ──────────────────────────────
        public static bool HasChosenLanguage() =>
            PlayerPrefs.GetInt(PREF_LANG_CHOSEN, 0) == 1;

        // ── UIBase overrides ────────────────────────────────────────────
        protected override void Setup()
        {
            base.Setup();
            if (btnEnglish    != null) GameUtil.ButtonOnClick(btnEnglish,    () => Choose("en"), false);
            if (btnVietnamese != null) GameUtil.ButtonOnClick(btnVietnamese, () => Choose("vi"), false);
        }

        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);

            // Highlight whichever language is currently active
            string cur = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.CurrentLanguage
                : "en";
            HighlightCard(cur);
        }

        // ── Private ─────────────────────────────────────────────────────
        private void Choose(string lang)
        {
            // Apply language
            LocalizationManager.Instance?.SetLanguage(lang);
            Loc.ClearCache();

            // Mark as chosen so we never show this screen again
            PlayerPrefs.SetInt(PREF_LANG_CHOSEN, 1);
            PlayerPrefs.Save();

            Hide();
            UIManager.Instance.ShowUI(UIName.Login);
        }

        private void HighlightCard(string lang)
        {
            bool isEn = lang == "en";

            // Swap sprite
            if (imgEnCardBg != null)
                imgEnCardBg.sprite = isEn ? spriteCardSelected : spriteCardNormal;
            if (imgViCardBg != null)
                imgViCardBg.sprite = isEn ? spriteCardNormal : spriteCardSelected;

            // Tint badge text
            Color activeColor   = new Color(0f, 0.85f, 1f, 1f);   // cyan
            Color inactiveColor = new Color(0.5f, 0.6f, 0.7f, 1f);
            if (txtEnBadge != null) txtEnBadge.color = isEn ? activeColor : inactiveColor;
            if (txtViBadge != null) txtViBadge.color = isEn ? inactiveColor : activeColor;
        }
    }
}
