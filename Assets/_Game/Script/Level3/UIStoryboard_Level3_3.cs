using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class UIStoryboard_Level3_3 : Storyboard
    {
        public List<ItemCheckBox_Level3_3> listCheckBox3 = new List<ItemCheckBox_Level3_3>();
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
            if (GetCurrentFieldIndex() < listCheckBox3.Count)
            {
                var strTitle = CurrentTitle();
                var strRequest = GetLevel3Prompt(strTitle, "");
                Send(strRequest);
            }
            else
            {
                conversationMain.ShowText(LocalizationManager.Instance.Get("ui.completed_all"));
                conversationTip.ShowText("");
                UIManager.Instance.uiTop.ShowBtnNext(true);
            }
        }
        private int GetCurrentFieldIndex()
        {
            var data = Data.listDataTitleTeach.Where(x => x.topic == Data.topic).Select(x => x.title).ToList();
            for (int i = 0; i < listCheckBox3.Count; i++)
            {
                if (!data.Contains(listCheckBox3[i].title))
                {
                    return i;
                }
            }
            return listCheckBox3.Count;
        }
        public void CheckSetCheckBox()
        {
            bool isAllTrue = true;
            var data = Data.listDataTitleTeach.Where(x => x.topic == Data.topic).Select(x => x.title).ToList();
            for (int i = 0; i < listCheckBox3.Count; i++)
            {
                var isOpen = data.Contains(listCheckBox3[i].title);
                listCheckBox3[i].bsUsing.Select(false);
                listCheckBox3[i].toggle.Select(isOpen);
                if(!isOpen)
                {
                    isAllTrue = false;
                }
            }
            if (isAllTrue)
            {
                return;
            }
            int lastTrueIndex = listCheckBox3.FindLastIndex(x => data.Contains(x.title));
            if (lastTrueIndex + 1 < listCheckBox3.Count)
            {
                listCheckBox3[lastTrueIndex+1].bsUsing.Select(true);
            }
        }
        private string CurrentTitle()
        {
            int index = Mathf.Clamp(GetCurrentFieldIndex(),0, listCheckBox3.Count-1);
            return listCheckBox3[index].title;
        }

        public void OnClickRefine()
        {
            isShowRefine = !isShowRefine;
            selectRefine.Select(isShowRefine);
        }
        public void OnClickSendRefine(string str)
        {
            var strTitle = CurrentTitle();
            var strRequest = GetLevel3Prompt(strTitle, str);
            _refineCountForCurrentSection++;
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

            var title = CurrentTitle();
            Level3Manager.Instance.Data.SetDataTitleTeach(title, dataRequest.suggestion);
            Level3Manager.Instance.Save();

            // Badge: used a differentiation / inclusivity filter
            if (Data.optionalFilters != null && Data.optionalFilters.Count > 0)
                SkillManager.Instance?.UnlockSkill(ESkillId.InclusivePlanner);

            // Reset per-section refine counter when moving to the next field
            _refineCountForCurrentSection = 0;

            if (GetCurrentFieldIndex() > listCheckBox3.Count - 1)
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
            var strTitle = CurrentTitle();
            var strRequest = GetLevel3Prompt(strTitle, "");
            strRequest = strRequest + LocalizationManager.Instance.Get("ui.regenerate_request");
            Send(strRequest);
        }
        private void OnDoneResults(string str)
        {
            try
            {
                var data = JsonUtility.FromJson<Level3_3Data>(str);
                dataRequest = data;
                conversationMain.ShowTextAnim(data.suggestion);
                conversationTip.ShowTextAnim(data.tips);
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

        private string GetLevel3Prompt(string currentField, string userRequest)
        {
            Level3Data data = Level3Manager.Instance.Data;
            string topic = data.topic;

            StringBuilder stringBuilder = new StringBuilder();
            if(data.optionalFilters != null && data.optionalFilters.Count > 0)
            {
                for (int i = 0; i < data.optionalFilters.Count; i++)
                {
                
                    stringBuilder.Append(data.optionalFilters[i]);
                    if (i < data.optionalFilters.Count - 1)
                        stringBuilder.Append(", ");
                }
            }
            var filters = stringBuilder.ToString();
            var baseObjective = data.learningObjective;
            var constraints = data.designContraints;

            return LocalizationManager.Instance.GetPrompt("prompts.level3_3_lesson", new System.Collections.Generic.Dictionary<string, string> {
                {"topic", topic},
                {"baseObjective", baseObjective},
                {"constraints", constraints},
                {"filters", filters},
                {"currentField", currentField},
                {"userRequest", userRequest}
            });
        }
    }
    public class Level3_3Data
    {
        public string suggestion;
        public string tips;
    }
}
