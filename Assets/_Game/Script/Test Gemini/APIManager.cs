using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class GeminiRequest
{
    public GeminiContent[] contents;
}

[System.Serializable]
public class GeminiContent
{
    public GeminiPart[] parts;
}

[System.Serializable]
public class GeminiPart
{
    public string text;
}

[System.Serializable]
public class GeminiResponse
{
    public GeminiCandidate[] candidates;
}

[System.Serializable]
public class GeminiCandidate
{
    public GeminiContent content;
    public string finishReason;
    public int index;
}

public class APIManager : Singleton<APIManager>
{
    [Header("Gemini API Configuration")]
    [SerializeField] private string apiKey = "YOUR_GEMINI_API_KEY_HERE";
    [SerializeField] private string modelName = "gemini-1.5-flash";
    
    [SerializeField] private string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/";
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // Events ?? UI có th? subscribe
    public static event Action<string> OnResponseReceived;
    public static event Action<string> OnErrorOccurred;
    public static event Action OnRequestStarted;
    
    public string ModelName => modelName;
    private void Start()
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
        {
            Debug.LogError("[APIManager] Vui lòng thiet lap API Key trong Inspector!");
        }
    }
    
    /// <summary>
    /// G?i câu h?i t?i Gemini API
    /// </summary>
    /// <param name="question">Câu h?i c?a ng??i dùng</param>
    /// <param name="onSuccess">Callback khi thành công</param>
    /// <param name="onError">Callback khi có l?i</param>
    public void SendQuestionToGemini(string question, Action<string> onSuccess = null, Action<string> onError = null)
    {
        if (string.IsNullOrEmpty(question))
        {
            string errorMsg = "Cau hoi khong duoc trong!";
            LogError(errorMsg);
            onError?.Invoke(errorMsg);
            OnErrorOccurred?.Invoke(errorMsg);
            return;
        }
        
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
        {
            string errorMsg = "API Key chua duoc thiet lap!";
            LogError(errorMsg);
            onError?.Invoke(errorMsg);
            OnErrorOccurred?.Invoke(errorMsg);
            return;
        }
        question = $"{question}. Nói ngắn gọn chỉ ít thôi.";
        StartCoroutine(SendRequestCoroutine(question, onSuccess, onError));
    }
    
    private IEnumerator SendRequestCoroutine(string question, Action<string> onSuccess, Action<string> onError)
    {
        OnRequestStarted?.Invoke();
        Log($"Dang gui cau hoi: {question}");
        
        // T?o URL request
        string url = $"{GEMINI_API_URL}{modelName}:generateContent?key={apiKey}";
        
        // T?o request body
        GeminiRequest requestData = new GeminiRequest
        {
            contents = new GeminiContent[]
            {
                new GeminiContent
                {
                    parts = new GeminiPart[]
                    {
                        new GeminiPart { text = question }
                    }
                }
            }
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        Log($"Request JSON: {jsonData}");
        
        // T?o UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            // G?i request
            yield return request.SendWebRequest();
            
            // X? lý k?t qu?
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Log($"Response nhan thanh cong: {responseText}");
                
                try
                {
                    GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(responseText);
                    
                    if (response?.candidates != null && response.candidates.Length > 0)
                    {
                        string answer = response.candidates[0].content.parts[0].text;
                        Log($"Câu tra loi: {answer}");
                        
                        onSuccess?.Invoke(answer);
                        OnResponseReceived?.Invoke(answer);
                    }
                    else
                    {
                        string errorMsg = "Không nhin thấy Gemini";
                        LogError(errorMsg);
                        onError?.Invoke(errorMsg);
                        OnErrorOccurred?.Invoke(errorMsg);
                    }
                }
                catch (Exception e)
                {
                    string errorMsg = $"Loi khi parse response: {e.Message}";
                    LogError(errorMsg);
                    onError?.Invoke(errorMsg);
                    OnErrorOccurred?.Invoke(errorMsg);
                }
            }
            else
            {
                string errorMsg = $"Loi API: {request.error}\nResponse Code: {request.responseCode}\nResponse: {request.downloadHandler.text}";
                LogError(errorMsg);
                onError?.Invoke(errorMsg);
                OnErrorOccurred?.Invoke(errorMsg);
            }
        }
    }
    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[APIManager] {message}");
    }
    
    private void LogError(string message)
    {
        if (enableDebugLogs)
            Debug.LogError($"[APIManager] {message}");
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        OnResponseReceived = null;
        OnErrorOccurred = null;
        OnRequestStarted = null;
    }
    private string myPrePrompt = "Tôi là một trợ lý AI phục vụ cho việc hỗ trợ giáo viên tiếp cận AI. Hãy trả lời các câu hỏi một cách ngắn gọn và chính xác.";
    private string MyProfilePersona
    {
        get
        {
            if(PersonaManager.Instance == null)
            {
                return string.Empty;
            }
            EPersonaType ePersonaType = PersonaManager.Instance.GetMyPersonaType();
            return "Tôi có tính cách " + ePersonaType.ToString() + ".";
        }
    }
    private string MyPostStringNormal(string str = null)
    {
        StringBuilder sb  = new StringBuilder();
        sb.AppendLine(myPrePrompt);
        if (!string.IsNullOrEmpty(MyProfilePersona))
        {
            sb.AppendLine(MyProfilePersona);
        }
        sb.AppendLine("Đây là câu hỏi : ");
        if (!string.IsNullOrEmpty(str))
        {
            sb.AppendLine(str);
        }
        return sb.ToString();
    }
    [Sirenix.OdinInspector.Button]
    public void SendMessageToAI(string msg)
    {
        string str = MyPostStringNormal(msg);
        SendQuestionToGemini(str);
    }

}
