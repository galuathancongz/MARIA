namespace Luzart
{
    using System;
    using UnityEngine;

    public class SyncManager : Singleton<SyncManager>
    {
        private bool isSyncing = false;

        private void Start()
        {
            if (AuthManager.Instance != null)
                AuthManager.Instance.OnLoginSuccess += OnLoginSuccess;
        }

        private void OnDestroy()
        {
            if (AuthManager.Instance != null)
                AuthManager.Instance.OnLoginSuccess -= OnLoginSuccess;
        }

        private void OnLoginSuccess()
        {
            LoadFromServer();
        }

        // ============ SAVE TO SERVER ============

        public void SaveToServer(Action onComplete = null)
        {
            if (!AuthManager.Instance.IsLoggedIn)
            {
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
            string json = JsonUtility.ToJson(request);

            ApiClient.Instance.Post<ApiResponse>("/api/gamedata/save", json,
                (response) =>
                {
                    isSyncing = false;
                    if (response.success)
                        Debug.Log("[SyncManager] Data saved to server");
                    else
                        Debug.LogWarning($"[SyncManager] Save failed: {response.message}");
                    onComplete?.Invoke();
                },
                (error) =>
                {
                    isSyncing = false;
                    Debug.LogWarning($"[SyncManager] Save error: {error}");
                    onComplete?.Invoke();
                }
            );
        }

        // ============ LOAD FROM SERVER ============

        public void LoadFromServer(Action onComplete = null)
        {
            if (!AuthManager.Instance.IsLoggedIn)
            {
                onComplete?.Invoke();
                return;
            }

            ApiClient.Instance.Get<GameDataLoadResponse>("/api/gamedata/load",
                (response) =>
                {
                    if (response.success && response.data != null)
                    {
                        ApplyGameData(response.data);
                        Debug.Log("[SyncManager] Data loaded from server");
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

        // ============ COLLECT FROM RAM ============

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

        // ============ APPLY TO RAM ============

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
