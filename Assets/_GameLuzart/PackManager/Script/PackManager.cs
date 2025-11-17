namespace Luzart
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class PackManager : Singleton<PackManager>
    {
        public DBPackSO dbPackSO;
        private GamePackData _gamePackData = null;
        public GamePackData GamePackData => _gamePackData;

        private void Awake()
        {
            Initialize();
        }

        #region GameData
        private const string KEY_GAME_PACK_DATA = "gamepackdata";
        public void Initialize()
        {
            LoadGameData();
        }
        private void LoadGameData()
        {
            _gamePackData = SaveLoadUtil.LoadDataPrefs<GamePackData>(KEY_GAME_PACK_DATA);
            if (_gamePackData == null)
            {
                _gamePackData = new GamePackData();
            }
        }
        public void SaveGameData()
        {
            SaveLoadUtil.SaveDataPrefs<GamePackData>(_gamePackData, KEY_GAME_PACK_DATA);
        }
        #endregion


        public List<EPack> ePacks = new List<EPack>()
        {
            //EPack.StarterPack,
            //EPack.MiniPack,
            //EPack.VIPPack,
            //EPack.SuperPack,
            //EPack.LifeAndCoinPack,
            //EPack.LargePack,
        };
        public List<EPack> ePackCurrentDefault = new List<EPack>()
        {
            //EPack.StarterPack,
            //EPack.MiniPack,
            //EPack.VIPPack,
        };
        public List<EPack> ePackCurrent = new List<EPack>()
        {
            //EPack.StarterPack,
            //EPack.MiniPack,
            //EPack.VIPPack,
        };
        public bool isCalShowPack = false;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void CalculateShowPack()
        {
            ePackCurrent = ePackCurrentDefault.ToList();
            int length = ePackCurrent.Count;
            for (int i = 0; i < length; i++)
            {
                int index = i;
                var curPack = ePackCurrent[index];
                DB_Pack db = dbPackSO.GetDBPack(curPack);
                SaveShowPack(db.productId);
            }
            for (int i = 0; i < length; i++)
            {
                int index = i;
                ReturnPack(index, 0);
            }
        }
        private void ReturnPack(int index, int indexDequy)
        {
            //if (indexDequy == ePacks.Count + 1)
            //{
            //    if (CurrentLevel < 16)
            //    {
            //        ePackCurrent[index] = EPack.None;
            //        return;
            //    }
            //    List<EPack> listUnder = ePacks.ToList();
            //    for (int i = 0; i <= index; i++)
            //    {
            //        listUnder.Remove(ePackCurrent[i]);
            //    }
            //    listUnder.Remove(EPack.StarterPack);
            //    int count = listUnder.Count;
            //    int ran = UnityEngine.Random.Range(0, count);
            //    ePackCurrent[index] = listUnder[ran];
            //    return;
            //}
            //var curPack = ePackCurrent[index];
            //DB_Pack db = dbPackSO.GetDBPack(curPack);
            //bool isShowLevel = !IsCanShowPack(db);
            //bool isBuyPack = (GetPackPurchaseCount(db.productId) > GetCountPackBuyMinResetIcon);
            //bool isShowCount = (curPack != EPack.StarterPack && GetPackShowCount(db.productId) >= 3);
            //bool isShowBefore = false;
            //int length = ePackCurrent.Count;
            //for (int i = 0; i < index; i++)
            //{
            //    if (ePackCurrent[i] == curPack)
            //    {
            //        isShowBefore = true;
            //        break;
            //    }
            //}

            //string str = "";
            //for (int i = 0; i < ePackCurrent.Count; i++)
            //{
            //    str += $" {ePackCurrent[i]} ";
            //}

            //GameUtil.LogError($"{curPack} : !IsCanShowPack(db) = {!IsCanShowPack(db)} \n " +
            //    $"GetPackPurchaseCount(db.productId) = {GetPackPurchaseCount(db.productId)} : GetCountPackBuyMin = {GetCountPackBuyMin} => isBuyPack = {isBuyPack}\n " +
            //    $"GetPackShowCount(db.productId) = {GetPackShowCount(db.productId)} => isShowCount = {isShowCount}\n" +
            //    $" all = {str}");



            //if (isShowLevel || isBuyPack || isShowCount || isShowBefore)
            //{
            //    ReplacePack(index);
            //    ReturnPack(index, indexDequy + 1);
            //}
        }

        public void ReplacePack(int index)
        {
            var curPack = ePackCurrent[index];
            var nextPack = GetNextPack(curPack);
            ePackCurrent[index] = nextPack;

            DB_Pack db = dbPackSO.GetDBPack(nextPack);
            SaveShowPack(db.productId);

            DB_Pack dbCur = dbPackSO.GetDBPack(curPack);
            ResetShowPack(dbCur.productId);


        }
        private EPack GetNextPack(EPack ePack)
        {
            int length = ePacks.Count;
            int cur = ePacks.IndexOf(ePack);
            int next = (cur + 1) % length;
            return ePacks[next];
        }
        public bool IsCanShowPack(DB_Pack db_Pack)
        {
            bool isShowPack = DataWrapperGame.CurrentLevel >= db_Pack.levelShowIcon;
            bool isBuyPack = (IsHasBuyPack(db_Pack.productId) && db_Pack.maxBuy <= GetPackPurchaseCount(db_Pack.productId));
            bool isStatus = isShowPack && !isBuyPack;
            return isStatus;
        }
        public bool IsHasBuyPack(string productId)
        {
            return GetPackPurchaseCount(productId) > 0;
        }

        public int GetPackPurchaseCount(string productId)
        {
            PackPurchaseData pack = GamePackData.listPack.Find(p => p.productId == productId);
            return pack != null ? pack.count : 0;
            //return IAPManager.GetPurchaseProductCount(productId);
        }

        public void SaveBuyPack(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return;
            }

            // Kiểm tra xem gói đã tồn tại trong danh sách chưa
            PackPurchaseData existingPack = GamePackData.listPack.Find(pack => pack.productId == productId);
            if (existingPack != null)
            {
                existingPack.count++; // Tăng số lần mua nếu đã tồn tại
            }
            else
            {
                GamePackData.listPack.Add(new PackPurchaseData(productId, 1)); // Thêm gói mới với số lần mua là 1
            }

            SaveGameData();
        }

        public int GetPackShowCount(string productId)
        {
            AddBuyPack();
            PackPurchaseData pack = GamePackData.listShowPack.Find(p => p.productId == productId);
            return pack != null ? pack.count : 0;
        }
        public int GetCountPackBuyMinResetIcon
        {
            get
            {
                int min = GetCountPackBuyMin;
                int countMin = 0;
                int length = GamePackData.listPack.Count;
                for (int i = 0; i < length; i++)
                {
                    var pack = GamePackData.listPack[i];
                    DB_Pack db = dbPackSO.GetDBPack(pack.productId);
                    if (db == null)
                    {
                        continue;
                    }
                    EPack ePack = db.ePack;
                    if (!IsEPackNeedGet(ePack))
                    {
                        continue;
                    }
                    if (min == pack.count)
                    {
                        countMin++;
                    }
                }
                if (countMin <= 3)
                {
                    min++;
                }
                return min;
            }
        }
        public int GetCountPackBuyMin
        {
            get
            {
                AddBuyPack();
                int length = GamePackData.listPack.Count;
                int min = int.MaxValue;
                for (int i = 0; i < length; i++)
                {
                    var pack = GamePackData.listPack[i];
                    DB_Pack db = dbPackSO.GetDBPack(pack.productId);
                    if (db == null)
                    {
                        continue;
                    }
                    EPack ePack = db.ePack;
                    if (!IsEPackNeedGet(ePack))
                    {
                        continue;
                    }

                    if (min > pack.count)
                    {
                        min = pack.count;
                    }
                }
                return min;
            }
        }

        public void SaveShowPack(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return;
            }

            // Kiểm tra xem gói đã tồn tại trong danh sách chưa
            PackPurchaseData existingPack = GamePackData.listShowPack.Find(pack => pack.productId == productId);
            if (existingPack != null)
            {
                existingPack.count++; // Tăng số lần mua nếu đã tồn tại
            }
            else
            {
                GamePackData.listShowPack.Add(new PackPurchaseData(productId, 1)); // Thêm gói mới với số lần mua là 1
            }

            SaveGameData();
        }
        public int GetCountPackShowMin
        {
            get
            {
                AddShowPack();
                int length = GamePackData.listShowPack.Count;
                string productIdMin = "";
                int min = int.MaxValue;
                for (int i = 0; i < length; i++)
                {
                    var pack = GamePackData.listShowPack[i];
                    EPack ePack = dbPackSO.GetDBPack(pack.productId).ePack;
                    if (!IsEPackNeedGet(ePack))
                    {
                        continue;
                    }

                    if (min > pack.count)
                    {
                        productIdMin = pack.productId;
                        min = pack.count;
                    }
                }
                return min;
            }
        }

        public bool IsEPackNeedGet(EPack ePack)
        {
            //if (/*ePack == EPack.StarterPack ||*/
            //    ePack == EPack.MiniPack ||
            //    ePack == EPack.LargePack ||
            //    ePack == EPack.SuperPack ||
            //    ePack == EPack.LifeAndCoinPack ||
            //    ePack == EPack.VIPPack)
            //{
            //    return true;
            //}
            return false;
        }
        public void AddShowPack()
        {
            int length = dbPackSO.dbPack.Length;
            int lengthList = GamePackData.listShowPack.Count;
            if (length == lengthList)
            {
                return;
            }
            for (int i = 0; i < length; i++)
            {
                var db = dbPackSO.dbPack[i];

                PackPurchaseData existingPack = GamePackData.listShowPack.Find(pack => pack.productId == db.productId);
                if (existingPack != null)
                {
                    continue;
                }
                else
                {
                    GamePackData.listShowPack.Add(new PackPurchaseData(db.productId, 0));
                }
            }
        }
        private void AddBuyPack()
        {
            int length = dbPackSO.dbPack.Length;
            int lengthList = GamePackData.listPack.Count;
            if (length == lengthList)
            {
                return;
            }
            for (int i = 0; i < length; i++)
            {
                var db = dbPackSO.dbPack[i];

                PackPurchaseData existingPack = GamePackData.listPack.Find(pack => pack.productId == db.productId);
                if (existingPack != null)
                {
                    continue;
                }
                else
                {
                    GamePackData.listPack.Add(new PackPurchaseData(db.productId, 0));
                }
            }
        }
        public void ResetShowPack(string productId)
        {
            PackPurchaseData existingPack = GamePackData.listShowPack.Find(pack => pack.productId == productId);
            if (existingPack != null)
            {
                existingPack.count = 0; // Tăng số lần mua nếu đã tồn tại
            }
            else
            {
                GamePackData.listShowPack.Add(new PackPurchaseData(productId, 0)); // Thêm gói mới với số lần mua là 1
            }
        }

        public bool IsCanShowPack(EPack ePack)
        {
            DB_Pack db_Pack = new DB_Pack();
            int countPack = 0;
            if(ePack == EPack.RemoveAds || ePack == EPack.RemoveAdsBundle)
            {
                var db_Pack_remveads = dbPackSO.GetDBPack(EPack.RemoveAds);
                int removeads = GetPackPurchaseCount(db_Pack_remveads.productId);

                var db_Pack_remveadsbundle = dbPackSO.GetDBPack(EPack.RemoveAdsBundle);
                int removeadsbundle = GetPackPurchaseCount(db_Pack_remveadsbundle.productId);

                if(removeads >= removeadsbundle)
                {
                    db_Pack = db_Pack_remveads;
                    countPack = removeads;
                }
                else
                {
                    db_Pack = db_Pack_remveadsbundle;
                    countPack = removeadsbundle;
                }

            }
            else
            {
                db_Pack = dbPackSO.GetDBPack(ePack);
                countPack = GetPackPurchaseCount(db_Pack.productId);
            }

            return countPack < db_Pack.maxBuy;
        }

    }
    [System.Serializable]
    public class GamePackData
    {
        // Sử dụng List thay cho Dictionary

        public List<PackPurchaseData> listPack = new List<PackPurchaseData>();
        public List<PackPurchaseData> listShowPack = new List<PackPurchaseData>();
    }
    [System.Serializable]
    public class PackPurchaseData
    {
        public string productId;
        public int count;

        public PackPurchaseData(string productId, int count)
        {
            this.productId = productId;
            this.count = count;
        }
    }
    [System.Serializable]
    public class DB_Pack
    {
        public EPack ePack;
        public string productId;
        public bool isRemoveAds = false;
        public GroupDataResources gift;
        public GroupDataResources resourcesOther;
        public string where;

        // Phương thức để tự động lấy tất cả các giá trị string từ PackWhere
        //private static IEnumerable<string> GetPackWhereValues()
        //{
        //    foreach (FieldInfo field in typeof(PackWhere).GetFields(BindingFlags.Static | BindingFlags.Public))
        //    {
        //        if (field.FieldType == typeof(string))
        //        {
        //            yield return (string)field.GetValue(null);
        //        }
        //    }
        //}


        [Header("Condition")]
        public int maxBuy = 1000000000;
        public int levelShowIcon = 16;
        public long timeStartShow = -1;
        public long timeEndShow = -1;
    }
    public static class PackWhere
    {
        public static string BattlePass = "BattlePass";
        public static string Ticket = "Ticket";
        public static string Shop = "Shop";
        public static string Home = "Home";
    }
    public enum EPack
    {
        None = -1,
        RemoveAds = 1,
        RemoveAdsBundle = 2,
        Gold1 = 3,
        Gold2 = 4,
        Gold3 = 5,
        Gold4 = 6,
        Gold5 = 7,
        Gold6 = 8,
        StarterPack = 9,
        Treasure = 10,
        Treasure1 = 11,
        Treasure2 = 12,
        Recovery = 13,
        Booster = 14,
        BattlePass = 15,
    }
}
