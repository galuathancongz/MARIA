using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luzart
{
    public class UIStoryboard_Level2_3_1 : Storyboard
    {
        [Header("Tab Switch")]
        public BaseSelect selectTab;
        [SerializeField][ReadOnly] private int currentTab = 0;

        [Header("Refine")]
        public BaseSelect selectRefine;
        [SerializeField][ReadOnly] private bool isShowRefine = false;

        [Header("Conversation")]
        public Level2_ConversationItem conversationItem;

        private Level2_3_1_Data _cachedData;
        private bool _isLoading;
        private bool _isHide;
        private string _errorMsg;

        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            _isHide = false;
            _cachedData = null;
            _errorMsg = null;
            SelectTab(0);
            var strRequest = GetRequest();
            SendMsg(strRequest);
        }

        public void SelectTab(int index)
        {
            currentTab = index;
            selectTab?.Select(index);
            ShowCachedContent();
        }

        private void ShowCachedContent()
        {
            if (conversationItem == null) return;

            if (_isLoading)
            {
                conversationItem.SetThinking();
                return;
            }
            if (_errorMsg != null)
            {
                conversationItem.ShowText(_errorMsg);
                return;
            }
            if (_cachedData != null)
            {
                string content = GetContentByTab(currentTab);
                conversationItem.ShowText(content);
            }
        }

        private string GetContentByTab(int tab)
        {
            if (_cachedData == null) return "";
            return tab switch
            {
                0 => _cachedData.script ?? "",
                1 => _cachedData.visual ?? "",
                2 => _cachedData.quiz ?? "",
                _ => ""
            };
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
            _cachedData = null;
            _errorMsg = null;
            _isLoading = true;
            conversationItem?.SetThinking();
        }

        private void OnResultString(string str)
        {
            _isLoading = false;
            if (_isHide || !gameObject) return;

            try
            {
                _cachedData = JsonConvert.DeserializeObject<Level2_3_1_Data>(str);
            }
            catch (Exception e)
            {
                Debug.LogError($"Level2_3_1_Data JsonUtility Error: {e.Message}");
                _errorMsg = $"{LocalizationManager.Instance.Get("ui.error_invalid_data")} \n{e.Message}\n {str}";
                conversationItem?.ShowText(_errorMsg);
                return;
            }

            string content = GetContentByTab(currentTab);
            conversationItem?.ShowTextAnim(content);
        }

        private string GetRequest()
        {
            return LocalizationManager.Instance.GetPrompt("prompts.level2_3_1_request", new Dictionary<string, string> {
                {"mentorName", MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)},
                {"userRequest", Level2Manager.Instance.Data.question2_3_1}
            });
        }

        private string GetRequestRefine(string str)
        {
            return LocalizationManager.Instance.GetPrompt("prompts.level2_3_1_refine", new Dictionary<string, string> {
                {"mentorName", MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)},
                {"userRequest", Level2Manager.Instance.Data.question2_3_1},
                {"refinement", str}
            });
        }

        private string GetRequestRegenerate()
        {
            return LocalizationManager.Instance.GetPrompt("prompts.level2_3_1_regenerate", new Dictionary<string, string> {
                {"mentorName", MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)},
                {"userRequest", Level2Manager.Instance.Data.question2_3_1}
            });
        }

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
