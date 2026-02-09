using Luzart;
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
    [SerializeField]
    [ReadOnly]
    private TMP_Text txt;
    [SerializeField]
    [ReadOnly]
    private BaseToggle baseToggle;
    public bool IsSelect
    {
        get
        {
            if (baseToggle)
            {
                return baseToggle.IsSelect;
            }
            else
            {
                return false;
            }
        }
    }
    public void OnClick()
    {
        onClick.Invoke(index);
    }
    public void SetText(string str)
    {
        if (txt)
        {
            txt.text = str;
        }
    }
    public void Select(bool isSelect)
    {
        if (baseToggle)
        {
            baseToggle.Select(isSelect);
        }
    }
    private void OnValidate()
    {
        index = transform.GetSiblingIndex();
        txt = transform.GetComponentInChildren<TMP_Text>();
        baseToggle = transform.GetComponentInChildren<BaseToggle>();
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
