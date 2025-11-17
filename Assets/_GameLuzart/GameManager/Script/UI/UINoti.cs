using DG.Tweening;
using Luzart;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UINoti : UIBase
{
    public TMP_Text txtContent;

    public void InitPopup(string strContent)
    {
        this.txtContent.DOText(strContent,1f);
    }
    private void OnDisable()
    {
        this.DOKill(true);
    }
    public string strFillName = "Please fill name Subjects";
    public void InitPopupFillName()
    {
        InitPopup(strFillName);
    }
}
