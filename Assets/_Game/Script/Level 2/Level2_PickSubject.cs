using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class Level2_PickSubject : MonoBehaviour
    {
        [SerializeField] private ESubject subject;
        public void OnClickSubject()
        {
            DataManager.Instance.GameData.subject = subject;
        }
    }
    public enum ESubject
    {
        History = 0,
        Science = 1,
        English = 2,
        Math = 3    
    }
}
