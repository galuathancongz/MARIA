namespace Luzart
{
    using UnityEngine;

    public static class PdfExporter
    {
        private static string ExportUrl(string endpoint)
        {
            string token = PlayerPrefs.GetString("auth_token", "");
            string baseUrl = ApiClient.BASE_URL;
            return $"{baseUrl}/api/export/{endpoint}?token={token}";
        }

        /// <summary>
        /// Export lesson plan as PDF. Call directly from button onClick (not from async callback).
        /// </summary>
        public static void ExportLessonPlan()
        {
            Application.OpenURL(ExportUrl("lesson-plan"));
        }

        /// <summary>
        /// Export player growth report as PDF. Call directly from button onClick.
        /// </summary>
        public static void ExportGrowthReport()
        {
            Application.OpenURL(ExportUrl("growth-report"));
        }
    }
}
