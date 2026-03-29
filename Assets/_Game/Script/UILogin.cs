namespace Luzart
{
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UILogin : UIBase
    {
        [Header("Tabs")]
        public BaseSelect btnTabSignIn;
        public BaseSelect btnTabSignUp;

        [Header("Panels")]
        public GameObject panelSignIn;
        public GameObject panelSignUp;

        [Header("Sign In Fields")]
        public TMP_InputField inputSignInUsername;
        public TMP_InputField inputSignInPassword;

        [Header("Sign Up Fields")]
        public TMP_InputField inputSignUpUsername;
        public TMP_InputField inputSignUpPassword;
        public TMP_InputField inputSignUpConfirm;
        public TMP_InputField inputSignUpEmail;

        [Header("Common")]
        public Button btnSubmit;
        public TMP_Text txtBtnSubmit;
        public TMP_Text txtError;

        private bool isSignIn = true;
        private bool isProcessing = false;

        // ────────────── UIBase overrides ──────────────

        protected override void Setup()
        {
            base.Setup();

            // Use Button.onClick ONLY for tab switching.
            // Never subscribe to UILoginTabButton.OnTabSelected from here —
            // that would create: SwitchTab → Select → OnTabSelected → SwitchTab (StackOverflow).
            if (btnTabSignIn != null)
            {
                var b = btnTabSignIn.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(() => SwitchTab(true));
            }
            if (btnTabSignUp != null)
            {
                var b = btnTabSignUp.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(() => SwitchTab(false));
            }
            if (btnSubmit != null)
                GameUtil.ButtonOnClick(btnSubmit, OnClickSubmit, false);

            SwitchTab(true);
        }

        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            ClearAll();
        }

        // ────────────── Tab Switching ──────────────

        private void SwitchTab(bool toSignIn)
        {
            isSignIn = toSignIn;
            panelSignIn?.SetActive(toSignIn);
            panelSignUp?.SetActive(!toSignIn);
            btnTabSignIn?.Select(toSignIn);
            btnTabSignUp?.Select(!toSignIn);
            if (txtBtnSubmit != null)
                txtBtnSubmit.text = toSignIn ? Loc.T("Log In") : Loc.T("Sign Up");
            ClearError();
        }

        // ────────────── Submit Logic ──────────────

        private void OnClickSubmit()
        {
            if (isProcessing) return;

            if (isSignIn)
                DoLogin();
            else
                DoRegister();
        }

        private void DoLogin()
        {
            string username = inputSignInUsername != null ? inputSignInUsername.text.Trim() : "";
            string password = inputSignInPassword != null ? inputSignInPassword.text : "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError(Loc.K("ui.err_fill_fields"));
                return;
            }

            SetLoading(true);
            AuthManager.Instance.Login(username, password,
                (response) => LoadServerDataThenNavigate(),
                (error) =>
                {
                    SetLoading(false);
                    ShowError(!string.IsNullOrEmpty(error) ? error : Loc.K("ui.err_login_failed"));
                }
            );
        }

        private void DoRegister()
        {
            string username = inputSignUpUsername != null ? inputSignUpUsername.text.Trim() : "";
            string password = inputSignUpPassword != null ? inputSignUpPassword.text : "";
            string confirm  = inputSignUpConfirm  != null ? inputSignUpConfirm.text  : "";
            string email    = inputSignUpEmail    != null ? inputSignUpEmail.text.Trim() : "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError(Loc.K("ui.err_fill_fields"));
                return;
            }
            if (password.Length < 6)
            {
                ShowError(Loc.K("ui.err_password_short"));
                return;
            }
            if (password != confirm)
            {
                ShowError(Loc.K("ui.err_password_mismatch"));
                return;
            }
            if (!string.IsNullOrEmpty(email) && !email.Contains("@"))
            {
                ShowError(Loc.K("ui.err_email_invalid"));
                return;
            }

            SetLoading(true);
            AuthManager.Instance.Register(username, password, email,
                (response) => LoadServerDataThenNavigate(),
                (error) =>
                {
                    SetLoading(false);
                    ShowError(!string.IsNullOrEmpty(error) ? error : Loc.K("ui.err_register_failed"));
                }
            );
        }

        // ────────────── Navigation ──────────────

        /// <summary>
        /// Load fresh server data for the logged-in user, THEN navigate.
        /// This prevents the old user's PlayerPrefs from determining the route.
        /// SyncManager.OnLoginSuccess may also fire LoadFromServer concurrently — that is fine,
        /// both calls are independent and the data is idempotent.
        /// </summary>
        private void LoadServerDataThenNavigate()
        {
            if (SyncManager.Instance != null)
            {
                SyncManager.Instance.LoadFromServer(() =>
                {
                    SetLoading(false);
                    NavigateToGame();
                });
            }
            else
            {
                SetLoading(false);
                NavigateToGame();
            }
        }

        private void NavigateToGame()
        {
            Hide();
            int level = DataManager.Instance != null ? DataManager.Instance.CurrentLevel : 0;
            UIManager.Instance.ShowUI(level == 0 ? UIName.Tutorial : UIName.MainMenu);
        }

        // ────────────── Helpers ──────────────

        private void SetLoading(bool loading)
        {
            isProcessing = loading;
            if (btnSubmit != null) btnSubmit.interactable = !loading;
            if (loading)
            {
                UIManager.Instance.ShowLoading();
            }
            else
            {
                UIManager.Instance.HideLoading();
            }
        }

        private void ShowError(string msg)
        {
            if (txtError == null) return;
            txtError.text = msg;
            txtError.gameObject.SetActive(true);
        }

        private void ClearError()
        {
            if (txtError == null) return;
            txtError.text = "";
            txtError.gameObject.SetActive(false);
        }

        private void ClearAll()
        {
            if (inputSignInUsername != null) inputSignInUsername.text = "";
            if (inputSignInPassword != null) inputSignInPassword.text = "";
            if (inputSignUpUsername != null) inputSignUpUsername.text = "";
            if (inputSignUpPassword != null) inputSignUpPassword.text = "";
            if (inputSignUpConfirm  != null) inputSignUpConfirm.text  = "";
            if (inputSignUpEmail    != null) inputSignUpEmail.text    = "";
            ClearError();
            SetLoading(false);
            SwitchTab(true);
        }
    }
}
