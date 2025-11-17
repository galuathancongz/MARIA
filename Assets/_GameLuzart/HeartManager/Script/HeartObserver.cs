using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Luzart
{
    public class HeartObserver : MonoBehaviour
    {
        public RectTransform rectHeart;

        public Button btnAddHeart;

        public BaseSelect selectInfinite;
        public BaseSelect selectMax;
        public HeartManager heartManager
        {
            get
            {
                return HeartManager.Instance;
            }
        }
        public TMP_Text txtTime;
        public TMP_Text txtValue;
        
        public void Start()
        {
            Observer.Instance.AddObserver(ObserverKey.TimeActionPerSecond, HeartPerSecond);
            GameUtil.ButtonOnClick(btnAddHeart, OnClickAddHeart, true);
            HeartPerSecond(null);
        }
        public void OnClickAddHeart()
        {
            Debug.LogError("OnClickAddHeart");
        }
        private void OnDestroy()
        {
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, HeartPerSecond);
        }
        private void HeartPerSecond(object data)
        {
            bool isInfinite = heartManager.EStateHeart == EStateHeart.Infinite;
            selectInfinite.Select(isInfinite);
            if (isInfinite)
            {
                SetActiveButton(false);
                txtTime.text = GameUtil.LongTimeSecondToUnixTime(heartManager.TimeHeartInfinite, true, "", "", "", "");
                return;
            }
            txtValue.text = heartManager.AmountHeart.ToString();
            if (heartManager.IsMaxHeart)
            {
                txtTime.text = "MAX";
                SetActiveButton(false);
                return;
            }
            SetActiveButton(true);
            txtTime.text = GameUtil.LongTimeSecondToUnixTime(heartManager.TimeCointainHeartNone, true, "", "", "", "");

        }
        private void SetActiveButton(bool active)
        {
            btnAddHeart.interactable = active;
            selectMax.Select(!active);
        }
    }
}


