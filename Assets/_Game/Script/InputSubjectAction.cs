// File: InputSubjectAction.cs
using DG.Tweening;
using Luzart;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputSubjectAction : StepAction
{
    [Header("Subject Input UI Components")]
    public TMP_InputField inputField;     // Sử dụng InputField cho môn học
    public Button btnConfirm;
    public TMP_Text txtInfor;
    [TextArea] public string strInfor;
    public float timeDuration = 1f;
    public Ease easing;

    private Tween _typingTween;

    private void Start()
    {
        // Gán listener cho nút xác nhận
        GameUtil.ButtonOnClick(btnConfirm, OnConfirm);
        inputField.onEndEdit.RemoveAllListeners();
        inputField.onEndEdit.AddListener(text =>
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnConfirm();
            }
        });
    }

    public override void Execute(Action<ActionResult> _onComplete)
    {
        base.Execute(_onComplete);
        // Hiển thị panel và xóa text cũ
        _typingTween = txtInfor.DOText(strInfor, timeDuration).SetEase(easing);
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    public void OnConfirm()
    {
        if (_typingTween != null && _typingTween.IsActive() && _typingTween.IsPlaying())
        {
            // Nếu đang gõ, hoàn tất ngay
            _typingTween.Complete();
            return;
        }
        string subject = inputField.text.Trim();
        if (string.IsNullOrEmpty(subject))
        {
            var ui = UIManager.Instance.ShowUI<UINoti>(UIName.Noti);
            ui.InitPopupFillName();
            return;
        }
        gameObject.SetActive(isSetActiveAfter);
        DataManager.Instance.GameData.subjectName = subject.ToUpper();
        onComplete?.Invoke(new ActionResult(actionResultType));
    }
}
