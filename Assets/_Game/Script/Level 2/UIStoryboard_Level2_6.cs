using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class UIStoryboard_Level2_6 : Storyboard
    {
        public void OnClickMainMenu()
        {
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.MainMenu);
        }
        public void OnClickNextScenario()
        {
            UIManager.Instance.ShowNextScenario();
        }
    }
}
