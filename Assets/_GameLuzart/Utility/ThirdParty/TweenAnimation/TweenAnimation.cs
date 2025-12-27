using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luzart
{
    public class TweenAnimation : TweenAnimationBase
    {
        [SerializeField] private EAnimation typeAnimation;
        [SerializeField] private TweenAnimationSettings tweenAnimationSettings = new TweenAnimationSettings();
        private ITweenAnimation _currentTweenAnimation;
        public TweenAnimationSettings TweenAnimationSettings => tweenAnimationSettings;
        protected override Tween DoShow()
        {
            var tweenAnimation = GetTweenAnimation();
            if (tweenAnimation == null)
            {
                Debug.LogError("Tween Animation Type not found: " + typeAnimation.ToString());
                return null;
            }
            if(tweenAnimationSettings.General.Target == null)
            {
                tweenAnimationSettings.General.Target = this.gameObject;
            }

            tweenAnimation.InitSetting(tweenAnimationSettings);
            _currentTweenAnimation = tweenAnimation;
            return tweenAnimation.Show();
        }

        protected override void DoDispose()
        {
            _currentTweenAnimation?.Dispose();
            _currentTweenAnimation = null;
        }

        private ITweenAnimation GetTweenAnimation()
        {
            return typeAnimation switch
            {
                EAnimation.Move => new TweenAnimationMove(),
                EAnimation.MoveLocal => new TweenAnimationMoveLocal(),
                EAnimation.MoveAnchors => new TweenAnimationMoveAnchors(),
                EAnimation.Rotation => new TweenAnimationRotation(),
                EAnimation.Euler => new TweenAnimationEuler(),
                EAnimation.Scale => new TweenAnimationScale(),
                EAnimation.SizeDelta => new TweenAnimationSizeDelta(),
                EAnimation.AnchorMin => new TweenAnimationAnchorMin(),
                EAnimation.AnchorMax => new TweenAnimationAnchorMax(),
                EAnimation.Fade => new TweenAnimationFade(),
                EAnimation.TextMeshPro => new TweenAnimationTextMeshPro(),
                _ => null
            };
        }

        [ContextMenu("Set Default Settings")]
        private void SetDefaultSettings()
        {
            tweenAnimationSettings.Values.Vector3To = Vector3Int.one * -1;
            tweenAnimationSettings.Values.Vector3From = Vector3Int.one * -1;
        }
    }

    #region Data Structures

    // Interface for all settings types
    public interface ITweenSettings
    {
        TweenTimingSettings Timing { get; }
        TweenLoopSettings Loop { get; }
    }

    // Full settings for individual animations
    [System.Serializable]
    public class TweenAnimationSettings : ITweenSettings
    {
        public TweenGeneralSettings General;
        
        public TweenTimingSettings Timing;
        
        public TweenLoopSettings Loop;
        
        public TweenValueSettings Values;

        TweenTimingSettings ITweenSettings.Timing => Timing;
        TweenLoopSettings ITweenSettings.Loop => Loop;

        public TweenAnimationSettings()
        {
            General = new TweenGeneralSettings();
            Timing = new TweenTimingSettings();
            Loop = new TweenLoopSettings();
            Values = new TweenValueSettings();
        }
    }

    [System.Serializable]
    public class TweenGeneralSettings
    {
        public UnityEngine.Object Target;
        public float Duration = 1f;
        public Ease Easing = Ease.OutQuart;
        public bool IsIgnoreTimeScale = false;
    }

    [System.Serializable]
    public class TweenTimingSettings
    {
        public float DelayStart = 0f;
        [ShowIf("../Loop.IsLoop", true)]
        public float TimeDelayPreLoop = 0f;
        [ShowIf("../Loop.IsLoop", true)]
        public float TimeDelayAfterLoop = 0f;
    }

    [System.Serializable]
    public class TweenLoopSettings
    {
        public bool IsLoop = false;
        public LoopType LoopType = LoopType.Restart;
        public int LoopCount = -1;
    }

    [System.Serializable]
    public class TweenValueSettings
    {
        [ShowIfAny( "../../typeAnimation", EAnimation.Move, 
                         "../../typeAnimation", EAnimation.MoveLocal,
                         "../../typeAnimation", EAnimation.MoveAnchors,
                         "../../typeAnimation", EAnimation.Scale,
                         "../../typeAnimation", EAnimation.Rotation,
                         "../../typeAnimation", EAnimation.Euler,
                         "../../typeAnimation", EAnimation.SizeDelta,
                         "../../typeAnimation", EAnimation.AnchorMin,
                         "../../typeAnimation", EAnimation.AnchorMax)]
        public Vector3 Vector3From = -Vector3Int.one;
        public bool _isVector3FromDefault => Vector3From == -Vector3Int.one;
        [ShowIf("_isVector3FromDefault",true)]
        public bool IsSetRuntimeVector3From = false;
        
        [ShowIfAny( "../../typeAnimation", EAnimation.Move, 
                         "../../typeAnimation", EAnimation.MoveLocal,
                         "../../typeAnimation", EAnimation.MoveAnchors,
                         "../../typeAnimation", EAnimation.Scale,
                         "../../typeAnimation", EAnimation.Rotation,
                         "../../typeAnimation", EAnimation.Euler,
                         "../../typeAnimation", EAnimation.SizeDelta,
                         "../../typeAnimation", EAnimation.AnchorMin,
                         "../../typeAnimation", EAnimation.AnchorMax)]
        public Vector3 Vector3To = -Vector3Int.one;
        public bool _isVector3ToDefault => Vector3To == -Vector3Int.one;
        [ShowIf("_isVector3ToDefault", true)]
        public bool IsSetRuntimeVector3To = false;

        [ShowIf("../../typeAnimation", EAnimation.Fade)]
        public float FloatFrom = 0f;
        
        [ShowIf("../../typeAnimation", EAnimation.Fade)]
        public float FloatTo = 1f;

        [ShowIf("../../typeAnimation", EAnimation.TextMeshPro)]
        public string StringFrom = "";
        
        [ShowIf("../../typeAnimation", EAnimation.TextMeshPro)]
        [DisableIf("../General.Target", null)]
        public string StringTo = "";

        // Helper methods to get type-specific values
        public Vector3 GetVector3From() => Vector3From;
        public Vector3 GetVector3To() => Vector3To;
        public float GetFloatFrom() => FloatFrom;
        public float GetFloatTo() => FloatTo;
        public string GetStringFrom() => StringFrom;
        public string GetStringTo() => StringTo;
    }
    

    public enum ETypeShow
    {
        None = 0,
        Awake = 1,
        Start = 2,
        OnEnable = 3,
    }

    public enum EAnimation
    {
        Move = 0,
        MoveLocal = 1,
        MoveAnchors = 2,
        Rotation = 3,
        Euler = 4,
        Scale = 5,
        SizeDelta = 6,
        AnchorMin = 7,
        AnchorMax = 8,
        Fade = 9,
        TextMeshPro = 10,
        Float = 11,
    }

    #endregion

    #region Base Classes and Interface

    public interface ITweenAnimation : IDisposable
    {
        void InitSetting(TweenAnimationSettings settings);
        Tween Show();
    }

    #endregion
}
