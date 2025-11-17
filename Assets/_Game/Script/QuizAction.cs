// File: QuizAction.cs
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class QuizOption
{
    public string text;                           // Nội dung lựa chọn
    public ActionResultType actionResultType;     // NextStep hoặc RepeatStep
    public List<StepAction> followupActions;      // Các action thực thi sau khi chọn
}

[RequireComponent(typeof(CanvasGroup))]
public class QuizAction : StepAction
{
    [Header("Quiz UI Components")]
    public GameObject quizPanel;
    public TMP_Text questionTxt;
    public string questionText;
    public QuizOption[] options;                  // Kéo-thả cấu hình ở Inspector

    private QuizOption _selectedOption;

    public override void Execute(Action<ActionResult> _onComplete)
    {
        base.Execute(_onComplete);
        quizPanel.SetActive(true);
        questionTxt.text = questionText;
    }

    public void OnOptionSelected(QuizOption opt)
    {
        _selectedOption = opt;

        if (opt.followupActions != null && opt.followupActions.Count > 0)
        {
            ExecuteFollowupActions(0);
        }
        else
        {
            CompleteOption();
        }
    }

    private void ExecuteFollowupActions(int index)
    {
        if (index >= _selectedOption.followupActions.Count)
        {
            CompleteOption();
            return;
        }

        var action = _selectedOption.followupActions[index];
        action.Execute(result =>
        {
            // ignore result of followups, tiếp tới action sau
            ExecuteFollowupActions(index + 1);
        });
    }

    private void CompleteOption()
    {
        // Gọi callback storyboard
        onComplete?.Invoke(new ActionResult(_selectedOption.actionResultType));
    }
}
