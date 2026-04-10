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
        public int    index;        // section index (0-6), maps to LessonPlanTemplate.SectionKeys
        public string content;      // AI generated content
        public string contextHash;  // topic + filters hash — để phân biệt các lần chơi khác nhau
    }
    [Serializable]
    public class Level3Data
    {
        public ESubject subject;                 // enum, đã là int bên dưới
        public int topicIndex = -1;              // index trong DesignChallengeTable (0-2)
        public List<int> filterIndices = new();  // FilterTable indices (0-3)
        public List<DataTitleTeachLevel3_3> listDataTitleTeach = new();
        public string currentContextHash;     // "{subjectIndex}_{topicIndex}_{filtersSorted}"
        public StudentFeedbackResponseDTO responseStudent = null;
        public string studentWork = "";
        public List<FeedbackSuggestion> listFeedbackSuggestions = new List<FeedbackSuggestion>();
        public List<string> listFeedbackSelected = new List<string>();

        // Scene 7 — AI dashboard results (lưu để tracking + PDF export)
        public int percentLevel2;
        public int percentLevel3;
        public string personalisedFeedback;
        public int totalRefineCount = 0;

        // ── Derived text (từ tables, KHÔNG lưu) ─────────────────────────────

        public DesignChallenge CurrentChallenge => DesignChallengeTable.Get(subject, topicIndex);
        public string Topic => Loc.K(CurrentChallenge?.topicKey ?? "");
        public string LearningObjective => Loc.K(CurrentChallenge?.objectiveKey ?? "");
        public string DesignConstraint => Loc.K(CurrentChallenge?.constraintKey ?? "");
        public string SubjectName => MentorSubjectExtension.GetSubjectName(subject);
        public string MentorName => MentorSubjectExtension.GetNameMentor(subject);

        public string FiltersText
        {
            get
            {
                if (filterIndices == null || filterIndices.Count == 0) return "";
                return string.Join(", ", filterIndices.Select(i => FilterTable.GetName(i)));
            }
        }

        // ── Context ──────────────────────────────────────────────────────────

        public string BuildCurrentContextHash()
        {
            var sorted = filterIndices != null ? new List<int>(filterIndices) : new List<int>();
            sorted.Sort();
            return $"{(int)subject}_{topicIndex}_{string.Join(",", sorted)}";
        }

        public void EnsureContext()
        {
            currentContextHash = BuildCurrentContextHash();
        }

        // ── Query (current context) ──────────────────────────────────────────

        /// <summary>Lấy sections của context hiện tại.</summary>
        public List<DataTitleTeachLevel3_3> GetCurrentSections()
        {
            return listDataTitleTeach.Where(x => x.contextHash == currentContextHash).ToList();
        }

        /// <summary>Check section index đã có content trong context hiện tại chưa.</summary>
        public bool HasSection(int index)
        {
            return listDataTitleTeach.Any(x => x.contextHash == currentContextHash && x.index == index);
        }

        /// <summary>Tìm section index đầu tiên chưa có content trong context hiện tại.</summary>
        public int GetFirstIncompleteIndex(int totalSections)
        {
            for (int i = 0; i < totalSections; i++)
                if (!HasSection(i)) return i;
            return totalSections;
        }

        // ── Write ────────────────────────────────────────────────────────────

        /// <summary>Set content cho section index trong context hiện tại. Update nếu đã có.</summary>
        public void SetSection(int index, string content)
        {
            var existing = listDataTitleTeach.Find(x => x.contextHash == currentContextHash && x.index == index);
            if (existing != null)
            {
                existing.content = content;
            }
            else
            {
                listDataTitleTeach.Add(new DataTitleTeachLevel3_3
                {
                    index = index,
                    content = content,
                    contextHash = currentContextHash,
                });
            }
        }

        // ── AI count ─────────────────────────────────────────────────────────

        public int GetAllSendAI()
        {
            return listConverstationState.Sum(x => x.listConverstationData.Count(y => y.role == ERole.Me));
        }

        // ── Summary (dùng cho Scene 3_6 và export) ───────────────────────────

        public string GetTitleSummary() => !string.IsNullOrEmpty(Topic) ? Topic : Loc.K("level3.lesson_title");

        public string GetObjectiveSummary() => LearningObjective;

        public string GetActivitiesSummary()
        {
            var sections = GetCurrentSections();
            StringBuilder sb = new StringBuilder();
            foreach (var s in sections)
            {
                string name = LessonPlanTemplate.GetSectionName(s.index);
                sb.AppendLine($"<b>{name}:</b> {s.content}\n");
            }
            return sb.ToString();
        }

        public string GetAssessmentSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>{Loc.K("level3.student_work")}:</b>\n{studentWork}\n");
            sb.AppendLine($"<b>{Loc.K("level3.student_feedback")}:</b>");
            if (listFeedbackSuggestions == null || listFeedbackSuggestions.Count == 0)
                sb.AppendLine($"({Loc.K("level3.feedback_summary")})");
            else
                foreach (var f in listFeedbackSuggestions)
                    sb.AppendLine($"- {f.text}");
            return sb.ToString();
        }

        /// <summary>Full content của context hiện tại — dùng cho AI prompt.</summary>
        public string GetStringFullContent()
        {
            var sections = GetCurrentSections();
            if (sections.Count == 0) return "";
            StringBuilder sb = new StringBuilder();
            foreach (var s in sections)
                sb.AppendLine($"{LessonPlanTemplate.GetSectionName(s.index)}: {s.content}");
            return sb.ToString();
        }

        // ── Filters ──────────────────────────────────────────────────────────

        public void AddFeedbackSuggestion(FeedbackSuggestion suggestion)
        {
            var item = listFeedbackSuggestions.Find(x => x.text == suggestion.text && x.type == suggestion.type);
            if (item == null)
                listFeedbackSuggestions.Add(suggestion);
            else
            {
                item.text = suggestion.text;
                item.type = suggestion.type;
            }
        }

        public void AddFilter(int filterIndex)
        {
            if (filterIndices == null) filterIndices = new List<int>();
            if (!filterIndices.Contains(filterIndex))
                filterIndices.Add(filterIndex);
        }

        public void RemoveFilter(int filterIndex)
        {
            if (filterIndices != null)
                filterIndices.Remove(filterIndex);
        }

        public bool HasFilter(int filterIndex)
        {
            return filterIndices != null && filterIndices.Contains(filterIndex);
        }

        public List<ConversationState> listConverstationState = new();
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
