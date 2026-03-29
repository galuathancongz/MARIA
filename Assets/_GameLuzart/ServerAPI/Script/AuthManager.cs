namespace Luzart
{
    using System;
    using UnityEngine;

    public class AuthManager : Singleton<AuthManager>
    {
        private const string PREF_TOKEN = "auth_token";
        private const string PREF_USERNAME = "auth_username";
        private const string PREF_USERID = "auth_userid";

        public bool IsLoggedIn => ApiClient.Instance != null && ApiClient.Instance.HasToken;
        public string CurrentUsername => PlayerPrefs.GetString(PREF_USERNAME, "");
        public int CurrentUserId => PlayerPrefs.GetInt(PREF_USERID, -1);

        public Action OnLoginSuccess;
        public Action OnLogout;

        private void Start()
        {
            // Try restore token from PlayerPrefs
            string savedToken = PlayerPrefs.GetString(PREF_TOKEN, "");
            if (!string.IsNullOrEmpty(savedToken))
            {
                ApiClient.Instance.SetToken(savedToken);
                Debug.Log($"[AuthManager] Token restored for user: {CurrentUsername}");
            }
        }

        public void Register(string username, string password, string email = "", Action<AuthResponse> onSuccess = null, Action<string> onError = null)
        {
            var request = new AuthRequest { username = username, password = password, email = email };

            ApiClient.Instance.Post<AuthRequest, AuthResponse>("/api/auth/register", request,
                (response) =>
                {
                    if (response.success)
                    {
                        SaveAuth(response.token, response.username, response.userId);
                        Debug.Log($"[AuthManager] Registered: {response.username}");
                        onSuccess?.Invoke(response);
                        OnLoginSuccess?.Invoke();
                    }
                    else
                    {
                        onError?.Invoke(response.message);
                    }
                },
                onError
            );
        }

        public void Login(string username, string password, Action<AuthResponse> onSuccess = null, Action<string> onError = null)
        {
            var request = new AuthRequest { username = username, password = password };

            ApiClient.Instance.Post<AuthRequest, AuthResponse>("/api/auth/login", request,
                (response) =>
                {
                    if (response.success)
                    {
                        SaveAuth(response.token, response.username, response.userId);
                        Debug.Log($"[AuthManager] Logged in: {response.username}");
                        onSuccess?.Invoke(response);
                        OnLoginSuccess?.Invoke();
                    }
                    else
                    {
                        onError?.Invoke(response.message);
                    }
                },
                onError
            );
        }

        public void Logout(Action onSuccess = null)
        {
            if (!IsLoggedIn)
            {
                ClearAuth();
                onSuccess?.Invoke();
                return;
            }

            // Save data to server before logout
            if (SyncManager.Instance != null)
            {
                SyncManager.Instance.SaveToServer(() =>
                {
                    DoLogout(onSuccess);
                });
            }
            else
            {
                DoLogout(onSuccess);
            }
        }

        private void DoLogout(Action onSuccess)
        {
            ApiClient.Instance.Post<ApiResponse>("/api/auth/logout", "{}",
                (response) =>
                {
                    ClearAuth();
                    Debug.Log("[AuthManager] Logged out");
                    onSuccess?.Invoke();
                    OnLogout?.Invoke();
                },
                (error) =>
                {
                    // Logout locally even if server call fails
                    ClearAuth();
                    onSuccess?.Invoke();
                    OnLogout?.Invoke();
                }
            );
        }

        private void SaveAuth(string token, string username, int userId)
        {
            ApiClient.Instance.SetToken(token);
            PlayerPrefs.SetString(PREF_TOKEN, token);
            PlayerPrefs.SetString(PREF_USERNAME, username);
            PlayerPrefs.SetInt(PREF_USERID, userId);
            PlayerPrefs.Save();
        }

        private void ClearAuth()
        {
            ApiClient.Instance.ClearToken();
            PlayerPrefs.DeleteKey(PREF_TOKEN);
            PlayerPrefs.DeleteKey(PREF_USERNAME);
            PlayerPrefs.DeleteKey(PREF_USERID);
            PlayerPrefs.Save();
        }
    }
}
