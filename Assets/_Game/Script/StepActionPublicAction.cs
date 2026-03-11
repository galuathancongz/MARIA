// File: StepAction.cs
using System;
using UnityEngine;

public class StepActionPublicAction : StepAction
{
    public virtual void OnClickOnDone()
    {
        CallOnComplete();
    }
    private void OnValidate()
    {
        if(!name.Contains(nameof(StepActionPublicAction)))
            name = nameof(StepActionPublicAction);
    }
}
