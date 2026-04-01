using TMPro;
using UnityEngine;

namespace Luzart
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ReflectionToggleItem
    //  Gắn lên mỗi toggle item trong reflection choices.
    //  ReflectionCollector sẽ đọc IsOn + Text khi Send.
    //
    //  Inspector wiring:
    //    toggle  — BaseToggle component trên item
    //    txtLabel — TMP_Text chứa nội dung reflection (VD "I hadn't realised...")
    // ══════════════════════════════════════════════════════════════════════════
    public class ReflectionToggleItem : MonoBehaviour
    {
        public BaseToggle toggle;
        public TMP_Text txtLabel;

        public bool IsOn => toggle != null && toggle.IsSelect;
        public string Text => txtLabel != null ? txtLabel.text : "";

        [ContextMenu("Auto Set")]
        private void AutoSet()
        {
            if (toggle == null) toggle = GetComponentInChildren<BaseToggle>();
            if (txtLabel == null) txtLabel = GetComponentInChildren<TMP_Text>();
        }
    }
}
