using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class UILevelShow : UIBase
    {
        public UIName scenarioShow = UIName.Level1_1;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            UIManager.Instance.ShowScenario(scenarioShow);
        }
    }
}
