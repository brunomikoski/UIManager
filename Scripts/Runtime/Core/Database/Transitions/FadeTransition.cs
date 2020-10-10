using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class FadeTransition : AnimatedTransition
    {
        [SerializeField] 
        private float fromValue = 0;
        [SerializeField]
        private float toValue = 1;
        [SerializeField] 
        private float duration = 0.3f;
        [SerializeField] 
        private Ease ease;

        public override void BeforeTransition(Window targetWindow)
        {
            targetWindow.CanvasGroup.alpha = fromValue;
        }

        public override IEnumerator ExecuteEnumerator(Window targetWindow, TransitionType transitionType,
            bool isBackwards)
        {
            if (!isBackwards)
            {
                yield return targetWindow.CanvasGroup.DOFade(toValue, duration)
                    .SetEase(ease)
                    .WaitForTweenCompletionEnumerator();
            }
            else
            {
                Tween tween = targetWindow.CanvasGroup.DOFade(toValue, duration)
                    .SetEase(ease).SetAutoKill(false);
                tween.Complete();
                tween.PlayBackwards();
                yield return tween.WaitForRewindEnumerator();
                tween.Kill();
            }
        }
    }
}
