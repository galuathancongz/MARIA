using System;
using UnityEngine;

public class GeminiQuestionStepAction : StepAction
{
    [Header("Gemini Question Settings")]
    [SerializeField] private string questionToAsk;
    [SerializeField] private bool waitForResponse = true;
    [SerializeField] private float timeoutSeconds = 30f;
    
    private APIManager apiManager;
    private bool isWaitingForResponse = false;
    private float timeoutTimer = 0f;
    
    private void Awake()
    {
        apiManager = FindObjectOfType<APIManager>();
    }
    
    public override void Execute(Action<ActionResult> _onComplete)
    {
        base.Execute(_onComplete);
        
        if (apiManager == null)
        {
            Debug.LogError("[GeminiQuestionStepAction] Không tìm th?y APIManager!");
            onComplete?.Invoke(new ActionResult(actionResultType));
            return;
        }
        
        if (string.IsNullOrEmpty(questionToAsk))
        {
            Debug.LogError("[GeminiQuestionStepAction] Câu h?i không ???c ?? tr?ng!");
            onComplete?.Invoke(new ActionResult(actionResultType));
            return;
        }
        
        Debug.Log($"[GeminiQuestionStepAction] ?ang g?i câu h?i: {questionToAsk}");
        
        if (waitForResponse)
        {
            isWaitingForResponse = true;
            timeoutTimer = 0f;
            
            // Subscribe to events
            APIManager.OnResponseReceived += OnResponseReceived;
            APIManager.OnErrorOccurred += OnErrorOccurred;
        }
        
        // G?i câu h?i
        apiManager.SendQuestionToGemini(questionToAsk, OnGeminiSuccess, OnGeminiError);
        
        if (!waitForResponse)
        {
            // Không ??i response, hoàn thành ngay l?p t?c
            onComplete?.Invoke(new ActionResult(actionResultType));
        }
    }
    
    private void Update()
    {
        if (isWaitingForResponse)
        {
            timeoutTimer += Time.deltaTime;
            
            if (timeoutTimer >= timeoutSeconds)
            {
                Debug.LogWarning($"[GeminiQuestionStepAction] Timeout sau {timeoutSeconds} giây");
                OnTimeout();
            }
        }
    }
    
    private void OnGeminiSuccess(string response)
    {
        Debug.Log($"[GeminiQuestionStepAction] Nh?n ???c ph?n h?i: {response}");
        
        // Có th? l?u response vào GameData ho?c x? lý theo nhu c?u
        // PlayerPrefs.SetString("LastGeminiResponse", response);
    }
    
    private void OnGeminiError(string error)
    {
        Debug.LogError($"[GeminiQuestionStepAction] L?i: {error}");
    }
    
    private void OnResponseReceived(string response)
    {
        if (isWaitingForResponse)
        {
            FinishStep();
        }
    }
    
    private void OnErrorOccurred(string error)
    {
        if (isWaitingForResponse)
        {
            FinishStep();
        }
    }
    
    private void OnTimeout()
    {
        Debug.LogWarning("[GeminiQuestionStepAction] Quá th?i gian ch? ph?n h?i");
        FinishStep();
    }
    
    private void FinishStep()
    {
        isWaitingForResponse = false;
        
        // Unsubscribe from events
        APIManager.OnResponseReceived -= OnResponseReceived;
        APIManager.OnErrorOccurred -= OnErrorOccurred;
        
        if (isSetActiveAfter)
        {
            gameObject.SetActive(false);
        }
        
        onComplete?.Invoke(new ActionResult(actionResultType));
    }
    
    private void OnDisable()
    {
        // Cleanup khi disable
        if (isWaitingForResponse)
        {
            APIManager.OnResponseReceived -= OnResponseReceived;
            APIManager.OnErrorOccurred -= OnErrorOccurred;
            isWaitingForResponse = false;
        }
    }
    
    // Method ?? set câu h?i t? code
    public void SetQuestion(string question)
    {
        questionToAsk = question;
    }
    
    // Method ?? set timeout
    public void SetTimeout(float timeout)
    {
        timeoutSeconds = timeout;
    }
}