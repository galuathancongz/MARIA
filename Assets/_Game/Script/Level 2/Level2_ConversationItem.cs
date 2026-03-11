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
    private void Awake()
    {
        if(txt && !inputField)
        {
            inputField = txt.GetComponent<TMP_InputField>();
        }
    }
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
        if (txt && !inputField)
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
            tw = inputField.DOSetTextCharByChar(str, displaySpeedChar).SetId(this);
        }
        if (txt && !inputField)
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
            tw = inputField.DOText("Thinking ...", 3f).SetLoops(-1, LoopType.Restart).SetId(this);
        }
        if (txt && !inputField)
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
            tw = inputField.DOText("Loading ...", 3f).SetLoops(-1, LoopType.Restart).SetId(this);
        }
        if (txt && !inputField)
        {
            txt.text = "";
            tw = txt.DOText("Loading ...", 3f).SetLoops(-1, LoopType.Restart).SetId(this);
        }
    }
    [ContextMenu("Setup InputField")]
    public void SetupInputField()
    {
        if(txt && !inputField)
        {
            inputField = txt.GetComponent<TMP_InputField>();
        }
        if(!inputField)
        {
            inputField = txt.gameObject.AddComponent<TMP_InputField>();
        }
        SetupInput(inputField);
    }
    public void SetupInput(TMP_InputField inputField)
    {
        var textComponent = inputField.gameObject.GetComponent<TMP_Text>();
        textComponent.raycastTarget = true;
        string str = textComponent.text;
        inputField.textComponent = textComponent;
        inputField.pointSize = textComponent.fontSize;
        inputField.lineType = TMP_InputField.LineType.MultiLineSubmit;
        inputField.fontAsset = textComponent.font;
        inputField.readOnly = true;
        inputField.transition = UnityEngine.UI.Selectable.Transition.None;
        inputField.text = str;
    }
    private void OnDestroy()
    {
        tw?.Kill(true);
    }
}
