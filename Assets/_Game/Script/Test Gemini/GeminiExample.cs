using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script demo cách sử dụng Gemini API
/// </summary>
public class GeminiExample : MonoBehaviour
{
    
    private void Start()
    {
        TestBasicQuestion();
    }
    
    private void TestBasicQuestion()
    {
        Debug.Log("[GeminiExample] Testing basic question...");
        
        GeminiHelper.AskQuestion(
            "Xin chào! Bạn có thể giúp tôi học toán không?",
            response => Debug.Log($"Phản hồi: {response}"),
            error => Debug.LogError($"Lỗi: {error}")
        );
    }
    
    private void TestEducationalQuestion()
    {
        Debug.Log("[GeminiExample] Testing educational question...");
        
        GeminiHelper.AskEducationalQuestion(
            "Toán học",
            "cơ bản",
            response => {
                Debug.Log($"✅ Câu hỏi giáo dục: {response}");
                // Có thể hiển thị lên UI ở đây
            }
        );
    }
    
    private void TestExplanation()
    {
        Debug.Log("[GeminiExample] Testing concept explanation...");
        
        GeminiHelper.ExplainConcept(
            "phân số",
            "tiểu học",
            response => {
                Debug.Log($"✅ Giải thích: {response}");
                // Có thể hiển thị lên UI ở đây
            }
        );
    }
    
    private void TestStudyTips()
    {
        Debug.Log("[GeminiExample] Testing study tips...");
        
        GeminiHelper.GetStudyTips(
            "Tiếng Anh",
            response => {
                Debug.Log($"✅ Mẹo học tập: {response}");
                // Có thể hiển thị lên UI ở đây
            }
        );
    }
    
    // Test với Storyboard system
    [ContextMenu("Test với Storyboard")]
    private void TestWithStoryboard()
    {
        var gameFlowManager = FindObjectOfType<GameFlowManager>();
        if (gameFlowManager != null)
        {
            Debug.Log("Đã tìm thấy GameFlowManager, có thể tích hợp GeminiQuestionStepAction vào storyboard");
        }
        
        // Example: Tạo một step action mới
        var stepAction = new GameObject("GeminiStep").AddComponent<GeminiQuestionStepAction>();
        stepAction.SetQuestion("Hãy tạo một câu hỏi toán đơn giản cho trẻ em.");
    }
    
    // Test tích hợp với UI Manager
    [ContextMenu("Test với UIManager")]
    private void TestWithUIManager()
    {
        var uiManager = Luzart.UIManager.Instance;
        if (uiManager != null)
        {
            Debug.Log("Có thể tích hợp Gemini với UIManager để hiển thị câu trả lời");
            
            // Example: Hiển thị toast với câu trả lời
            GeminiHelper.AskQuestion(
                "Cho tôi một câu động viên ngắn",
                response => uiManager.ShowToast($"🤖 {response}")
            );
        }
    }
}