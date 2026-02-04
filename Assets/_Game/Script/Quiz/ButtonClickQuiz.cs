using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ButtonClickQuiz : MonoBehaviour
{
    public string str;
    public bool isChange = false;
    public int index;
    public UnityEvent<int> onClick;
    public void OnClick()
    {
        onClick.Invoke(index);
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
