using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepActionClick : StepAction
{
    public void OnClickOnDone()
    {
        onComplete?.Invoke(new ActionResult(actionResultType));
        gameObject.SetActive(isSetActiveAfter);
    }
}
