using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class Step_NextUIStoryboard : StepAction
    {
        public override void Execute(Action<ActionResult> _onComplete)
        {
            base.Execute(_onComplete);
            UIManager.Instance.ShowNextScenario();
        }
    }
}
