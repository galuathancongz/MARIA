namespace Luzart
{
    using System;
    using System.Collections;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Networking;

    public class ApiClient : Singleton<ApiClient>
    {
        // ============ CONFIGURATION ============
        // Thay doi URL nay khi deploy len server
        // Neu chay local:  http://localhost:3000
        // Neu deploy VPS:  http://YOUR_SERVER_IP:3000
        // Neu co domain:   https://yourdomain.com
#if UNITY_EDITOR
        public const string BASE_URL = "https://api.gameaids.org";   // ← local dev
#else
        public const string BASE_URL = "https://api.gameaids.org"; // ← VPS production
#endif
        private const float TIMEOUT = 30f;

        private string authToken = "";

        public bool HasToken => !string.IsNullOrEmpty(authToken);

        public void SetToken(string token)
        {
            authToken = token;
        }

        public void ClearToken()
        {
            authToken = "";
        }

        // ============ PUBLIC API ============

        public void Get<TResponse>(string endpoint, Action<TResponse> onSuccess, Action<string> onError = null)
        {
            StartCoroutine(SendRequest("GET", endpoint, null, onSuccess, onError));
        }

        public void Post<TRequest, TResponse>(string endpoint, TRequest body, Action<TResponse> onSuccess, Action<string> onError = null)
        {
            string json = JsonUtility.ToJson(body);
            StartCoroutine(SendRequest("POST", endpoint, json, onSuccess, onError));
        }

        public void Post<TResponse>(string endpoint, string jsonBody, Action<TResponse> onSuccess, Action<string> onError = null)
        {
            StartCoroutine(SendRequest("POST", endpoint, jsonBody, onSuccess, onError));
        }

        // ============ CORE ============

        private IEnumerator SendRequest<TResponse>(string method, string endpoint, string jsonBody, Action<TResponse> onSuccess, Action<string> onError)
        {
            string url = BASE_URL + endpoint;
            UnityWebRequest request;

            if (method == "GET")
            {
                request = UnityWebRequest.Get(url);
            }
            else
            {
                byte[] bodyRaw = jsonBody != null ? Encoding.UTF8.GetBytes(jsonBody) : null;
                request = new UnityWebRequest(url, "POST");
                if (bodyRaw != null)
                {
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                }
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
            }

            // Attach auth token
            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
            }

            request.timeout = (int)TIMEOUT;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                try
                {
                    TResponse response = JsonUtility.FromJson<TResponse>(responseText);
                    onSuccess?.Invoke(response);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ApiClient] Parse error: {e.Message}\nResponse: {responseText}");
                    onError?.Invoke("Parse error: " + e.Message);
                }
            }
            else
            {
                string errorMsg = request.error;
                // Try to parse server error message
                if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    try
                    {
                        ApiResponse errorResponse = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);
                        if (!string.IsNullOrEmpty(errorResponse.message))
                        {
                            errorMsg = errorResponse.message;
                        }
                    }
                    catch { }
                }
                Debug.LogWarning($"[ApiClient] {method} {endpoint} failed: {errorMsg}");
                onError?.Invoke(errorMsg);
            }

            request.Dispose();
        }
    }
}
