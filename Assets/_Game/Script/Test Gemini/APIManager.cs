using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Luzart
{
    /// <summary>
    /// APIManager – MARIA
    /// - SingletonSaveLoad based
    /// - Uses OpenAI Responses API
    /// - Maintains AI memory via previous_response_id (resp_xxx)
    /// - Public API: Send(prompt, callback)
    /// </summary>
    public class APIManager : SingletonSaveLoad<APIManagerData, APIManager>
    {
        #region === Config ===

        [Header("OpenAI")]
        [SerializeField]
        private List<string> listAPIKey;
        private string _apiKey = null;
        [SerializeField] private string apiKey
        {
            get
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    StringBuilder strBuilder = new StringBuilder();
                    foreach(string key in listAPIKey)
                    {
                        strBuilder.Append(key);
                    }
                    _apiKey = strBuilder.ToString();
                }
                return _apiKey;
            }
        }
        [SerializeField] private string model = "gpt-5";

        private const string ENDPOINT = "https://api.openai.com/v1/responses";

        protected override string KEYLOAD => "MARIA_API_SESSION";
        [SerializeField][ReadOnly] List<string> listConversation = new List<string>();

        #endregion

        #region === Public API (ONLY ONE) ===
        [Sirenix.OdinInspector.Button]
        void SendEditor(string prompt)
        {
            Send(prompt, (response) =>
            {
                Debug.Log("[APIManager] Response:\n" + response);
            });
        }

        /// <summary>
        /// Send user prompt to OpenAI
        /// Callback returns ONLY assistant text
        /// </summary>
        public void Send(string prompt, Action<string> onResult)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                onResult?.Invoke(string.Empty);
                return;
            }

            StartCoroutine(SendCoroutine(prompt, onResult));
            listConversation.Add($"Me : {prompt}");
        }

        #endregion

        #region === Coroutine Core ===

        private IEnumerator SendCoroutine(string prompt, Action<string> onResult)
        {
            string json = BuildRequestJson(prompt);
            byte[] body = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(ENDPOINT, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[APIManager] OpenAI Error\n" + request.error);
                    Debug.LogError(request.downloadHandler.text);
                    onResult?.Invoke(null);
                    yield break;
                }

                HandleResponse(request.downloadHandler.text, onResult);
            }
        }

        #endregion

        #region === Build Request JSON ===

        private string BuildRequestJson(string prompt)
        {
            // Prepend language instruction so AI responds in the user's current language
            string finalPrompt = PrependLanguageInstruction(prompt);

            if (string.IsNullOrEmpty(Data.currentResponseId))
            {
                var req = new ResponsesRequestFirst
                {
                    model = model,
                    input = finalPrompt,
                    store = true
                };
                return JsonUtility.ToJson(req);
            }
            else
            {
                var req = new ResponsesRequestContinue
                {
                    model = model,
                    input = finalPrompt,
                    store = true,
                    previous_response_id = Data.currentResponseId
                };
                return JsonUtility.ToJson(req);
            }
        }

        /// <summary>
        /// Prepend a strong language directive so OpenAI responds in the user's
        /// selected language instead of defaulting to English.
        /// Reads the current language from LocalizationManager.
        /// </summary>
        private static string PrependLanguageInstruction(string prompt)
        {
            string lang = "en";
            if (LocalizationManager.Instance != null)
                lang = LocalizationManager.Instance.CurrentLanguage;

            string directive;
            switch (lang)
            {
                case "vi":
                    directive =
                        "SYSTEM DIRECTIVE (MANDATORY): You MUST respond ENTIRELY in Vietnamese (Tiếng Việt). " +
                        "Every word, sentence, JSON value, and any content you generate must be written in Vietnamese. " +
                        "Do NOT use English under any circumstance, even if the user's message below is in English. " +
                        "Technical keywords (JSON keys, code, variable names) may stay in English, but all natural-language text MUST be in Vietnamese.\n" +
                        "----------\n";
                    break;
                case "en":
                default:
                    directive =
                        "SYSTEM DIRECTIVE: Respond in English.\n" +
                        "----------\n";
                    break;
            }

            return directive + prompt;
        }

        [Serializable]
        private class ResponsesRequestFirst
        {
            public string model;
            public string input;
            public bool store;
        }

        [Serializable]
        private class ResponsesRequestContinue
        {
            public string model;
            public string input;
            public bool store;
            public string previous_response_id;
        }

        #endregion

        #region === Parse Response ===

        private void HandleResponse(string json, Action<string> onResult)
        {
            ResponsesResponse data;

            try
            {
                data = JsonUtility.FromJson<ResponsesResponse>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("[APIManager] JSON Parse Error: " + e.Message);
                onResult?.Invoke(null);
                return;
            }

            if (data == null)
            {
                onResult?.Invoke(null);
                return;
            }

            // Persist response id for next turn
            Data.currentResponseId = data.id;
            Save();

            string output = ExtractOutputText(data);
            onResult?.Invoke(output);
            listConversation.Add($"AI : {output}");
        }

        private string ExtractOutputText(ResponsesResponse data)
        {
            if (data.output == null) return null;

            foreach (var item in data.output)
            {
                if (item.type != "message" || item.content == null)
                    continue;

                foreach (var c in item.content)
                {
                    if (c.type == "output_text")
                        return c.text;
                }
            }

            return null;
        }

        #endregion

        #region === Internal Control ===

        /// <summary>
        /// Reset AI memory chain (new lesson / new persona)
        /// </summary>
        internal void ResetSession()
        {
            Data.currentResponseId = null;
            Save();
        }

        #endregion

        #region === DTO ===

        [Serializable]
        private class ResponsesResponse
        {
            public string id;
            public OutputItem[] output;
        }

        [Serializable]
        private class OutputItem
        {
            public string type;
            public string role;
            public ContentItem[] content;
        }

        [Serializable]
        private class ContentItem
        {
            public string type;
            public string text;
        }

        #endregion
    }

    // ===== Persisted Data =====
    [Serializable]
    public class APIManagerData
    {
        public string currentResponseId;
    }
}
