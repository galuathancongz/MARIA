using DG.Tweening;
using Eco.TweenAnimation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenAnimationStepAction : StepAction
{
    public TweenAnimation[] twAnimation;
    public override void Execute(Action<ActionResult> _onComplete)
    {
        base.Execute(_onComplete);
        int length = twAnimation.Length;
        float time = 0f;
        for (int i = 0; i < length; i++)
        {
            twAnimation[i].Show();
            if(time < twAnimation[i].BaseOptions.Duration)
            {
                time = twAnimation[i].BaseOptions.Duration;
            }
            Debug.Log($"Call {twAnimation[i].Vector3Options.From} to {twAnimation[i].Vector3Options.EndTo}");
        }
        DOVirtual.DelayedCall(time, ()=> Call());
    }
    public void Call()
    {
        onComplete?.Invoke(new ActionResult(actionResultType));
    }
}
