#if UNITY_EDITOR
namespace Luzart.Editor
{
    using System.IO;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Builds the UILanguageSelect prefab from existing game art.
    /// Run via: Tools ▶ Login ▶ Build UILanguageSelect Prefab
    /// </summary>
    public static class UILanguageSelectBuilder
    {
        // ── Art paths ────────────────────────────────────────────────────
        private const string BG_PATH    = "Assets/_Game/Art/Newfix/log in/Layer 2134.png";
        private const string FRAME_PATH = "Assets/_Game/Art/Newfix/log in/frame.png";
        private const string LANG_ICON  = "Assets/GUI_Sci_FI/Sliced Elements/12_Setting/setting_icon_language.png";
        private const string CARD_N     = "Assets/GUI_Sci_FI/Sliced Elements/00_Common/item_frame_n.png";
        private const string CARD_F     = "Assets/GUI_Sci_FI/Sliced Elements/00_Common/item_frame_f.png";
        private const string DIMMED     = "Assets/GUI_Sci_FI/Sliced Elements/00_Common/screen_dimmed.png";

        private const string FONT_REG   = "Assets/_GameLuzart/Font/Montserrat-SemiBold SDF.asset";
        private const string FONT_TECH  = "Assets/_GameLuzart/Font/Technology-Bold SDF.asset";
        private const string SAVE_PATH  = "Assets/_Game/Resources/UILanguageSelect.prefab";

        // ── Palette ──────────────────────────────────────────────────────
        private static readonly Color Cyan      = new Color(0f,    0.85f, 1f,    1f);
        private static readonly Color CyanDim   = new Color(0f,    0.60f, 0.80f, 0.5f);
        private static readonly Color DarkBg    = new Color(0.03f, 0.07f, 0.12f, 0.97f);
        private static readonly Color CardDark  = new Color(0.06f, 0.12f, 0.18f, 1f);

        [MenuItem("Tools/Login/Build UILanguageSelect Prefab")]
        public static void Build()
        {
            var spBg      = Load<Sprite>(BG_PATH);
            var spFrame   = Load<Sprite>(FRAME_PATH);
            var spLangIco = Load<Sprite>(LANG_ICON);
            var spCardN   = Load<Sprite>(CARD_N);
            var spCardF   = Load<Sprite>(CARD_F);
            var spDimmed  = Load<Sprite>(DIMMED);
            var fontReg   = Load<TMP_FontAsset>(FONT_REG);
            var fontTech  = Load<TMP_FontAsset>(FONT_TECH);

            // ── Root ─────────────────────────────────────────────────────
            var root   = new GameObject("UILanguageSelect");
            var rootRT = root.AddComponent<RectTransform>();
            Stretch(rootRT);

            var uiLang = root.AddComponent<UILanguageSelect>();
            uiLang.uiName  = UIName.LanguageSelect;
            uiLang.isCache = true;
            uiLang.spriteCardNormal   = spCardN;
            uiLang.spriteCardSelected = spCardF;

            // Background
            if (spBg != null)
            {
                var imgBg = CreateImg("ImgBg", root.transform, spBg, Color.white, false);
                Stretch(RT(imgBg));
            }
            else
            {
                var imgBg = CreateImg("ImgBg", root.transform, null, DarkBg, true);
                Stretch(RT(imgBg));
            }

            // Dimmed overlay
            if (spDimmed != null)
            {
                var dim = CreateImg("ImgDim", root.transform, spDimmed, new Color(0,0,0,0.5f), false);
                Stretch(RT(dim));
            }

            // ── Center card ───────────────────────────────────────────────
            var card   = new GameObject("Card");
            var cardRT = card.AddComponent<RectTransform>();
            card.transform.SetParent(root.transform, false);
            Anchor(cardRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            cardRT.anchoredPosition = Vector2.zero;
            cardRT.sizeDelta        = new Vector2(560f, 480f);

            if (spFrame != null)
            {
                var cImg  = card.AddComponent<Image>();
                cImg.sprite = spFrame;
                cImg.type   = Image.Type.Sliced;
                cImg.color  = Color.white;
            }
            else
            {
                var cImg  = card.AddComponent<Image>();
                cImg.color = CardDark;
            }

            // Language icon
            if (spLangIco != null)
            {
                var ico   = CreateImg("ImgLangIcon", card.transform, spLangIco, Cyan, false);
                var icoRT = RT(ico);
                Anchor(icoRT, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
                icoRT.anchoredPosition = new Vector2(0f, -30f);
                icoRT.sizeDelta        = new Vector2(64f, 64f);
            }

            // Title
            var titleGO = CreateTMP("TxtTitle", card.transform,
                "SELECT LANGUAGE", fontTech ?? fontReg, 26f, Color.white);
            var titleRT = RT(titleGO);
            Anchor(titleRT, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            titleRT.anchoredPosition = new Vector2(0f, -108f);
            titleRT.sizeDelta        = new Vector2(480f, 40f);
            titleGO.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            // Subtitle
            var subGO = CreateTMP("TxtSubtitle", card.transform,
                "Chọn ngôn ngữ", fontReg, 15f, CyanDim);
            var subRT = RT(subGO);
            Anchor(subRT, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            subRT.anchoredPosition = new Vector2(0f, -150f);
            subRT.sizeDelta        = new Vector2(480f, 28f);
            subGO.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            // ── Divider ────────────────────────────────────────────────────
            var div   = new GameObject("Divider");
            var divRT = div.AddComponent<RectTransform>();
            div.transform.SetParent(card.transform, false);
            Anchor(divRT, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            divRT.anchoredPosition = new Vector2(0f, -184f);
            divRT.sizeDelta        = new Vector2(460f, 2f);
            div.AddComponent<Image>().color = CyanDim;

            // ── Language Cards Row ─────────────────────────────────────────
            var row   = new GameObject("LanguageRow");
            var rowRT = row.AddComponent<RectTransform>();
            row.transform.SetParent(card.transform, false);
            Anchor(rowRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rowRT.anchoredPosition = new Vector2(0f, 10f);
            rowRT.sizeDelta        = new Vector2(460f, 180f);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing            = 20f;
            hlg.childAlignment     = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(0, 0, 0, 0);

            var (enGO, enBtn, enBg, enBadge) = CreateLangCard("CardEnglish",  row.transform, "EN", "English",      spCardN, spCardF, fontReg, fontTech);
            var (viGO, viBtn, viBg, viBadge) = CreateLangCard("CardVietnamese", row.transform, "VI", "Tiếng Việt", spCardN, spCardF, fontReg, fontTech);

            // Hint at bottom
            var hintGO = CreateTMP("TxtHint", card.transform,
                "Tap to select • Nhấn để chọn", fontReg, 11f, new Color(0.5f,0.6f,0.7f,0.8f));
            var hintRT = RT(hintGO);
            Anchor(hintRT, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            hintRT.anchoredPosition = new Vector2(0f, 38f);
            hintRT.sizeDelta        = new Vector2(460f, 28f);
            hintGO.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            // ── Wire References ────────────────────────────────────────────
            uiLang.btnEnglish    = enBtn;
            uiLang.btnVietnamese = viBtn;
            uiLang.imgEnCardBg   = enBg;
            uiLang.imgViCardBg   = viBg;
            uiLang.txtEnBadge    = enBadge;
            uiLang.txtViBadge    = viBadge;

            // ── Save Prefab ────────────────────────────────────────────────
            string dir = Path.GetDirectoryName(SAVE_PATH);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            PrefabUtility.SaveAsPrefabAsset(root, SAVE_PATH);
            GameObject.DestroyImmediate(root);
            AssetDatabase.Refresh();

            Debug.Log($"[UILanguageSelectBuilder] Saved → {SAVE_PATH}");
            EditorUtility.DisplayDialog("UILanguageSelect Builder",
                $"Prefab created!\n\n{SAVE_PATH}", "OK");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Language Card factory
        // ═══════════════════════════════════════════════════════════════════
        private static (GameObject go, Button btn, Image bg, TMP_Text badge)
            CreateLangCard(string name, Transform parent,
                string code, string label,
                Sprite spNormal, Sprite spFocus,
                TMP_FontAsset fontReg, TMP_FontAsset fontTech)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            // Card background image
            var bg        = go.AddComponent<Image>();
            bg.sprite     = spNormal;
            bg.type       = spNormal != null ? Image.Type.Sliced : Image.Type.Simple;
            bg.color      = new Color(0.06f, 0.13f, 0.20f, 1f);

            // Button (no transition — visuals handled by UILanguageSelect.HighlightCard)
            var btn           = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.transition    = Selectable.Transition.None;

            // Inner vertical layout
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing            = 10f;
            vlg.childAlignment     = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth  = false;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(10, 10, 24, 16);

            // Code badge ("EN" / "VI") — large
            var badgeGO  = CreateTMP("TxtCode", go.transform, code, fontTech ?? fontReg, 48f, Cyan);
            var badgeTMP = badgeGO.GetComponent<TMP_Text>();
            badgeTMP.fontStyle = FontStyles.Bold;
            badgeTMP.alignment = TextAlignmentOptions.Center;
            var badgeRT = RT(badgeGO);
            badgeRT.sizeDelta = new Vector2(160f, 64f);
            var badgeLe = badgeGO.AddComponent<LayoutElement>();
            badgeLe.preferredWidth  = 160f;
            badgeLe.preferredHeight = 64f;

            // Language label ("English" / "Tiếng Việt")
            var lblGO  = CreateTMP("TxtLabel", go.transform, label, fontReg, 15f, Color.white);
            var lblTMP = lblGO.GetComponent<TMP_Text>();
            lblTMP.alignment = TextAlignmentOptions.Center;
            var lblRT  = RT(lblGO);
            lblRT.sizeDelta = new Vector2(160f, 28f);
            var lblLe  = lblGO.AddComponent<LayoutElement>();
            lblLe.preferredWidth  = 160f;
            lblLe.preferredHeight = 28f;

            // Cyan bottom accent line
            var line   = new GameObject("AccentLine");
            line.transform.SetParent(go.transform, false);
            var lineRT = line.AddComponent<RectTransform>();
            Anchor(lineRT, new Vector2(0.1f, 0f), new Vector2(0.9f, 0f));
            lineRT.anchoredPosition = new Vector2(0f, 10f);
            lineRT.sizeDelta        = new Vector2(0f, 3f);
            line.AddComponent<Image>().color = new Color(0f, 0.85f, 1f, 0.6f);
            var lineLe = line.AddComponent<LayoutElement>();
            lineLe.ignoreLayout = true;

            return (go, btn, bg, badgeTMP);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Helpers
        // ═══════════════════════════════════════════════════════════════════
        private static T Load<T>(string path) where T : Object
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null) Debug.LogWarning($"[LangBuilder] Not found: {path}");
            return a;
        }
        private static RectTransform RT(GameObject go) => go.GetComponent<RectTransform>();
        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }
        private static void Anchor(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min; rt.anchorMax = max;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }
        private static GameObject CreateImg(string name, Transform parent,
            Sprite sp, Color col, bool raycast)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            var img = go.AddComponent<Image>();
            img.sprite = sp; img.color = col; img.raycastTarget = raycast;
            return go;
        }
        private static GameObject CreateTMP(string name, Transform parent,
            string text, TMP_FontAsset font, float size, Color col)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.font = font;
            tmp.fontSize = size; tmp.color = col;
            tmp.raycastTarget = false;
            return go;
        }
    }
}
#endif
