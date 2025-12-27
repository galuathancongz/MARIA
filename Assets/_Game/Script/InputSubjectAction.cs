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
        if (!TryDetectSubject(subject))
        {
            var ui = UIManager.Instance.ShowUI<UINoti>(UIName.Noti);
            ui.InitPopupFillName();
            return;
        }
        gameObject.SetActive(isSetActiveAfter);
        DataManager.Instance.GameData.subjectName = subject.ToUpper();
        onComplete?.Invoke(new ActionResult(actionResultType));
    }
    private readonly string[] coreSubjects = {
        // Math
        "math","algebra","geometry","calculus","statistics","probability",
        "trigonometry","logic","number theory","linear algebra","discrete math",

        // Physics
        "physics","mechanics","thermodynamics","optics","electromagnetism",
        "quantum physics","nuclear physics","astrophysics","relativity",

        // Chemistry
        "chemistry","organic chemistry","inorganic chemistry","physical chemistry",
        "analytical chemistry","biochemistry","materials chemistry",

        // Biology
        "biology","cell biology","genetics","microbiology","ecology",
        "evolution","zoology","botany","anatomy","physiology","neuroscience",

        // Materials & Engineering
        "materials","material science","materials science","metallurgy",
        "polymer","ceramics","composites","nanomaterials",
        "mechanical engineering","civil engineering","electrical engineering",
        "electronics","mechatronics",

        // Computer
        "computer","computer science","programming","coding","software",
        "algorithms","data structures","databases","web development",
        "game development","artificial intelligence","machine learning",
        "data science","computer graphics","networking","cyber security",

        // Social
        "history","geography","economics","politics","psychology","philosophy",
        "sociology","law","ethics",

        // Art
        "art","music","drawing","painting","animation","film","photography",

        // Health
        "physical education","sports","fitness","nutrition","medicine",

        // Environment
        "environmental science","geology","climate science"
    };
    private bool TryDetectSubject(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        input = input.Trim().ToLowerInvariant();
        for (int i = 0; i < coreSubjects.Length; i++)
        {
            var subject = coreSubjects[i];

            if (input.Contains(subject))
            {
                return true;
            }
        }

        return false;
    }


}
