using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonQuiz : MonoBehaviour
{
    public BaseSelect bsQuiz;
    public UnityEvent onDoneClickTrue;
    public UnityEvent onDoneClickFalse;
    public bool isTrue;

    // Track correct answers across tutorial quiz questions (2 needed for QuizAce)
    private static int s_correctAnswers = 0;
    public static void ResetQuizAceTracker() { s_correctAnswers = 0; }

    private void Start()
    {
        bsQuiz.Select(0);
    }
    public void OnClickButton()
    {
        if (isTrue)
        {
            bsQuiz.Select(1);
            s_correctAnswers++;
            if (s_correctAnswers >= 2)
                Luzart.SkillManager.Instance?.UnlockSkill(Luzart.ESkillId.QuizAce);
            onDoneClickTrue.Invoke();
        }
        else
        {
            bsQuiz.Select(2);
            onDoneClickFalse.Invoke();
        }
    }
}
