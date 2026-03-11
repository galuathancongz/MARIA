// File: StepAction.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class StepAction : MonoBehaviour
{
    [SerializeField] protected List<GameObject> _targetStepAction = new List<GameObject>();
    public ActionResultType actionResultType = ActionResultType.NextStep;
    public ActionOnCompleteStep actionOnCompleteStep = ActionOnCompleteStep.None;
    public Action<ActionResult> onComplete;
    public List<GameObject> TargetStepAction
    {
        get
        {
            if (_targetStepAction == null || _targetStepAction.Count == 0)
            {
                _targetStepAction = new List<GameObject>();
            }
            if(!_targetStepAction.Contains(this.gameObject))
                _targetStepAction.Add(this.gameObject);
            return _targetStepAction;
        }
    }
    public virtual void Initialize()
    {
        SetActiveTarget(false);
    }
    public virtual void PreExcute()
    {
        SetActiveTarget(true);
    }
    private void SetActiveTarget(bool isActive)
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
    public virtual void CallOnComplete()
    {
        onComplete?.Invoke(new ActionResult(actionResultType));
        onComplete = null;
        switch (actionOnCompleteStep)
        {
            case ActionOnCompleteStep.Enable:
                SetActiveTarget(true);
                break;
            case ActionOnCompleteStep.Disable:
                SetActiveTarget(false);
                break;
        }
    }

    private void OnValidate()
    {
        if (!name.Contains("Step"))
        {
            name = this.GetType().Name;
        }
    }
}
[Serializable]
public enum ActionOnCompleteStep
{
    None = 0,
    Enable = 1,
    Disable = 2,
}