using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Luzart
{
    public class UIStoryboard_Level3_7 : Storyboard
    {
        public TMP_Text txtPersona;
        public TMP_Text txtColor;
        public ProgressBarUI progressLevel2;
        public ProgressBarUI progressLevel3;
        public TMP_Text txtProgressLevel2;
        public TMP_Text txtProgressLevel3;
        public TMP_Text txtResponse;
        private DataLevel3_7 _data = new DataLevel3_7();
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            EPersonaType persona = PersonaManager.Instance.GetMyPersonaType();
            txtPersona.text = persona.ToString();
            txtColor.text = PersonaManager.Instance.GetNameColorPersonaType();
            Send();
            UIManager.Instance.ShowLoading();
        }
        public void OnBackToMain()
        {
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.MainMenu);
        }

        private void Send()
        {
            var msg = GetRequestLevel7();
            Level3Manager.Instance.Send(7, msg, OnResultString);

        }
        private void OnResultString(string str)
        {
            UIManager.Instance.HideLoading();
            try
            {
                DataLevel3_7 json = JsonUtility.FromJson<DataLevel3_7>(str);
                _data = json;
                OnDisplayData();
            }
            catch (Exception e)
            {
                txtResponse.text = $"Error parsing level 3-7 response: {e.Message}";
                Debug.LogError($"Error parsing level 3-7 response: {e.Message}");
            }
        }

        private string GetRequestLevel7()
        {
            var data = Level3Manager.Instance.Data;

            // Thu thập các thông tin quan trọng từ quá trình chơi để AI có cơ sở nhận xét
            string topic = data.topic;
            string subject = MentorSubjectExtension.GetSubjectName(data.subject);
            int revisionCount = data.GetAllSendAI();
            bool usedInclusion = data.optionalFilters.Contains("Inclusion") || data.optionalFilters.Contains("Accessibility");

            return "Context: You are AI Assistant MARIA and the Pedagogical Mentor. The player has just finished the entire Co-Design journey.\n" +
                   "Task: Generate a professional, encouraging, and personalised feedback summary for the teacher's final dashboard.\n\n" +
                   "Player's Achievement Data:\n" +
                   $"- Subject: {subject}\n" +
                   $"- Lesson Topic: {topic}\n" +
                   $"- Total AI Collaborations/Revisions: {revisionCount}\n" +
                   $"- Focus on Inclusion/Accessibility: {(usedInclusion ? "Yes" : "No")}\n" +
                   $"- Student Feedback handled: {data.studentWork}\n\n" +

                   "Output Requirement:\n" +
                   "1. Provide ONLY a single JSON object. No markdown, no introductory text.\n" +
                   "2. The 'personalisedFeedback' should be a cohesive paragraph (40-60 words).\n" +
                   "3. Tone: Celebratory, insightful, and professional. Highlight their specific strengths (e.g., iterative design, student-centeredness, or subject expertise).\n" +
                   "4. Calculate 'percentLevel2' and 'percentLevel3' based on the completion of the tasks (Values: 0-100).\n\n" +

                   "Output JSON Format:\n" +
                   "{\n" +
                   "  \"percentLevel2\": int,\n" +
                   "  \"percentLevel3\": int,\n" +
                   "  \"personalisedFeedback\": \"Your feedback text here...\"\n" +
                   "}";
        }

        public void OnDisplayData()
        {
            float percentLevel2 = (_data.percentLevel2 / 100f);
            float percentLevel3 = (_data.percentLevel3 / 100f);
            progressLevel2.SetSlider(percentLevel2, percentLevel2);
            progressLevel3.SetSlider(percentLevel3, percentLevel3);
            txtProgressLevel2.text = $"{_data.percentLevel2}%";
            txtProgressLevel3.text = $"{_data.percentLevel3}%";
            txtResponse.text = _data.personalisedFeedback;
        }
    }
    [Serializable]
    public class DataLevel3_7
    {
        public int percentLevel2;
        public int percentLevel3;
        public string personalisedFeedback;
    }
}
