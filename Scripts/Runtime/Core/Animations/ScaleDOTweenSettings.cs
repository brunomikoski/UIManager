using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager.Animation
{
    [Serializable]
    public sealed class RotationDOTweenSettings : DOTweenSettingsBase
    {
        [SerializeField]
        private Vector3 rotation;
        [SerializeField]
        private bool local;

        protected override Tween GenerateTweenInternal(GameObject target, AnimationDirection animationDirection, float duration)
        {
            TweenerCore<Quaternion, Vector3, QuaternionOptions> tween 
                = local ? target.transform.DOLocalRotate(rotation, duration) : target.transform.DORotate(rotation, duration);
            
            if (animationDirection == AnimationDirection.From)
                tween.From();
            
            return tween;
        }
    }

    [Serializable]
    public sealed class PunchPositionDOTweenSettings : DOTweenSettingsBase
    {
        [SerializeField]
        private Vector3 punch;
        [SerializeField]
        private bool snapping;
        [SerializeField]
        private int vibrato = 10;
        [SerializeField]
        private float elasticity = 1;

        protected override Tween GenerateTweenInternal(GameObject target, AnimationDirection animationDirection, float duration)
        {
            Tweener tween = target.transform.DOPunchPosition(punch, duration, vibrato, elasticity, snapping);

            if (animationDirection == AnimationDirection.From)
                tween.From();

            return tween;
        }
    }
    
    [Serializable]
    public sealed class PunchRotationDOTweenSettings : DOTweenSettingsBase
    {
        [SerializeField]
        private Vector3 punch;
        [SerializeField]
        private int vibrato = 10;
        [SerializeField]
        private float elasticity = 1;

        protected override Tween GenerateTweenInternal(GameObject target, AnimationDirection animationDirection, float duration)
        {
            Tweener tween = target.transform.DOPunchRotation(punch, duration, vibrato, elasticity);

            if (animationDirection == AnimationDirection.From)
                tween.From();

            return tween;
        }
    }
    
    [Serializable]
    public sealed class PunchScaleDOTweenSettings : DOTweenSettingsBase
    {
        [SerializeField]
        private Vector3 punch;
        [SerializeField]
        private int vibrato = 10;
        [SerializeField]
        private float elasticity = 1;

        protected override Tween GenerateTweenInternal(GameObject target, AnimationDirection animationDirection, float duration)
        {
            Tweener tween = target.transform.DOPunchScale(punch, duration, vibrato, elasticity);

            if (animationDirection == AnimationDirection.From)
                tween.From();

            return tween;
        }
    }
    
    [Serializable]
    public sealed class ColorDOTweenSettings : DOTweenSettingsBase
    {
        [SerializeField]
        private Color targetColor;

        protected override Tween GenerateTweenInternal(GameObject target, AnimationDirection animationDirection, float duration)
        {
            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic == null)
                return null;

            TweenerCore<Color, Color, ColorOptions> tween = graphic.DOColor(targetColor, duration);
            if (animationDirection == AnimationDirection.From)
                tween.From();

            return tween;
        }
    }
    
    [Serializable]
    public sealed class ScaleDOTweenSettings : DOTweenSettingsBase
    {
        [SerializeField]
        private Vector3 scale;

        protected override Tween GenerateTweenInternal(GameObject target, AnimationDirection animationDirection, float duration)
        {
            TweenerCore<Vector3, Vector3, VectorOptions> tween = target.transform.DOScale(scale, duration);
            if (animationDirection == AnimationDirection.From)
                tween.From();
            return tween;
        }
    }
}
