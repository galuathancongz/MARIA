using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GeminiChatUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField questionInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private TextMeshProUGUI answerText;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private ScrollRect scrollRect;
    
    [Header("Settings")]
    [SerializeField] private string placeholderQuestion = "Hãy h?i tôi b?t c? ?i?u gì...";
    [SerializeField] private string loadingMessage = "?ang x? lý câu h?i c?a b?n...";
    
    private APIManager apiManager;
    
    private void Start()
    {
        InitializeUI();
        SetupAPIManager();
        SubscribeToEvents();
    }
    
    private void InitializeUI()
    {
        // Setup placeholder
        if (questionInputField != null)
        {
            questionInputField.placeholder.GetComponent<TextMeshProUGUI>().text = placeholderQuestion;
        }
        
        // Setup button
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }
        
        // Setup initial state
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        
        if (answerText != null)
        {
            answerText.text = "Chào m?ng b?n! Hãy h?i tôi b?t c? ?i?u gì.";
        }
    }
    
    private void SetupAPIManager()
    {
        apiManager = FindObjectOfType<APIManager>();
        if (apiManager == null)
        {
            Debug.LogError("[GeminiChatUI] Không tìm th?y APIManager trong scene!");
        }
    }
    
    private void SubscribeToEvents()
    {
        APIManager.OnRequestStarted += OnRequestStarted;
        APIManager.OnResponseReceived += OnResponseReceived;
        APIManager.OnErrorOccurred += OnErrorOccurred;
    }
    
    private void OnSendButtonClicked()
    {
        string question = questionInputField?.text?.Trim();
        
        if (string.IsNullOrEmpty(question))
        {
            ShowError("Vui lòng nh?p câu h?i!");
            return;
        }
        
        if (apiManager != null)
        {
            apiManager.SendQuestionToGemini(question);
        }
        else
        {
            ShowError("APIManager không kh? d?ng!");
        }
    }
    
    private void OnRequestStarted()
    {
        SetUIState(false);
        ShowLoading(true);
        
        if (answerText != null)
        {
            answerText.text = loadingMessage;
        }
    }
    
    private void OnResponseReceived(string answer)
    {
        SetUIState(true);
        ShowLoading(false);
        
        if (answerText != null)
        {
            answerText.text = $"<color=#00ff00>?? Gemini:</color>\n{answer}";
        }
        
        // Clear input field
        if (questionInputField != null)
        {
            questionInputField.text = "";
        }
        
        // Scroll to bottom
        ScrollToBottom();
    }
    
    private void OnErrorOccurred(string error)
    {
        SetUIState(true);
        ShowLoading(false);
        
        ShowError($"? L?i: {error}");
    }
    
    private void ShowError(string errorMessage)
    {
        if (answerText != null)
        {
            answerText.text = $"<color=#ff0000>{errorMessage}</color>";
        }
        
        Debug.LogError($"[GeminiChatUI] {errorMessage}");
    }
    
    private void SetUIState(bool enabled)
    {
        if (sendButton != null)
        {
            sendButton.interactable = enabled;
        }
        
        if (questionInputField != null)
        {
            questionInputField.interactable = enabled;
        }
    }
    
    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
        }
    }
    
    private void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        APIManager.OnRequestStarted -= OnRequestStarted;
        APIManager.OnResponseReceived -= OnResponseReceived;
        APIManager.OnErrorOccurred -= OnErrorOccurred;
        
        // Cleanup button listener
        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(OnSendButtonClicked);
        }
    }
    
    // Public methods for external use
    public void SendQuestion(string question)
    {
        if (questionInputField != null)
        {
            questionInputField.text = question;
        }
        OnSendButtonClicked();
    }
    
    public void ClearChat()
    {
        if (answerText != null)
        {
            answerText.text = "Chat ?ã ???c xóa. Hãy h?i tôi b?t c? ?i?u gì.";
        }
        
        if (questionInputField != null)
        {
            questionInputField.text = "";
        }
    }
}