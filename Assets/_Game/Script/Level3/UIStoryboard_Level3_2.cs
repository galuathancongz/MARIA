using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Luzart
{
    public class UIStoryboard_Level3_2 : Storyboard
    {
        public Button btnNext;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            btnNext.gameObject.SetActive(false);
        }
        public void OnClickToTopic()
        {
            btnNext.gameObject.SetActive(true);
        }
    }
}
