using DG.Tweening;
using Luzart;
using TMPro;
using UnityEngine;

public class Level2_ConversationItem : MonoBehaviour
{
    public TMP_InputField inputField;
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
        if(inputField)
        {
            inputField.text = str;
        }
        if(txt)
        {
            txt.text = str;
        }
    }
    public void ShowTextAnim(string str)
    {
        tw?.Kill();
        if (!gameObject)
        {
            return;
        }
        gameObject.SetActive(true);
        if(inputField)
        {
            tw = inputField.textComponent.DOSetTextCharByChar(str, displaySpeedChar).SetId(this);
        }
        if(txt)
        {
            tw = txt.DOSetTextCharByChar(str, displaySpeedChar).SetId(this);
        }
    }
    public void SetThinking()
    {
        tw?.Kill(true);
        if (!gameObject)
        {
            return;
        }
        if(inputField)
        {
            inputField.text = "";
            tw = inputField.textComponent.DOText("Thinking ...", 3f).SetLoops(-1, LoopType.Restart).SetId(this);
        }
        if(txt)
        {   
            txt.text = "";
            tw = txt.DOText("Thinking ...",3f).SetLoops(-1, LoopType.Restart).SetId(this);
        }
    }
    public void SetLoading()
    {
        tw?.Kill(true);
        if (!gameObject)
        {
            return;
        }
        if(inputField)
        {
            inputField.text = "";
            tw = inputField.textComponent.DOText("Loading ...", 3f).SetLoops(-1, LoopType.Restart).SetId(this);
        }
        if(txt)
        {
            txt.text = "";
            tw = txt.DOText("Loading ...", 3f).SetLoops(-1, LoopType.Restart).SetId(this);
        }
    }
    private void OnDestroy()
    {
        tw?.Kill(true);
    }
}
