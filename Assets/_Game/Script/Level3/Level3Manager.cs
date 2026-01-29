using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class Level3Manager : SingletonSaveLoad<Level3Data, Level3Manager>
    {
        public static bool IsSendStartCoCreatorStudio = false;
        protected override string KEYLOAD => "Level3_Data";
        public void Send(byte indexConversation, string str, Action<string> onResult)
        {
            Action<string> newOnResult = (result) =>
            {
                var converstationState = Data.AddConvesationData(indexConversation, new ConverstationData() { str = result, role = ERole.AI });
                converstationState.State = EState.CanWrite;
                onResult?.Invoke(result);
            };
            APIManager.Instance.Send(str, newOnResult);
            var state = Data.AddConvesationData(indexConversation, new ConverstationData() { str = str, role = ERole.Me });
            state.State = EState.WaitAI;
        }
    }
    [Serializable]
    public class DataTitleTeachLevel3_3
    {
        public string title;
        public string content;
        public string topic;
    }
    [Serializable]
    public class Level3Data
    {
        public ESubject subject;
        public string topic;
        public string learningObjective;
        public string designContraints;
        public List<string> optionalFilters;
        public List<DataTitleTeachLevel3_3> listDataTitleTeach;
        public StudentFeedbackResponseDTO responseStudent = null;
        public string studentWork = "";
        public List<FeedbackSuggestion> listFeedbackSuggestions = new List<FeedbackSuggestion>();
        public List<string> listFeedbackSelected = new List<string>();

        public int GetAllSendAI()
        {
            return listConverstationState.Sum(x => x.listConverstationData.Count(y => y.role == ERole.Me));
        }
        public string GetObjectiveSummary() => learningObjective;

        public string GetTitleSummary()
        {
            // Ưu tiên trả về topic nếu có
            if (!string.IsNullOrEmpty(topic)) return topic;

            // Nếu không có topic, lấy title của phần tử đầu tiên trong list giáo án
            if (listDataTitleTeach != null && listDataTitleTeach.Count > 0)
            {
                return listDataTitleTeach[0].title;
            }

            return "Untitled Lesson"; // Giá trị mặc định nếu trống trơn
        }
        public string GetActivitiesSummary()
        {
            var activities = listDataTitleTeach.Where(x =>
                x.topic == topic &&
                !x.title.ToLower().Contains("objective")
            ).ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var act in activities)
            {
                sb.AppendLine($"<b>{act.title}:</b> {act.content}\n");
            }
            return sb.ToString();
        }
        public string GetAssessmentSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>Student Work:</b>\n{studentWork}\n");
            sb.AppendLine("<b>Your Feedback:</b>");

            if (listFeedbackSuggestions == null || listFeedbackSuggestions.Count == 0)
            {
                sb.AppendLine("(No feedback selected)");
            }
            else
            {
                foreach (var f in listFeedbackSuggestions)
                {
                    sb.AppendLine($"- {f.text}");
                }
            }
            return sb.ToString();
        }
        public void AddFeedbackSuggestion(FeedbackSuggestion suggestion)
        {
            var item = listFeedbackSuggestions.Find(x => x.text == suggestion.text && x.type == suggestion.type);
            if (item == null)
            {
                listFeedbackSuggestions.Add(suggestion);
            }
            else
            {
                item.text = suggestion.text;
                item.type = suggestion.type;
            }
        }
        public void AddFilter(string filter)
        {
            if (!optionalFilters.Contains(filter))
            {
                optionalFilters.Add(filter);
            }
        }
        public void RemoveFilter(string filter)
        {
            if (optionalFilters.Contains(filter))
            {
                optionalFilters.Remove(filter);
            }
        }
        public string GetStringFullContent()
        {
            var allList = GetDataInTopic(topic);
            string fullContent = "";
            foreach (var item in allList)
            {
                fullContent += $"{item.title}: {item.content}\n";
            }
            return fullContent;
        }
        public List<DataTitleTeachLevel3_3> GetDataInTopic(string topic)
        {
            return listDataTitleTeach.Where(x => x.topic == topic).ToList();
        }
        public void SetDataTitleTeach(string title, string content)
        {
            
            foreach (var item in listDataTitleTeach)
            {
                if (item.title == title && item.topic == topic)
                {
                    item.content = content;
                    return;
                }
            }
            listDataTitleTeach.Add(new DataTitleTeachLevel3_3()
            {
                title = title,
                content = content,
                topic = topic,
            });
        }

        public List<ConversationState> listConverstationState;
        public ConversationState AddConvesationData(byte indexConverstation, ConverstationData data)
        {
            var converstationDataList = listConverstationState.Find(x => x.indexConverstation == indexConverstation);
            if (converstationDataList == null)
            {
                converstationDataList = new ConversationState()
                {
                    indexConverstation = indexConverstation,
                    listConverstationData = new List<ConverstationData>(),
                };
                listConverstationState.Add(converstationDataList);
            }
            converstationDataList.listConverstationData.Add(data);
            return converstationDataList;
        }
        public List<ConverstationData> GetConverstationData(byte indexConverstation)
        {
            var converstationDataList = listConverstationState.Find(x => x.indexConverstation == indexConverstation);
            if (converstationDataList == null)
            {
                return new List<ConverstationData>();
            }
            return converstationDataList.listConverstationData;
        }
        public EState GetConverstationState(byte indexConverstation)
        {
            var converstationDataList = listConverstationState.Find(x => x.indexConverstation == indexConverstation);
            if (converstationDataList == null)
            {
                return EState.CanWrite;
            }
            return converstationDataList.State;
        }
    }
}
