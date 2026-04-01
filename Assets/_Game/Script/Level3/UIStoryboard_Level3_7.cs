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
    public class UIStoryboard_Level3_7 : Storyboard
    {
        public TMP_Text txtPersona;
        public TMP_Text txtColor;
        public ProgressBarUI progressLevel2;
        public ProgressBarUI progressLevel3;
        public TMP_Text txtProgressLevel2;
        public TMP_Text txtProgressLevel3;
        public TMP_InputField txtResponse;
        public TMP_Text txtAIUsed;
        public TMP_Text txtRevisionsMade;
        public TMP_Text txtEngagementTypes;
        public TMP_Text txtBadgesCount;
        private DataLevel3_7 _data = new DataLevel3_7();
        public override void Show(Action onHideDone)
        {
            try
            {
                base.Show(onHideDone);
                string persona = PersonaManager.Instance.GetStringPersonaType();
                txtPersona.text = persona;
                txtColor.text = PersonaManager.Instance.GetNameColorPersonaType();

                // Performance insights
                var l3 = Level3Manager.Instance.Data;
                int aiL2 = Level2Manager.Instance?.Data?.listConverstationState
                    ?.Sum(x => x.listConverstationData.Count(y => y.role == ERole.Me)) ?? 0;
                int aiL3 = l3.GetAllSendAI();

                if (txtAIUsed) txtAIUsed.text = (aiL2 + aiL3).ToString();
                if (txtRevisionsMade) txtRevisionsMade.text = l3.totalRefineCount.ToString();
                if (txtEngagementTypes) txtEngagementTypes.text = (l3.filterIndices?.Count ?? 0).ToString();
                if (txtBadgesCount) txtBadgesCount.text = $"{SkillManager.Instance?.CountAll() ?? 0} / {SkillManager.Instance?.TotalAll() ?? 20}";

                // Badge: lesson design aligned with the player's Level 1 teaching persona
                SkillManager.Instance?.UnlockSkill(ESkillId.PersonaAligned);


                Send();
                UIManager.Instance.ShowLoading();
            }
            catch (Exception ex)
            {
                Debug.LogError("Show Level3_7: " + ex.Message);
                txtResponse.text = $"{LocalizationManager.Instance.Get("ui.error_try_again")} + {ex}";
            }

        }
        public void OnExportPlan()
        {
            PdfExporter.ExportLessonPlan();
        }
        public void OnBackToMain()
        {
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.MainMenu);
        }
        public void OnNextPostQuiz()
        {
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.Level4);
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

                // Lưu vào Level3Data để tracking + PDF export
                var data = Level3Manager.Instance.Data;
                data.percentLevel2 = json.percentLevel2;
                data.percentLevel3 = json.percentLevel3;
                data.personalisedFeedback = json.personalisedFeedback;

                OnDisplayData();
            }
            catch (Exception e)
            {
                txtResponse.text = $"{LocalizationManager.Instance.Get("ui.error_try_again")} {e.Message}";
                Debug.LogError($"Error parsing level 3-7 response: {e.Message}");
            }
        }

        private string GetRequestLevel7()
        {
            var data = Level3Manager.Instance.Data;
            int revisionCount = data.GetAllSendAI();
            // Filter index 0 = "Differentiation required", 2 = "Accessibility support"
            bool usedInclusion = data.HasFilter(0) || data.HasFilter(2);

            return LocalizationManager.Instance.GetPrompt("prompts.level3_7_final", new System.Collections.Generic.Dictionary<string, string> {
                {"subject", data.SubjectName},
                {"topic", data.Topic},
                {"revisionCount", revisionCount.ToString()},
                {"usedInclusion", usedInclusion ? "Yes" : "No"},
                {"studentWork", data.studentWork ?? ""}
            });
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
