namespace Luzart
{
    /// <summary>
    /// Bảng cố định topic + learning objective + design constraint theo GDD Level 3 Scene 2.
    /// Runtime lưu (subjectIndex, topicIndex). Lấy text khi cần hiển thị hoặc export.
    /// </summary>
    [System.Serializable]
    public class DesignChallenge
    {
        public string topicKey;
        public string objectiveKey;
        public string constraintKey;
    }

    public static class DesignChallengeTable
    {
        // Index = ESubject: 0=English, 1=Math, 2=History, 3=Science
        // Mỗi subject có 2-3 topics
        // Lưu localization key, dùng Loc.K() khi cần hiển thị
        public static readonly DesignChallenge[][] Challenges =
        {
            // ── Subject 0: English (Austen) ──────────────────────────────────
            new[]
            {
                new DesignChallenge
                {
                    topicKey = "level3.topic_narrative",
                    objectiveKey = "level3.constraint_perspective",
                    constraintKey = "level3.constraint_voice_prompts",
                },
                new DesignChallenge
                {
                    topicKey = "level3.topic_persuasive",
                    objectiveKey = "level3.constraint_arguments",
                    constraintKey = "level3.constraint_debate",
                },
            },

            // ── Subject 1: Math (Euclidea) ───────────────────────────────────
            new[]
            {
                new DesignChallenge
                {
                    topicKey = "level3.topic_fractions",
                    objectiveKey = "level3.constraint_visual_real",
                    constraintKey = "level3.constraint_common_materials",
                },
                new DesignChallenge
                {
                    topicKey = "level3.topic_algebra",
                    objectiveKey = "level3.constraint_expressions",
                    constraintKey = "level3.constraint_ai_guided",
                },
                new DesignChallenge
                {
                    topicKey = "level3.topic_measurement",
                    objectiveKey = "level3.constraint_perimeter",
                    constraintKey = "level3.constraint_movement",
                },
            },

            // ── Subject 2: History (Thucy) ───────────────────────────────────
            new[]
            {
                new DesignChallenge
                {
                    topicKey = "level3.topic_ancient_civ",
                    objectiveKey = "level3.constraint_compare_civ",
                    constraintKey = "level3.constraint_collaborative",
                },
                new DesignChallenge
                {
                    topicKey = "level3.topic_colonialism",
                    objectiveKey = "level3.constraint_cultures",
                    constraintKey = "level3.constraint_student_voice",
                },
                new DesignChallenge
                {
                    topicKey = "level3.timeline_construction",
                    objectiveKey = "level3.constraint_sequence",
                    constraintKey = "level3.constraint_scaffolds",
                },
            },

            // ── Subject 3: Science (Darwinia) ────────────────────────────────
            new[]
            {
                new DesignChallenge
                {
                    topicKey = "level3.topic_states_matter",
                    objectiveKey = "level3.constraint_materials_form",
                    constraintKey = "level3.constraint_low_cost",
                },
                new DesignChallenge
                {
                    topicKey = "level3.topic_ecosystems",
                    objectiveKey = "level3.constraint_food_chains",
                    constraintKey = "level3.constraint_storytelling",
                },
                new DesignChallenge
                {
                    topicKey = "level3.topic_scientific_method",
                    objectiveKey = "level3.constraint_prediction",
                    constraintKey = "level3.constraint_literacy",
                },
            },
        };

        /// <summary>Lấy DesignChallenge theo subject + topicIndex. Null nếu out of range.</summary>
        public static DesignChallenge Get(int subjectIndex, int topicIndex)
        {
            if (subjectIndex < 0 || subjectIndex >= Challenges.Length) return null;
            var topics = Challenges[subjectIndex];
            if (topicIndex < 0 || topicIndex >= topics.Length) return null;
            return topics[topicIndex];
        }

        public static DesignChallenge Get(ESubject subject, int topicIndex)
        {
            return Get((int)subject, topicIndex);
        }

        public static int TopicCount(int subjectIndex)
        {
            if (subjectIndex < 0 || subjectIndex >= Challenges.Length) return 0;
            return Challenges[subjectIndex].Length;
        }
    }

    /// <summary>
    /// 4 optional filters cố định — GDD Level 3 Scene 2.
    /// Runtime lưu List&lt;int&gt; filterIndices. Lấy text khi cần.
    /// </summary>
    public static class FilterTable
    {
        public static readonly string[] Keys =
        {
            "level3.differentiation_required",    // 0
            "level3.filter_time_constrained",     // 1
            "level3.accessibility_support",       // 2
            "level3.filter_multilingual",         // 3
        };

        public static int Count => Keys.Length; // 4

        public static string GetName(int index)
        {
            if (index >= 0 && index < Keys.Length) return Loc.K(Keys[index]);
            return $"Filter {index}";
        }
    }
}
