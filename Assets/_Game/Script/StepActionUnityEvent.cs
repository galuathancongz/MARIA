using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StepActionUnityEvent : StepAction
{
    public UnityEvent unityEvent;
    public override void Execute(Action<ActionResult> _onComplete)
    {
        unityEvent?.Invoke();
        base.Execute(_onComplete);
        onComplete?.Invoke(new ActionResult(ActionResultType.NextStep));
    }
}
