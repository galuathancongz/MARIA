namespace Luzart
{
    using DG.Tweening;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class UISplash : UIBase
    {
        public UIName nextUI = UIName.Tutorial;
        public TweenAnimationBase tweenAnimation;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            Sequence sq = DOTween.Sequence();
            sq.Append(tweenAnimation.Show());
            sq.AppendInterval(0.1f);
            sq.AppendCallback(NextScene);
        }
        private void NextScene()
        {
            Hide();
            if (DataManager.Instance.CurrentLevel == 0)
            {
                nextUI = UIName.Tutorial;
            }
            else
            {
                nextUI = UIName.MainMenu;
            }
                UIManager.Instance.ShowUI(nextUI);
        }
    }
}
