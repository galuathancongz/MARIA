using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luzart
{
    public class UIStoryboard_Level3_5 : Storyboard
    {
        public Button btnNextStep;
        public Level2_ConversationItem conversationItemMain;
        public TMP_InputField inputFieldStrength;
        public TMP_InputField inputFieldImprovement;
        public TMP_InputField inputFieldNextStep;

        public Transform content;
        public List<Item_ClickToImport> listItemClickToImport = new List<Item_ClickToImport>();
        public Item_ClickToImport itemPrefab;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            string promptStudentWork = GetStudentWorkPrompt();
            Level3Manager.Instance.Send(2, promptStudentWork, OnGetStudentWork);
            UIManager.Instance.ShowLoading();
            btnNextStep.gameObject.SetActive(false);
        }
        private void OnGetStudentWork(string str)
        {
            UIManager.Instance.HideLoading();
            try
            {
                var studentWorkDTO = JsonUtility.FromJson<StudentWorkDTO>(str);
                string studentWork = studentWorkDTO.studentWork;
                Data.studentWork= studentWork;
                conversationItemMain.ShowText($"Student response:\n\"{studentWork}\"");
                GenerateFeedback();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing student work response: {ex.Message}");
                conversationItemMain.ShowText("Error parsing student response.");
            }


        }

        private void OnGetFeedbackSuggestions(string str)
        {

            try
            {
                var feedbackDTO = JsonUtility.FromJson<FormativeFeedbackSuggestionsDTO>(str);
                List<FeedbackSuggestion> suggestions = new List<FeedbackSuggestion>();
                suggestions.AddRange(feedbackDTO.strengths.Select(s => new FeedbackSuggestion(s, EFeedback.Strength)));
                suggestions.AddRange(feedbackDTO.improvements.Select(s => new FeedbackSuggestion(s, EFeedback.Improvement)));
                suggestions.AddRange(feedbackDTO.nextSteps.Select(s => new FeedbackSuggestion(s, EFeedback.NextStep)));

                MasterHelper.InitListObj(suggestions.Count,itemPrefab, listItemClickToImport, content, (item, index) =>
                {
                    var data = suggestions[index];
                    item.gameObject.SetActive(true);
                    item.Initialize(data.type,data.text,OnClick);
                });


            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing feedback suggestions response: {ex.Message}");
                UIManager.Instance.ShowToast("Error parsing feedback suggestions.");
            }
        }
        public void OnClickGenerateFeedback()
        {
            if(Data.GetConverstationState(2) == EState.WaitAI)
            {
                UIManager.Instance.ShowToast("Generating feedback, please wait.");
                return;
            }
            GenerateFeedback();
        }
        void GenerateFeedback()
        {
            string objective = Level3Manager.Instance.Data.learningObjective;
            string promptFeedback = GetFeedbackSuggestionsPrompt(Data.studentWork, objective);
            MasterHelper.InitListObj(1, itemPrefab, listItemClickToImport, content, (item, index) =>
            {
                item.gameObject.SetActive(true);
                item.Initialize(EFeedback.Strength, "Generating feedback suggestion...", null);
            });
            Level3Manager.Instance.Send(2, promptFeedback, OnGetFeedbackSuggestions);
        }
        private void OnClick(string str, EFeedback type)
        {
            if(type == EFeedback.Strength)
            {
                inputFieldStrength.text = str;
            }
            else if(type == EFeedback.Improvement)
            {
                inputFieldImprovement.text = str;
            }
            else if(type == EFeedback.NextStep)
            {
                inputFieldNextStep.text = str;
            }
        }
        List<FeedbackSuggestion> listFeedbackSuggestions = new List<FeedbackSuggestion>();
        public void OnClickSendStrength(string str)
        {
            if(string.IsNullOrEmpty(str))
                return;
            var data = new FeedbackSuggestion(str, EFeedback.Strength);
            Data.AddFeedbackSuggestion(data);
            if(!listFeedbackSuggestions.Contains(data))
                listFeedbackSuggestions.Add(data);
            CheckShowNextStep();
        }
        public void OnClickSendImprovement(string str)
        {
            if(string.IsNullOrEmpty(str)) return;
            var data = new FeedbackSuggestion(str, EFeedback.Improvement);
            Data.AddFeedbackSuggestion(data);
            if (!listFeedbackSuggestions.Contains(data))
                listFeedbackSuggestions.Add(data);
            CheckShowNextStep();
        }
        public void OnClickSendNextStep(string str)
        {
            if (string.IsNullOrEmpty(str)) return;
            var data = new FeedbackSuggestion(str, EFeedback.NextStep);
            Data.AddFeedbackSuggestion(data);
            if (!listFeedbackSuggestions.Contains(data))
                listFeedbackSuggestions.Add(data);
            CheckShowNextStep();
        }
        private void CheckShowNextStep()
        {
            if(listFeedbackSuggestions.Count >=3)
            {
                btnNextStep.gameObject.SetActive(true);
            }
            else
            {
                btnNextStep.gameObject.SetActive(false);
            }
        }
        // Hàm 1: Tạo bài làm của học sinh
        private Level3Data Data => Level3Manager.Instance.Data;
        public string GetStudentWorkPrompt()
        {
            string studentName = Data.responseStudent.student_info.name;
            string studentStyle = Data.responseStudent.student_info.style;
            string topic = Data.topic;
            string objective = Data.learningObjective;
            string lessonContent = Data.GetStringFullContent();
            string subject = MentorSubjectExtension.GetSubjectName(Data.subject);

            return $@"Context: You are an AI simulating a student.
Based on the following lesson materials:
Subject: {subject}
Topic: {topic}
Objective: {objective}
Lesson Content:
{lessonContent}

Task: You are a student named {studentName} with a {studentStyle} learning style. Submit a simulated assignment.
Requirements:
1. The submission must reflect the {studentStyle} (VARK) learning style.
2. The content should be realistic and concise (under 40 words), and may include one minor mistake.
3. Return ONLY a single JSON object: {{ ""studentWork"": ""..."" }}";
        }

        // Hàm 2: Gợi ý Feedback cho giáo viên
        public string GetFeedbackSuggestionsPrompt(string studentWork, string objective)
        {
            string subject = MentorSubjectExtension.GetSubjectName(Data.subject);
            return $@"Context: You are a Pedagogical AI Mentor.
Subject: {subject}
Student Submission: ""{studentWork}""
Learning Objective: ""{objective}""

Task: Suggest concise feedback snippets for the teacher to choose from.
Requirements:
1. Return ONLY a single JSON object.
2. Each array (strengths, improvements, nextSteps) must contain exactly 3 items.
3. Each item must be extremely concise (under 10 words).
4. Language: Encouraging English.

JSON Format:
{{
  ""strengths"": [""..."", ""..."", ""...""],
  ""improvements"": [""..."", ""..."", ""...""],
  ""nextSteps"": [""..."", ""..."", ""...""]
}}";
        }
    }
    [Serializable]
    public class StudentWorkDTO
    {
        public string studentWork; // Nội dung bài làm AI gen dựa trên giáo án Scene 3
    }
    [Serializable]
    public class FormativeFeedbackSuggestionsDTO
    {
        public string[] strengths;
        public string[] improvements;
        public string[] nextSteps;
    }
    public enum EFeedback
    {
        Strength = 0,
        Improvement = 1,
        NextStep = 2
    }
    public class FeedbackSuggestion
    {
        public string text;
        public EFeedback type;
        public FeedbackSuggestion(string text, EFeedback type)
        {
            this.text = text;
            this.type = type;
        }
    }
}
