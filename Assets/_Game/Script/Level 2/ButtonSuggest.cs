using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonSuggest : MonoBehaviour
{
    public TMP_Text txt;
    public string strSuggest;
    [SerializeField] private Button btn;
    public UnityEvent<string> OnClickHandle;
    public void OnClick()
    {
        OnClickHandle?.Invoke(strSuggest);
    }
    private void OnValidate()
    {
        txt ??= GetComponentInChildren<TMP_Text>();
        if(txt != null)
            txt.text = strSuggest;
        if(btn == null)
        {
            btn = GetComponent<UnityEngine.UI.Button>();    
            btn.SetPersistentOnClick(this,nameof(OnClick));
        }

    }
}
