namespace Luzart
{
    /// <summary>
    /// 7 section cố định của lesson plan — GDD Level 3 Scene 3.
    /// Dùng index (0-6) trong runtime, map sang tên khi export.
    /// </summary>
    public static class LessonPlanTemplate
    {
        public static readonly string[] SectionNames =
        {
            "Lesson title",                      // 0
            "Learning objective",                // 1
            "Introduction starter activity",     // 2
            "Main learning activity",            // 3
            "Assessment / reflection",           // 4
            "Differentiation / inclusion notes", // 5
            "Materials / set up requirements",   // 6
        };

        public static int SectionCount => SectionNames.Length; // 7

        public static string GetSectionName(int index)
        {
            if (index >= 0 && index < SectionNames.Length)
                return SectionNames[index];
            return $"Section {index}";
        }

        // Context hash giờ nằm trong Level3Data.BuildCurrentContextHash() — dùng số hoàn toàn
    }
}
