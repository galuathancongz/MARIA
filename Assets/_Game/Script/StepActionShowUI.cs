using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepActionShowUI : StepAction
{
    public bool isHideAll = false;
    public UIName uiNameHide;
    public UIName uiNameShow;
    public override void Execute(Action<ActionResult> _onComplete)
    {
        base.Execute(_onComplete);
        if(isHideAll)
        {
            UIManager.Instance.HideAllUiActive();
        }
        else if (uiNameHide != UIName.None)
        {
            UIManager.Instance.HideUiActive(uiNameHide);
        }
        if (uiNameShow != UIName.None)
        {
            UIManager.Instance.ShowUI(uiNameShow);
        }
    }
}
