using TMPro;
using UnityEngine;

namespace Luzart
{
    // ══════════════════════════════════════════════════════════════════════════
    //  FeedbackItem — 1 card trong ScrollView của UIFeedbackSummary
    //
    //  Inspector wiring:
    //    txtTitle   — tiêu đề (VD "Level 3 - Student Feedback")
    //    txtContent — nội dung feedback
    // ══════════════════════════════════════════════════════════════════════════
    public class FeedbackItem : MonoBehaviour
    {
        public TMP_Text txtTitle;
        public TMP_InputField txtContent;

        public void Setup(string title, string content)
        {
            if (txtTitle) txtTitle.text = title;
            if (txtContent) txtContent.text = content;
        }
    }
}
