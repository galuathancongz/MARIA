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
        public TMP_Text txtResponse;
        public Button btnExportPlan;
        private DataLevel3_7 _data = new DataLevel3_7();
        public override void Show(Action onHideDone)
        {
            try
            {
                base.Show(onHideDone);
                string persona = PersonaManager.Instance.GetStringPersonaType();
                txtPersona.text = persona;
                txtColor.text = PersonaManager.Instance.GetNameColorPersonaType();

                // Badge: lesson design aligned with the player's Level 1 teaching persona
                SkillManager.Instance?.UnlockSkill(ESkillId.PersonaAligned);

                GameUtil.ButtonOnClick(btnExportPlan, PdfExporter.ExportLessonPlan);

                Send();
                UIManager.Instance.ShowLoading();
            }
            catch (Exception ex)
            {
                Debug.LogError("Show Level3_7: " + ex.Message);
                txtResponse.text = $"{LocalizationManager.Instance.Get("ui.error_try_again")} + {ex}";
            }

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
            bool usedInclusion = data.optionalFilters.Contains("Inclusion") || data.optionalFilters.Contains("Accessibility");

            return LocalizationManager.Instance.GetPrompt("prompts.level3_7_final", new System.Collections.Generic.Dictionary<string, string> {
                {"subject", MentorSubjectExtension.GetSubjectName(data.subject)},
                {"topic", data.topic},
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
