namespace Luzart
{
    /// <summary>
    /// 7 section cố định của lesson plan — GDD Level 3 Scene 3.
    /// Dùng index (0-6) trong runtime, map sang tên khi export.
    /// </summary>
    public static class LessonPlanTemplate
    {
        public static readonly string[] SectionKeys =
        {
            "level3.lesson_title",               // 0
            "level3.learning_objective",         // 1
            "level3.intro_activity",             // 2
            "level3.main_activity",              // 3
            "level3.assessment_reflection",      // 4
            "level3.differentiation_notes",      // 5
            "level3.materials",                  // 6
        };

        public static int SectionCount => SectionKeys.Length; // 7

        public static string GetSectionName(int index)
        {
            if (index >= 0 && index < SectionKeys.Length)
                return Loc.K(SectionKeys[index]);
            return $"Section {index}";
        }

        // Context hash giờ nằm trong Level3Data.BuildCurrentContextHash() — dùng số hoàn toàn
    }
}
