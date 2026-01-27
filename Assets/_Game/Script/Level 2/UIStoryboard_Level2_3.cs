using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class UIStoryboard_Level2_3 : Storyboard
    {
        public void OnSetMessageQuestion(string question)
        {
            Level2Manager.Instance.Data.question2_3_1 = question;
        }
    }
}
