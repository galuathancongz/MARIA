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
            OnSendMsg(strRequest);
        }
        public void OnClickRefine()
        {
            isShowRefine = !isShowRefine;
            selectRefine.Select(isShowRefine);
        }
        public void OnClickRegenerate()
        {
            var strRequest = GetRequestRegenerate();
            OnSendMsg(strRequest);
        }
        public void OnSendMsgRefine(string str)
        {
            var strRequest = GetRequestRefine(str);

        }
        public void OnSendMsg(string strRequest)
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
            return $"System: Bạn là Mentor AI {Level2Manager.Instance.GetNameMentor()}." +
                $" Hãy giải quyết thử thách dạy học sau." +
                $"User Request: {Level2Manager.Instance.Data.question2_3_1}" +
                $"Output Requirement: Phản hồi CHỈ chứa mã JSON hợp lệ, không có văn bản thừa, không có dấu nháy ngược (markdown). Nội dung trong các trường phải là một chuỗi văn bản duy nhất, sử dụng \n để xuống dòng giữa các ý." +
                "JSON Structure: { \"script\": \"dòng thoại 1\\ndòng thoại 2\", \"visual\": \"mô tả visual 1\\nmô tả visual 2\", \"quiz\": \"câu hỏi 1\\ncâu hỏi 2\" }";
        }
        private string GetRequestRefine(string str)
        {
            return $"System: Bạn là Mentor AI {Level2Manager.Instance.GetNameMentor()}." +
                $" Hãy giải quyết thử thách dạy học sau." +
                $"User Request: {Level2Manager.Instance.Data.question2_3_1} thêm {str}" +
                $"Output Requirement: Phản hồi CHỈ chứa mã JSON hợp lệ, không có văn bản thừa, không có dấu nháy ngược (markdown). Nội dung trong các trường phải là một chuỗi văn bản duy nhất, sử dụng \n để xuống dòng giữa các ý." +
                "JSON Structure: { \"script\": \"dòng thoại 1\\ndòng thoại 2\", \"visual\": \"mô tả visual 1\\nmô tả visual 2\", \"quiz\": \"câu hỏi 1\\ncâu hỏi 2\" }";
        }
        private string GetRequestRegenerate()
        {
            return $"System: Bạn là Mentor AI {Level2Manager.Instance.GetNameMentor()}." +
                $" Hãy giải quyết thử thách dạy học sau." +
                $"User Request: Tạo lại {Level2Manager.Instance.Data.question2_3_1}" +
                $"Output Requirement: Phản hồi CHỈ chứa mã JSON hợp lệ, không có văn bản thừa, không có dấu nháy ngược (markdown). Nội dung trong các trường phải là một chuỗi văn bản duy nhất, sử dụng \n để xuống dòng giữa các ý." +
                "JSON Structure: { \"script\": \"dòng thoại 1\\ndòng thoại 2\", \"visual\": \"mô tả visual 1\\nmô tả visual 2\", \"quiz\": \"câu hỏi 1\\ncâu hỏi 2\" }";
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
