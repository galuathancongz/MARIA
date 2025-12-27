using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

namespace Luzart
{
    public abstract class TweenAnimationWorker : ITweenAnimation
    {
        protected Sequence _tweener;
        protected TweenAnimationSettings _settings;
        protected TweenAnimationSettings Settings => _settings;

        void ITweenAnimation.InitSetting(TweenAnimationSettings settings)
        {
            DoInitSetting(settings);
        }

        Tween ITweenAnimation.Show()
        {
            return DoShow();
        }

        void IDisposable.Dispose()
        {
            DoDispose();
        }

        protected virtual Tween DoShow()
        {
            return null;
        }

        protected virtual void DoInitSetting(TweenAnimationSettings settings)
        {
            this._settings = settings;
        }

        protected virtual void DoDispose()
        {
            _tweener?.Kill(true);
            _tweener = null;
        }

        protected Sequence CreateBaseTween()
        {
            var sequence = DOTween.Sequence();
            sequence.SetUpdate(Settings.General.IsIgnoreTimeScale);
            if (Settings.Timing.DelayStart > 0)
                sequence.AppendInterval(Settings.Timing.DelayStart);
            return sequence;
        }

        protected void AppendTweenToSequence(Tween tween)
        {
            if (Settings.Loop.IsLoop)
            {
                Sequence loopTween = DOTween.Sequence();
                if (Settings.Timing.TimeDelayPreLoop > 0)
                    loopTween.AppendInterval(Settings.Timing.TimeDelayPreLoop);
                loopTween.Append(tween);
                if (Settings.Timing.TimeDelayAfterLoop > 0)
                    loopTween.AppendInterval(Settings.Timing.TimeDelayAfterLoop);
                loopTween.SetLoops(Settings.Loop.LoopCount, Settings.Loop.LoopType);
                _tweener.Append(loopTween);
            }
            else
            {
                _tweener.Append(tween);
            }
        }

        /// <summary>
        /// High-performance method to get target component with smart casting
        /// </summary>
        protected T GetTargetComponent<T>() where T : Component
        {
            var targetObject = Settings?.General?.Target;
            if (targetObject == null) return null;

            // Fast path: Direct cast if target is already the desired type
            if (targetObject is T directCast)
            {
                return directCast;
            }

            // Get GameObject reference efficiently
            GameObject gameObject = null;
            if (targetObject is GameObject go)
            {
                gameObject = go;
            }
            else if (targetObject is Component comp)
            {
                gameObject = comp.gameObject;
            }

            // Get component from GameObject
            return gameObject?.GetComponent<T>();
        }

        /// <summary>
        /// Get existing component or add it if not found (used for CanvasGroup)
        /// </summary>
        protected T GetOrAddTargetComponent<T>() where T : Component
        {
            var existingComponent = GetTargetComponent<T>();
            if (existingComponent != null) return existingComponent;

            // If component doesn't exist, try to add it
            var targetObject = Settings?.General?.Target;
            if (targetObject == null) return null;

            GameObject gameObject = null;
            if (targetObject is GameObject go)
            {
                gameObject = go;
            }
            else if (targetObject is Component comp)
            {
                gameObject = comp.gameObject;
            }

            return gameObject?.GetComponent<T>() ?? gameObject?.AddComponent<T>();
        }
    }

    // Helper class for working with different settings types
    public static class TweenSettingsHelper
    {
        public static Sequence CreateBaseTween(ITweenSettings settings, bool ignoreTimeScale = false)
        {
            var sequence = DOTween.Sequence();
            sequence.SetUpdate(ignoreTimeScale);
            if (settings.Timing.DelayStart > 0)
                sequence.AppendInterval(settings.Timing.DelayStart);
            return sequence;
        }

        public static void AppendTweenToSequence(Sequence mainSequence, Tween tween, ITweenSettings settings)
        {
            if (settings.Loop.IsLoop)
            {
                Sequence loopTween = DOTween.Sequence();
                if (settings.Timing.TimeDelayPreLoop > 0)
                    loopTween.AppendInterval(settings.Timing.TimeDelayPreLoop);
                loopTween.Append(tween);
                if (settings.Timing.TimeDelayAfterLoop > 0)
                    loopTween.AppendInterval(settings.Timing.TimeDelayAfterLoop);
                loopTween.SetLoops(settings.Loop.LoopCount, settings.Loop.LoopType);
                mainSequence.Append(loopTween);
            }
            else
            {
                mainSequence.Append(tween);
            }
        }
    }

    #region TweenAnimation Workers

    public class TweenAnimationMove : TweenAnimationWorker
    {
        private Transform _target;
        private Transform Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetTargetComponent<Transform>();
                }
                return _target;
            }
        }
        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            SetSettingsDefaultFromTo(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationMove: Target Transform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenMove = DOTween.Sequence();
            tweenMove.AppendCallback(() =>
            {
               SetSettingsDefaultFromTo(true);
               Target.position = Settings.Values.GetVector3From();
            });
            tweenMove.Append(Target.DOMove(Settings.Values.GetVector3To(), Settings.General.Duration)
                                  .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenMove);
            _tweener.SetTarget(Target);
            return _tweener;
        }
        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if ( Settings.Values.GetVector3From() == Vector3Int.one * -1 )
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3From)
                {
                    Settings.Values.Vector3From = Target.position;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if(isRuntime == Settings.Values.IsSetRuntimeVector3To)
                {
                    Settings.Values.Vector3To = Target.position;
                }
            }
        }
    }

    public class TweenAnimationMoveLocal : TweenAnimationWorker
    {
        private Transform _target;
        private Transform Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetTargetComponent<Transform>();
                }
                return _target;
            }
        }

        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            SetSettingsDefaultFromTo(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationMoveLocal: Target Transform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenMove = DOTween.Sequence();
            tweenMove.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                Target.localPosition = Settings.Values.GetVector3From();
            });
            tweenMove.Append(Target.DOLocalMove(Settings.Values.GetVector3To(), Settings.General.Duration)
                                  .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenMove);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (Settings.Values.GetVector3From() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3From)
                {
                    Settings.Values.Vector3From = Target.localPosition;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3To)
                {
                    Settings.Values.Vector3To = Target.localPosition;
                }
            }
        }
    }

    public class TweenAnimationMoveAnchors : TweenAnimationWorker
    {
        private RectTransform _target;
        private RectTransform Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetTargetComponent<RectTransform>();
                }
                return _target;
            }
        }

        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            SetSettingsDefaultFromTo(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationMoveAnchors: Target RectTransform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenMove = DOTween.Sequence();
            tweenMove.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                Target.anchoredPosition = Settings.Values.GetVector3From();
            });
            tweenMove.Append(Target.DOAnchorPos(Settings.Values.GetVector3To(), Settings.General.Duration)
                                  .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenMove);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (Settings.Values.GetVector3From() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3From)
                {
                    Settings.Values.Vector3From = Target.anchoredPosition;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3To)
                {
                    Settings.Values.Vector3To = Target.anchoredPosition;
                }
            }
        }
    }

    public class TweenAnimationRotation : TweenAnimationWorker
    {
        private Transform _target;
        private Transform Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetTargetComponent<Transform>();
                }
                return _target;
            }
        }

        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            SetSettingsDefaultFromTo(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationRotation: Target Transform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenRotation = DOTween.Sequence();
            tweenRotation.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                Target.rotation = Quaternion.Euler(Settings.Values.GetVector3From());
            });
            tweenRotation.Append(Target.DORotate(Settings.Values.GetVector3To(), Settings.General.Duration)
                                       .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenRotation);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (Settings.Values.GetVector3From() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3From)
                {
                    Settings.Values.Vector3From = Target.rotation.eulerAngles;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3To)
                {
                    Settings.Values.Vector3To = Target.rotation.eulerAngles;
                }
            }
        }
    }

    public class TweenAnimationEuler : TweenAnimationWorker
    {
        private Transform _target;
        private Transform Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetTargetComponent<Transform>();
                }
                return _target;
            }
        }

        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            SetSettingsDefaultFromTo(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationEuler: Target Transform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenEuler = DOTween.Sequence();
            tweenEuler.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                Target.eulerAngles = Settings.Values.GetVector3From();
            });
            tweenEuler.Append(Target.DORotate(Settings.Values.GetVector3To(), Settings.General.Duration, RotateMode.FastBeyond360)
                                    .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenEuler);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (Settings.Values.GetVector3From() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3From)
                {
                    Settings.Values.Vector3From = Target.eulerAngles;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3To)
                {
                    Settings.Values.Vector3To = Target.eulerAngles;
                }
            }
        }
    }

    public class TweenAnimationScale : TweenAnimationWorker
    {
        private Transform _target;
        private Transform Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetTargetComponent<Transform>();
                }
                return _target;
            }
        }

        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            SetSettingsDefaultFromTo(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationScale: Target Transform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenScale = DOTween.Sequence();
            tweenScale.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                Target.localScale = Settings.Values.GetVector3From();
            });
            tweenScale.Append(Target.DOScale(Settings.Values.GetVector3To(), Settings.General.Duration)
                                    .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenScale);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (Settings.Values.GetVector3From() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3From)
                {
                    Settings.Values.Vector3From = Target.localScale;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3To)
                {
                    Settings.Values.Vector3To = Target.localScale;
                }
            }
        }
    }

    public class TweenAnimationSizeDelta : TweenAnimationWorker
    {
        private RectTransform _target;
        private RectTransform Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetTargetComponent<RectTransform>();
                }
                return _target;
            }
        }

        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            SetSettingsDefaultFromTo(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationSizeDelta: Target RectTransform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenSize = DOTween.Sequence();
            tweenSize.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                Target.sizeDelta = Settings.Values.GetVector3From();
            });
            tweenSize.Append(Target.DOSizeDelta(Settings.Values.GetVector3To(), Settings.General.Duration)
                                   .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenSize);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (Settings.Values.GetVector3From() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3From)
                {
                    Settings.Values.Vector3From = Target.sizeDelta;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3To)
                {
                    Settings.Values.Vector3To = Target.sizeDelta;
                }
            }
        }
    }

    public class TweenAnimationAnchorMin : TweenAnimationWorker
    {
        private RectTransform _target;
        private RectTransform Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetTargetComponent<RectTransform>();
                }
                return _target;
            }
        }

        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            SetSettingsDefaultFromTo(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationAnchorMin: Target RectTransform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenAnchorMin = DOTween.Sequence();
            tweenAnchorMin.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                Target.anchorMin = Settings.Values.GetVector3From();
            });
            tweenAnchorMin.Append(Target.DOAnchorMin(Settings.Values.GetVector3To(), Settings.General.Duration)
                                        .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenAnchorMin);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (Settings.Values.GetVector3From() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3From)
                {
                    Settings.Values.Vector3From = Target.anchorMin;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3To)
                {
                    Settings.Values.Vector3To = Target.anchorMin;
                }
            }
        }
    }

    public class TweenAnimationAnchorMax : TweenAnimationWorker
    {
        private RectTransform _target;
        private RectTransform Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetTargetComponent<RectTransform>();
                }
                return _target;
            }
        }

        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            SetSettingsDefaultFromTo(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationAnchorMax: Target RectTransform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenAnchorMax = DOTween.Sequence();
            tweenAnchorMax.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                Target.anchorMax = Settings.Values.GetVector3From();
            });
            tweenAnchorMax.Append(Target.DOAnchorMax(Settings.Values.GetVector3To(), Settings.General.Duration)
                                        .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenAnchorMax);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (Settings.Values.GetVector3From() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3From)
                {
                    Settings.Values.Vector3From = Target.anchorMax;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeVector3To)
                {
                    Settings.Values.Vector3To = Target.anchorMax;
                }
            }
        }
    }

    public class TweenAnimationFade : TweenAnimationWorker
    {
        private CanvasGroup _target;
        private CanvasGroup Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetOrAddTargetComponent<CanvasGroup>();
                }
                return _target;
            }
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationFade: Target CanvasGroup is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenFade = DOTween.Sequence();
            tweenFade.AppendCallback(() =>
            {
                Target.alpha = Settings.Values.GetFloatFrom();
            });
            tweenFade.Append(Target.DOFade(Settings.Values.GetFloatTo(), Settings.General.Duration)
                                   .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenFade);
            _tweener.SetTarget(Target);
            return _tweener;
        }
    }

    public class TweenAnimationTextMeshPro : TweenAnimationWorker
    {
        private TMP_Text _target;
        private TMP_Text Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetTargetComponent<TMP_Text>();
                }
                return _target;
            }
        }
        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            if(string.IsNullOrEmpty(Settings.Values.StringTo))
            {
                Settings.Values.StringTo = Target.text;
            }
        }
        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationTextMeshPro: Target TMP_Text is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenText = DOTween.Sequence();
            tweenText.AppendCallback(() =>
            {
                Target.text = Settings.Values.GetStringFrom();
            });
            tweenText.Append(Target.DOText(Settings.Values.GetStringTo(), Settings.General.Duration)
                                   .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenText);
            _tweener.SetTarget(Target);
            return _tweener;
        }
    }

    #endregion

}
