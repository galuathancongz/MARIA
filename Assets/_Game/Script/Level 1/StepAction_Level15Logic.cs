using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Luzart
{
    public class StepAction_Level1Scene5Logic : StepAction
    {
        public override void Execute(Action<ActionResult> _onComplete)
        {
            base.Execute(_onComplete);
            DataManager.Instance.GameData.level = 2;
        }
    }
}
