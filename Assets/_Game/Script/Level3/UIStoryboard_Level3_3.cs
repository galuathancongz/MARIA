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
                conversationMain.ShowText("Bạn đã hoàn thành tất cả các mục trong giáo án!");
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
                UIManager.Instance.ShowToast("Vui lòng chờ AI hoàn thành phản hồi.");
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
            var strTitle = CurrentTitle();
            var strRequest = GetLevel3Prompt(strTitle, "");
            strRequest = strRequest + "\n\nYêu cầu: Hãy tạo lại nội dung cho mục này.";
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
                conversationMain.ShowText("Đã có lỗi xảy ra, vui lòng thử lại.");
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
                    stringBuilder.Append(',');
            }
            var baseObjective = data.learningObjective;
            var constraints = data.designContraints;
            var filters = stringBuilder.ToString();
            return "Context: Bạn là trợ lý thiết kế bài giảng trong một 'Co-design studio'. Giáo viên là người dẫn dắt, bạn là trợ lý đáp ứng.\n" +
                       "Thông tin thiết lập từ Scene 2:\n" +
                       $"- Chủ đề: {topic} \n" +
                       $"- Mục tiêu học tập gốc: {baseObjective} \n" +
                       $"- Ràng buộc thiết kế: {constraints} \n" +
                       $"- Bộ lọc bổ sung (Optional Filters): {filters} \n\n" +

                       $"Nhiệm vụ: Hỗ trợ soạn nội dung cho mục '{currentField}' trong giáo án.\n" +
                       $"Yêu cầu cụ thể của giáo viên: \"{userRequest}\"\n\n" +

                       "Yêu cầu kỹ thuật:\n" +
                       "1. Tính thực tế: Nội dung phải bám sát mục tiêu gốc và ràng buộc thiết kế.\n" +
                       "2. Bản địa hóa: Sử dụng ví dụ phù hợp văn hóa Việt Nam.\n" +
                       "3. Gợi ý sư phạm: Phần 'tips' phải đưa ra các mẹo thực tế (ví dụ: cách làm cho hoạt động tập trung vào học sinh hơn).\n" +
                       "4. Định dạng: CHỈ trả về JSON duy nhất theo cấu trúc bên dưới, không có văn bản giải thích thừa.\n" +
                       "5. Lời khuyên của MARIA hoặc Mentor ngắn thôi chỉ 20 chữ\n" +
                       "6. Tập trung chủ yếu vào phần nhiệm vụ, đảm bảo ngắn gọn và khả thi\n\n" +

                       "Output JSON Format:\n" +
                       "{\n" +
                       "  \"suggestion\": \"nội dung gợi ý chi tiết cho mục này (bao gồm cả kịch bản, vật liệu nếu cần)\",\n" +
                       "  \"tips\": \"lời khuyên của MARIA hoặc Mentor để cải thiện hoạt động\"\n" +
                       "}";
        }
    }
    public class Level3_3Data
    {
        public string suggestion;
        public string tips;
    }
}
