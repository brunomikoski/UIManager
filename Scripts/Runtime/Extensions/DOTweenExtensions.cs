using System.Collections;
using UnityEngine.Experimental.GlobalIllumination;

namespace DG.Tweening
{
    public static class DOTweenExtensions
    {
        public static IEnumerator WaitForTweenCompletionEnumerator(this object targetOrId)
        {
            while (DOTween.IsTweening(targetOrId))
                yield return null;
        }

        public static IEnumerator WaitForCompletionEnumerator(this Tween tween)
        {
            while (tween != null && tween.IsActive() && !tween.IsComplete())
                yield return null;
        }

        public static IEnumerator WaitForRewindEnumerator(this Tween tween)
        {
            while (tween.IsActive() && (!tween.playedOnce || (double) tween.position * (tween.CompletedLoops() + 1) > 0.0))
                yield return null;
        }
    }
}