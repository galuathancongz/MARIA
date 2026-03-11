using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Luzart
{
    public class StepAction_SetLevelLogic : StepAction
    {
        [SerializeField] private int level = 2;
        public override void Execute(Action<ActionResult> _onComplete)
        {
            base.Execute(_onComplete);
            DataManager.Instance.GameData.level = level;
            DataManager.Instance.SaveGameData();
            CallOnComplete();
            if (level == 3)
            {
                Level3Manager.IsSendStartCoCreatorStudio = false;
            }
        }
    }
}
