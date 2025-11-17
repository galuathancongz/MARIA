using UnityEngine;

namespace Luzart
{
    public partial class HeartManager : Singleton<HeartManager>
    {
        public EStateHeart EStateHeart => dataHeart.EStateHeart;
        private const int TimeCountdownAddHeartNone = 900;
        public int MaxHeartNone
        {
            get
            {
                //if (EventManager.Instance.IsHasEvent(EEventName.BattlePass) && EventManager.Instance.IsUnlockLevel(EEventName.BattlePass))
                //{
                //    BattlePassManager manager = EventManager.Instance.battlePassManager;
                //    if (manager.dataEvent.isBuyIAP)
                //    {
                //        return 8;
                //    }
                //}
                return 5;
            }
        }
        private const string PATH_HEART = "data_heart";
        public DataHeart dataHeart;

        public int TimeCointainHeartNone
        {
            get
            {
                return TimeCountdownAddHeartNone - dataHeart.timeHeartCurrent;
            }
        }
        public bool IsMaxHeart
        {
            get
            {
                if (dataHeart.CountHeart >= MaxHeartNone)
                {
                    return true;
                }
                return false;
            }
        }
        public int AmountHeart => dataHeart.CountHeart;
        public bool IsCanPlayNewGame
        {
            get
            {
                return !(EStateHeart == EStateHeart.None && AmountHeart <= 0);
            }
        }

        public void UseHeart(int amount)
        {
            DataWrapperGame.SubtractResources(new DataResource(new DataTypeResource(RES_type.Heart), -amount), null, ValueFirebase.LevelFinish);
        }

        public int TimeHeartInfinite => dataHeart.timeHeartInfinite;
        private bool IsInit = false;

        private void Awake()
        {
            Initialize();
        }
        private void OnApplicationFocus(bool focus)
        {
            SaveData();
        }
        public void Initialize()
        {
            LoadData();
            CalculateTimeOnReturn();
        }

        public void LoadData()
        {
            dataHeart = SaveLoadUtil.LoadDataPrefs<DataHeart>(PATH_HEART);
            if (dataHeart == null)
            {
                dataHeart = new DataHeart();
                dataHeart.lastTimeEnd = TimeUtils.GetLongTimeCurrent;
            }
            IsInit = true;
        }
        public void SaveData()
        {
            if (!IsInit)
            {
                return;
            }
            SaveLoadUtil.SaveDataPrefs<DataHeart>(dataHeart, PATH_HEART);
        }
        private void Start()
        {
            Observer.Instance?.AddObserver(ObserverKey.TimeActionPerSecond, OnPerSecond);
            StartFuncionWrapper();
        }
        private void OnDestroy()
        {
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnPerSecond);
            DestroyFuncionWrapper();
        }

        private void OnPerSecond(object data)
        {
            dataHeart.lastTimeEnd = TimeUtils.GetLongTimeCurrent;
            if (dataHeart.timeHeartInfinite > 0)
            {
                dataHeart.timeHeartInfinite--;
            }
            if (dataHeart.CountHeart >= MaxHeartNone)
            {
                dataHeart.timeHeartCurrent = 0;
                return;
            }
            dataHeart.timeHeartCurrent++;
            if (dataHeart.timeHeartCurrent >= TimeCountdownAddHeartNone)
            {
                AddHeartNoneInTime();
                dataHeart.timeHeartCurrent = 0;
            }

        }
        private void CalculateTimeOnReturn()
        {
            long timeNow = TimeUtils.GetLongTimeCurrent;
            long deltaTimeLong = timeNow - dataHeart.lastTimeEnd;
            int deltaTime = (int)deltaTimeLong;

            // Nếu có heart Infinite thì trừ thời gian 
            AddTimeInfinite(-deltaTime);

            // Tính số heart tăng lên
            int heartsToAdd = deltaTime / TimeCountdownAddHeartNone;

            // Thêm heart dựa trên thời gian trôi qua
            for (int i = 0; i < heartsToAdd; i++)
            {
                if (AmountHeart < MaxHeartNone)
                {
                    AddHeartNone(1);
                }
            }

            // Cập nhật thời gian đếm ngược còn lại
            dataHeart.timeHeartCurrent += deltaTime % TimeCountdownAddHeartNone;
            if (dataHeart.timeHeartCurrent >= TimeCountdownAddHeartNone)
            {
                AddHeartNoneInTime();
                dataHeart.timeHeartCurrent = 0;
            }
        }
        public void AddTimeInfinite(int time)
        {
            dataHeart.timeHeartInfinite = Mathf.Clamp(dataHeart.timeHeartInfinite + time, 0, int.MaxValue);
            if(time < 0)
            {
                dataHeart.timeHeartCurrent = 5;
            }
        }
        public void AddHeartNone(int value)
        {
            int heart = dataHeart.CountHeart;
            heart += value;
            if(heart >= 0)
            {
                dataHeart.CountHeart = heart;
            }
            SaveData();
        }
        private void AddHeartNoneInTime()
        {
            if (IsMaxHeart)
            {
                return;
            }
            AddHeartNone(1);
        }
    }
    [System.Serializable]
    public enum EStateHeart
    {
        None,
        Infinite,
    }

    [System.Serializable]
    public class DataHeart
    {
        public int valueHeart = 5;
        public int timeHeartCurrent;
        public int timeHeartInfinite = 900;
        public long lastTimeEnd = 0;
        public int CountHeart
        {
            get
            {
                return valueHeart;
            }
            set
            {
                FirebaseEvent.SetUserProperty("live_number",valueHeart.ToString());
                valueHeart = value;
            }
        }
        public EStateHeart EStateHeart
        {
            get
            {
                if (timeHeartInfinite > 0)
                {
                    return EStateHeart.Infinite;
                }
                return EStateHeart.None;
            }
        }
    }
}



