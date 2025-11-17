namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    
    [CreateAssetMenu(fileName = "DB_PackSO", menuName = "SO/DB_PackSO")]
    public class DBPackSO : ScriptableObject
    {
        public DB_Pack[] dbPack;
    
        private Dictionary<string, DB_Pack> dictDBPack = new Dictionary<string, DB_Pack>();
        private Dictionary<EPack, DB_Pack> dictDBEPack = new Dictionary<EPack, DB_Pack>();
    
        private void InitDictDBPack()
        {
            if (dictDBPack != null && dictDBPack.Count > 0)
            {
                return;
            }
            int length = dbPack.Length;
            for (int i = 0; i < length; i++)
            {
                var db = dbPack[i];
                dictDBPack.Add(db.productId, db);
            }
        }
        private void InitDictDBEPack()
        {
            if (dictDBEPack != null && dictDBEPack.Count > 0)
            {
                return;
            }
            int length = dbPack.Length;
            for (int i = 0; i < length; i++)
            {
                var db = dbPack[i];
                dictDBEPack.Add(db.ePack, db);
            }
        }
    
        public DB_Pack GetDBPack(string productId)
        {
            InitDictDBPack();
            if (IsHasDBPack(productId))
            {
                return dictDBPack[productId];
            }
            else
            {
                return null;
            }
        }
        public DB_Pack GetDBPack(EPack ePack)
        {
            InitDictDBEPack();
            if (IsHasDBPack(ePack))
            {
                return dictDBEPack[ePack];
            }
            else
            {
                return null;
            }
        }
        public bool IsHasDBPack(string productId)
        {
            return dictDBPack.ContainsKey(productId);
        }
        public bool IsHasDBPack(EPack productId)
        {
            return dictDBEPack.ContainsKey(productId);
        }
        public bool IsHasBuyPack(EPack ePack)
        {
            var db_Pack = GetDBPack(ePack);
            if(db_Pack == null)
            {
                return false;
            }
            return PackManager.Instance.IsHasBuyPack(db_Pack.productId) && db_Pack.maxBuy <= PackManager.Instance.GetPackPurchaseCount(db_Pack.productId);
        }
    }
}
