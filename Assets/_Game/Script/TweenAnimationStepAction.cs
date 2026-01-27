using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class TweenAnimationStepAction : StepAction
    {
        private Tween tw;
        public TweenAnimationBase[] twAnimation;
        public override void Execute(Action<ActionResult> _onComplete)
        {
            base.Execute(_onComplete);
            int length = twAnimation.Length;
            float time = 0f;
            for (int i = 0; i < length; i++)
            {
                twAnimation[i].Show();
                ITweenAnimation tweenAnimation = twAnimation[i];
                float duration = tweenAnimation.Settings.Duration;
                if (time < duration)
                {
                    time = duration;
                }
            }
            tw?.Kill(true);
            UIManager.Instance.BlockRaycast(true);
            tw = DOVirtual.DelayedCall(time, () => Call());
        }
        public void Call()
        {
            UIManager.Instance.BlockRaycast(false);
            onComplete?.Invoke(new ActionResult(actionResultType));
        }
        private void OnDisable()
        {
            tw?.Kill(true);
        }

        [ContextMenu("AutoSet")]
        public void AutoSet()
        {
            twAnimation = GetComponents<TweenAnimationBase>();
        }
    }
}
