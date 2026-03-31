namespace Luzart
{
    using System.Linq;
    using UnityEngine;

    // ══════════════════════════════════════════════════════════════════════════
    //  AnalyticsManager
    //  Computes an AnalyticsData snapshot from the current state of all
    //  game managers.  Called by SyncManager just before each server save.
    //
    //  No additional persistent state — all values are derived from
    //  Level2Manager, Level3Manager, Level4Manager, SkillManager, and
    //  PersonaManager which are already persisted independently.
    // ══════════════════════════════════════════════════════════════════════════
    public class AnalyticsManager : Singleton<AnalyticsManager>
    {
        /// <summary>
        /// Build a fresh AnalyticsData snapshot from the current state of all managers.
        /// </summary>
        public AnalyticsData Build()
        {
            var a = new AnalyticsData();

            // ── AI send counts ────────────────────────────────────────────────
            if (Level2Manager.Instance?.Data != null)
            {
                a.aiSendCountLevel2 = Level2Manager.Instance.Data.listConverstationState
                    .Sum(x => x.listConverstationData.Count(y => y.role == ERole.Me));
            }

            if (Level3Manager.Instance?.Data != null)
            {
                var d3 = Level3Manager.Instance.Data;
                a.aiSendCountLevel3  = d3.GetAllSendAI();
                a.totalRefineCount   = d3.totalRefineCount;

                if (d3.optionalFilters != null)
                {
                    a.optionalFiltersUsed = d3.optionalFilters.Count;
                    a.optionalFilters     = string.Join(", ", d3.optionalFilters);
                }
            }

            // ── GenAI competency indicators (from badge unlocks) ──────────────
            if (SkillManager.Instance != null)
            {
                a.c1_firstAIPrompt     = SkillManager.Instance.HasSkill(ESkillId.FirstAIPrompt);
                a.c2_lessonCoCreator   = SkillManager.Instance.HasSkill(ESkillId.LessonCoCreator);
                a.c3_inclusivePlanner  = SkillManager.Instance.HasSkill(ESkillId.InclusivePlanner);
                a.c4_feedbackArchitect = SkillManager.Instance.HasSkill(ESkillId.FeedbackArchitect);
                a.c5_iterationChampion = SkillManager.Instance.HasSkill(ESkillId.IterationChampion);
            }

            // ── Context ───────────────────────────────────────────────────────
            if (PersonaManager.Instance != null)
                a.personaType = PersonaManager.Instance.GetMyPersonaType().ToString();

            if (Level4Manager.Instance?.Data != null)
                a.quizAnswersCount = Level4Manager.Instance.Data.listQuestion.Count;

            return a;
        }
    }
}
