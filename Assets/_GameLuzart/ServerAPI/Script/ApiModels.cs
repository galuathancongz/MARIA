namespace Luzart
{
    [System.Serializable]
    public class ApiResponse
    {
        public bool success;
        public string message;
    }

    // ========== AUTH ==========
    [System.Serializable]
    public class AuthRequest
    {
        public string username;
        public string password;
        public string email;
    }

    [System.Serializable]
    public class AuthResponse
    {
        public bool success;
        public string message;
        public string token;
        public int userId;
        public string username;
    }

    // ========== GAME DATA ==========
    [System.Serializable]
    public class GameDataSaveRequest
    {
        public int level;
        public string namePlayer;
        public int age;
        public int subject;
        public string subjectName;
        public string personaJson;   // PersonaData JSON (Level 1 persona + reflections)
        public string level2Json;
        public string level3Json;
        public string settingsJson;
        public string skillsJson;   // SkillSaveData JSON
        public string level4Json;   // Level4Data JSON (quiz answers)
        public string analyticsJson; // AnalyticsData JSON (derived metrics)
    }

    [System.Serializable]
    public class GameDataLoadResponse
    {
        public bool success;
        public string message;
        public GameDataPayload data;
    }

    [System.Serializable]
    public class GameDataPayload
    {
        public int level;
        public string namePlayer;
        public int age;
        public int subject;
        public string subjectName;
        public string personaJson;   // PersonaData JSON
        public string level2Json;
        public string level3Json;
        public string settingsJson;
        public string skillsJson;   // SkillSaveData JSON
        public string level4Json;   // Level4Data JSON (quiz answers)
        public string analyticsJson; // AnalyticsData JSON (derived metrics)
        public string updatedAt;
    }

    // ========== ANALYTICS (computed snapshot, not stored locally) ==========
    [System.Serializable]
    public class AnalyticsData
    {
        // AI usage
        public int aiSendCountLevel2;      // total user prompts in Level 2
        public int aiSendCountLevel3;      // total user prompts in Level 3

        // Revision
        public int totalRefineCount;       // total refine calls across all Level 3 sections

        // Inclusivity / differentiation
        public int optionalFiltersUsed;    // number of optional filters applied
        public string optionalFilters;     // comma-separated filter names

        // GenAI competency indicators (derived from badge unlocks)
        public bool c1_firstAIPrompt;      // C1: prompted AI (FirstAIPrompt badge)
        public bool c2_lessonCoCreator;    // C2: built lesson with AI (LessonCoCreator)
        public bool c3_inclusivePlanner;   // C3: used differentiation (InclusivePlanner)
        public bool c4_feedbackArchitect;  // C4: gave structured feedback (FeedbackArchitect)
        public bool c5_iterationChampion;  // C5: refined output 3+ times (IterationChampion)

        // Context
        public string personaType;         // player's dominant persona
        public int quizAnswersCount;       // number of quiz questions answered
    }

    [System.Serializable]
    public class SettingsData
    {
        public float sfxVolume = 1f;
        public float musicVolume = 1f;
        public int muteVibra = 0;
    }
}
