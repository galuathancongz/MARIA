using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepActionShowUI : StepAction
{
    public UIName uiNameHide;
    public UIName uiNameShow;
    public override void Execute(Action<ActionResult> _onComplete)
    {
        base.Execute(_onComplete);
        if (uiNameHide != UIName.None)
        {
            UIManager.Instance.HideUiActive(uiNameHide);
        }
        if (uiNameShow != UIName.None)
        {
            UIManager.Instance.ShowUI(uiNameShow);
        }
    }
}
