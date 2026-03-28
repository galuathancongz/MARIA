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
        //public TMP_Text txtName;
        //public TMP_Text txtAge;
        //public TMP_Text txtStyle;
        //public TMP_Text txtFeedback;
        //public TMP_Text txtSummaryLiked;
        //public TMP_Text txtSummaryStruggled;
        //public TMP_Text txtSummarySuggestedChange;
        
        public TMP_InputField ifName;
        public TMP_InputField ifAge;
        public TMP_InputField ifStyle;
        public TMP_InputField ifFeedback;
        public TMP_InputField ifSummaryLiked;
        public TMP_InputField ifSummaryStruggled;
        public TMP_InputField ifSummarySuggestedChange;


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
                Debug.LogError($"Error parsing response: {ex}");
                //txtFeedback.text = "Lỗi phân tích phản hồi.";
                ifFeedback.text = LocalizationManager.Instance.Get("ui.error_parse_response");
            }
            Data.responseStudent = responseDTO;
            SetUpUI();
        }
        private void SetUpUI()
        {
            if (responseDTO == null) return;
            //txtName.text = responseDTO.student_info.name;
            //txtAge.text = responseDTO.student_info.age.ToString();
            //txtStyle.text = responseDTO.student_info.style;
            //txtFeedback.text = responseDTO.feedback;
            //txtSummaryLiked.text = responseDTO.summary.liked;
            //txtSummaryStruggled.text = responseDTO.summary.struggled;
            //txtSummarySuggestedChange.text = responseDTO.summary.suggested_change;

            ifName.text = responseDTO.student_info.name;
            ifAge.text = responseDTO.student_info.age.ToString();
            ifStyle.text = responseDTO.student_info.style;
            ifFeedback.text = responseDTO.feedback;
            ifSummaryLiked.text = responseDTO.summary.liked;
            ifSummaryStruggled.text = responseDTO.summary.struggled;
            ifSummarySuggestedChange.text = responseDTO.summary.suggested_change;

        }
        private string GetShortStudentFeedbackPrompt(string fullContent)
        {
            return LocalizationManager.Instance.GetPrompt("prompts.level3_4_feedback", new System.Collections.Generic.Dictionary<string, string> {
                {"fullContent", fullContent}
            });
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
