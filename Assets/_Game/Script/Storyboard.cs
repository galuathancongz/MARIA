// File: Storyboard.cs
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace Luzart
{
    public class Storyboard : UIBase
    {
        public List<StepAction> Steps;

        public int _currentStep;
        [ReadOnly][SerializeField] int currentStoryboard = 0;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            InitStoryBoard(0);
            StartStoryboard(null);
        }
        public void InitStoryBoard(int index)
        {
            InitStepAction();
            this.currentStoryboard = index;
        }
        // Disable all steps at the beginning
        private void InitStepAction()
        {
            for (int i = 0; i < Steps.Count; i++)
            {
                if (Steps[i] != null)
                {
                    Steps[i].Initialize();
                }
            }
        }
        public Action<int> onDoneStoryBoard;
        public void StartStoryboard(Action<int> onComplete)
        {
            InitStepAction();
            _currentStep = 0;
            this.onDoneStoryBoard = onComplete;
            ExecuteCurrentStep(onComplete);
        }

        private void ExecuteCurrentStep(Action<int> onComplete)
        {
            if (_currentStep < 0 || _currentStep >= Steps.Count)
            {
                //Hide();
                UIManager.Instance.ShowNextScenario();
                //onDoneStoryBoard?.Invoke(currentStoryboard);
                return;
            }
            if (_currentStep <= Steps.Count - 1)
            {
                Steps[_currentStep].PreExcute();
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
        [ContextMenu("ClearNullInSteps")]
        public void ClearNullInSteps()
        {
            Steps.RemoveAll(item => item == null);
        }
        [ContextMenu("Setup All Action Step")]
        public void SetupActionStep()
        {
            Undo.RecordObject(this, "Setup Action Step");
            Steps = gameObject.GetComponentsInChildren<StepAction>(true).ToList();
            EditorUtility.SetDirty(this);
        }
    }
}
