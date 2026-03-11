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
    private readonly string[] thinkingMessages = {
    // --- Analytical & Technical ---
    "Analyzing dataset", "Processing neural weights", "Calculating probability",
    "Optimizing logic gate", "Parsing binary streams", "Querying central memory",
    "Executing subroutines", "Running diagnostics", "Cross-referencing indices",
    "Compiling runtime variables", "Validating input parameters",

    // --- Detective & Mystery ---
    "Connecting the dots", "Reviewing hidden evidence", "Reconstructing events",
    "Decoding encrypted logs", "Searching for potential leads", "Analyzing witness statements",
    "Cross-checking alibis", "Uncovering structural anomalies", "Mapping timelines",
    "Evaluating forensic output", "Decrypting legacy data",

    // --- Creative & Philosophical ---
    "Gathering abstract thoughts", "Formulating conceptual response", "Brainstorming possibilities",
    "Deep diving into core memory", "Filtering signal from noise", "Synthesizing creative output",
    "Weighing ethical constraints", "Drafting initial draft", "Refining tone of voice",
    "Searching through archives", "Simulating hypothetical scenarios", "Structuring narrative flow"
};

    public void SetThinking()
    {
        tw?.Kill(true);
        if (!gameObject) return;

        Sequence s = DOTween.Sequence();
        tw = s;

        // Build sẵn vài vòng thay vì append runtime
        for (int round = 0; round < 20; round++)
        {
            string msg = thinkingMessages[Random.Range(0, thinkingMessages.Length)];
            int blinkCount = Random.Range(2, 7);

            // Gõ text chính
            s.AppendCallback(() =>
            {
                if (inputField) inputField.text = msg;
                if (txt) txt.text = msg;
            });

            // Nháy dấu chấm
            for (int i = 0; i < blinkCount*3; i++)
            {
                string dots = new string('.', (i % 3) + 1);
                s.AppendCallback(() =>
                {
                    if (inputField) inputField.text = msg + dots;
                    if (txt) txt.text = msg + dots;
                });
                s.AppendInterval(0.3f);
            }

            // Pause giữa các câu
            s.AppendInterval(Random.Range(0.5f, 1.5f));
        }

        s.SetLoops(-1);
        s.SetId(this);
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
