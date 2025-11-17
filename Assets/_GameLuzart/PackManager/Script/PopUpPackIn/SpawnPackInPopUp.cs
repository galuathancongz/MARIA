using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPackInPopUp : MonoBehaviour
{
    public EPack[] ePacks;
    private List<ItemPackInPopUp> listItemPackInPopUp = new List<ItemPackInPopUp>();
    private List<EPack> listEPackCurrent = new List<EPack>();
    public ScrollPro scrollPro;

    private const string dirPackInPopUp = "PackInPopUp";
    private Dictionary<EPack, string> dictEPackInPopUp = new Dictionary<EPack, string>()
        {
            { EPack.StarterPack, "PackStarter" },
            { EPack.RemoveAds, "PackNoAds" },
            { EPack.RemoveAdsBundle, "PackNoAdsBundle" },
            { EPack.Recovery, "PackRecovery" },
            { EPack.Booster, "PackBooster" },
        };
    private string GetPath(EPack ePack)
    {
        if (dictEPackInPopUp.ContainsKey(ePack))
        {
            return $"{dirPackInPopUp}/{dictEPackInPopUp[ePack]}";
        }
        else
        {
            Debug.LogError($"Dont have Pack {ePack} to Init");
            return null;
        }

    }
    private void Awake()
    {
        UIManager.AddActionRefreshUI(RefreshUI);
    }
    private void OnDestroy()
    {
        UIManager.RemoveActionRefreshUI(RefreshUI);
    }
    private void OnEnable()
    {
        RefreshUI();
    }

    public ItemPackInPopUp GetItemPackInPopUp(EPack ePack, Transform parent)
    {
        string path = GetPath(ePack);
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        var item = Resources.Load<ItemPackInPopUp>(path);
        var spawnItem = Instantiate<ItemPackInPopUp>(item, parent);
        listItemPackInPopUp.Add(spawnItem);
        var rect = spawnItem.GetComponent<RectTransform>();
        if(rect != null)
        {
            rect.anchoredPosition = new Vector2(0, 0);
        }
        return spawnItem;
    }

    private void RefreshUI()
    {
        int length = listItemPackInPopUp.Count;
        for (int i = 0; i < length; i++)
        {
            int index = i;
            var item = listItemPackInPopUp[0];
            listItemPackInPopUp.Remove(item);
            Destroy(item.gameObject);
        }
        SpawnInPack();
    }
    private void SpawnInPack()
    {
        listEPackCurrent.Clear();
        int length = ePacks.Length;
        for (int i = 0; i < length; i++)
        {
            var ePack = ePacks[i];
            bool isCanShow = PackManager.Instance.IsCanShowPack(ePack);
            if (isCanShow)
            {
                listEPackCurrent.Add(ePack);
            }
        }
        int lengthePackCurrent = listEPackCurrent.Count;
        scrollPro.Initialize(lengthePackCurrent,SpawnPack);


        void SpawnPack(ParentSpawnItemPack parentSpawn, int index)
        {
            var item = GetItemPackInPopUp(listEPackCurrent[index], parentSpawn.parentSpawn);
            if(item == null)
            {
                return;
            }
            item.Initialize();
        }
    }

}
