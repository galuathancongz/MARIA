// File: DialogueAction.cs
using System;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Luzart;

public class DialogueAction : StepAction
{
    [Header("Dialogue Settings")]
    public TMP_Text txt;                  // TextMeshPro component
    [TextArea] public string str;         // Nội dung thoại
    public float timeDuration = 1f;       // Thời gian gõ chữ
    public Ease ease = Ease.Linear;       // Kiểu easing cho animate
    public Button btnClick;
    public bool isActiveButton = true;

    private void Start()
    {
        GameUtil.ButtonOnClick(btnClick, OnClickAction);
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
        btnClick.gameObject.SetActive(true);
    }

    public void OnClickAction()
    {
        if (_typingTween != null && _typingTween.IsActive() && _typingTween.IsPlaying())
        {
            // Nếu đang gõ, hoàn tất ngay
            _typingTween.Complete();
            return;
        }
        gameObject.SetActive(isSetActiveAfter);
        onComplete?.Invoke(new ActionResult(actionResultType));
    }
}