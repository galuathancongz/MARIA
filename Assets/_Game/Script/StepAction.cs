// File: StepAction.cs
using System;
using UnityEngine;

public class StepAction : MonoBehaviour
{
    public ActionResultType actionResultType = ActionResultType.NextStep;
    public Action<ActionResult> onComplete;
    public bool isSetActiveAfter = false;
    public virtual void Execute(Action<ActionResult> _onComplete)
    {
        this.onComplete = _onComplete;
    }
}
