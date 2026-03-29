#if UNITY_EDITOR
namespace Luzart.Editor
{
    using System.IO;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// One-click tool that builds the UILogin prefab from game art assets.
    /// Run via: Tools ▶ Login ▶ Build UILogin Prefab
    /// Prefab saved to: Assets/_Game/Resources/UILogin.prefab
    /// </summary>
    public static class UILoginBuilder
    {
        private const string ART_PATH    = "Assets/_Game/Art/Newfix/log in/";
        private const string FONT_REGULAR = "Assets/_GameLuzart/Font/Montserrat-SemiBold SDF.asset";
        private const string FONT_TECH    = "Assets/_GameLuzart/Font/Technology-Bold SDF.asset";
        private const string SAVE_PATH    = "Assets/_Game/Resources/UILogin.prefab";

        // Cyan / teal palette matching the sci-fi reference design
        private static readonly Color CyanMain   = new Color(0f,    0.80f, 1f,    1f);
        private static readonly Color CyanDim    = new Color(0f,    0.60f, 0.80f, 0.5f);
        private static readonly Color DarkPanel  = new Color(0.04f, 0.08f, 0.12f, 0.95f);
        private static readonly Color DarkField  = new Color(0.08f, 0.14f, 0.20f, 1f);
        private static readonly Color ErrorRed   = new Color(1f,    0.30f, 0.30f, 1f);

        [MenuItem("Tools/Login/Build UILogin Prefab")]
        public static void Build()
        {
            // ── Load Art Assets ────────────────────────────────────────────
            Sprite spBg      = Load<Sprite>(ART_PATH + "Layer 2134.png");
            Sprite spLayer   = Load<Sprite>(ART_PATH + "Layer 2135.png");
            Sprite spFrame   = Load<Sprite>(ART_PATH + "frame.png");
            Sprite spBoard   = Load<Sprite>(ART_PATH + "board_phu.png");
            Sprite spProfile = Load<Sprite>(ART_PATH + "icon_profile.png");
            Sprite spRect    = Load<Sprite>(ART_PATH + "Rectangle 1986.png");
            Sprite spRectAlt = Load<Sprite>(ART_PATH + "Rectangle 1986 copy.png");

            TMP_FontAsset fontRegular = Load<TMP_FontAsset>(FONT_REGULAR);
            TMP_FontAsset fontTech    = Load<TMP_FontAsset>(FONT_TECH);

            // ── Root: UILogin ─────────────────────────────────────────────
            var root   = new GameObject("UILogin");
            var rootRT = root.AddComponent<RectTransform>();
            Stretch(rootRT);

            var uiLogin = root.AddComponent<UILogin>();
            uiLogin.uiName  = UIName.Login;
            uiLogin.isCache = true;

            // Dark overlay (blocks raycasts so user can't click behind)
            var blocker = CreateImage("Blocker", root.transform, null, DarkPanel, true);
            Stretch(RT(blocker));

            // Background layer 2134
            if (spBg != null)
            {
                var imgBg = CreateImage("ImgBackground", root.transform, spBg, Color.white, false);
                Stretch(RT(imgBg));
            }

            // Decorative layer 2135
            if (spLayer != null)
            {
                var imgLayer = CreateImage("ImgLayer", root.transform, spLayer, Color.white, false);
                Stretch(RT(imgLayer));
            }

            // ── Main Card / Frame ─────────────────────────────────────────
            var card = new GameObject("Card");
            var cardRT = card.AddComponent<RectTransform>();
            card.transform.SetParent(root.transform, false);
            Center(cardRT, new Vector2(560f, 680f));

            if (spFrame != null)
            {
                var imgFrame = card.AddComponent<Image>();
                imgFrame.sprite = spFrame;
                imgFrame.type   = Image.Type.Sliced;
                imgFrame.color  = Color.white;
                card.AddComponent<CanvasRenderer>();
            }
            else
            {
                // Fallback: dark rounded panel
                var imgFrame = card.AddComponent<Image>();
                imgFrame.color = DarkPanel;
            }

            // Board decoration (top-left corner accent)
            if (spBoard != null)
            {
                var imgBoard = CreateImage("ImgBoard", card.transform, spBoard, Color.white, false);
                var rt = RT(imgBoard);
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot     = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(-28f, 28f);
                rt.sizeDelta = new Vector2(110f, 110f);
            }

            // ── Profile Icon ─────────────────────────────────────────────
            var iconObj = CreateImage("ImgProfile", card.transform, spProfile, Color.white, false);
            var iconRT  = RT(iconObj);
            iconRT.anchorMin = iconRT.anchorMax = new Vector2(0.5f, 1f);
            iconRT.pivot     = new Vector2(0.5f, 1f);
            iconRT.anchoredPosition = new Vector2(0f, -24f);
            iconRT.sizeDelta = new Vector2(78f, 78f);

            // ── Title ─────────────────────────────────────────────────────
            var titleGO = CreateTMP("TxtTitle", card.transform, "MARIA",
                fontTech ?? fontRegular, 40f, CyanMain);
            var titleRT = RT(titleGO);
            titleRT.anchorMin = titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -112f);
            titleRT.sizeDelta = new Vector2(360f, 52f);
            titleGO.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            // ── Subtitle ─────────────────────────────────────────────────
            var subtGO = CreateTMP("TxtSubtitle", card.transform, "TEACHING CLASSROOM",
                fontRegular, 13f, CyanDim);
            var subtRT = RT(subtGO);
            subtRT.anchorMin = subtRT.anchorMax = new Vector2(0.5f, 1f);
            subtRT.anchoredPosition = new Vector2(0f, -158f);
            subtRT.sizeDelta = new Vector2(360f, 26f);
            subtGO.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            // ── Tab Row ────────────────────────────────────────────────────
            var tabRow   = new GameObject("TabRow");
            var tabRowRT = tabRow.AddComponent<RectTransform>();
            tabRow.transform.SetParent(card.transform, false);
            tabRowRT.anchorMin = tabRowRT.anchorMax = new Vector2(0.5f, 1f);
            tabRowRT.anchoredPosition = new Vector2(0f, -202f);
            tabRowRT.sizeDelta = new Vector2(460f, 52f);

            var hlg = tabRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing            = 6f;
            hlg.childAlignment     = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(4, 4, 0, 0);

            var (tabSignInGO, tabSignIn) = CreateTabButton("BtnTabSignIn", tabRow.transform,
                "Sign In", spRect, fontRegular);
            var (tabSignUpGO, tabSignUp) = CreateTabButton("BtnTabSignUp", tabRow.transform,
                "Sign Up", spRectAlt, fontRegular);

            // ── Divider ───────────────────────────────────────────────────
            var div   = new GameObject("Divider");
            var divRT = div.AddComponent<RectTransform>();
            div.transform.SetParent(card.transform, false);
            divRT.anchorMin = divRT.anchorMax = new Vector2(0.5f, 1f);
            divRT.anchoredPosition = new Vector2(0f, -257f);
            divRT.sizeDelta = new Vector2(460f, 2f);
            var divImg = div.AddComponent<Image>();
            divImg.color = CyanDim;

            // ── Panel Sign In ─────────────────────────────────────────────
            var panelIn   = MakeFieldPanel("PanelSignIn", card.transform, new Vector2(0f, -20f), 200f);
            var inUser    = CreateInputField("InputUsername", panelIn.transform, "Username",  fontRegular, false);
            var inPass    = CreateInputField("InputPassword", panelIn.transform, "Password",  fontRegular, true);

            // ── Panel Sign Up ─────────────────────────────────────────────
            var panelUp   = MakeFieldPanel("PanelSignUp",    card.transform, new Vector2(0f, -30f), 356f);
            var upUser    = CreateInputField("InputUsername",    panelUp.transform, "Username",         fontRegular, false);
            var upPass    = CreateInputField("InputPassword",    panelUp.transform, "Password",         fontRegular, true);
            var upConfirm = CreateInputField("InputConfirmPass", panelUp.transform, "Confirm Password", fontRegular, true);
            var upEmail   = CreateInputField("InputEmail",       panelUp.transform, "Email (optional)", fontRegular, false);
            panelUp.SetActive(false);

            // ── Error Text ────────────────────────────────────────────────
            var errGO = CreateTMP("TxtError", card.transform, "", fontRegular, 13f, ErrorRed);
            errGO.SetActive(false);
            var errRT = RT(errGO);
            errRT.anchorMin = errRT.anchorMax = new Vector2(0.5f, 0f);
            errRT.anchoredPosition = new Vector2(0f, 145f);
            errRT.sizeDelta = new Vector2(460f, 40f);
            errGO.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            // ── Submit Button ─────────────────────────────────────────────
            var btnGO  = new GameObject("BtnSubmit");
            var btnRT  = btnGO.AddComponent<RectTransform>();
            btnGO.transform.SetParent(card.transform, false);
            btnRT.anchorMin = btnRT.anchorMax = new Vector2(0.5f, 0f);
            btnRT.anchoredPosition = new Vector2(0f, 68f);
            btnRT.sizeDelta = new Vector2(210f, 52f);

            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = CyanMain;
            var btn    = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            // Hover tint
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.7f, 1f, 1f, 1f);
            colors.pressedColor     = new Color(0f, 0.5f, 0.7f, 1f);
            btn.colors = colors;

            var txtBtnGO = CreateTMP("TxtBtnSubmit", btnGO.transform, "Log In",
                fontRegular, 18f, Color.black);
            Stretch(RT(txtBtnGO));
            var txtBtnComp = txtBtnGO.GetComponent<TMP_Text>();
            txtBtnComp.fontStyle = FontStyles.Bold;
            txtBtnComp.alignment = TextAlignmentOptions.Center;

            // ── Loading Spinner (simple pulsing dot) ──────────────────────
            var loadGO = CreateImage("ObjLoading", card.transform, null, CyanMain, true);
            loadGO.SetActive(false);
            var loadRT = RT(loadGO);
            loadRT.anchorMin = loadRT.anchorMax = new Vector2(0.5f, 0f);
            loadRT.anchoredPosition = new Vector2(80f, 70f);
            loadRT.sizeDelta = new Vector2(36f, 36f);
            loadGO.AddComponent<UISimpleSpin>();     // tiny spin component (created below)

            // ── Wire References into UILogin ──────────────────────────────
            uiLogin.btnTabSignIn = tabSignIn;
            uiLogin.btnTabSignUp = tabSignUp;
            uiLogin.panelSignIn  = panelIn;
            uiLogin.panelSignUp  = panelUp;
            uiLogin.inputSignInUsername = inUser;
            uiLogin.inputSignInPassword = inPass;
            uiLogin.inputSignUpUsername = upUser;
            uiLogin.inputSignUpPassword = upPass;
            uiLogin.inputSignUpConfirm  = upConfirm;
            uiLogin.inputSignUpEmail    = upEmail;
            uiLogin.btnSubmit           = btn;
            uiLogin.txtBtnSubmit        = txtBtnComp;
            uiLogin.txtError            = errGO.GetComponent<TMP_Text>();
            uiLogin.loadingIndicator    = loadGO;

            // ── Save Prefab ───────────────────────────────────────────────
            string dir = Path.GetDirectoryName(SAVE_PATH);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, SAVE_PATH);
            GameObject.DestroyImmediate(root);
            AssetDatabase.Refresh();

            Debug.Log($"[UILoginBuilder] Prefab saved → {SAVE_PATH}");
            EditorUtility.DisplayDialog("UILogin Builder",
                $"Prefab created successfully!\n\n{SAVE_PATH}", "OK");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Helper factory methods
        // ═══════════════════════════════════════════════════════════════════

        private static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                Debug.LogWarning($"[UILoginBuilder] Asset not found: {path}");
            return asset;
        }

        private static RectTransform RT(GameObject go) => go.GetComponent<RectTransform>();

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = Vector2.zero;
        }

        private static void Center(RectTransform rt, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
        }

        private static GameObject CreateImage(string name, Transform parent,
            Sprite sprite, Color color, bool raycast)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            var img = go.AddComponent<Image>();
            img.sprite         = sprite;
            img.color          = color;
            img.raycastTarget  = raycast;
            img.preserveAspect = sprite != null;
            return go;
        }

        private static GameObject CreateTMP(string name, Transform parent,
            string text, TMP_FontAsset font, float size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.font          = font;
            tmp.fontSize      = size;
            tmp.color         = color;
            tmp.raycastTarget = false;
            return go;
        }

        // ── Tab Button ─────────────────────────────────────────────────────
        private static (GameObject go, UILoginTabButton tab) CreateTabButton(
            string name, Transform parent, string label, Sprite bgSprite, TMP_FontAsset font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var bgImg   = go.AddComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.type   = bgSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            bgImg.color  = new Color(0.08f, 0.14f, 0.20f, 0.9f); // deselected

            var btn     = go.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            btn.transition    = Selectable.Transition.None; // handled by UILoginTabButton

            var tab = go.AddComponent<UILoginTabButton>();
            tab.tabBackground = bgImg;

            // Label
            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(go.transform, false);
            var lblRT = lblGO.AddComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.anchoredPosition = Vector2.zero;
            lblRT.sizeDelta = Vector2.zero;
            var tmp = lblGO.AddComponent<TextMeshProUGUI>();
            tmp.text          = label;
            tmp.font          = font;
            tmp.fontSize      = 16f;
            tmp.fontStyle     = FontStyles.Bold;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.color         = new Color(0.7f, 0.9f, 1f, 1f);
            tmp.raycastTarget = false;

            tab.tabLabel = tmp;

            return (go, tab);
        }

        // ── Input Field Panel ──────────────────────────────────────────────
        private static GameObject MakeFieldPanel(string name, Transform parent,
            Vector2 offset, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(460f, height);

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing            = 16f;
            vlg.padding            = new RectOffset(0, 0, 0, 0);
            vlg.childAlignment     = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth  = false;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight     = false;
            vlg.childControlWidth      = false;

            return go;
        }

        // ── TMP Input Field ────────────────────────────────────────────────
        private static TMP_InputField CreateInputField(string name, Transform parent,
            string placeholder, TMP_FontAsset font, bool isPassword)
        {
            // Root
            var root   = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(460f, 52f);

            var rootImg = root.AddComponent<Image>();
            rootImg.color = new Color(0.08f, 0.14f, 0.20f, 1f);

            var inputField = root.AddComponent<TMP_InputField>();
            if (isPassword)
            {
                inputField.contentType = TMP_InputField.ContentType.Password;
                inputField.asteriskChar = '●';
            }

            // Text Area (with mask)
            var textArea   = new GameObject("Text Area");
            textArea.transform.SetParent(root.transform, false);
            var textAreaRT = textArea.AddComponent<RectTransform>();
            textAreaRT.anchorMin = Vector2.zero;
            textAreaRT.anchorMax = Vector2.one;
            textAreaRT.anchoredPosition = Vector2.zero;
            textAreaRT.sizeDelta = new Vector2(-24f, -8f);
            textArea.AddComponent<RectMask2D>();

            // Placeholder text
            var placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(textArea.transform, false);
            var phRT = placeholderGO.AddComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.anchoredPosition = Vector2.zero;
            phRT.sizeDelta = Vector2.zero;
            var phTMP = placeholderGO.AddComponent<TextMeshProUGUI>();
            phTMP.text      = placeholder;
            phTMP.font      = font;
            phTMP.fontSize  = 15f;
            phTMP.color     = new Color(0.5f, 0.7f, 0.8f, 0.7f);
            phTMP.alignment = TextAlignmentOptions.MidlineLeft;
            phTMP.fontStyle = FontStyles.Italic;

            // Actual input text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(textArea.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.anchoredPosition = Vector2.zero;
            textRT.sizeDelta = Vector2.zero;
            var textTMP = textGO.AddComponent<TextMeshProUGUI>();
            textTMP.font      = font;
            textTMP.fontSize  = 15f;
            textTMP.color     = new Color(0.85f, 0.95f, 1f, 1f);
            textTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // Wire TMP_InputField
            inputField.textComponent = textTMP;
            inputField.placeholder   = phTMP;
            inputField.fontAsset     = font;
            inputField.pointSize     = 15f;

            return inputField;
        }
    }
}
#endif
