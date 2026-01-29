using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2_Scene2_1_Logic : MonoBehaviour
{
    private bool _isCall = true;
    private void OnEnable()
    {
        _isCall = true;
    }
    public void OnCallMsg(string str)
    {
        str = $"You are an AI Mentor named {MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)}. Your subject is {MentorSubjectExtension.GetSubjectName(Level2Manager.Instance.Data.subject)}. Please answer the following question as concisely as possible. Limit 200 token. Question: {str}";
        Level2Manager.Instance.Send(0,str, (result) =>
        {
            if (!_isCall)
            {
                Observer.Instance.Notify(ObserverKey.OnUpdateNewChat, result);
            }
        });
    }
    private void OnDisable()
    {
        _isCall = false;
    }
}
