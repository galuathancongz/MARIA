namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class ItemPackInPopUp : PackItem
    {
        [SerializeField]
        private ListResUI listResUI;
    
        [SerializeField]
        private ResUI[] resUIOther;
    
        public override void InitIAP()
        {
            base.InitIAP();
            if (db_Pack.gift.dataResources != null && db_Pack.gift.dataResources.Count > 0 && listResUI != null)
            {
                listResUI.InitResUI(db_Pack.gift.dataResources.ToArray());
            }
            if (db_Pack.resourcesOther.dataResources != null && db_Pack.resourcesOther.dataResources.Count > 0 && resUIOther != null)
            {
                int length = db_Pack.resourcesOther.dataResources.Count;
                for (int i = 0; i < length; i++)
                {
                    var data = db_Pack.resourcesOther.dataResources[i];
                    if (i >= resUIOther.Length || data == null)
                    {
                        continue;
                    }
                    var resUI = resUIOther[i];
                    resUI.InitData(data);
                }
            }
        }
        protected override void OnCompleteBuy()
        {
            if (db_Pack.isRemoveAds)
            {
                AdsWrapperManager.PurchaseRemoveAds();
            }
            List<DataResource> listRes = new List<DataResource>();
            if (db_Pack.gift.dataResources != null && db_Pack.gift.dataResources.Count > 0)
            {
                listRes.AddRange(db_Pack.gift.dataResources);
            }
            if (db_Pack.resourcesOther.dataResources != null && db_Pack.resourcesOther.dataResources.Count > 0)
            {
                listRes.AddRange(db_Pack.resourcesOther.dataResources);
            }
            if (listRes.Count > 0)
            {
                var ui = Luzart.UIManager.Instance.ShowUI<UIReceiveRes>(UIName.ReceiveRes, OnDoneReceivePopUp);
                ui.Initialize(dataResources: listRes.ToArray());
            }
            PackManager.Instance.SaveBuyPack(db_Pack.productId);
            UIManager.Instance.RefreshUI();
        }
        protected virtual void OnDoneReceivePopUp()
        {
            UIManager.Instance.RefreshUI();
        }
    }
}
