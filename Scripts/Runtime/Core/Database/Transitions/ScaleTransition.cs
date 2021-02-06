using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class ScaleTransition : AnimatedTransition
    {
        [SerializeField]
        private Vector3 fromScale = Vector3.zero;
        [SerializeField]
        private Vector3 toScale = Vector3.one;
        [SerializeField]
        private float duration = .6f;
        [SerializeField]
        private Ease ease = Ease.OutBack;
        

        public override void BeforeTransition(Window targetWindow)
        {
            targetWindow.transform.localScale = fromScale;
        }

        public override IEnumerator ExecuteEnumerator(Window targetWindow,
            bool isBackwards)
        {
            targetWindow.transform.DOKill();
            if (!isBackwards)
            {
                yield return targetWindow.transform.DOScale(toScale, duration).SetEase(ease).WaitForCompletionEnumerator();
            }
            else
            {
                Tween tween = targetWindow.transform.DOScale(toScale, duration).Pause().SetEase(ease).SetAutoKill(false);
                tween.Complete();
                tween.PlayBackwards();
                yield return tween.WaitForRewindEnumerator();
                tween.Kill();
            }
        }

        public void SetAnimationValues(Vector3 from, Vector3 to, float duration, Ease ease)
        {
            this.fromScale = from;
            this.toScale = to;
            this.duration = duration;
            this.ease = ease;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
