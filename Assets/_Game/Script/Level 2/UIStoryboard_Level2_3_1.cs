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
                UIManager.Instance.ShowToast(LocalizationManager.Instance.Get("ui.wait_ai_refine"));
                return;
            }
            var strRequest = GetRequestRegenerate();
            SendMsg(strRequest);
        }
        public void OnSendMsgRefine(string str)
        {
            if (Level2Manager.Instance.Data.GetConverstationState(1) != EState.CanWrite)
            {
                UIManager.Instance.ShowToast(LocalizationManager.Instance.Get("ui.wait_ai_refine"));
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
                var errorMsg = $"{LocalizationManager.Instance.Get("ui.error_invalid_data")} \n{e.Message}\n {str}";
                scriptConversation.ShowText(errorMsg);
                visualConversationItem.ShowText(errorMsg);
                quizzConversationItem.ShowText(errorMsg);
                return;
            }
            scriptConversation.ShowTextAnim(json.script);
            visualConversationItem.ShowTextAnim(json.visual);
            quizzConversationItem.ShowTextAnim(json.quiz);
        }

        private string GetRequest()
        {
            return LocalizationManager.Instance.GetPrompt("prompts.level2_3_1_request", new System.Collections.Generic.Dictionary<string, string> {
                {"mentorName", MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)},
                {"userRequest", Level2Manager.Instance.Data.question2_3_1}
            });
        }

        private string GetRequestRefine(string str)
        {
            return LocalizationManager.Instance.GetPrompt("prompts.level2_3_1_refine", new System.Collections.Generic.Dictionary<string, string> {
                {"mentorName", MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)},
                {"userRequest", Level2Manager.Instance.Data.question2_3_1},
                {"refinement", str}
            });
        }

        private string GetRequestRegenerate()
        {
            return LocalizationManager.Instance.GetPrompt("prompts.level2_3_1_regenerate", new System.Collections.Generic.Dictionary<string, string> {
                {"mentorName", MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)},
                {"userRequest", Level2Manager.Instance.Data.question2_3_1}
            });
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
