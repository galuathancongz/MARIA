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
        [Space]
        public TMP_InputField inputFieldStrength;
        public TMP_InputField inputFieldImprovement;
        public TMP_InputField inputFieldNextStep;
        [Space]
        public BaseSelect selectStrength;
        public BaseSelect selectImprovement;
        public BaseSelect selectNextStep;
        [Space]
        public Transform content;
        public List<Item_ClickToImport> listItemClickToImport = new List<Item_ClickToImport>();
        public Item_ClickToImport itemPrefab;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            try
            {
                string promptStudentWork = GetStudentWorkPrompt();
                Level3Manager.Instance.Send(2, promptStudentWork, OnGetStudentWork);
                UIManager.Instance.ShowLoading();
                btnNextStep.gameObject.SetActive(false);
                selectStrength.Select(false);
                selectImprovement.Select(false);
                selectNextStep.Select(false);

            }
            catch (Exception ex)
            {
                Debug.LogError("Show Level3_5: " + ex.Message);
                conversationItemMain.ShowText($"{LocalizationManager.Instance.Get("ui.error_try_again")} + {ex}");
            }
        }
        private void OnGetStudentWork(string str)
        {
            UIManager.Instance.HideLoading();
            try
            {
                var studentWorkDTO = JsonUtility.FromJson<StudentWorkDTO>(str);
                string studentWork = studentWorkDTO.studentWork;
                // Defensive: normalize literal "\n" (2 chars) into real newlines
                // so TMP renders line breaks instead of the raw escape sequence.
                if (!string.IsNullOrEmpty(studentWork))
                    studentWork = studentWork.Replace("\\n", "\n");
                Data.studentWork= studentWork;
                conversationItemMain.ShowText(LocalizationManager.Instance.GetFormat("ui.student_response", studentWork));
                GenerateFeedback();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing student work response: {ex.Message}");
                conversationItemMain.ShowText(LocalizationManager.Instance.Get("ui.error_parse_student"));
            }


        }

        private static string NormalizeAiText(string s)
        {
            // Convert literal "\n" (2 chars) into real newlines so TMP renders
            // line breaks. No-op when the text already contains real newlines.
            return string.IsNullOrEmpty(s) ? s : s.Replace("\\n", "\n");
        }

        private void OnGetFeedbackSuggestions(string str)
        {

            try
            {
                var feedbackDTO = JsonUtility.FromJson<FormativeFeedbackSuggestionsDTO>(str);
                List<FeedbackSuggestion> suggestions = new List<FeedbackSuggestion>();
                suggestions.AddRange((feedbackDTO.strengths ?? new string[0]).Select(s => new FeedbackSuggestion(NormalizeAiText(s), EFeedback.Strength)));
                suggestions.AddRange((feedbackDTO.improvements ?? new string[0]).Select(s => new FeedbackSuggestion(NormalizeAiText(s), EFeedback.Improvement)));
                suggestions.AddRange((feedbackDTO.nextSteps ?? new string[0]).Select(s => new FeedbackSuggestion(NormalizeAiText(s), EFeedback.NextStep)));

                MasterHelper.InitListObj(suggestions.Count,itemPrefab, listItemClickToImport, content, (item, index) =>
                {
                    var data = suggestions[index];
                    item.gameObject.SetActive(true);
                    item.Initialize(data.type,data.text,OnClick);
                });


            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing feedback suggestions response: {ex}");
                UIManager.Instance.ShowToast(LocalizationManager.Instance.Get("ui.error_parse_feedback"));
            }
        }
        public void OnClickGenerateFeedback()
        {
            if(Data.GetConverstationState(2) == EState.WaitAI)
            {
                UIManager.Instance.ShowToast(LocalizationManager.Instance.Get("ui.generating_feedback"));
                return;
            }
            GenerateFeedback();
        }
        void GenerateFeedback()
        {
            string objective = Level3Manager.Instance.Data.LearningObjective;
            string promptFeedback = GetFeedbackSuggestionsPrompt(Data.studentWork, objective);
            MasterHelper.InitListObj(1, itemPrefab, listItemClickToImport, content, (item, index) =>
            {
                item.gameObject.SetActive(true);
                item.Initialize(EFeedback.Strength, LocalizationManager.Instance.Get("ui.generating_suggestion"), null);
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
            selectStrength.Select(true);
            // Badge: gave strength feedback
            SkillManager.Instance?.UnlockSkill(ESkillId.FeedbackArchitect);
        }
        public void OnClickSendImprovement(string str)
        {
            if(string.IsNullOrEmpty(str)) return;
            var data = new FeedbackSuggestion(str, EFeedback.Improvement);
            Data.AddFeedbackSuggestion(data);
            if (!listFeedbackSuggestions.Contains(data))
                listFeedbackSuggestions.Add(data);
            CheckShowNextStep();
            selectImprovement.Select(true);
            // Badge: gave area-of-improvement feedback
            SkillManager.Instance?.UnlockSkill(ESkillId.SeekingImprovement);
        }
        public void OnClickSendNextStep(string str)
        {
            if (string.IsNullOrEmpty(str)) return;
            var data = new FeedbackSuggestion(str, EFeedback.NextStep);
            Data.AddFeedbackSuggestion(data);
            if (!listFeedbackSuggestions.Contains(data))
                listFeedbackSuggestions.Add(data);
            CheckShowNextStep();
            selectNextStep.Select(true);
            // Badge: gave concrete next-step feedback
            SkillManager.Instance?.UnlockSkill(ESkillId.ForwardLookingDesigner);
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
            return LocalizationManager.Instance.GetPrompt("prompts.level3_5_student_work", new System.Collections.Generic.Dictionary<string, string> {
                {"subject", Data.SubjectName},
                {"topic", Data.Topic},
                {"objective", Data.LearningObjective},
                {"lessonContent", Data.GetStringFullContent()},
                {"studentName", Data.responseStudent.student_info.name},
                {"studentStyle", Data.responseStudent.student_info.style}
            });
        }

        public string GetFeedbackSuggestionsPrompt(string studentWork, string objective)
        {
            return LocalizationManager.Instance.GetPrompt("prompts.level3_5_suggestions", new System.Collections.Generic.Dictionary<string, string> {
                {"subject", Data.SubjectName},
                {"studentWork", studentWork},
                {"objective", objective}
            });
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
