namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class DataManager : SingletonSaveLoad<GameData,DataManager>
    {
        protected override string KEYLOAD => "key_gamedata";
        #region GameData
        public int CurrentLevel => Data.level;
        public void Initialize()
        {
        }
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void SaveGameData()
        {
            Save();
        }
        #endregion
        public static int ADSLIMIT = 3;
        private void Start()
        {
            Observer.Instance?.AddObserver(ObserverKey.OnNewDay, OnNewDay);
        }
        private void OnDestroy()
        {
            Observer.Instance?.RemoveObserver(ObserverKey.OnNewDay, OnNewDay);
        }
        private void OnNewDay(object data)
        {
            SaveGameData();
        }
    }
    [System.Serializable]
    public class GameData
    {
        public int level = 0;
        public string namePlayer = "username";
        public int age = 24;
        public string subjectName;
        public ESubject subject;
    }

    public class GameRes
    {
        private static string playerResourcesKey = "PlayerResources";
        private static PlayerResources cachedPlayerResources = null;
        public PlayerResources playerResource
        {
            get
            {
                return cachedPlayerResources;
            }
            set
            {
                cachedPlayerResources = value;
            }
        }

        public static bool isAddRes(DataResource resource)
        {
            PlayerResources playerResources = GetCachedPlayerResources();
            int amountCurrent = playerResources.GetResourceAmount(resource.type);
            return amountCurrent + resource.amount >= 0;
        }

        public static int GetRes(DataTypeResource dataTypeResource)
        {
            PlayerResources playerResources = GetCachedPlayerResources();
            return playerResources.GetResourceAmount(dataTypeResource);
        }

        public static void AddRes(DataTypeResource dataTypeResource, int amount)
        {
            PlayerResources playerResources = GetCachedPlayerResources();
            playerResources.AddResource(new DataResource(dataTypeResource, amount));
            SavePlayerResources(playerResources);
            Debug.Log($"To Add RES {dataTypeResource.type}_{dataTypeResource.id} _ currentvalue {amount}");

            if (dataTypeResource.type == RES_type.Gold)
            {
                Observer.Instance.Notify(ObserverKey.CoinObserverNormal);
            }
        }

        public static void SavePlayerResources()
        {
            SavePlayerResources(cachedPlayerResources);
        }

        public static void SavePlayerResources(PlayerResources playerResources)
        {
            string json = JsonUtility.ToJson(playerResources);
            PlayerPrefs.SetString(playerResourcesKey, json);
            PlayerPrefs.Save();
            cachedPlayerResources = playerResources; // Cập nhật bộ nhớ cache
        }

        public static PlayerResources GetCachedPlayerResources()
        {
            if (cachedPlayerResources == null)
            {
                cachedPlayerResources = LoadPlayerResources();
            }
            return cachedPlayerResources;
        }

        private static PlayerResources LoadPlayerResources()
        {
            if (PlayerPrefs.HasKey(playerResourcesKey))
            {
                string json = PlayerPrefs.GetString(playerResourcesKey);
                return JsonUtility.FromJson<PlayerResources>(json);
            }
            else
            {
                return new PlayerResources();
            }
        }
    }
    [System.Serializable]
    public class PlayerResources
    {
        public List<DataResource> resources;

        public PlayerResources()
        {
            resources = new List<DataResource>();
        }

        public void AddResource(DataResource resource)
        {
            DataResource existingResource = resources.Find(r => r.type.Compare(resource.type));
            if (existingResource != null)
            {
                existingResource.amount += resource.amount;
            }
            else
            {
                resources.Add(resource);
            }
        }

        public int GetResourceAmount(DataTypeResource dataTypeResource)
        {
            DataResource resource = resources.Find(r => r.type.Compare(dataTypeResource));
            return resource != null ? resource.amount : 0;
        }
    }

}