// File: Storyboard.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class Storyboard : MonoBehaviour
{
    public List<StepAction> Steps;

    public int _currentStep;
    public int currentStoryboard = 0;
    public void InitStoryBoard(int index)
    {
        gameObject.SetActive(false);
        DisableAllStep();
        this.currentStoryboard = index;
    }
    private void DisableAllStep()
    {
        for (int i = 0; i < Steps.Count; i++)
        {
            if( Steps[i] != null)
            {
                Steps[i].gameObject.SetActive(false);
            }
        }
    }
    public Action<int> onDoneStoryBoard;
    public void StartStoryboard(Action<int> onComplete)
    {
        DisableAllStep();
        _currentStep = 0;
        this.onDoneStoryBoard = onComplete;
        ExecuteCurrentStep(onComplete);
    }

    private void ExecuteCurrentStep(Action<int> onComplete)
    {
        if (_currentStep < 0 || _currentStep >= Steps.Count)
        {
            onDoneStoryBoard?.Invoke(currentStoryboard);
            return;
        }
        if(_currentStep <= Steps.Count - 1)
        {
            Steps[_currentStep].gameObject.SetActive(true);
            Steps[_currentStep].Execute(result => HandleResult(result, onComplete));
        }
    }

    private void HandleResult(ActionResult result, Action<int> onComplete)
    {
        switch (result.Type)
        {
            case ActionResultType.NextStep:
                _currentStep++;
                ExecuteCurrentStep(onComplete);
                break;
            case ActionResultType.RepeatStep:
                ExecuteCurrentStep(onComplete);
                break;
            case ActionResultType.JumpToStepIndex:
                ExecuteCurrentStep(onComplete);
                break;
            default:
                ExecuteCurrentStep(onComplete);
                break;
        }
    }
}