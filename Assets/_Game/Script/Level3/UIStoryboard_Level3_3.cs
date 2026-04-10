using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Luzart
{
    public class UIStoryboard_Level3_3 : Storyboard
    {
        public List<ItemCheckBox_Level3_3> listCheckBox3 = new List<ItemCheckBox_Level3_3>();
        public Button btnNext;
        public BaseSelect selectRefine;
        [SerializeField]
        [ReadOnly]
        private bool isShowRefine = false;
        public Level2_ConversationItem conversationMain;
        public Level2_ConversationItem conversationTip;
        private Level3Data Data => Level3Manager.Instance.Data;
        private Level3_3Data dataRequest = new Level3_3Data();

        // Badge tracking
        private int _refineCountForCurrentSection = 0;

        public override void Show(Action onHideDone)
        {
            try
            {
                base.Show(onHideDone);

                btnNext?.gameObject.SetActive(false);
                // Ensure context hash matches current topic + filters
                Data.EnsureContext();
                CheckOrSendNext();
            }
            catch (Exception ex)
            {
                Debug.LogError("Show Level3_3: " + ex.Message);
                conversationMain.ShowText($"{LocalizationManager.Instance.Get("ui.error_try_again")}\n {ex}");
            }
        }

        private void CheckOrSendNext()
        {
            CheckSetCheckBox();
            int currentIndex = GetCurrentFieldIndex();
            if (currentIndex < listCheckBox3.Count)
            {
                var strRequest = GetLevel3Prompt(currentIndex, "");
                Send(strRequest);
            }
            else
            {
                conversationMain.ShowText(LocalizationManager.Instance.Get("ui.completed_all"));
                conversationTip.ShowText("");
                UIManager.Instance.uiTop.ShowBtnNext(true);
                btnNext?.gameObject.SetActive(true);
            }
        }

        /// <summary>Tìm section index đầu tiên chưa có content.</summary>
        private int GetCurrentFieldIndex()
        {
            return Data.GetFirstIncompleteIndex(listCheckBox3.Count);
        }

        public void CheckSetCheckBox()
        {
            bool isAllTrue = true;
            for (int i = 0; i < listCheckBox3.Count; i++)
            {
                bool done = Data.HasSection(i);
                listCheckBox3[i].bsUsing.Select(false);
                listCheckBox3[i].toggle.Select(done);
                if (!done) isAllTrue = false;
            }
            if (isAllTrue) return;

            // Highlight section đang làm
            int currentIndex = GetCurrentFieldIndex();
            if (currentIndex < listCheckBox3.Count)
                listCheckBox3[currentIndex].bsUsing.Select(true);
        }

        private int CurrentIndex()
        {
            return Mathf.Clamp(GetCurrentFieldIndex(), 0, listCheckBox3.Count - 1);
        }

        public void OnClickRefine()
        {
            isShowRefine = !isShowRefine;
            selectRefine.Select(isShowRefine);
        }

        public void OnClickSendRefine(string str)
        {
            int idx = CurrentIndex();
            var strRequest = GetLevel3Prompt(idx, str);
            _refineCountForCurrentSection++;
            Data.totalRefineCount++;

            // Badge: refined one section 3+ times
            if (_refineCountForCurrentSection >= 3)
                SkillManager.Instance?.UnlockSkill(ESkillId.IterationChampion);
            Send(strRequest);
        }

        public void OnClickAccept()
        {
            if (Data.GetConverstationState(0) == EState.WaitAI)
            {
                UIManager.Instance.ShowToast(LocalizationManager.Instance.Get("ui.wait_ai_complete"));
                return;
            }

            int idx = CurrentIndex();
            Data.SetSection(idx, dataRequest.suggestion);

            // Badge: used a differentiation / inclusivity filter
            if (Data.filterIndices != null && Data.filterIndices.Count > 0)
                SkillManager.Instance?.UnlockSkill(ESkillId.InclusivePlanner);

            // Reset per-section refine counter
            _refineCountForCurrentSection = 0;

            if (GetCurrentFieldIndex() >= listCheckBox3.Count)
            {
                UIManager.Instance.ShowNextScenario();
                return;
            }
            conversationMain.ShowText("");
            conversationTip.ShowText("");
            CheckOrSendNext();
        }

        public void OnClickRegenerate()
        {
            if (Data.GetConverstationState(0) == EState.WaitAI)
            {
                UIManager.Instance.ShowToast(LocalizationManager.Instance.Get("ui.wait_ai_complete"));
                return;
            }
            int idx = CurrentIndex();
            var strRequest = GetLevel3Prompt(idx, "");
            strRequest = strRequest + LocalizationManager.Instance.Get("ui.regenerate_request");
            Send(strRequest);
        }
        public ScrollRect scrollRect;
        void UpdateScrollRect()
        {
            if(scrollRect == null) return;
            scrollRect.ScrollTo(0, 0.1f);
        }
        private void OnDoneResults(string str)
        {
            try
            {
                var data = JsonUtility.FromJson<Level3_3Data>(str);
                dataRequest = data;
                // Defensive: some AI responses emit the two literal characters
                // "\" + "n" instead of the proper JSON \n escape (which would
                // parse to a real newline). TMP would otherwise render the
                // literal \n. Normalize so both forms show as a real line break.
                if (!string.IsNullOrEmpty(dataRequest.suggestion))
                    dataRequest.suggestion = dataRequest.suggestion.Replace("\\n", "\n");
                if (!string.IsNullOrEmpty(dataRequest.tips))
                    dataRequest.tips = dataRequest.tips.Replace("\\n", "\n");
                conversationMain.ShowTextAnim(dataRequest.suggestion, UpdateScrollRect);
                conversationTip.ShowTextAnim(dataRequest.tips);
            }
            catch (Exception ex)
            {
                Debug.LogError("OnDoneResults Level3_3: " + ex.Message);
                conversationMain.ShowText(LocalizationManager.Instance.Get("ui.error_try_again"));
            }
        }

        private void Send(string strRequest)
        {
            Level3Manager.Instance.Send(0, strRequest, OnDoneResults);
            conversationMain.SetThinking();

            // Badge: used AI suggestions 5+ times in lesson co-creation
            if (Data.GetAllSendAI() >= 5)
                SkillManager.Instance?.UnlockSkill(ESkillId.LessonCoCreator);
        }

        private string GetLevel3Prompt(int sectionIndex, string userRequest)
        {
            Level3Data data = Level3Manager.Instance.Data;

            string currentField     = LessonPlanTemplate.GetSectionName(sectionIndex);
            string sectionGuidance  = LessonPlanTemplate.GetSectionGuidance(sectionIndex);
            string filtersText      = string.IsNullOrWhiteSpace(data.FiltersText)
                                      ? LocalizationManager.Instance.Get("level3.filter_none")
                                      : data.FiltersText;

            // Format the user-refinement line: omit entirely when empty
            string userRequestLine = string.IsNullOrWhiteSpace(userRequest)
                ? ""
                : LocalizationManager.Instance.GetPrompt("prompts.level3_user_request_line",
                    new System.Collections.Generic.Dictionary<string, string> { {"userRequest", userRequest} });

            return LocalizationManager.Instance.GetPrompt("prompts.level3_3_lesson", new System.Collections.Generic.Dictionary<string, string> {
                {"topic",           data.Topic},
                {"baseObjective",   data.LearningObjective},
                {"constraints",     data.DesignConstraint},
                {"filters",         filtersText},
                {"currentField",    currentField},
                {"sectionGuidance", sectionGuidance},
                {"userRequestLine", userRequestLine}
            });
        }
    }
    public class Level3_3Data
    {
        public string suggestion;
        public string tips;
    }
}
