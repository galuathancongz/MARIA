namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;
    
    public class ResetPackInPopUp : MonoBehaviour
    {
        public EPack[] ePacks;
        public Transform[] parentSpawns;

        private List<bool> listBoolTransform = new List<bool>();
        protected List<EPack> eCurrentPacks = new List<EPack>();
        protected List<ItemPackInPopUp> listItem = new List<ItemPackInPopUp>();

        private void OnEnable()
        {
            RefreshUI();
        }
        public virtual void RefreshUI()
        {
            CalculatePack();
            ShowPack();
        }
        public virtual void ShowPack()
        {
            int lengthCurrent = eCurrentPacks.Count;
            for (int i = 0; i < lengthCurrent; i++)
            {
 
                var ePackCur = eCurrentPacks[i];
                var item = GetItemPackInPopUp(ePackCur, parentSpawns[i]);
            }
            PushFirebase();
        }
        public virtual void PushFirebase()
        {

        }
        public virtual void CalculatePack()
        {
            eCurrentPacks = new List<EPack>();
            int length = ePacks.Length;
            for (int i = 0; i < length; i++)
            {
                GetPack(ePacks[i], 0);
            }
        }
        private int minCountPurchase = -1;
        public virtual void GetPack(EPack ePack, int index)
        {
            index++;
            if(eCurrentPacks.Count >= 3)
            {
                return;
            }
            if(index > ePacks.Length)
            {
                eCurrentPacks.Add(ePack);
                return;
            }
            string productId = PackManager.Instance.dbPackSO.GetDBPack(ePack).productId;
            int countPurchasePack = PackManager.Instance.GetPackPurchaseCount(productId);
            if(countPurchasePack > minCountPurchase)
            {
                minCountPurchase = countPurchasePack;
            }
            int minCountAll = PackManager.Instance.GetCountPackBuyMin;
            if (WithOut(ePack))
            {
                GetPack(GetNextPack(ePack), index);
                return;
            }
            if (IsHasPackShow(ePack))
            {
                GetPack(GetNextPack(ePack), index);
                return;
            }
            else if (countPurchasePack > minCountAll)
            {
                GetPack(GetNextPack(ePack), index);
                return;
            }
            else
            {
                eCurrentPacks.Add(ePack);
            }
    
        }
        private EPack GetNextPack(EPack ePack)
        {
            int length = ePacks.Length;
            int currentIndex = GetCurrentPack(ePack);
            int nextPack = (currentIndex +1 ) %length;
            return ePacks[nextPack];
        }
        private int GetCurrentPack(EPack ePack)
        {
            int length = ePacks.Length;
            for (int i = 0; i < length; i++)
            {
                if(ePack == ePacks[i])
                {
                    return i;
                }
            }
            return 0;
        }
        private bool IsHasPackShow(EPack ePack)
        {
            int length = eCurrentPacks.Count ;
            for (int i = 0; i < length; i++)
            {
                if(ePack == eCurrentPacks[i]) { return true; }             
            }
            return false;
        }
        protected virtual bool WithOut(EPack ePack)
        {
            if(ePack == EPack.StarterPack)
            {
                var db = PackManager.Instance.dbPackSO.GetDBPack(EPack.StarterPack);
                string productId = db.productId;
                int countPurchase = PackManager.Instance.GetPackPurchaseCount(productId);
                if(db.maxBuy <= countPurchase)
                {
                    return true;
                }
            }
            //if(ePack == EPack.BattlePass)
            //{
            //    var dB_Event = EventManager.Instance.GetEvent(EEventName.BattlePass);
            //    if(dB_Event == null || dB_Event.eventStatus == EEventStatus.Finish || EventManager.Instance.battlePassManager.dataBattlePass.isBuyIAP)
            //    {
            //        return true;
            //    }
            //}
            //if(ePack == EPack.SpecialGold)
            //{
            //    return true;
            //}
            return false;
        }

        private const string dirPackInPopUp = "PackInPopUp";
        private Dictionary<EPack, string> dictEPackInPopUp = new Dictionary<EPack, string>()
        {
            { EPack.StarterPack, "PackStarter" },
            { EPack.RemoveAds, "PackNoAds" },
            { EPack.RemoveAdsBundle, "PackNoAdsBundle" },
        };
        private string GetPath(EPack ePack)
        {
            if (dictEPackInPopUp.ContainsKey(ePack))
            {
                return $"{dirPackInPopUp}/{dictEPackInPopUp[ePack]}";
            }
            else
            {
                Debug.LogError("Dont have Pack to Init");
                return null;
            }
            
        }

        public ItemPackInPopUp GetItemPackInPopUp(EPack ePack, Transform parent)
        {
            string path = GetPath(ePack);
            var item = Resources.Load<ItemPackInPopUp>(path);
            item.transform.SetParent(parent);
            return item;
        }
    }
}
