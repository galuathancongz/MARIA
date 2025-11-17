namespace Luzart
{
    using System;
    using UnityEngine;
    #if DOTWEEN
    using DG.Tweening;
    #endif
    
    public class UILoadScene : UIBase
    {
        public Transform transload;
        private Action onLoad, onDone;
        protected override void Setup()
        {
            base.Setup();
        }
    #if DOTWEEN
        Sequence sequence;
    #endif
        public void LoadSceneCloud(Action onLoad, Action onDone, float timeLoad = 2, float timeHide = 1f)
        {
            this.onLoad = onLoad;
            this.onDone = onDone;
    #if DOTWEEN
            sequence?.Kill();
            sequence = DOTween.Sequence();
            sequence.Append(transload.DORotate(new Vector3(0, 0, 720), timeLoad, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            sequence.AppendCallback(() => onLoad?.Invoke());
            sequence.Append(transload.DORotate(new Vector3(0, 0, 360), timeHide, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            sequence.AppendCallback(() => onDone?.Invoke());
            sequence.AppendCallback(Hide);
    #else
            onLoad?.Invoke();
            onDone?.Invoke();
    #endif
        }
        private void LoadFake()
        {
        }
        private void LoadDone()
        {
    
            Hide();
        }
    
    }
}
