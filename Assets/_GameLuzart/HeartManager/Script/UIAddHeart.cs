using Luzart;
using System;
using TMPro;

public class UIAddHeart : UIBase
{
    private void Start()
    {
        UIManager.AddActionRefreshUI(RefreshUI);
        Observer.Instance?.AddObserver(ObserverKey.TimeActionPerSecond, HeartPerSecond);
    }
    private void OnDestroy()
    {
        UIManager.RemoveActionRefreshUI(RefreshUI);
        Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, HeartPerSecond);
    }

    public TMP_Text txtAmount;
    public TMP_Text txtTime;
    public TMP_Text txtBuyValueCoin;

    public DataResource dB_ResourcesBuyGold;
    public DataResource dB_ResourcesReceiveGold;
    public DataResource dB_ResourcesReceiveAds;
    private void HeartPerSecond(object data)
    {
        if (HeartManager.Instance.IsMaxHeart)
        {
            Hide();
            return;
        }
        txtAmount.text = HeartManager.Instance.AmountHeart.ToString();
        txtTime.text = GameUtil.LongTimeSecondToUnixTime(HeartManager.Instance.TimeCointainHeartNone, true, "", "", "", "");

    }
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        HeartPerSecond(null);
        txtBuyValueCoin.text = $"<sprite=0>{dB_ResourcesBuyGold.amount}";
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        bool isInfinite = HeartManager.Instance.EStateHeart == EStateHeart.Infinite;
        bool isMaxHeart = (HeartManager.Instance.IsMaxHeart);
        if (isInfinite || isMaxHeart)
        {
            Hide();
            return;
        }
    }
    public void OnClickBuyCoin()
    {
        DataWrapperGame.SubtractResources(new DataResource(new DataTypeResource(RES_type.Gold), dB_ResourcesBuyGold.amount),
            OnDoneReceive, ValueFirebase.ReFillHeart);
    }
    public void OnClickBuyAds()
    {
        AdsWrapperManager.ShowReward(KeyAds.AdsUIAddCoin, OnDoneReceiveAds, UIManager.Instance.ShowToastInternet);
    }
    private void OnDoneReceive()
    {
        int value = HeartManager.Instance.MaxHeartNone - HeartManager.Instance.AmountHeart;
        var resReceiveHeartByGold = new DataResource(new DataTypeResource(RES_type.Heart), value);
        DataWrapperGame.ReceiveRewardShowPopUp(ValueFirebase.ReFillHeart, UIManager.Instance.RefreshUI, dataResource: resReceiveHeartByGold);
    }
    private void OnDoneReceiveAds()
    {
        DataWrapperGame.ReceiveRewardShowPopUp(ValueFirebase.RewardAds, UIManager.Instance.RefreshUI, dataResource: dB_ResourcesReceiveAds);
    }
}
