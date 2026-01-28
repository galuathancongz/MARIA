using DG.Tweening;
using Luzart;
using TMPro;
using UnityEngine;

public class Level2_ConversationItem : MonoBehaviour
{
    public TMP_Text txt;
    private float displaySpeedChar = 75f;
    private Tween tw;
    public void ShowText(string str)
    {
        tw?.Kill();
        if (!gameObject)
        {
            return;
        }
        gameObject.SetActive(true);
        txt.text = str;
    }
    public void ShowTextAnim(string str)
    {
        tw?.Kill();
        if (!gameObject)
        {
            return;
        }
        gameObject.SetActive(true);
        txt.DOSetTextCharByChar(str, displaySpeedChar);
    }
    public void SetThinking()
    {
        tw?.Kill(true);
        if (!gameObject)
        {
            return;
        }
        txt.text = "";
        tw = txt.DOText("Thinking ...",3f).SetLoops(-1, LoopType.Restart);
    }
    public void SetLoading()
    {
        tw?.Kill(true);
        if (!gameObject)
        {
            return;
        }
        txt.text = "";
        tw = txt.DOText("Loading ...", 3f).SetLoops(-1, LoopType.Restart);
    }

}
