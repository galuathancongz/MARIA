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

        // Sử dụng AppendCallback để mỗi vòng lặp lại chọn câu và số lần nháy mới
        s.AppendCallback(() =>
        {
            string msg = thinkingMessages[Random.Range(0, thinkingMessages.Length)];

            // 1. Gõ text chính trước
            if (inputField) inputField.text = msg;
            if (txt) txt.text = msg;

            // 2. Random số lần nháy (ví dụ từ 2 đến 6 lần)
            int randomBlinkCount = Random.Range(2, 7);

            // Tạo một sequence con cho phần dấu chấm
            Sequence dotSeq = DOTween.Sequence();
            for (int i = 0; i < randomBlinkCount; i++)
            {
                // Nháy các dấu chấm . -> .. -> ... -> . -> .. -> ...
                int dotCount = (i % 3) + 1;
                string dots = new string('.', dotCount);

                dotSeq.AppendCallback(() => {
                    if (inputField) inputField.text = msg + dots;
                    if (txt) txt.text = msg + dots;
                });
                // Random tốc độ nháy một chút để cảm giác không bị đều tăm tắp
                dotSeq.AppendInterval(0.3f);
            }

            // 3. Random thời gian "đứng hình" sau khi nháy xong trước khi đổi câu
            dotSeq.AppendInterval(Random.Range(0.5f, 1.5f));

            s.Append(dotSeq);
        });

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
