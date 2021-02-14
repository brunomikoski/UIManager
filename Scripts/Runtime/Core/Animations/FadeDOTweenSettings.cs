using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BrunoMikoski.UIManager.Animation
{
    [Serializable]
    public sealed class FadeDOTweenSettings : DOTweenSettingsBase
    {
        [SerializeField]
        private float alpha;

        protected override Tween GenerateTweenInternal(GameObject target, AnimationDirection animationDirection, float duration)
        {
            CanvasGroup canvasGroup = target.GetOrAddComponent<CanvasGroup>();
            TweenerCore<float, float, FloatOptions> canvasTween = canvasGroup.DOFade(alpha, duration);
            if (direction == AnimationDirection.From)
                canvasTween.From();
            return canvasTween;
        }
    }
}
