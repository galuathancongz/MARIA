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
    private void Start()
    {
        bsQuiz.Select(0);
    }
    public void OnClickButton()
    {
        if (isTrue)
        {
            bsQuiz.Select(1);
            onDoneClickTrue.Invoke();
        }
        else
        {
            bsQuiz.Select(2);
            onDoneClickFalse.Invoke();
        }
    }
}
