using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemChoiceLevel1Scene5 : MonoBehaviour
{
    public string strStory;
    public bool IsChoice => bsChoice.IsSelect;
    public BaseToggle bsChoice;

    private void Reset()
    {
        bsChoice = GetComponentInChildren<BaseToggle>();
        strStory = GetComponentInChildren<TMPro.TMP_Text>().text;
    }
    private void OnValidate()
    {
        GetComponentInChildren<TMPro.TMP_Text>().text = strStory;
    }
}
