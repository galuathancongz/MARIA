using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CallSelectSubject : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<int> onCall;
    private void OnEnable()
    {
        int index = (int)Level2Manager.Instance.Data.subject;
        onCall?.Invoke(index);
    }
    [Sirenix.OdinInspector.Button]
    void CallEditor(int index)
    {
        onCall?.Invoke(index);
    }
}
