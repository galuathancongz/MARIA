using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Luzart
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ReflectionCollector
    //  Gắn lên bất kỳ chỗ nào có reflection choices + text editor.
    //  Tự tìm toggles + input field, khi Send thì lưu vào đúng level data.
    //
    //  Inspector wiring:
    //    level          — 1, 2, hoặc 3 (xác định lưu vào manager nào)
    //    listToggles    — kéo các toggle items vào (mỗi toggle có text bên cạnh)
    //    inputField     — TMP_InputField "Write your feedback"
    //
    //  Gọi OnClickSend() từ nút Send trên prefab.
    // ══════════════════════════════════════════════════════════════════════════
    public class ReflectionCollector : MonoBehaviour
    {
        [Header("Config")]
        public int level = 1; // 1 = PersonaData, 2 = Level2Data, 3 = Level3Data

        [Header("References")]
        public List<ReflectionToggleItem> listToggles = new List<ReflectionToggleItem>();
        public TMP_InputField inputField;

        /// <summary>
        /// Gọi từ nút Send. Thu thập tất cả toggle đang ON + text tự viết → lưu vào manager.
        /// </summary>
        public void OnClickSend()
        {
            var reflections = Collect();
            if (reflections.Count == 0) return;

            List<string> target = null;
            switch (level)
            {
                case 1:
                    if (PersonaManager.Instance?.Data != null)
                        target = PersonaManager.Instance.Data.reflections;
                    break;
                case 2:
                    if (Level2Manager.Instance?.Data != null)
                        target = Level2Manager.Instance.Data.reflections;
                    break;
                case 3:
                    if (Level3Manager.Instance?.Data != null)
                        target = Level3Manager.Instance.Data.listFeedbackSelected;
                    break;
            }

            if (target != null)
            {
                int added = 0;
                foreach (var r in reflections)
                {
                    if (!target.Contains(r))
                    {
                        target.Add(r);
                        added++;
                    }
                }
                Debug.Log($"[ReflectionCollector] Level {level}: added {added} new, total {target.Count}");
            }
        }

        private List<string> Collect()
        {
            var result = new List<string>();

            // Thu thập từ toggles đang ON
            if (listToggles != null)
            {
                foreach (var item in listToggles)
                {
                    if (item != null && item.IsOn)
                        result.Add(item.Text);
                }
            }

            // Thu thập từ text editor (nếu có nội dung)
            if (inputField != null && !string.IsNullOrEmpty(inputField.text))
            {
                string custom = inputField.text.Trim();
                if (custom.Length > 0)
                    result.Add(custom);
            }

            return result;
        }
    }
}
