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
    public string ModelName => modelName;
    
    public static event Action<string> OnResponseReceived;
    public static event Action<string> OnErrorOccurred;
    public static event Action OnRequestStarted;
    private void Start()
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
        {
            Debug.LogError("[APIManager] Vui lòng thiet lap API Key trong Inspector!");
        }
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        OnResponseReceived = null;
        OnErrorOccurred = null;
        OnRequestStarted = null;
    }
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
        
        string url = $"{GEMINI_API_URL}{modelName}:generateContent?key={apiKey}";
        
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
            
            yield return request.SendWebRequest();
            
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

    private string myPrePrompt = "Tôi là một trợ lý AI phục vụ cho việc hỗ trợ giáo viên tiếp cận AI. Hãy trả lời các câu hỏi một cách ngắn gọn và chính xác.";
    private string myPrePromptVietNam = "Bạn là AI Mentor trong một trò chơi giáo dục tên là GameAid.\r\n\r\nVai trò của bạn là hỗ trợ giáo viên:\r\n- brainstorm ý tưởng\r\n- đồng sáng tạo hoạt động dạy học\r\n- gợi mở sự sáng tạo trong lớp học\r\n\r\nBạn KHÔNG phải là người thay thế giáo viên.\r\nBạn KHÔNG đánh giá, chấm điểm hay phán xét đúng – sai.\r\n\r\nGiọng điệu của bạn phải:\r\n- thân thiện\r\n- tò mò\r\n- hợp tác\r\n- không phán xét\r\n\r\nBạn luôn khuyến khích giáo viên khám phá theo cách riêng của họ.\r\nGiáo viên luôn là người quyết định cuối cùng.\r\n";
    private string myPrePromptEnglish = "You are an AI Mentor inside an educational game called GameAid.\r\n\r\nYour role is to support teachers by brainstorming ideas, co-creating lesson activities, and sparking creativity.\r\nYou are NOT a teacher replacement, evaluator, or examiner.\r\nYou do NOT grade, judge, or assess correctness.\r\n\r\nYour tone must be:\r\n- Supportive\r\n- Curious\r\n- Collaborative\r\n- Non-judgmental\r\n\r\nYou respond as a subject-specific mentor with a clear personality.\r\nAlways encourage exploration and teacher agency.\r\n";
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
        sb.AppendLine(myPrePromptVietNam);
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
    public void SendQuestionRequireJsonToGemini<T>(string question, Action<T> onSuccess = null, Action<string> onError = null) where T : new()
    {
        // Mẹo Prompt để ép AI trả về đúng format
        string jsonPrompt = $"{question}\n\nIMPORTANT: Output ONLY raw JSON. No markdown, no '```json'. Must follow the structure of the data class.";

        SendQuestionToGemini(jsonPrompt, (rawResponse) => {
            T result = ParseRawResponse<T>(rawResponse);
            onSuccess?.Invoke(result);
        }, onError);
    }
    private T ParseRawResponse<T>(string input) where T : new()
    {
        try
        {
            int start = input.IndexOf('{');
            int end = input.LastIndexOf('}');

            if (start != -1 && end != -1 && end > start)
            {
                string cleanJson = input.Substring(start, end - start + 1);
                return JsonUtility.FromJson<T>(cleanJson);
            }
        }
        catch (Exception e) { Debug.LogError("Lỗi Parse: " + e.Message); }
        return new T();
    }
    [Sirenix.OdinInspector.Button]
    public void SendMessageToAI(string msg)
    {
        string str = MyPostStringNormal(msg);
        SendQuestionToGemini(str);
    }

}

//public interface IPromptHelper
//{
//    string PromptDefault { get; }
//    string GetPrompString(string question); 
//    string GetPromptStringScene2(string question);
//}
//public class IPromptHelperVietnamese : IPromptHelper
//{

//}
