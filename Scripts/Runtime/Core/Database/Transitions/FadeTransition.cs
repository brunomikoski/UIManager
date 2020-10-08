using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class FadeTransition : TransitionBase
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
            targetWindow.CanvasGroup.alpha = 0;
        }

        public override IEnumerator ExecuteEnumerator(Window targetWindow)
        {
            yield return targetWindow.CanvasGroup.DOFade(toValue, duration).SetEase(ease)
                .WaitForTweenCompletionEnumerator();
        }
    }
}
