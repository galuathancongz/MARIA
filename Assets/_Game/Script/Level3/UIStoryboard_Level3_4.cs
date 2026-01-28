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
            return "Context: Bạn là học sinh trung học. Hãy tự fake 1 tên (chỉ 1 từ, ví dụ: Bảo), 1 tuổi (15), và 1 phong cách VARK ngẫu nhiên.\n" +
                   "Nhiệm vụ: Phản hồi siêu ngắn gọn về giáo án sau:\n" + fullContent + "\n\n" +
                   "Yêu cầu:\n" +
                   "1. Trả về JSON duy nhất, không giải thích.\n" +
                   "2. Tên chỉ được dùng 1 từ duy nhất.\n" +
                   "3. Các trường nội dung không quá 15 từ.\n\n" +
                   "Output JSON Format:\n" +
                   "{\n" +
                   "  \"student_info\": { \"name\": \"Tên_1_từ\", \"age\": 15, \"style\": \"VARK_ngẫu_nhiên\" },\n" +
                   "  \"feedback\": \"Câu_nói_ngắn\",\n" +
                   "  \"summary\": {\n" +
                   "    \"liked\": \"Điểm_thích_ngắn\",\n" +
                   "    \"struggled\": \"Điểm_khó_ngắn\",\n" +
                   "    \"suggested_change\": \"Đề_xuất_ngắn\"\n" +
                   "  },\n" +
                   "  \"emotion\": \"hào hứng/bối rối/tò mò\"\n" +
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
