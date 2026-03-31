namespace Luzart
{
    using System;
    using UnityEngine;

    public class SyncManager : Singleton<SyncManager>
    {
        private bool isSyncing = false;

        private void Start()
        {
            // Listen for login success to auto-load from server
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess += OnLoginSuccess;
            }
        }

        private void OnDestroy()
        {
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess -= OnLoginSuccess;
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus && AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
            {
                SaveToServer();
            }
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
                Debug.LogWarning("[SyncManager] Not logged in, skip save");
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
                    {
                        Debug.Log("[SyncManager] Data saved to server");
                    }
                    else
                    {
                        Debug.LogWarning($"[SyncManager] Save failed: {response.message}");
                    }
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
                Debug.LogWarning("[SyncManager] Not logged in, skip load");
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

        // ============ COLLECT ALL DATA ============

        private GameDataSaveRequest CollectGameData()
        {
            var request = new GameDataSaveRequest();

            // GameData from DataManager
            if (DataManager.Instance != null && DataManager.Instance.Data != null)
            {
                var gd = DataManager.Instance.Data;
                request.level = gd.level;
                request.namePlayer = gd.namePlayer;
                request.age = gd.age;
                request.subject = (int)gd.subject;
                request.subjectName = gd.subjectName;
            }

            // PlayerResources
            PlayerResources res = GameRes.GetCachedPlayerResources();
            if (res != null)
            {
                request.resourcesJson = JsonUtility.ToJson(res);
            }

            // Heart
            if (HeartManager.Instance != null && HeartManager.Instance.dataHeart != null)
            {
                request.heartJson = JsonUtility.ToJson(HeartManager.Instance.dataHeart);
            }

            // Pack
            if (PackManager.Instance != null && PackManager.Instance.GamePackData != null)
            {
                request.packJson = JsonUtility.ToJson(PackManager.Instance.GamePackData);
            }

            // Level2 data
            string level2Json = PlayerPrefs.GetString("Level2_AI", "");
            if (!string.IsNullOrEmpty(level2Json))
            {
                request.level2Json = level2Json;
            }

            // Level3 data
            string level3Json = PlayerPrefs.GetString("Level3_Data", "");
            if (!string.IsNullOrEmpty(level3Json))
            {
                request.level3Json = level3Json;
            }

            // Settings
            var settings = new SettingsData
            {
                sfxVolume = PlayerPrefs.GetFloat("volumn_sfx", 1f),
                musicVolume = PlayerPrefs.GetFloat("volumn_music", 1f),
                muteVibra = PlayerPrefs.GetInt("mute_vibra", 0)
            };
            request.settingsJson = JsonUtility.ToJson(settings);

            // Skills
            if (SkillManager.Instance != null)
                request.skillsJson = SkillManager.Instance.ToJson();

            // Level4 quiz answers
            string level4Json = PlayerPrefs.GetString("Level_4", "");
            if (!string.IsNullOrEmpty(level4Json))
                request.level4Json = level4Json;

            // Analytics snapshot (derived from all managers)
            if (AnalyticsManager.Instance != null)
                request.analyticsJson = JsonUtility.ToJson(AnalyticsManager.Instance.Build());

            return request;
        }

        // ============ APPLY DATA FROM SERVER ============

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
                DataManager.Instance.SaveGameData();
            }

            // PlayerResources
            if (!string.IsNullOrEmpty(data.resourcesJson) && data.resourcesJson != "{}")
            {
                PlayerResources res = JsonUtility.FromJson<PlayerResources>(data.resourcesJson);
                if (res != null)
                {
                    GameRes.SavePlayerResources(res);
                }
            }

            // Heart
            if (HeartManager.Instance != null && !string.IsNullOrEmpty(data.heartJson) && data.heartJson != "{}")
            {
                DataHeart heart = JsonUtility.FromJson<DataHeart>(data.heartJson);
                if (heart != null)
                {
                    HeartManager.Instance.dataHeart = heart;
                    HeartManager.Instance.SaveData();
                }
            }

            // Pack
            if (PackManager.Instance != null && !string.IsNullOrEmpty(data.packJson) && data.packJson != "{}")
            {
                GamePackData packData = JsonUtility.FromJson<GamePackData>(data.packJson);
                if (packData != null)
                {
                    // Use reflection-free approach: save to PlayerPrefs and reload
                    SaveLoadUtil.SaveDataPrefs(packData, "gamepackdata");
                    PackManager.Instance.Initialize();
                }
            }

            // Level2 data
            if (!string.IsNullOrEmpty(data.level2Json) && data.level2Json != "{}")
            {
                PlayerPrefs.SetString("Level2_AI", data.level2Json);
            }

            // Level3 data
            if (!string.IsNullOrEmpty(data.level3Json) && data.level3Json != "{}")
            {
                PlayerPrefs.SetString("Level3_Data", data.level3Json);
            }

            // Level4 quiz data
            if (!string.IsNullOrEmpty(data.level4Json) && data.level4Json != "{}")
            {
                PlayerPrefs.SetString("Level_4", data.level4Json);
            }

            // analyticsJson is read-only (computed on save, not restored)

            // Settings
            if (!string.IsNullOrEmpty(data.settingsJson) && data.settingsJson != "{}")
            {
                SettingsData settings = JsonUtility.FromJson<SettingsData>(data.settingsJson);
                if (settings != null)
                {
                    PlayerPrefs.SetFloat("volumn_sfx", settings.sfxVolume);
                    PlayerPrefs.SetFloat("volumn_music", settings.musicVolume);
                    PlayerPrefs.SetInt("mute_vibra", settings.muteVibra);
                }
            }

            // Skills
            if (!string.IsNullOrEmpty(data.skillsJson) && data.skillsJson != "{}")
            {
                SkillManager.Instance?.FromJson(data.skillsJson);
            }

            PlayerPrefs.Save();

            // Notify observers
            Observer.Instance?.Notify(ObserverKey.PersonaDataChange);
            Observer.Instance?.Notify(ObserverKey.CoinObserverNormal);
        }
    }
}
