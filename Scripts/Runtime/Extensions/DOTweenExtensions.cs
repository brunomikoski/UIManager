using System.Collections;

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
    }
}