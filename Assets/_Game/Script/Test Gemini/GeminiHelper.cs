using UnityEngine;

/// <summary>
/// Helper class ?? s? d?ng Gemini API d? dàng h?n
/// </summary>
public static class GeminiHelper
{
    private static APIManager _apiManager;
    
    /// <summary>
    /// L?y reference t?i APIManager
    /// </summary>
    private static APIManager ApiManager
    {
        get
        {
            if (_apiManager == null)
            {
                _apiManager = Object.FindObjectOfType<APIManager>();
            }
            return _apiManager;
        }
    }
    
    /// <summary>
    /// G?i câu h?i ??n gi?n t?i Gemini
    /// </summary>
    public static void AskQuestion(string question)
    {
        if (ApiManager != null)
        {
            ApiManager.SendQuestionToGemini(question);
        }
        else
        {
            Debug.LogError("[GeminiHelper] Không tìm th?y APIManager trong scene!");
        }
    }
    
    /// <summary>
    /// G?i câu h?i v?i callback
    /// </summary>
    public static void AskQuestion(string question, System.Action<string> onSuccess, System.Action<string> onError = null)
    {
        if (ApiManager != null)
        {
            ApiManager.SendQuestionToGemini(question, onSuccess, onError);
        }
        else
        {
            Debug.LogError("[GeminiHelper] Không tìm th?y APIManager trong scene!");
            onError?.Invoke("APIManager không kh? d?ng");
        }
    }
    
    /// <summary>
    /// Ki?m tra xem API có s?n sàng không
    /// </summary>
    public static bool IsAPIReady()
    {
        return ApiManager != null;
    }
    
    /// <summary>
    /// T?o câu h?i cho game giáo d?c
    /// </summary>
    public static void AskEducationalQuestion(string subject, string difficulty, System.Action<string> onSuccess)
    {
        string question = $"Hãy t?o m?t câu h?i {difficulty} v? ch? ?? {subject} phù h?p cho h?c sinh. Kèm theo ?áp án và gi?i thích.";
        AskQuestion(question, onSuccess);
    }
    
    /// <summary>
    /// Yêu c?u gi?i thích m?t khái ni?m
    /// </summary>
    public static void ExplainConcept(string concept, string level, System.Action<string> onSuccess)
    {
        string question = $"Hãy gi?i thích khái ni?m '{concept}' m?t cách ??n gi?n cho h?c sinh {level}.";
        AskQuestion(question, onSuccess);
    }
    
    /// <summary>
    /// T?o g?i ý h?c t?p
    /// </summary>
    public static void GetStudyTips(string subject, System.Action<string> onSuccess)
    {
        string question = $"Hãy ??a ra 5 m?o h?c t?p hi?u qu? cho môn {subject}.";
        AskQuestion(question, onSuccess);
    }
    
    /// <summary>
    /// Ki?m tra ?áp án c?a h?c sinh
    /// </summary>
    public static void CheckAnswer(string question, string studentAnswer, System.Action<string> onSuccess)
    {
        string prompt = $"Câu h?i: {question}\n?áp án c?a h?c sinh: {studentAnswer}\nHãy ?ánh giá ?áp án này và ??a ra ph?n h?i chi ti?t.";
        AskQuestion(prompt, onSuccess);
    }
}