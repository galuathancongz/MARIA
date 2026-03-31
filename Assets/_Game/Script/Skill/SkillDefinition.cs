namespace Luzart
{
    // ══════════════════════════════════════════════════════════════════════════
    //  Skill / Badge IDs — derived from GDD (all 4 documents)
    //
    //  0xx = Tutorial
    //  1xx = Level 1 (Teaching Persona)
    //  2xx = Level 2 (AI Ideation Lab)
    //  3xx = Level 3 (Co-Creation Studio)
    // ══════════════════════════════════════════════════════════════════════════
    public enum ESkillId
    {
        // ── Tutorial ──────────────────────────────────────────────────────────
        TutorialComplete = 001,   // Finished the full tutorial
        FirstAIPrompt    = 002,   // Sent first AI prompt in Scene 3
        QuizAce          = 003,   // Answered both quiz questions correctly

        // ── Level 1: Teaching Persona ─────────────────────────────────────────
        // Persona reveal (exactly ONE unlocks based on dominant score)
        PersonaCreative   = 101,
        PersonaLogical    = 102,
        PersonaEmpathic   = 103,
        PersonaStructured = 104,
        // Activity badges (unlock during Level 1 scenes)
        ReflectionJournal = 105,  // Completed Scene 5 reflection  (GDD: "Reflection toolkit")
        PersonalTouch     = 106,  // Customised all 3 avatar elements in Scene 2 (GDD: "Add your own touch")

        // ── Level 2: AI Ideation Lab ──────────────────────────────────────────
        // All 4 unlock on Level 2 completion
        AIMentorTools            = 201,
        CreativeIdeaGenerator    = 202,
        InquiryBasedLearning     = 203,
        TeachbackWithAI          = 204,

        // ── Level 3: Co-Creation Studio ───────────────────────────────────────
        // Scene 3 — Iteration rewards
        LessonCoCreator        = 301,   // Used AI suggest 5+ times
        IterationChampion      = 302,   // Refined one section 3+ times
        InclusivePlanner       = 303,   // Added a differentiation strategy
        PersonaAligned         = 304,   // Lesson matched Level 1 teaching persona
        // Scene 5 — Feedback mini-badges
        FeedbackArchitect      = 305,   // Gave strength feedback
        SeekingImprovement     = 306,   // Gave area-of-improvement feedback
        ForwardLookingDesigner = 307,   // Gave concrete next-step feedback
    }

    [System.Serializable]
    public class SkillInfo
    {
        public ESkillId id;
        public int      forLevel;   // 0=Tutorial, 1=Level1, 2=Level2, 3=Level3
        public string   nameKey;    // localization key
        public string   descKey;    // localization key
        public string   emoji;      // emoji icon (art-independent)
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Static skill table — single source of truth
    //  Total: 3 + 6 + 4 + 7 = 20 badges
    // ══════════════════════════════════════════════════════════════════════════
    public static class SkillDefinition
    {
        public static readonly SkillInfo[] All =
        {
            // ── Tutorial (3 badges) ───────────────────────────────────────────
            new SkillInfo { id = ESkillId.TutorialComplete, forLevel = 0,
                nameKey = "skill.tutorial_complete", descKey = "skill.tutorial_complete_desc", emoji = "🎓" },
            new SkillInfo { id = ESkillId.FirstAIPrompt, forLevel = 0,
                nameKey = "skill.first_ai_prompt",   descKey = "skill.first_ai_prompt_desc",   emoji = "💬" },
            new SkillInfo { id = ESkillId.QuizAce, forLevel = 0,
                nameKey = "skill.quiz_ace",          descKey = "skill.quiz_ace_desc",          emoji = "✅" },

            // ── Level 1 (6 badges: 4 persona + 2 activity) ───────────────────
            new SkillInfo { id = ESkillId.PersonaCreative,   forLevel = 1,
                nameKey = "skill.persona_creative",   descKey = "skill.persona_creative_desc",   emoji = "🎨" },
            new SkillInfo { id = ESkillId.PersonaLogical,    forLevel = 1,
                nameKey = "skill.persona_logical",    descKey = "skill.persona_logical_desc",    emoji = "🧠" },
            new SkillInfo { id = ESkillId.PersonaEmpathic,   forLevel = 1,
                nameKey = "skill.persona_empathic",   descKey = "skill.persona_empathic_desc",   emoji = "💛" },
            new SkillInfo { id = ESkillId.PersonaStructured, forLevel = 1,
                nameKey = "skill.persona_structured", descKey = "skill.persona_structured_desc", emoji = "📋" },
            new SkillInfo { id = ESkillId.ReflectionJournal, forLevel = 1,
                nameKey = "skill.reflection_journal", descKey = "skill.reflection_journal_desc", emoji = "📓" },
            new SkillInfo { id = ESkillId.PersonalTouch,     forLevel = 1,
                nameKey = "skill.personal_touch",     descKey = "skill.personal_touch_desc",     emoji = "✨" },

            // ── Level 2 (4 badges — all unlock on completion) ────────────────
            new SkillInfo { id = ESkillId.AIMentorTools,         forLevel = 2,
                nameKey = "skill.ai_mentor_tools",         descKey = "skill.ai_mentor_tools_desc",         emoji = "🤖" },
            new SkillInfo { id = ESkillId.CreativeIdeaGenerator, forLevel = 2,
                nameKey = "skill.creative_idea_generator", descKey = "skill.creative_idea_generator_desc", emoji = "💡" },
            new SkillInfo { id = ESkillId.InquiryBasedLearning,  forLevel = 2,
                nameKey = "skill.inquiry_based_learning",  descKey = "skill.inquiry_based_learning_desc",  emoji = "🔍" },
            new SkillInfo { id = ESkillId.TeachbackWithAI,       forLevel = 2,
                nameKey = "skill.teachback_with_ai",       descKey = "skill.teachback_with_ai_desc",       emoji = "🔄" },

            // ── Level 3 (7 badges — earned individually during gameplay) ──────
            new SkillInfo { id = ESkillId.LessonCoCreator,        forLevel = 3,
                nameKey = "skill.lesson_cocreator",         descKey = "skill.lesson_cocreator_desc",         emoji = "📘" },
            new SkillInfo { id = ESkillId.IterationChampion,      forLevel = 3,
                nameKey = "skill.iteration_champion",        descKey = "skill.iteration_champion_desc",        emoji = "🔁" },
            new SkillInfo { id = ESkillId.InclusivePlanner,       forLevel = 3,
                nameKey = "skill.inclusive_planner",         descKey = "skill.inclusive_planner_desc",         emoji = "🌍" },
            new SkillInfo { id = ESkillId.PersonaAligned,         forLevel = 3,
                nameKey = "skill.persona_aligned",           descKey = "skill.persona_aligned_desc",           emoji = "🎯" },
            new SkillInfo { id = ESkillId.FeedbackArchitect,      forLevel = 3,
                nameKey = "skill.feedback_architect",         descKey = "skill.feedback_architect_desc",         emoji = "📝" },
            new SkillInfo { id = ESkillId.SeekingImprovement,     forLevel = 3,
                nameKey = "skill.seeking_improvement",        descKey = "skill.seeking_improvement_desc",        emoji = "🔧" },
            new SkillInfo { id = ESkillId.ForwardLookingDesigner, forLevel = 3,
                nameKey = "skill.forward_looking_designer",   descKey = "skill.forward_looking_designer_desc",   emoji = "🚀" },
        };

        /// <summary>Get SkillInfo by ID. Returns null if not found.</summary>
        public static SkillInfo Get(ESkillId id)
        {
            foreach (var s in All)
                if (s.id == id) return s;
            return null;
        }

        /// <summary>All skills for a given level (0=Tutorial, 1-3=Levels).</summary>
        public static SkillInfo[] ForLevel(int level)
            => System.Array.FindAll(All, s => s.forLevel == level);

        public static int TotalForLevel(int level) => ForLevel(level).Length;
        public static int Total => All.Length; // 20

        /// <summary>Returns list of ESkillId that the player has unlocked.</summary>
        public static System.Collections.Generic.List<ESkillId> GetUnlocked()
        {
            var list = new System.Collections.Generic.List<ESkillId>();
            if (SkillManager.Instance == null) return list;
            foreach (var s in All)
                if (SkillManager.Instance.HasSkill(s.id))
                    list.Add(s.id);
            return list;
        }
    }
}
