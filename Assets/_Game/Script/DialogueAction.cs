// File: DialogueAction.cs
using System;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Luzart;
using UnityEditor;

public class DialogueAction : StepAction
{
    [Header("Dialogue Settings")]
    public TMP_Text txt;                  // TextMeshPro component
    [TextArea] public string str;         // Nội dung thoại
    public float timeDuration = 1f;       // Thời gian gõ chữ
    public Ease ease = Ease.Linear;       // Kiểu easing cho animate
    public Button btnClick;
    public bool isCompleteAndNextAction = false; // Có tự động chuyển sang action tiếp theo sau khi hoàn thành gõ text không
    [SerializeField] private Mode modeClick = Mode.Button;
    enum Mode
    {
        Button = 0,
        Mouse = 1
    }

    private void Start()
    {
        if(modeClick == Mode.Button)
        {
            GameUtil.ButtonOnClick(btnClick, OnClickAction);
        }
        txt.text = string.Empty;
    }
    private void OnEnable()
    {
        txt.text = string.Empty;
    }
    public override void PreExcute()
    {
        base.PreExcute();
        txt.text = string.Empty;

    }
    protected Tween _typingTween;

    public override void Execute(Action<ActionResult> _onComplete)
    {
        base.Execute(_onComplete);
        // Reset lại text
        txt.text = string.Empty;

        // Bắt đầu gõ text với DOTween
        _typingTween = txt.DOText(str, timeDuration)
            .SetEase(ease);
        if(btnClick != null)
            btnClick.gameObject.SetActive(true);
    }

    public void OnClickAction()
    {
        if (_typingTween != null && _typingTween.IsActive() && _typingTween.IsPlaying())
        {
            // Nếu đang gõ, hoàn tất ngay
            _typingTween.Complete();
            if (isCompleteAndNextAction)
            {
                OnActionInvoke();
            }
            return;
        }
        OnActionInvoke();
    }
    private void OnActionInvoke()
    {
        gameObject.SetActive(isSetActiveAfter);
        onComplete?.Invoke(new ActionResult(actionResultType));
    }
    private void Update()
    {
        if(modeClick == Mode.Mouse)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnClickAction();
            }
        }
    }
}