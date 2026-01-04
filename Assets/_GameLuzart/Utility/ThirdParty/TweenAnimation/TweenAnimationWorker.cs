using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luzart
{
    public abstract class TweenAnimationWorker : ITweenAnimation
    {
        protected Sequence _tweener;
        protected TweenAnimationSettings _settings;
        protected TweenAnimationSettings Settings => _settings;

        ITweenSettings ITweenAnimation.Settings => Settings;

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
            if (gameObject == null)
            {
                Debug.LogError("GameObject in this is null");
                return null;
            }
            if (gameObject.TryGetComponent<T>(out T componentGet))
            {
                return componentGet;
            }
            else
            {
                var component = gameObject.AddComponent<T>();
                return component;
            }
        }
    }

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

    #region Vector3 Based Animations

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
            SetValueFrom(false);
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
                SetValueFrom(true);
            });
            tweenMove.Append(Target.DOMove(Settings.Values.GetVector3To(), Settings.General.Duration)
                                  .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenMove);
            _tweener.SetTarget(Target);
            return _tweener;
        }
        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (Settings.Values.GetVector3From() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeFrom)
                {
                    Settings.Values.Vector3From = Target.position;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeTo)
                {
                    Settings.Values.Vector3To = Target.position;
                }
            }
        }
        private void SetValueFrom(bool isRuntime)
        {
            if (isRuntime == Settings.Values.IsSetFromInInit)
            {
                Target.position = Settings.Values.GetVector3From();

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
            SetValueFrom(false);
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
                SetValueFrom(true);
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
                if (isRuntime == Settings.Values.IsSetRuntimeFrom)
                {
                    Settings.Values.Vector3From = Target.localPosition;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeTo)
                {
                    Settings.Values.Vector3To = Target.localPosition;
                }
            }
        }

        private void SetValueFrom(bool isRuntime)
        {
            if (isRuntime == Settings.Values.IsSetFromInInit)
            {
                Target.localPosition = Settings.Values.GetVector3From();
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
            SetValueFrom(false);
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
                SetValueFrom(true);
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
                if (isRuntime == Settings.Values.IsSetRuntimeFrom)
                {
                    Settings.Values.Vector3From = Target.anchoredPosition;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeTo)
                {
                    Settings.Values.Vector3To = Target.anchoredPosition;
                }
            }
        }

        private void SetValueFrom(bool isRuntime)
        {
            if (isRuntime == Settings.Values.IsSetFromInInit)
            {
                Target.anchoredPosition = Settings.Values.GetVector3From();
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
            SetValueFrom(false);
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
                SetValueFrom(true);
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
                if (isRuntime == Settings.Values.IsSetRuntimeFrom)
                {
                    Settings.Values.Vector3From = Target.localScale;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeTo)
                {
                    Settings.Values.Vector3To = Target.localScale;
                }
            }
        }

        private void SetValueFrom(bool isRuntime)
        {
            if (isRuntime == Settings.Values.IsSetFromInInit)
            {
                Target.localScale = Settings.Values.GetVector3From();
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
            SetValueFrom(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationEuler: Target Transform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenRotation = DOTween.Sequence();
            tweenRotation.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                SetValueFrom(true);
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
                if (isRuntime == Settings.Values.IsSetRuntimeFrom)
                {
                    Settings.Values.Vector3From = Target.eulerAngles;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeTo)
                {
                    Settings.Values.Vector3To = Target.eulerAngles;
                }
            }
        }

        private void SetValueFrom(bool isRuntime)
        {
            if (isRuntime == Settings.Values.IsSetFromInInit)
            {
                Target.eulerAngles = Settings.Values.GetVector3From();
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
            SetValueFrom(false);
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
                SetValueFrom(true);
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
                if (isRuntime == Settings.Values.IsSetRuntimeFrom)
                {
                    Settings.Values.Vector3From = Target.sizeDelta;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeTo)
                {
                    Settings.Values.Vector3To = Target.sizeDelta;
                }
            }
        }

        private void SetValueFrom(bool isRuntime)
        {
            if (isRuntime == Settings.Values.IsSetFromInInit)
            {
                Target.sizeDelta = Settings.Values.GetVector3From();
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
            SetValueFrom(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationAnchorMin: Target RectTransform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenAnchor = DOTween.Sequence();
            tweenAnchor.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                SetValueFrom(true);
            });
            tweenAnchor.Append(Target.DOAnchorMin(Settings.Values.GetVector3To(), Settings.General.Duration)
                                    .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenAnchor);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (Settings.Values.GetVector3From() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeFrom)
                {
                    Settings.Values.Vector3From = Target.anchorMin;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeTo)
                {
                    Settings.Values.Vector3To = Target.anchorMin;
                }
            }
        }

        private void SetValueFrom(bool isRuntime)
        {
            if (isRuntime == Settings.Values.IsSetFromInInit)
            {
                Target.anchorMin = Settings.Values.GetVector3From();
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
            SetValueFrom(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationAnchorMax: Target RectTransform is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenAnchor = DOTween.Sequence();
            tweenAnchor.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                SetValueFrom(true);
            });
            tweenAnchor.Append(Target.DOAnchorMax(Settings.Values.GetVector3To(), Settings.General.Duration)
                                    .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenAnchor);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (Settings.Values.GetVector3From() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeFrom)
                {
                    Settings.Values.Vector3From = Target.anchorMax;
                }
            }
            if (Settings.Values.GetVector3To() == Vector3Int.one * -1)
            {
                if (isRuntime == Settings.Values.IsSetRuntimeTo)
                {
                    Settings.Values.Vector3To = Target.anchorMax;
                }
            }
        }

        private void SetValueFrom(bool isRuntime)
        {
            if (isRuntime == Settings.Values.IsSetFromInInit)
            {
                Target.anchorMax = Settings.Values.GetVector3From();
            }
        }
    }

    #endregion

    #region Float Based Animations

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

        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            SetSettingsDefaultFromTo(false);
            SetValueFrom(false);
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
                SetSettingsDefaultFromTo(true);
                SetValueFrom(true);
            });
            tweenFade.Append(Target.DOFade(Settings.Values.GetFloatTo(), Settings.General.Duration)
                                  .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenFade);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            // For float values, we don't use -1 as default, instead we use the current alpha value
            if (Mathf.Approximately(Settings.Values.GetFloatFrom(), -1))
            {
                if (isRuntime == Settings.Values.IsSetRuntimeFrom)
                {
                    Settings.Values.FloatFrom = Target.alpha;
                }
            }
            if (Mathf.Approximately(Settings.Values.GetFloatTo(), -1))
            {
                if (isRuntime == Settings.Values.IsSetRuntimeTo)
                {
                    Settings.Values.FloatTo = Target.alpha;
                }
            }
        }

        private void SetValueFrom(bool isRuntime)
        {
            if (isRuntime != Settings.Values.IsSetFromInInit)
            {
                float from = Settings.Values.GetFloatFrom();
                Target.alpha = from;
            }
        }
    }

    #endregion

    #region Text Based Animations

    public class TweenAnimationTextMeshPro : TweenAnimationWorker
    {
        private TextMeshProUGUI _target;
        private TextMeshProUGUI Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetTargetComponent<TextMeshProUGUI>();
                }
                return _target;
            }
        }

        protected override void DoInitSetting(TweenAnimationSettings settings)
        {
            base.DoInitSetting(settings);
            SetSettingsDefaultFromTo(false);
            SetValueFrom(false);
        }

        protected override Tween DoShow()
        {
            if (Target == null)
            {
                Debug.LogWarning("TweenAnimationTextMeshPro: Target TextMeshProUGUI is null");
                return null;
            }

            _tweener = CreateBaseTween();
            Sequence tweenText = DOTween.Sequence();
            tweenText.AppendCallback(() =>
            {
                SetSettingsDefaultFromTo(true);
                SetValueFrom(true);
            });
            tweenText.Append(Target.DOText(Settings.Values.GetStringTo(), Settings.General.Duration)
                                  .SetEase(Settings.General.Easing));

            AppendTweenToSequence(tweenText);
            _tweener.SetTarget(Target);
            return _tweener;
        }

        private void SetSettingsDefaultFromTo(bool isRuntime)
        {
            if (string.IsNullOrEmpty(Settings.Values.GetStringFrom()))
            {
                if (isRuntime == Settings.Values.IsSetRuntimeFrom)
                {
                    Settings.Values.StringFrom = Target.text;
                }
            }
            if (string.IsNullOrEmpty(Settings.Values.GetStringTo()))
            {
                if (isRuntime == Settings.Values.IsSetRuntimeTo)
                {
                    Settings.Values.StringTo = Target.text;
                }
            }
        }

        private void SetValueFrom(bool isRuntime)
        {
            if (isRuntime == Settings.Values.IsSetFromInInit)
            {
                Target.text = Settings.Values.GetStringFrom();
            }
        }
    }

    #endregion
}