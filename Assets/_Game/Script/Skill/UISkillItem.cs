namespace Luzart
{
    using Luzart.NewBase;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    // ══════════════════════════════════════════════════════════════════════════
    //  UISkillItem
    //  A single skill / badge card shown inside UIProfile.
    //  Extends SelectToggle so locked / unlocked state is cached in _isSelect.
    //
    //  Wire in Inspector:
    //    txtEmoji   — large emoji (🎨 / 🤖 / 📘 …)
    //    txtName    — localized skill name
    //    txtDesc    — localized description (shows "???" when locked)
    //    imgBg      — card background Image
    //    imgBorder  — card border / outline Image
    //    objLock    — GameObject with lock icon (active when locked)
    //    objUnlock  — GameObject with star/check icon (active when unlocked)
    // ══════════════════════════════════════════════════════════════════════════
    public class UISkillItem : SelectToggle
    {
        [Header("Skill Card References")]
        public TMP_Text   txtEmoji;
        public TMP_Text   txtName;
        public TMP_Text   txtDesc;
        public Image      imgBg;
        public Image      imgBorder;
        public GameObject objLock;
        public GameObject objUnlock;

        // ── Palette ──────────────────────────────────────────────────────────
        private static readonly Color BgUnlocked     = new Color(0f,   0.80f, 1f,   0.15f);
        private static readonly Color BgLocked       = new Color(0.08f,0.10f, 0.14f,0.60f);
        private static readonly Color BorderUnlocked = new Color(0f,   0.80f, 1f,   0.80f);
        private static readonly Color BorderLocked   = new Color(0.25f,0.28f, 0.35f,0.40f);
        private static readonly Color TextUnlocked   = new Color(0f,   0.90f, 1f,   1f);
        private static readonly Color TextLocked     = new Color(0.40f,0.42f, 0.50f,1f);

        // ── Setup ─────────────────────────────────────────────────────────────
        /// <summary>Populate card from skill data and current unlock state.</summary>
        public void Setup(ESkillId skillId)
        {
            SkillInfo info     = SkillDefinition.Get(skillId);
            bool      unlocked = SkillManager.Instance != null && SkillManager.Instance.HasSkill(skillId);

            if (info == null) return;

            if (txtEmoji) txtEmoji.text = info.emoji;
            if (txtName)  txtName.text  = Loc.K(info.nameKey);
            if (txtDesc)  txtDesc.text  = unlocked ? Loc.K(info.descKey) : "???";

            Select(unlocked);
        }

        // ── Visual state ──────────────────────────────────────────────────────
        public override void Select(bool value)
        {
            base.Select(value); // caches _isSelect in SelectToggle

            if (imgBg)     imgBg.color     = value ? BgUnlocked     : BgLocked;
            if (imgBorder) imgBorder.color = value ? BorderUnlocked : BorderLocked;
            if (txtName)   txtName.color   = value ? TextUnlocked   : TextLocked;
            if (txtEmoji)  txtEmoji.color  = value ? TextUnlocked   : TextLocked;
            if (txtDesc)   txtDesc.color   = value ? TextUnlocked   : TextLocked;
            if (objLock)   objLock.SetActive(!value);
            if (objUnlock) objUnlock.SetActive(value);
        }
    }
}
