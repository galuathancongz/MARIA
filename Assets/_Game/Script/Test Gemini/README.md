# H??ng d?n s? d?ng Gemini API trong Unity

## ?? T?ng quan
System này cho phép b?n tích h?p Google Gemini AI vào Unity game ?? t?o ra các tính n?ng giáo d?c thông minh.

## ?? C?u trúc files
- `APIManager.cs` - Core class qu?n lý API calls
- `GeminiChatUI.cs` - UI component cho chat interface
- `GeminiQuestionStepAction.cs` - Tích h?p v?i Storyboard system
- `GeminiHelper.cs` - Helper functions cho vi?c s? d?ng d? dàng
- `GeminiExample.cs` - Ví d? cách s? d?ng

## ?? Setup

### 1. Cài ??t API Key
1. Truy c?p [Google AI Studio](https://makersuite.google.com/app/apikey)
2. T?o API key cho Gemini
3. Copy API key vào field `apiKey` trong component APIManager

### 2. Setup Scene
1. T?o GameObject có APIManager component
2. Thi?t l?p API Key trong Inspector
3. Có th? enable debug logs ?? theo dõi

### 3. T?o UI Chat (Optional)
1. T?o Canvas v?i:
   - InputField cho câu h?i
   - Button ?? g?i
   - TextMeshPro ?? hi?n th? câu tr? l?i
   - Loading panel (optional)
2. Add component GeminiChatUI
3. Assign các references trong Inspector

## ?? Cách s? d?ng

### Basic Usage
```csharp
// Cách ??n gi?n nh?t
GeminiHelper.AskQuestion("Xin chào Gemini!");

// V?i callback
GeminiHelper.AskQuestion(
    "Gi?i thích phân s? là gì?",
    response => Debug.Log($"Câu tr? l?i: {response}"),
    error => Debug.LogError($"L?i: {error}")
);
```

### Educational Features
```csharp
// T?o câu h?i giáo d?c
GeminiHelper.AskEducationalQuestion("Toán h?c", "c? b?n", response => {
    // Hi?n th? câu h?i lên UI
});

// Gi?i thích khái ni?m
GeminiHelper.ExplainConcept("phân s?", "ti?u h?c", response => {
    // Hi?n th? gi?i thích
});

// L?y m?o h?c t?p
GeminiHelper.GetStudyTips("Ti?ng Anh", response => {
    // Hi?n th? tips
});

// Ki?m tra ?áp án h?c sinh
GeminiHelper.CheckAnswer("2 + 2 = ?", "4", response => {
    // Hi?n th? feedback
});
```

### Advanced Usage v?i APIManager
```csharp
var apiManager = FindObjectOfType<APIManager>();

apiManager.SendQuestionToGemini(
    "T?o m?t bài t?p toán cho h?c sinh l?p 3",
    onSuccess: answer => {
        // X? lý thành công
        UIManager.Instance.ShowToast($"?? {answer}");
    },
    onError: error => {
        // X? lý l?i
        UIManager.Instance.ShowToast($"? {error}");
    }
);
```

### Tích h?p v?i Storyboard System
```csharp
// Trong Storyboard, add GeminiQuestionStepAction
var geminiStep = stepGameObject.AddComponent<GeminiQuestionStepAction>();
geminiStep.SetQuestion("Hãy h?i h?c sinh m?t câu h?i toán");
```

### Events System
```csharp
private void Start()
{
    APIManager.OnRequestStarted += () => {
        // Hi?n th? loading
    };
    
    APIManager.OnResponseReceived += (response) => {
        // X? lý khi nh?n ???c ph?n h?i
    };
    
    APIManager.OnErrorOccurred += (error) => {
        // X? lý l?i
    };
}
```

## ?? Tích h?p v?i Game Systems

### V?i UIManager
```csharp
// Hi?n th? câu tr? l?i b?ng Toast
GeminiHelper.AskQuestion("Câu h?i", response => {
    Luzart.UIManager.Instance.ShowToast(response);
});

// Tích h?p vào UI screens
var chatUI = UIManager.Instance.GetUI<GeminiChatUI>(UIName.ChatScreen);
chatUI.SendQuestion("Tôi c?n giúp ??");
```

### V?i Storyboard
- Add `GeminiQuestionStepAction` vào Steps list
- Set câu h?i trong Inspector ho?c code
- Configure timeout và behavior

## ?? Configuration

### APIManager Settings
- `apiKey`: API key t? Google AI Studio
- `modelName`: Model name (m?c ??nh: gemini-1.5-flash)
- `enableDebugLogs`: B?t/t?t debug logs

### GeminiQuestionStepAction Settings
- `questionToAsk`: Câu h?i ?? g?i
- `waitForResponse`: Có ??i ph?n h?i không
- `timeoutSeconds`: Th?i gian timeout

## ?? Troubleshooting

### Common Issues
1. **API Key không ho?t ??ng**
   - Ki?m tra API key có ?úng không
   - ??m b?o API key có permissions

2. **Request timeout**
   - T?ng timeout value
   - Ki?m tra internet connection

3. **JSON parsing error**
   - Response t? API có th? b? malformed
   - Check logs ?? debug

### Debug Tips
- B?t `enableDebugLogs` ?? xem request/response
- S? d?ng `[ContextMenu("Test API")]` trong APIManager
- Check Unity Console cho error messages

## ?? Examples

### T?o Quiz Game
```csharp
public class QuizManager : MonoBehaviour
{
    public void GenerateQuestion(string subject)
    {
        GeminiHelper.AskEducationalQuestion(subject, "trung bình", question => {
            // Parse question và hi?n th? UI
            DisplayQuestion(question);
        });
    }
    
    public void CheckStudentAnswer(string question, string answer)
    {
        GeminiHelper.CheckAnswer(question, answer, feedback => {
            // Hi?n th? feedback cho h?c sinh
            ShowFeedback(feedback);
        });
    }
}
```

### Smart Tutor
```csharp
public class SmartTutor : MonoBehaviour
{
    public void ExplainToStudent(string concept)
    {
        GeminiHelper.ExplainConcept(concept, "phù h?p v?i ?? tu?i", explanation => {
            // Hi?n th? gi?i thích b?ng speech bubble ho?c UI
            ShowExplanation(explanation);
        });
    }
}
```

## ?? Security Notes
- Không commit API key vào Git
- S? d?ng environment variables ho?c external config
- Validate user input tr??c khi g?i API

## ?? Support
- Check Unity Console cho error logs
- Verify API key permissions
- Test v?i simple questions tr??c