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
    public class UIStoryboard_Level2_3_1 : Storyboard
    {
        public BaseSelect selectRefine;
        [SerializeField]
        [ReadOnly]
        private bool isShowRefine = false;
        public Level2_ConversationItem scriptConversation;
        public Level2_ConversationItem visualConversationItem;
        public Level2_ConversationItem quizzConversationItem;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            _isHide = false;
            var strRequest = GetRequest();
            SendMsg(strRequest);
        }
        public void OnClickRefine()
        {
            isShowRefine = !isShowRefine;
            selectRefine.Select(isShowRefine);
        }
        public void OnClickRegenerate()
        {
            if (Level2Manager.Instance.Data.GetConverstationState(1) != EState.CanWrite)
            {
                UIManager.Instance.ShowToast("Please wait for AI to finish before refining.");
                return;
            }
            var strRequest = GetRequestRegenerate();
            SendMsg(strRequest);
        }
        public void OnSendMsgRefine(string str)
        {
            if (Level2Manager.Instance.Data.GetConverstationState(1) != EState.CanWrite)
            {
                UIManager.Instance.ShowToast("Please wait for AI to finish before refining.");
                return;
            }
            var strRequest = GetRequestRefine(str);
            SendMsg(strRequest);
        }
        public void SendMsg(string strRequest)
        {
            Level2Manager.Instance.Send(1, strRequest, OnResultString);
            isShowRefine = false;
            selectRefine.Select(false);
            scriptConversation.SetThinking();
            visualConversationItem.SetThinking();
            quizzConversationItem.SetThinking();
        }
        private void OnResultString(string str)
        {
            if (_isHide || !gameObject)
            {
                return;
            }
            Level2_3_1_Data json= new Level2_3_1_Data();
            try
            {
                json = JsonConvert.DeserializeObject<Level2_3_1_Data>(str);

            }
            catch (Exception e)
            {
                Debug.LogError($"Level2_3_1_Data JsonUtility Error: {e.Message}");
                scriptConversation.ShowText($"[Error] Dữ liệu không hợp lệ, vui lòng thử lại. \n{e.Message}\n {str}");
                visualConversationItem.ShowText($"[Error] Dữ liệu không hợp lệ, vui lòng thử lại. \n{e.Message}\n {str}");
                quizzConversationItem.ShowText($"[Error] Dữ liệu không hợp lệ, vui lòng thử lại. \n{e.Message}\n {str}");
                return;
            }
            scriptConversation.ShowTextAnim(json.script);
            visualConversationItem.ShowTextAnim(json.visual);
            quizzConversationItem.ShowTextAnim(json.quiz);
        }

        private string GetRequest()
        {
            return $"System: You are AI Mentor {MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)}." +
                $" Please solve the following teaching challenge. Limit your response to approximately 200 tokens." +
                $"User Request: {Level2Manager.Instance.Data.question2_3_1}" +
                $"The output must include: 'script' (the AI Mentor's dialogue), 'visual' (descriptions of supporting illustrations), and 'quiz' (a simple, fun quick-check question)." +
                $"Output Requirement: Provide ONLY valid JSON code. No markdown, no backticks, and no extra text. Content within fields must be a single string, using \\n for line breaks between points." +
                "JSON Structure: { \"script\": \"line 1\\nline 2\", \"visual\": \"visual description 1\\nvisual description 2\", \"quiz\": \"question 1\\nquestion 2\" }";
        }

        private string GetRequestRefine(string str)
        {
            return $"System: You are AI Mentor {MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)}." +
                $" Please solve the following teaching challenge. Limit your response to approximately 200 tokens." +
                $"User Request: {Level2Manager.Instance.Data.question2_3_1} with the following addition: {str}" +
                $"The output must include: 'script' (the AI Mentor's dialogue), 'visual' (descriptions of supporting illustrations), and 'quiz' (a simple, fun quick-check question)." +
                $"Output Requirement: Provide ONLY valid JSON code. No markdown, no backticks, and no extra text. Content within fields must be a single string, using \\n for line breaks between points." +
                "JSON Structure: { \"script\": \"line 1\\nline 2\", \"visual\": \"visual description 1\\nvisual description 2\", \"quiz\": \"question 1\\nquestion 2\" }";
        }

        private string GetRequestRegenerate()
        {
            return $"System: You are AI Mentor {MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)}." +
                $" Please solve the following teaching challenge. Limit your response to approximately 200 tokens." +
                $"User Request: Regenerate the content for: {Level2Manager.Instance.Data.question2_3_1}" +
                $"The output must include: 'script' (the AI Mentor's dialogue), 'visual' (descriptions of supporting illustrations), and 'quiz' (a simple, fun quick-check question)." +
                $"Output Requirement: Provide ONLY valid JSON code. No markdown, no backticks, and no extra text. Content within fields must be a single string, using \\n for line breaks between points." +
                "JSON Structure: { \"script\": \"line 1\\nline 2\", \"visual\": \"visual description 1\\nvisual description 2\", \"quiz\": \"question 1\\nquestion 2\" }";
        }
        private bool _isHide = false;
        public override void Hide()
        {
            base.Hide();
            _isHide = true;
        }

    }
    [Serializable]
    public class Level2_3_1_Data
    {
        public string script;
        public string visual;
        public string quiz;
    }
}
