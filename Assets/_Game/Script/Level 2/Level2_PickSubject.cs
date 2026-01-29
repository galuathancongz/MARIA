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
        [SerializeField] private int level = 2;
        [SerializeField] private ESubject subject;
        public void OnClickSubject()
        {
            if (level == 2)
            {
                Level2Manager.Instance.Data.subject = subject;
            }
            else if (level == 3)
            {
                Level3Manager.Instance.Data.subject = subject;
            }
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
