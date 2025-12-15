// File: StepAction.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class StepAction : MonoBehaviour
{
    [SerializeField] protected List<GameObject> _targetStepAction = new List<GameObject>();
    public ActionResultType actionResultType = ActionResultType.NextStep;
    public Action<ActionResult> onComplete;
    public bool isSetActiveAfter = false;
    public List<GameObject> TargetStepAction
    {
        get
        {
            if(_targetStepAction == null || _targetStepAction.Count == 0)
            {
                _targetStepAction = new List<GameObject>();
                _targetStepAction.Add(this.gameObject);
            }
            return _targetStepAction;
        }
    }
    public void SetActiveTarget(bool isActive)
    {
        if (TargetStepAction != null)
        {
            for (int i = 0; i < TargetStepAction.Count; i++)
            {
                if (TargetStepAction[i] != null)
                {
                    TargetStepAction[i].SetActive(isActive);
                }
            }
        }
    }
    public virtual void Execute(Action<ActionResult> _onComplete)
    {
        this.onComplete = _onComplete;
    }
}
