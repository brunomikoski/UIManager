using System.Collections;

namespace BrunoMikoski.UIManager
{
    public abstract class AnimatedTransition : TransitionBase
    {
        public abstract IEnumerator ExecuteEnumerator(Window targetWindow, TransitionType transitionType,
            bool isBackwards);

        public virtual void BeforeTransition(Window targetWindow){}
    }
}
