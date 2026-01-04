using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Luzart
{
    public class StepActionSwitch : StepAction
    {
        public List<ListStepAction> listListStepActions = new List<ListStepAction>();
        private int _stepUse;
        [ReadOnly][SerializeField] int _currentStep = 0;
        public List<StepAction> Steps
        {
            get
            {
                if (_stepUse >= 0 && _stepUse < listListStepActions.Count)
                {
                    return listListStepActions[_stepUse].stepActions;
                }
                return new List<StepAction>();
            }
        }
        public void UseStep(int index)
        {
            _stepUse = index;
        }
        public override void Initialize()
        {
            base.Initialize();
            gameObject.SetActive(false);
            InitStepAction();
        }
        public override void Execute(Action<ActionResult> _onComplete)
        {
            base.Execute(_onComplete);
            StartStoryboard(_onComplete);
        }
        // Disable all steps at the beginning
        private void InitStepAction()
        {
            for (int i = 0; i < listListStepActions.Count; i++)
            {
                if (listListStepActions[i] != null && listListStepActions[i].stepActions.Count > 0)
                {
                    for (int j = 0; j < listListStepActions[i].stepActions.Count; j++)
                    {
                        if (listListStepActions[i].stepActions[j] != null)
                        {
                            listListStepActions[i].stepActions[j].Initialize();
                        }
                    }
                }
            }
        }
        private Action<ActionResult> onDoneStoryBoard;
        private void StartStoryboard(Action<ActionResult> onComplete)
        {                                                     
            InitStepAction();
            _currentStep = 0;
            this.onDoneStoryBoard = onComplete;
            ExecuteCurrentStep();
        }

        private void ExecuteCurrentStep()
        {
            if (_stepUse >= listListStepActions.Count)
            {
                onDoneStoryBoard?.Invoke(new ActionResult(actionResultType));
            }
            if (_currentStep < 0 || _currentStep >= Steps.Count)
            {
                onDoneStoryBoard?.Invoke(new ActionResult(actionResultType));
                return;
            }
            if (_currentStep <= Steps.Count - 1)
            {
                Steps[_currentStep].PreExcute();
                Steps[_currentStep].Execute(result => HandleResult(result));
            }
        }

        private void HandleResult(ActionResult result)
        {
            _currentStep++;
            ExecuteCurrentStep();
        }
    }
    [System.Serializable]
    public class ListStepAction
    {
        public List<StepAction> stepActions = new List<StepAction>();
    }
}
