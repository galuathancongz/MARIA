using Luzart;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupTeaching : MonoBehaviour
{
    public TMP_Text txtTitle;
    public TMP_Text txtMainWord;
    public TMP_Text[] txtWords;

    [Space, Header("Set up")]
    public string strTitle;
    public string strMainWord;
    public string[] strAnswers;

    private void Start()
    {
        LocalizeTexts();
    }
    public void LocalizeTexts()
    {
        if (txtTitle != null) txtTitle.text = Loc.T(strTitle);
        if (txtMainWord != null) txtMainWord.text = Loc.T(strMainWord);
        if (txtWords != null && strAnswers != null)
        {
            for (int i = 0; i < txtWords.Length && i < strAnswers.Length; i++)
            {
                if (txtWords[i] != null && !string.IsNullOrWhiteSpace(strAnswers[i]))
                    txtWords[i].text = Loc.T(strAnswers[i]);
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (txtTitle == null || txtMainWord == null || txtWords == null || strAnswers == null) return;
        if (string.IsNullOrWhiteSpace(txtTitle.text))
        {
            strTitle = txtTitle.text;
        }
        if (string.IsNullOrWhiteSpace(txtMainWord.text))
        {
            strMainWord = txtMainWord.text;
        }
        strAnswers = new string[txtWords.Length];
        for (int i = 0; i < txtWords.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(txtWords[i].text))
            {
                continue;
            }
            strAnswers[i] = txtWords[i].text;
        }
        txtTitle.text = strTitle;
        txtMainWord.text = strMainWord;
        for (int i = 0; i < txtWords.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(strAnswers[i]))
            {
                continue;
            }
            txtWords[i].text = strAnswers[i];
        }
    }
}
