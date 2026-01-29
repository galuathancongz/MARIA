using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CallSelectSubject : MonoBehaviour
{
    [SerializeField] int level = 2;
    [SerializeField]
    private UnityEvent<int> onCall;
    private void OnEnable()
    {
        int index = 0;
        if (level == 2)
        {
            index = (int)Level2Manager.Instance.Data.subject;
        }else if(level == 3)
        {
            index = (int)Level3Manager.Instance.Data.subject;
        }
        onCall?.Invoke(index);
    }
    [Sirenix.OdinInspector.Button]
    void CallEditor(int index)
    {
        onCall?.Invoke(index);
    }
}
