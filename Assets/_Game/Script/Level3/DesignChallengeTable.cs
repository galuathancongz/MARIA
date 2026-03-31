namespace Luzart
{
    /// <summary>
    /// Bảng cố định topic + learning objective + design constraint theo GDD Level 3 Scene 2.
    /// Runtime lưu (subjectIndex, topicIndex). Lấy text khi cần hiển thị hoặc export.
    /// </summary>
    [System.Serializable]
    public class DesignChallenge
    {
        public string topic;
        public string learningObjective;
        public string designConstraint;
    }

    public static class DesignChallengeTable
    {
        // Index = ESubject: 0=English, 1=Math, 2=History, 3=Science
        // Mỗi subject có 2-3 topics
        public static readonly DesignChallenge[][] Challenges =
        {
            // ── Subject 0: English (Austen) ──────────────────────────────────
            new[]
            {
                new DesignChallenge
                {
                    topic = "Narrative writing",
                    learningObjective = "Help students write from a different character's perspective",
                    designConstraint = "Must use visual or voice-based creative prompts",
                },
                new DesignChallenge
                {
                    topic = "Persuasive language",
                    learningObjective = "Teach students to structure arguments and counterpoints",
                    designConstraint = "Must include a debate or roleplay component",
                },
            },

            // ── Subject 1: Math (Euclidea) ───────────────────────────────────
            new[]
            {
                new DesignChallenge
                {
                    topic = "Fractions",
                    learningObjective = "Design a visual real world activity",
                    designConstraint = "Must use common materials (e.g. food, money, paper shapes)",
                },
                new DesignChallenge
                {
                    topic = "Algebraic thinking",
                    learningObjective = "Help students solve and simplify expressions",
                    designConstraint = "Must include step-by-step AI-guided practice",
                },
                new DesignChallenge
                {
                    topic = "Measurement",
                    learningObjective = "Compare perimeter and area of everyday objects",
                    designConstraint = "Must include a hands-on physical movement activity",
                },
            },

            // ── Subject 2: History (Thucy) ───────────────────────────────────
            new[]
            {
                new DesignChallenge
                {
                    topic = "Ancient civilisations",
                    learningObjective = "Compare cultural innovations between two civilisations",
                    designConstraint = "Must use collaborative group work or presentations",
                },
                new DesignChallenge
                {
                    topic = "Colonialism",
                    learningObjective = "Critically examine impact on local cultures",
                    designConstraint = "Must include student voice / reflection element",
                },
                new DesignChallenge
                {
                    topic = "Timeline construction",
                    learningObjective = "Sequence events in a historical period",
                    designConstraint = "Must support students with visual and linguistic scaffolds",
                },
            },

            // ── Subject 3: Science (Darwinia) ────────────────────────────────
            new[]
            {
                new DesignChallenge
                {
                    topic = "States of matter",
                    learningObjective = "Explore how materials change form",
                    designConstraint = "Must use low-cost materials for an experiment",
                },
                new DesignChallenge
                {
                    topic = "Ecosystems",
                    learningObjective = "Understand food chains and interdependence",
                    designConstraint = "Must involve a creative storytelling or role-play element",
                },
                new DesignChallenge
                {
                    topic = "Scientific method",
                    learningObjective = "Teach prediction, testing and reflection",
                    designConstraint = "Must be adaptable for learners with different literacy levels",
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
        public static readonly string[] Names =
        {
            "Differentiation required",           // 0
            "Time-constrained lesson",            // 1
            "Accessibility support",              // 2
            "Multilingual classroom",             // 3
        };

        public static int Count => Names.Length; // 4

        public static string GetName(int index)
        {
            if (index >= 0 && index < Names.Length) return Names[index];
            return $"Filter {index}";
        }
    }
}
