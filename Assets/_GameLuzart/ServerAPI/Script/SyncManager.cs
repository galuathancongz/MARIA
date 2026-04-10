namespace Luzart
{
    using System;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// SyncManager — MARIA server sync
    ///
    /// STRATEGY (Hybrid Auto-Sync):
    /// 1. Polling hash    — every 5s, hash all syncable data and compare with last synced hash.
    ///                      Detects changes WITHOUT requiring MarkDirty() calls from other code.
    /// 2. Debounce        — after data changes, wait 3s of "quiet" before syncing.
    ///                      Batches many rapid changes into one sync.
    /// 3. Periodic force  — if data has been dirty for 30s (user keeps changing),
    ///                      force a sync anyway so we don't wait forever.
    /// 4. Min throttle    — never sync more than once per 10s.
    /// 5. Failure backoff — after a failure, wait 30s before retrying.
    ///
    /// External code can still call SaveToServer() directly for immediate saves
    /// (e.g., "level_complete", "focus_loss", "logout") and MarkDirty() as a hint.
    /// </summary>
    public class SyncManager : Singleton<SyncManager>
    {
        // ── Config ────────────────────────────────────────────────────────
        [Header("Auto-Sync Config")]
        [SerializeField] private bool autoSyncEnabled = true;
        [SerializeField] private float pollInterval       = 5f;   // check data hash every 5s
        [SerializeField] private float debounceSeconds    = 3f;   // wait N seconds of stable data
        [SerializeField] private float periodicForceSec   = 30f;  // force sync if dirty this long
        [SerializeField] private float minSyncInterval    = 10f;  // min gap between syncs
        [SerializeField] private float failureBackoff     = 30f;  // wait after a failure

        // ── Runtime state ─────────────────────────────────────────────────
        private bool   isSyncing = false;
        private string lastSyncedHash   = "";
        private string lastKnownHash    = "";
        private float  lastPollTime     = -999f;
        private float  lastSyncTime     = -999f;
        private float  lastChangeTime   = -999f;
        private float  lastFailureTime  = -999f;
        private bool   markedDirty      = false;
        // Must stay false until LoadFromServer has completed at least once for the
        // current login. Prevents the client from pushing its default in-RAM state
        // (level=0, etc.) up to the server before the real data has been loaded.
        private bool   hasLoadedFromServer = false;

        // ── Public state read-only ────────────────────────────────────────
        public bool  IsSyncing          => isSyncing;
        public bool  HasUnsyncedChanges => lastKnownHash != lastSyncedHash || markedDirty;
        public float SecondsSinceLastSync => Time.unscaledTime - lastSyncTime;

        // ── Unity lifecycle ───────────────────────────────────────────────
        private void Start()
        {
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess += OnLoginSuccess;
                AuthManager.Instance.OnLogout       += OnLogoutReset;
            }
        }

        private void OnDestroy()
        {
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess -= OnLoginSuccess;
                AuthManager.Instance.OnLogout       -= OnLogoutReset;
            }
        }

        private void OnLoginSuccess()
        {
            // LoadFromServer already seeds hasLoadedFromServer and the baseline hash
            // inside its success handler, so no post-load fixup is needed here.
            LoadFromServer();
        }

        private void OnLogoutReset()
        {
            // Next login must re-load before we trust RAM state again.
            hasLoadedFromServer = false;
            lastSyncedHash = "";
            lastKnownHash  = "";
            markedDirty    = false;
        }

        private void Update()
        {
            if (!autoSyncEnabled) return;
            if (AuthManager.Instance == null || !AuthManager.Instance.IsLoggedIn) return;
            // CRITICAL: never auto-save until we have pulled real data from the server at
            // least once. Otherwise the default in-RAM state (level=0, empty persona, ...)
            // would be pushed up and clobber the user's real progress on the server.
            if (!hasLoadedFromServer) return;
            if (isSyncing) return;

            float now = Time.unscaledTime;

            // 1. Poll the data hash periodically
            if (now - lastPollTime >= pollInterval)
            {
                lastPollTime = now;
                string currentHash = ComputeDataHash();
                if (currentHash != lastKnownHash)
                {
                    lastKnownHash  = currentHash;
                    lastChangeTime = now;
                }
            }

            // 2. Dirty check
            bool dirty = markedDirty || (lastKnownHash != lastSyncedHash);
            if (!dirty) return;

            // 3. Respect min throttle
            if (now - lastSyncTime < minSyncInterval) return;

            // 4. Respect failure backoff
            if (now - lastFailureTime < failureBackoff) return;

            // 5. Either debounce ready OR periodic force ready
            bool debounceReady = (now - lastChangeTime) >= debounceSeconds;
            bool forceReady    = (now - lastSyncTime)   >= periodicForceSec;

            if (debounceReady || forceReady)
            {
                SaveToServer(null, forceReady ? "auto_periodic" : "auto_debounce");
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  PUBLIC API
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Hint that data has changed. Optional — the polling hash will also
        /// detect changes. Use this if you want to force the next sync cycle
        /// to definitely run even if the hash somehow misses the change.
        /// </summary>
        public void MarkDirty(string reason = "")
        {
            markedDirty = true;
            lastChangeTime = Time.unscaledTime;
            // Uncomment for verbose logging:
            // Debug.Log($"[SyncManager] MarkDirty: {reason}");
        }

        /// <summary>
        /// Force an immediate sync regardless of throttle/debounce/backoff.
        /// Use for critical moments (logout, app quit, level complete).
        /// </summary>
        public void ForceSyncNow(Action onComplete = null, string trigger = "force")
        {
            // Bypass throttle and backoff
            lastSyncTime    = -999f;
            lastFailureTime = -999f;
            SaveToServer(onComplete, trigger);
        }

        /// <summary>Enable/disable automatic sync. Manual calls still work.</summary>
        public void SetAutoSyncEnabled(bool enabled) => autoSyncEnabled = enabled;

        // ══════════════════════════════════════════════════════════════════
        //  SAVE TO SERVER
        // ══════════════════════════════════════════════════════════════════
        public void SaveToServer(Action onComplete = null, string saveTrigger = "manual")
        {
            if (AuthManager.Instance == null || !AuthManager.Instance.IsLoggedIn)
            {
                onComplete?.Invoke();
                return;
            }

            // Refuse to push if we never successfully loaded from server for this
            // login — we would overwrite real progress with default in-RAM values.
            if (!hasLoadedFromServer)
            {
                Debug.LogWarning($"[SyncManager] Skip save ({saveTrigger}) — server data not loaded yet");
                onComplete?.Invoke();
                return;
            }

            if (isSyncing)
            {
                onComplete?.Invoke();
                return;
            }

            isSyncing = true;

            var request = CollectGameData();
            request.sessionId  = AuthManager.Instance.CurrentSessionId;
            request.saveTrigger = saveTrigger;
            string json = JsonUtility.ToJson(request);

            // Capture the hash AT send time — if the sync succeeds we'll
            // use it as the new baseline (even if more changes happen during
            // the round-trip, we'll catch them on the next poll).
            string sendHash = ComputeDataHash();

            ApiClient.Instance.Post<ApiResponse>("/api/gamedata/save", json,
                (response) =>
                {
                    isSyncing = false;
                    if (response.success)
                    {
                        lastSyncedHash = sendHash;
                        lastSyncTime   = Time.unscaledTime;
                        markedDirty    = false;
                        Debug.Log($"[SyncManager] Saved ({saveTrigger})\n{json}");
                    }
                    else
                    {
                        lastFailureTime = Time.unscaledTime;
                        Debug.LogWarning($"[SyncManager] Save failed: {response.message}");
                    }
                    onComplete?.Invoke();
                },
                (error) =>
                {
                    isSyncing = false;
                    lastFailureTime = Time.unscaledTime;
                    Debug.LogWarning($"[SyncManager] Save error: {error}");
                    onComplete?.Invoke();
                }
            );
        }

        // ══════════════════════════════════════════════════════════════════
        //  LOAD FROM SERVER
        // ══════════════════════════════════════════════════════════════════

        [SerializeField] private GameDataPayload dataEditor;
        public void LoadFromServer(Action onComplete = null)
        {
            if (AuthManager.Instance == null || !AuthManager.Instance.IsLoggedIn)
            {
                onComplete?.Invoke();
                return;
            }

            ApiClient.Instance.Get<GameDataLoadResponse>("/api/gamedata/load",
                (response) =>
                {
                    if (response.success && response.data != null)
                    {
                        dataEditor= response.data;
                        ApplyGameData(response.data);
                        // Unlock auto-save and manual SaveToServer now that RAM reflects
                        // the real server state. Seed the baseline hash in the same breath
                        // so we don't immediately re-sync what we just loaded.
                        hasLoadedFromServer = true;
                        lastSyncedHash      = ComputeDataHash();
                        lastKnownHash       = lastSyncedHash;
                        lastSyncTime        = Time.unscaledTime;
                        markedDirty         = false;
                        Debug.Log($"[SyncManager] Data loaded from server \n {response.data}");
                    }
                    else
                    {
                        Debug.LogWarning($"[SyncManager] Load failed: {response.message}");
                    }
                    onComplete?.Invoke();
                },
                (error) =>
                {
                    Debug.LogWarning($"[SyncManager] Load error: {error}");
                    onComplete?.Invoke();
                }
            );
        }

        // ══════════════════════════════════════════════════════════════════
        //  DATA HASH  (change detection without MarkDirty)
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Compute a cheap, stable hash of all syncable data. Detects any
        /// change without requiring explicit MarkDirty() calls from managers.
        /// </summary>
        private string ComputeDataHash()
        {
            var sb = new StringBuilder(512);

            if (DataManager.Instance?.Data != null)
            {
                var gd = DataManager.Instance.Data;
                sb.Append(gd.level).Append('|')
                  .Append(gd.namePlayer).Append('|')
                  .Append(gd.age).Append('|')
                  .Append((int)gd.subject).Append('|')
                  .Append(gd.subjectName).Append('§');
            }

            if (PersonaManager.Instance?.Data != null)
                sb.Append(JsonUtility.ToJson(PersonaManager.Instance.Data)).Append('§');
            if (Level2Manager.Instance?.Data != null)
                sb.Append(JsonUtility.ToJson(Level2Manager.Instance.Data)).Append('§');
            if (Level3Manager.Instance?.Data != null)
                sb.Append(JsonUtility.ToJson(Level3Manager.Instance.Data)).Append('§');
            if (Level4Manager.Instance?.Data != null)
                sb.Append(JsonUtility.ToJson(Level4Manager.Instance.Data)).Append('§');
            if (SkillManager.Instance != null)
                sb.Append(SkillManager.Instance.ToJson()).Append('§');

            sb.Append(PlayerPrefs.GetFloat("volumn_sfx", 1f)).Append('|')
              .Append(PlayerPrefs.GetFloat("volumn_music", 1f)).Append('|')
              .Append(PlayerPrefs.GetInt("mute_vibra", 0));

            // DJB-variant 64-bit hash + length to minimize collisions
            string content = sb.ToString();
            unchecked
            {
                int h1 = 5381;
                int h2 = 5381;
                for (int i = 0; i < content.Length; i++)
                {
                    if ((i & 1) == 0) h1 = ((h1 << 5) + h1) ^ content[i];
                    else              h2 = ((h2 << 5) + h2) ^ content[i];
                }
                return $"{h1:X8}{h2:X8}_{content.Length}";
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  COLLECT FROM RAM
        // ══════════════════════════════════════════════════════════════════
        private GameDataSaveRequest CollectGameData()
        {
            var request = new GameDataSaveRequest();

            // GameData
            if (DataManager.Instance?.Data != null)
            {
                var gd = DataManager.Instance.Data;
                request.level = gd.level;
                request.namePlayer = gd.namePlayer;
                request.age = gd.age;
                request.subject = (int)gd.subject;
                request.subjectName = gd.subjectName;
            }

            // Persona (Level 1 persona stats + reflections)
            if (PersonaManager.Instance?.Data != null)
                request.personaJson = JsonUtility.ToJson(PersonaManager.Instance.Data);

            // Level2 — serialize from RAM
            if (Level2Manager.Instance?.Data != null)
                request.level2Json = JsonUtility.ToJson(Level2Manager.Instance.Data);

            // Level3 — serialize from RAM
            if (Level3Manager.Instance?.Data != null)
                request.level3Json = JsonUtility.ToJson(Level3Manager.Instance.Data);

            // Level4 — serialize from RAM
            if (Level4Manager.Instance?.Data != null)
                request.level4Json = JsonUtility.ToJson(Level4Manager.Instance.Data);

            // Settings
            request.settingsJson = JsonUtility.ToJson(new SettingsData
            {
                sfxVolume = PlayerPrefs.GetFloat("volumn_sfx", 1f),
                musicVolume = PlayerPrefs.GetFloat("volumn_music", 1f),
                muteVibra = PlayerPrefs.GetInt("mute_vibra", 0)
            });

            // Skills
            if (SkillManager.Instance != null)
                request.skillsJson = SkillManager.Instance.ToJson();

            // Analytics snapshot
            if (AnalyticsManager.Instance != null)
                request.analyticsJson = JsonUtility.ToJson(AnalyticsManager.Instance.Build());

            return request;
        }

        // ══════════════════════════════════════════════════════════════════
        //  APPLY TO RAM
        // ══════════════════════════════════════════════════════════════════
        private void ApplyGameData(GameDataPayload data)
        {
            // GameData
            if (DataManager.Instance != null)
            {
                DataManager.Instance.Data.level = data.level;
                DataManager.Instance.Data.namePlayer = data.namePlayer;
                DataManager.Instance.Data.age = data.age;
                DataManager.Instance.Data.subject = (ESubject)data.subject;
                DataManager.Instance.Data.subjectName = data.subjectName;
            }

            // Level2 — deserialize to RAM
            if (Level2Manager.Instance != null)
            {
                Level2Manager.Instance.Data = !string.IsNullOrEmpty(data.level2Json) && data.level2Json != "{}"
                    ? JsonUtility.FromJson<Level2Data>(data.level2Json) ?? new Level2Data()
                    : new Level2Data();
            }

            // Level3 — deserialize to RAM
            if (Level3Manager.Instance != null)
            {
                Level3Manager.Instance.Data = !string.IsNullOrEmpty(data.level3Json) && data.level3Json != "{}"
                    ? JsonUtility.FromJson<Level3Data>(data.level3Json) ?? new Level3Data()
                    : new Level3Data();
            }

            // Level4 — deserialize to RAM
            if (Level4Manager.Instance != null)
            {
                Level4Manager.Instance.Data = !string.IsNullOrEmpty(data.level4Json) && data.level4Json != "{}"
                    ? JsonUtility.FromJson<Level4Data>(data.level4Json) ?? new Level4Data()
                    : new Level4Data();
            }

            // Persona
            if (PersonaManager.Instance != null)
            {
                PersonaManager.Instance.Data = !string.IsNullOrEmpty(data.personaJson) && data.personaJson != "{}"
                    ? JsonUtility.FromJson<PersonaData>(data.personaJson) ?? new PersonaData()
                    : new PersonaData();
            }

            // Settings
            if (!string.IsNullOrEmpty(data.settingsJson) && data.settingsJson != "{}")
            {
                var settings = JsonUtility.FromJson<SettingsData>(data.settingsJson);
                if (settings != null)
                {
                    PlayerPrefs.SetFloat("volumn_sfx", settings.sfxVolume);
                    PlayerPrefs.SetFloat("volumn_music", settings.musicVolume);
                    PlayerPrefs.SetInt("mute_vibra", settings.muteVibra);
                }
            }

            // Skills
            if (!string.IsNullOrEmpty(data.skillsJson) && data.skillsJson != "{}")
                SkillManager.Instance?.FromJson(data.skillsJson);
            else
                SkillManager.Instance?.FromJson("{\"unlocked\":[]}");

            // Notify observers
            Observer.Instance?.Notify(ObserverKey.PersonaDataChange);
        }
    }
}
