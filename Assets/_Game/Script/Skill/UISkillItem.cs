namespace Luzart
{
    using Luzart.NewBase;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    // ══════════════════════════════════════════════════════════════════════════
    //  UISkillItem
    //  A single badge card shown inside UIProfile's scroll list.
    //  Only unlocked badges are spawned, so no locked/unlocked state needed.
    //
    //  Wire in Inspector:
    //    imgIcon  — badge icon Image (sprite from SkillConfigDatabase)
    //    btnClick — Button component (for click action later)
    // ══════════════════════════════════════════════════════════════════════════
    public class UISkillItem : MonoBehaviour
    {
        public Image  imgIcon;
        public Button btnClick;

        private ESkillId _skillId;
        public ESkillId SkillId => _skillId;

        public void Setup(ESkillId skillId)
        {
            _skillId = skillId;

            if (imgIcon)
            {
                var config = SkillConfigDatabase.Instance?.Get(skillId);
                if (config != null && config.icon != null)
                    imgIcon.sprite = config.icon;
            }
        }
    }
}
