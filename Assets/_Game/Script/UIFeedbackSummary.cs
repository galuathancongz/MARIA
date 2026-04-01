using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Luzart
{
    // ══════════════════════════════════════════════════════════════════════════
    //  UIFeedbackSummary
    //  Popup tổng hợp toàn bộ reflection + feedback của player qua mọi level.
    //
    //  Data sources:
    //    Level 1 Scene 5 — PersonaData.reflections (predefined + custom)
    //    Level 2 Scene 5 — Level2Data.reflections (predefined + custom)
    //    Level 3 Scene 5 — Level3Data.listFeedbackSuggestions (strength/improvement/next step)
    //    Level 3 Scene 6 — Level3Data.listFeedbackSelected (reflection write-in)
    //    Level 3 Scene 7 — Level3Data.personalisedFeedback (AI mentor)
    //
    //  Inspector wiring:
    //    itemPrefab     — FeedbackItem prefab
    //    contentParent  — ScrollView → Viewport → Content
    // ══════════════════════════════════════════════════════════════════════════
    public class UIFeedbackSummary : UIBase
    {
        [Header("Feedback List")]
        public FeedbackItem itemPrefab;
        public Transform contentParent;

        private List<FeedbackItem> _items = new List<FeedbackItem>();

        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);
            BuildFeedbackList();
        }

        private void BuildFeedbackList()
        {
            var entries = CollectAllFeedback();

            MasterHelper.InitListObj(entries.Count, itemPrefab, _items, contentParent,
                (item, i) =>
                {
                    item.gameObject.SetActive(true);
                    item.Setup(entries[i].title, entries[i].content);
                });
        }

        private List<FeedbackEntry> CollectAllFeedback()
        {
            var list = new List<FeedbackEntry>();

            // ── Level 1 Scene 5: Reflection ──────────────────────────────────
            var persona = PersonaManager.Instance?.Data;
            if (persona != null && persona.reflections != null && persona.reflections.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var r in persona.reflections)
                    sb.AppendLine($"- {r}");
                list.Add(new FeedbackEntry
                {
                    title = "Level 1 - Reflection",
                    content = sb.ToString().TrimEnd()
                });
            }

            // ── Level 2 Scene 5: Reflection ──────────────────────────────────
            var l2 = Level2Manager.Instance?.Data;
            if (l2 != null && l2.reflections != null && l2.reflections.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var r in l2.reflections)
                    sb.AppendLine($"- {r}");
                list.Add(new FeedbackEntry
                {
                    title = "Level 2 - Reflection",
                    content = sb.ToString().TrimEnd()
                });
            }

            // ── Level 3 Scene 5: Your Feedback to Student ────────────────────
            var l3 = Level3Manager.Instance?.Data;
            if (l3 != null)
            {
                if (l3.listFeedbackSuggestions != null && l3.listFeedbackSuggestions.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var f in l3.listFeedbackSuggestions)
                    {
                        string typeName = f.type == EFeedback.Strength ? "Strength"
                            : f.type == EFeedback.Improvement ? "Improvement"
                            : "Next Step";
                        sb.AppendLine($"- [{typeName}] {f.text}");
                    }
                    list.Add(new FeedbackEntry
                    {
                        title = "Level 3 - Feedback to Student",
                        content = sb.ToString().TrimEnd()
                    });
                }

                // ── Level 3 Scene 6: Reflection ──────────────────────────────
                if (l3.listFeedbackSelected != null && l3.listFeedbackSelected.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var r in l3.listFeedbackSelected)
                        sb.AppendLine($"- {r}");
                    list.Add(new FeedbackEntry
                    {
                        title = "Level 3 - Reflection",
                        content = sb.ToString().TrimEnd()
                    });
                }

                // ── Level 3 Scene 7: AI Personalised Feedback ────────────────
                if (!string.IsNullOrEmpty(l3.personalisedFeedback))
                {
                    list.Add(new FeedbackEntry
                    {
                        title = "Level 3 - AI Mentor Feedback",
                        content = l3.personalisedFeedback
                    });
                }
            }

            if (list.Count == 0)
            {
                list.Add(new FeedbackEntry
                {
                    title = "No feedback yet",
                    content = "Complete more levels to see your reflections and feedback here."
                });
            }

            return list;
        }

        private struct FeedbackEntry
        {
            public string title;
            public string content;
        }
    }
}
