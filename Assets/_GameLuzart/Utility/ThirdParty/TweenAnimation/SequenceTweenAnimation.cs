using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Luzart
{
    public class SequenceTweenAnimation : TweenAnimationBase
    {
        [SerializeField] private List<TweenSequence> tweenSequences = new List<TweenSequence>();
        [SerializeField] private TweenSequenceSettings sequenceSettings;
        
        private Sequence _sequenceTweener;

        protected override Tween DoShow()
        {
            _sequenceTweener = DOTween.Sequence();
            
            // Apply ignore time scale settings
            _sequenceTweener.SetUpdate(sequenceSettings.IsIgnoreTimeScale);
            
            // Apply timing settings
            if (sequenceSettings.Timing.DelayStart > 0)
                _sequenceTweener.AppendInterval(sequenceSettings.Timing.DelayStart);

            // Create the main sequence
            Sequence mainSequence = DOTween.Sequence();
            // Apply ignore time scale to main sequence as well
            mainSequence.SetUpdate(sequenceSettings.IsIgnoreTimeScale);
            
            for (int i = 0; i < tweenSequences.Count; i++)
            {
                var tweenSequence = tweenSequences[i];
                ITweenAnimation tweenAnimation = tweenSequence.TweenAnimation;
                var tween = tweenAnimation.Show();
                if (tween != null)
                {
                    if(tweenSequence.SequenceType == ESequenceType.Append)
                    {
                        mainSequence.Append(tween);
                    }else if(tweenSequence.SequenceType == ESequenceType.Join)
                    {
                        mainSequence.Join(tween);
                    }
                }
            }

            // Apply loop settings
            if (sequenceSettings.Loop.IsLoop)
            {
                Sequence loopSequence = DOTween.Sequence();
                // Apply ignore time scale to loop sequence
                loopSequence.SetUpdate(sequenceSettings.IsIgnoreTimeScale);
                
                if (sequenceSettings.Timing.TimeDelayPreLoop > 0)
                    loopSequence.AppendInterval(sequenceSettings.Timing.TimeDelayPreLoop);
                    
                loopSequence.Append(mainSequence);
                
                if (sequenceSettings.Timing.TimeDelayAfterLoop > 0)
                    loopSequence.AppendInterval(sequenceSettings.Timing.TimeDelayAfterLoop);
                    
                loopSequence.SetLoops(sequenceSettings.Loop.LoopCount, sequenceSettings.Loop.LoopType);
                _sequenceTweener.Append(loopSequence);
            }
            else
            {
                _sequenceTweener.Append(mainSequence);
            }

            return _sequenceTweener;
        }

        protected override void DoDispose()
        {
            _sequenceTweener?.Kill(true);
            _sequenceTweener = null;
        }

        private void OnValidate()
        {
            if (sequenceSettings == null)
            {
                sequenceSettings = new TweenSequenceSettings();
            }
        }

        [System.Serializable]
        class TweenSequence
        {
            public TweenAnimationBase TweenAnimation;
            public ESequenceType SequenceType;
        }
        public enum ESequenceType
        {
            Append,
            Join
        }
    }
    
    // Minimal settings for sequence animations with IgnoreTimeScale option
    [System.Serializable]
    public class TweenSequenceSettings : ITweenSettings
    {
        public bool IsIgnoreTimeScale = false;
        
        public TweenTimingSettings Timing;
        
        public TweenLoopSettings Loop;

        TweenTimingSettings ITweenSettings.Timing => Timing;
        TweenLoopSettings ITweenSettings.Loop => Loop;

        public TweenSequenceSettings()
        {
            Timing = new TweenTimingSettings();
            Loop = new TweenLoopSettings();
        }
    }
}
