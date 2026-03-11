using DG.Tweening;
using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogActionTeacher : DialogueAction
{
    protected override void StartTyping()
    {
        // Reset lại text
        txt.text = string.Empty;

        // Bắt đầu gõ text với DOTween
        _typingTween = txt.DOText($"Great! A {DataManager.Instance.Data.subjectName} teacher, wonderful",
            timeDuration)
            .SetEase(ease);
    }
}
