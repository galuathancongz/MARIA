using DG.Tweening;
using Luzart;
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
    public string strFillName => LocalizationManager.Instance.Get("ui.fill_subject");
    public void InitPopupFillName()
    {
        InitPopup(strFillName);
    }
}
