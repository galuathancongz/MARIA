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
        public List<ItemCheckBox_Level3_3> listCheckBox3;
        public BaseSelect selectRefine;
        [SerializeField]
        [ReadOnly]
        private bool isShowRefine = false;
        public Level2_ConversationItem conversationMain;
        public Level2_ConversationItem conversationTip;
        private Level3Data Data => Level3Manager.Instance.Data;
        private Level3_3Data dataRequest = new Level3_3Data();
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            CheckOrSendNext();
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
                conversationMain.ShowText("You have completed all the items in the lesson plan!");
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
            var data = Data.listDataTitleTeach.Where(x => x.topic == Data.topic).Select(x => x.title).ToList();
            for (int i = 0; i < listCheckBox3.Count; i++)
            {
                var isOpen = data.Contains(listCheckBox3[i].title);
                                listCheckBox3[i].toggle.Select(isOpen);
            }
        }
        private string CurrentTitle()
        {
            return listCheckBox3[GetCurrentFieldIndex()].title;
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
            Send(strRequest);
        }
        public void OnClickAccept()
        {
            if (Data.GetConverstationState(0) == EState.WaitAI)
            {
                UIManager.Instance.ShowToast("Please wait AI complete response !");
                return;
            }

            var title = CurrentTitle();
            Level3Manager.Instance.Data.SetDataTitleTeach(title, dataRequest.suggestion);
            Level3Manager.Instance.Save();
            if(GetCurrentFieldIndex() > listCheckBox3.Count - 1)
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
                UIManager.Instance.ShowToast("Please wait AI complete response !");
                return;
            }
            var strTitle = CurrentTitle();
            var strRequest = GetLevel3Prompt(strTitle, "");
            strRequest = strRequest + "\n\nRequest: Please recreate the content for this section.";
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
                conversationMain.ShowText("Error! Try again !");
            }
        }
        private void Send(string strRequest)
        {
            Level3Manager.Instance.Send(0, strRequest, OnDoneResults);
            conversationMain.SetThinking();
        }

        private string GetLevel3Prompt(string currentField, string userRequest)
        {
            Level3Data data = Level3Manager.Instance.Data;
            string topic = data.topic;

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < data.optionalFilters.Count; i++)
            {
                stringBuilder.Append(data.optionalFilters[i]);
                if (i < data.optionalFilters.Count - 1)
                    stringBuilder.Append(", ");
            }

            var baseObjective = data.learningObjective;
            var constraints = data.designContraints;
            var filters = stringBuilder.ToString();

            return "Context: You are a lesson design assistant in a 'Co-design Studio'. The teacher leads the process, and you provide supportive, high-quality content.\n" +
                   "Established Setup from Scene 2:\n" +
                   $"- Topic: {topic} \n" +
                   $"- Core Learning Objective: {baseObjective} \n" +
                   $"- Design Constraints: {constraints} \n" +
                   $"- Optional Filters: {filters} \n\n" +

                   $"Task: Generate content for the '{currentField}' section of the lesson plan.\n" +
                   $"Teacher's Specific Request: \"{userRequest}\"\n\n" +

                   "Technical Requirements:\n" +
                   "1. Practicality: Content must align strictly with the core objective and design constraints.\n" +
                   "2. Localization: Use examples suitable for the England educational context.\n" +
                   "3. Pedagogical Insight: The 'tips' section should offer actionable advice, including many small tips (e.g., how to make activities more student-centered).\n" +
                   "4. Formatting: Return ONLY a single JSON object. No markdown, no backticks, no introductory text.\n" +
                   "5. Conciseness: The Mentor/MARIA's advice (tips) must be extremely brief, under 20 words.\n" +
                   "6. Focus: Prioritize the main task content, ensuring it is concise and feasible.\n\n" +

                   "Output JSON Format:\n" +
                   "{\n" +
                   "  \"suggestion\": \"detailed suggested content for this section (including scripts, materials if needed)\",\n" +
                   "  \"tips\": \"brief advice from MARIA or the Mentor to improve the activity\"\n" +
                   "}";
        }
    }
    public class Level3_3Data
    {
        public string suggestion;
        public string tips;
    }
}
