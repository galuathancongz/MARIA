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
        str = LocalizationManager.Instance.GetPrompt("prompts.level2_2_1_question", new System.Collections.Generic.Dictionary<string, string> { {"question", str} });
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
