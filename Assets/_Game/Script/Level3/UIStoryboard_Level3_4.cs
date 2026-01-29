using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Luzart
{
    public class UIStoryboard_Level3_4 : Storyboard
    {
        public TMP_Text txtName;
        public TMP_Text txtAge;
        public TMP_Text txtStyle;
        public TMP_Text txtFeedback;
        public TMP_Text txtSummaryLiked;
        public TMP_Text txtSummaryStruggled;
        public TMP_Text txtSummarySuggestedChange;

        private StudentFeedbackResponseDTO responseDTO;
        private Level3Data Data => Level3Manager.Instance.Data;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            var str = GetShortStudentFeedbackPrompt(Data.GetStringFullContent());
            Level3Manager.Instance.Send(1, str, OnResult);
            UIManager.Instance.ShowLoading();
        }
        private void OnResult(string str)
        {
            UIManager.Instance.HideLoading();
            try
            {
                responseDTO = JsonConvert.DeserializeObject<StudentFeedbackResponseDTO>(str);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing response: {ex.Message}");
                txtFeedback.text = "Lỗi phân tích phản hồi.";
            }
            Data.responseStudent = responseDTO;
            SetUpUI();
        }
        private void SetUpUI()
        {
            if (responseDTO == null) return;
            txtName.text = responseDTO.student_info.name;
            txtAge.text = responseDTO.student_info.age.ToString();
            txtStyle.text = responseDTO.student_info.style;
            txtFeedback.text = responseDTO.feedback;
            txtSummaryLiked.text = responseDTO.summary.liked;
            txtSummaryStruggled.text = responseDTO.summary.struggled;
            txtSummarySuggestedChange.text = responseDTO.summary.suggested_change;
        }
        private string GetShortStudentFeedbackPrompt(string fullContent)
        {
            return "Context: You are a high school student. Create a fake identity for yourself: a single-word name (e.g., Leo), age (15), and a random VARK learning style.\n" +
                   "Task: Provide ultra-concise feedback on the following lesson plan:\n" + fullContent + "\n\n" +
                   "Requirements:\n" +
                   "1. Return ONLY a single JSON object. No explanations, no markdown, no backticks.\n" +
                   "2. The name MUST be exactly one word.\n" +
                   "3. Each content field must NOT exceed 15 words.\n\n" +
                   "Output JSON Format:\n" +
                   "{\n" +
                   "  \"student_info\": { \"name\": \"OneWordName\", \"age\": 15, \"style\": \"Random_VARK\" },\n" +
                   "  \"feedback\": \"Short feedback sentence\",\n" +
                   "  \"summary\": {\n" +
                   "    \"liked\": \"What you liked (short)\",\n" +
                   "    \"struggled\": \"What you found difficult (short)\",\n" +
                   "    \"suggested_change\": \"Short suggestion\"\n" +
                   "  },\n" +
                   "  \"emotion\": \"excited/confused/curious\"\n" +
                   "}";
        }
    }
    public class StudentInfoDTO
    {
        public string name;  // Tên 1 từ (ví dụ: Linh, Bảo)
        public int age;     // Thường là 15
        public string style; // VARK (ví dụ: Visual, Auditory...)
    }

    [Serializable]
    public class FeedbackSummaryDTO
    {
        public string liked;            // Điểm thích
        public string struggled;        // Điểm khó khăn
        public string suggested_change; // Đề xuất sửa đổi
    }

    [Serializable]
    public class StudentFeedbackResponseDTO
    {
        public StudentInfoDTO student_info = new StudentInfoDTO();
        public string feedback;          // Câu thoại trực tiếp
        public FeedbackSummaryDTO summary = new FeedbackSummaryDTO();
        public string emotion;           // hào hứng, bối rối, tò mò
    }
}
