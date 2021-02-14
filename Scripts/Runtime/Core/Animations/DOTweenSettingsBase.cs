using System;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.UIManager.Animation
{
    [Serializable]
    public abstract class DOTweenSettingsBase
    {
        [SerializeField]
        private bool enabled;
        public bool Enabled => enabled;
        [SerializeField]
        protected Ease ease = Ease.InOutCirc;
        [SerializeField]
        protected AnimationDirection direction;

        protected abstract Tween GenerateTweenInternal(GameObject target, AnimationDirection animationDirection, float duration);

        public Tween GenerateTween(GameObject target, float duration)
        {
            Tween tween = GenerateTweenInternal(target, direction, duration);
            tween.SetEase(ease);
            return tween;
        }
    }
}
