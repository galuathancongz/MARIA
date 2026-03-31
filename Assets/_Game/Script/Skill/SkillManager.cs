namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;

    // ══════════════════════════════════════════════════════════════════════════
    //  Save model — serialised to PlayerPrefs and synced to server as skillsJson
    // ══════════════════════════════════════════════════════════════════════════
    [System.Serializable]
    public class SkillSaveData
    {
        public List<int> unlocked = new List<int>();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  SkillManager
    //  - Singleton (no scene dependency)
    //  - Persists to PlayerPrefs key "key_skills" as JSON
    //  - Server-synced via SyncManager → skillsJson
    //  - Fires ObserverKey.OnSkillUnlocked after each new unlock
    // ══════════════════════════════════════════════════════════════════════════
    public class SkillManager : Singleton<SkillManager>
    {
        private const string PREFS_KEY = "key_skills";
        private SkillSaveData _data;

        private SkillSaveData Data
        {
            get
            {
                if (_data == null) Load();
                return _data;
            }
        }

        // ── Unlock ──────────────────────────────────────────────────────────

        /// <summary>Unlock a single skill. Safe to call multiple times.</summary>
        public void UnlockSkill(ESkillId id)
        {
            int intId = (int)id;
            if (Data.unlocked.Contains(intId)) return;

            Data.unlocked.Add(intId);
            Save();
            Debug.Log($"[SkillManager] Unlocked: {id}");

            // Notify other systems (e.g. UIProfile live-refresh)
            Observer.Instance?.Notify(ObserverKey.OnSkillUnlocked, id);

            // Show badge popup
            if (UIManager.Instance != null)
            {
                var popup = UIManager.Instance.ShowUI<UIBadgeUnlock>(UIName.BadgeUnlock, isNeedCheck: false);
                popup?.Enqueue(id);
            }
        }

        /// <summary>
        /// Unlock all skills tied to a game level.
        /// Call at level-completion summary screen.
        /// Level 1 → persona type only (call UnlockPersona separately).
        /// Level 2 → all 4 AI tools.
        /// Level 3 → badges are unlocked individually during gameplay.
        /// </summary>
        public void UnlockSkillsForLevel(int level)
        {
            foreach (var info in SkillDefinition.ForLevel(level))
                UnlockSkill(info.id);
        }

        /// <summary>
        /// Unlock the persona skill matching the player's dominant persona.
        /// Call after Level 1 completion.
        /// </summary>
        public void UnlockPersonaSkill()
        {
            if (PersonaManager.Instance == null) return;
            var persona = PersonaManager.Instance.GetMyPersonaType();
            ESkillId skillId = persona switch
            {
                EPersonaType.Creative       => ESkillId.PersonaCreative,
                EPersonaType.LogicOrStruct  => ESkillId.PersonaLogical,
                EPersonaType.Empathy        => ESkillId.PersonaEmpathic,
                _                           => ESkillId.PersonaStructured,
            };
            UnlockSkill(skillId);
        }

        // ── Query ────────────────────────────────────────────────────────────

        public bool HasSkill(ESkillId id) => Data.unlocked.Contains((int)id);

        public List<ESkillId> GetUnlockedForLevel(int level)
        {
            var result = new List<ESkillId>();
            foreach (var info in SkillDefinition.ForLevel(level))
                if (HasSkill(info.id)) result.Add(info.id);
            return result;
        }

        public int CountForLevel(int level) => GetUnlockedForLevel(level).Count;
        public int TotalForLevel(int level) => SkillDefinition.TotalForLevel(level);

        public int CountAll()
        {
            int n = 0;
            for (int lvl = 0; lvl <= 3; lvl++) n += CountForLevel(lvl); // lvl 0 = Tutorial
            return n;
        }
        public int TotalAll() => SkillDefinition.Total; // 20 (3 Tutorial + 6 L1 + 4 L2 + 7 L3)

        // ── Persistence ──────────────────────────────────────────────────────

        public void Save()
        {
            PlayerPrefs.SetString(PREFS_KEY, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        public void Load()
        {
            string json = PlayerPrefs.GetString(PREFS_KEY, "");
            _data = !string.IsNullOrEmpty(json)
                ? JsonUtility.FromJson<SkillSaveData>(json) ?? new SkillSaveData()
                : new SkillSaveData();
        }

        // ── Server sync helpers (used by SyncManager) ────────────────────────

        public string ToJson() => JsonUtility.ToJson(Data);

        public void FromJson(string json)
        {
            if (string.IsNullOrEmpty(json) || json == "{}") return;
            var loaded = JsonUtility.FromJson<SkillSaveData>(json);
            if (loaded?.unlocked == null) return;
            _data = loaded;
            Save();
        }
    }
}
