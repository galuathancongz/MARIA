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
            if (level == 3)
            {
                Level3Manager.IsSendStartCoCreatorStudio = false;
            }
            if(DataManager.Instance.Data.level >= level)
            {
                CallOnComplete();
                SyncManager.Instance?.SaveToServer(saveTrigger: "level_complete");
                return;
            }
            DataManager.Instance.Data.level = level;
            DataManager.Instance.SaveGameData();
            CallOnComplete();
        }
    }
}
