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
        base.Execute(_onComplete);
        unityEvent?.Invoke();
        CallOnComplete();
    }
}
