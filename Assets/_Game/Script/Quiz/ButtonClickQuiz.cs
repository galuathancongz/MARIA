using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonClickQuiz : MonoBehaviour
{
    public string str;
    public bool isChange = false;
    public void OnClick()
    {
        
    }
    private void OnValidate()
    {
        var txt = transform.GetComponentInChildren<TMP_Text>();
        if (txt != null)
        {
            if (isChange)
            {
                txt.text = str;
            }
            else
            {
                str = txt.text;
            }
        }
    }
}
