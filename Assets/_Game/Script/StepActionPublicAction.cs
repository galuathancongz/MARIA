// File: StepAction.cs
using System;
using UnityEngine;

public class StepActionPublicAction : StepAction
{
    public virtual void OnClickOnDone()
    {
        onComplete?.Invoke(new ActionResult(actionResultType));
        gameObject.SetActive(isSetActiveAfter);
    }
}
