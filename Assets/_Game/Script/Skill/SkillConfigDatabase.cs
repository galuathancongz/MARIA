using System.Collections.Generic;
using UnityEngine;

namespace Luzart
{
    // ══════════════════════════════════════════════════════════════════════════
    //  SkillConfigEntry — per-badge visual + notification settings
    //  Configured entirely in the Inspector on the SkillConfigDatabase asset.
    // ══════════════════════════════════════════════════════════════════════════
    [System.Serializable]
    public class SkillConfigEntry
    {
        [Header("Identity")]
        public ESkillId skillId;

        [Header("Visual")]
        [Tooltip("Badge icon sprite (optional — falls back to emoji if null)")]
        public Sprite icon;

        [Tooltip("Background colour for this badge card / popup")]
        public Color  badgeColor = new Color(0.22f, 0.82f, 0.9f, 1f);   // cyan default

        [Header("Popup Notification")]
        [Tooltip("Show an unlock popup when this badge is earned.\n" +
                 "Uncheck for badges that are silently tracked " +
                 "(e.g. persona type, tutorial completion).")]
        public bool showPopupOnUnlock = true;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  SkillConfigDatabase — single ScriptableObject with all 20 badge entries
    //
    //  How to create:
    //    Assets → Create → Luzart / Skill / Config Database
    //
    //  For Resources.Load to work, save the asset inside any folder named
    //  "Resources" and name the file exactly "SkillConfigDatabase".
    //  Example path:  Assets/_Game/Resources/SkillConfigDatabase.asset
    // ══════════════════════════════════════════════════════════════════════════
    [CreateAssetMenu(
        fileName = "SkillConfigDatabase",
        menuName  = "Luzart/Skill/Config Database",
        order     = 0)]
    public class SkillConfigDatabase : ScriptableObject
    {
        [SerializeField]
        private List<SkillConfigEntry> _entries = new List<SkillConfigEntry>();

        // ── Lookup ────────────────────────────────────────────────────────────

        /// <summary>Returns the config for a given skill ID, or null if not found.</summary>
        public SkillConfigEntry Get(ESkillId id)
        {
            foreach (var e in _entries)
                if (e.skillId == id) return e;
            return null;
        }

        /// <summary>All entries (read-only).</summary>
        public IReadOnlyList<SkillConfigEntry> Entries => _entries;

        // ── Resources auto-loader ─────────────────────────────────────────────

        private static SkillConfigDatabase _instance;

        /// <summary>
        /// Loads the database from Resources/SkillConfigDatabase.asset.
        /// Returns null if the asset has not been created yet.
        /// </summary>
        public static SkillConfigDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<SkillConfigDatabase>("SkillConfigDatabase");
                return _instance;
            }
        }
    }
}
